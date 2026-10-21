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
import TransferDetailsModal from '../components/lists/transfer/TransferDetailsModal';
import TransferData from '../data/TransferData';
import TransferDatabase from '../database/TransferDatabase';
import TransferSettings from '../configuration/TransferSettings';
import { FormatQuantity } from '../variables/TransferRules';

const twoDigits = (value) => value.toString().padStart(2, '0');

const FormatDate = (date) =>
  `${twoDigits(date.getDate())}/${twoDigits(date.getMonth() + 1)}/${date.getFullYear()} ${twoDigits(date.getHours())}:${twoDigits(date.getMinutes())}`;

// Transfer : download the stock of the location we transfer from (settings), prepare the transfer offline
// to one or multiple locations, then upload it
const TransferScreen = props => {

  const [server, setServer] = useState('');
  const [location, setLocation] = useState(null);
  const [machineName, setMachineName] = useState('');
  const [info, setInfo] = useState(null);
  const [header, setHeader] = useState({ multipleLocations: 0, destination: '', deliverBy: '', remarks: '' });
  const [summary, setSummary] = useState({ lines: 0, lots: 0, qty: 0, destinations: 0, pending: 0 });
  const [detailsVisible, setDetailsVisible] = useState(false);

  // { title, detail, percent } while downloading
  const [progress, setProgress] = useState(null);
  const cancelRequested = useRef(false);

  const LoadStatus = async () => {
    try {
      setServer(await TransferSettings.server());
      setLocation(await TransferSettings.location());
      setMachineName(await TransferSettings.machineName());
      setInfo(await TransferDatabase.GetInfo());
      setHeader(await TransferDatabase.GetHeader());
      setSummary(await TransferDatabase.GetSummary());
    } catch (error) {
      alert(error.message);
    }
  };

  useFocusEffect(
    useCallback(() => {
      LoadStatus();
    }, [])
  );

  // The downloaded stock is the one of the location of the settings
  const sameLocation = info !== null && location !== null && info.location === location.locationCode;
  const stockReady = sameLocation && info.complete === 1;

  //#region Download

  const BeforeDownload = () => {

    if (server === '') {
      alert('Please fill the server in the transfer settings');
    }
    else if (location === null) {
      alert('Please choose the location to transfer from in the transfer settings');
    }
    else if (machineName === '') {
      alert('Please fill the machine name in the transfer settings');
    }
    else if (info !== null && !sameLocation && summary.lines > 0) {
      alert(`The transfer has ${summary.lines} lines from location ${info.location}, upload or clear them before downloading location ${location.locationCode}`);
    }
    else if (sameLocation && info.complete === 0 && info.server === server) {
      Alert.alert('Download Not Completed', 'The last download of the stock did not finish.', [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Restart Download', onPress: () => RunDownload(true) },
        { text: 'Continue Download', onPress: () => RunDownload(false) },
      ]);
    }
    else if (info !== null) {
      Alert.alert('Download', `The stock of this device will be replaced by the stock of location ${location.locationCode}. The transfer being prepared is kept.`, [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Download', onPress: () => RunDownload(true) },
      ]);
    }
    else {
      RunDownload(true);
    }

  };

  // Downloads the pages of one list from the last record stored, until a page is not full
  const DownloadList = async (title, count, startPercent, total, fetchPage, savePage, storedOf, lastOf) => {

    const pageSize = await TransferSettings.pageSize();
    let current = await TransferDatabase.GetInfo();

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
        current = await TransferDatabase.GetInfo();
      }
      if (rows.length < pageSize) return current;

    }

  };

  // restart = false : continues the download in progress (on the server and location it was started on)
  const RunDownload = async (restart) => {

    cancelRequested.current = false;

    try {

      setProgress({ title: 'Download', detail: 'Connecting To The Server', percent: 0 });

      if (restart) {
        const locations = await TransferData.GetTransferLocations(server);
        const serverInfo = await TransferData.GetTransferInfo(server, location.locationCode);
        await TransferDatabase.StartDownload(server, serverInfo, locations);
      }

      const local = await TransferDatabase.GetInfo();
      const total = local.itemsCount + local.lotsCount + local.barcodesCount;

      await DownloadList('Items', local.itemsCount, 0, total,
        (after, pageSize) => TransferData.GetTransferItems(local.server, local.location, after, pageSize),
        rows => TransferDatabase.SaveItemsPage(rows),
        current => current.itemsStored, current => current.lastItemRecID);

      await DownloadList('Lots', local.lotsCount, local.itemsCount, total,
        (after, pageSize) => TransferData.GetTransferLots(local.server, local.location, after, pageSize),
        rows => TransferDatabase.SaveLotsPage(rows),
        current => current.lotsStored, current => current.lastLotRecID);

      await DownloadList('Barcodes', local.barcodesCount, local.itemsCount + local.lotsCount, total,
        (after, pageSize) => TransferData.GetTransferBarcodes(local.server, after, pageSize),
        rows => TransferDatabase.SaveBarcodesPage(rows),
        current => current.barcodesStored, current => current.lastBarcodeRecID);

      await TransferDatabase.CompleteDownload(FormatDate(new Date()));

      const done = await TransferDatabase.GetInfo();
      setProgress(null);
      alert(`Download Completed - ${done.itemsStored} Items, ${done.lotsStored} Lots With Stock`);

    } catch (error) {
      setProgress(null);
      alert(error.message);
    }

    LoadStatus();

  };

  //#endregion

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

            <TouchableOpacity style={styles.buttonCancel} onPress={() => { cancelRequested.current = true; }}>
              <Text style={styles.buttonCancelText}>Cancel</Text>
            </TouchableOpacity>

          </View>
        </View>
      </Modal>
    );
  };

  let stockToolTip = '';
  if (info !== null && !sameLocation) stockToolTip = `Location ${info.location}`;
  else if (info !== null && info.complete === 0) stockToolTip = 'Download Not Completed';
  else if (info !== null) stockToolTip = info.downloadDate;

  const wayOfWork = header.multipleLocations === 1 ? `Multiple Locations  ·  ${summary.destinations} Used`
    : header.destination !== '' ? `One Location  ·  To ${header.destination}` : 'One Location';

  return (
    <View style={styles.container}>

      {/* Called as a function so the modal is not re-created on every progress update */}
      {ProgressView()}

      <TransferDetailsModal
        visible={detailsVisible}
        deliverBy={header.deliverBy}
        remarks={header.remarks}
        onSave={(deliverBy, remarks) => {
          setDetailsVisible(false);
          TransferDatabase.SaveHeaderTexts(deliverBy, remarks).then(LoadStatus).catch(err => alert(err.message));
        }}
        onClose={() => setDetailsVisible(false)}
      />

      {/* The header only holds a description : left out on the small portable scanners so the screen fits without scrolling */}
      {!Sizing.isPortableScanner ?
        <ScreenHeader
          description='Transfer stock from this location to one or more locations.'
          imageSource={require('../assets/icons/transfer.png')}
        /> :
        <></>}

      <ScrollView>

        <ChoiceField
          title='From Location'
          value={location ? `${location.locationCode} - ${location.locationDescription}` : 'Not Selected'}
          onPress={() => props.navigation.navigate('TransferSettings')}
          style={{ borderRightWidth: 0 }}
        />

        <View style={choiceRowStyle.row}>
          <View style={styles.fieldHalf}>
            <ChoiceField
              title='Server'
              value={server === '' ? 'Not Defined' : server}
              onPress={() => props.navigation.navigate('TransferSettings')}
              style={{ borderBottomWidth: 0 }}
            />
          </View>
          <View style={styles.fieldHalf}>
            <ChoiceField
              title='Machine'
              value={machineName === '' ? 'Not Defined' : machineName}
              onPress={() => props.navigation.navigate('TransferSettings')}
              style={{ borderRightWidth: 0, borderBottomWidth: 0 }}
            />
          </View>
        </View>

        <DataField
          title='Stock'
          data={info === null ? 'Not Downloaded' : `${info.itemsStored} Items  ·  ${info.lotsStored} Lots With Stock`}
          toolTip={stockToolTip}
        />

        <ChoiceField
          title='Deliver By / Remarks'
          value={[header.deliverBy, header.remarks].filter(text => text !== '').join('  ·  ') || 'None'}
          onPress={() => setDetailsVisible(true)}
          style={{ borderRightWidth: 0 }}
        />

        {/* The transfer : items | qty, the way of work under the items */}
        <View style={styles.linesRow}>
          <View style={styles.linesCell}>
            <Text style={styles.linesNumberText} numberOfLines={1}>{summary.lots}</Text>
            <Text style={styles.linesLabelText} numberOfLines={1}>{wayOfWork}</Text>
          </View>
          <View style={[styles.linesCell, styles.linesCellSeparator]}>
            <Text style={styles.linesNumberText} numberOfLines={1}>{FormatQuantity(summary.qty)}</Text>
            <Text style={styles.linesLabelText} numberOfLines={1}>Total Qty</Text>
          </View>
        </View>

        <View style={styles.buttonsContainer}>

          <MainButton title='Download' imageSource={require('../assets/icons/download.png')} onPress={() => BeforeDownload()} />

          <MainButton title='Prepare Transfer' imageSource={require('../assets/icons/scanItems.png')} onPress={() => {
            if (!stockReady) {
              alert(info !== null && !sameLocation
                ? `The stock downloaded is the one of location ${info.location}, please download location ${location?.locationCode ?? ''}`
                : 'Please download the stock first');
            }
            else {
              props.navigation.navigate('TransferScan');
            }
          }} />

          <MainButton title='View Items' imageSource={require('../assets/icons/stock.png')} onPress={() => props.navigation.navigate('TransferItems')} />

          <MainButton title='Settings' imageSource={require('../assets/icons/settings.png')} onPress={() => props.navigation.navigate('TransferSettings')} />

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
    paddingHorizontal: 4,
  },
  linesCellSeparator: {
    borderLeftWidth: 1,
    borderLeftColor: 'silver',
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

export default TransferScreen;
