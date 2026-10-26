import React from 'react';
import { useState, useEffect, useRef } from 'react';

import Sizing from '../../../variables/Sizing';
import Colors from '../../../variables/Colors';
import StockData from '../../../data/StockData';
import SalesOrdersData from '../../../data/SalesOrdersData';
import { FontSize } from '../../../variables/ResponsiveFont';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity, Modal, ScrollView, Keyboard, FlatList } from 'react-native';
import ModalTitle from '../../ModalTitle';
import SalesOrderQtyLine from './SalesOrderQtyLine';
import LoadingData from '../../LoadingData';
import ServerError from '../../ServerError';
import MessageBox from '../../MessageBox';

const SalesOrderQty = props => {

    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState(null);

    const [itemImage, setItemImage] = useState('');
    const [ordersDetailedQty, setOrdersDetailedQty] = useState([]);

    const [scanner, setScanner] = useState('');
    const [expiryDate, setExpiryDate] = useState('');
    const [batchNumber, setBatchNumber] = useState('');
    const [qty, setQty] = useState('');
    const [totalQty, setTotalQty] = useState(0);

    const [manualEntryVisible, setManualEntryVisible] = useState(false);

    const scannerInputControl = useRef();
    const qtyInputControl = useRef();

    const expiryDateInputControl = useRef();
    const batchNumberInputControl = useRef();

    useEffect(() => {

        if (Sizing.isTablet) {
            StockData.GetItemImage(
                props.selectedItemRecID,
                { setItemImage },
            );
        };

        return(() => {
            //Cleanup
            setItemImage([]);
        })

    }, []);

    useEffect(() => {

        SalesOrdersData.GetSalesOrderDetailedQty(
            props.selectedHRecid.toString(),
            props.selectedCounter.toString(),
            { setOrdersDetailedQty },
            { setIsLoading },
            { setError }
        );

    }, []);

    useEffect(() => {

        if (ordersDetailedQty.length > 0) {

            var vatTotalQty = 0;

            for (i = 0; i < ordersDetailedQty.length; i++) {
                vatTotalQty += ordersDetailedQty[i].sopqQty;
            }

            setTotalQty(vatTotalQty);

        }
        else {
            setTotalQty(0);
        }

    }, [ordersDetailedQty]);

    //While Loading Or Searching Data
    //  if (isLoading) {
    //      return (
    //          <LoadingData />
    //      );
    //  };

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

                    <SalesOrderQtyLine
                        sopqRowNumber=''
                        sopqInfo={['Total Qty (out of ', props.selectedItemQtyNeeded, ')'].join('')}
                        sopqQty={totalQty}
                        isTotalSection={true}
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

    const ImageSection = () => {
        return (
            <View style={styles.itemImageContainer}>
                <Image
                    source={{ uri: "data:image/png;base64," + itemImage }}
                    style={styles.itemImage}
                    resizeMode='contain'
                />
            </View>
        )
    };

    const DetailsLine = dlData => {
        return (

            <SalesOrderQtyLine
                sopqRowNumber={dlData.item.sopqRowNumber}
                sopqInfo={dlData.item.sopqExpiryDate + ' - ' + dlData.item.sopqBatchNumber}
                sopqQty={dlData.item.sopqQty}
                deleteLine={() => {

                    MessageBox.alert('Delete Line', 'Do you want to delete the record ?', [
                        {
                            text: 'No',
                            onPress: () => { },
                            style: 'cancel',
                        },
                        {
                            text: 'Yes', onPress: () => {

                                SalesOrdersData.DeleteSalesOrderDetailedQty(
                                    dlData.item.sopqRecID,
                                    props.selectedHRecid,
                                    props.selectedCounter,
                                    { setOrdersDetailedQty },
                                    { setIsLoading },
                                    { setError });

                                scannerInputControl.current.focus();

                            }
                        },
                    ]);

                }}
            />

        );
    };

    return (
        <View style={styles.container}>

            <View style={styles.headerContainer}>
                <Text style={styles.itemReferenceText}>{props.selectedItemReference}</Text>
                <Text style={styles.itemDescriptionText} numberOfLines={2}>{props.selectedItemDescription}</Text>
            </View>

            <View style={styles.detailsContainer}>

                <View style={[styles.detailsContainerSplit, { width: Sizing.isTablet ? '50%' : '100%' }]}>

                    <View style={styles.detailsDataEntryContainer}>

                        <TouchableOpacity style={styles.manualEntryButtonContainer} onPress={() => {
                            setManualEntryVisible(true);
                            setExpiryDate('');
                            setBatchNumber('');
                            expiryDateInputControl.current.focus();
                        }}>
                            <Image
                                source={require('../../../assets/icons/keyboard.png')}
                                style={styles.manualEntryButtonImage}
                                resizeMode='contain'
                            />
                        </TouchableOpacity>

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
                            onSubmitEditing={() => {
                                qtyInputControl.current.focus()
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
                                    props.selectedCounter,
                                    props.selectedItemRecID,
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

                    <View style={[styles.detailsDataEntryManualContainer, {
                        height: manualEntryVisible ? 50 : 0 , 
                        width : manualEntryVisible ? '100%' : 0, 
                        padding: manualEntryVisible ? 5 : 0,
                        position: 'absolute'}]}>

                        <TextInput
                            ref={expiryDateInputControl}
                            style={styles.detailsDataEntryExpiryDate}
                            value={expiryDate}
                            onChangeText={setExpiryDate}
                            placeholder='Expiry Date'
                            keyboardType='numeric'
                            maxLength={10}
                            showSoftInputOnFocus={!Sizing.isPortableScanner}
                            onPressIn={() => {
                                if (Sizing.isPortableScanner) {
                                    Keyboard.dismiss();
                                }
                            }}
                        />

                        <TextInput
                            ref={batchNumberInputControl}
                            style={styles.detailsDataEntryBatchNumber}
                            value={batchNumber}
                            onChangeText={setBatchNumber}
                            placeholder='Batch Number'
                            maxLength={15}
                            showSoftInputOnFocus={!Sizing.isPortableScanner}
                            onPressIn={() => {
                                if (Sizing.isPortableScanner) {
                                    Keyboard.dismiss();
                                }
                            }}
                        />

                        <TouchableOpacity style={styles.detailsDataEntryAddButton} disabled={isLoading} onPress={() => {

                            var expiryString;

                            if (expiryDate.length == 7) {
                                expiryString = '00.' + expiryDate;
                            }
                            else {
                                expiryString = expiryDate;
                            };
                            
                            if (expiryString === '') {
                                
                                alert('Please fill the expiry date');
                                expiryDateInputControl.current.focus();
                            }
                            else if (expiryString.length !== 10) {
                                alert('Invalid Expiry Date');
                                expiryDateInputControl.current.focus();
                            }
                            else if (batchNumber === '') {
                                alert('Please fill the batch number');
                                batchNumberInputControl.current.focus();
                            }
                            else {

                                setScanner(props.selectedItemRecID.toString() + '|' + expiryString.replaceAll('#','/').replaceAll('*','/').replaceAll('.','/') + '|' + batchNumber);
                                setManualEntryVisible(false);
                                qtyInputControl.current.focus()

                            };

                        }}>
                            <Image
                                source={require('../../../assets/icons/ok.png')}
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
                    <ImageSection />
                </View>

            </View>

            <View style={styles.footerContainer}>

                <View style={styles.noteContainer}>
                    <Text style={styles.noteText}>{totalQty > props.selectedItemQtyNeeded ? 'Qty Prepared is greater than qty needed !' : ''}</Text>
                </View>

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
        backgroundColor: 'white',
        width: '100%',
        height: '100%',
        alignItems: 'center',
        justifyContent: 'space-between',
    },
    headerContainer: {
        backgroundColor: 'gainsboro',
        height: 80,
        width: '100%',
        alignItems: 'flex-start',
        justifyContent: 'center',
        paddingLeft: 20,
        paddingRight: 20,
    },
    itemReferenceText: {
        color: 'black',
        width: '100%',
        //fontWeight: 'bold',
        fontSize: FontSize(18),
    },
    itemDescriptionText: {
        color: Colors.brandBlue,
        fontWeight: 'bold',
        width: '100%',
        fontSize: FontSize(24),
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
        backgroundColor: Colors.brandBlue,
        width: '100%',
        height: 50,
        padding: 5,
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-evenly'
    },
    detailsDataEntryManualContainer: {
        backgroundColor: Colors.brandGreen,
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-evenly'
    },
    manualEntryButtonContainer: {
        backgroundColor: 'gainsboro',
        width: 20,
        height: '100%',
        alignItems: 'center',
        justifyContent: 'center',
        borderRadius: 4,
    },
    manualEntryButtonImage: {
        //alignItems: 'center',
        //marginHorizontal: 10,
        resizeMode: 'contain',
        width: '90%',
        height: '90%',
        //backgroundColor:'red',
    },
    detailsDataEntryScanner: {
        backgroundColor: 'white',
        color: 'black',
        fontSize: FontSize(15),
        borderWidth: 1,
        borderColor: 'gainsboro',
        width: '40%',
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
        width: '30%',
        height: '100%',
        borderRadius: 4,
        alignItems: 'center',
        justifyContent: 'center',
        textAlign: 'center',
        textAlignVertical: 'center',
    },
    detailsDataEntryExpiryDate: {
        backgroundColor: 'white',
        color: 'black',
        fontSize: FontSize(15),
        borderWidth: 1,
        borderColor: 'gainsboro',
        width: '40%',
        height: '100%',
        borderRadius: 4,
        alignItems: 'center',
        justifyContent: 'center',
        textAlign: 'left',
        textAlignVertical: 'center',
    },
    detailsDataEntryBatchNumber: {
        backgroundColor: 'white',
        color: 'black',
        fontSize: FontSize(15),
        borderWidth: 1,
        borderColor: 'gainsboro',
        width: '40%',
        height: '100%',
        borderRadius: 4,
        alignItems: 'center',
        justifyContent: 'center',
        textAlign: 'left',
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
    itemImageContainer: {
        //backgroundColor: 'red',
        width: '100%',
        height: '100%',
        //padding: 0,
    },
    itemImage: {
        width: '100%',
        height: '100%',
        resizeMode: 'contain',
        //borderRadius: 20,
        overflow: 'hidden',
    },
    footerContainer: {
        backgroundColor: 'gainsboro',
        height: 60,
        width: '100%',
        alignItems: 'center',
        justifyContent: 'space-around',
        flexDirection: 'row',
        paddingRight: 10
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

export default SalesOrderQty;
