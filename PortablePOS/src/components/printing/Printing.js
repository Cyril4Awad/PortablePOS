//https://github.com/AllInOneYT/react-native-thermal-printer

import { PermissionsAndroid } from 'react-native';
import ThermalPrinterModule from 'react-native-thermal-printer';

const PrintSlipReport = async (text) => {

    //BluetoothEscposPrinter.printAndFeed(5)

    // Asked only when printing (Android 12 and later), the screens do not ask it when they open
    // The result is not checked : Android 11 has no such permission
    PermissionsAndroid.requestMultiple([PermissionsAndroid.PERMISSIONS.BLUETOOTH_CONNECT, PermissionsAndroid.PERMISSIONS.BLUETOOTH_SCAN]).then(async (result) => {

  

    try {
      await ThermalPrinterModule.printBluetooth({
        payload: text,
        printerNbrCharactersPerLine: 38,
      });
    } catch (err) {
      //error handling
      console.log(err.message);
    }

})

  };


  export default {
    PrintSlipReport: PrintSlipReport,
  }