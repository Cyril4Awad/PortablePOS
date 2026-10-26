import AsyncStorage from '@react-native-async-storage/async-storage';
import { DEFAULT_PAGE_SIZE } from '../variables/PhysicalCountRules';
import { MachineName } from './MachineName';

// Device settings of the physical count (the location / server profiles are in PhysicalCountDatabase)
// The machine name is the name of the device (MachineName)

const pageSizeStorageKey = '@app_physicalCountPageSize';

const machineName = MachineName;

const pageSize = () => AsyncStorage.getItem(pageSizeStorageKey).then(result => result ? parseInt(result, 10) : DEFAULT_PAGE_SIZE);

const savePageSize = (value) => AsyncStorage.setItem(pageSizeStorageKey, value.toString());

export default {
    machineName,
    pageSize,
    savePageSize,
}
