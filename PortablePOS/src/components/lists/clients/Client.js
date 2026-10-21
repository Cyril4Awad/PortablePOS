import React from 'react';
import { useState } from 'react';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity, Modal } from 'react-native';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';
import { UseLocation } from '../../../variables/AppConfig';

const Client = props => {

    const [itemOpened, setItemOpened] = useState(false);

    const [checkMark, setCheckMark] = props.clientVisited === 'Y' ? '\u2713' : '';

    return ( 

        <View style={[styles.container, {height: itemOpened ? 75 : 50 }]} >

            <View style={styles.buttonContainer}>

                <TouchableOpacity style={styles.button} onPress={props.onPress} onLongPress={props.onLongPress}>

                    <View style={styles.clientCodeNameGeoContainer}>

                        <View style={styles.clientCodeContainer}>
                            <Text style={styles.clientCodeText}>{checkMark} {props.clientCode}</Text>
                        </View>

                        <View style={styles.clientNameContainer}>
                            <Text style={styles.clientNameText}>{props.clientName}</Text>
                        </View>

                        {UseLocation ? (
                        <TouchableOpacity style={styles.clientGeoContainer} onPress={props.onGeoLocationPress}>
                            <Image
                                source={props.clientGeoStatus !== -1 ? props.clientGeoStatus === 0 ? require('../../../assets/icons/geoLocationBW.png') : require('../../../assets/icons/geoLocation.png') : require('../../../assets/icons/geoLocationEmpty.png')}
                                style={styles.geoImage}
                                resizeMode="contain"
                            />
                        </TouchableOpacity>
                        ) : null}

                    </View>

                    <View style={[styles.clientDetailsContainer, { height: itemOpened ? 25 : 0 }]}>

                        <View style={[styles.detailedSection, { height: itemOpened ? 25 : 0 }]}>
                            <Text style={[styles.detailedText, { fontWeight: 'bold' }]}>Account Executive : </Text>
                            <Text style={styles.detailedText}>{props.clientAccountExecutive}</Text>
                        </View>

                    </View>

                </TouchableOpacity>

            </View>

        </View>

    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.bgColor,
        width: '100%',
        //borderWidth: 1,
        //borderRadius: 10,
        alignItems: 'flex-start',
        justifyContent: 'center',
        //padding: 5,
        //marginBottom: 5,
        paddingBottom: 2,
    },
    buttonContainer: {
        backgroundColor : Colors.brandGreen,
        alignItems: 'flex-start',
        justifyContent: 'center',
        width: '100%',
        height: '100%',
        overflow: 'hidden',
        //borderWidth: 1,
        borderColor: 'silver',
        //elevation: 10,
    },
    button: {
        backgroundColor : Colors.listItemBackColor,
        width: '100%',
        height: '100%',
        //backgroundColor: 'red',
        alignItems: 'flex-start',
        justifyContent: 'center',
        paddingLeft: 8,
        paddingRight: 5,
    },
    clientCodeNameGeoContainer: {
        flexDirection: 'row',
        //backgroundColor: 'red',
        height: 50,
    },
    clientCodeContainer: {
        //backgroundColor: 'blue',
        width: 70,
        height: '100%',
        alignItems: 'flex-start',
        justifyContent: 'center',
    },
    clientCodeText: {
        color: 'black',
        fontSize: FontSize(14),
    },
    clientNameContainer: {
        //backgroundColor: 'yellow',
        flex: 1,
        height: '100%',
        alignItems: 'flex-start',
        justifyContent: 'center',
    },
    clientNameText: {
        fontWeight: 'bold',
        color: Colors.brandBlue,
        fontSize: FontSize(16),
    },
    clientGeoContainer :{
        //backgroundColor: 'red',
        width: 50,
        height: '100%',
        alignItems: 'center',
        justifyContent: 'center',
    },
    geoImage: {
        width: '75%',
        height: '75%',
        overflow: 'hidden',
        resizeMode: 'contain',
    },
    clientDetailsContainer: {
        width: '100%',
        //backgroundColor: 'yellow',
    },
    detailedSection: {
        width: '100%',
        //backgroundColor: 'blue',
        flexDirection: 'row',
    },
    detailedText: {
        fontSize : FontSize(15),
        color: 'gray',
    }
});

export default Client;
