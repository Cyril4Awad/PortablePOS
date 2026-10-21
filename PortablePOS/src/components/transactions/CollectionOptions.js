import React from 'react';
import { useState, useEffect } from 'react';
import Sizing from '../../variables/Sizing';
import Colors from '../../variables/Colors';
import { FontSize } from '../../variables/ResponsiveFont';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity, ScrollView, FlatList } from 'react-native';
import StockData from '../../data/StockData';

const CollectionOptions = props => {

    const [amountToPayOp3, setAmountToPayOp3] = useState(0);

    const SettleAllDues = () => {
        try {

            props.duesList.forEach((due, index) => {

                if (due.dueCurrency == props.receiptCurrency) {

                props.duesList[index] = {
                    ...due,
                    amountToPay: due.dueValue
                };

            };

            });

            props.setDuesList(props.duesList);

        } catch (error) {
            console.log(error);
        }
    };

    const SettleAllDuesTillToday = () => {
        try {

            var todaysDate = new Date()

            props.duesList.forEach((due, index) => {

                if (due.dueCurrency == props.receiptCurrency) {
                
                var timestamp = parseInt(due.dueValueDate.match(/\d+/)[0]);

                var dueValueDate = new Date(timestamp);

                console.log(todaysDate);
                console.log(dueValueDate);

                if (dueValueDate <= todaysDate) {
                    props.duesList[index] = {
                        ...due,
                        amountToPay: due.dueValue
                    }
                }
                else {
                    props.duesList[index] = {
                        ...due,
                        amountToPay: 0
                    }
                }

            };

            });

            props.setDuesList(props.duesList);

        } catch (error) {
            console.log(error);
        }
    };

    const SettleAmount = () => {
        try {


            if (isNaN(Number(amountToPayOp3))) {
                alert('Please enter a valid amount')
            }
            else {

                if (amountToPayOp3 > props.totalDues) {
                    alert('The amount cannot be greater than total dues')
                }
                else {

                    var remaining = amountToPayOp3;

                    props.duesList.forEach((due, index) => {

                        if (due.dueCurrency == props.receiptCurrency) {

                            // 1000    750
                            if (remaining > due.dueValue) {

                                props.duesList[index] = {
                                    ...due,
                                    amountToPay: parseFloat(due.dueValue) //750
                                };

                                remaining = remaining - due.dueValue //250

                            }
                            // 250    750
                            else if (remaining <= due.dueValue) {

                                props.duesList[index] = {
                                    ...due,
                                    amountToPay: parseFloat(remaining)
                                };

                                remaining = 0

                            }

                        };

                    });

                    props.setDuesList(props.duesList);

                }
            }

        } catch (error) {
            console.log(error);
        }
    };

    const CollectionButton = cbprops => {
        return (
            <TouchableOpacity style={styles.optionButton} onPress={() => {
                cbprops.onPress();
                props.setCollectionOptionsVisible(false);
            }}>
                <Text style={styles.optionButtonText}>{cbprops.text}</Text>
            </TouchableOpacity>
        )
    }

    return (
        <View style={styles.container}>

            <View style={styles.header}>
                <Text style={styles.headerTitleText}>Collection Options</Text>
            </View>

            <View style={styles.optionsContainer}>

                <View style={styles.optionContainer}>

                    <View style={styles.optionTitleContainer}>
                        <Text style={styles.optionTitleText}>Option 1</Text>
                    </View>

                    <View style={styles.optionContentContainer}>

                        <CollectionButton text='Settle All Dues' onPress={() => { SettleAllDues() }} />

                    </View>

                </View>

                <View style={styles.optionContainer}>

                    <View style={styles.optionTitleContainer}>
                        <Text style={styles.optionTitleText}>Option 2</Text>
                    </View>

                    <View style={styles.optionContentContainer}>
                        <CollectionButton text='Settle All Dues Till Today' onPress={() => { SettleAllDuesTillToday() }} />
                    </View>

                </View>

                <View style={styles.optionContainer}>

                    <View style={styles.optionTitleContainer}>
                        <Text style={styles.optionTitleText}>Option 3</Text>
                    </View>

                    <View style={styles.optionContentContainer}>

                        <Text style={styles.amountToPayCaptionText}>Amount To Pay {props.receiptCurrency}</Text>

                        <TextInput
                            value={amountToPayOp3.toString()}//Intl.NumberFormat("ar-de").format(
                            style={styles.amountToPayText}
                            onChangeText={setAmountToPayOp3}
                            keyboardType='numeric' 
                        
                        />

                        <CollectionButton text='Settle Amount' onPress={() => { SettleAmount() }} />

                    </View>

                </View>

            </View>

            <View style={styles.footer}>

                <TouchableOpacity style={styles.button} onPress={() => {
                    props.setCollectionOptionsVisible(false);
                }}>
                    <Text style={styles.buttonText}>Cancel</Text>
                </TouchableOpacity>

            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: 'white',
        width: '100%',
        height: '100%',
        alignItems: 'center',
        justifyContent: 'space-between',
    },
    header: {
        backgroundColor: 'gainsboro',
        height: 50,
        width: '100%',
        alignItems: 'flex-start',
        justifyContent: 'center',
        paddingLeft: 20,
        paddingRight: 20,
    },
    headerTitleText: {
        color: Colors.brandBlue,
        width: '100%',
        fontWeight: 'bold',
        fontSize: FontSize(22),
    },
    optionsContainer: {
        //backgroundColor :'red',
        alignItems: 'center',
        // /justifyContent: 'space-around',
        width: '100%',
    },
    optionContainer: {
        //backgroundColor :'yellow',
        alignItems: 'flex-start',
        justifyContent: 'flex-start',
        // /borderBottomWidth: 5,
        //borderColor: 'gainsboro',
        width: '100%',
        //paddingTop: 10,
        //paddingBottom: 10,
    },
    optionTitleContainer: {
        //backgroundColor: 'red',
        width: '100%',
        height: 25,
        alignItems: 'flex-start',
        justifyContent: 'center',
        paddingLeft: 5,
        paddingRight: 5,
    },
    optionTitleText: {
        color: 'dimgray',
        fontSize: FontSize(15),
    },
    optionContentContainer: {
        backgroundColor: 'lightsteelblue',
        width: '100%',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 10,
    },
    optionButton: {
        //backgroundColor: Colors.brandBlue,
        //width: '100%',
        height: 25,
        alignItems: 'flex-start',
        justifyContent: 'center',
        borderRadius: 5
    },
    optionButtonText: {
        fontSize: FontSize(18),
        fontWeight: 'bold',
        textDecorationStyle: 'solid',
        textDecorationLine: 'underline',
        color: Colors.brandBlue,
    },
    amountToPayCaptionText: {
        fontSize: FontSize(17),
        Color: 'black',
        paddingBottom: 5,
    },
    amountToPayText: {
        color: 'black',
        textAlign: 'center',
        textAlignVertical: 'center',
        backgroundColor: 'white',
        fontSize: FontSize(20),
        width: 150,
        height: 40,
        borderColor: 'dimgray',
        borderWidth: 1,
        borderRadius: 5,
        marginBottom: 10,
    },
    footer: {
        backgroundColor: 'gainsboro',
        height: 50,
        width: '100%',
        alignItems: 'center',
        justifyContent: 'flex-end',
        flexDirection: 'row',
        paddingLeft: 7,
        paddingRight: 7,
    },
    button: {
        backgroundColor: Colors.brandBlue,
        width: 100,
        height: '70%',
        alignItems: 'center',
        justifyContent: 'center',
        borderRadius: 5
    },
    buttonText: {
        fontSize: FontSize(15),
        color: 'white',
    }
});

export default CollectionOptions;
