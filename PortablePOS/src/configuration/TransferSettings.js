import AsyncStorage from '@react-native-async-storage/async-storage';
import ServerInfo from './ServerInfo';
import { MachineName } from './MachineName';

// Device settings of Transfer : the location we transfer from (the one of the server)
// The server is the one of the login screen, the machine name is the name of the device
// and the last side of the stock search (product groups / items)

const locationStorageKey = '@app_transferLocation';
const searchItemsModeStorageKey = '@app_transferSearchItemsMode';

const server = () => ServerInfo.serverIP().then(result => result ?? '');

// { locationCode, locationDescription } or null
const location = () => AsyncStorage.getItem(locationStorageKey).then(result => result ? JSON.parse(result) : null).catch(() => null);

const machineName = MachineName;

const searchItemsMode = () => AsyncStorage.getItem(searchItemsModeStorageKey).then(result => result === 'Y').catch(() => false);

const saveLocation = (value) => AsyncStorage.setItem(locationStorageKey, JSON.stringify(value));

const saveSearchItemsMode = (itemsMode) => AsyncStorage.setItem(searchItemsModeStorageKey, itemsMode ? 'Y' : 'N').catch(() => { });

export default {
    server,
    location,
    machineName,
    searchItemsMode,
    saveLocation,
    saveSearchItemsMode,
}
