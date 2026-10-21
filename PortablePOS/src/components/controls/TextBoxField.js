import React from 'react';
import Sizing from '../../variables/Sizing';
import Colors from '../../variables/Colors';
import { FontSize } from '../../variables/ResponsiveFont';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity } from 'react-native';

const TextBoxField = props => {

    return (
        <View style={styles.container}>

            <View style={styles.infoContainer}>

                <View style={styles.titleContainer}>
                    <Text style={styles.titleText}>{props.title}</Text>
                </View>

                <View style={styles.dataContainer}>

                    <TextInput 
                        value={props.value}
                        multiline={props.multiline}
                        maxLength={props.maxLength}
                        numberOfLines={props.numberOfLines}
                        autoCapitalize={props.autoCapitalize}
                        autoCorrect={props.autoCorrect}
                        style={styles.dataText}
                        onChangeText={props.onChangeText}
                     />

                </View>

            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        //backgroundColor: 'red',
        height: 50,
        flexDirection: 'row',
        borderLeftWidth: 2,
        borderBottomWidth: 2,
        borderColor: 'silver',
        marginBottom: 10,
    },
    infoContainer: {
        width: '100%',
    },
    titleContainer: {
        //backgroundColor: 'green',
        alignItems: 'flex-start',
        justifyContent: 'center',
        width: '100%',
        height: 15,
        paddingLeft: 8,
        paddingRight: 5,
    },
    titleText: {
        fontSize: FontSize(14),
        fontWeight: 'bold',
        color: Colors.brandGreen,
    },
    dataContainer: {
        //backgroundColor: 'purple',
        alignItems: 'flex-start',
        justifyContent: 'center',
        width: '100%',
        height: 35,
        paddingLeft: 5,
        paddingRight: 5,
    },
    dataText: {
        //backgroundColor: 'red',
        color: Colors.brandBlue,
        fontSize: FontSize(15),
        width: '100%',
        height: '100%',
    },
});

export default TextBoxField;
