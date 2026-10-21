import React from 'react';
import { useState } from 'react';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity, Modal } from 'react-native';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';
import { QtySelector } from '../../controls/QtySelector';

const Item = props => {
    
    const [tmpQty, setTmpQty] = useState(1);

    return (

        <View style={styles.container} >

            <View style={styles.buttonContainer}>

                <TouchableOpacity style={styles.button} onPress={() => {
                    if (typeof props.setSelectedItemQty === 'function') {
                        props.setSelectedItemQty(tmpQty);
                    }
                    if (typeof props.onPress === 'function') {
                        props.onPress();
                    }
                    }} onLongPress={typeof props.onLongPress === 'function' ? props.onLongPress : undefined}>

                    <View style={styles.itemContainer}>

                        <View style={styles.itemReferenceBarcodeContainer}>

                            <View style={styles.itemReferenceContainer}>
                                <Text style={styles.itemReferenceText}>{props.itemReference}</Text>
                            </View>

                            <View style={styles.itemBarcodeContainer}>
                                <Text style={styles.itemBarcodeText}>{props.itemBarcode}</Text>
                            </View>

                        </View>

                        <View style={styles.itemDescriptionPriceContainer}>

                            <View style={styles.itemDescriptionContainer}>
                                <Text style={styles.itemDescriptionText}>{props.itemDescription}</Text>
                            </View>

                            <View style={styles.itemPriceAvailablilityContainer}>

                                {/* infoText (optional) : shown instead of the price (the Receiving Order shows the unit) */}
                                {props.infoText !== undefined ?
                                    <View style={styles.itemPriceContainer}>
                                        <Text style={styles.itemCurrencyText}>{props.infoText}</Text>
                                    </View> :
                                    <View style={styles.itemPriceContainer}>
                                        <Text style={styles.itemPriceText}>{Intl.NumberFormat("ar-de").format(props.itemPrice)} </Text>
                                        <Text style={styles.itemCurrencyText}>{props.itemCurrency}</Text>
                                    </View>}

                                <View style={styles.itemAvailablilityContainer}>
                                    {/* <Text style={[styles.itemAvailableQtyText, { color: props.itemAvailableQty > 0 ? 'black' : 'red' }]}>{Intl.NumberFormat("ar-de").format(props.itemAvailableQty)} </Text>
                                    <Text style={styles.itemUnitText}>{props.itemUnit}</Text> */}
                                </View>

                            </View>

                        </View>

                    </View>

                   {/* <View style={styles.itemQtyButtonsContainer}>
                            <QtySelector value={tmpQty} onChange={setTmpQty} min={1} />
                    </View>*/} 
      
                </TouchableOpacity>

            </View>

        </View>

    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.bgColor,
        width: '100%',
        height: 70,
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
        alignItems: 'flex-start',
        justifyContent: 'flex-start',
        //backgroundColor: 'red',
        //paddingLeft: 10,
        flexDirection: 'row',
        paddingRight: 10,
    },
    itemQtyButtonsContainer: {
        //backgroundColor: 'blue',
        width: '15%',
        height: '100%',
    },
    itemContainer: { 
        //backgroundColor: 'yellow',
        width: '100%',
        height: '100%',
        paddingLeft: 5,
        paddingRight: 5
    },
    itemReferenceBarcodeContainer:{
        //backgroundColor: 'red',
        //width: '100%',
        height: 25,
        flexDirection: 'row',
    },
    itemReferenceContainer:{
        //backgroundColor: 'red',
        width: '50%',
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'flex-start',
    },
    itemReferenceText:{
        color: 'black',
        fontWeight: 'bold',
        fontSize: FontSize(16),
    },
    itemBarcodeContainer:{
        //backgroundColor: 'green',
        width: '50%',
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'flex-end',
    },
    itemBarcodeText:{
        color: Colors.brandGreen,
        fontStyle: 'italic',
        fontSize: FontSize(16),
    },
    itemDescriptionPriceContainer: {
        //backgroundColor: 'yellow',
        //width: '100%',
        height: 40,
        flexDirection: 'row',
        alignItems: 'flex-start',
        justifyContent: 'space-between'
    },
    itemDescriptionContainer: {
        //backgroundColor: 'red',
        width: '70%',
        alignItems: 'center',
        justifyContent: 'space-evenly',
    },
    itemDescriptionText: {
        color: Colors.brandBlue,
        //backgroundColor: 'purple',
        fontSize: FontSize(16),
        width: '100%'
    },
    itemPriceAvailablilityContainer: {
        //backgroundColor: 'red',
        overflow:'hidden'
        //width: '30%',
    },
    itemPriceContainer: {
        //backgroundColor: 'lightblue',
        //width: '100%',
        alignItems: 'flex-start',
        justifyContent: 'flex-end',
        flexDirection: 'row',
    },
    itemPriceText: {
        color: Colors.brandBlue,
        fontWeight: 'bold',
        fontSize: FontSize(17),
    },
    itemCurrencyText: {
        color: 'darkgray',
        fontSize: FontSize(16),
    },
    itemAvailablilityContainer: {
        //backgroundColor: 'lightblue',
        // width: '100%',
        alignItems: 'flex-start',
        justifyContent: 'flex-end',
        flexDirection: 'row',
        opacity : 0.5
    },
    itemAvailableQtyText: {
        fontWeight: 'bold',
        fontStyle: 'italic',
        fontSize: FontSize(14),
    },
    itemUnitText: {
        color: 'black',
        fontStyle: 'italic',
        fontSize: FontSize(14),
    },
});

export default Item;
