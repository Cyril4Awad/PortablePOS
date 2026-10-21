import React from 'react';
import { useState, useEffect } from 'react';
import Sizing from '../../../variables/Sizing';
import Colors from '../../../variables/Colors';
import { View, StyleSheet, Text, Image, Button, FlatList } from 'react-native';
import StockData from '../../../data/StockData';
import DataField from '../../controls/DataField';
import LoadingData from '../../LoadingData';
import ServerError from '../../ServerError';
import { FontSize } from '../../../variables/ResponsiveFont';

const ItemDetails = props => {

    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState(null);

    const [itemImage, setItemImage] = useState('');
    const [itemSpecsData, setItemSpecsData] = useState([]);

    useEffect(() => {

        if (!props.selectedItem || !props.selectedItem.itemRecID) {
            return;
        }

        StockData.GetItemSpecs(
            props.selectedItem.itemRecID,
            { setItemSpecsData },
            { setIsLoading },
            { setError }
        );

        StockData.GetItemImage(
            props.selectedItem.itemRecID,
            { setItemImage },
        );

        return(() => {
            //Cleanup
            setItemSpecsData([]);
            setItemImage('');
        })
    
      }, [props.selectedItem]);

    //While Loading Or Searching Data
    if (isLoading) {
        return (
            <LoadingData />
        );
    };

    //In case of error
    if (error) {
        return (
            <ServerError errorMessage={error.toString()} setError={ setError } />
        );
    };

    if (!props.selectedItem || !props.selectedItem.itemRecID) {
        return (
            <View style={styles.container}>
                <View style={styles.buttonsContainer}>
                    <Button title='Close' style={styles.button} onPress={props.onCloseButtonPress} />
                </View>
            </View>
        );
    };

    const ItemSpecsItem = itemSpecsData => {
        return (
            <DataField
                title={itemSpecsData.item.fieldTitle}
                data={itemSpecsData.item.fieldData} />
        );
    };    

    return (
        <View style={styles.container}>

            <View style={[styles.itemImageContainer, {height: itemImage == '' ? 0 : '30%'}]}>
                <Image
                    source={{ uri: "data:image/png;base64," + itemImage }}
                    style={styles.itemImage}
                    resizeMode='contain'
                />
            </View>

            <View style={styles.itemReferenceBarcodeContainer}>
                <Text style={styles.itemReferenceBarcodeText}>{props.selectedItem.itemReference}</Text>
                <Text style={styles.itemReferenceBarcodeText}>{props.selectedItem.itemBarcode}</Text>
            </View>

            <View style={styles.itemDescriptionContainer}>
                <Text style={styles.itemDescriptionText}>{props.selectedItem.itemDescription}</Text>
            </View>

            <View style={styles.itemPriceCurrencyContainer}>

                <View style={styles.itemPriceContainer}>
                    <Text style={styles.itemPriceText}>{Intl.NumberFormat("ar-de").format(props.selectedItem.itemPrice)}</Text>
                </View>

                <View style={styles.itemCurrencyContainer}>
                    <Text style={styles.itemCurrencyText}>{props.selectedItem.itemCurrency}</Text>
                </View>

            </View>

            <View style={styles.itemAvailabilityContainer}>

                <View style={styles.itemAvailableQtyContainer}>
                    <Text style={styles.itemAvailableQtyText}>Available Qty</Text>
                </View>

                <View style={styles.itemUnitContainer}>
                    <Text style={styles.itemAvailableQtyUnitText}>{Intl.NumberFormat("ar-de").format(props.selectedItem.itemAvailableQty)} {props.selectedItem.itemUnit}</Text>
                </View>

            </View>
            
            {/* 
            <View style={styles.itemAvailabilityContainer}>

                <View style={styles.itemAvailableQtyContainer}>
                    <Text style={styles.itemAvailableQtyText}>Available In Warehouses</Text>
                </View>

                <View style={styles.itemUnitContainer}>
                    <Text style={styles.itemAvailableQtyUnitText}>0 {props.selectedItem.itemUnit}</Text>
                </View>

            </View>

            <View style={styles.itemAvailabilityContainer}>

                <View style={styles.itemAvailableQtyContainer}>
                    <Text style={styles.itemAvailableQtyText}>Available In Company</Text>
                </View>

                <View style={styles.itemUnitContainer}>
                    <Text style={styles.itemAvailableQtyUnitText}>{props.selectedItem.itemAvailableQty} {props.selectedItem.itemUnit}</Text>
                </View>

            </View> */}

            <FlatList
                keyExtractor={(item, index) => item.fieldTitle.toString()}
                data={itemSpecsData}
                renderItem={ItemSpecsItem}
                numColumns={1}
                style={{ width: '100%', backgroundColor: Colors.bgColor }}
                contentContainerStyle={{ alignItems: 'center', justifyContent: 'center' }}
            />

            <View style={styles.buttonsContainer}>
                {/* <Button title='Print' style={styles.button} onPress={() => alert('Printer is Not Connected')} /> */}
                <Button title='Close' style={styles.button} onPress={props.onCloseButtonPress} />
            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
    },
    itemImageContainer: {
        //backgroundColor: 'black',
        width: '100%',
    },
    itemImage: {
        width: '100%',
        height: '100%',
        resizeMode: 'contain',
    },
    itemReferenceBarcodeContainer: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        padding: 5,
    },
    itemReferenceBarcodeText:{
        fontSize: FontSize(22),
        //fontWeight: 'bold',
        color: 'black',
    },
    itemDescriptionContainer: {
        //backgroundColor: 'red',
        height: 60,
        padding: 5,
        alignItems: 'center',
        justifyContent: 'center',
    },
    itemDescriptionText: {
        fontSize: FontSize(20),
        color: Colors.brandBlue,
    },
    itemPriceCurrencyContainer: {
        //backgroundColor: 'red',
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 5,
        marginBottom: 10,
    },
    itemPriceContainer: {
        //backgroundColor: 'blue',
        paddingRight: 5,
    },
    itemPriceText: {
        color: Colors.brandBlue,
        fontSize: FontSize(30),
    },
    itemCurrencyContainer: {
        //backgroundColor: 'green',
        paddingLeft: 5,
    },
    itemCurrencyText: {
        color: Colors.brandGreen,
        fontSize: FontSize(30),
    },
    itemAvailabilityContainer: {
        backgroundColor: Colors.listItemBackColor,
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 5,
        borderWidth: 1,
        borderColor: 'white',
    },
    itemAvailableQtyContainer: {
        //backgroundColor: 'blue',
        paddingRight: 5,
    },
    itemAvailableQtyText: {
        color: 'dimgray',
        fontSize: FontSize(19),
    },
    itemUnitContainer: {
        //backgroundColor: 'green',
        paddingLeft: 5,
    },
    itemAvailableQtyUnitText: {
        color: 'black',
        fontSize: FontSize(19),
    },
    buttonsContainer: {
        //backgroundColor: 'red',
        flexDirection: 'row',
        padding: 5,
        alignItems: 'center',
        justifyContent: 'flex-end',
        borderTopWidth: 2,
        borderColor: 'gray'
    },
    // button: {
    //     width: '40%',
    //     backgroundColor: 'red'
    // }
});

export default ItemDetails;
