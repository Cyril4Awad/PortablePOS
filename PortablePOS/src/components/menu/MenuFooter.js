import React, { useEffect, useState } from 'react';
import Colors from '../../variables/Colors';
import Sizing from '../../variables/Sizing';
import { getCurrentDate } from '../../variables/LoginInfo';
import { View, Text, Image, StyleSheet, TouchableOpacity } from 'react-native';
import { FontSize } from '../../variables/ResponsiveFont';
import GeoLocationInfo from '../../variables/GeoLocationInfo';
import { UseLocation } from '../../variables/AppConfig';
import { CheckServer } from '../../variables/ConnectionStatus';

// Online = the server of the login screen answers (not only the Wi-Fi being connected)
const statusCheckMs = 10000;

const MenuFooter = props => {

    const [isOnline, setIsOnline] = useState(null);

    useEffect(() => {

        let isMounted = true;

        const checkServer = () => {
            CheckServer().then(online => { if (isMounted) setIsOnline(online); });
        };

        checkServer();
        const timer = setInterval(checkServer, statusCheckMs);

        return () => {
            isMounted = false;
            clearInterval(timer);
        };

    }, []);

    return (
        <View style={styles.container}>

            <View style={styles.textContainer}>

                {UseLocation ? (
                <TouchableOpacity onPress={() => {GeoLocationInfo.GetLocation(true).then((loc) => {console.log('Refreshed')})}}>
                    <Text style={styles.text}>Refresh</Text>
                </TouchableOpacity>
                ) : <View />}

                {/* <Text style={styles.text}>Working : {getCurrentDate()}</Text> */}

                {isOnline !== null ? (
                <View style={styles.statusContainer}>
                    <View style={[styles.statusDot, { backgroundColor: isOnline ? Colors.bgColor : 'rgb(220, 53, 69)' }]} />
                    <Text style={styles.text}>{isOnline ? 'Online' : 'Offline'}</Text>
                </View>
                ) : null}

            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.brandGreen,
        height: 35,
        width : '100%',
    },
    textContainer: {
        height: '100%',
        width: '100%',
        //backgroundColor: 'red',
        alignItems: 'center',
        justifyContent: 'space-between',
        paddingLeft: 10,
        paddingRight: 10,
        flexDirection: 'row',
    },
    text: {
        fontSize: FontSize(20),
        color: Colors.bgColor,
        fontStyle: 'italic',
    },
    statusContainer: {
        flexDirection: 'row',
        alignItems: 'center',
    },
    statusDot: {
        width: 12,
        height: 12,
        borderRadius: 6,
        marginRight: 6,
        borderWidth: 1,
        borderColor: Colors.bgColor,
    },
});

export default MenuFooter;
