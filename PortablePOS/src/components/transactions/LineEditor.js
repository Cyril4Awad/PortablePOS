import React from 'react';
import { useState, useEffect } from 'react';
import Sizing from '../../variables/Sizing';
import Colors from '../../variables/Colors';
import { FontSize } from '../../variables/ResponsiveFont';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity, ScrollView } from 'react-native';
import StockData from '../../data/StockData';

const LineEditor = props => {

    const [itemImage, setItemImage] = useState('');

    useEffect(() => {
    
        if (Sizing.isTablet){
            StockData.GetItemImage(
            props.selectedItemRecID,
            { setItemImage },
        );
        }

        return(() => {
            //Cleanup
            setItemImage('');
        })
        
      }, []);    

    const ImageSection = () => {
        return(
            <View style={styles.itemImageContainer}>
                <Image
                    source={{ uri: "data:image/png;base64," + itemImage }}
                    style={styles.itemImage}
                    resizeMode='contain'
                />
            </View>
        )
    };

    return (
        <View style={styles.container}>

            <View style={styles.header}>
                <Text style={styles.itemReferenceText}>{props.selectedItemReference}</Text>
                <Text style={styles.itemDescriptionText} numberOfLines={2}>{props.selectedItemDescription}</Text>
            </View>

            <View style={styles.fieldSectionSplit}>

                <View style={{width: Sizing.isTablet ? '50%' : '100%'}}>

                    <View style={styles.fieldSection}>

                        <View style={styles.fieldSectionTitle}>
                            <Text style={styles.fieldSectionTitleText}>Qty</Text>
                        </View>

                        <View style={styles.fieldSectionData}>
                            <TextInput
                                style={styles.fieldSectionDataText}
                                value={props.selectedItemQty.toString()}
                                onChangeText={props.setSelectedItemQty}
                                keyboardType='numeric'
                                autoFocus={true}
                                blurOnSubmit={true}
                                onBlur={() => {

                                    props.refreshPrices(props.selectedItemQty);

                                }}
                            />
                        </View>

                    </View>

                    <View style={styles.fieldSection}>

                        <View style={styles.fieldSectionTitle}>
                            <Text style={styles.fieldSectionTitleText}>U. Price {props.transactionCurrency}</Text>
                        </View>

                        <View style={styles.fieldSectionData}>
                            <TextInput
                                style={styles.fieldSectionDataText}
                                value={props.selectedItemPrice.toString()}
                                onChangeText={props.setSelectedItemPrice}
                                editable={false}  //pending
                                keyboardType='numeric'
                            />

                        </View>

                    </View>

                    <View style={styles.fieldSection}>

                        <View style={styles.fieldSectionTitle}>
                            <Text style={styles.fieldSectionTitleText}>Discount</Text>
                        </View>

                        <View style={styles.fieldSectionData}>
                            <TextInput
                                style={styles.fieldSectionDataText}
                                value={props.selectedItemDiscount.toString()}
                                onChangeText={props.setSelectedItemDiscount}
                                editable={false}  //pending
                                keyboardType='numeric'
                            />
                        </View>

                    </View>

                    <View style={styles.fieldSection}>

                        <View style={styles.fieldSectionTitle}>
                            <Text style={styles.fieldSectionTitleText}>Total {props.transactionCurrency}</Text>
                        </View>

                        <View style={styles.fieldSectionData}>
                            <Text style={styles.fieldSectionDataText}>{Intl.NumberFormat("ar-de").format(props.selectedItemTotal)}</Text>
                        </View>

                    </View>

                </View>

                <View style={{width : Sizing.isTablet ? '50%' : '100%'}}>

                    <ImageSection />

                </View>

            </View>

            <View style={styles.fieldSection}>

                <TextInput
                    style={[styles.fieldSectionDataText, { height: '60%' }]}
                    value={props.selectedItemRemark.toString()}
                    onChangeText={props.setSelectedItemRemark}
                    placeholder='Remarks'
                    editable={true}  //pending
                />

            </View>

            <View style={styles.footer}>
                
                <TouchableOpacity style={styles.button} onPress={() => {
                    props.setLineEditorVisible(false);
                    }}>
                    <Text style={styles.buttonText}>Cancel</Text>
                </TouchableOpacity>

                <TouchableOpacity style={styles.button} onPress={() => {
                    if (Number(props.selectedItemPrice) === 0) {
                        alert('This item has no price');
                    }
                    else {
                        props.setLineEditorVisible(false);
                        props.onSubmit();
                    }
                    }}>
                    <Text style={styles.buttonText}>OK</Text>
                </TouchableOpacity>

            </View>

        </View>
    );
};

const modalHeight = (Sizing.windowsHeight * 0.55) - 10; //10 for border width (5+5)
const modalHeaderHeight = modalHeight * 0.20;
const modalFooterHeight = modalHeight * 0.10;
const modalDetailsHeight = modalHeight * 0.70;

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
        height: modalHeaderHeight,
        width: '100%',
        alignItems: 'flex-start',
        justifyContent: 'center',
        paddingLeft: 20,
        paddingRight: 20,
    },
    itemReferenceText: {
        color: 'black',
        width: '100%',
        //fontWeight: 'bold',
        fontSize: FontSize(18),
    },
    itemDescriptionText: {
        color: Colors.brandBlue,
        fontWeight: 'bold',
        width: '100%',
        fontSize: FontSize(20),
    },
    fieldSectionSplit: {
        width: '100%',
        height: (modalDetailsHeight / 5) * 4,
        //backgroundColor: 'red',
        justifyContent: 'space-between',
        flexDirection: Sizing.isTablet? 'row' : 'column',
    },
    itemImageContainer: {
        //backgroundColor: 'red',
        width: '100%',
        height: '100%',
        padding: 20,
    },
    itemImage: {
        width: '100%',
        height: '100%',
        resizeMode: 'contain',
        //borderRadius: 20,
        overflow: 'hidden',
    },
    fieldSection: {
        //backgroundColor: 'yellow',
        width: '100%',
        height: modalDetailsHeight/ 5,
        alignItems: 'center',
        justifyContent: 'center',
        flexDirection: 'row'
    },
    fieldSectionTitle: {
        width: '40%',
        //backgroundColor: 'yellow',
        alignItems: 'flex-end',
        justifyContent: 'center',
        paddingRight: 10,
    },
    fieldSectionTitleText: {
        fontSize: FontSize(17),
        fontWeight: 'bold',
        color: 'black',
    },
    fieldSectionData: {
        width: '60%',
        //backgroundColor: 'blue',
        alignItems: 'flex-start',
        justifyContent: 'center',
        paddingLeft: 10,
        paddingRight: 10,
        height: 35,
    },
    fieldSectionDataText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
        borderWidth: 1,
        width: '85%' ,
        height: '100%',
        borderRadius: 4,
        alignItems: 'center',
        justifyContent: 'center',
        textAlign: 'left',
        textAlignVertical: 'center',
        paddingLeft: 5,
    },
    footer: {
        backgroundColor: 'gainsboro',
        height: modalFooterHeight,
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

export default LineEditor;
