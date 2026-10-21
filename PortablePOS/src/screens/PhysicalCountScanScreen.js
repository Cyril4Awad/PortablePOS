import React, { useState, useEffect, useRef } from 'react';
import { View, Text, TextInput, StyleSheet, ScrollView, TouchableOpacity, Modal, Keyboard, Image } from 'react-native';
import Colors from '../variables/Colors';
import { FontSize } from '../variables/ResponsiveFont';
import DataField from '../components/controls/DataField';
import PhysicalCountItemPicker from '../components/lists/physicalCount/PhysicalCountItemPicker';
import PhysicalCountDatabase from '../database/PhysicalCountDatabase';
import { PlayScanOk, PlayScanError } from '../variables/PhysicalCountSounds';
import {
  AllowedDecimals, FormatQuantity, LookupScan, LineFields, CodeDescription,
  ScanCount, UpdateLastScanQty, CancelLastScan, UpdateTotalScanned
} from '../variables/PhysicalCountRules';

// Counting screen : the scanner types the barcode then Enter (keyboard wedge)
// Opened with a lineRecID (from View Items) the line is only shown, not counted
const PhysicalCountScanScreen = ({ route, navigation }) => {

  const { server, location, lineRecID } = route.params;
  const countKey = { server, location };

  // Downloaded header : decimals, trading / fashion / expiry modes, available qty privilege
  const [header, setHeader] = useState(null);
  const [decimals, setDecimals] = useState(0);

  const [scanner, setScanner] = useState('');
  const [line, setLine] = useState(null);
  const [totalText, setTotalText] = useState('');
  const [lastScannedText, setLastScannedText] = useState('');
  const [lastScanEnabled, setLastScanEnabled] = useState(false);

  // { text, isError }
  const [status, setStatus] = useState(null);
  // Scanned text when it is the start of more than one reference
  const [pickSearch, setPickSearch] = useState(null);
  // true : pickSearch is a barcode of more than one line
  const [pickByBarcode, setPickByBarcode] = useState(false);

  const scannerInputControl = useRef();
  // Scans and buttons run one at a time, in order, so fast scans never lose a count
  const queue = useRef(Promise.resolve());

  useEffect(() => {

    PhysicalCountDatabase.GetHeader(countKey).then(countHeader => {
      setHeader(countHeader);
      setDecimals(AllowedDecimals(countHeader.decimals));
    });

    if (lineRecID) {
      ShowLine(lineRecID, null, false);
    }

  }, []);

  const enqueue = (action) => {
    queue.current = queue.current.then(action).catch(error => {
      PlayScanError();
      setStatus({ text: error.message, isError: true });
    });
  };

  const focusScanner = () => {
    setTimeout(() => scannerInputControl.current?.focus(), 100);
  };

  // lastScanned : null keeps the last scan fields empty (line opened from the list)
  const ShowLine = async (recID, lastScanned, enableLastScan) => {
    const shownLine = await PhysicalCountDatabase.GetLine(countKey, recID);
    setLine(shownLine);
    setTotalText(FormatQuantity(shownLine.counted));
    setLastScannedText(lastScanned === null ? '' : FormatQuantity(lastScanned));
    setLastScanEnabled(enableLastScan);
  };

  // +1 on the line (scan or pick)
  const CountLine = async (recID) => {
    const currentLine = await PhysicalCountDatabase.GetLine(countKey, recID);
    const result = ScanCount(currentLine.counted);
    if (!result.ok) throw new Error(result.message);

    await PhysicalCountDatabase.SaveLineCounted(countKey, recID, result.total);
    await ShowLine(recID, result.lastScanned, result.lastScanEnabled);

    PlayScanOk();
    setStatus({ text: `${currentLine.itemReference} : ${FormatQuantity(result.total)}`, isError: false });
  };

  const NotFound = (text) => {
    setLine(null);
    setTotalText('');
    setLastScannedText('');
    setLastScanEnabled(false);
    PlayScanError();
    setStatus({ text, isError: true });
  };

  const HandleScan = (scannedText) => {

    enqueue(async () => {

      const result = await LookupScan(PhysicalCountDatabase, countKey, scannedText);

      switch (result.kind) {
        case 'found':
          await CountLine(result.lineRecID);
          break;
        case 'pick':
          setPickByBarcode(result.byBarcode === true);
          setPickSearch(result.value);
          break;
        case 'invalidQr':
          NotFound(`Invalid QR : ${scannedText.trim()}`);
          break;
        case 'notFound':
          NotFound(`Item Not Found : ${scannedText.trim()}`);
          break;
      }

    });

  };

  // Applies one of the 3 buttons to the shown line
  const ApplyButton = (computeResult) => {

    if (!line) return;

    enqueue(async () => {

      const result = computeResult();
      if (!result.ok) throw new Error(result.message);

      await PhysicalCountDatabase.SaveLineCounted(countKey, line.lineRecID, result.total);
      setLine({ ...line, counted: result.total, state: 1 });
      setTotalText(FormatQuantity(result.total));

      if (result.lastScanned !== undefined) {
        setLastScannedText(FormatQuantity(result.lastScanned));
        setLastScanEnabled(result.lastScanEnabled);
      }

      setStatus({ text: `${line.itemReference} : ${FormatQuantity(result.total)}`, isError: false });

    });

    focusScanner();

  };

  // The fields shown depend on the kind of item scanned (trading, fashion, with or without expiry)
  const fields = LineFields(header, line);

  return (
    <View style={styles.container}>

      <Modal visible={pickSearch !== null}>
        <PhysicalCountItemPicker
          countKey={countKey}
          searchString={pickSearch ?? ''}
          byBarcode={pickByBarcode}
          header={header}
          onPick={(recID) => {
            setPickSearch(null);
            enqueue(() => CountLine(recID));
            focusScanner();
          }}
          onCloseButtonPress={() => {
            setPickSearch(null);
            focusScanner();
          }}
        />
      </Modal>

      <View style={styles.scannerContainer}>

        <Image
          source={require('../assets/icons/barcode.png')}
          style={styles.scannerImage}
          resizeMode='contain'
        />

        <TextInput
          ref={scannerInputControl}
          style={styles.scannerText}
          value={scanner}
          onChangeText={setScanner}
          showSoftInputOnFocus={false}
          onPressIn={() => Keyboard.dismiss()}
          placeholder='Scan Item'
          maxLength={100}
          autoFocus={true}
          blurOnSubmit={false}
          onSubmitEditing={() => {
            HandleScan(scanner);
            setScanner('');
          }}
        />

      </View>

      <View style={[styles.statusContainer, { backgroundColor: status?.isError ? 'red' : Colors.brandBlue }]}>
        <Text style={styles.statusText} numberOfLines={1}>{status ? status.text : 'Ready to scan'}</Text>
      </View>

      <ScrollView keyboardShouldPersistTaps='handled'>

        <DataField title='Line' data={line ? line.lineRecID : '-'} toolTip={line?.state === 2 ? 'Uploaded' : line?.state === 1 ? 'Not Uploaded' : ''} />
        <DataField title='Reference' data={line ? line.itemReference : '-'} />
        <DataField title='Description' data={line ? line.itemDescription : '-'} />

        {fields.showBook ?
          <DataField title='Title' data={line.bookTitle} /> :
          <></>}

        {fields.showFashion ?
          <>
            <DataField title='Color' data={CodeDescription(line.colorCode, line.colorDescription) || '-'} />
            <DataField title='Grid' data={CodeDescription(line.gridCode, line.gridDescription) || '-'} />
            <DataField title='Size' data={CodeDescription(line.sizeOrder, line.sizeDescription) || '-'} />
          </> :
          <></>}

        {fields.showExpiry ?
          <>
            <DataField title='Expiry' data={line.expiry || '-'} />
            <DataField title='Lot' data={line.batch || '-'} />
          </> :
          <></>}

        {fields.showAvailableQty ?
          <DataField title='Avlb Qty' data={line && line.systemQty !== null ? FormatQuantity(line.systemQty) : '-'} /> :
          <></>}

        <View style={styles.qtyContainer}>

          <Text style={styles.qtyTitleText}>Total Qty Scanned</Text>

          <View style={styles.qtyRow}>
            <TextInput
              style={styles.qtyText}
              value={totalText}
              onChangeText={setTotalText}
              keyboardType='numeric'
              editable={line !== null}
              selectTextOnFocus={true}
            />
            <TouchableOpacity style={[styles.qtyButton, { opacity: line ? 1 : 0.4 }]} disabled={!line}
              onPress={() => ApplyButton(() => UpdateTotalScanned(totalText, decimals))}>
              <Text style={styles.qtyButtonText}>Update Total Scanned</Text>
            </TouchableOpacity>
          </View>

          <Text style={styles.qtyTitleText}>Last Scanned</Text>

          <View style={styles.qtyRow}>
            <TextInput
              style={styles.qtyText}
              value={lastScannedText}
              onChangeText={setLastScannedText}
              keyboardType='numeric'
              editable={lastScanEnabled}
              selectTextOnFocus={true}
            />
            <TouchableOpacity style={[styles.qtyButton, { opacity: lastScanEnabled ? 1 : 0.4 }]} disabled={!lastScanEnabled}
              onPress={() => ApplyButton(() => UpdateLastScanQty(totalText, lastScannedText, decimals))}>
              <Text style={styles.qtyButtonText}>Update Last Scan Qty</Text>
            </TouchableOpacity>
          </View>

          <TouchableOpacity style={[styles.cancelLastScanButton, { opacity: lastScanEnabled ? 1 : 0.4 }]} disabled={!lastScanEnabled}
            onPress={() => ApplyButton(() => CancelLastScan(totalText, lastScannedText, decimals))}>
            <Text style={styles.qtyButtonText}>Cancel Last Scan</Text>
          </TouchableOpacity>

        </View>

      </ScrollView>

      <View style={styles.footerContainer}>
        <TouchableOpacity style={styles.buttonOk} onPress={() => navigation.goBack()}>
          <Text style={styles.buttonOkText}>OK</Text>
        </TouchableOpacity>
      </View>

    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.bgColor,
  },
  scannerContainer: {
    backgroundColor: Colors.brandGreen,
    width: '100%',
    height: 55,
    padding: 5,
    flexDirection: 'row',
    alignItems: 'center',
  },
  scannerImage: {
    width: 40,
    height: '90%',
    marginRight: 5,
  },
  scannerText: {
    flex: 1,
    height: '100%',
    backgroundColor: 'white',
    color: 'black',
    fontSize: FontSize(18),
    borderWidth: 1,
    borderColor: 'gainsboro',
    borderRadius: 4,
    paddingLeft: 10,
  },
  statusContainer: {
    width: '100%',
    height: 35,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 10,
  },
  statusText: {
    color: 'white',
    fontSize: FontSize(17),
    fontWeight: 'bold',
  },
  qtyContainer: {
    padding: 10,
  },
  qtyTitleText: {
    color: Colors.brandBlue,
    fontSize: FontSize(15),
    marginTop: 5,
    marginBottom: 3,
  },
  qtyRow: {
    flexDirection: 'row',
    alignItems: 'center',
    height: 45,
  },
  qtyText: {
    width: '40%',
    height: '100%',
    backgroundColor: 'white',
    color: 'black',
    fontSize: FontSize(20),
    borderWidth: 1,
    borderColor: 'silver',
    borderRadius: 4,
    textAlign: 'center',
  },
  qtyButton: {
    flex: 1,
    height: '100%',
    marginLeft: 10,
    borderRadius: 15,
    backgroundColor: Colors.brandBlue,
    alignItems: 'center',
    justifyContent: 'center',
  },
  cancelLastScanButton: {
    height: 45,
    marginTop: 10,
    borderRadius: 15,
    backgroundColor: 'red',
    alignItems: 'center',
    justifyContent: 'center',
  },
  qtyButtonText: {
    color: 'white',
    fontSize: FontSize(16),
  },
  footerContainer: {
    backgroundColor: 'gainsboro',
    height: 60,
    width: '100%',
    alignItems: 'center',
    justifyContent: 'space-around',
    flexDirection: 'row',
    borderTopWidth: 3,
    borderTopColor: 'darkgray',
  },
  buttonOk: {
    backgroundColor: Colors.brandBlue,
    width: 100,
    height: 40,
    alignItems: 'center',
    justifyContent: 'center',
    borderRadius: 15
  },
  buttonOkText: {
    fontSize: FontSize(15),
    color: 'white',
  },
});

export default PhysicalCountScanScreen;
