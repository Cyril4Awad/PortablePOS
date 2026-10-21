import React from 'react';
import Sizing from '../variables/Sizing';
import Colors from '../variables/Colors';
import { FontSize } from '../variables/ResponsiveFont';
import { View, StyleSheet, Text, Image, TouchableOpacity } from 'react-native';

const ListFooter = props => {

    return (
        <View style={styles.container}>

            <TouchableOpacity style={styles.button} onPress={() => {props.setSearchVisible(false)}}>
                <Text style={styles.buttonText}>Close Search</Text>
            </TouchableOpacity>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.brandBlue,
        elevation: 15,
        width: '100%',
        height: 40,
        alignItems: 'center',
        justifyContent: 'center',
    },
    button: {
        backgroundColor: Colors.brandGreen,
        width:'100%',
        height:'100%',
        alignItems: 'center',
        justifyContent: 'center',
    },
    buttonText: {
        fontSize: FontSize(17),
        color: 'white',
    }
});

export default ListFooter;
