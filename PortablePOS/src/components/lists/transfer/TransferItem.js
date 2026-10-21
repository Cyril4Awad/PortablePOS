import React from 'react';
import { View, StyleSheet, Text, TouchableOpacity } from 'react-native';
import Colors from '../../../variables/Colors';
import { FontSize } from '../../../variables/ResponsiveFont';
import { FormatQuantity } from '../../../variables/TransferRules';

// One item of the downloaded stock : available qty in the location, qty already in the transfer, price
// props : item (with available, transferQty), currencyLC, onPress (optional)
const TransferItem = props => {

    const item = props.item;

    return (

        <View style={styles.container}>

            <TouchableOpacity style={styles.button} onPress={props.onPress} disabled={!props.onPress}>

                <View style={styles.lineRow}>
                    <Text style={styles.referenceText} numberOfLines={1}>{item.itemReference}</Text>
                    <Text style={styles.priceText}>{FormatPrice(item.priceLC)} {props.currencyLC ?? ''}</Text>
                </View>

                <Text style={styles.descriptionText} numberOfLines={1}>{item.itemDescription}</Text>

                <View style={styles.lineRow}>
                    <Text style={[styles.availableText, { color: item.available > 0 ? 'black' : 'red' }]}>
                        Avlb {FormatQuantity(item.available)} {item.itemUnit}
                    </Text>
                    {item.transferQty > 0 ?
                        <Text style={styles.transferText}>In Transfer {FormatQuantity(item.transferQty)}</Text> :
                        <></>}
                </View>

            </TouchableOpacity>

        </View>

    );
};

export const FormatPrice = (value) => Number(value || 0).toLocaleString('en-US', { maximumFractionDigits: 2 });

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.bgColor,
        width: '100%',
        height: 72,
        paddingBottom: 2,
    },
    button: {
        backgroundColor: Colors.listItemBackColor,
        width: '100%',
        height: '100%',
        justifyContent: 'center',
        paddingLeft: 10,
        paddingRight: 10,
    },
    lineRow: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between',
        height: 22,
    },
    referenceText: {
        color: 'black',
        fontWeight: 'bold',
        fontSize: FontSize(16),
        flex: 1,
    },
    priceText: {
        color: 'gray',
        fontSize: FontSize(14),
        marginLeft: 8,
    },
    descriptionText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
        height: 22,
    },
    availableText: {
        fontSize: FontSize(14),
        fontWeight: 'bold',
    },
    transferText: {
        color: Colors.brandGreen,
        fontSize: FontSize(14),
        fontWeight: 'bold',
    },
});

export default TransferItem;
