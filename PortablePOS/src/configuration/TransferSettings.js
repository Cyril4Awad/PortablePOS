import AsyncStorage from '@react-native-async-storage/async-storage';
import { DEFAULT_PAGE_SIZE } from '../variables/TransferRules';

// Device settings of Transfer : the server, the location we transfer from, machine name, download size
// and the search field of the scan screen

const serverStorageKey = '@app_transferServer';
const locationStorageKey = '@app_transferLocation';
const machineNameStorageKey = '@app_transferMachineName';
const pageSizeStorageKey = '@app_transferPageSize';
const searchFieldStorageKey = '@app_transferSearchField';

const server = () => AsyncStorage.getItem(serverStorageKey).then(result => result ?? '');

// { locationCode, locationDescription } or null
const location = () => AsyncStorage.getItem(locationStorageKey).then(result => result ? JSON.parse(result) : null).catch(() => null);

const machineName = () => AsyncStorage.getItem(machineNameStorageKey).then(result => result ?? '');

const pageSize = () => AsyncStorage.getItem(pageSizeStorageKey).then(result => result ? parseInt(result, 10) : DEFAULT_PAGE_SIZE);

const searchField = () => AsyncStorage.getItem(searchFieldStorageKey).then(result => result ?? '').catch(() => '');

const saveServer = (value) => AsyncStorage.setItem(serverStorageKey, value);

const saveLocation = (value) => AsyncStorage.setItem(locationStorageKey, JSON.stringify(value));

const saveMachineName = (value) => AsyncStorage.setItem(machineNameStorageKey, value);

const savePageSize = (value) => AsyncStorage.setItem(pageSizeStorageKey, value.toString());

const saveSearchField = (value) => AsyncStorage.setItem(searchFieldStorageKey, value).catch(() => { });

export default {
    server,
    location,
    machineName,
    pageSize,
    searchField,
    saveServer,
    saveLocation,
    saveMachineName,
    savePageSize,
    saveSearchField,
}
