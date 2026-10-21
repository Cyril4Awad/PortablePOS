import React, { useState, useCallback, useRef } from 'react';
import { View, Text, StyleSheet, ScrollView, Modal, TouchableOpacity, Alert } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import ScreenHeader from '../components/ScreenHeader';
import DataField from '../components/controls/DataField';
import ChoiceField, { choiceRowStyle } from '../components/controls/ChoiceField';
import MainButton from '../components/controls/MainButton';
import FreeReceivingData from '../data/FreeReceivingData';
import FreeReceivingDatabase from '../database/FreeReceivingDatabase';
import FreeReceivingSettings from '../configuration/FreeReceivingSettings';
import { FormatQuantity } from '../variables/FreeReceivingRules';

const twoDigits = (value) => value.toString().padStart(2, '0');

const FormatDate = (date) =>
  `${twoDigits(date.getDate())}/${twoDigits(date.getMonth() + 1)}/${date.getFullYear()} ${twoDigits(date.getHours())}:${twoDigits(date.getMinutes())}`;

// Free Receiving : download the stock file and the suppliers of the server, receive the items of a supplier offline, then upload
const FreeReceivingScreen = props => {

  const [server, setServer] = useState('');
  const [machineName, setMachineName] = useState('');
  const [info, setInfo] = useState(null);
  const [summary, setSummary] = useState({ lines: 0, qty: 0, pending: 0 });

  // { title, detail, percent, cancellable } while downloading
  const [progress, setProgress] = useState(null);
  const cancelRequested = useRef(false);

  const LoadStatus = async () => {
    try {
      setServer(await FreeReceivingSettings.server());
      setMachineName(await FreeReceivingSettings.machineName());
      setInfo(await FreeReceivingDatabase.GetInfo());
      setSummary(await FreeReceivingDatabase.GetLinesSummary());
    } catch (error) {
      alert(error.message);
    }
  };

  useFocusEffect(
    useCallback(() => {
      LoadStatus();
    }, [])
  );

  const stockReady = info !== null && info.complete === 1;

  //#region Download

  const BeforeDownload = () => {

    if (server === '') {
      alert('Please fill the server in the free receiving settings');
    }
    else if (machineName === '') {
      alert('Please fill the machine name in the free receiving settings');
    }
    else if (info !== null && info.complete === 0 && info.server === server) {
      Alert.alert('Download Not Completed', 'The last download of the stock file did not finish.', [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Restart Download', onPress: () => RunDownload(true) },
        { text: 'Continue Download', onPress: () => RunDownload(false) },
      ]);
    }
    else if (info !== null) {
      Alert.alert('Download', `The stock file of this device will be replaced by the one of ${server}. The collected items are kept.`, [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Download', onPress: () => RunDownload(true) },
      ]);
    }
    else {
      RunDownload(true);
    }

  };

  // Downloads the pages of one list from the last record stored, until a page is not full
  // fetchPage(afterRecID) => rows, savePage(rows), stored / last : read from the info after each page
  const DownloadList = async (title, count, startPercent, total, fetchPage, savePage, storedOf, lastOf) => {

    const pageSize = await FreeReceivingSettings.pageSize();
    let current = await FreeReceivingDatabase.GetInfo();

    while (true) {

      if (cancelRequested.current) throw new Error('Download cancelled, you can continue it later');

      const stored = storedOf(current);
      setProgress({
        title: 'Download',
        detail: `Synchronizing ${title} ${Math.min(stored, count)} / ${count}`,
        percent: total === 0 ? 0 : Math.min(1, (startPercent + Math.min(stored, count)) / total),
      });

      const rows = await fetchPage(lastOf(current), pageSize);
      if (rows.length > 0) {
        await savePage(rows);
        current = await FreeReceivingDatabase.GetInfo();
      }
      if (rows.length < pageSize) return current;

    }

  };

  // restart = false : continues the download in progress (on the server it was started on)
  const RunDownload = async (restart) => {

    cancelRequested.current = false;

    try {

      setProgress({ title: 'Download', detail: 'Connecting To The Server', percent: 0 });

      if (restart) {
        const serverInfo = await FreeReceivingData.GetReceivingInfo(server);
        await FreeReceivingDatabase.StartDownload(server, serverInfo);
      }

      const local = await FreeReceivingDatabase.GetInfo();
      const downloadServer = local.server;
      const total = local.itemsCount + local.barcodesCount + local.supplierRefsCount + local.suppliersCount;

      await DownloadList('Items', local.itemsCount, 0, total,
        (after, pageSize) => FreeReceivingData.GetReceivingItems(downloadServer, after, pageSize),
        rows => FreeReceivingDatabase.SaveItemsPage(rows),
        current => current.itemsStored, current => current.lastItemRecID);

      await DownloadList('Barcodes', local.barcodesCount, local.itemsCount, total,
        (after, pageSize) => FreeReceivingData.GetReceivingItemCodes(downloadServer, 'barcode', after, pageSize),
        rows => FreeReceivingDatabase.SaveCodesPage('barcode', rows),
        current => current.barcodesStored, current => current.lastBarcodeRecID);

      await DownloadList('Supplier Refs', local.supplierRefsCount, local.itemsCount + local.barcodesCount, total,
        (after, pageSize) => FreeReceivingData.GetReceivingItemCodes(downloadServer, 'supplierRef', after, pageSize),
        rows => FreeReceivingDatabase.SaveCodesPage('supplierRef', rows),
        current => current.supplierRefsStored, current => current.lastSupplierRefRecID);

      if (cancelRequested.current) throw new Error('Download cancelled, you can continue it later');

      setProgress({ title: 'Download', detail: `Synchronizing Suppliers ( ${local.suppliersCount} )`, percent: (total - local.suppliersCount) / Math.max(total, 1), cancellable: false });
      await FreeReceivingDatabase.SaveSuppliers(await FreeReceivingData.GetReceivingSuppliers(downloadServer));

      await FreeReceivingDatabase.CompleteDownload(FormatDate(new Date()));

      const done = await FreeReceivingDatabase.GetInfo();
      setProgress(null);
      alert(`Download Completed - ${done.itemsStored} Items, ${done.suppliersStored} Suppliers`);

    } catch (error) {
      setProgress(null);
      alert(error.message);
    }

    LoadStatus();

  };

  //#endregion

  // The upload of the collected items comes later
  const BeforeUpload = () => {

    if (summary.lines === 0) {
      alert('No Collected Items Found');
    }
    else {
      alert(`${summary.pending} collected lines are ready. The upload of the receiving is not available yet.`);
    }

  };

  const ProgressView = () => {
    return (
      <Modal visible={progress !== null} transparent={true} presentationStyle="overFullScreen">
        <View style={styles.modalContainer}>
          <View style={styles.modalCard}>

            <Text style={styles.progressTitleText}>{progress?.title}</Text>
            <Text style={styles.progressDetailText}>{progress?.detail}</Text>

            <View style={styles.progressBar}>
              <View style={[styles.progressBarFill, { width: `${Math.round((progress?.percent ?? 0) * 100)}%` }]} />
            </View>

            {progress?.cancellable !== false ?
              <TouchableOpacity style={styles.buttonCancel} onPress={() => { cancelRequested.current = true; }}>
                <Text style={styles.buttonCancelText}>Cancel</Text>
              </TouchableOpacity> :
              <></>}

          </View>
        </View>
      </Modal>
    );
  };

  const stockText = info === null ? 'Not Downloaded'
    : `${info.itemsStored} Items  ·  ${info.suppliersStored} Suppliers`;

  return (
    <View style={styles.container}>

      {/* Called as a function so the modal is not re-created on every progress update */}
      {ProgressView()}

      {/* The header only holds a description : left out on the small portable scanners so the screen fits without scrolling */}
      {!Sizing.isPortableScanner ?
        <ScreenHeader
          description='Receive supplier items offline then upload the receiving.'
          imageSource={require('../assets/icons/freeReceiving.png')}
        /> :
        <></>}

      <ScrollView>

        <View style={choiceRowStyle.row}>
          <View style={styles.fieldHalf}>
            <ChoiceField
              title='Server'
              value={server === '' ? 'Not Defined' : server}
              onPress={() => props.navigation.navigate('FreeReceivingSettings')}
              style={{ borderBottomWidth: 0 }}
            />
          </View>
          <View style={styles.fieldHalf}>
            <ChoiceField
              title='Machine'
              value={machineName === '' ? 'Not Defined' : machineName}
              onPress={() => props.navigation.navigate('FreeReceivingSettings')}
              style={{ borderRightWidth: 0, borderBottomWidth: 0 }}
            />
          </View>
        </View>

        <DataField
          title='Stock File'
          data={stockText}
          toolTip={info === null ? '' : info.complete === 0 ? 'Download Not Completed' : info.downloadDate}
        />

        {/* Collected : lines | total qty, the lines to upload as a note */}
        <View style={styles.linesRow}>
          <View style={styles.linesCell}>
            <Text style={styles.linesNumberText} numberOfLines={1}>{summary.lines}</Text>
            <Text style={styles.linesLabelText} numberOfLines={1}>
              Collected Lines
              {summary.pending > 0 ? <Text style={styles.linesPendingText}>{`  ·  ${summary.pending} to upload`}</Text> : ''}
            </Text>
          </View>
          <View style={[styles.linesCell, styles.linesCellSeparator]}>
            <Text style={styles.linesNumberText} numberOfLines={1}>{FormatQuantity(summary.qty)}</Text>
            <Text style={styles.linesLabelText} numberOfLines={1}>Total Qty</Text>
          </View>
        </View>

        <View style={styles.buttonsContainer}>

          <MainButton title='Download' imageSource={require('../assets/icons/download.png')} onPress={() => BeforeDownload()} />

          <MainButton title='Upload Receiving' imageSource={require('../assets/icons/upload.png')} onPress={() => BeforeUpload()} />

          <MainButton title='Scan Items' imageSource={require('../assets/icons/scanItems.png')} onPress={() => {
            if (!stockReady) {
              alert('Please download the stock file first');
            }
            else if (info.suppliersStored === 0) {
              alert('No suppliers were downloaded, please download again');
            }
            else {
              props.navigation.navigate('FreeReceivingScan');
            }
          }} />

          <MainButton title='View Items' imageSource={require('../assets/icons/stock.png')} onPress={() => props.navigation.navigate('FreeReceivingItems')} />

          <MainButton title='Settings' imageSource={require('../assets/icons/settings.png')} onPress={() => props.navigation.navigate('FreeReceivingSettings')} />

        </View>

      </ScrollView>

    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    backgroundColor: Colors.bgColor,
    height: '100%',
  },
  fieldHalf: {
    width: '50%',
  },
  linesRow: {
    flexDirection: 'row',
    height: 50,
    borderColor: 'silver',
    borderBottomWidth: 1,
  },
  linesCell: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  linesCellSeparator: {
    borderLeftWidth: 1,
    borderLeftColor: 'silver',
  },
  linesPendingText: {
    color: 'red',
    fontWeight: 'bold',
  },
  linesNumberText: {
    color: 'black',
    fontSize: FontSize(18),
    fontWeight: 'bold',
  },
  linesLabelText: {
    color: Colors.brandBlue,
    fontSize: FontSize(13),
  },
  buttonsContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    alignItems: 'center',
    justifyContent: Sizing.isTablet ? 'flex-end' : 'space-between',
    width: '100%',
    padding: 10,
  },
  modalContainer: {
    width: '100%',
    height: '100%',
    backgroundColor: 'rgba(0,0,0,0.5)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  modalCard: {
    width: Sizing.windowsWidth * 0.9,
    backgroundColor: 'white',
    borderRadius: 20,
    overflow: 'hidden',
    elevation: 10,
    borderWidth: 5,
    borderColor: Colors.brandBlue,
    alignItems: 'center',
    justifyContent: 'center',
    padding: 20,
  },
  progressTitleText: {
    fontSize: FontSize(24),
    color: Colors.brandBlue,
    fontWeight: 'bold',
    marginBottom: 10,
  },
  progressDetailText: {
    fontSize: FontSize(17),
    color: 'black',
    marginBottom: 15,
  },
  progressBar: {
    width: '100%',
    height: 14,
    borderRadius: 7,
    backgroundColor: 'gainsboro',
    overflow: 'hidden',
    marginBottom: 20,
  },
  progressBarFill: {
    height: '100%',
    backgroundColor: Colors.brandGreen,
  },
  buttonCancel: {
    backgroundColor: Colors.brandBlue,
    width: 100,
    height: 40,
    alignItems: 'center',
    justifyContent: 'center',
    borderRadius: 15
  },
  buttonCancelText: {
    fontSize: FontSize(15),
    color: 'white',
  },
});

export default FreeReceivingScreen;
