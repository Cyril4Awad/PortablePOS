import React from 'react';
import Sizing from '../variables/Sizing';
import Colors from '../variables/Colors';
import { FontSize } from '../variables/ResponsiveFont';
import { View, StyleSheet, Text, Image } from 'react-native';

const ScreenTitle = props => {

    return (
        <View style={styles.container}>

            <View style={styles.headerTextContainer}>
                <Text style={styles.headerText}>{props.title}</Text>
            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        elevation: 15,
        width: '100%',
        height: '10%',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: Colors.headerBackColor,
    },
    headerTextContainer: {
        //backgroundColor : 'red',
        width: '100%',
        alignItems: 'center',
        justifyContent: 'center',
    },
    headerText: {
        color: Colors.brandGreen,
        fontSize: FontSize(24),
        fontWeight: 'bold',
    },
});

export default ScreenTitle;
