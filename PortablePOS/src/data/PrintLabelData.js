import AsyncStorage from '@react-native-async-storage/async-storage';
import { NativeModules, PermissionsAndroid, Platform } from 'react-native';
import ServerInfo from "../configuration/ServerInfo";
import DeviceInfo from "react-native-device-info";

// Print Label : the server makes the label with BarTender from the designs of tbl_CPP_LabelDesigns,
// the device sends it to its Bluetooth label printer (LabelPrinterModule.java)

const labelPrinterStorageKey = '@app_printLabelPrinter';
const labelDesignStorageKey = '@app_printLabelDesign';

const { LabelPrinter } = NativeModules;

// Returns the result string of the method, rejects with the error message
const callService = (method, body) => {

    return ServerInfo.serverURL().then((serverURL) =>

        fetch(serverURL + method,
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token: DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify(body)
            },
        )
            .then(response => response.json())
            .then(results => {

                var strResult = results[method + 'Result'];

                if (strResult == null) {
                    throw new Error('No response from the server');
                }

                if (strResult.startsWith('Error')) {
                    throw new Error(strResult.replace(/^Error /, ''));
                }

                return strResult;

            })
    );

};

// Scanned code (any reference, barcode or supplier reference of the item) or the item picked in the search (itemRecID)
// Resolves with { item: { itemRecID, itemReference, itemDescription, itemUnit, priceLC, currencyLC, priceCC, currencyCC },
//                 codes: [{ group: 'coding' | 'barcodes' | 'supplier', title, value }], printCode }
// or { matches: [{ itemRecID, itemReference, itemDescription }] } when the scanned code belongs to more than one item
const GetLabelItem = (barcode, itemRecID = '') => {
    return callService('GetLabelItem', { barcode: barcode.trim(), itemRecID: itemRecID }).then(result => JSON.parse(result));
};

// Items with a reference, barcode or supplier reference containing the text : [{ itemRecID, itemReference, itemDescription }]
const SearchLabelItems = (searchText) => {
    return callService('SearchLabelItems', { searchText: searchText.trim() }).then(result => result === '' ? [] : JSON.parse(result));
};

// Resolves with [{ labelDesign (code), description (name), isDefault }], the default design first
const GetLabelDesigns = () => {
    return callService('GetLabelDesigns', {}).then(result => result === '' ? [] : JSON.parse(result));
};

// Resolves with the PNG image (base 64) of the label, nothing is printed
const PreviewLabel = (itemRecID, printCode, labelDesign) => {
    return callService('PreviewLabel', { itemRecID: itemRecID, printCode: printCode, labelDesign: labelDesign });
};

// Android 12 and later ask the user before an app connects to Bluetooth devices
const askBluetoothPermission = async () => {
    if (Platform.OS !== 'android' || Platform.Version < 31) return;
    const result = await PermissionsAndroid.request(PermissionsAndroid.PERMISSIONS.BLUETOOTH_CONNECT);
    if (result !== PermissionsAndroid.RESULTS.GRANTED) {
        throw new Error('Bluetooth Permission Is Needed To Print');
    }
};

// Resolves with [{ name, address }] of the paired Bluetooth devices
const GetPairedPrinters = async () => {
    await askBluetoothPermission();
    return LabelPrinter.getPairedDevices();
};

// Sends the printer commands to the printer, the errors of Bluetooth are told in plain words
// (LabelPrinterModule rejects with the code Bluetooth, Connect or Send)
const sendToPrinter = async (printer, printData) => {
    try {
        await LabelPrinter.print(printer.address, printData);
    } catch (error) {
        switch (error.code) {
            case 'Connect':
                throw new Error(`Cannot Connect To The Printer ${printer.name}. Check It Is Turned On, Near This Device And Not Connected To Another Device, Then Try Again`);
            case 'Send':
                throw new Error(`The Connection To The Printer ${printer.name} Was Lost While Printing. Check The Printer (Paper, Battery) And Try Again`);
            default:
                throw new Error(error.message);
        }
    }
};

// The server makes the printer commands of qty labels, the device sends them to its printer ({ name, address })
const PrintOnBluetooth = async (printer, itemRecID, printCode, labelDesign, qty) => {
    await askBluetoothPermission();
    const printData = await callService('GetLabelPrintData', { itemRecID: itemRecID, printCode: printCode, labelDesign: labelDesign, qty: qty });
    await sendToPrinter(printer, printData);
};

// Test print to set up the printer : a ruler from the first dot of the print head (LabelOffsetXMM of the server)
const PrintRulerOnBluetooth = async (printer) => {
    await askBluetoothPermission();
    const printData = await callService('GetLabelRulerData', {});
    await sendToPrinter(printer, printData);
};

// The design chosen on this device, chosen again when the screen opens
const savedLabelDesign = () => AsyncStorage.getItem(labelDesignStorageKey).then(result => result ?? '');

const saveLabelDesign = (value) => AsyncStorage.setItem(labelDesignStorageKey, value);

// The printer of this device : { name, address } or null
const savedLabelPrinter = () => AsyncStorage.getItem(labelPrinterStorageKey).then(result => result ? JSON.parse(result) : null);

const saveLabelPrinter = (printer) => AsyncStorage.setItem(labelPrinterStorageKey, JSON.stringify(printer));

export default {
    GetLabelItem,
    SearchLabelItems,
    GetLabelDesigns,
    PreviewLabel,
    GetPairedPrinters,
    PrintOnBluetooth,
    PrintRulerOnBluetooth,
    savedLabelDesign,
    saveLabelDesign,
    savedLabelPrinter,
    saveLabelPrinter,
}
