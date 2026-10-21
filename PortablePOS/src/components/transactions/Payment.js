import React from 'react';
import { useState, useEffect } from 'react';
import Sizing from '../../variables/Sizing';
import Colors from '../../variables/Colors';
import { FontSize } from '../../variables/ResponsiveFont';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity, ScrollView, FlatList } from 'react-native';
import StockData from '../../data/StockData';

const Payment = props => {

    const [totalAmountLC, setTotalAmountLC] = useState(props.totalLC);
    const [pendingAmountLC, setPendingAmountLC] = useState(props.totalLC);

    useEffect(() => {

        Calculate()
    
      }, [props.paidAmountLC, props.paidAmountCC]);


    const Calculate = () => {
        
        setPendingAmountLC(Number(totalAmountLC) - (Number(props.paidAmountLC) + Number(Number(props.paidAmountCC) * Number(props.rateLCCC))))

    }

    return (
        <View style={styles.container}>

            <View style={styles.header}>
                <Text style={styles.headerTitleText}>Payment ( Rate : {Intl.NumberFormat("ar-de").format(props.rateLCCC)} )</Text>
            </View>

            <View style={styles.totalsContainer}>

                <View style={styles.totalsSplit}>
                        
                    <Text style={styles.totalCaption}>Amount To Pay {props.localCurrency}</Text>
                    <Text style={styles.totalText}>{Intl.NumberFormat("ar-de").format(totalAmountLC)}</Text>

                </View>

                <View style={styles.totalsSplit}>

                    <Text style={styles.totalCaption}>Amount To Pay {props.controlCurrency}</Text>
                    <Text style={styles.totalText}>{Intl.NumberFormat("ar-de").format(props.totalCC)}</Text>

                </View>


                <View style={styles.totalsSplit}>

                    <Text style={styles.totalCaption}>Pending {props.localCurrency}</Text>
                    <Text style={styles.totalText}>{Intl.NumberFormat("ar-de").format(pendingAmountLC)}</Text>

                </View>

            </View>

            <View style={[styles.totalsContainer, {alignItems:'center', justifyContent: 'center', backgroundColor: pendingAmountLC === 0? Colors.brandBlue: Colors.brandGreen}]}>

                <View style={styles.totalsSplit}>

                    <Text style={styles.totalCaption}>Pending {props.localCurrency}</Text>
                    <Text style={styles.totalText}>{Intl.NumberFormat("ar-de").format(pendingAmountLC)}</Text>

                </View>

            </View>

            <View style={styles.paymentModesContainer}>

                <View style={styles.paymentModeContainer}>

                    <Text style={styles.paidAmountCaptionText}>Cash {props.localCurrency}</Text>

                    <TextInput
                        value={props.paidAmountLC.toString()}
                        style={styles.paidAmountText}
                        onChangeText={props.setPaidAmountLC}
                        keyboardType='numeric'
                        onBlur={() => {

                            if (isNaN(Number(props.paidAmountLC)) || props.paidAmountLC < 0) {
                                props.setPaidAmountLC(0)
                            }
                            else {
                                Calculate();
                            }

                        }}
                    />

                </View>

                <View style={styles.paymentModeContainer}>

                    <Text style={styles.paidAmountCaptionText}>Cash {props.controlCurrency}</Text>

                    <TextInput
                        value={props.paidAmountCC.toString()}
                        style={styles.paidAmountText}
                        onChangeText={props.setPaidAmountCC}
                        keyboardType='numeric'
                        onBlur={() => {

                            if (isNaN(Number(props.paidAmountCC)) || props.paidAmountCC < 0) {
                                props.setPaidAmountCC(0)
                            }
                            else {
                                Calculate();
                            }

                        }}
                    />

                </View>
                    
            </View>

            <View style={styles.footer}>

                <TouchableOpacity style={styles.button} onPress={() => {
                    props.setPaymentVisible(false);
                }}>
                    <Text style={styles.buttonText}>Cancel</Text>
                </TouchableOpacity>

                <TouchableOpacity style={styles.button} onPress={() => {

                    if (pendingAmountLC !== 0) {
                         alert('Receipt not settled yet')   
                    }
                    else {

                        props.setPaymentVisible(false);
                        props.onSubmit();

                    }

                }}>
                    <Text style={styles.buttonText}>Save</Text>
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
        justifyContent: 'center',
        width: '100%',
        flex: 1,
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
        justifyContent: 'space-evenly',
        flexDirection: 'row',
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

export default Payment;
