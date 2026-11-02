import React, { useState, useEffect, useRef } from 'react';
import { View, Text, TextInput, StyleSheet, TouchableOpacity, Keyboard, Image, Modal } from 'react-native';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import DataField from '../components/controls/DataField';
import ChoiceField, { choiceRowStyle, choiceTitleTextStyle } from '../components/controls/ChoiceField';
import MainButton from '../components/controls/MainButton';
import NumberPadModal from '../components/controls/NumberPadModal';
import LoadingData from '../components/LoadingData';
import ModalTitle from '../components/ModalTitle';
import PhysicalCountFilterPicker from '../components/lists/physicalCount/PhysicalCountFilterPicker';
import PrintLabelItemSearch from '../components/lists/printLabel/PrintLabelItemSearch';
import PrintLabelCodePicker from '../components/lists/printLabel/PrintLabelCodePicker';
import PrintLabelData from '../data/PrintLabelData';
import { PlayScanOk, PlayScanError } from '../variables/PhysicalCountSounds';
import { ItemRecIDFromLink } from '../variables/ItemLink';
import { useScanWithoutEnter } from '../variables/ScanInput';
import MessageBox from '../components/MessageBox';

const MAX_QTY = 500;

// Scan an item (keyboard wedge : the scanner types the code then Enter) or search it (the scan icon), see its prices and print its label
// Any code of the item is scanned : its references, its barcodes, its supplier references. The code printed as the barcode / QR
// of the label is the scanned one, it can be changed (Barcode / QR To Print)
// The server makes the label with the chosen BarTender design, the device prints it on its Bluetooth label printer
const PrintLabelScreen = () => {

  const [scanner, setScanner] = useState('');
  const scanWithoutEnter = useScanWithoutEnter(text => {
    HandleScan(text);
    setScanner('');
  });

  // { itemRecID, itemReference, itemDescription, itemUnit, priceLC, currencyLC, priceCC, currencyCC }
  const [item, setItem] = useState(null);
  // The codes of the item [{ group, title, value }] and the one printed as the barcode / QR (the code itself, not only its value :
  // a supplier reference can have the value of a reference of the item)
  const [codes, setCodes] = useState([]);
  const [printCode, setPrintCode] = useState(null);
  const [codePickerVisible, setCodePickerVisible] = useState(false);

  // Item search : { searchText, matches } while it is open
  const [search, setSearch] = useState(null);

  const [designs, setDesigns] = useState([]);
  const [labelDesign, setLabelDesign] = useState('');
  const [designPickerVisible, setDesignPickerVisible] = useState(false);

  // { name, address } of the Bluetooth printer of this device
  const [printer, setPrinter] = useState(null);
  const [pairedPrinters, setPairedPrinters] = useState([]);
  const [printerPickerVisible, setPrinterPickerVisible] = useState(false);

  const [qtyText, setQtyText] = useState('1');
  const [qtyPadVisible, setQtyPadVisible] = useState(false);
  const [isBusy, setIsBusy] = useState(false);
  // PNG (base 64) of the label made by the server, shown without printing
  const [previewImage, setPreviewImage] = useState('');

  // { text, isError }
  const [status, setStatus] = useState(null);

  const scannerInputControl = useRef();

  useEffect(() => {

    // The designs of tbl_LabelDesigns : the last one chosen on this device, else the default one (none when there is no design)
    Promise.all([PrintLabelData.GetLabelDesigns(), PrintLabelData.savedLabelDesign()])
      .then(([serverDesigns, savedDesign]) => {
        setDesigns(serverDesigns);
        const startDesign = serverDesigns.find(design => design.labelDesign === savedDesign) ??
          serverDesigns.find(design => design.isDefault === 1);
        if (startDesign) {
          setLabelDesign(startDesign.labelDesign);
        }
      })
      .catch(error => setStatus({ text: error.message, isError: true }));

    PrintLabelData.savedLabelPrinter().then(setPrinter);

  }, []);

  const focusScanner = () => {
    setTimeout(() => scannerInputControl.current?.focus(), 100);
  };

  const ShowError = (message) => {
    PlayScanError();
    setStatus({ text: message, isError: true });
  };

  // The item scanned (scannedText) or picked in the search (itemRecID)
  const LoadItem = (scannedText, itemRecID = '') => {

    setIsBusy(true);

    PrintLabelData.GetLabelItem(scannedText, itemRecID)
      .then(result => {

        // A code of more than one item : its items are shown to pick one
        if (result.matches) {
          PlayScanError();
          setStatus({ text: `${result.matches.length} Items Have This Code, Choose One`, isError: false });
          setSearch({ searchText: scannedText.trim(), matches: result.matches });
          return;
        }

        setItem(result.item);
        setCodes(result.codes);
        setPrintCode(result.codes.find(code => code.value === result.printCode) ?? null);
        setQtyText('1');
        PlayScanOk();
        setStatus({ text: result.item.itemReference, isError: false });

      })
      .catch(error => {
        setItem(null);
        setCodes([]);
        setPrintCode(null);
        ShowError(error.message);
      })
      .finally(() => {
        setIsBusy(false);
        focusScanner();
      });

  };

  const HandleScan = (scannedText) => {
    if (scannedText.trim() === '' || isBusy) return;
    // The QR of a label (link ending with the record id) : the item itself, its codes are shown to choose the one to print
    const linkRecID = ItemRecIDFromLink(scannedText);
    if (linkRecID !== null) LoadItem('', linkRecID);
    else LoadItem(scannedText);
  };

  const ChoosePrinter = () => {

    PrintLabelData.GetPairedPrinters()
      .then(devices => {
        if (devices.length === 0) {
          ShowError('Pair The Printer In The Bluetooth Settings First');
          return;
        }
        setPairedPrinters(devices);
        setPrinterPickerVisible(true);
      })
      .catch(error => ShowError(error.message));

  };

  const ChooseCode = () => {
    if (!item) {
      ShowError('Scan An Item First');
      return;
    }
    setCodePickerVisible(true);
  };

  // Design chosen, else opens its list : true when it can go on
  const CheckDesign = () => {
    if (labelDesign !== '') return true;
    setDesignPickerVisible(true);
    return false;
  };

  const Print = () => {

    if (isBusy) return;
    if (!item) {
      ShowError('Scan An Item First');
      return;
    }
    if (!CheckDesign()) return;

    if (!printer) {
      ChoosePrinter();
      return;
    }

    const qty = Number(qtyText);
    if (!Number.isInteger(qty) || qty < 1 || qty > MAX_QTY) {
      ShowError(`The Quantity Should Be Between 1 And ${MAX_QTY}`);
      return;
    }

    setIsBusy(true);
    setStatus({ text: 'Printing...', isError: false });

    PrintLabelData.PrintOnBluetooth(printer, item.itemRecID, printCode?.value ?? '', labelDesign, qty)
      .then(() => {
        setStatus({ text: `${qty} Label${qty > 1 ? 's' : ''} Printed : ${item.itemReference}`, isError: false });
      })
      .catch(error => ShowError(error.message))
      .finally(() => {
        setIsBusy(false);
        focusScanner();
      });

  };

  // Long press on the printer : test ruler to set up the position of the print on the stickers
  const PrintRuler = () => {

    if (isBusy) return;

    if (!printer) {
      ChoosePrinter();
      return;
    }

    setIsBusy(true);
    setStatus({ text: 'Printing Test Ruler...', isError: false });

    PrintLabelData.PrintRulerOnBluetooth(printer)
      .then(() => setStatus({ text: 'Test Ruler Printed', isError: false }))
      .catch(error => ShowError(error.message))
      .finally(() => {
        setIsBusy(false);
        focusScanner();
      });

  };

  const Preview = () => {

    if (isBusy) return;
    if (!item) {
      ShowError('Scan An Item First');
      return;
    }
    if (!CheckDesign()) return;

    setIsBusy(true);

    PrintLabelData.PreviewLabel(item.itemRecID, printCode?.value ?? '', labelDesign)
      .then(image => setPreviewImage(image))
      .catch(error => ShowError(error.message))
      .finally(() => setIsBusy(false));

  };

  const designDescription = designs.find(design => design.labelDesign === labelDesign)?.description;
  // A supplier reference shows its supplier (not printed, only to know which reference it is)
  const printCodeTitle = !printCode ? 'Barcode / QR To Print' :
    printCode.group === 'supplier' ? `Sup. Reference : ${printCode.title}` : `Barcode / QR To Print : ${printCode.title}`;

  const ChangeQty = (step) => {
    const qty = (parseInt(qtyText, 10) || 0) + step;
    setQtyText(String(Math.min(Math.max(qty, 1), MAX_QTY)));
  };

  return (
    <View style={styles.container}>

      <NumberPadModal
        visible={qtyPadVisible}
        title='Number Of Labels'
        preview={(value) => value < 1 || value > MAX_QTY ? `Between 1 And ${MAX_QTY}` : `Print ${value} Label${value > 1 ? 's' : ''}`}
        onSubmit={(value) => {
          if (value < 1 || value > MAX_QTY) return;
          setQtyText(String(value));
          setQtyPadVisible(false);
          focusScanner();
        }}
        onCancel={() => {
          setQtyPadVisible(false);
          focusScanner();
        }}
      />

      <PrintLabelItemSearch
        visible={search !== null}
        searchText={search?.searchText}
        matches={search?.matches}
        onPick={(itemRecID) => {
          setSearch(null);
          setStatus(null);
          LoadItem('', itemRecID);
        }}
        onClose={() => {
          setSearch(null);
          setStatus(null);
          focusScanner();
        }}
      />

      <PrintLabelCodePicker
        visible={codePickerVisible}
        codes={codes}
        selected={printCode}
        onSubmit={(code) => {
          setPrintCode(code);
          setCodePickerVisible(false);
          focusScanner();
        }}
        onCancel={() => {
          setCodePickerVisible(false);
          focusScanner();
        }}
      />

      <PhysicalCountFilterPicker
        visible={designPickerVisible}
        title='Label Design'
        options={designs.map(design => ({ code: design.labelDesign, description: design.description }))}
        selected={labelDesign === '' ? [] : [labelDesign]}
        multiple={false}
        onSubmit={(selectedCodes) => {
          setLabelDesign(selectedCodes[0]);
          PrintLabelData.saveLabelDesign(selectedCodes[0]);
          setDesignPickerVisible(false);
          focusScanner();
        }}
        onCancel={() => {
          setDesignPickerVisible(false);
          focusScanner();
        }}
      />

      <PhysicalCountFilterPicker
        visible={printerPickerVisible}
        title='Label Printer'
        options={pairedPrinters.map(device => ({ code: device.address, description: device.name }))}
        selected={printer ? [printer.address] : []}
        multiple={false}
        onSubmit={(selectedCodes) => {
          const chosenPrinter = pairedPrinters.find(device => device.address === selectedCodes[0]);
          setPrinter(chosenPrinter);
          PrintLabelData.saveLabelPrinter(chosenPrinter);
          setPrinterPickerVisible(false);
          focusScanner();
        }}
        onCancel={() => {
          setPrinterPickerVisible(false);
          focusScanner();
        }}
      />

      <Modal visible={previewImage !== ''} transparent={true} presentationStyle="overFullScreen"
        onRequestClose={() => { setPreviewImage(''); focusScanner(); }}>
        <View style={styles.modalContainer}>
          <View style={styles.modalCard}>
            <ModalTitle title={designDescription ?? 'Label'} onCloseButtonPress={() => { setPreviewImage(''); focusScanner(); }} />
            <Image
              source={{ uri: 'data:image/png;base64,' + previewImage }}
              style={styles.previewImage}
              resizeMode='contain'
            />
            {/* Prints the label shown */}
            <TouchableOpacity style={styles.previewPrintButton} onPress={() => { setPreviewImage(''); Print(); }}>
              <Image source={require('../assets/icons/reprintDocuments.png')} style={styles.previewPrintImage} resizeMode='contain' />
            </TouchableOpacity>
          </View>
        </View>
      </Modal>

      <View style={styles.scannerContainer}>

        {/* The icon opens the search of the item */}
        <TouchableOpacity style={styles.scannerButton} disabled={isBusy}
          onPress={() => setSearch({ searchText: '', matches: [] })}>
          <Image
            source={require('../assets/icons/barcode.png')}
            style={styles.scannerImage}
            resizeMode='contain'
          />
        </TouchableOpacity>

        <TextInput
          ref={scannerInputControl}
          style={styles.scannerText}
          value={scanner}
          onChangeText={text => { scanWithoutEnter.watch(text, scanner); setScanner(text); }}
          showSoftInputOnFocus={false}
          onPressIn={() => Keyboard.dismiss()}
          placeholder='Scan Item'
          maxLength={200}
          autoFocus={true}
          blurOnSubmit={false}
          onSubmitEditing={() => {
            scanWithoutEnter.cancel();
            HandleScan(scanner);
            setScanner('');
          }}
        />

      </View>

      {/* One line : a long message is cut, a tap shows all of it */}
      <TouchableOpacity style={[styles.statusContainer, { backgroundColor: status?.isError ? 'red' : Colors.brandBlue }]}
        disabled={!status}
        onPress={() => MessageBox.alert(status.isError ? 'Error' : 'Message', status.text, [{ text: 'OK', onPress: focusScanner }])}>
        <Text style={styles.statusText} numberOfLines={1}>{status ? status.text : 'Ready to scan'}</Text>
      </TouchableOpacity>

      <View style={styles.contentContainer}>

        <DataField title='Reference' data={item ? item.itemReference : '-'} toolTip={item?.itemUnit ? `Unit : ${item.itemUnit}` : ''} />
        {/* Two lines : the long descriptions are not cut on the first line */}
        <View style={styles.descriptionContainer}>
          <Text style={styles.descriptionTitleText}>Description</Text>
          <Text style={styles.descriptionText} numberOfLines={2}>{item ? item.itemDescription : '-'}</Text>
        </View>

        <View style={choiceRowStyle.row}>
          <ChoiceField title={printCodeTitle}
            value={item ? printCode?.value ?? 'Choose' : 'Scan An Item First'}
            onPress={ChooseCode}
            style={{ borderRightWidth: 0, borderBottomWidth: 0 }} />
        </View>

        <View style={styles.priceContainer}>
          <Text style={styles.priceText} numberOfLines={1}>
            {item ? Intl.NumberFormat("ar-de").format(item.priceLC) : '-'}
            <Text style={styles.currencyText}> {item ? item.currencyLC : ''}</Text>
          </Text>
          <Text style={styles.priceText} numberOfLines={1}>
            {item ? Intl.NumberFormat("ar-de", { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(item.priceCC) : '-'}
            <Text style={styles.currencyText}> {item ? item.currencyCC : ''}</Text>
          </Text>
        </View>

        <View style={choiceRowStyle.row}>
          <ChoiceField title='Printer' value={printer?.name ?? 'Choose'} onPress={ChoosePrinter} onLongPress={PrintRuler}
            style={{ borderBottomWidth: 0 }} />
          <ChoiceField title='Design' value={designs.length === 0 ? 'No Designs' : designDescription ?? 'Choose'}
            onPress={() => setDesignPickerVisible(true)}
            style={{ borderRightWidth: 0, borderBottomWidth: 0 }} />
        </View>

        {/* - / + change it by 1, a tap on the number opens the number pad */}
        <View style={styles.qtyRow}>
          <Text style={[choiceTitleTextStyle, styles.qtyTitleText]}>Number Of Labels</Text>
          <TouchableOpacity style={styles.qtyButton} onPress={() => ChangeQty(-1)}>
            <Text style={styles.qtyButtonText}>−</Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.qtyValueBox} onPress={() => setQtyPadVisible(true)}>
            <Text style={styles.qtyValueText}>{qtyText}</Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.qtyButton} onPress={() => ChangeQty(1)}>
            <Text style={styles.qtyButtonText}>+</Text>
          </TouchableOpacity>
        </View>

        <View style={styles.buttonsContainer}>
          <MainButton title='Preview' imageSource={require('../assets/icons/qr.png')} onPress={Preview} />
          <MainButton title='Print' imageSource={require('../assets/icons/reprintDocuments.png')} onPress={Print} />
        </View>

      </View>

      {isBusy ? <LoadingData /> : <></>}

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
  scannerButton: {
    width: 44,
    height: '100%',
    marginRight: 5,
    borderRadius: 6,
    backgroundColor: 'white',
    alignItems: 'center',
    justifyContent: 'center',
  },
  scannerImage: {
    width: 34,
    height: '80%',
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
  contentContainer: {
    flex: 1,
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
  priceContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-around',
    height: 44,
    paddingHorizontal: 5,
    borderColor: 'silver',
    borderBottomWidth: 1,
  },
  priceText: {
    color: Colors.brandBlue,
    fontSize: FontSize(24),
    flexShrink: 1,
  },
  currencyText: {
    color: Colors.brandGreen,
  },
  descriptionContainer: {
    height: 62,
    paddingHorizontal: 15,
    paddingVertical: 3,
    justifyContent: 'center',
    borderColor: 'silver',
    borderBottomWidth: 1,
  },
  descriptionTitleText: {
    color: Colors.brandBlue,
    fontSize: FontSize(15),
    fontWeight: 'bold',
  },
  descriptionText: {
    color: 'black',
    fontSize: FontSize(17),
  },
  qtyRow: {
    flexDirection: 'row',
    alignItems: 'center',
    height: 50,
    paddingHorizontal: 15,
    borderColor: 'silver',
    borderBottomWidth: 1,
  },
  qtyTitleText: {
    flex: 1,
  },
  qtyButton: {
    width: 40,
    height: 36,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: Colors.buttonBorderColor,
    backgroundColor: Colors.buttonBackColor1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  qtyButtonText: {
    color: Colors.brandBlue,
    fontSize: FontSize(22),
    fontWeight: 'bold',
  },
  qtyValueBox: {
    width: 70,
    height: 36,
    marginHorizontal: 8,
    borderRadius: 6,
    borderWidth: 1,
    borderColor: 'silver',
    backgroundColor: 'white',
    alignItems: 'center',
    justifyContent: 'center',
  },
  qtyValueText: {
    fontSize: FontSize(20),
    fontWeight: 'bold',
    color: Colors.brandBlue,
  },
  previewPrintButton: {
    position: 'absolute',
    right: 10,
    bottom: 10,
    width: 52,
    height: 52,
    borderRadius: 26,
    borderWidth: 1,
    borderColor: Colors.buttonBorderColor,
    backgroundColor: Colors.buttonBackColor1,
    alignItems: 'center',
    justifyContent: 'center',
    elevation: 6,
  },
  previewPrintImage: {
    width: 34,
    height: 34,
  },
  buttonsContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    width: '100%',
    paddingHorizontal: 10,
    paddingTop: 6,
  },
  modalContainer: {
    width: '100%',
    height: '100%',
    backgroundColor: 'rgba(0,0,0,0.5)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  modalCard: {
    width: Sizing.windowsWidth * 0.92,
    backgroundColor: 'white',
    borderRadius: 20,
    overflow: 'hidden',
    elevation: 10,
    borderWidth: 5,
    borderColor: Colors.brandBlue,
  },
  previewImage: {
    width: '100%',
    height: Sizing.windowsHeight * 0.4,
    backgroundColor: 'white',
  },
});

export default PrintLabelScreen;
