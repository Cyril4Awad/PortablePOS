import React from 'react';
import { View, StyleSheet, Text, TouchableOpacity } from 'react-native';
import Colors from '../../../variables/Colors';
import { FontSize } from '../../../variables/ResponsiveFont';

// One item of the downloaded stock file (Stock File list and item picker)
// props : item, onPress (optional)
const FreeReceivingItem = props => {

    const item = props.item;
    const coding = [item.itemSupplier, item.itemCategory, item.itemSerie].filter(text => (text ?? '') !== '').join(' / ');

    return (

        <View style={styles.container}>

            <TouchableOpacity style={styles.button} onPress={props.onPress} disabled={!props.onPress}>

                <View style={styles.lineRow}>
                    <Text style={styles.referenceText} numberOfLines={1}>{item.itemReference}</Text>
                    <Text style={styles.unitText}>{item.itemUnit}</Text>
                </View>

                <Text style={styles.descriptionText} numberOfLines={1}>{item.itemDescription}</Text>

                <View style={styles.lineRow}>
                    <Text style={styles.infoText} numberOfLines={1}>{coding}</Text>
                    <Text style={styles.infoText} numberOfLines={1}>{item.itemSingleRef2 || item.itemSingleRef1}</Text>
                </View>

            </TouchableOpacity>

        </View>

    );
};

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
    unitText: {
        color: 'gray',
        fontSize: FontSize(14),
        marginLeft: 8,
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
        flexShrink: 1,
    },
});

export default FreeReceivingItem;
