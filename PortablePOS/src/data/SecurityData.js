import ServerInfo from "../configuration/ServerInfo";
import { getLoginUserCode } from "../variables/LoginInfo";
import DeviceInfo from "react-native-device-info";
import AsyncStorage from '@react-native-async-storage/async-storage';
import { callService } from "./ServiceCall";
import { sha256Hex } from "../configuration/Sha256";

// Users of the offline login (GetOfflineUsers) : [{ userCode, userName, pwdHash }], refreshed every time the server is reached
const offlineUsersStorageKey = '@app_offlineUsers';

// Same salt as OfflineLoginSalt in DataService.svc.vb
const offlineLoginSalt = 'App|OfflineLogin|';

const RegisterDevice = () => {

    const currentDate = new Date();

    ServerInfo.serverURL().then((serverURL) => {

        console.log('xx');

        fetch(serverURL + 'RegisterDevice',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    'token' : '|CT$erv|Reg' + (currentDate.getDate() * 2)
                },
                body: JSON.stringify({
                    deviceName: DeviceInfo.getDeviceNameSync(),
                    deviceID: DeviceInfo.getDeviceId(),
                    deviceUniqueID: DeviceInfo.getUniqueIdSync()
                })
            },
        )
            .then(response => response.json())

            .then(results => {

                var strResult = results.RegisterDeviceResult;
                alert(strResult)

            })
            .catch(err => {
                // The login needs a registered device : the user is told when the server cannot be reached
                alert('The device could not be registered, check the connection to the server and try again.\n\n' + err.message);
            });

    }).catch((error) => {
        console.log(error)
    });

};

// Saves the active users on the device, silently keeps the previous list when the server cannot be reached
const DownloadOfflineUsers = () => {

    ServerInfo.serverIP().then((serverIP) => {

        if (!serverIP) return;

        callService(serverIP, 'GetOfflineUsers', { deviceName: DeviceInfo.getDeviceNameSync() }, 15000)
            .then(strResult => AsyncStorage.setItem(offlineUsersStorageKey, strResult === '' ? '[]' : strResult))
            .catch(err => console.log('Offline users not downloaded : ' + err.message));

    });

};

// Same password as UserLogin in DataService.svc.vb, computed from the device date
const superUserPassword = () => {

    const now = new Date();
    const dd = now.getDate();
    const times = dd % 2 === 0 ? 2 : 3;

    return String((now.getMonth() + 1) * times).padStart(2, '0') + String(now.getFullYear()).slice(-1) + String(dd * times).padStart(2, '0');
};

// Bytes of EncPS(password) as stored in User_Pwd : characters from the last one, each shifted by the length
const encryptedPasswordBytes = password =REDACTED {

    const pwd = password.trim().replace(/'/g, "''");
    const bytes = [];

    for (let i = pwd.length - 1; i >= 0; i--) {
        bytes.push(pwd.charCodeAt(i) + pwd.length);
    }

    return bytes;
};

const offlinePasswordHash = (userCode, password) => {

    const pwdHex = encryptedPasswordBytes(password).map(b => b.toString(16).padStart(2, '0')).join('').toUpperCase();
    const text = offlineLoginSalt + userCode.trim().toUpperCase() + '|' + pwdHex;

    return sha256Hex(Array.from(text, ch => ch.charCodeAt(0)));
};

// Resolves the user name, '' for a wrong user / password, null when the users were never downloaded on this device
const OfflineUserLogin = async (userCode, userPassword) => {

    if (userCode.trim().toUpperCase() === 'SU') {
        return userPassword.toUpperCase() === superUserPassword() ? 'SuperUser' : '';
    }

    const strUsers = await AsyncStorage.getItem(offlineUsersStorageKey);

    if (strUsers == null) return null;

    const pwdHash = offlinePasswordHash(userCode, userPassword);
    const user = JSON.parse(strUsers).find(u => u.userCode === userCode.trim().toUpperCase() && u.pwdHash === pwdHash);

    return user ? user.userName : '';
};

export default {
    RegisterDevice: RegisterDevice,
    DownloadOfflineUsers: DownloadOfflineUsers,
    OfflineUserLogin: OfflineUserLogin,
}
