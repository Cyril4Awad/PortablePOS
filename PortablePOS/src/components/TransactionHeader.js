import React from 'react';
import { useState, useEffect } from 'react';
import Sizing from '../variables/Sizing';
import Colors from '../variables/Colors';
import { FontSize } from '../variables/ResponsiveFont';
import { getCurrentDate, getLoginUserCodeName } from '../variables/LoginInfo';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity } from 'react-native';
import PosOperationsData from '../data/PosOperationsData';

const TransactionHeader = props => {
    
    const [orderVatTypeDesc, setOrderVatTypeDesc] = useState('');
    const [workingDay, setWorkingDay] = useState('Loading...');

    useEffect(() => {
 
        PosOperationsData.GetWorkingDay({setWorkingDay})

        return(() => {
            //Cleanup
            setWorkingDay('')
        })
    
      }, []);

    useEffect(() => {
 
        if (props.orderVatType == 'N') {
            setOrderVatTypeDesc('Not Taxable');
        }
        else if (props.orderVatType == 'T') {
            setOrderVatTypeDesc('Taxable');
        }
        else if (props.orderVatType == 'A') {
            setOrderVatTypeDesc('Add-On');
        }
        else {
            setOrderVatTypeDesc('');
        }

        return(() => {
            //Cleanup
            
        })
    
      }, [props.orderVatType]);

    return (
        <View style={styles.container}>

            <View style={styles.headerContainer}>

                <View style={styles.fieldContainer}>

                    <View style={styles.fieldTitleContainer}>
                        <Text style={styles.fieldTitleText}>Number</Text>
                    </View>

                    <View style={styles.fieldDataContainer}>
                        <Text style={styles.fieldDataText}>{props.orderNumber}</Text>
                    </View>

                </View>

                <View style={styles.fieldContainer}>

                    <View style={styles.fieldTitleContainer}>
                        <Text style={styles.fieldTitleText}>Date</Text>
                    </View>

                    <View style={styles.fieldDataContainer}>
                        <Text style={styles.fieldDataText}>{workingDay}</Text>
                    </View>

                </View>

            </View>

            <View style={styles.headerContainer}>

                <View style={styles.fieldContainer}>

                    <View style={styles.fieldTitleContainer}>
                        <Text style={styles.fieldTitleText}>Sales Man</Text>
                    </View>

                    <View style={styles.fieldDataContainer}>
                        <Text style={styles.fieldDataText}>{getLoginUserCodeName()}</Text>
                    </View>

                </View>

                <View style={styles.fieldContainer}>

                    <TouchableOpacity style={styles.fieldDataContainer} onPress={() => props.onCurrencyClick()}>
                        <Text style={styles.fieldDataText}>
                            <Text style={{color: Colors.brandGreen, fontWeight: 'bold'}}>{props.orderCurrency} </Text>
                            {orderVatTypeDesc}
                        </Text>
                    </TouchableOpacity>

                    {typeof props.onHeaderRemarkPress === 'function' &&
                        <TouchableOpacity
                            style={styles.headerRemarkButton}
                            onPress={props.onHeaderRemarkPress}>
                            <Text style={[
                                styles.headerRemarkButtonText,
                                {
                                    color: props.headerRemarkColor || Colors.brandBlue,
                                    fontWeight: props.hasHeaderRemark ? 'bold' : 'normal',
                                }]}>
                                Remark
                            </Text>
                        </TouchableOpacity>
                    }

                </View>

            </View>


            {/* <View style={styles.fieldContainer}>

                <View style={styles.fieldTitleContainer}>
                    <Text style={styles.fieldTitleText}>H. Remark</Text>
                </View>

                <View style={styles.fieldDataContainer}>
                    <TextInput
                        style={[styles.fieldDataText, { width: Sizing.windowsWidth - 95 }]}
                        value={props.orderHeaderRemark}
                        onChangeText={props.setOrderHeaderRemark}
                        maxLength={500} />
                </View>

            </View> */}

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        elevation: 15,
        marginBottom: 10,
        width: '100%',
        height: 50,
        backgroundColor: Colors.headerBackColor,
        justifyContent: 'space-around',
    },
    headerContainer: {
        //backgroundColor: 'black',
        width: '100%',
        height: 35,
        flexDirection: 'row',
        justifyContent: 'space-between'
    },
    fieldContainer: {
        //backgroundColor: 'red',
        flexDirection: 'row',
        // /width: '100%',
        height: 35,
    },
    fieldTitleContainer: {
        //backgroundColor: 'yellow',
        width: 70,
        paddingLeft: 5,
        alignItems: 'flex-start',
        justifyContent: 'center',
    },
    fieldTitleText: {
        fontWeight: 'bold',
        fontSize: FontSize(15),
        color: Colors.brandBlue,
    },
    fieldDataContainer: {
        //backgroundColor: 'green',
        alignItems: 'flex-start',
        justifyContent: 'center',
        paddingLeft: 5,
        paddingRight: 5,
    },
    fieldDataText: {
        fontSize: FontSize(15),
        color: 'black',
    },
    headerRemarkButton: {
        alignItems: 'center',
        justifyContent: 'center',
        paddingLeft: 8,
        paddingRight: 5,
    },
    headerRemarkButtonText: {
        fontSize: FontSize(13),
    },
})

export default TransactionHeader;
