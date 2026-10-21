import React from 'react';
import Sizing from '../../variables/Sizing';
import Colors from '../../variables/Colors';
import { FontSize } from '../../variables/ResponsiveFont';
import { View, Text, Image, StyleSheet, TouchableOpacity } from 'react-native';

const MenuHeader = props => {

    return (
        <View style={styles.container}>

            <Text style={styles.titleText}>{props.title}</Text>            

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        width: Sizing.windowsWidth,
        height: 35,
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor : 'gainsboro',
        //backgroundColor : Colors.brandBlue,
        elevation: 7,
        //marginBottom : 0,
        //borderWidth : 1
    },
    titleText: {
        fontSize: FontSize(19),
        color: Colors.brandBlue,
        //color: 'white',
        fontWeight: 'bold'
    }
});

export default MenuHeader;
