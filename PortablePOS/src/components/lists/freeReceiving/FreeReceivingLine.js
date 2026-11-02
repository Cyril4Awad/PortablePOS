import React from 'react';
import { View, StyleSheet, Text, TouchableOpacity } from 'react-native';
import Colors from '../../../variables/Colors';
import { FontSize } from '../../../variables/ResponsiveFont';
import { STATUS_LABELS, FormatQty } from '../../../variables/ReceivingOrderRules';

const STATUS_COLORS = { N: Colors.brandGreen, F: Colors.brandBlue, R: 'rgb(200, 60, 60)' };

// One line of a receiving order saved on the device : item, status (Normal / Free / Return), qty, lots of an expiry item,
// supplier and RO Ref #, RO # once uploaded
// props : line, onPress, onLongPress
const FreeReceivingLine = props => {

    const line = props.line;
    const receipt = [line.supplierCode, `Ref ${line.roRef}`, line.state === 2 ? `RO # ${line.roRecID}` : 'Not uploaded'].join('  ·  ');

    return (

        <View style={styles.container}>

            <TouchableOpacity style={styles.button} onPress={props.onPress} onLongPress={props.onLongPress}>

                <View style={styles.lineRow}>
                    <Text style={styles.referenceText} numberOfLines={1}>{line.itemReference}</Text>
                    <View style={[styles.typeBadge, { backgroundColor: STATUS_COLORS[line.lineStatus] ?? 'gray' }]}>
                        <Text style={styles.typeBadgeText}>{STATUS_LABELS[line.lineStatus] ?? line.lineStatus}</Text>
                    </View>
                </View>

                <Text style={styles.descriptionText} numberOfLines={1}>{line.itemDescription}</Text>

                {line.lotsText ? <Text style={styles.lotsText} numberOfLines={1}>{line.lotsText}</Text> : <></>}

                <View style={styles.lineRow}>
                    <Text style={[styles.infoText, line.state === 2 ? null : styles.pendingText]} numberOfLines={1}>{receipt}</Text>
                    <Text style={[styles.qtyText, line.state === 2 ? styles.uploadedQtyText : null]}>{FormatQty(line.qty)} {line.itemUnit}</Text>
                </View>

            </TouchableOpacity>

        </View>

    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.bgColor,
        width: '100%',
        paddingBottom: 2,
    },
    button: {
        backgroundColor: Colors.listItemBackColor,
        width: '100%',
        justifyContent: 'center',
        paddingHorizontal: 10,
        paddingVertical: 5,
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
    lotsText: {
        color: 'black',
        fontSize: FontSize(13),
    },
    infoText: {
        color: 'gray',
        fontSize: FontSize(13),
        fontStyle: 'italic',
        flex: 1,
    },
    pendingText: {
        color: 'red',
    },
    qtyText: {
        color: Colors.brandGreen,
        fontSize: FontSize(15),
        fontWeight: 'bold',
        marginLeft: 8,
    },
    uploadedQtyText: {
        color: 'gray',
    },
});

export default FreeReceivingLine;
