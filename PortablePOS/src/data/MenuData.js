import ServerInfo from "../configuration/ServerInfo";
import { getLoginUserCode } from "../variables/LoginInfo";
import AsyncStorage from '@react-native-async-storage/async-storage';
import { callService } from "./ServiceCall";

// Last menu read from the server, used when the device is offline
const menuStorageKey = '@app_controlizedMenu';

//Returns { recKey: menuActive } from tbl_CPP_ControlizedMenu, or from the device when the server cannot be reached
const GetControlizedMenu = (
    { setMenuActiveKeys },
    { setIsLoading },
    { setError }) => {

    ServerInfo.serverIP().then((serverIP) => {

        setIsLoading(true);

        callService(serverIP, 'GetControlizedMenu', {}, 5000)
            .then(strResult => {

                var menuActiveKeys = {};
                if (strResult != '') {
                    JSON.parse(strResult).forEach(item => {
                        // The bit can come as "1" / "0", and "0" would be truthy
                        menuActiveKeys[item.recKey.trim()] = item.menuActive === true || String(item.menuActive).trim() === '1' || String(item.menuActive).trim().toLowerCase() === 'true';
                    });
                }

                AsyncStorage.setItem(menuStorageKey, JSON.stringify(menuActiveKeys));

                setMenuActiveKeys(menuActiveKeys);
                setIsLoading(false);

            })
            .catch(err => {

                AsyncStorage.getItem(menuStorageKey)
                    .then(strMenu => {
                        setIsLoading(false);
                        if (strMenu == null) {
                            setError(err.message);
                        }
                        else {
                            setMenuActiveKeys(JSON.parse(strMenu));
                        }
                    })
                    .catch(() => {
                        setIsLoading(false);
                        setError(err.message);
                    });

            });

    }).catch((error) => {
        console.log(error)
    });

};

//menuItems : [{ recKey, menuActive }]
const SaveControlizedMenu = (
    menuItems,
    { setIsLoading },
    onSaved) => {

    ServerInfo.serverIP().then((serverIP) => {

        setIsLoading(true);

        callService(serverIP, 'SaveControlizedMenu',
            {
                userCode: getLoginUserCode(),
                menuItems: JSON.stringify(menuItems)
            },
            15000)
            .then(result => {

                setIsLoading(false);

                if (result == 'Success') {
                    var menuActiveKeys = {};
                    menuItems.forEach(item => {
                        menuActiveKeys[item.recKey] = item.menuActive;
                    });
                    AsyncStorage.setItem(menuStorageKey, JSON.stringify(menuActiveKeys));

                    alert('Menu Items Saved');
                    onSaved();
                }
                else {
                    alert(result);
                }

            })
            .catch(err => {
                setIsLoading(false);
                alert('Cannot Save The Menu Items : ' + err.message);
            });

    }).catch((error) => {
        console.log(error)
    });

};

export default {
    GetControlizedMenu: GetControlizedMenu,
    SaveControlizedMenu: SaveControlizedMenu,
}
