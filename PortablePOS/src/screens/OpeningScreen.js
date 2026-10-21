import React from 'react';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import DataField from '../components/controls/DataField';
import MainButton from '../components/controls/MainButton';
import ScreenHeader from '../components/ScreenHeader';
import { View, Text, ScrollView, TextInput, StyleSheet, TouchableOpacity, Image } from 'react-native';

const OpeningScreen = props => {

    return (
        <View style={styles.container}>

            <ScreenHeader 
                description='Open a new Day, then Open a new Session to start issuing Transactions'
                imageSource={require('../assets/icons/opening.png')}
            />
            
            {/* <DataField title='Cash Register' data='JJ - Jad Tablet' /> */}
            {/* <DataField title='Session #' data='12' /> */}
            <DataField title='Last Closing Date' data='31/05/2024' toolTip='dd/MM/yyyy' />
            <DataField title='Last Working Date' data='03/06/2024' toolTip='dd/MM/yyyy' />

            <View style={styles.buttonsContainer}>
                {/* <MainButton title='Open Day' onPress={() => { alert('Day Opened') }} /> */}
                <MainButton title='Open Cashier Session' onPress={() => { alert('Cannot Open a new Session Now') }} />
                <MainButton title='Close Cashier Session' onPress={() => { alert('Cannot Close the Session Now') }} />
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

export default OpeningScreen;
