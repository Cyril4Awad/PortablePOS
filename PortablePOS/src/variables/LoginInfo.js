import AsyncStorage from '@react-native-async-storage/async-storage';

const LoginUserCodeStorageKey = '@app_loginUserCode';
const LoginUserNameStorageKey = '@app_loginUserName';
const LoginUserCodeNameStorageKey = '@app_loginUserCodeName';

const date = new Date();

let day = date.getDate();
let month = date.getMonth() + 1;
let year = date.getFullYear();
let lcCurrentDate = `${day}-${month}-${year}`; //replace by POS Working Day

let lcLoginUserCode = '';
let lcLoginUserName = '';
let lcLoginUserCodeName = `${lcLoginUserCode} - ${lcLoginUserName}`;

export function setLoginUserCode(userCode) {
    lcLoginUserCode = userCode;
};

export function setLoginUserName(userName) {
    lcLoginUserName = userName;
};

export function getLoginUserCode() {
    return lcLoginUserCode;
};

export function getLoginUserName() {
    return lcLoginUserName;
};

export function getLoginUserCodeName() {
    return `${lcLoginUserCode} - ${lcLoginUserName}` ;
};

export function getCurrentDate() {
    return lcCurrentDate;
};

