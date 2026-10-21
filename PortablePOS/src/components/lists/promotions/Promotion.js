import React from 'react';
import { useState } from 'react';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity, Modal } from 'react-native';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';

const Promotion = props => {

    return (

        <View style={styles.container} >

            {props.promType === '' ? <></> : 
                <View style={styles.promotionHeaderContainer}>
                    <Text style={styles.promotionHeaderDescriptionText} numberOfLines={1}>{props.promDescription}</Text>
                    <Text style={styles.promotionHeaderTypeText}>{props.promType}</Text>
                </View>
            }

            {props.promBuyGet.startsWith('Buy') ?
                <View style={styles.buyContainer}>
                    <Text style={styles.buyText}>{props.promBuyGet} {props.promItemCategoryIdentificationType} : {props.promItemCategoryIdentification}</Text>
                </View>
                :
                <View style={styles.getContainer}>
                    <Text style={styles.getText}>{props.promBuyGet} {props.promItemCategoryIdentificationType} : {props.promItemCategoryIdentification}</Text>
                    <Text style={styles.getText}>Qty : {props.promGetQty} - Discount : {props.promGetDisc} %</Text>
                </View>
            }

        </View>								
    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.bgColor,
        width: '100%',
        //height: 70,
        //borderWidth: 1,
        //borderRadius: 10,
        alignItems: 'flex-start',
        justifyContent: 'center',
    },
    promotionHeaderContainer: {
        //backgroundColor: 'red',
        width: '100%',
        height: 28,
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between',
        paddingLeft: 10, 
        paddingRight: 10,
    },
    promotionHeaderDescriptionText: {
        color: Colors.brandBlue,
        fontWeight: 'bold',
        fontSize: FontSize(17)
    },
    promotionHeaderTypeText: {
        color: Colors.brandGreen,
        fontStyle: 'italic',
        fontSize: FontSize(17)
    },
    buyContainer: {
        //backgroundColor: 'red',
        width: '100%',
        height: 30,
        paddingLeft: 10, 
        paddingRight: 10,
        alignItems: 'flex-start',
        justifyContent: 'center',
    },
    buyText: {
        color: 'black',
        fontSize: FontSize(16),
    },
    getContainer: {
        //backgroundColor: 'yellow',
        width: '100%',
        //height: 30,
        paddingLeft: 10, 
        paddingRight: 10,
        paddingBottom: 10,
        alignItems: 'flex-end',
        justifyContent: 'center',
        borderBottomColor: Colors.brandBlue,
        borderBottomWidth: 1,
    },
    getText: {
        color: 'black',
        fontStyle: 'italic',
        fontSize: FontSize(16),
    },
});

export default Promotion;
