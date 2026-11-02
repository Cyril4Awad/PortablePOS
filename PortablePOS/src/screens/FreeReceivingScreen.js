import React, { useState, useCallback, useRef } from 'react';
import { View, Text, StyleSheet, ScrollView, Modal, TouchableOpacity } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import ScreenHeader from '../components/ScreenHeader';
import DataField from '../components/controls/DataField';
import MainButton from '../components/controls/MainButton';
import FreeReceivingData from '../data/FreeReceivingData';
import PhysicalCountData from '../data/PhysicalCountData';
import FreeReceivingDatabase from '../database/FreeReceivingDatabase';
import ServerInfo from '../configuration/ServerInfo';
import { FormatQuantity, PAGE_SIZE } from '../variables/FreeReceivingRules';
import { StartDraft, GetDraft } from '../variables/ReceivingOrderDraft';
import { UploadOrders, OrderName } from '../components/lists/freeReceiving/FreeReceivingSave';
import MessageBox from '../components/MessageBox';

const twoDigits = (value) => value.toString().padStart(2, '0');

const FormatDate = (date) =>
  `${twoDigits(date.getDate())}/${twoDigits(date.getMonth() + 1)}/${date.getFullYear()} ${twoDigits(date.getHours())}:${twoDigits(date.getMinutes())}`;

// Time taken by a download or an upload : "Worked for 12.4 (s)"
const WorkedFor = (startTime) => `Worked for ${((Date.now() - startTime) / 1000).toFixed(1)} (s)`;

const sameServer = (server1, server2) => (server1 ?? '').trim().toUpperCase() === (server2 ?? '').trim().toUpperCase();

const ItemsTotal = (count) => `${count} item(s) total including expiries`;

const OrdersText = (count) => `${count} receiving order${count === 1 ? '' : 's'}`;

