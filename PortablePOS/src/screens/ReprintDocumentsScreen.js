import React, { useState, useEffect, useRef } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, FlatList } from 'react-native';
import Colors from '../variables/Colors';
import { FontSize } from '../variables/ResponsiveFont';
import LoadingData from '../components/LoadingData';
import ChoiceField from '../components/controls/ChoiceField';
import PhysicalCountFilterPicker from '../components/lists/physicalCount/PhysicalCountFilterPicker';
import ReprintDocumentsData from '../data/ReprintDocumentsData';
import { getLoginUserCode } from '../variables/LoginInfo';

// Periods of the list : the documents of the last days, today included
const PERIODS = [
  { title: 'Today', days: 0 },
  { title: '7 Days', days: 6 },
  { title: '30 Days', days: 29 },
  { title: '1 Year', days: 364 },
];

// Reprint Documents (proof of concept) : choose the printer, the type and the period, tap a document to print it
// Direct print of the ERP (Reprint Documents) : the server fills the A4 Crystal Report of the document and prints it on the
// chosen printer of the server, or sends it as a PDF and the print dialog of Android opens (This Device)
const ReprintDocumentsScreen = () => {

  const [docType, setDocType] = useState(ReprintDocumentsData.DocumentTypes[0].docType);
  const [period, setPeriod] = useState(PERIODS[2]);
  const [documents, setDocuments] = useState([]);
  const [isBusy, setIsBusy] = useState(false);

  // The printers of the server [{ printerName, isDefault }] and the one chosen (a printer name or DevicePrinter)
  const [printers, setPrinters] = useState([]);
  const [printerName, setPrinterName] = useState(ReprintDocumentsData.DevicePrinter);
  const [printerPickerVisible, setPrinterPickerVisible] = useState(false);

  // { text, isError }
  const [status, setStatus] = useState(null);

  // Set at the tap itself (isBusy only changes at the next render) : a second tap on a document never prints it twice
  const printing = useRef(false);

  // The printer chosen last on this device while the server still has it, else the default printer of the server
  useEffect(() => {
    Promise.all([ReprintDocumentsData.GetReprintPrinters(), ReprintDocumentsData.savedReprintPrinter()])
      .then(([serverPrinters, savedPrinter]) => {
        setPrinters(serverPrinters);
        if (savedPrinter === ReprintDocumentsData.DevicePrinter || serverPrinters.some(printer => printer.printerName === savedPrinter)) {
          setPrinterName(savedPrinter);
        } else if (serverPrinters.length > 0) {
          setPrinterName(serverPrinters[0].printerName);
        }
      })
      .catch(error => setStatus({ text: error.message, isError: true }));
  }, []);

  const printerTitle = (name) => name === ReprintDocumentsData.DevicePrinter ? 'This Device (Android Print)' : name;

  useEffect(() => {

    const toDate = new Date();
    const fromDate = new Date();
    fromDate.setDate(fromDate.getDate() - period.days);

    setIsBusy(true);
    setStatus(null);

    ReprintDocumentsData.GetReprintDocuments(docType, fromDate, toDate)
      .then(result => {
        setDocuments(result);
        if (result.length === 0) setStatus({ text: 'No Documents In This Period', isError: false });
      })
      .catch(error => {
        setDocuments([]);
        setStatus({ text: error.message, isError: true });
      })
      .finally(() => setIsBusy(false));

  }, [docType, period]);

  const PrintDocument = (document) => {

    if (isBusy || printing.current) return;
    printing.current = true;

    const typeTitle = ReprintDocumentsData.DocumentTypes.find(type => type.docType === docType).title;

    setIsBusy(true);
    setStatus({ text: `Preparing ${document.docNumber}...`, isError: false });

    ReprintDocumentsData.PrintDocument(docType, document.recID, getLoginUserCode(), printerName, `${typeTitle} ${document.docNumber}`)
      .then(() => setStatus({ text: `${document.docNumber} Sent To ${printerTitle(printerName)}`, isError: false }))
      .catch(error => setStatus({ text: error.message, isError: true }))
      .finally(() => {
        printing.current = false;
        setIsBusy(false);
      });

  };

  const Choices = ({ items, isSelected, onPress }) => (
    <View style={styles.choicesRow}>
      {items.map(item => (
        <TouchableOpacity
          key={item.title}
          style={[styles.choice, isSelected(item) ? styles.choiceSelected : null]}
          onPress={() => onPress(item)}
          disabled={isBusy}>
          <Text style={[styles.choiceText, isSelected(item) ? styles.choiceTextSelected : null]} numberOfLines={1}>{item.title}</Text>
        </TouchableOpacity>
      ))}
    </View>
  );

  const DocumentRow = ({ item }) => (
    <TouchableOpacity style={styles.documentRow} onPress={() => PrintDocument(item)}>
      <View style={styles.documentLine}>
        <Text style={styles.documentNumber} numberOfLines={1}>{item.docNumber}</Text>
        <Text style={styles.documentDate}>{item.docDate}</Text>
      </View>
      <View style={styles.documentLine}>
        <Text style={styles.documentDescription} numberOfLines={1}>{item.description}</Text>
        {item.amount !== null ?
          <Text style={styles.documentAmount}>{`${item.amount} ${item.currency}`}</Text> : null}
      </View>
      {item.remarks.trim() !== '' ?
        <Text style={styles.documentRemarks} numberOfLines={1}>{item.remarks.trim()}</Text> : null}
    </TouchableOpacity>
  );

  return (
    <View style={styles.container}>

      <ChoiceField
        title='Printer'
        value={printerTitle(printerName)}
        onPress={isBusy ? null : () => setPrinterPickerVisible(true)}
        style={styles.printerField} />

      <Choices
        items={ReprintDocumentsData.DocumentTypes}
        isSelected={type => type.docType === docType}
        onPress={type => setDocType(type.docType)} />

      <Choices
        items={PERIODS}
        isSelected={item => item.title === period.title}
        onPress={setPeriod} />

      {status ?
        <View style={[styles.statusContainer, { backgroundColor: status.isError ? 'firebrick' : Colors.brandBlue }]}>
          <Text style={styles.statusText} numberOfLines={1}>{status.text}</Text>
        </View> : null}

      <FlatList
        data={documents}
        keyExtractor={item => item.docKey}
        renderItem={DocumentRow} />

      <PhysicalCountFilterPicker
        visible={printerPickerVisible}
        title='Printer'
        options={[
          ...printers.map(printer => ({ code: printer.printerName, description: printer.printerName })),
          { code: ReprintDocumentsData.DevicePrinter, description: printerTitle(ReprintDocumentsData.DevicePrinter) },
        ]}
        selected={[printerName]}
        multiple={false}
        onSubmit={(selectedCodes) => {
          setPrinterName(selectedCodes[0]);
          ReprintDocumentsData.saveReprintPrinter(selectedCodes[0]);
          setPrinterPickerVisible(false);
        }}
        onCancel={() => setPrinterPickerVisible(false)} />

      {isBusy ? <LoadingData /> : null}

    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.bgColor,
  },
  printerField: {
    flex: 0,
    borderRightWidth: 0,
  },
  choicesRow: {
    flexDirection: 'row',
    paddingHorizontal: 5,
    paddingVertical: 4,
    borderColor: 'silver',
    borderBottomWidth: 1,
  },
  choice: {
    flex: 1,
    height: 38,
    marginHorizontal: 3,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: Colors.buttonBorderColor,
    backgroundColor: Colors.buttonBackColor1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  choiceSelected: {
    backgroundColor: Colors.brandBlue,
  },
  choiceText: {
    color: Colors.brandBlue,
    fontSize: FontSize(15),
    fontWeight: 'bold',
  },
  choiceTextSelected: {
    color: 'white',
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
  documentRow: {
    paddingHorizontal: 10,
    paddingVertical: 8,
    borderColor: 'silver',
    borderBottomWidth: 1,
  },
  documentLine: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  documentNumber: {
    flex: 1,
    color: Colors.brandBlue,
    fontSize: FontSize(17),
    fontWeight: 'bold',
  },
  documentDate: {
    color: 'black',
    fontSize: FontSize(15),
  },
  documentDescription: {
    flex: 1,
    color: 'black',
    fontSize: FontSize(16),
  },
  documentAmount: {
    color: 'black',
    fontSize: FontSize(16),
    fontWeight: 'bold',
    marginLeft: 8,
  },
  documentRemarks: {
    color: 'gray',
    fontSize: FontSize(14),
  },
});

export default ReprintDocumentsScreen;
