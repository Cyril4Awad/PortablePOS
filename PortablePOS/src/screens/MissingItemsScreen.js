import React, { useRef } from 'react';
import { useState, useEffect } from 'react';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import ScreenHeader from '../components/ScreenHeader';
import { View, Text, ScrollView, StyleSheet, Button } from 'react-native';
import TextBoxField from '../components/controls/TextBoxField';
import MainButton from '../components/controls/MainButton';
import MissingItemsData from '../data/MissingItemsData';

const MissingItemsScreen = props => {

    const [description, setDescription] = useState('');
    const [brand, setBrand] = useState('');
    const [requestedBy, setRequestedBy] = useState('');

    const ClearData = () => {
        setDescription('');
        setBrand('');
        setRequestedBy('');
    };

    return (
        <View style={styles.container}>

            <ScreenHeader
                description='Send Missing Item requested by clients to Stock Controller.'
                imageSource={require('../assets/icons/missingItems.png')}
            />

            <TextBoxField
                title='Description'
                value={description}
                multiline={true}
                maxLength={500}
                numberOfLines={5}
                onChangeText={(value) => {setDescription(value)}} />

            <TextBoxField
                title='Brand'
                value={brand}
                multiline={false}
                maxLength={100}
                numberOfLines={1}
                onChangeText={(value) => {setBrand(value)}} />

            <TextBoxField
                title='Requested By'
                value={requestedBy}
                multiline={false}
                maxLength={100}
                numberOfLines={1}
                onChangeText={(value) => {setRequestedBy(value)}} />

            <View style={styles.buttonsContainer}>

                <MainButton title='Save Report' onPress={() => {
                    if (description == '') {
                        alert('Description is Mandatory');
                    }
                    else {
                        MissingItemsData.SaveMissingItem(description, brand, requestedBy);
                        ClearData();
                    }
                }} />

                <MainButton title='Clear Report' onPress={() => {
                    ClearData();
                }} />

            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.bgColor,
        height: '100%',
        padding: 10,
    },
    buttonsContainer: {
        alignItems: Sizing.isTablet ? 'flex-end' : 'center',
        justifyContent: 'center',
        width: '100%',
        padding: 10,
    },
});

export default MissingItemsScreen;
