import React, { useState , useEffect } from 'react';
import Sizing from '../../../variables/Sizing';
import Colors from '../../../variables/Colors';
import { View, StyleSheet, Image, Text, FlatList } from 'react-native';
import WebView from 'react-native-webview';
import { FontSize } from '../../../variables/ResponsiveFont';
import ModalTitle from '../../ModalTitle';
import ClientsData from '../../../data/ClientsData';

const ClientLastPurchases = props => {

    const [clientLastPurchasesList, setClientLastPurchasesList] = useState([]);

    useEffect(() => {

        ClientsData.GetClientLastPurchases(props.clientRecID,
            { setClientLastPurchasesList })      

            return(() => {
                //Cleanup
                setClientLastPurchasesList([]);
            })

        }
      , []);

    const LastPurchaseRecord = lprData => {
        return (
            <View style={styles.lastPurchaseRecordContainer}>

                <View style={[styles.lprInvoiceDateNumber, {height: lprData.item.invoiceNumber === '' ? 0 : 25}]}>
                    <Text style={styles.lprInvoiceDateNumberText}>{lprData.item.invoiceDate}</Text>
                    <Text style={styles.lprInvoiceDateNumberText}>{lprData.item.invoiceNumber}</Text>
                </View>

                <View style={styles.lprItemDescription}>
                    <Text style={styles.lprItemDescriptionText}>{lprData.item.itemReference} - {lprData.item.itemDescription}</Text>
                </View>

                <View style={[styles.lprItemDetails, {height: lprData.item.itemIsGift === 'Y' ? 0 : 30}]}>

                    <Text style={styles.lprItemDetailsText}>(</Text>
                    <Text style={styles.lprItemDetailsText}>{Intl.NumberFormat("ar-de").format(lprData.item.itemQty)}</Text>
                    <Text style={styles.lprItemDetailsText}>x</Text>
                    <Text style={styles.lprItemDetailsTextImportant}>{Intl.NumberFormat("ar-de").format(lprData.item.itemUnitPrice)} {lprData.item.invoiceCurrency}</Text>
                    <Text style={styles.lprItemDetailsText}>) -</Text>
                    <Text style={styles.lprItemDetailsText}>{Intl.NumberFormat("ar-de").format(lprData.item.itemDiscount)} % =</Text>
                    <Text style={styles.lprItemDetailsTextImportant}>{Intl.NumberFormat("ar-de").format(lprData.item.itemNetTotal)} {lprData.item.invoiceCurrency}</Text>
                    <Text style={styles.lprItemDetailsText}>{lprData.item.itemVatType}</Text>

                </View>

                <View style={[styles.lprItemGift, {height: lprData.item.itemIsGift === 'Y' ? 30 : 0}]}>

                    <Text style={styles.lprItemGiftText}>GIFT</Text>

                </View>

                <View>

                </View>

            </View>
        );
      };

    const DataView = () => {
        if (clientLastPurchasesList.length !== 0) {

            return (
                <FlatList
                    keyExtractor={(item, index) => item.recKey.toString()}
                    data={clientLastPurchasesList}
                    renderItem={LastPurchaseRecord}
                    numColumns={1}
                    style={{ width: '100%', backgroundColor: Colors.bgColor }}
                    contentContainerStyle={{ alignItems: 'flex-start', justifyContent: 'center' }}
                />
            )

        }
        else {

            return (
                <View style={styles.emptyDataContainer}>
                    <Image
                        source={require('../../../assets/images/noDataFound.png')}
                        style={styles.emptyDataImage}
                        resizeMode="contain"
                    />
                    <Text style={styles.emptyDataText}>No Data Found !</Text>
                </View>
            )

        }
    };  

    return (
        <View style={styles.container}>

            <ModalTitle
                title={props.clientName}
                onCloseButtonPress={() => { props.setClientLastPurchasesModalVisible(false) }}
                clearData={() => {
                    // setClientsData([]);
                    // setClientsDataFiltered([]);
                }}
            />
            
            <View style={styles.titleContainer}>
                <Text style={styles.titleText}>Last Purchases</Text>
            </View>

            <DataView />

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        width: '100%',
        height: '100%',
        alignItems: 'center',
        justifyContent: 'center',
    },
    titleContainer: {
        backgroundColor: Colors.brandGreen,
        height: 28,
        alignItems: 'center',
        justifyContent: 'center',
        width: '100%',
    },
    titleText: {
        color: Colors.brandBlue,
        fontSize: FontSize(20)
    },
    lastPurchaseRecordContainer: {
        backgroundColor: Colors.bgColor,
        width: (Sizing.isPortableScanner ? Sizing.windowsWidth * 0.9 : Sizing.windowsWidth * 0.9) - 11 ,
        //borderRadius: 10,
        alignItems: 'flex-start',
        justifyContent: 'center',
        //padding: 5,
        //marginBottom: 5,
        //padding: 2,
    },
    lprInvoiceDateNumber: {
        backgroundColor: 'gainsboro',
        width: '100%',
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-around',
    },
    lprInvoiceDateNumberText: {
        color: 'dimgray',
        fontSize: FontSize(17),
        //fontWeight: 'bold',
    },
    lprItemDescription: {
        //backgroundColor: 'blue',
        width: '100%',
        //height: 20,
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-around',
        paddingLeft: 5,
        paddingRight: 5,
    },
    lprItemDescriptionText: {
        color: 'black',
        fontSize: FontSize(17),
        //fontWeight: 'bold',
    },
    lprItemDetails: {
        //backgroundColor: 'gainsboro',
        width: '100%',
        //height: 0,
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-around',
    },
    lprItemDetailsText: {
        color: Colors.brandBlue,
        fontSize: FontSize(17),
    },
    lprItemDetailsTextImportant: {
        color: Colors.brandBlue,
        fontSize: FontSize(17),
        fontWeight: 'bold'
    },
    lprItemGift: {
        width: '100%',
        //height: 0,
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
        borderBottomWidth: 2,
        borderBottomColor: 'gainsboro',
    },
    lprItemGiftText: {
        color: Colors.brandGreen,
        fontSize: FontSize(17),
        fontWeight: 'bold',
        fontStyle: 'italic'
    },
    emptyDataContainer: {
        //backgroundColor: 'red',
        flex: 1,
        width: '100%',
        alignItems: 'center',
        justifyContent: 'center',
    },
    emptyDataImage: {
        width: '75%',
        height: '75%',
        overflow: 'hidden',
        resizeMode: 'contain',
    },
    emptyDataText: {
        fontSize: FontSize(20),
        color: Colors.brandBlue
    }
});

export default ClientLastPurchases;
