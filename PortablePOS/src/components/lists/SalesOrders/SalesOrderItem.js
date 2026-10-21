import React from 'react';
import { useState } from 'react';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity, Modal, Keyboard } from 'react-native';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';
import SalesOrderQty from './SalesOrderQty';

const SalesOrderItem = props => {

    const [modalVisible, setModalVisible] = useState(false);
    const [selectedItemQtyNeeded, setSelectedItemQtyNeeded] = useState(0);
    const [selectedHRecid, setSelectedHRecid] = useState(0);
    const [selectedCounter, setSelectedCounter] = useState(0);
    const [selectedItemRecID, setSelectedItemRecID] = useState('');
    const [selectedItemQty, setSelectedItemQty] = useState(1);
    const [selectedItemReference, setSelectedItemReference] = useState('');
    const [selectedItemDescription, setSelectedItemDescription] = useState('');

    const [detailsVisible, setDetailsVisible] = useState(false);

    const imageSource = () => {
        if (props.sodQtyPrepared == 0) {
            return  require('../../../assets/icons/trolleyAddEmpty.png')
        }
        else if (props.sodQtyPending == 0 || props.sodQtyPending < 0) {
            return  require('../../../assets/icons/trolleyFull.png')
        }
        else {
            return  require('../../../assets/icons/trolleyEdit.png')
        }
    } 

    const Quantities = () => {
        if (detailsVisible) {

            return (

                <TouchableOpacity style={styles.itemQtysContainer} onLongPress={() => {setDetailsVisible(false)}}>

                    <View style={styles.itemQtyContainer}>
                        <Text style={styles.itemQtyCaptionText}>Offered</Text>
                        <Text style={styles.itemQtyDataText}>{props.sodQtyOffered}</Text>
                    </View>

                    <View style={styles.itemQtyContainer}>
                        <Text style={styles.itemQtyCaptionText}>Delivered</Text>
                        <Text style={styles.itemQtyDataText}>{props.sodQtyDelivered}</Text>
                    </View>

                    <View style={styles.itemQtyContainer}>
                        <Text style={styles.itemQtyCaptionText}>Invoiced</Text>
                        <Text style={styles.itemQtyDataText}>{props.sodQtyInvoiced}</Text>
                    </View>

                    <View style={styles.itemQtyContainer}>
                        <Text style={styles.itemQtyCaptionText}>Prepared</Text>
                        <Text style={styles.itemQtyDataText}>{props.sodQtyPrepared}</Text>
                    </View>

                    <View style={styles.itemQtyContainer}>
                        <Text style={styles.itemQtyCaptionText}>Pending</Text>
                        <Text style={styles.itemQtyDataText}>{props.sodQtyPending}</Text>
                    </View>

                </TouchableOpacity>

            )

        }
        else {

            return (

                <TouchableOpacity style={styles.itemQtysContainer} onLongPress={() => {setDetailsVisible(true)}}>

                <View style={[styles.itemQtyContainer, {width : '30%'}]}>
                    <Text style={styles.itemQtyCaptionText}>Needed</Text>
                    <Text style={styles.itemQtyDataText}>{props.sodQtyNeeded}</Text>
                </View>

                <View style={[styles.itemQtyContainer, {width : '30%'}]}>
                    <Text style={styles.itemQtyCaptionText}>Prepared</Text>
                    <Text style={styles.itemQtyDataText}>{props.sodQtyPrepared}</Text>
                </View>

            </TouchableOpacity>

            )

        };
    };

    return (

        <View style={styles.container} >

            <Modal visible={modalVisible}
                transparent={true}
                presentationStyle="overFullScreen"
                >
                <View style={styles.modalContainer}>
                    <View style={styles.modalCard}>

                        <SalesOrderQty
                            setModalVisible={setModalVisible}
                            selectedItemQtyNeeded={selectedItemQtyNeeded}
                            setSelectedItemQtyNeeded={setSelectedItemQtyNeeded}
                            selectedHRecid={selectedHRecid}
                            selectedCounter={selectedCounter}
                            selectedItemRecID={selectedItemRecID}
                            selectedItemReference={selectedItemReference}
                            selectedItemDescription={selectedItemDescription}
                            refreshSalesOrderItems={() => props.refreshSalesOrderItems()}
                        />

                    </View>
                </View>
            </Modal>

            <View style={styles.itemReferenceDescriptionContainer}>

                <View style={styles.itemReferenceContainer}>
                    <Text numberOfLines={1} style={styles.itemReferenceText}>{props.sodItemReference}</Text>
                </View>

                <View style={styles.itemDescriptionContainer}>
                    <Text numberOfLines={1} style={styles.itemDescriptionText}>{props.sodItemDescription} - {props.sodUnit}</Text>
                </View>

            </View>

            <View style={styles.itemQtysAddQtyContainer}>

                <Quantities />

                <TouchableOpacity style={styles.addQtyContainer} onPress={() => {
                    setSelectedHRecid(props.sodHRecid);
                    setSelectedCounter(props.sodCounter);
                    setSelectedItemRecID(props.sodItemRecID);
                    setSelectedItemQty(1);
                    setSelectedItemReference(props.sodItemReference);
                    setSelectedItemDescription(props.sodItemDescription);
                    setSelectedItemQtyNeeded(props.sodQtyNeeded);
                    setModalVisible(true);
                    //alert(props.sodItemRecID)
                }}>
                    <Image
                        source={imageSource()}
                        style={styles.addQtyImage}
                        resizeMode='contain'
                    />
                </TouchableOpacity>

            </View>

            <View style={[styles.remarksContainer, {height: props.sodRemarks !== '' ? 20 : 0}]}>
                <Text numberOfLines={1} style={styles.remarksText}>{props.sodRemarks}</Text>
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
        //elevation: 10,
    },
    modalContainer: {
        width: '100%',
        height: '100%',
        backgroundColor: 'rgba(0,0,0,0.5)',
        alignItems: 'center',
        justifyContent: 'center',
      },
      modalCard: {
        width: Sizing.isPortableScanner ? Sizing.windowsWidth * 0.9 : Sizing.windowsWidth * 0.9,
        height: Sizing.isPortableScanner ? Sizing.windowsHeight * 0.8 : Sizing.windowsHeight * 0.6,
        backgroundColor: 'white',
        borderRadius: 20,
        overflow: 'hidden',
        elevation: 10,
        borderWidth: 5,
        borderColor: Colors.brandBlue,
        alignItems: 'center',
        justifyContent: 'center',
      },
    itemReferenceDescriptionContainer: {
        backgroundColor: 'gainsboro',
        width: '100%',
        height: 30,
        flexDirection: 'row',
        elevation: 10,
    },
    itemReferenceContainer: {
        //backgroundColor: 'yellow',
        width: 130,
        height: '100%',
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent:'center',    
    },
    itemReferenceText: {
        fontSize: FontSize(18),
        color: Colors.brandBlue,
        fontWeight: 'bold',
    },
    itemDescriptionContainer: {
        //backgroundColor: 'blue',
        flex: 1,
        height: '100%',
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent:'flex-start',
        paddingLeft: 3,
    },
    itemDescriptionText: {
        fontSize: FontSize(18),
        color: Colors.brandBlue,
    },
    itemQtysAddQtyContainer: {
        //backgroundColor: 'yellow',
        flexDirection: Sizing.isTablet ? 'row' : 'row',
        width: '100%',
        alignItems: Sizing.isTablet ? 'center' : 'flex-start',
        //justifyContent: 'space-around',
        //padding: 5,
    },
    itemQtysContainer: {
        flex: 1,
        // /backgroundColor: 'red',
        height: '100%',
        flexDirection: 'row',
        alignItems : 'center',
        justifyContent: 'space-evenly',
    },
    itemQtyContainer: {
        backgroundColor: 'gainsboro',
        opacity: 0.85,
        //flexDirection: 'row',
        //padding: 5,
        borderRadius: 5,
        alignItems: 'center',
        justifyContent: 'center',
        width: '18%',
        height: 50,
        //margin: 5,
        //elevation: 5,
    },
    itemQtyCaptionText: {
        fontSize: FontSize(12),
        paddingLeft: 5,
        paddingRight: 5,
        color: 'gray',
    },
    itemQtyDataText: {
        fontSize: FontSize(15),
        paddingLeft: 5,
        paddingRight: 5,
        fontWeight: 'bold',
        color: 'black',
    },
    addQtyContainer: {
        backgroundColor: 'gainsboro',
        //flexDirection: 'row',
        width: 50,
        height: 50,
        padding: 5,
        borderRadius: 5,
        alignItems: 'center',
        justifyContent: 'center',
        margin: 5,
    },
    addQtyImage: {
        //alignItems: 'center',
        //marginHorizontal: 10,
        resizeMode: 'contain',
        width: '90%',
        height: '90%',
        //backgroundColor:'red',
    },
    remarksContainer: {
        //backgroundColor:'red',
        width: '100%',
        alignItems: 'flex-start',
        justifyContent: 'center',
    },
    remarksText: {
        fontSize: FontSize(17),
        fontStyle: 'italic',
        color: 'black',
    },
});

export default SalesOrderItem;
