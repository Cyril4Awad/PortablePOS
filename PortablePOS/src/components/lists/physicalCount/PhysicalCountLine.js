import React from 'react';
import { View, StyleSheet, Text, TouchableOpacity } from 'react-native';
import Colors from '../../../variables/Colors';
import { FontSize } from '../../../variables/ResponsiveFont';
import { FormatQuantity, LineFields } from '../../../variables/PhysicalCountRules';

// One line of the physical count (Items list and item picker)
// The fields depend on the item : expiry / lot, color / grid / size, and the available qty only with the privilege
const PhysicalCountLine = props => {

    const line = props.line;
    const fields = LineFields(props.header, line);

    const infoTexts = [];
    if (fields.showFashion) {
        infoTexts.push([line.colorDescription || line.colorCode, line.gridDescription || line.gridCode, line.sizeDescription || line.sizeOrder]
            .filter(text => (text ?? '') !== '').join(' / '));
    }
    if (fields.showExpiry) {
        infoTexts.push(`Exp ${line.expiry || '-'}   Lot ${line.batch || '-'}`);
    }

    return (

        <View style={styles.container}>

            <TouchableOpacity style={styles.button} onPress={props.onPress}>

                <View style={styles.lineRow}>
                    <Text style={styles.referenceText} numberOfLines={1}>{line.itemReference}</Text>
                    <Text style={styles.lineRecIDText}>#{line.lineRecID}</Text>
                </View>

                {/* Counted lines : uploaded or waiting for the upload (state 1 to upload, 2 uploaded) */}
                <View style={styles.lineRow}>
                    <Text style={styles.descriptionText} numberOfLines={1}>{fields.showBook ? line.bookTitle : line.itemDescription}</Text>
                    {line.state === 1 ? <Text style={[styles.stateText, { color: 'red' }]}>● To upload</Text> : <></>}
                    {line.state === 2 ? <Text style={[styles.stateText, { color: Colors.brandGreen }]}>✓ Uploaded</Text> : <></>}
                </View>

                <View style={styles.lineRow}>

                    <Text style={styles.infoText} numberOfLines={1}>{infoTexts.join('   ')}</Text>

                    <Text style={styles.qtyText}>
                        {fields.showAvailableQty && line.systemQty !== null ? `Avlb ${FormatQuantity(line.systemQty)}   ` : ''}
                        <Text style={styles.countedText}>Counted {FormatQuantity(line.counted)}</Text>
                    </Text>

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
    lineRecIDText: {
        color: 'gray',
        fontSize: FontSize(14),
    },
    descriptionText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
        flex: 1,
    },
    stateText: {
        marginLeft: 8,
        fontSize: FontSize(13),
        fontWeight: 'bold',
    },
    infoText: {
        color: 'gray',
        fontSize: FontSize(13),
        fontStyle: 'italic',
        flex: 1,
    },
    qtyText: {
        color: 'black',
        fontSize: FontSize(14),
    },
    countedText: {
        color: Colors.brandGreen,
        fontWeight: 'bold',
    },
});

export default PhysicalCountLine;
