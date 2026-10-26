import DeviceInfo from 'react-native-device-info';
import { CheckMachineName } from '../variables/PhysicalCountRules';

// The machine name is the name of the device (Settings of the device > About phone > Device name), read only in the app
// Same name as the one sent at the login. Physical Count, Free Receiving and Transfer all use it

export const DeviceName = () => (DeviceInfo.getDeviceNameSync() ?? '').trim().toUpperCase();

// '' when the name of the device cannot be a machine name (1 to 5 letters or digits)
export const MachineName = () => Promise.resolve(CheckMachineName(DeviceName()) === '' ? DeviceName() : '');

export const MachineNameMessage = () =>
    `The name of this device (${DeviceName() || 'empty'}) is not a valid machine name : it must be 1 to 5 letters or digits.\n\nChange it in the Settings of the device : About phone > Device name.`;
