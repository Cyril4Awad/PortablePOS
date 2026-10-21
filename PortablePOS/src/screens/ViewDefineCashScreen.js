import React, { useState, useEffect } from 'react';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import ScreenHeader from '../components/ScreenHeader';
import { View, Text, ScrollView, StyleSheet, Button, Alert } from 'react-native';
import DataField from '../components/controls/DataField';
import MainButton from '../components/controls/MainButton';
import TextBoxNumericField from '../components/controls/TextBoxNumericField';
import PosOperationsData from '../data/PosOperationsData';
// import { useNavigation, useFocusEffect } from '@react-navigation/native';

const ViewDefineCashScreen = props => {

    const [cashLC, setCashLC] = useState(0);
    const [cashCC, setCashCC] = useState(0);

    // const [preventGoingBack, setPreventGoingBack] = useState(false);
    // const navigation = useNavigation();

    // useFocusEffect(
    //     React.useCallback(() => {
    //       const onBeforeRemove = (e) => {

    //         console.log('here we go')
    //         console.log(cashLC)
      
    //         if (cashLC !== 0) {
    //           // If there's no changes in the form, we don't need to show a prompt
    //           return;
    //         }
    
    //         // Prevent default behavior of leaving the screen
    //         e.preventDefault();
    
    //         // Show confirmation alert
    //         Alert.alert(
    //           'Discard changes?',
    //           'You have unsaved changes. Are you sure you want to discard them and leave the screen?',
    //           [
    //             { text: 'Cancel', style: 'cancel', onPress: () => {} },
    //             {
    //               text: 'Discard',
    //               style: 'destructive',
    //               // Go back if the user confirms
    //               onPress: () => navigation.dispatch(e.data.action),
    //             },
    //           ]
    //         );
    //       };
    
    //       navigation.addListener('beforeRemove', onBeforeRemove);
    
    //       return () => {
    //         navigation.removeListener('beforeRemove', onBeforeRemove);
    //       };
    //     }, [navigation])
    //   );

    const clearData = () => {
        setCashLC(0);
        setCashCC(0);
    }

    return (
        <View style={styles.container}>

            <ScreenHeader
                description='Define Cash Counted'
                imageSource={require('../assets/icons/viewDefineCash.png')}
            />

            <TextBoxNumericField
                title='CASH LBP'
                value={cashLC}
                multiline={true}
                maxLength={30}
                numberOfLines={5}
                onChangeText={(value) => {setCashLC(value)}} />

            <TextBoxNumericField
                title='CASH USD'
                value={cashCC}
                multiline={true}
                maxLength={30}
                numberOfLines={1}
                onChangeText={(value) => {setCashCC(value)}} />

            <View style={styles.buttonsContainer}>

                <MainButton title='Define Cash Counted' onPress={() => {
                    PosOperationsData.SaveCashCount(cashLC, cashCC);
                    clearData()
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

export default ViewDefineCashScreen;
