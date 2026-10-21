import React from 'react';
import Sizing from '../../variables/Sizing';
import Colors from '../../variables/Colors';
import { FontSize } from '../../variables/ResponsiveFont';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity, Alert } from 'react-native';

const Line = props => {

    const Delete = () => {

        if (props.header == false) {

            if (props.itemIsPromotion === 'N') {
                return (
                    <TouchableOpacity style={styles.lineDeleteButton} onPress={() => { 
                        Alert.alert('Delete Line', 'Do you want to delete line ?', [
                            {
                                text: 'No',
                                onPress: () => { },
                                style: 'cancel',
                            },
                            {
                                text: 'Yes', onPress: () => {
                                    props.DeleteLine()
                                }
                            },
                        ]);
                        
                         }}>
                        <Image
                            source={require('../../assets/icons/deleteLine.png')}
                            style={styles.lineDeleteButtonImage}
                            resizeMode="contain"
                        />
                    </TouchableOpacity>
                )
            }
            else {
                return (
                    <View style={styles.lineDeleteButton} onPress={() => { props.DeleteLine() }}>
                        <Image
                            source={require('../../assets/icons/promotion.png')}
                            style={styles.lineDeleteButtonImage}
                            resizeMode="contain"
                        />
                    </View>
                )
            }

        }
        else {

            if (props.onHeaderTogglePress) {
                return (
                    <TouchableOpacity
                        style={styles.headerToggleButton}
                        onPress={props.onHeaderTogglePress}>
                        <Text style={styles.headerToggleText}>
                            {props.headerToggleCollapsed ? '▼' : '▲'}
                        </Text>
                    </TouchableOpacity>
                );
            }

            return (
                <View></View>
            )
        };

    };

    return (
        <View style={styles.container}>

                <View style={[styles.lineContainer,{backgroundColor: props.header ? 'rgba(0,0,0,0.3)' : 'white'}]}>

                    <View style={styles.lineDataContainer}>

                        <View style={styles.lineDescriptionContainer}>
                            <Text style={[styles.descriptionText, {color: props.header ? 'white' : 'black'}]} numberOfLines={1}>{props.itemReference} - {props.itemDescription}</Text>
                        </View>

                        <View style={styles.lineInfoContainer}>

                            <View style={styles.qtyContainer}>
                                <Text style={[styles.infoText, {color: props.header ? 'white' : 'black'}]}>{isNaN(props.itemQty) ? props.itemQty : Intl.NumberFormat("ar-de").format(props.itemQty)}</Text>
                            </View>

                            <View style={styles.priceContainer}>
                                <Text style={[styles.infoText, {color: props.header ? 'white' : 'black'}]}>{isNaN(props.itemPrice) ? props.itemPrice : Intl.NumberFormat("ar-de").format(props.itemPrice)}</Text>
                            </View>

                            <View style={styles.discountContainer}>
                                <Text style={[styles.infoText, {color: props.header ? 'white' : 'black'}]}>{isNaN(props.itemDiscount) ? props.itemDiscount : Intl.NumberFormat("ar-de").format(props.itemDiscount)}</Text>
                            </View>

                            <View style={styles.totalContainer}>
                                <Text style={[styles.infoText, {color: props.header ? 'white' : 'black'}]}>{isNaN(props.itemTotal) ? props.itemTotal : props.itemTotal === 0 ? 'Free' : Intl.NumberFormat("ar-de").format(props.itemTotal)}</Text>
                            </View>

                        </View>

                    </View>

                    <View style={styles.lineButtonsContainer}>

                        <Delete />

                    </View>

                </View>

            </View>
    );
};

const styles = StyleSheet.create({
    container: {
        //backgroundColor: 'red'
    },  
    lineContainer: {
        backgroundColor:'yellow',
        width: '100%',
        height: Sizing.isTablet ? 40 : 45,
        flexDirection: 'row',
        justifyContent: 'space-evenly',
        borderWidth: 1,
        borderColor: 'silver',
    },
    lineDataContainer: {
        width: Sizing.isTablet ? '95%' : '90%',
        //backgroundColor: 'silver',
        flexDirection: Sizing.isTablet ? 'row' : 'column',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 5,
    },
    lineDescriptionContainer: {
        //backgroundColor: 'lightgreen',
        width: Sizing.isTablet ? '50%' : '100%',
        alignItems: 'flex-start',
        justifyContent: 'center',
    },
    descriptionText: {
        color: 'black',
        fontSize: FontSize(16),
        fontWeight: 'bold',
    },
    lineInfoContainer: {
        //backgroundColor: 'blue',
        width: Sizing.isTablet ? '50%' : '100%',
        alignItems: 'center',
        justifyContent: 'space-around',
        flexDirection: 'row',
    },    
    qtyContainer: {
        //backgroundColor: 'red',
        alignItems: 'center',
        justifyContent: 'center',
        width: 60,
    },
    priceContainer: {
        //backgroundColor: 'blue',
        alignItems: 'center',
        justifyContent: 'center',
        width: 100,
    },
    discountContainer: {
        //backgroundColor: 'green',
        alignItems: 'center',
        justifyContent: 'center',
        width: 50,
    },
    totalContainer: {
        //backgroundColor: 'yellow',
        alignItems: 'center',
        justifyContent: 'center',
        width: 100,
    },
    infoText: {
        fontSize: FontSize(17),
        color: 'black',
    },
    lineButtonsContainer: {
        width: Sizing.isTablet ? '5%' : '10%',
        alignItems: Sizing.isTablet ? 'flex-end' : 'center' ,
        justifyContent: 'center',
        //backgroundColor: 'red',
        padding: 1,
        overflow: 'hidden',
    },
    lineDeleteButton: {
        width: 45,
        height: 45,
        alignItems: 'center',
        justifyContent: 'center',
    },
    lineDeleteButtonImage: {
        //backgroundColor: 'yellow',
        width: '70%',
        height: '70%',
        overflow: 'hidden',
        resizeMode: 'contain',
    },
    headerToggleButton: {
        width: 22,
        height: 22,
        alignItems: 'center',
        justifyContent: 'center',
    },
    headerToggleText: {
        color: 'white',
        fontSize: FontSize(11),
        fontWeight: 'bold',
        lineHeight: FontSize(12),
    },
});

export default Line;
