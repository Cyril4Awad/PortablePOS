import React from 'react';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import DataField from '../components/controls/DataField';
import MainButton from '../components/controls/MainButton';
import ScreenHeader from '../components/ScreenHeader';
import { View, Text, ScrollView, TextInput, StyleSheet, TouchableOpacity } from 'react-native';

const ClosingScreen = props => {

    return (
        <View style={styles.container}>

            <ScreenHeader
                description='Close cashier session, then Close the working day'
                imageSource={require('../assets/icons/closing.png')}
            />

            <DataField title='Cash Register' data='JJ - Jad Tablet' />
            <DataField title='Session #' data='12' />
            <DataField title='Working Date' data='03/06/2024' toolTip='dd/MM/yyyy' />

            <View style={styles.buttonsContainer}>
                <MainButton title='Close Session' onPress={() => { alert('Session Closed') }} />
                <MainButton title='Close Day' onPress={() => { alert('Day Closed') }} />
            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.bgColor,
        height: '100%',
    },
    buttonsContainer: {
        alignItems: Sizing.isTablet ? 'flex-end' : 'center',
        justifyContent: 'center',
        width: '100%',
        padding: 10,
    },
});

export default ClosingScreen;
