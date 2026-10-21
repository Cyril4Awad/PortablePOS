import React from 'react';
import { View, StyleSheet, Text, TouchableOpacity, Image } from 'react-native';
import Colors from '../../../variables/Colors';
import { FontSize } from '../../../variables/ResponsiveFont';
import { FormatQuantity } from '../../../variables/PhysicalCountRules';
import { AvailableQtyText } from '../../../variables/PhysicalCountCreateRules';

// One item of Create Physical Count, same look as the lines of View Items with a check box (tap the line to check it)
// The available qty comes only with the privilege and is not shown when it is 0
// props : item, checked, onToggle(item) - memo : checking an item redraws only that line
const PhysicalCountCreateLine = props => {

    const item = props.item;
    const availableText = AvailableQtyText(item, FormatQuantity);
    const infoText = [item.brand, item.productGroup].filter(text => (text ?? '') !== '').join('  /  ');

    return (

        <View style={styles.container}>

            <TouchableOpacity style={styles.button} onPress={() => props.onToggle(item)}>

                <Image
                    source={props.checked ? require('../../../assets/icons/checked.png') : require('../../../assets/icons/unchecked.png')}
                    style={styles.checkBoxImage}
                    resizeMode='contain'
                />

                <View style={styles.textsContainer}>

                    <View style={styles.lineRow}>
                        <Text style={styles.referenceText} numberOfLines={1}>{item.itemReference}</Text>
                        {item.unit ? <Text style={styles.unitText}>{item.unit}</Text> : <></>}
                    </View>

                    <View style={styles.lineRow}>
                        <Text style={styles.descriptionText} numberOfLines={1}>{item.itemDescription}</Text>
                    </View>

                    <View style={styles.lineRow}>
                        <Text style={styles.infoText} numberOfLines={1}>{infoText}</Text>
                        {availableText !== '' ? <Text style={styles.qtyText}>{availableText}</Text> : <></>}
                    </View>

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
        flexDirection: 'row',
        alignItems: 'center',
        paddingLeft: 8,
        paddingRight: 10,
    },
    checkBoxImage: {
        width: 30,
        height: 30,
        marginRight: 8,
    },
    textsContainer: {
        flex: 1,
        justifyContent: 'center',
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
    },
    descriptionText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
        flex: 1,
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
        marginLeft: 8,
    },
});

export default React.memo(PhysicalCountCreateLine);
