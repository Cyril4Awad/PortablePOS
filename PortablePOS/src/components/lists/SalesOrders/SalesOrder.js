import React from 'react';
import { useState } from 'react';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity, Modal, Keyboard } from 'react-native';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';
import SalesOrderItems from './SalesOrderItems';
import SalesOrdersData from '../../../data/SalesOrdersData';

const SalesOrder = props => {

    const [modalVisible, setModalVisible] = useState(false);
    const [selectedSoLocation, setSelectedSoLocation] = useState('');
    const [selectedSoRecID, setSelectedSoRecID] = useState('');
    const [selectedSoOfferReference, setSelectedSoOfferReference] = useState('');
    const [selectedSoClientCode, setSelectedSoClientCode] = useState('');
    const [selectedSoClientNumber, setSelectedSoClientNumber] = useState('');
    const [selectedSoClientName, setSelectedSoClientName] = useState('');

    return (

        <View style={[styles.container, {backgroundColor: (props.soRowNumber % 2 !== 0) ? 'white' : 'gainsboro'}]} >

            <Modal visible={modalVisible}>
                <SalesOrderItems 
                setModalVisible={setModalVisible} 
                selectedSoLocation={selectedSoLocation} 
                selectedSoRecID={selectedSoRecID} 
                selectedSoOfferReference={selectedSoOfferReference}
                selectedSoClientCode={selectedSoClientCode}
                selectedSoClientNumber={selectedSoClientNumber}
                selectedSoClientName={selectedSoClientName}
                />
            </Modal>

            <View style={styles.dataContainer}>

                {/* <Text>{props.soKey}</Text>
            <Text>{props.soLocation}</Text>
            <Text>{props.soRecID}</Text> */}

                <View style={styles.dataNumberDateContainer}>

                    <Text style={styles.dataNumberText}>{props.soOfferReference}</Text>
                    <Text style={styles.dataDateText}>{props.soDate}</Text>

                </View>

                <View style={styles.dataClientContainer}>

                    <Text numberOfLines={1} style={styles.dataClientText}>{props.soClientCode} / {props.soClientNumber} - {props.soClientName}</Text>

                </View>

                {/* <Text>{props.soGeneralTotal} {props.soCurrency}</Text> */}

                <View style={[styles.dataSubjectRemarksContainer, {height: props.soSubjectRemarks !== '' ? 50 : 0}]}>
                    <Text numberOfLines={2} style={styles.dataSubjectRemarksText}>{props.soSubjectRemarks}</Text>
                </View>

                <View style={styles.dataStatusContainer}>
                    <Text style={styles.dataStatusText}>{props.soStatus}</Text>
                </View>

            </View>

            <View style={styles.buttonsContainer}>

                <TouchableOpacity style={styles.buttonContainer}
                                  onPress={() => {

                                    //added to handle a bug in Speedata devices when trying to scan a barcode whithout focusing on an input control
                                    if (modalVisible) { 
                                        setModalVisible(false);
                                    };

                                    setSelectedSoLocation(props.soLocation);
                                    setSelectedSoRecID(props.soRecID);
                                    setSelectedSoOfferReference(props.soOfferReference);
                                    setSelectedSoClientCode(props.soClientCode);
                                    setSelectedSoClientNumber(props.soClientNumber);
                                    setSelectedSoClientName(props.soClientName);
                                    
                                    SalesOrdersData.OpenSalesOrder(props.soRecID,
                                                                { setModalVisible }
                                                                )
                                  }}  >

                    <View style={styles.buttonImageContainer}>
                        <Image
                            source={require('../../../assets/icons/prepare.png')}
                            style={styles.buttonImage}
                            resizeMode="contain"
                        />
                    </View>

                    <View style={styles.buttonTextContainer}>
                        <Text style={styles.buttonText}>Prepare</Text>
                    </View>

                </TouchableOpacity>

            </View>

        </View>

    );
};

const styles = StyleSheet.create({
    container: {
        //backgroundColor: 'red',
        width: '100%',
        alignItems: 'flex-start',
        //justifyContent: 'space-evenly',
        //marginBottom: 10,
        borderColor: Colors.brandBlue,
        borderWidth: 1.2,
        borderColor: 'silver',
        //elevation: 10,
        flexDirection: Sizing.isTablet ? 'row' : 'column',
    },
    dataContainer: {
        flex: 1,
        //backgroundColor: 'white',
        width: Sizing.isTablet ? '90%' : '100%',
        //borderWidth: 1,
    },
    dataNumberDateContainer: {
        //backgroundColor : 'blue',
        alignItems: 'center',
        justifyContent: 'space-between',
        width: '100%',
        height: 35,
        flexDirection: 'row',
        padding: 4,
    },
    dataNumberText: {
        fontSize: FontSize(21),
        fontWeight: 'bold',
        color: 'navy',
        //elevation: 100,
        //transform: [{rotate: '45deg'}]
    },
    dataDateText: {
        fontSize: FontSize(19),
        fontStyle: 'italic',
        color: 'navy',
    },
    dataClientContainer: {
        //backgroundColor: 'white',
        alignItems: 'center',
        justifyContent: 'center',
        width: '100%',
        height: 35,
        padding: 5,
    },
    dataClientText: {
        fontSize: FontSize(23),
        color: 'navy',
    },
    dataSubjectRemarksContainer: {
        //backgroundColor: 'red',
        width: '100%',
        alignItems: 'flex-start',
        justifyContent: 'center',
        paddingLeft: 11,
        paddingRight: 11,
    },
    dataSubjectRemarksText: {
        fontSize: FontSize(17),
        color: 'royalblue',
    },
    dataStatusContainer: {
        //backgroundColor: 'red',
        width: '100%',
        height: 30,
        alignItems: 'flex-start',
        justifyContent: 'center',
        paddingLeft: 11,
    },
    dataStatusText: {
        fontSize: FontSize(17),
        color: 'dimgray',
        fontStyle: 'italic',
    },
    buttonsContainer: {
        alignItems: Sizing.isTablet ? 'center' : 'flex-end',
        //justifyContent: Sizing.isTablet ? 'flex-end' : 'flex-end',
        //backgroundColor: 'white',
        height: Sizing.isTablet ? '100%' : 50,
        width: Sizing.isTablet ? 150 : '100%',
        flexDirection: Sizing.isTablet ? 'column-reverse' : 'column',
        padding: 7,
    },
    buttonContainer: {
        //flex: Sizing.isTablet ? 1 : 0,
        //flex: 2,
        alignItems: 'center',
        justifyContent: 'flex-end',
        backgroundColor: 'gainsboro',
        width: Sizing.isTablet ? '100%' : 120,
        height: Sizing.isTablet ? 50 : '100%',
        borderRadius: 5,
        borderWidth: 1,
        elevation: 10,
        borderColor: 'rgb(8, 98, 170)',
        overflow: 'hidden',
        marginLeft: Sizing.isTablet ? 0 : 7,
        marginBottom: 0,
        flexDirection: 'row',
    },
    buttonImageContainer: {
        backgroundColor: 'white',
        alignItems: 'center',
        justifyContent: 'center',
        width: '40%',
        height: '100%'
    },
    buttonImage: {
        width: '85%',
        height: '85%',
        overflow: 'hidden',
        resizeMode: 'contain',
    },
    buttonTextContainer: {
        backgroundColor: 'white',
        alignItems: 'flex-start',
        justifyContent: 'center',
        paddingLeft: 10,
        width: '60%',
        height: '100%'
    },
    buttonText: {
        fontSize: FontSize(15),
        color: 'navy'
    },
});

export default SalesOrder;
