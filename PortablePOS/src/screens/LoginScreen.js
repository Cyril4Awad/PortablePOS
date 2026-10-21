import React, { useState, useEffect, useRef }  from 'react';
import CompanyLogo from '../components/CompanyLogo';
import Sizing from '../variables/Sizing';
import Colors from '../variables/Colors';
import { FontSize } from '../variables/ResponsiveFont';
import DialogInput from 'react-native-dialog-input';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { View, Text, TextInput, StyleSheet, TouchableOpacity, ImageBackground, Image } from 'react-native';
import { setLoginUserCode , setLoginUserName } from '../variables/LoginInfo';
import ServerInfo from '../configuration/ServerInfo';
import LoadingData from '../components/LoadingData';
import ServerError from '../components/ServerError';
import DeviceInfo from 'react-native-device-info';
import SecurityData from '../data/SecurityData';
import PosOperationsData from '../data/PosOperationsData';
import { callService } from '../data/ServiceCall';

// After that time the server is considered unreachable and the offline login is used
const loginTimeoutMs = 8000;

const LoginScreen = props => {

    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState(null);

    const [userCode, setUserCode] = useState('');
    const [userPassword, setUserPassword] = useState('');

    const [serverIpDialogIsOpened, setServerIpDialogIsOpened] = useState(false);
    const [serverIP, setServerIP] = useState('');

    useEffect(() => {

        getServerIP();
        SecurityData.DownloadOfflineUsers();

    }, [])

    const getServerIP = () => {
        ServerInfo.serverIP().then((srvIP) => {
            setServerIP(srvIP);
        })
    };

    const changeServerIP = (newIP) => {
        setServerIpDialogIsOpened(false);
        saveServerIP(newIP);
        getServerIP();
    };

    const saveServerIP = async (newIP) => {
        try {
            await AsyncStorage.setItem(ServerInfo.serverIpStorageKey, newIP);
            alert('Server IP successfully saved');
        } catch (e) {
            alert('Failed to Save the data to the storage.')
        }
    };

    const UserLogin = () => {

        setIsLoading(true);

        callService(serverIP, 'UserLogin',
            {
                userCode: userCode,
                userPassword: userPassword,
                deviceName: DeviceInfo.getDeviceNameSync()
            },
            loginTimeoutMs)
            .then(strResult => {

                if (strResult == '') {
                    setLoginUserName('');
                    alert('Invalid User Name / Password');
                } else if (strResult === 'UAL') {
                    setLoginUserName('');
                    alert('User already logged in');
                }
                else {

                    var userName = JSON.parse(strResult);

                    // Set the user before logging in, the home drawer reads it on first render (Menu Items is su only)
                    setLoginUserCode(userCode);
                    setLoginUserName(userName[0].UserName);
                    props.setIsLoggedIn(true);

                    PosOperationsData.GetCashRegister();
                    SecurityData.DownloadOfflineUsers();

                }

                setIsLoading(false);

            })
            .catch(err => {
                OfflineUserLogin(err);
            });

    };

    // Server not reachable : checks the users downloaded on the device (no "already logged in" check offline)
    const OfflineUserLogin = serverError => {

        SecurityData.OfflineUserLogin(userCode, userPassword)
            .then(userName => {

                setIsLoading(false);

                if (userName === null) {
                    setError(serverError);
                } else if (userName === '') {
                    setLoginUserName('');
                    alert('Invalid User Name / Password');
                }
                else {
                    setLoginUserCode(userCode);
                    setLoginUserName(userName);
                    props.setIsLoggedIn(true);
                    alert('No connection to the server, you are working offline');
                }

            })
            .catch(err => {
                setIsLoading(false);
                setError(err);
            });

    };

    //While Loading Or Searching Data
    if (isLoading) {
        return (
            <LoadingData />
        );
    };

    //In case of error
    if (error) {
        return (
            <ServerError errorMessage={error.toString()} setError={ setError } />
        );
    };

    return (

        <ImageBackground style={styles.container} source={require('../assets/images/background.jpg')}  resizeMode="cover">

            <DialogInput isDialogVisible={serverIpDialogIsOpened}
                title={'Application Configuration'}
                message={'Please Enter Application Server IP'}
                hintInput={'___.___.___.___'}
                initValueTextInput={serverIP}
                submitInput={(newIP) => {
                    if (newIP === '') {
                        alert('No Changes To Save')
                    }
                    else {
                        changeServerIP(newIP)
                    }
                }}
                closeDialog={() => { setServerIpDialogIsOpened(false) }}
                >
            </DialogInput>

            <View style={styles.loginCard}>

                <View style={[styles.cardSection, {height: '20%'}]}>
                    <Image
                        source={require('../assets/images/logo.png')}
                        style={styles.brandLogo}
                        resizeMode='contain'
                    />
                </View>

                <View style={[styles.cardSection, {height: '25%'}]}>
                    <View style={styles.inputBox}>

                        <View style={styles.inputImageContainer}>
                            <Image
                                source={require('../assets/login/loginUser.png')}
                                style={styles.inputImage}
                                resizeMode='contain'
                            />
                        </View>

                        <TextInput
                            placeholder={'User Code'}
                            value={userCode}
                            onChangeText={setUserCode}
                            maxLength={4}
                            autoCapitalize={'none'}
                            style={styles.inputText} />

                    </View>
                </View>

                <View style={[styles.cardSection, {height: '25%'}]}>
                    <View style={styles.inputBox}>

                        <View style={styles.inputImageContainer}>
                            <Image
                                source={require('../assets/login/loginPassword.png')}
                                style={styles.inputImage}
                                resizeMode='contain'
                            />
                        </View> 
                        
                        <TextInput
                            placeholder={'Password'}
                            value={userPassword}
                            onChangeText={setUserPassword}
                            maxLength={10}
                            autoCapitalize={'none'}
                            secureTextEntry={true}
                            password={true}
                            style={styles.inputText} />

                    </View>
                </View>

                <View style={[styles.cardSection, { height: '7%', flexDirection: 'row', justifyContent: 'space-evenly' }]}>

                    <TouchableOpacity style={styles.registrationButton} onPress={
                        () => {
                            setServerIpDialogIsOpened(true);
                        }
                    }>
                        <Text style={styles.registrationButtonText}>Connect to Server</Text>
                    </TouchableOpacity>

                    <TouchableOpacity style={styles.registrationButton} onPress={
                        () => {
                            
                            SecurityData.RegisterDevice();
                            
                        }
                    }>
                        <Text style={styles.registrationButtonText}>Register Device</Text>
                    </TouchableOpacity>

                </View>

                <View style={[styles.cardSection, {height: '20%'}]}>
                    <View style={styles.loginButtonContainer}>
                        <TouchableOpacity style={styles.loginButton} onPress={() => {
                            // props.setIsLoggedIn(true);
                            // setLoginUserCode(userCode);
                            // setLoginUserName(userPassword);

                            if (serverIP == '' || serverIP == null) {
                                alert('Please define the server IP')
                            }
                            else if (userCode !== '' & userPassword !== '') {
                                UserLogin();
                            }
                            else {
                                alert('Please enter User Name and Password')
                            }

                            }}>
                            <Text style={styles.loginText}>Login</Text>
                        </TouchableOpacity>
                    </View>
                </View>

            </View>

        </ImageBackground>

    )
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        alignItems: 'center',
        justifyContent: 'center',
        paddingVertical: 0,
        backgroundColor: Colors.brandBlue,
    },
    loginCard: {
        width: Sizing.isTablet ? Sizing.windowsWidth/2 : Sizing.windowsWidth/1.2,
        height: Sizing.isTablet ? Sizing.windowsHeight/2.8 : Sizing.isPortableScanner ? Sizing.windowsHeight/1.5 : Sizing.windowsHeight/2.2,
        //height: '30%',
        backgroundColor: 'rgba(255,255,255,0.7)',
        alignItems: 'center',
        justifyContent: 'center',
        borderRadius: 10,
        borderWidth: 1,
        borderColor: 'rgba(255,255,255,0.5)'
        //elevation: 1,
        //overflow: 'hidden',
    },
    cardSection: {
        width: '100%',
        alignItems: 'center',
        justifyContent: 'center',
        //borderWidth: 2,
    },
    registrationButton: {
        //backgroundColor: 'red',
    },
    registrationButtonText: {
        fontSize: FontSize(17),
        color: Colors.brandBlue,
        fontStyle: 'italic',
        textDecorationLine: 'underline',
    },
    brandLogo: {
        //alignItems: 'center',
        //marginHorizontal: 10,
        resizeMode: 'contain',
        width: '80%',
        height: '70%',
        //backgroundColor:'red',
    },
    inputBox: {
        //backgroundColor:'red',
        width: '80%',
        height: '50%',
        alignItems: 'flex-start',
        justifyContent: 'center',
        flexDirection: 'row',
        borderBottomWidth: 2,
        borderBottomColor: 'rgba(255,255,255,0.8)'
    },
    inputImageContainer: {
        //backgroundColor: 'white',
        width: '15%',
        height: '100%',
        alignItems: 'center',
        justifyContent: 'center',
    },
    inputImage: {
        //alignItems: 'center',
        //marginHorizontal: 10,
        opacity: 0.8,
        resizeMode: 'contain',
        width: '70%',
        height: '70%',
        //backgroundColor:'red',
    },
    inputText: {
        //backgroundColor: 'white',
        color: 'white',
        width: '85%',
        height: '100%',
        borderColor: Colors.brandBlue,
        //borderWidth: 2,
        //borderRadius: 3,
        //paddingHorizontal: 10,
        fontSize: FontSize(20),
        color: Colors.brandBlue,
    },
    loginButtonContainer: {
        //backgroundColor: 'red',
        width: '100%',
        height: '100%',
        alignItems: 'center',
        justifyContent: 'center',
    },
    loginButton: {
        backgroundColor: Colors.brandBlue,
        //backgroundColor: 'white',
        width: '80%',
        height: '50%',
        borderRadius: 7,
        alignItems: 'center',
        justifyContent: 'center',
        elevation: 4
    },
    loginText: {
        color: 'white',
        fontSize: FontSize(22),
        fontWeight: 'bold'
    }
});

export default LoginScreen;