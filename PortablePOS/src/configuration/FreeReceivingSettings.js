import AsyncStorage from '@react-native-async-storage/async-storage';
import { DEFAULT_PAGE_SIZE } from '../variables/FreeReceivingRules';

// Device settings of Free Receiving, and the supplier / invoice / search field of the scan screen (kept for the next scans)

const serverStorageKey = '@app_freeReceivingServer';
const machineNameStorageKey = '@app_freeReceivingMachineName';
const pageSizeStorageKey = '@app_freeReceivingPageSize';
const removeCheckDigitStorageKey = '@app_freeReceivingRemoveCheckDigit';
const scanStateStorageKey = '@app_freeReceivingScanState';

const server = () => AsyncStorage.getItem(serverStorageKey).then(result => result ?? '');

const machineName = () => AsyncStorage.getItem(machineNameStorageKey).then(result => result ?? '');

const pageSize = () => AsyncStorage.getItem(pageSizeStorageKey).then(result => result ? parseInt(result, 10) : DEFAULT_PAGE_SIZE);

const removeCheckDigit = () => AsyncStorage.getItem(removeCheckDigitStorageKey).then(result => result === 'Y');

// { supplierRecID, invoiceNumber, searchField }
const scanState = () => AsyncStorage.getItem(scanStateStorageKey).then(result => result ? JSON.parse(result) : {}).catch(() => ({}));

const saveServer = (value) => AsyncStorage.setItem(serverStorageKey, value);

const saveMachineName = (value) => AsyncStorage.setItem(machineNameStorageKey, value);

const savePageSize = (value) => AsyncStorage.setItem(pageSizeStorageKey, value.toString());

const saveRemoveCheckDigit = (value) => AsyncStorage.setItem(removeCheckDigitStorageKey, value ? 'Y' : 'N');

const saveScanState = (value) => AsyncStorage.setItem(scanStateStorageKey, JSON.stringify(value)).catch(() => { });

export default {
    server,
    machineName,
    pageSize,
    removeCheckDigit,
    scanState,
    saveServer,
    saveMachineName,
    savePageSize,
    saveRemoveCheckDigit,
    saveScanState,
}
