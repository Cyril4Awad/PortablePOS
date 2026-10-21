import React from 'react';
import { useState, useEffect } from 'react';
import Sizing from '../../variables/Sizing';
import Colors from '../../variables/Colors';
import { FontSize } from '../../variables/ResponsiveFont';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity, ScrollView, FlatList } from 'react-native';
import StockData from '../../data/StockData';

const Settlement = props => {

    const [pendingAmount, setPendingAmount] = useState(props.totalLC);

    return (
        <View style={styles.container}>

            <View style={styles.header}>
                <Text style={styles.headerTitleText}>Payment ( Rate : {Intl.NumberFormat("ar-de").format(props.rateLCCC)} )</Text>
            </View>

            <View style={[styles.totalsContainer, {alignItems:'center', justifyContent: 'center'}]}>

                <View style={styles.totalsSplit}>
                        
                    <Text style={styles.totalCaption}>Amount To Pay</Text>
                    <Text style={styles.totalText}>{Intl.NumberFormat("ar-de").format(props.amountToPay)}</Text>

                </View>

            </View>

            <View style={styles.paymentModesContainer}>

                <View style={styles.paymentButtonContainer}>
                    <TouchableOpacity style={styles.paymentButton} onPress={() => {props.setSettlementVisible(false);
                                                                                    props.onSubmit()}}>
                        <Text style={styles.paymentButtonText}>Cash</Text>
                    </TouchableOpacity>
                </View>

                <View style={styles.paymentButtonContainer}>
                    <TouchableOpacity style={styles.paymentButton} disabled={true}>
                        <Text style={styles.paymentButtonText}>Visa</Text>
                    </TouchableOpacity>
                </View>

                <View style={styles.paymentButtonContainer}>
                    <TouchableOpacity style={styles.paymentButton} disabled={true}>
                        <Text style={styles.paymentButtonText}>On Account</Text>
                    </TouchableOpacity>
                </View>

            </View>

            <View style={styles.footer}>

                <TouchableOpacity style={styles.button} onPress={() => {
                    props.setSettlementVisible(false);
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
    totalsContainer: {
        backgroundColor: Colors.brandBlue,
        width :'100%',
        flexDirection: 'row',
  },
    totalsSplit: {
        //backgroundColor: 'red',
        width: '50%',
        height: 50,
        alignItems: 'center',
        justifyContent: 'center',
    },
    totalCaption: {
        color: 'gainsboro',
        fontSize: FontSize(20),
    },
    totalText: {
        color: 'white',
        fontSize: FontSize(22),
    },
    paymentModesContainer: {
        //backgroundColor: 'yellow',
        alignItems: 'center',
        justifyContent: 'space-evenly',
        width: '100%',
        flex: 1,
    },
    paymentButtonContainer: {
        backgroundColor: Colors.brandBlue,
        width: '50%',
        height: '20%',
        borderRadius: 10,
        alignItems: 'center',
        justifyContent: 'center',
        elevation: 10,
        overflow: 'hidden'
    },
    paymentButton: {
        backgroundColor: Colors.brandGreen,
        width: '100%',
        height: '100%',
        alignItems: 'center',
        justifyContent: 'center',
        elevation: 10,
    },
    paymentButtonText: {
        color: 'white',
        fontSize: FontSize(25)
    },
    paymentModeContainer: {
        backgroundColor: 'lightsteelblue',
        width: '100%',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 10,
    },
    paidAmountCaptionText: {
        fontSize: FontSize(17),
        Color: 'black',
        paddingBottom: 5,
    },
    paidAmountText: {
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
        paddingRight: 15
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
    },
});

export default Settlement;
