import AsyncStorage from '@react-native-async-storage/async-storage';
import { DEFAULT_PAGE_SIZE } from '../variables/PhysicalCountRules';

// Device settings of the physical count (the location / server profiles are in PhysicalCountDatabase)

const machineNameStorageKey = '@app_physicalCountMachineName';
const pageSizeStorageKey = '@app_physicalCountPageSize';

const machineName = () => AsyncStorage.getItem(machineNameStorageKey).then(result => result ?? '');

const pageSize = () => AsyncStorage.getItem(pageSizeStorageKey).then(result => result ? parseInt(result, 10) : DEFAULT_PAGE_SIZE);

const saveMachineName = (value) => AsyncStorage.setItem(machineNameStorageKey, value);

const savePageSize = (value) => AsyncStorage.setItem(pageSizeStorageKey, value.toString());

export default {
    machineName,
    pageSize,
    saveMachineName,
    savePageSize,
}
