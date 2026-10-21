//https://github.com/AllInOneYT/react-native-thermal-printer

import { PermissionsAndroid } from 'react-native';
import ThermalPrinterModule from 'react-native-thermal-printer';

const PrintSlipReport = async (text) => {

    //BluetoothEscposPrinter.printAndFeed(5)

    PermissionsAndroid.request(PermissionsAndroid.PERMISSIONS.BLUETOOTH_CONNECT).then(async (result) => {

  

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