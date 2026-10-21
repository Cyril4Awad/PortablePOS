import React, { useState } from 'react';
import Sizing from '../../variables/Sizing';
import Colors from '../../variables/Colors';
import { FontSize } from '../../variables/ResponsiveFont';
import ItemProductGroups from '../lists/stock/ItemProductGroups';
import Items from '../lists/stock/Items';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity, Modal } from 'react-native';

const StockItemSearchBox = props => {

const [stockScreenVisible, setStockScreenVisible] = useState('');
const [stockSearchVisible, setStockSearchVisible] = useState('');

    return (
        <View style={styles.container}>

            <Modal visible={stockScreenVisible}>
                <ItemProductGroups
                    setSelectedItemRecID={props.setSelectedItemRecID}
                    setSelectedItemQty={props.setSelectedItemQty}
                    selectedItemQty={props.selectedItemQty}
                    setSelectedItemReference={props.setSelectedItemReference}
                    setSelectedItemDescription={props.setSelectedItemDescription}
                    setSelectedItemVatType={props.setSelectedItemVatType}
                    setSelectedItemVatPerc={props.setSelectedItemVatPerc}
                    setSelectedItemPromotionCriteria={props.setSelectedItemPromotionCriteria}
                    setStockScreenVisible={setStockScreenVisible}
                />
            </Modal>

            <Modal visible={stockSearchVisible}>
                <Items
                    title='Search...'
                    itemProductGroupCode=''
                    itemPromotionCriteria=''
                    searchString={props.selectedItemDescription}
                    autoSelectSingleResult={true}
                    setSelectedItemRecID={props.setSelectedItemRecID}
                    setSelectedItemQty={props.setSelectedItemQty}
                    selectedItemQty={props.selectedItemQty}
                    setSelectedItemReference={props.setSelectedItemReference}
                    setSelectedItemDescription={props.setSelectedItemDescription}
                    setSelectedItemVatType={props.setSelectedItemVatType}
                    setSelectedItemVatPerc={props.setSelectedItemVatPerc}
                    setSelectedItemPromotionCriteria={props.setSelectedItemPromotionCriteria}
                    setStockScreenVisible={setStockSearchVisible}
                    onCloseButtonPress={() => { setStockSearchVisible(false) }}
                />
            </Modal>

            <View style={styles.infoContainer}>

                <View style={styles.titleContainer}>
                    <Text style={styles.titleText}>Stock Item</Text>
                </View>

                <View style={styles.dataContainer}>

                    <TextInput
                        value={props.selectedItemDescription}
                        style={styles.dataText}
                        onChangeText={(text) => {
                            props.setSelectedItemDescription(text);
                            props.setSelectedItemRecID('');
                            if (typeof props.setSelectedItemQty === 'function') {
                                props.setSelectedItemQty(1);
                            }
                        }}
                        editable={!props.disabled}
                        onSubmitEditing={() => {
                            
                            if (props.selectedItemDescription.trim().length > 2) {
                                setStockSearchVisible(true)
                            } 
                            else {
                                if (props.selectedItemDescription.trim() !== '') {
                                    alert('You must write at least 3 chars');
                                }
                                props.setSelectedItemDescription('');
                            }

                        }}
                    />

                </View>

            </View>

            <View style={styles.buttonsContainer}>

                {/* <View style={styles.buttonContainer}>
                    <TouchableOpacity style={styles.button} onPress={() => { alert('CI : Cannot Access The Camera From EMULATOR') }}>

                        <View style={styles.buttonImageContainer}>
                            <Image
                                source={require('./../../assets/letters/letterC.png')}
                                style={styles.buttonImage}
                                resizeMode="contain"
                            />
                        </View>

                        <View style={styles.buttonTextContainer}>
                            <Text style={styles.buttonText}>View Promotions</Text>
                        </View>

                    </TouchableOpacity>
                </View> */}

                {/* <View style={styles.buttonContainer}>
                    <TouchableOpacity style={styles.button} disabled={props.disabled} onPress={() => { alert('SB : Please use the machine scanner') }}>

                        <View style={styles.buttonImageContainer}>
                            <Image
                                source={require('./../../assets/letters/letterB.png')}
                                style={styles.buttonImage}
                                resizeMode="contain"
                            />
                        </View>

                        <View style={styles.buttonTextContainer}>
                            <Text style={styles.buttonText}>Scan Barcode</Text>
                        </View>

                    </TouchableOpacity>
                </View> */}

                <View style={styles.buttonContainer}>
                    <TouchableOpacity style={styles.button} disabled={props.disabled} onPress={() => {setStockScreenVisible(true) }}>

                        <View style={styles.buttonImageContainer}>
                            <Image
                                source={require('./../../assets/letters/letterS.png')}
                                style={styles.buttonImage}
                                resizeMode="contain"
                            />
                        </View>

                        <View style={styles.buttonTextContainer}>
                            <Text style={styles.buttonText}>Stock Search</Text>
                        </View>

                    </TouchableOpacity>
                </View>

            </View>

            {/* <TouchableOpacity style={styles.buttonContainer} onPress={() => {alert('Stock Item Search')}}>
                <Image
                    source={require('./../../assets/icons/stock.png')}
                    style={styles.buttonImage}
                    resizeMode="contain"
                />
            </TouchableOpacity> */}

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        //backgroundColor: 'red',
        height: 85,
        //flexDirection: 'row',
        borderLeftWidth: 2,
        borderBottomWidth: 2,
        borderColor: 'silver',
        marginBottom: 10,
    },
    infoContainer: {
        width: '100%',
    },
    titleContainer: {
        //backgroundColor: 'green',
        alignItems: 'flex-start',
        justifyContent: 'center',
        width: '100%',
        height: 15,
        paddingLeft: 8,
        paddingRight: 5,
    },
    titleText: {
        fontSize: FontSize(15),
        fontWeight: 'bold',
        color: Colors.brandGreen,
    },
    dataContainer: {
        //backgroundColor: 'purple',
        alignItems: 'flex-start',
        justifyContent: 'center',
        width: '100%',
        height: 35,
        paddingLeft: 5,
        paddingRight: 5,
    },
    dataText: {
        color: Colors.brandBlue,
        //backgroundColor: 'red',
        fontSize: FontSize(15),
        width: '100%',
        height: '100%',
    },
    buttonsContainer: {
        //backgroundColor: 'yellow',
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: Sizing.isTablet ? 'flex-end' : 'space-around',
        height: 35,
        width: '100%',
    },
    buttonContainer: {
        //backgroundColor: 'purple',
        alignItems: 'center',
        justifyContent: 'center',
        height: '80%',
        width: Sizing.isTablet ? '20%' : '30%',
        marginLeft: Sizing.isTablet ? 10 : 0,
        borderRadius: 10,
        overflow: 'hidden',
    },
    button: {
        //backgroundColor: 'green',
        flexDirection: 'row',
        width: '100%',
        height: '100%',
    },
    buttonImageContainer: {
        //backgroundColor: 'red',
        width: '30%',
        alignItems: 'center',
        justifyContent: 'center',
    },
    buttonImage: {
        width: '70%',
        height: '70%',
        overflow: 'hidden',
        resizeMode: 'contain',
    },
    buttonTextContainer: {
        //backgroundColor: 'blue',
        width: '70%',
        alignItems: 'flex-start',
        justifyContent: 'center',
        padding: 4,
    },
    buttonText: {
        fontSize: FontSize(12.5),
        color: 'black',
    }
});

export default StockItemSearchBox;
