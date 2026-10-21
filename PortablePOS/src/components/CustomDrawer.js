import React , { useState, useEffect, useRef } from 'react';
import Colors from '../variables/Colors';
import {
    View,
    Text,
    ImageBackground,
    Image,
    TouchableOpacity,
    StyleSheet
} from 'react-native';
import {
    DrawerContentScrollView,
    DrawerItemList,
    useDrawerStatus
} from '@react-navigation/drawer';
import { getLoginUserCodeName } from '../variables/LoginInfo';
import { FontSize } from '../variables/ResponsiveFont';
import ServerInfo from '../configuration/ServerInfo';
import LoadingData from './LoadingData';
import ServerError from './ServerError';
import DeviceInfo from 'react-native-device-info';

const CustomDrawer = props => {

    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState(null);

    const UserLogout = () => { 
    
        ServerInfo.serverURL().then((serverURL) => {
        
            console.log(serverURL);
            
            setIsLoading(true);

            const request = fetch(serverURL + 'UserLogout',
                {
                    method: 'POST',
                    headers: {
                        Accept: 'application/json',
                        'Content-Type': 'application/json',
                        token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                    },
                    body: JSON.stringify({
                        deviceName: DeviceInfo.getDeviceNameSync()
                    })
                },
            );

            Promise.race([
                request,
                new Promise((resolve, reject) => setTimeout(() => reject(new Error('Server not reachable')), 5000)),
            ])
                .then(response => response.json())
                .then(results => {
    
                    var strResult = results.UserLogoutResult;
                    
                    if (strResult !== '') {
                        props.setIsLoggedIn(false)
                    }
                    else {
                        alert('Cannot logout now')
                    }

                    setIsLoading(false);
    
                })
                .catch(err => {
                    // Offline : logs out on the device only, the next online login of this device replaces the logged in user
                    setIsLoading(false);
                    props.setIsLoggedIn(false);
                });
    
        }).catch((error) => {
            console.log(error)
        });
    
    };

    return (
        <View style={styles.container}>

            <DrawerContentScrollView contentContainerStyle={styles.drawerContentScrollView}>

                <View style={styles.userImageContainer}>
                    <Image
                        style={styles.userImage}
                        source={require('./../assets/images/user.png')}
                        resizeMode="contain" />
                </View>

                <View style={styles.userContainer}>
                    <Text style={styles.userText}>{getLoginUserCodeName()}</Text>
                </View>

                <View style={styles.drawerItemListContainer}>
                    <DrawerItemList {...props} />
                </View>

            </DrawerContentScrollView>

            <View style={styles.footerContainer}>

                <View style={styles.logOutButtonContainer}>

                    <TouchableOpacity style={styles.logOutButton} onPress={()=> UserLogout()}>

                        <View style={styles.logOutButtonImageContainer}>
                            <Image
                                style={styles.logOutButtonImage}
                                source={require('./../assets/icons/logOut.png')}
                                resizeMode="contain" />
                        </View>

                        <View style={styles.logOutButtonTextContainer}>
                            <Text style={styles.logOutButtonText}>Log Out</Text>
                        </View>

                    </TouchableOpacity>

                </View>

            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: 'white',//exception
    },
    drawerContentScrollView: {
        backgroundColor: Colors.brandBlue,
    },
    userImageContainer: {
        alignItems: 'center',
        justifyContent: 'center',
    },
    userImage: {
        height: 100,
        width: 100,
        borderRadius: 50,
    },
    userContainer: {
        alignItems: 'center',
        justifyContent: 'center',
        height: 40,
    },
    userText: {
        fontSize: FontSize(21),
        color: Colors.navTitleColor,
    },
    drawerItemListContainer: {
        flex: 1,
        backgroundColor: 'white',//exception
    },
    footerContainer: {
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: Colors.brandGreen,
        height: 50,
    },
    logOutButtonContainer: {
        backgroundColor: Colors.brandBlue,
        alignItems: 'center',
        justifyContent: 'center',
        width: '60%',
        height: '80%',
        borderWidth: 1,
        borderColor: Colors.navTitleColor,
        borderRadius: 20,
        overflow: 'hidden',
    },
    logOutButton: {
        backgroundColor: Colors.brandGreen,
        alignItems: 'center',
        justifyContent: 'center',
        flexDirection: 'row',
        width: '100%',
        height: '100%',
    },
    logOutButtonImageContainer: {
        //backgroundColor: 'red',
        alignItems: 'flex-end',
        justifyContent: 'center',
        height: '100%',
        width: '45%',
    },
    logOutButtonImage: {
        height: '60%',
        width: '60%',
    },
    logOutButtonTextContainer: {
        //backgroundColor: 'blue',
        alignItems: 'flex-start',
        justifyContent: 'center',
        height: '100%',
        width: '55%',
    },
    logOutButtonText: {
        color: Colors.navTitleColor,
        fontSize: FontSize(18),
    }
});

export default CustomDrawer;