import React, { useState, useCallback, useRef } from 'react';
import { View, Text, StyleSheet, ScrollView, Modal, TouchableOpacity } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import DialogInput from 'react-native-dialog-input';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import ScreenHeader from '../components/ScreenHeader';
import DataField from '../components/controls/DataField';
import ChoiceField, { choiceRowStyle } from '../components/controls/ChoiceField';
import MainButton from '../components/controls/MainButton';
import PhysicalCountData from '../data/PhysicalCountData';
import PhysicalCountDatabase from '../database/PhysicalCountDatabase';
import PhysicalCountSettings from '../configuration/PhysicalCountSettings';
import { ToMilli, FormatForUpload, CheckCountNumber, FormatQuantity } from '../variables/PhysicalCountRules';
import MessageBox from '../components/MessageBox';
import { MachineNameMessage } from '../configuration/MachineName';

// Time taken by a download or an upload : "Worked for 12.4 (s)"
const WorkedFor = (startTime) => `Worked for ${((Date.now() - startTime) / 1000).toFixed(1)} (s)`;

// Physical count : download a count from the server of the selected location, count offline, then upload
const PhysicalCountScreen = props => {

  const [profile, setProfile] = useState(null);
  const [machineName, setMachineName] = useState('');
  const [header, setHeader] = useState(null);
  const [summary, setSummary] = useState({ notCounted: 0, pending: 0, uploaded: 0, countedQty: 0 });

  const [countNumberDialogVisible, setCountNumberDialogVisible] = useState(false);

  // { title, detail, percent } while downloading or uploading
  const [progress, setProgress] = useState(null);
  const cancelRequested = useRef(false);

  // Returns what was loaded ({ profile, machineName, header, summary }), null on error
  const LoadStatus = async () => {
    try {
      const selectedProfile = await PhysicalCountDatabase.GetSelectedProfile();
      const loadedMachineName = await PhysicalCountSettings.machineName();
      let loadedSummary = { notCounted: 0, pending: 0, uploaded: 0, countedQty: 0 };
      let loadedHeader = null;
      setProfile(selectedProfile);
      setMachineName(loadedMachineName);

      if (selectedProfile) {
        loadedHeader = await PhysicalCountDatabase.GetHeader(selectedProfile);
        loadedSummary = await PhysicalCountDatabase.GetLinesSummary(selectedProfile);
      }
      setHeader(loadedHeader);
      setSummary(loadedSummary);

      return { profile: selectedProfile, machineName: loadedMachineName, header: loadedHeader, summary: loadedSummary };
    } catch (error) {
      alert(error.message);
      return null;
    }
  };

  useFocusEffect(
    useCallback(() => {
      LoadStatus().then(status => {
        if (!status || !status.profile) return;
        // Opened by Create Physical Count : offer to download the new count (asked once)
        const newCountNumber = props.route.params?.newCountNumber;
        if (newCountNumber) {
          props.navigation.setParams({ newCountNumber: undefined });
          OfferNewCountDownload(newCountNumber, status);
        }
        // Opened by My Counts (the user already chose to download it)
        const downloadCountNumber = props.route.params?.downloadCountNumber;
        if (downloadCountNumber) {
          props.navigation.setParams({ downloadCountNumber: undefined });
          DownloadCount(downloadCountNumber, status);
        }
      });
    }, [props.route.params?.newCountNumber, props.route.params?.downloadCountNumber])
  );

  const countReady = header !== null && header.complete === 1;

  //#region Download

  const BeforeDownload = () => {

    if (!profile) {
      alert('Please select a location in the physical count settings');
    }
    else if (machineName === '') {
      alert(MachineNameMessage());
    }
    else if (header !== null && header.complete === 0) {
      MessageBox.alert('Download Not Completed', `The download of physical count # ${header.countNumber} did not finish.`, [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Restart Download', onPress: () => AskNewCount() },
        { text: 'Continue Download', onPress: () => RunDownload(null) },
      ]);
    }
    else {
      AskNewCount();
    }

  };

  // "Count # 82, still not uploaded ..." : the counted items of the count on the device would be lost by a new download
  const NotUploadedText = (deviceHeader, deviceSummary) =>
    `Count # ${deviceHeader?.countNumber ?? ''}, still not uploaded (${deviceSummary.pending} counted items total including expiries). Do you want to download anyway ? The counted quantities will be discarded.`;

  // Counted lines that are not uploaded would be lost by a new download
  const AskNewCount = () => {

    if (summary.pending > 0) {
      MessageBox.alert('Count Not Uploaded', NotUploadedText(header, summary), [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Download Anyway', style: 'destructive', onPress: () => setCountNumberDialogVisible(true) },
        { text: 'Upload First', onPress: () => BeforeUpload() },
      ]);
    }
    else {
      setCountNumberDialogVisible(true);
    }

  };

  // The count created by Create Physical Count : download now or later (with the Download button)
  // status : what LoadStatus loaded (the state is not updated yet when this runs)
  const OfferNewCountDownload = (countNumber, status) => {

    MessageBox.alert('Physical Count Created', `Physical count # ${countNumber} was created for machine ${status.machineName}. Do you want to download it now ?`, [
      { text: 'Later', style: 'cancel' },
      { text: 'Download', onPress: () => DownloadCount(countNumber, status) },
    ]);

  };

  // Downloads a count chosen in Create Physical Count (new count or My Counts)
  // Counted lines that are not uploaded would be lost by the download : asked first
  const DownloadCount = (countNumber, status) => {

    const download = () => RunDownload(countNumber, status.profile, status.machineName);

    if (status.summary.pending > 0) {
      MessageBox.alert('Count Not Uploaded', NotUploadedText(status.header, status.summary), [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Download Anyway', style: 'destructive', onPress: download },
      ]);
    }
    else {
      download();
    }

  };

  // countNumber null = continue the download in progress
  // countProfile / countMachineName : given when the state may not be loaded yet (new count)
  const RunDownload = async (countNumber, countProfile = profile, countMachineName = machineName) => {

    // The count of the selected server + location
    const countKey = { server: countProfile.server, location: countProfile.location };
    cancelRequested.current = false;
    const startTime = Date.now();

    try {

      setProgress({ title: 'Download', detail: 'Synchronizing Physical Count Header', percent: 0 });

      if (countNumber !== null) {
        const serverHeader = await PhysicalCountData.GetPhysicalCountHeader(countKey.server, countKey.location, countNumber, countMachineName);
        await PhysicalCountDatabase.StartDownload(countKey, countMachineName, serverHeader);
      }

      let localHeader = await PhysicalCountDatabase.GetHeader(countKey);
      const pageSize = await PhysicalCountSettings.pageSize();
      const total = localHeader.linesCount + localHeader.barcodesCount;

      while (localHeader.linesStored < localHeader.linesCount) {

        if (cancelRequested.current) throw new Error('Download cancelled, you can continue it later');

        setProgress({
          title: 'Download',
          detail: `Synchronizing Details ${localHeader.linesStored} / ${localHeader.linesCount}`,
          percent: total === 0 ? 0 : localHeader.linesStored / total
        });

        const lines = await PhysicalCountData.GetPhysicalCountDetails(localHeader.server, localHeader.location, localHeader.countNumber,
          localHeader.machineName, localHeader.linesStored + 1, pageSize);

        // The count changed on the server since the download started
        if (lines.length === 0) throw new Error(`The server returned ${localHeader.linesStored} of ${localHeader.linesCount} lines, please restart the download`);

        await PhysicalCountDatabase.SaveLinesPage(countKey, lines, ToMilli);
        localHeader = await PhysicalCountDatabase.GetHeader(countKey);

      }

      while (localHeader.barcodesStored < localHeader.barcodesCount) {

        if (cancelRequested.current) throw new Error('Download cancelled, you can continue it later');

        setProgress({
          title: 'Download',
          detail: `Synchronizing Barcodes ${localHeader.barcodesStored} / ${localHeader.barcodesCount}`,
          percent: (localHeader.linesCount + localHeader.barcodesStored) / total
        });

        const barcodes = await PhysicalCountData.GetPhysicalCountBarcodes(localHeader.server, localHeader.location, localHeader.countNumber,
          localHeader.machineName, localHeader.barcodesStored + 1, pageSize);

        if (barcodes.length === 0) throw new Error(`The server returned ${localHeader.barcodesStored} of ${localHeader.barcodesCount} barcodes, please restart the download`);

        await PhysicalCountDatabase.SaveBarcodesPage(countKey, barcodes);
        localHeader = await PhysicalCountDatabase.GetHeader(countKey);

      }

      await PhysicalCountDatabase.CompleteDownload(countKey);

      setProgress(null);
      alert(`Download Completed - Count # ${localHeader.countNumber}\n${localHeader.linesCount} items total including expiries\n${WorkedFor(startTime)}`);

    } catch (error) {
      setProgress(null);
      alert(error.message);
    }

    LoadStatus();

  };

  //#endregion

  //#region Upload

  const BeforeUpload = () => {

    if (!countReady) {
      alert('Please download the physical count first');
    }
    else if (summary.pending === 0) {
      alert('No Counted Items Found');
    }
    else {
      MessageBox.alert('Upload', `Count # ${header.countNumber} contains ${summary.pending} counted items total including expiries. Do you want to upload ?`, [
        { text: 'No', style: 'cancel' },
        { text: 'Yes', onPress: () => RunUpload() },
      ]);
    }

  };

  // All the counted lines are sent in one call and saved in one transaction on the server : all or nothing.
  // If the upload fails (connection lost, count posted or cancelled meanwhile...) nothing is saved on the server
  // and the lines stay "not uploaded" on the device, so the upload can simply be done again
  // (the totals replace the server values, sending them twice is harmless)
  const RunUpload = async () => {

    const startTime = Date.now();

    try {

      const pendingLines = await PhysicalCountDatabase.GetPendingLines(header);

      setProgress({ title: 'Upload', detail: `Sending ${pendingLines.length} Items`, percent: 0.5, cancellable: false });

      await PhysicalCountData.SavePhysicalCountLines(header.server, header.location, header.countNumber, header.machineName,
        pendingLines.map(line => ({ lineRecID: line.lineRecID, counted: FormatForUpload(line.counted) })));

      await PhysicalCountDatabase.MarkLinesUploaded(header, pendingLines.map(line => line.lineRecID));

      setProgress(null);
      alert(`Sending Finished - Count # ${header.countNumber}\n${pendingLines.length} items total including expiries\n${WorkedFor(startTime)}`);

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

  return (
    <View style={styles.container}>

      {/* Called as a function so the modal is not re-created on every progress update */}
      {ProgressView()}

      <DialogInput isDialogVisible={countNumberDialogVisible}
        title={'Download'}
        message={'Enter Physical Count Number'}
        hintInput={'Physical Count Number'}
        textInputProps={{ keyboardType: 'number-pad' }}
        submitInput={(countNumber) => {
          const errorMessage = CheckCountNumber(countNumber);
          if (errorMessage !== '') {
            alert(errorMessage);
          }
          else {
            setCountNumberDialogVisible(false);
            RunDownload(countNumber.trim());
          }
        }}
        closeDialog={() => { setCountNumberDialogVisible(false) }}
      >
      </DialogInput>

      {/* The header only holds a description : left out on the small portable scanners so the screen fits without scrolling */}
      {!Sizing.isPortableScanner ?
        <ScreenHeader
          description='Count stock offline then upload the counted quantities.'
          imageSource={require('../assets/icons/barcode.png')}
        /> :
        <></>}

      {/* Compact fields so everything fits on one screen, the scroll view only helps on very small screens */}
      <ScrollView>

        <ChoiceField
          title='Location'
          value={profile ? `${profile.location}${header?.locationDescription ? ' - ' + header.locationDescription : ''}` : 'Not Selected'}
          onPress={() => props.navigation.navigate('PhysicalCountSettings')}
          style={{ borderRightWidth: 0 }}
        />

        <View style={choiceRowStyle.row}>
          <View style={styles.fieldHalf}>
            <ChoiceField
              title='Server'
              value={profile ? profile.server : '-'}
              onPress={() => props.navigation.navigate('PhysicalCountSettings')}
              style={{ borderBottomWidth: 0 }}
            />
          </View>
          <View style={styles.fieldHalf}>
            <ChoiceField
              title='Machine'
              value={machineName === '' ? 'Not Valid' : machineName}
              onPress={() => props.navigation.navigate('PhysicalCountSettings')}
              style={{ borderRightWidth: 0, borderBottomWidth: 0 }}
            />
          </View>
        </View>

        <DataField title='Physical Count' data={header ? `# ${header.countNumber}  ( ${header.warehouse} - ${header.dateCount} )` : 'Not Downloaded'} toolTip={header && header.complete === 0 ? 'Download Not Completed' : ''} />

        {/* Lines : not counted | counted, the upload state is a note of the counted lines (not uploaded lines are counted lines) */}
        <View style={styles.linesRow}>
          <View style={styles.linesCell}>
            <Text style={styles.linesNumberText} numberOfLines={1}>{summary.notCounted}</Text>
            <Text style={styles.linesLabelText} numberOfLines={1}>Not Counted</Text>
          </View>
          <View style={[styles.linesCell, styles.linesCellSeparator, styles.linesCellWide]}>
            <Text style={styles.linesNumberText} numberOfLines={1}>{summary.pending + summary.uploaded}</Text>
            <Text style={styles.linesLabelText} numberOfLines={1}>
              Counted
              {summary.pending > 0 ?
                <Text style={styles.linesPendingText}>{`  ·  ${summary.pending} to upload`}</Text> :
                summary.uploaded > 0 ? '  ·  all uploaded' : ''}
            </Text>
          </View>
          <View style={[styles.linesCell, styles.linesCellSeparator]}>
            <Text style={styles.linesNumberText} numberOfLines={1}>{FormatQuantity(summary.countedQty)}</Text>
            <Text style={styles.linesLabelText} numberOfLines={1}>Total Qty</Text>
          </View>
        </View>

        <View style={styles.buttonsContainer}>

          <MainButton title='Download' imageSource={require('../assets/icons/download.png')} onPress={() => BeforeDownload()} />

          <MainButton title='Upload' imageSource={require('../assets/icons/upload.png')} onPress={() => BeforeUpload()} />

          {/* The scan screen of the count (the former Scan For Testing), the older PhysicalCountScan screen is not shown */}
          <MainButton title='Scan Items/QR' imageSource={require('../assets/icons/scanItems.png')} secondImageSource={require('../assets/icons/qr.png')} onPress={() => {
            if (countReady) {
              props.navigation.navigate('PhysicalCountScanTesting', { server: header.server, location: header.location });
            }
            else {
              alert('Please download the physical count first');
            }
          }} />

          <MainButton title='View Items' imageSource={require('../assets/icons/stock.png')} onPress={() => {
            if (countReady) {
              props.navigation.navigate('PhysicalCountItems', { server: header.server, location: header.location });
            }
            else {
              alert('Please download the physical count first');
            }
          }} />

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
  fieldsRow: {
    flexDirection: 'row',
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

export default PhysicalCountScreen;
