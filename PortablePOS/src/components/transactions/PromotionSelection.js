import React from 'react';
import { useState, useEffect } from 'react';
import Sizing from '../../variables/Sizing';
import Colors from '../../variables/Colors';
import { FontSize } from '../../variables/ResponsiveFont';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity, ScrollView, FlatList, Modal } from 'react-native';
import StockData from '../../data/StockData';
import ModalTitle from '../ModalTitle';
import Items from '../lists/stock/Items';

const PromotionSelection = props => {

    const [stockSearchVisible, setStockScreenVisible] = useState(false);
    const [selectedPromotionRecID, setSelectedPromotionRecID] = useState(0);
    const [selectedPromotionCriteria, setSelectedPromotionCriteria] = useState('');
    const [selectedBuyQty, setSelectedBuyQty] = useState(0);

    const [selectedItemPrice, setSelectedItemPrice] = useState(0);
    const [selectedPromotionDiscount, setSelectedPromotionDiscount] = useState(0);

    // const DATATEST =  [{"tpBulkId": 0, "tpLineItemQty": "06", "tpLineNumber": 1, "tpPromotionLineNumber": 3, "tpPromotionRecId": 162, "tpTakenQty": 5},
    //                    {"tpBulkId": 1, "tpLineItemQty": "04", "tpLineNumber": 4, "tpPromotionLineNumber": 6, "tpPromotionRecId": 162, "tpTakenQty": 5}]

    useEffect(() => {
    
        console.log('rerenderrrrrrrrrrr')
        console.log(props.promotionRecords)
        console.log(props.selectedClientRecID)
        //setPromotionRecords(DATA);

    }, []);    

    const promotionRecord = (promData) => {

        if (promData.item.type === 'I') {
            return(
                <View style={styles.itemContainer}>

                    <View style={styles.itemDetailsContainer}>
                        <Text style={styles.itemDetailsQty}>{promData.item.getQty}</Text>
                        <Text style={styles.itemDetailsOperator}> x </Text>
                        <Text style={styles.itemDetailsDescription} numberOfLines={2}>{promData.item.itemReference} - {promData.item.itemDescription}</Text>
                    </View>

                    <View style={styles.itemCheckBoxContainer}>

                        <TouchableOpacity style={styles.itemCheckBoxImageContainer} disabled={promData.item.canSeleted === 'N'} onPress={() => {
                           const NewArr = [...props.promotionRecords];
                           NewArr[NewArr.findIndex(obj => obj.recKey == promData.item.recKey)].selected = promData.item.selected === 'Y' ? 'N' : 'Y';
                           props.setPromotionRecords(NewArr)
                        }}>
                            <Image
                                source={promData.item.selected === 'Y' ? require('../../assets/icons/checked.png') : require('../../assets/icons/unchecked.png')}
                                style={styles.itemCheckBoxButtonImage}
                                resizeMode='contain'
                            />
                        </TouchableOpacity>

                    </View>

                </View>
            )
        } 
        else {
            return(
                <View style={styles.criteriaContainer}>

                    <View style={styles.criteriaStrip} />

                    <View style={styles.criteriaDetailsContainer}>
                        <Text style={styles.criteriaDetailsQty}>{promData.item.getQty}</Text>
                        <Text style={styles.criteriaDetailsOperator}> x </Text>
                        <Text style={styles.criteriaDetailsDescription} numberOfLines={2}>{promData.item.crCode} - {promData.item.crDescription}</Text>
                    </View>

                    <View style={styles.criteriaButtonContainer}>
                            
                        <TouchableOpacity style={styles.criteriaButtonImageContainer} disabled={promData.item.getQty === 0} onPress={() => {
                            // const NewArr = [...props.promotionRecords];
                            // NewArr[NewArr.findIndex(obj => obj.recKey == promData.item.recKey)].selected = promData.item.selected === 'Y' ? 'N' : 'Y';
                            // props.setPromotionRecords(NewArr)

                            if (promData.item.getQty > 0) {
                                setSelectedPromotionRecID(promData.item.promRecID)
                                setSelectedPromotionCriteria(promData.item.crCode)
                                setSelectedBuyQty(promData.item.buyQty)

                                setSelectedPromotionDiscount(promData.item.itemDiscount)

                                setStockScreenVisible(true)
                            }

                        }}>
                            <Image
                                source={require('../../assets/icons/stock.png')}
                                style={styles.criteriaButtonImage}
                                resizeMode='contain'
                            />
                        </TouchableOpacity>

                    </View>
                  
                </View> 
            )
        } 
    }

    const onSelectItemFromSearch = async (itemRecID, itemReference, itemDescription, itemPrice) => {

        for (var i in props.promotionRecords) {
            if (props.promotionRecords[i].promRecID == selectedPromotionRecID && props.promotionRecords[i].crCode == selectedPromotionCriteria) {
                props.promotionRecords[i].getQty = props.promotionRecords[i].getQty - 1;
                break; //Stop this loop, we found it!
            }
        }

        var maxRecKey = Math.max(...props.promotionRecords.map(o => o.recKey));
        console.log(maxRecKey);

        StockData.GetPromotionItemPrice(
            maxRecKey,
            itemRecID,
            itemReference,
            itemDescription,
            props.selectedClientRecID,
            props.orderCurrency,
            { setSelectedItemPrice },
            { AddPromotionLine });

    };

    const AddPromotionLine = (maxRecKey, itemRecID, itemReference, itemDescription, itemPrice) => {

        console.log('here we need to add the new promotion line')

        props.setPromotionRecords(oldData => [...oldData,
            {
                recKey: maxRecKey + 1,
                promRecID: selectedPromotionRecID,
                type: "I",
                typeDescription: "Item",
                getQty: 1, //pending
                itemRecID: itemRecID,
                itemReference: itemReference,
                itemDescription: itemDescription, //pending
                itemPrice: itemPrice, //itemPrice,//pending
                itemDiscount: selectedPromotionDiscount, //pending
                crCode: "",
                crDescription: "",
                andOr: "OR",
                buyQty: selectedBuyQty,
                selected: "Y",
                canSeleted: "N",
            }])

    }

    const AddPromotionLines = async () => {

        props.promotionRecords.forEach(async (itm, index) => {

            if (itm.selected === 'Y' && itm.type === 'I') {

                return new Promise((resolve, reject) => {
                    try {

                        props.setGridItems(oldData => [...oldData,
                        {
                            lineNumber: props.lineNumber + itm.recKey,
                            lineItemRecID: itm.itemRecID,
                            lineItemReference: itm.itemReference,
                            lineItemDescription: itm.itemDescription,
                            lineItemVatType: 'T',//pending
                            lineItemVatPerc: 11,//pending
                            lineItemQty: itm.getQty, //pending
                            lineItemPrice: itm.itemPrice,
                            lineItemDiscount: itm.itemDiscount,
                            lineItemTotal: 0,//pending
                            lineItemVatValue: 0,//pending
                            lineItemRemark: 'Promotion',
                            lineItemIsPromotion: 'Y',
                            lineItemPromotionCriteria : ''
                        }]);

                        props.setTakenPromotions(oldData => [...oldData,
                        {
                            tpLineNumber: props.lineNumber,
                            tpPromotionLineNumber: props.lineNumber + itm.recKey,
                            tpPromotionRecId: itm.promRecID,
                            tpLineItemQty: props.itemQty,
                            tpTakenQty: itm.buyQty,
                            tpBulkId: props.promotionBulkId 
                        }])

                        resolve(props.setLineNumber(props.lineNumber + itm.recKey))

                    }
                    catch {
                        reject('Cannot Add Line')
                    }
                });

            }

        })

        await props.setPromotionBulkId(props.promotionBulkId + 1)

    };

    return (
        <View style={styles.container}>

            <Modal visible={stockSearchVisible}>
                <Items //pending
                    title='Promotions'
                    itemProductGroupCode=''
                    itemPromotionCriteria={selectedPromotionCriteria}
                    searchString=''
                    onSelectItem={(itemRecID, itemReference, itemDescription, itemPrice) => {
                        onSelectItemFromSearch(itemRecID, itemReference, itemDescription, itemPrice);
                    }}
                    setStockScreenVisible={setStockScreenVisible}
                    onCloseButtonPress={() => { setStockScreenVisible(false) }}
                />
            </Modal>

            <View style={styles.header}>
                <Text style={styles.titleText} numberOfLines={2}>Select a promotion !</Text>
            </View>

            <FlatList
                    keyExtractor={(item, index) => item.recKey.toString()}
                    data={props.promotionRecords}
                    renderItem={promotionRecord}
                    numColumns={1}
                    style={{ width: '100%', backgroundColor: Colors.bgColor }}
                    contentContainerStyle={{ alignItems: 'flex-start', justifyContent: 'center', width: 500 }}
                />

            <View style={styles.footer}>
                
                <TouchableOpacity style={styles.button} onPress={() => {
                    props.setPromotionVisible(false);
                    }}>
                    <Text style={styles.buttonText}>Cancel</Text>
                </TouchableOpacity>

                <TouchableOpacity style={styles.button} onPress={ async () => {
                    
                    props.setPromotionVisible(false);
                    AddPromotionLines();

                }}>
                    <Text style={styles.buttonText}>OK</Text>
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
    titleText: {
        color: Colors.brandBlue,
        fontWeight: 'bold',
        width: '100%',
        fontSize: FontSize(20),
    },
    itemContainer: {
        backgroundColor: Colors.bgColor,
        //flex: 1,
        //padding: 5,
        height: 50,
        alignItems: 'center',
        justifyContent: 'center',
        flexDirection: 'row',
        width:  Sizing.windowsWidth * 0.725,
        borderBottomWidth: 2,
        borderBottomColor: 'gainsboro',   
    },
    itemDetailsContainer: {
        //backgroundColor: 'yellow',
        height: '100%',
        width: '100%',
        flex: 1,
        alignItems: 'center',
        justifyContent: 'flex-start',
        flexDirection: 'row',
        padding: 5,
        overflow: 'hidden',
    },
    itemDetailsQty: {
        color: Colors.brandBlue,
        fontSize: FontSize(20),
        fontWeight: 'bold',
    },
    itemDetailsOperator: {
        color: Colors.brandGreen,
        fontSize: FontSize(17),
    },
    itemDetailsDescription: {
        color: Colors.brandBlue,
        fontSize: FontSize(17),
    },
    itemCheckBoxContainer: {
        //backgroundColor: 'green',
        height: '100%',
        width: 50,
        alignItems: 'center',
        justifyContent: 'center',
        //padding: 10
    },
    itemCheckBoxImageContainer: {
        backgroundColor: 'gainsboro',
        width: 25,
        height: 25,
        borderRadius: 25,
        alignItems: 'center',
        justifyContent: 'center',
    },
    itemCheckBoxButtonImage: {
        //alignItems: 'center',
        //marginHorizontal: 10,
        resizeMode: 'contain',
        width: '100%',
        height: '100%',
        //backgroundColor:'red',
    },
    criteriaContainer: {
        backgroundColor: Colors.bgColor,
        //flex: 1,
        //padding: 5,
        height: 50,
        alignItems: 'center',
        justifyContent: 'flex-start',
        flexDirection: 'row',
        width:  Sizing.windowsWidth * 0.725,
        borderBottomWidth: 2,
        borderBottomColor: 'gainsboro',   
    },
    criteriaStrip: {
        backgroundColor: Colors.brandGreen,
        width: 10,
        height: '100%'
    },
    criteriaDetailsContainer: {
        //backgroundColor: 'yellow',
        height: '100%',
        flex: 1,
        alignItems: 'center',
        justifyContent: 'center',
        flexDirection: 'row',
        padding: 5,
    },
    criteriaDetailsQty: {
        color: Colors.brandBlue,
        fontSize: FontSize(20),
        fontWeight: 'bold',
    },
    criteriaDetailsOperator: {
        color: Colors.brandGreen,
        fontSize: FontSize(17),
    },
    criteriaDetailsDescription: {
        color: Colors.brandBlue,
        fontSize: FontSize(17),
    },
    criteriaButtonContainer: {
        // /backgroundColor: 'green',
        height: '100%',
        width: 50,
        alignItems: 'center',
        justifyContent: 'center',
        //padding: 10
    },
    criteriaButtonImageContainer: {
        //backgroundColor: 'gainsboro',
        width: '80%',
        height: '80%',
        alignItems: 'center',
        justifyContent: 'center',
    },
    criteriaButtonImage: {
        //alignItems: 'center',
        //marginHorizontal: 10,
        resizeMode: 'contain',
        width: '100%',
        height: '100%',
        //backgroundColor:'red',
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

export default PromotionSelection;
