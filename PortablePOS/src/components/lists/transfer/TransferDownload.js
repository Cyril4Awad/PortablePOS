import React, { useState, useRef } from 'react';
import { View, Text, StyleSheet, Modal, TouchableOpacity, Alert } from 'react-native';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';
import TransferData from '../../../data/TransferData';
import TransferDatabase from '../../../database/TransferDatabase';
import TransferSettings from '../../../configuration/TransferSettings';

const twoDigits = (value) => value.toString().padStart(2, '0');

const FormatDate = (date) =>
  `${twoDigits(date.getDate())}/${twoDigits(date.getMonth() + 1)}/${date.getFullYear()} ${twoDigits(date.getHours())}:${twoDigits(date.getMinutes())}`;

// The download of the stock of the location we transfer from (the location of the server) :
// Download(state) asks and downloads, ProgressModal() shows the progress, onDone() is called at the end
// state : { server, location, machineName, info, lines }
export const useTransferDownload = (onDone) => {

  // { title, detail, percent } while downloading
  const [progress, setProgress] = useState(null);
  const cancelRequested = useRef(false);

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
  const RunDownload = async (restart, server, location) => {

    cancelRequested.current = false;

    try {

      setProgress({ title: 'Download', detail: 'Connecting To The Server', percent: 0 });

      if (restart) {
        const locations = await TransferData.GetTransferLocations(server);
        // We transfer from the location of the server : the one saved in the settings may be the one chosen before
        const current = locations.find(row => Number(row.isCurrent) === 1);
        if (!current || current.locationCode !== location.locationCode) {
          throw new Error(current
            ? `The location of the server is ${current.locationCode}, open the transfer settings and save them`
            : 'The location of the server is not a transfer location');
        }
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

    onDone();

  };

  const Download = ({ server, location, machineName, info, lines }) => {

    const sameLocation = info !== null && location !== null && info.location === location.locationCode;

    if (server === '') {
      alert('Please fill the server in the transfer settings');
    }
    else if (location === null) {
      alert('The location of the server is not known, open the transfer settings and save them');
    }
    else if (machineName === '') {
      alert('Please fill the machine name in the transfer settings');
    }
    else if (info !== null && !sameLocation && lines > 0) {
      alert(`The transfer has ${lines} lines from location ${info.location}, upload or clear them before downloading location ${location.locationCode}`);
    }
    else if (sameLocation && info.complete === 0 && info.server === server) {
      Alert.alert('Download Not Completed', 'The last download of the stock did not finish.', [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Restart Download', onPress: () => RunDownload(true, server, location) },
        { text: 'Continue Download', onPress: () => RunDownload(false, server, location) },
      ]);
    }
    else if (info !== null) {
      Alert.alert('Download', `The stock of this device will be replaced by the stock of location ${location.locationCode}. The transfer being prepared is kept.`, [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Download', onPress: () => RunDownload(true, server, location) },
      ]);
    }
    else {
      RunDownload(true, server, location);
    }

  };

  // Called as a function so the modal is not re-created on every progress update
  const ProgressModal = () => (
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

  return { Download, ProgressModal };
};

const styles = StyleSheet.create({
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
