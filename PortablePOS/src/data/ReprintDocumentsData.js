import AsyncStorage from '@react-native-async-storage/async-storage';
import { NativeModules } from 'react-native';
import ServerInfo from "../configuration/ServerInfo";
import DeviceInfo from "react-native-device-info";

// Reprint Documents : the server fills the Crystal Report of the ERP (A4 layout), then
// - prints it on one of its printers (the printers of the server PC, shared printers of other PCs included), or
// - exports it to PDF and the device prints the PDF with the print service of Android (ReportPrinterModule.java)

const reprintPrinterStorageKey = '@app_reprintPrinter';

// The printer choice that prints with the print service of the device instead of a printer of the server
const DevicePrinter = '@device';

const { ReportPrinter } = NativeModules;

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

// The types of documents of the proof of concept (code of the ERP)
const DocumentTypes = [
    { docType: 'RC', title: 'Client Receipt' },
    { docType: 'CS', title: 'Consumption' },
];

const formatDate = (date) => {
    const pad = (value) => String(value).padStart(2, '0');
    return `${pad(date.getDate())}/${pad(date.getMonth() + 1)}/${date.getFullYear()}`;
};

// Documents of the type between the two dates (Date objects), the newest first
// Resolves with [{ recID, docKey, docNumber, docDate, description, amount, currency, remarks }]
const GetReprintDocuments = (docType, fromDate, toDate) => {
    return callService('GetReprintDocuments', { docType: docType, fromDate: formatDate(fromDate), toDate: formatDate(toDate) })
        .then(result => result === '' ? [] : JSON.parse(result));
};

// The printers of the server : [{ printerName, isDefault }], its default printer first
const GetReprintPrinters = () => {
    return callService('GetReprintPrinters', {}).then(result => result === '' ? [] : JSON.parse(result));
};

// printerName : a printer of the server (the server prints the document on it),
// DevicePrinter : the PDF of the document opens in the print dialog of Android
const PrintDocument = (docType, recID, userCode, printerName, jobName) => {

    if (printerName !== DevicePrinter) {
        return callService('PrintReprintDocument', { docType: docType, recID: String(recID), userCode: userCode, printerName: printerName });
    }

    return callService('GetReprintDocumentPdf', { docType: docType, recID: String(recID), userCode: userCode })
        .then(pdfBase64 => ReportPrinter.printPdf(pdfBase64, jobName));
};

// The printer chosen last on this device (a printer name of the server or DevicePrinter), null the first time
const savedReprintPrinter = () => AsyncStorage.getItem(reprintPrinterStorageKey);

const saveReprintPrinter = (printerName) => AsyncStorage.setItem(reprintPrinterStorageKey, printerName);

export default {
    DocumentTypes,
    DevicePrinter,
    GetReprintDocuments,
    GetReprintPrinters,
    PrintDocument,
    savedReprintPrinter,
    saveReprintPrinter,
};
