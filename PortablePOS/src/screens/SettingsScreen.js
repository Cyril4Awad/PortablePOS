import React, { useState, useEffect, useRef }  from 'react';
import Colors from '../variables/Colors';
import { View, Text, ScrollView, StyleSheet, Button } from 'react-native';
import DataField from '../components/controls/DataField';
import ScreenHeader from '../components/ScreenHeader';
import MainButton from '../components/controls/MainButton';
import SwitchButtonField from '../components/controls/SwitchButtonField';
import DialogInput from 'react-native-dialog-input';
import ServerInfo from '../configuration/ServerInfo';
import AsyncStorage from '@react-native-async-storage/async-storage';
import DeviceInfo from 'react-native-device-info';
import PosOperationsData from '../data/PosOperationsData';

import { getDBConnection, executeNonQuery, executeReader, executeWithinTransaction, TransactionObject } from '../database/Database';
import { getCashRegisterCode } from '../variables/CashierInfo';

const SettingsScreen = props => {

    const [serverIP, setServerIP] = useState('Loading...');
    const [locationPretty, setLocationPretty] = useState('Loading...')
    const [cashRegisterPretty, setCashRegisterPretty] = useState('Loading...')
    const [workingDay, setWorkingDay] = useState('Loading...')
    
    useEffect(() => { 
        
        getServerIP();

        PosOperationsData.GetLocationPretty({setLocationPretty})
        PosOperationsData.GetCashRegisterPretty({setCashRegisterPretty})
        PosOperationsData.GetWorkingDay({setWorkingDay})

    }, [])


    // CreateDatabase = async () => {

    //     let DB = await getDBConnection().catch(err => { console.log(err) });
    
    //     let str = [];

    //     str.push("CREATE table table_user11123311321(user_name VARCHAR(20) )");
        
        
    //     let results = await executeWithinTransaction(str, DB).catch(err => console.log('error', err));

    //   }

    const getServerIP = () => {
        ServerInfo.serverIP().then((serverIP) => {
            setServerIP(serverIP);
        })
    };

    return (
        <View style={styles.container}>
            
            <ScreenHeader 
                description='Adjust system settings'
                imageSource={require('../assets/icons/settings.png')}
            />

            <DataField title='Device Name' data={DeviceInfo.getDeviceNameSync()} />

            <DataField title='Server IP' data={serverIP}/>

            <DataField title='Location' data={locationPretty} />

            <DataField title='Working Day' data={workingDay} />

            <DataField title='Cash Register' data={cashRegisterPretty} />

            <DataField title='Language' data='English' />

            <DataField title='Working Mode' data='Online' />

            {/* <SwitchButtonField title='Working Mode' description='Switching between online and offline mode' data='ONLINE' option1='ONLINE' option2='OFFLINE' />

            <DataField title='Import Data from Data Source' data='To Local Storage' toolTip='Need Internet Connection' onPress={() => {alert('Data Imported')}}/>
            <DataField title='Export Data to Data Source' data='From Local Storage' toolTip='Need Internet Connection' onPress={() => {alert('Data Exported')}}/>
             */}

            {/* <Button title='create database' onPress={() => 
                TestRawSQlite()
                } /> */}

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.bgColor,
        height: '100%',
    },
});

export default SettingsScreen;
