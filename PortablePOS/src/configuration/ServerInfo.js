import AsyncStorage from '@react-native-async-storage/async-storage';
import { serviceURL } from '../data/ServiceCall';

const serverIpStorageKey = '@app_serverIP';
const printerStorageKey = '@app_printerName';

let serverURL = function readData() {

    return new Promise((resolve, reject)=>{
        try {
            AsyncStorage.getItem(serverIpStorageKey, (err, result) => {
                resolve(serviceURL(result));
            });
        }
        catch{
            reject('Cannot Get Server URL');
        }
    } ) 
};

let serverIP = function readData() {

    return new Promise((resolve, reject)=>{
        try {
            AsyncStorage.getItem(serverIpStorageKey, (err, result) => {
                resolve(result);
            });
            //resolve('0.0.0.0');
        }
        catch{
            reject('Cannot Get Server IP');
        }
    } ) 
};

let printerName = function readData() {

    return new Promise((resolve, reject)=>{
        try {
            AsyncStorage.getItem(printerStorageKey, (err, result) => {
                resolve(result);
            });
        }
        catch{
            reject('Cannot Get Printer Name')
        }
    } ) 
};

const successString = "Success";

export default {
    serverURL : serverURL,
    serverIP : serverIP,
    serverIpStorageKey: serverIpStorageKey,
    printerName: printerName,
    printerStorageKey: printerStorageKey,
    successString : successString,
    serverIpStorageKey: serverIpStorageKey
}