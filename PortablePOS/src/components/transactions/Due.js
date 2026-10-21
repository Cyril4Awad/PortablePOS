import React, { useEffect, useState, useRef } from 'react';
import Sizing from '../../variables/Sizing';
import Colors from '../../variables/Colors';
import { FontSize } from '../../variables/ResponsiveFont';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity } from 'react-native';

const Due = props => {

    const amountInputControl = useRef();

    //const [amountToPay, setAmountToPay] = useState(props.amountToPay);
    const [checked, setChecked] = useState(props.checked);

    // useEffect(() => {

    //     setAmountToPay(props.amountToPay)
    
    //   }, [props.amountToPay]);

    const RemarkSection = () => {

        if (props.dueRemark !== '') {
            return (
                <View style={styles.remarkContainer}>

                    <Text style={styles.remarkText}>{props.dueRemark}</Text>

                </View>
            )
        }
        else {
            return (<></>)
        }

    };

    return (

        <View style={styles.container}>

            <View style={styles.dueContainer}>

                <View style={styles.dataContainer}>

                    <View style={styles.invoiceNumberDateContainer}>

                        <Text style={styles.invoiceNumberDateText}>{props.invoiceNumber}</Text>
                        <Text style={styles.invoiceNumberDateText}>{props.invoiceDate}</Text>

                    </View>

                    <View style={styles.locationContainer}>

                        <Text style={styles.locationText}>{props.locationCode} - {props.locationDescription}</Text>

                    </View>

                    {/* <View style={styles.invoiceTotalContainer}>

                        <Text style={styles.invoiceTotalCaption}>Invoice Total : </Text>
                        <Text style={styles.invoiceTotalText}>{props.invoiceNetTotal} {props.invoiceCurrency}</Text>

                    </View> */}

                    <View style={styles.dueInfoCaptionContainer}>

                        <Text style={styles.dueInfoCaptionText}>Value Date</Text>
                        <Text style={styles.dueInfoCaptionText}>Due Value</Text>
                        <Text style={styles.dueInfoCaptionText}>Pending Value</Text>

                    </View>

                    <View style={styles.dueInfoContainer}>

                        <Text style={styles.dueInfoText}>{props.dueValueDateDisplay}</Text>
                        <Text style={styles.dueInfoText}>{Intl.NumberFormat("ar-de").format(props.initDueValue)} {props.dueCurrency}</Text>
                        <Text style={styles.dueInfoText}>{Intl.NumberFormat("ar-de").format(props.dueValue)} {props.dueCurrency}</Text>

                    </View>

                    <RemarkSection />

                </View>

                <View style={styles.checkBoxAndAmountToPayContainer}>

                    <View style={styles.amountToPayContainer}>

                        <Text style={styles.amountToPayCaption}> Amount To Pay {props.dueCurrency} </Text>

                        <Text style={styles.amountToPayText}>{Intl.NumberFormat("ar-de").format(props.amountToPay.toString())}</Text>

                    </View>

                    {/* <TouchableOpacity style={styles.checkBoxButton} onPress={() => {

                        if (checked) {
                            props.setAmountToPay(0);
                        }
                        else {
                            props.setAmountToPay(props.dueValue);
                        };

                        setChecked(!checked);

                    }}>

                        <Image
                            source={checked ? require('../../assets/icons/checked.png') : require('../../assets/icons/unchecked.png')}
                            style={styles.checkBoxButtonImage}
                            resizeMode='contain'
                        />

                    </TouchableOpacity> */}

                </View>

            </View>

        </View>

    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: 'white',
        width: '100%',
        flexDirection: 'row',
        justifyContent: 'space-evenly',
        marginBottom: 10
    },
    dueContainer: {
        // /flex: 1,
        backgroundColor: 'white',
        borderColor: Colors.brandBlue,
        borderWidth: 1,
        overflow: 'hidden',
        borderRadius: 10,
        //height: 200,
    },
    dataContainer: {
        backgroundColor: 'white',
        borderColor: 'silver',
        borderBottomWidth: 2,
        paddingBottom: 3
    },
    invoiceNumberDateContainer: {
        backgroundColor: Colors.brandBlue,
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-around',
        //width: '100%',
        height: 30,
    },
    invoiceNumberDateText: {
        color: 'white',
        //fontWeight: 'bold',
        fontSize: FontSize(22),
    },
    locationContainer: {
        backgroundColor: 'white',
        alignItems: 'center',
        justifyContent: 'center',
        //width: '100%',
        height: 25,
    },
    locationText: {
        color: Colors.brandGreen,
        fontSize: FontSize(18),
    },
    invoiceTotalContainer: {
        backgroundColor: 'white',
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
        //width: '100%',
        height: 20,
        paddingLeft: 7,
        paddingRight: 7
    },
    invoiceTotalCaption: {
        color: 'dimgray',
        fontSize: FontSize(18),
    },
    invoiceTotalText: {
        color: 'black',
        fontSize: FontSize(18),
    },
    dueInfoCaptionContainer: {
        backgroundColor: 'silver',
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-around',
        //width: '100%',
        height: 15,
    },
    dueInfoCaptionText: {
        color: 'white',
        fontWeight: 'bold',
        fontSize: FontSize(12),
        textAlign: 'center',
        width: '30%',
    },
    dueInfoContainer: {
        backgroundColor: 'white',
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-around',
        //width: '100%',
        height: 35,
    },
    dueInfoText: {
        color: Colors.brandBlue,
        fontWeight: 'bold',
        fontSize: FontSize(18),
        textAlign: 'center',
        width: '30%',
    },
    remarkContainer: {
        backgroundColor: 'white',
        alignItems: 'flex-start',
        justifyContent: 'center',
        paddingLeft: 7,
        paddingRight: 7,
        paddingTop: 2,
        borderTopWidth: 2,
        borderColor: 'silver',
    },
    remarkText: {
        color: 'red',
        fontStyle: 'italic',
        fontSize: FontSize(15),
    },
    checkBoxAndAmountToPayContainer: {
        //backgroundColor: 'silver',
        width: '100%',
        height: 50,
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'flex-end',
    },
    amountToPayContainer: {
        flex: 1,
        //backgroundColor: 'yellow',
        height: '100%',
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-around',
        paddingLeft: 7,
        paddingRight: 7,
    },
    amountToPayCaption: {
        color: 'dimgray',
        fontSize: FontSize(17),
    },
    amountToPayText: {
        color: 'black',
        textAlign: 'center',
        textAlignVertical: 'center',
        backgroundColor: 'white',
        fontSize: FontSize(20),
        width: '50%',
        height: '75%',
        borderColor: 'dimgray',
        borderWidth: 1,
        borderRadius: 5,
    },
    checkBoxButton: {
        //backgroundColor: 'red',
        width: 50,
        height: '100%',
        alignItems: 'center',
        justifyContent: 'center',
    },
    checkBoxButtonImage: {
        //alignItems: 'center',
        //marginHorizontal: 10,
        resizeMode: 'contain',
        width: '100%',
        height: '100%',
        //backgroundColor:'red',
    },
});

export default Due;