// Free Receiving : the Receiving Order screens (Scan Items), online on the stock of the server, offline on the stock file downloaded
// from the server of the login screen (on its location). A receiving order is saved on the device then uploaded (at once when online)
const FreeReceivingScreen = props => {

  // The server of the login screen and its location (null offline)
  const [server, setServer] = useState('');
  const [serverLocation, setServerLocation] = useState(null);
  const [serverMessage, setServerMessage] = useState('');
  const [info, setInfo] = useState(null);
  const [summary, setSummary] = useState({ orders: 0, lines: 0, qty: 0, pending: 0, pendingOrders: 0 });

  // { title, detail, percent, cancellable } while downloading or uploading
  const [progress, setProgress] = useState(null);
  const cancelRequested = useRef(false);

  const LoadStatus = async () => {
    try {
      setInfo(await FreeReceivingDatabase.GetInfo());
      setSummary(await FreeReceivingDatabase.GetLinesSummary());

      const loginServer = ((await ServerInfo.serverIP()) ?? '').trim();
      setServer(loginServer);
      if (loginServer === '') {
        setServerLocation(null);
        setServerMessage('Please define the server on the login screen');
        return;
      }
      try {
        setServerLocation(await PhysicalCountData.GetServerLocation(loginServer));
        setServerMessage('');
      } catch (error) {
        setServerLocation(null);
        setServerMessage(`Server not reached : ${error.message}`);
      }
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

  // The stock file on the device is the one of the server and location of the login screen
  const sameLocation = info !== null && serverLocation !== null && sameServer(info.server, server) && info.location === serverLocation.location;

  //#region Download

  const BeforeDownload = () => {

    if (!serverLocation) {
      alert(serverMessage !== '' ? serverMessage : 'The location of the server is not loaded, connect to the server and try again');
    }
    else if (info !== null && !sameLocation && summary.pendingOrders > 0) {
      // The received items of the previous server + location would be left on the device
      alert(`Location ${info.location} has received items not uploaded to server ${info.server}, please upload them before working on location ${serverLocation.location} of server ${server}`);
    }
    else if (info !== null && info.complete === 0 && sameLocation) {
      MessageBox.alert('Download Not Completed', 'The last download of the stock file did not finish.', [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Restart Download', onPress: () => RunDownload(true) },
        { text: 'Continue Download', onPress: () => RunDownload(false) },
      ]);
    }
    else if (info !== null) {
      MessageBox.alert('Download', `The stock file of location ${serverLocation.location} will be downloaded again. The received items are kept.`, [
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

    let current = await FreeReceivingDatabase.GetInfo();

    while (true) {

      if (cancelRequested.current) throw new Error('Download cancelled, you can continue it later');

      const stored = storedOf(current);
      setProgress({
        title: 'Download',
        detail: `Synchronizing ${title} ${Math.min(stored, count)} / ${count}`,
        percent: total === 0 ? 0 : Math.min(1, (startPercent + Math.min(stored, count)) / total),
      });

      const rows = await fetchPage(lastOf(current), PAGE_SIZE);
      if (rows.length > 0) {
        await savePage(rows);
        current = await FreeReceivingDatabase.GetInfo();
      }
      if (rows.length < PAGE_SIZE) return current;

    }

  };

  // restart = false : continues the download in progress (on the server it was started on)
  const RunDownload = async (restart) => {

    cancelRequested.current = false;
    const startTime = Date.now();

    try {

      setProgress({ title: 'Download', detail: 'Connecting To The Server', percent: 0 });

      if (restart) {
        const serverInfo = await FreeReceivingData.GetReceivingInfo(server);
        await FreeReceivingDatabase.StartDownload(server, serverLocation.location, serverLocation.locationDescription, serverInfo);
      }

      const local = await FreeReceivingDatabase.GetInfo();
      const downloadServer = local.server;
      const total = local.itemsCount + local.barcodesCount + local.supplierRefsCount + local.lotsCount + local.suppliersCount;

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

      await DownloadList('Lots', local.lotsCount, local.itemsCount + local.barcodesCount + local.supplierRefsCount, total,
        (after, pageSize) => FreeReceivingData.GetReceivingLots(downloadServer, after, pageSize),
        rows => FreeReceivingDatabase.SaveLotsPage(rows),
        current => current.lotsStored, current => current.lastLotRecID);

      if (cancelRequested.current) throw new Error('Download cancelled, you can continue it later');

      setProgress({ title: 'Download', detail: `Synchronizing Suppliers ( ${local.suppliersCount} )`, percent: (total - local.suppliersCount) / Math.max(total, 1), cancellable: false });
      await FreeReceivingDatabase.SaveSuppliers(await FreeReceivingData.GetReceivingSuppliers(downloadServer));

      await FreeReceivingDatabase.CompleteDownload(FormatDate(new Date()));

      const done = await FreeReceivingDatabase.GetInfo();
      setProgress(null);
      alert(`Download Completed - Location ${done.location}\n${done.itemsStored} items, ${done.suppliersStored} suppliers, ${done.lotsStored} lots\n${WorkedFor(startTime)}`);

    } catch (error) {
      setProgress(null);
      alert(error.message);
    }

    LoadStatus();

  };

  //#endregion

  //#region Upload

  const BeforeUpload = () => {

    if (summary.pendingOrders === 0) {
      alert('No Received Items Found');
    }
    else {
      MessageBox.alert('Upload', `${OrdersText(summary.pendingOrders)} to save, ${ItemsTotal(summary.pending)}. Do you want to upload ?`, [
        { text: 'No', style: 'cancel' },
        { text: 'Yes', onPress: () => RunUpload() },
      ]);
    }

  };

  // All the receiving orders are sent in one call and saved in ONE transaction on the server : all of them or none.
  // If one fails (a check, the connection...) nothing is saved and they all stay "not uploaded" : correct it and upload again
  // (orders saved without the answer reaching the device are recognized by the server and not saved twice)
  const RunUpload = async () => {

    const startTime = Date.now();

    try {

      const orders = await FreeReceivingDatabase.GetPendingOrders();
      setProgress({ title: 'Upload', detail: `Sending ${OrdersText(orders.length)}`, percent: 0.5, cancellable: false });

      const results = await UploadOrders(orders);
      const saved = orders.map((order, index) => `RO # ${results[index].roRecID}  ( ${OrderName(order)} )`);
      const sentLines = orders.reduce((total, order) => total + order.draft.lines.length, 0);

      setProgress(null);
      alert(`Sending Finished - ${OrdersText(orders.length)}\n${saved.join('\n')}\n${ItemsTotal(sentLines)}\n${WorkedFor(startTime)}`);

    } catch (error) {
      setProgress(null);
      alert(`Nothing was uploaded, all the receiving orders stay on the device.\n\n${error.message}`);
    }

    LoadStatus();

  };

  // The Receiving Order screens : the new receiving order being prepared, or a new one
  // (not one opened from View Items : saved on the device or uploaded). Online the stock of the server, offline the stock file downloaded
  const ScanItems = () => {
    if (!serverLocation && !stockReady) {
      alert('Offline : download the stock file first');
      return;
    }
    const current = GetDraft('FREE');
    if (!current || current.orderID || current.mode === 'VIEW') StartDraft('ADD', null, 'FREE');
    props.navigation.navigate('ReceivingOrderEdit', { free: true });
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

  // The location of the server, offline the one of the stock file on the device
  const locationText = serverLocation
    ? `${serverLocation.location}${serverLocation.locationDescription ? ' - ' + serverLocation.locationDescription : ''}`
    : info !== null
      ? `${info.location}${info.locationDescription ? ' - ' + info.locationDescription : ''}`
      : 'Not Loaded';

  const stockText = info === null ? 'Not Downloaded'
    : `${info.itemsStored} Items  ·  ${info.suppliersStored} Suppliers${info.location !== serverLocation?.location && serverLocation ? `  ·  Location ${info.location}` : ''}`;

  return (
    <View style={styles.container}>

      {/* Called as a function so the modal is not re-created on every progress update */}
      {ProgressView()}

      {/* The header only holds a description : left out on the small portable scanners so the screen fits without scrolling */}
      {!Sizing.isPortableScanner ?
        <ScreenHeader
          description='Receive supplier items offline then upload them as receiving orders.'
          imageSource={require('../assets/icons/freeReceiving.png')}
        /> :
        <></>}

      <ScrollView>

        {/* The location of the server (read only) */}
        <DataField
          title='Location'
          data={locationText}
          toolTip={serverLocation ? '' : info !== null ? 'Offline' : ''}
          onPress={serverMessage !== '' ? () => alert(serverMessage) : undefined}
        />

        <DataField
          title='Stock File'
          data={stockText}
          toolTip={info === null ? '' : info.complete === 0 ? 'Download Not Completed' : info.downloadDate}
        />

        {/* The received lines on the device (not uploaded) | their total qty : 0 once uploaded */}
        <View style={styles.linesRow}>
          <View style={[styles.linesCell, styles.linesCellWide]}>
            <Text style={styles.linesNumberText} numberOfLines={1}>{summary.lines}</Text>
            <Text style={styles.linesLabelText} numberOfLines={1}>
              Received
              {summary.pendingOrders > 0 ? <Text style={styles.linesPendingText}>  ·  to upload</Text> : ''}
            </Text>
          </View>
          <View style={[styles.linesCell, styles.linesCellSeparator]}>
            <Text style={styles.linesNumberText} numberOfLines={1}>{FormatQuantity(Math.round(summary.qty * 1000))}</Text>
            <Text style={styles.linesLabelText} numberOfLines={1}>Total Qty</Text>
          </View>
        </View>

        <View style={styles.buttonsContainer}>

          <MainButton title='Download' imageSource={require('../assets/icons/download.png')} onPress={() => BeforeDownload()} />

          <MainButton title='Upload' imageSource={require('../assets/icons/upload.png')} onPress={() => BeforeUpload()} />

          <MainButton title='Scan Items' imageSource={require('../assets/icons/scanItems.png')} onPress={ScanItems} />

          <MainButton title='View Items' imageSource={require('../assets/icons/stock.png')} onPress={() => props.navigation.navigate('FreeReceivingItems')} />

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
  linesCellWide: {
    flex: 1.6,
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
    textAlign: 'center',
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
