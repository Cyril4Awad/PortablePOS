import React from 'react';
import { useState } from 'react';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity, Modal, Keyboard } from 'react-native';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';

const SalesOrderQtyLine = props => {

    const ItemDescription = () => {

        if (props.showItemDescription) {

            return (
                <Text style={styles.itemDescriptionText} numberOfLines={1}>{props.sopqItemDescription}</Text>
            );

        }
        else {

            return(
                <View></View>
            );

        }
        
    };

    return (

        <TouchableOpacity onLongPress={() => {
            props.deleteLine()
        }
        }
            style={[styles.container, { backgroundColor: props.isTotalSection ? Colors.brandGreen : 'white' ,
                                        height: props.showItemDescription ? 55 : 40,}]} >

            <View style={[styles.infoContainer, { alignItems: props.isTotalSection ? 'flex-end' : 'flex-start',
                                                  justifyContent: props.showItemDescription ? 'space-evenly' : 'center', }]}>

                <ItemDescription/>

                <Text style={styles.infoText}>{props.sopqInfo}</Text>

            </View>

            <View style={styles.qtyContainer}>
                <Text style={styles.qtyText}>{props.sopqQty}</Text>
            </View>

        </TouchableOpacity>

    );
};

const styles = StyleSheet.create({
    container: {
        width: '100%',
        alignItems: 'center',
        justifyContent: 'center',
        marginTop: 2,
        flexDirection: 'row',
        borderBottomWidth: 3,
        borderBottomColor: 'gainsboro',
    },
    infoContainer: {
        width: '70%',
        height: '100%',
        paddingRight: 10,
        paddingLeft: 10,
    },
    itemDescriptionText: {
        color: Colors.brandGreen,
        fontSize: FontSize(18),
        //fontWeight: 'bold',
    },
    infoText: {
        color: Colors.brandBlue,
        fontSize: FontSize(20),
    },
    qtyContainer: {
        //backgroundColor: 'yellow',
        alignItems: 'center',
        justifyContent: 'center',
        width: '30%',
        height: '100%',
    },
    qtyText: {
        color: Colors.brandBlue,
        fontSize: FontSize(22),
        fontWeight: 'bold',
    },
});

export default SalesOrderQtyLine;
