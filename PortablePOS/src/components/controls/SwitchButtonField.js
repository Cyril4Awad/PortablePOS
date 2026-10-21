import React from 'react';
import { useState, useEffect } from 'react';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity } from 'react-native';
import Colors from '../../variables/Colors';
import { FontSize } from '../../variables/ResponsiveFont';

const SwitchButtonField = props => {

    const [option, setOption] = useState(props.data);

    return (
        <TouchableOpacity style={styles.container} onPress={() => {setOption( option === props.option1 ? props.option2 : props.option1)}}>

            <View style={styles.fieldTitleDescription}>

                <View style={styles.fieldTitle}>
                    <Text style={styles.fieldTitleText}>{props.title}</Text>
                </View>

                <View style={styles.fieldDescription}>
                    <Text style={styles.fieldDescriptionText}>{props.description}</Text>
                </View>

            </View>

            <View style={styles.fieldData}>
                <Text style={styles.fieldDataText}>{option}</Text>
            </View>

        </TouchableOpacity>
    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.bgColor,
        //backgroundColor: 'blue',
        height: 50,
        paddingLeft: 10,
        paddingRight: 10,
        borderColor: 'silver',
        borderBottomWidth: 1,
        flexDirection: 'row',
        overflow: 'hidden',
    },
    fieldTitleDescription: {
        height: '100%',
        width: '70%',
    },
    fieldTitle: {
        //backgroundColor: 'red',
        width: '100%',
        height: '50%',
        paddingLeft: 5,
        alignItems: 'flex-start',
        justifyContent: 'center',
    },
    fieldTitleText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
        fontWeight: 'bold',
    },
    fieldDescription: {
        //backgroundColor: 'black',
        width: '100%',
        height: '50%',
        paddingLeft: 5,
        alignItems: 'flex-start',
        justifyContent: 'center',
    },
    fieldDescriptionText: {
        color: 'silver',
        fontSize: FontSize(17),
        fontStyle: 'italic',
    },
    fieldData: {
        //backgroundColor: 'green',
        width: '30%',
        height: '100%',
        paddingLeft: 5,
        alignItems: 'center',
        justifyContent: 'center',
    },
    fieldDataText: {
        fontSize: FontSize(18),
        color: Colors.brandBlue,
        fontWeight: 'bold',
    },
});

export default SwitchButtonField;
