import React from 'react';
import { View, StyleSheet, Text, TouchableOpacity } from 'react-native';
import Colors from '../../../variables/Colors';
import { FontSize } from '../../../variables/ResponsiveFont';
import { FormatQuantity, RECEIVE_TYPE_LABELS, RECEIVE_TYPE_COLORS, RECEIVE_TYPES } from '../../../variables/FreeReceivingRules';

// One collected line : item, type (Normal / Promotion / Free), received qty, supplier and invoice, linked item of a promotion
// props : line, onPress
const FreeReceivingLine = props => {

    const line = props.line;
    const receipt = [line.supplierCode, line.invoiceNumber !== '' ? `Inv ${line.invoiceNumber}` : ''].filter(text => text !== '').join('  ·  ');

    return (

        <View style={styles.container}>

            <TouchableOpacity style={styles.button} onPress={props.onPress}>

                <View style={styles.lineRow}>
                    <Text style={styles.referenceText} numberOfLines={1}>{line.itemReference}</Text>
                    <View style={[styles.typeBadge, { backgroundColor: RECEIVE_TYPE_COLORS[line.receiveType] }]}>
                        <Text style={styles.typeBadgeText}>{RECEIVE_TYPE_LABELS[line.receiveType]}</Text>
                    </View>
                </View>

                <Text style={styles.descriptionText} numberOfLines={1}>{line.itemDescription}</Text>

                <View style={styles.lineRow}>
                    <Text style={styles.infoText} numberOfLines={1}>
                        {line.receiveType === RECEIVE_TYPES.PROMOTION && line.linkedItemRecID !== '' ? `Linked to ${line.linkedItemReference}   ` : ''}
                        {receipt}
                    </Text>
                    <Text style={styles.qtyText}>{FormatQuantity(line.qty)} {line.itemUnit}</Text>
                </View>

            </TouchableOpacity>

        </View>

    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.bgColor,
        width: '100%',
        height: 75,
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
    typeBadge: {
        borderRadius: 9,
        paddingHorizontal: 8,
        height: 18,
        justifyContent: 'center',
        marginLeft: 8,
    },
    typeBadgeText: {
        color: 'white',
        fontSize: FontSize(12),
        fontWeight: 'bold',
    },
    descriptionText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
        height: 22,
    },
    infoText: {
        color: 'gray',
        fontSize: FontSize(13),
        fontStyle: 'italic',
        flex: 1,
    },
    qtyText: {
        color: Colors.brandGreen,
        fontSize: FontSize(15),
        fontWeight: 'bold',
        marginLeft: 8,
    },
});

export default FreeReceivingLine;
