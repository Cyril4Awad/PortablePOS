import React from 'react';
import { View, StyleSheet, Text, TouchableOpacity } from 'react-native';
import Colors from '../../../variables/Colors';
import { FontSize } from '../../../variables/ResponsiveFont';
import { FormatQuantity, LotText } from '../../../variables/TransferRules';

// One line of the transfer : item, lot, destination and qty
// props : line, showDestination, onPress
const TransferLine = props => {

    const line = props.line;
    const lotText = LotText(line);

    return (

        <View style={styles.container}>

            <TouchableOpacity style={styles.button} onPress={props.onPress}>

                <View style={styles.lineRow}>
                    <Text style={styles.referenceText} numberOfLines={1}>{line.itemReference}</Text>
                    {props.showDestination ?
                        <View style={styles.destinationBadge}>
                            <Text style={styles.destinationBadgeText}>→ {line.destination}</Text>
                        </View> :
                        <></>}
                </View>

                <Text style={styles.descriptionText} numberOfLines={1}>{line.itemDescription}</Text>

                <View style={styles.lineRow}>
                    <Text style={styles.infoText} numberOfLines={1}>{lotText}</Text>
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
    destinationBadge: {
        borderRadius: 9,
        paddingHorizontal: 8,
        height: 18,
        justifyContent: 'center',
        marginLeft: 8,
        backgroundColor: Colors.brandBlue,
    },
    destinationBadgeText: {
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

export default TransferLine;
