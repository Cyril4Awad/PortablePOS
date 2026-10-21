import React from 'react';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity } from 'react-native';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';

const ClientGroup = props => {

    return (

        <View style={styles.container} >

            <View style={styles.buttonContainer}>

                <TouchableOpacity style={styles.button} onPress={props.onPress}>

                    <View style={styles.clientGroupDescContainer}>
                        <Text style={styles.clientGroupDescText}>{props.clientGroupDesc}</Text>
                    </View>

                </TouchableOpacity>

            </View>

        </View>

    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.bgColor,
        width: Sizing.isTablet ? (Sizing.windowsWidth / 5) : (Sizing.windowsWidth / 2),
        height: Sizing.isTablet ? 120 : 80,
        //borderWidth: 1,
        //borderRadius: 10,
        alignItems: 'center',
        justifyContent: 'center',
        //padding: 5,
        //marginBottom: 5,
        padding: Sizing.isTablet? 10 : 5,
    },
    buttonContainer: {
        backgroundColor : Colors.brandGreen,
        alignItems: 'center',
        justifyContent: 'center',
        width: '100%',
        height: '100%',
        borderRadius: 10,
        overflow: 'hidden',
        borderWidth: 1,
        borderColor: 'silver',
        //elevation: 5,
    },
    button: {
        backgroundColor : Colors.listItemBackColor,
        width: '100%',
        height: '100%',
        //backgroundColor: 'red',
        alignItems: 'center',
        justifyContent: 'center',
        paddingLeft: 10,
        paddingRight: 10,
    },
    clientGroupDescContainer: {
        //backgroundColor: 'yellow',
        width: '100%',
        height: Sizing.isTablet ? '70%' : '60%',
        alignItems: 'center',
        justifyContent: 'center',
    },
    clientGroupDescText: {
        fontWeight: 'bold',
        color: Colors.brandBlue,
        fontSize: FontSize(19),
    },
});

export default ClientGroup;
