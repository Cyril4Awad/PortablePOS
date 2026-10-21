import React from 'react';
import { useState, useEffect, useRef } from 'react';

import Sizing from '../../../variables/Sizing';
import Colors from '../../../variables/Colors';
import StockData from '../../../data/StockData';
import SalesOrdersData from '../../../data/SalesOrdersData';
import { FontSize } from '../../../variables/ResponsiveFont';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity, Modal, ScrollView, Keyboard, FlatList, Alert } from 'react-native';
import ModalTitle from '../../ModalTitle';
import SalesOrderQtyLine from './SalesOrderQtyLine';
import LoadingData from '../../LoadingData';
import ServerError from '../../ServerError';
import SwitchButtonField from '../../controls/SwitchButtonField';

const SalesOrderScanner = props => {

    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState(null);

    const [skipLine, setSkipLine] = useState(true);

    const [ordersDetailedQty, setOrdersDetailedQty] = useState([]);

    const [scanner, setScanner] = useState('');
    const [qty, setQty] = useState('1');

    const scannerInputControl = useRef();
    const qtyInputControl = useRef();

    const expiryDateInputControl = useRef();
    const batchNumberInputControl = useRef();


    useEffect(() => {

        SalesOrdersData.GetSalesOrderDetailedQty(
            props.selectedHRecid.toString(),
            '',
            { setOrdersDetailedQty },
            { setIsLoading },
            { setError }
        );

        return(() => {
            //Cleanup
            setOrdersDetailedQty([]);
        })

    }, []);

    const DetailsView = () => {
        if (isLoading) {
            return (
                <View style={styles.detailsDataEntryList}>
                    <LoadingData />
                </View>
            )
        }
        else {
            return (
                <View style={styles.detailsDataEntryList}>

                    <FlatList
                        data={ordersDetailedQty}
                        //contentContainerStyle={{ justifyContent: 'center' }}
                        keyExtractor={(item) => item.sopqRowNumber.toString()}
                        renderItem={DetailsLine}
                    />

                </View>
            )
        }
    };

    //In case of error
    if (error) {
        return (
            <ServerError errorMessage={error.toString()} setError={ setError } />
        );
    };

    const DetailsLine = dlData => {
        return (

            <SalesOrderQtyLine
                showItemDescription={true}
                sopqItemDescription={dlData.item.sopqItemDescription}
                sopqRowNumber={dlData.item.sopqRowNumber}
                sopqInfo={dlData.item.sopqExpiryDate + ' - ' + dlData.item.sopqBatchNumber}
                sopqQty={dlData.item.sopqQty}
                deleteLine={() => {}}
            />

        );
    };

    return (
        <View style={styles.container}>

            <View style={styles.optionsContainer}>

                <Image
                            source={require('../../../assets/icons/qr.png')}
                            style={styles.qrImage}
                            resizeMode='contain'
                        />

                <TouchableOpacity style={styles.skipLineButtonContainer} onPress={() => {
                    setSkipLine(!skipLine);
                    setQty('1');

                    }}>

                    <View style={styles.skipLineButtonTextContainer}>
                        <Text style={styles.skipLineButtonText}>Skip Line</Text>
                    </View>

                    <View style={styles.skipLineButtonImageContainer}>
                        <Image
                            source={skipLine ? require('../../../assets/icons/checked.png') : require('../../../assets/icons/unchecked.png')}
                            style={styles.skipLineButtonImage}
                            resizeMode='contain'
                        />
                    </View>

                </TouchableOpacity>

            </View>

            <View style={styles.detailsContainer}>

                <View style={[styles.detailsContainerSplit, { width: Sizing.isTablet ? '50%' : '100%' }]}>

                    <View style={styles.detailsDataEntryContainer}>

                        <TextInput
                            ref={scannerInputControl}
                            style={styles.detailsDataEntryScanner}
                            value={scanner}
                            onChangeText={setScanner}
                            showSoftInputOnFocus={false}
                            onPressIn={() => Keyboard.dismiss()}
                            placeholder='Scan Item'
                            maxLength={100}
                            //editable={!isLoading}
                            secureTextEntry={true}
                            autoFocus={true}
                            blurOnSubmit={!skipLine}
                            onSubmitEditing={() => {

                                if (skipLine) {
                                    
                                    SalesOrdersData.SaveSalesOrderDetailedQty(
                                        props.selectedHRecid,
                                        '',
                                        '',
                                        scanner,
                                        { setScanner },
                                        qty,
                                        { setQty },
                                        '1',
                                        { setOrdersDetailedQty },
                                        { setIsLoading },
                                        { setError });

                                }
                                else {
                                    qtyInputControl.current.focus()
                                }

                            }}

                        />

                        <TextInput
                            ref={qtyInputControl}
                            style={styles.detailsDataEntryQty}
                            value={qty}
                            onChangeText={setQty}
                            keyboardType='numeric'
                            placeholder='Qty'
                            maxLength={15}
                            editable={!skipLine}
                            //editable={!isLoading}
                            showSoftInputOnFocus={!Sizing.isPortableScanner}
                            onPressIn={() => {
                                if (Sizing.isPortableScanner) {
                                    Keyboard.dismiss();
                                }
                            }}

                        />

                        <TouchableOpacity style={styles.detailsDataEntryAddButton} disabled={isLoading} onPress={() => {

                            if (scanner == '') {
                                alert('Please scan the item');
                                scannerInputControl.current.focus();
                            }
                            else if (qty == '') {
                                alert('Please fill the qty');
                                qtyInputControl.current.focus();
                            }
                            else {

                                SalesOrdersData.SaveSalesOrderDetailedQty(
                                    props.selectedHRecid,
                                    '',
                                    '',
                                    scanner,
                                    { setScanner },
                                    qty,
                                    { setQty },
                                    '',
                                    { setOrdersDetailedQty },
                                    { setIsLoading },
                                    { setError });

                                scannerInputControl.current.focus();

                            }

                        }}>
                            <Image
                                source={require('../../../assets/icons/add.png')}
                                style={styles.detailsDataEntryButtonImage}
                                resizeMode='contain'
                            />
                        </TouchableOpacity>

                    </View>

                    <DetailsView />

                </View>

                <View style={[styles.detailsContainerSplit, {
                    width: Sizing.isTablet ? '50%' : 0,
                    borderLeftWidth: Sizing.isTablet ? 4 : 0,
                    borderLeftColor: 'gainsboro',
                }]}>
                    
                </View>

            </View>

            <View style={styles.footerContainer}>

                {/* <View style={styles.noteContainer}>
                    <Text style={styles.noteText}>{totalQty > props.selectedItemQtyNeeded ? 'Qty Prepared is greater than qty needed !' : ''}</Text>
                </View> */}

                <TouchableOpacity style={styles.buttonOk} disabled={isLoading} onPress={() => {
                    setOrdersDetailedQty([]);
                    props.setModalVisible(false);
                    props.refreshSalesOrderItems();
                    //props.onSubmit();
                }}>
                    <Text style={styles.buttonOkText}>OK</Text>
                </TouchableOpacity>

            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.brandGreen,
        width: '100%',
        height: '100%',
        alignItems: 'center',
        justifyContent: 'space-between',
    },
    // headerContainer: {
    //     backgroundColor: 'white',
    //     height: 30,
    //     width: '100%',
    //     alignItems: 'flex-start',
    //     justifyContent: 'center',
    //     paddingLeft: 20,
    //     paddingRight: 20,
    // },
    // titleText: {
    //     color: Colors.brandBlue,
    //     fontWeight: 'bold',
    //     width: '100%',
    //     fontSize: FontSize(24),
    // },
    optionsContainer: {
        //backgroundColor: Colors.brandBlue,
        height: 40,
        paddingTop: 5,
        paddingLeft: 7,
        paddingRight: 10,
        paddingBottom: 3,
        width: '100%',
        alignItems: 'flex-end',
        justifyContent: 'space-between',
        flexDirection: 'row',
    },
    qrImage: {
        //backgroundColor: 'red',
        //alignItems: 'center',
        //marginHorizontal: 10,
        resizeMode: 'contain',
        width: 30,
        height: 30,
        //backgroundColor:'red',
    },
    skipLineButtonContainer: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
    },
    skipLineButtonTextContainer: {
        //backgroundColor: 'darkblue',
        //width: 100,
        height: 30,
        alignItems: 'flex-end',
        justifyContent: 'center',
        paddingLeft: 3,
        paddingRight: 6,
    },
    skipLineButtonText: {
        fontSize: FontSize(18),
        color: 'white',
    },
    skipLineButtonImageContainer: {
        backgroundColor: 'white',
        width: 25,
        height: 25,
        borderRadius: 25,
        alignItems: 'center',
        justifyContent: 'center',
    },
    skipLineButtonImage: {
        //alignItems: 'center',
        //marginHorizontal: 10,
        resizeMode: 'contain',
        width: '100%',
        height: '100%',
        //backgroundColor:'red',
    },
    detailsContainer: {
        width: '100%',
        flex: 1,
        backgroundColor: 'white',
        alignItems: 'flex-start',
        justifyContent: 'center',
        flexDirection: 'row',
    },
    detailsContainerSplit: {
        //backgroundColor: 'yellow',
        height: '100%',
    },
    detailsDataEntryContainer: {
        backgroundColor: Colors.brandGreen,
        width: '100%',
        height: 50,
        padding: 5,
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between'
    },
    detailsDataEntryScanner: {
        backgroundColor: 'white',
        color: 'black',
        fontSize: FontSize(15),
        borderWidth: 1,
        borderColor: 'gainsboro',
        width: '45%',
        height: '100%',
        borderRadius: 4,
        alignItems: 'center',
        justifyContent: 'center',
        textAlign: 'left',
        textAlignVertical: 'center',
    },
    detailsDataEntryQty: {
        backgroundColor: 'white',
        color: 'black',
        fontSize: FontSize(15),
        borderWidth: 1,
        borderColor: 'gainsboro',
        width: '35%',
        height: '100%',
        borderRadius: 4,
        alignItems: 'center',
        justifyContent: 'center',
        textAlign: 'center',
        textAlignVertical: 'center',
    },
    detailsDataEntryAddButton: {
        backgroundColor: 'gainsboro',
        //flexDirection: 'row',
        width: 50,
        height: '100%',

        borderRadius: 5,
        alignItems: 'center',
        justifyContent: 'center',

    },
    detailsDataEntryButtonImage: {
        //alignItems: 'center',
        //marginHorizontal: 10,
        resizeMode: 'contain',
        width: '90%',
        height: '90%',
        //backgroundColor:'red',
    },
    detailsDataEntryList: {
        //backgroundColor: 'yellow',
        width: '100%',
        height: '80%',
        flex: 1,
    },
    footerContainer: {
        backgroundColor: 'gainsboro',
        height: 60,
        width: '100%',
        alignItems: 'center',
        justifyContent: 'space-around',
        flexDirection: 'row',
        paddingRight: 10,
        borderTopWidth: 3,
        borderTopColor: 'darkgray',
    },
    noteContainer: {
        //backgroundColor: 'yellow',
        flex: 1,
        width : '70%',
        height: 40,
        paddingRight: 7,
        paddingLeft: 7,
        alignItems: 'center',
        justifyContent: 'center',
    },
    noteText: {
        fontSize: FontSize(15),
        color: 'red',
    },
    buttonOk: {
        backgroundColor: Colors.brandBlue,
        width: 100,
        height: 40,
        alignItems: 'center',
        justifyContent: 'center',
        borderRadius: 15
    },
    buttonOkText: {
        fontSize: FontSize(15),
        color: 'white',
    }
});

export default SalesOrderScanner;
