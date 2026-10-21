import React from 'react';
import Sizing from '../variables/Sizing';
import Colors from '../variables/Colors';
import { View, StyleSheet, Image, Text } from 'react-native';
import WebView from 'react-native-webview';
import { FontSize } from '../variables/ResponsiveFont';
import ModalTitle from '../components/ModalTitle';

const MapView = props => {

    return (
        <View style={styles.container}>

            <ModalTitle
                title={'Clients Name - ' + props.clientName}
                onCloseButtonPress={() => {props.setMapVisible(false)}}
                clearData={() => {
                    // setClientsData([]);
                    // setClientsDataFiltered([]);
                }}
            />

            <View style={styles.mapContainer}>
                <WebView
                    source={{ uri: `https://maps.google.com/?q=${props.latitude},${props.longitude}` }}
                />
            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        //flex: 1, 
        width: '100%',
        height: '100%',
        //backgroundColor: 'red',
    },
    mapContainer: {
        backgroundColor: 'red',
        width: '100%',
        flex: 1
    }
});

export default MapView;
