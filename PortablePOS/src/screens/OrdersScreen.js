import React from 'react';
import { useState, useEffect } from 'react';
import StockData from '../data/StockData';
import OrdersData from '../data/OrdersData';
import CommonData from '../data/CommonData';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import { View, Text, ScrollView, Image, StyleSheet, Button, TouchableOpacity, FlatList, Modal, TextInput, PermissionsAndroid } from 'react-native';
import TransactionHeader from '../components/TransactionHeader';
import ClientSearchBox from '../components/searchBoxes/ClientSearchBox';
import StockItemSearchBox from '../components/searchBoxes/StockItemSearchBox';
import MainButton from '../components/controls/MainButton';
import Line from '../components/transactions/Line';
import LineEditor from '../components/transactions/LineEditor';
import ItemProductGroups from '../components/lists/stock/ItemProductGroups';
import Orders from '../components/lists/orders/Orders';
import PromotionSelection from '../components/transactions/PromotionSelection';
import PromotionData from '../data/PromotionData';
import Printing from '../components/printing/Printing';
import { getCurrentDate, getLoginUserCodeName } from '../variables/LoginInfo';
import { useNavigation, useFocusEffect } from '@react-navigation/native';
import MessageBox from '../components/MessageBox';

const OrdersScreen = ({route, naviagation}) => {

  const {isReturn} = route.params;

  //Common
  const [localCurrency, setLocalCurrency] = useState('');
  const [controlCurrency, setControlCurrency] = useState('');
  const [operatingCurrency, setOperatingCurrency] = useState('');
  const [defaultCurrency, setDefaultCurrency] = useState('');
  const [rateLCCC, setRateLCCC] = useState(0);

  //Order
  const [ordersSearchVisible, setOrdersSearchVisible] = useState(false);
  const [headerRemarkModalVisible, setHeaderRemarkModalVisible] = useState(false);
  const [orderNumber, setOrderNumber] = useState('Assigned On Save');
  const [orderVatType, setOrderVatType] = useState('');
  const [orderHeaderRemark, setOrderHeaderRemark] = useState('');
  const [orderFooterRemark, setOrderFooterRemark] = useState('');
  const [orderCurrency, setOrderCurrency] = useState('');
  const [orderTotal, setOrderTotal] = useState(0);
  const [orderTotalVAT, setOrderTotalVAT] = useState(0);
  const [orderDiscrepancy, setOrderDiscrepancy] = useState(false);

  //Client
  const [selectedClientRecID, setSelectedClientRecID] = useState('');
  const [selectedClientCode, setSelectedClientCode] = useState('');
  const [selectedClientName, setSelectedClientName] = useState('');
  const [selectedClientPriceCat, setSelectedClientPriceCat] = useState('');

  //Stock Item
  const [selectedItemRecID, setSelectedItemRecID] = useState('');
  const [selectedItemReference, setSelectedItemReference] = useState('');
  const [selectedItemDescription, setSelectedItemDescription] = useState('');
  const [selectedItemVatType, setSelectedItemVatType] = useState('');
  const [selectedItemVatPerc, setSelectedItemVatPerc] = useState(0);
  const [selectedItemQty, setSelectedItemQty] = useState(1);
  const [selectedItemPrice, setSelectedItemPrice] = useState(0);
  const [selectedItemDiscount, setSelectedItemDiscount] = useState(0);
  const [selectedItemTotal, setSelectedItemTotal] = useState(0);
  const [selectedItemVatValue, setSelectedItemVatValue] = useState(0);
  const [selectedItemRemark, setSelectedItemRemark] = useState('');
  const [selectedItemPromotionCriteria, setSelectedItemPromotionCriteria] = useState('');

  //Line Editor 
  const [lineEditorVisible, setLineEditorVisible] = useState(false);

  //Promotion
  const [promotionVisible, setPromotionVisible] = useState(false);
  const [promotionRecords, setPromotionRecords] = useState([]);
  const [takenPromotions, setTakenPromotions] = useState([]); //tpLineNumber, tpPromotionRecId, tpTakenQty
  const [promotionBulkId, setPromotionBulkId] = useState(0);
  const [lastItemQty, setLastItemQty] = useState(0);

  const [gridItems, setGridItems] = useState([]);
  const [lineNumber, setLineNumber] = useState(0);
  const [entrySectionsCollapsed, setEntrySectionsCollapsed] = useState(false);

  const navigation = useNavigation();

  useFocusEffect(
    React.useCallback(() => {
      const onBeforeRemove = (e) => {

        if (gridItems.length === 0) {
          // If there's no data in the form, we don't need to show a prompt
          return;
        }

        // Prevent default behavior of leaving the screen
        e.preventDefault();

        // Show confirmation alert
        MessageBox.alert(
          'Discard changes?',
          'You have unsaved changes. Are you sure you want to discard them and leave the screen?',
          [
            { text: 'Cancel', style: 'cancel', onPress: () => {} },
            {
              text: 'Discard',
              style: 'destructive',
              // Go back if the user confirms
              onPress: () => navigation.dispatch(e.data.action),
            },
          ]
        );
      };

      navigation.addListener('beforeRemove', onBeforeRemove);

      return () => {
        navigation.removeListener('beforeRemove', onBeforeRemove);
      };
    }, [navigation])
  );

  // The bluetooth permission is asked when printing (Printing.PrintSlipReport), not when the page opens
  useEffect( () => {

    console.log('gridItems.length');

    return (() => {
      //Cleanup
      setPromotionRecords([]);
      setTakenPromotions([]);
      setGridItems([]);
    })

}, []);

  useEffect(() => {

    console.log('taken Promotions')
    console.log('taken Promotions')
    console.log(gridItems)
    console.log(takenPromotions)

    if (orderCurrency === '') {

      CommonData.GetCurrencies(
        { setLocalCurrency },
        { setControlCurrency },
        { setOperatingCurrency },
        { setDefaultCurrency },
        { setRateLCCC }
      );

    };

  }, []);

  useEffect(() => {

    if (orderCurrency === '') {
      setOrderCurrency(defaultCurrency);
    };

  }, [defaultCurrency]);

  useEffect(() => {

    if (selectedClientPriceCat !== '') {

      OrdersData.GetOrderVatType(
        selectedClientPriceCat,
        { setOrderVatType });
      
    }
    else {
      setOrderVatType('');
    };

  }, [selectedClientPriceCat]);

  useEffect(() => {

    var qty = selectedItemQty;
    var price = selectedItemPrice;
    var discount = selectedItemDiscount;

    var total = ( (100-discount) * (qty * price) ) / 100

    setSelectedItemTotal( total );

  }, [selectedItemQty, selectedItemPrice, selectedItemDiscount]);

  useEffect(() => {

    StockData.GetItemVatValue(
        selectedItemTotal,
        selectedItemVatType,
        selectedItemVatPerc,
        selectedClientPriceCat,
        { setSelectedItemVatValue });

  }, [selectedItemTotal]);

  useEffect(() => {

    var total = 0;
    var totalVat = 0;

    for (i = 0; i < gridItems.length; i++) {
      total += gridItems[i].lineItemTotal;
      totalVat += gridItems[i].lineItemVatValue;
    }

    setOrderTotal(total);
    setOrderTotalVAT(totalVat);

  }, [gridItems]);

  const LineItem = itemData => {
    return (
      <Line
        header={false}
        itemRecID={itemData.item.lineItemRecID}
        itemReference={itemData.item.lineItemReference}
        itemDescription={itemData.item.lineItemDescription}
        itemQty={itemData.item.lineItemQty}
        itemPrice={itemData.item.lineItemPrice}
        itemDiscount={itemData.item.lineItemDiscount}
        itemTotal={itemData.item.lineItemTotal}
        itemVatValue={itemData.item.lineItemVatValue}
        itemIsPromotion={itemData.item.lineItemIsPromotion}
        DeleteLine={() => DeleteLine(itemData.item.lineNumber.toString())}
      />
    )
  };

  const ChangeCurrency = () => {
    if (operatingCurrency === 'FC' && gridItems.length == 0) {

      setOrderCurrency(orderCurrency == localCurrency ? controlCurrency : localCurrency);

    }
  };

  const BeforeAddLine = async () => {

    if (selectedClientRecID == '') {
      alert('Please Fill Client');
    }
    else if (selectedItemRecID == '') {
      alert('Please Fill Item');
    }
    else {

      return new Promise((resolve, reject) => {
        try {

          StockData.GetItemPriceDiscount(
            selectedItemRecID,
            selectedClientRecID,
            orderCurrency,
            selectedItemQty,
            { setSelectedItemPrice },
            { setSelectedItemDiscount });

          resolve(setLineEditorVisible(true));

        }
        catch {
          reject('Cannot Get Item Price')
        }

      });

    };

  };

  const AddLine = () => {

    var newLine = lineNumber + 1;
    setLastItemQty(selectedItemQty);

    return new Promise((resolve, reject) => {
      try {

        setGridItems(oldData => [...oldData,
        {
          lineNumber: newLine,
          lineItemRecID: selectedItemRecID,
          lineItemReference: selectedItemReference,
          lineItemDescription: selectedItemDescription,
          lineItemVatType: selectedItemVatType,
          lineItemVatPerc: selectedItemVatPerc,
          lineItemQty: selectedItemQty,
          lineItemPrice: selectedItemPrice,
          lineItemDiscount: selectedItemDiscount,
          lineItemTotal: selectedItemTotal,
          lineItemVatValue: selectedItemVatValue,
          lineItemRemark: selectedItemRemark,
          lineItemIsPromotion: 'N',
          lineItemPromotionCriteria: selectedItemPromotionCriteria
        }]);

        const data1 = [...gridItems,
          {
            lineNumber: newLine,
            lineItemRecID: selectedItemRecID,
            lineItemReference: selectedItemReference,
            lineItemDescription: selectedItemDescription,
            lineItemVatType: selectedItemVatType,
            lineItemVatPerc: selectedItemVatPerc,
            lineItemQty: selectedItemQty,
            lineItemPrice: selectedItemPrice,
            lineItemDiscount: selectedItemDiscount,
            lineItemTotal: selectedItemTotal,
            lineItemVatValue: selectedItemVatValue,
            lineItemRemark: selectedItemRemark,
            lineItemIsPromotion: 'N',
            lineItemPromotionCriteria: selectedItemPromotionCriteria
          }].reduce((acc, obj) => {
          acc[obj.lineNumber] = obj;
          return acc;
        }, {});

        const data2 = takenPromotions.reduce((acc, obj) => {
          acc[obj.tpLineNumber] = obj;
          return acc;
        }, {});

        const result = Object.keys(data1).map(key => {
          const item1 = data1[key];
          const item2 = data2[key];
          const takenQty = item2 ? item2.tpTakenQty : 0;
          const tpPromotionRecId = item2 ? item2.tpPromotionRecId : '';
          const newItemQty = item1.lineItemQty - takenQty;
          return {
            lineKey: item1.lineNumber,
            itemRecID: item1.lineItemRecID,
            itemPromotionCriteria: item1.lineItemPromotionCriteria,
            tpPromotionRecId: tpPromotionRecId,
            newItemQty: newItemQty // Ensure newItemQty is non-negative
          };
        });

        console.log('...........')
        console.log(result)
        console.log('...........')

        PromotionData.GetPromotion(selectedItemRecID, selectedItemQty, selectedClientRecID, JSON.stringify(result), orderCurrency, { setPromotionRecords }, { setPromotionVisible })


        console.log(promotionRecords)

        resolve(setLineNumber(newLine));
        resolve(ClearLine());

      } 
      catch (error) {
        reject(error)
      }
    });

  };

  const DeleteLine = lineNumber => {

    if (takenPromotions.filter(function (tp) {
        return tp.tpLineNumber == lineNumber
      }).length > 0) 
      {
        alert('You cannot delete item related to promotion')
      }
      else
      {
        setGridItems(gridItems.filter(function (selectedLineNumber) {
          return selectedLineNumber.lineNumber != lineNumber
        }))
      }

  };

  const SaveTransaction = () => {

    if (selectedClientRecID == '') {
      alert('Please Fill Client');
    }
    else if (gridItems.length == 0) {
      alert('No Items to Save');
    }
    else {
      OrdersData.SaveOrder(
        isReturn,
        selectedClientRecID,
        orderVatType,
        JSON.stringify(gridItems),
        orderHeaderRemark,
        orderFooterRemark,
        orderCurrency,
        orderTotal,
        orderTotalVAT,
        orderDiscrepancy,
        PrintTransaction);
    };

  };

  const PrintTransaction = async (number) => {

    console.log('Printing the order')

    var returnOrd = isReturn ? "Return " : ""

    try {

      var text =
      "[L]\n" +
      "[C]<u><font size='big'>" + returnOrd + "Order Number : " + number + "</font></u>\n" +
      "[L]\n" +
      "[L]Date : " + getCurrentDate() + "\n" +
      "[L]User : " + getLoginUserCodeName() + "\n" +
      "[L]Curr : " + orderCurrency + "\n" +
      "[L]\n" +
      "[C]==============================================\n" +
      "[L]\n" +
      "[L]<b>Client : </b> " + selectedClientName + "\n" +
      "[L]\n" +
      "[C]----------------------------------------------\n" 
      "[C]\n" 
  
      gridItems.forEach((itm) => {
        text = text + "[L] " + itm.lineItemDescription + "\n"
        text = text + "[L] " + Intl.NumberFormat("ar-de").format(itm.lineItemQty) + " x " + Intl.NumberFormat("ar-de").format(itm.lineItemPrice) + " - " + Intl.NumberFormat("ar-de").format(itm.lineItemDiscount) + " % = "

        if (itm.lineItemTotal === 0) {
          text = text + "GIFT"
        } 
        else {
          text = text + Intl.NumberFormat("ar-de").format(itm.lineItemTotal)
        } 

        if (itm.lineItemIsPromotion === 'Y') {
          text = text + " - Promotion"
        } 

        text = text + "\n"
        text = text + "[L]..............................................\n"
      })
  
      text = text + "[C]----------------------------------------------\n" +
      "[C]\n" +
      "[R]<b>Total " + orderCurrency + " :</b>[R]" + Intl.NumberFormat("ar-de").format(orderTotal) + "\n"

      if (orderTotalVAT !== 0) {
        text = text + "[R]<b>VAT " + orderCurrency + " :</b>[R]" + Intl.NumberFormat("ar-de").format(orderTotalVAT) + "\n" 
      }

      text = text + "[R]<b>NET " + orderCurrency + " :</b>[R]" + Intl.NumberFormat("ar-de").format(orderTotal + (orderVatType == 'T' ? 0 : orderTotalVAT)) + "\n" +
      "[L]\n" +
      "[C]==============================================\n" +
      "[L]\n" +
      "[C]<qrcode size='20'>" + returnOrd + "Order " + number + "</qrcode>\n" +
      "[L]\n";
  
      console.log(text)
  
      await Printing.PrintSlipReport(text);
  
      ClearScreen();  

    } catch (error) {
      alert(error)
    }

  }

  const ClearLine = () => {
    setSelectedItemRecID('');
    setSelectedItemReference('');
    setSelectedItemDescription('');
    setSelectedItemVatType('');
    setSelectedItemVatPerc(0);
    setSelectedItemQty(1);
    setSelectedItemPrice(0);
    setSelectedItemDiscount(0);
    setSelectedItemTotal(0);
    setSelectedItemVatValue(0);
    setSelectedItemRemark('');
    setSelectedItemPromotionCriteria('');
  };

  const ClearClient = () => {
    setSelectedClientRecID('');
    setSelectedClientCode('');
    setSelectedClientName('');
    setSelectedClientPriceCat('');
  };

  const ClearGrid = () => {
    setLineNumber(0);
    setPromotionBulkId(0);
    setGridItems([]);
    setPromotionRecords([]);
    setTakenPromotions([]);
  };

  const ClearScreen = () => {
    setOrderVatType('');
    setOrderHeaderRemark('');
    setOrderFooterRemark('');
    setOrderTotal(0);
    setOrderTotalVAT(0);
    setOrderDiscrepancy(false);
    ClearLine();
    ClearGrid();
    ClearClient();
    setLineNumber(0);
    setPromotionBulkId(0);
  };

  return (
    <View style={styles.container}>

      {(!Sizing.isPortableScanner || !entrySectionsCollapsed) && (
        <>
          <TransactionHeader
            orderNumber={orderNumber}
            orderCurrency={orderCurrency}
            onCurrencyClick={ChangeCurrency}
            orderVatType={orderVatType}
            onHeaderRemarkPress={() => setHeaderRemarkModalVisible(true)}
            hasHeaderRemark={orderHeaderRemark.trim() !== ''}
            headerRemarkColor={isReturn ? 'red' : Colors.brandBlue}
          />

          <ClientSearchBox
            selectedClientName={selectedClientName}
            setSelectedClientRecID={setSelectedClientRecID}
            setSelectedClientCode={setSelectedClientCode}
            setSelectedClientName={setSelectedClientName}
            setSelectedClientPriceCat={setSelectedClientPriceCat}
            disabled={gridItems.length !== 0}
          />
        </>
      )}

      <Modal visible={ordersSearchVisible}>
        <Orders isReturn={isReturn} onCloseButtonPress={() => {setOrdersSearchVisible(false)}} />
      </Modal>

      {(!Sizing.isPortableScanner || !entrySectionsCollapsed) && (
        <>
          <StockItemSearchBox
            selectedItemDescription={selectedItemDescription}
            setSelectedItemRecID={setSelectedItemRecID}
            setSelectedItemQty={setSelectedItemQty}
            selectedItemQty={selectedItemQty}
            setSelectedItemReference={setSelectedItemReference}
            setSelectedItemDescription={setSelectedItemDescription}
            setSelectedItemVatType={setSelectedItemVatType}
            setSelectedItemVatPerc={setSelectedItemVatPerc}
            setSelectedItemPromotionCriteria={setSelectedItemPromotionCriteria}
            disabled={selectedClientRecID == ''}
          />

          <View style={styles.addLineButtonContainer}>
            <TouchableOpacity
              style={[styles.addLineButton, {backgroundColor: isReturn ? 'red' : Colors.brandGreen}]}
              disabled={selectedItemRecID == ''}
              onPress={() => {

                //Temporary test added by Jad - to be applied in case of starting qty only
                if (!isReturn && gridItems.filter(function (itm) {
                  return itm.lineItemRecID == selectedItemRecID
                }).length > 0) {
                  alert('Item Already Exist')
                }
                else {
                  BeforeAddLine()
                }

              }
              }>
              <Text style={styles.addLineButtonText}>Add to {isReturn ? 'Return ' : ''}Order</Text>
            </TouchableOpacity>
          </View>
        </>
      )}

      <Modal
        visible={lineEditorVisible}
        transparent={true}
        presentationStyle="overFullScreen">
        <View style={styles.modalContainer}>
          <View style={[styles.modalCard, {borderColor: isReturn ? 'red' : Colors.brandBlue,}]}>
            <LineEditor
              setLineEditorVisible={setLineEditorVisible}
              lineStatus={isReturn? 'X' : 'S'}
              selectedItemRecID={selectedItemRecID}
              selectedItemReference={selectedItemReference}
              selectedItemDescription={selectedItemDescription}
              selectedItemVatType={selectedItemVatType}
              selectedItemVatPerc={selectedItemVatPerc}
              selectedItemQty={selectedItemQty}
              setSelectedItemQty={setSelectedItemQty}
              selectedItemPrice={selectedItemPrice}
              setSelectedItemPrice={setSelectedItemPrice}
              selectedItemDiscount={selectedItemDiscount}
              setSelectedItemDiscount={setSelectedItemDiscount}
              selectedItemTotal={selectedItemTotal}
              setSelectedItemTotal={setSelectedItemTotal}
              selectedItemRemark={selectedItemRemark}
              setSelectedItemRemark={setSelectedItemRemark}
              selectedItemPromotionCriteria={selectedItemPromotionCriteria}
              setSelectedItemPromotionCriteria={setSelectedItemPromotionCriteria}
              selectedClientRecID={selectedClientRecID}
              transactionCurrency={orderCurrency}
              onSubmit={() => AddLine()}
              refreshPrices={(qty) => {

                StockData.GetItemPriceDiscount(
                  selectedItemRecID,
                  selectedClientRecID,
                  orderCurrency,
                  qty,
                  { setSelectedItemPrice },
                  { setSelectedItemDiscount });

              }}
            />
          </View>
        </View>
      </Modal>

      <Modal
        visible={headerRemarkModalVisible}
        transparent={true}
        presentationStyle="overFullScreen">
        <View style={styles.modalContainer}>
          <View style={[styles.headerRemarkModalCard, { borderColor: isReturn ? 'red' : Colors.brandBlue }]}>
            <Text style={[styles.headerRemarkModalTitle, { color: isReturn ? 'red' : Colors.brandBlue }]}>
              Header Remark
            </Text>
            <TextInput
              style={styles.headerRemarkModalInput}
              value={orderHeaderRemark}
              onChangeText={setOrderHeaderRemark}
              maxLength={500}
              multiline={true}
              placeholder="Enter header remark..."
              placeholderTextColor="silver"
              autoFocus={true}
            />
            <View style={styles.headerRemarkModalButtons}>
              <TouchableOpacity
                style={styles.headerRemarkModalClearButton}
                onPress={() => setOrderHeaderRemark('')}>
                <Text style={styles.headerRemarkModalClearButtonText}>Clear</Text>
              </TouchableOpacity>
              <TouchableOpacity
                style={[styles.headerRemarkModalDoneButton, { backgroundColor: isReturn ? 'red' : Colors.brandBlue }]}
                onPress={() => setHeaderRemarkModalVisible(false)}>
                <Text style={styles.headerRemarkModalDoneButtonText}>Done</Text>
              </TouchableOpacity>
            </View>
          </View>
        </View>
      </Modal>
 
      <Modal
        visible={promotionVisible}
        transparent={true}
        presentationStyle="overFullScreen">
        <View style={styles.modalContainer}>
        <View style={[styles.modalCard, {borderColor: isReturn ? 'red' : Colors.brandBlue,}]}>

            <PromotionSelection 
              setPromotionVisible={setPromotionVisible}
              setGridItems={setGridItems}
              gridItems={gridItems}
              setLineNumber={setLineNumber}
              takenPromotions={takenPromotions}
              setTakenPromotions={setTakenPromotions}
              lineNumber={lineNumber}
              itemQty={lastItemQty}
              promotionRecords={promotionRecords}
              setPromotionRecords={setPromotionRecords}
              promotionBulkId={promotionBulkId}
              setPromotionBulkId={setPromotionBulkId}
              orderCurrency={orderCurrency}
              selectedClientRecID={selectedClientRecID}
            />

          </View>
        </View>
      </Modal>

      <View style={styles.gridContainer}>
      <Line
        header={true}
        itemRecID={''}
        itemReference={'Reference'}
        itemDescription={'Description'}
        itemQty={'Qty'}
        itemPrice={`Price ${orderCurrency}`}
        itemDiscount={'Disc %'}
        itemTotal={`Total ${orderCurrency}`}
        headerToggleCollapsed={entrySectionsCollapsed}
        onHeaderTogglePress={Sizing.isPortableScanner ? () => setEntrySectionsCollapsed(prev => !prev) : undefined}
      />

      <FlatList
        keyExtractor={(item, index) => item.lineNumber.toString()}
        data={gridItems}
        renderItem={LineItem}
        numColumns={1}
        style={styles.gridList}
        contentContainerStyle={{
          alignItems: 'center',
          justifyContent: 'center',
        }}
      />
      </View>

      {/* 
      <View style={styles.footerContainer}>

        <View style={styles.footerTitleContainer}>
          <Text style={styles.footerTitleText}>Footer Remark</Text>
        </View>

        <View style={styles.footerDataContainer}>
          <TextInput
            style={styles.footerDataText}
            value={orderFooterRemark}
            onChangeText={setOrderFooterRemark}
            maxLength={500} />
        </View>

      </View> */}

      <View style={styles.totalsContainer}>

        {isReturn &&
          <View style={styles.discrepancyContainer}>
            <TouchableOpacity
              style={styles.discrepancyCheckBoxContainer}
              onPress={() => setOrderDiscrepancy(!orderDiscrepancy)}>
              <Image
                source={orderDiscrepancy ? require('../assets/icons/checked.png') : require('../assets/icons/unchecked.png')}
                style={styles.discrepancyCheckBoxImage}
                resizeMode='contain'
              />
            </TouchableOpacity>
            <Text style={styles.discrepancyLabel}>Discrepancy</Text>
          </View>
        }

        <Text style={styles.totalTitle}>Total :</Text>
        <Text style={styles.totalDataText}>{Intl.NumberFormat("ar-de").format(orderTotal)}</Text>

        <Text style={styles.totalTitle}>    VAT {orderVatType == 'T' ? 'TTC' : orderVatType == 'A' ? 'Add-On' : ''} :</Text>
        <Text style={styles.totalDataText}>{Intl.NumberFormat("ar-de").format(orderTotalVAT)}</Text>

      </View>
      <View style={styles.totalsContainer}>

        <Text style={styles.totalTitle}>    NET :</Text>
        <Text style={styles.totalDataText}>{Intl.NumberFormat("ar-de").format(orderTotal + (orderVatType == 'T' ? 0 : orderTotalVAT))}</Text>

      </View>

      <View style={styles.mainButtonsContainer}>

        <TouchableOpacity
          style={styles.buttonContainer}
          onPress={() => setOrdersSearchVisible(true)}>
          <Text style={[styles.buttonText, {color: isReturn ? 'red' : Colors.brandBlue}]}>Search {isReturn ? 'R.' : ''}Orders</Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={styles.buttonContainer}
          onPress={() => {
            SaveTransaction();
          }}>
          <Text style={[styles.buttonText, {color: isReturn ? 'red' : Colors.brandBlue}]}>Save {isReturn ? 'R.' : ''}Order</Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={styles.buttonContainer}
          onPress={() => {
            ClearScreen();
            alert('Reset Order');
          }}>
          <Text style={[styles.buttonText, {color: isReturn ? 'red' : Colors.brandBlue}]}>Reset {isReturn ? 'R.' : ''}Order</Text>
        </TouchableOpacity>

      </View>

    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    backgroundColor: Colors.bgColor,
    height: '100%',
    paddingTop: 10,
    paddingLeft: 10,
    paddingRight: 10,
  },
  gridContainer: {
    flex: 1,
    width: '100%',
  },
  gridList: {
    flex: 1,
    width: '100%',
    backgroundColor: Colors.bgColor,
  },
  addLineButtonContainer: {
    backgroundColor: Colors.brandBlue,
    alignItems: 'center',
    justifyContent: 'center',
    width: '100%',
    height: 25,
    elevation: 10,
    marginBottom: 10,
  },
  addLineButton: {
    width: '100%',
    height: '100%',
    alignItems: 'center',
    justifyContent: 'center',
  },
  addLineButtonText: {
    color: 'white',
  },
  modalContainer: {
    width: '100%',
    height: '100%',
    backgroundColor: 'rgba(0,0,0,0.5)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  modalCard: {
    width: Sizing.windowsWidth * 0.75,
    height: Sizing.windowsHeight * 0.55,
    backgroundColor: 'white',
    borderRadius: 20,
    overflow: 'hidden',
    elevation: 10,
    borderWidth: 5,
    alignItems: 'center',
    justifyContent: 'center',
  },
  headerRemarkModalCard: {
    width: Sizing.windowsWidth * 0.85,
    height: Sizing.windowsHeight * 0.35,
    backgroundColor: 'white',
    borderRadius: 20,
    overflow: 'hidden',
    elevation: 10,
    borderWidth: 5,
    padding: 15,
  },
  headerRemarkModalTitle: {
    fontSize: FontSize(18),
    fontWeight: 'bold',
    marginBottom: 10,
  },
  headerRemarkModalInput: {
    flex: 1,
    borderWidth: 1,
    borderColor: 'silver',
    borderRadius: 8,
    padding: 10,
    fontSize: FontSize(16),
    color: 'black',
    textAlignVertical: 'top',
  },
  headerRemarkModalButtons: {
    flexDirection: 'row',
    justifyContent: 'flex-end',
    marginTop: 12,
  },
  headerRemarkModalClearButton: {
    paddingVertical: 8,
    paddingHorizontal: 16,
    marginRight: 10,
  },
  headerRemarkModalClearButtonText: {
    color: 'gray',
    fontSize: FontSize(16),
  },
  headerRemarkModalDoneButton: {
    paddingVertical: 8,
    paddingHorizontal: 20,
    borderRadius: 6,
  },
  headerRemarkModalDoneButtonText: {
    color: 'white',
    fontSize: FontSize(16),
    fontWeight: 'bold',
  },
  totalsContainer: {
    width: '100%',
    height: 30,
    backgroundColor: 'rgba(0,0,0,0.2)',
    alignItems: 'center',
    justifyContent: 'flex-end',
    flexDirection: 'row',
    paddingRight: 10,
    //elevation : 10,
  },
  discrepancyContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginRight: 'auto',
    paddingLeft: 10,
  },
  discrepancyCheckBoxContainer: {
    width: 22,
    height: 22,
    alignItems: 'center',
    justifyContent: 'center',
  },
  discrepancyCheckBoxImage: {
    width: 22,
    height: 22,
  },
  discrepancyLabel: {
    fontSize: FontSize(14),
    fontWeight: 'bold',
    color: 'red',
    marginLeft: 6,
  },
  totalTitle: {
    fontSize: FontSize(15),
    paddingRight: 10,
    fontWeight: 'bold',
    color: 'black',
  },
  totalDataText: {
    fontSize: FontSize(17),
    fontWeight: 'bold',
    color: Colors.brandBlue,
  },
  footerContainer: {
    //backgroundColor: 'blue',
    width: '100%',
    height: 40,
    alignItems: 'center',
    justifyContent: 'flex-start',
    flexDirection: 'row',
    borderTopWidth: 3,
    borderColor: 'rgba(0,0,0,0.2)'
  },
  footerTitleContainer: {
    //backgroundColor: 'green',
    width: Sizing.isTablet ? '8%' : '25%',
    height: 40,
    alignItems: 'center',
    justifyContent: 'center',
  },
  footerTitleText: {
    fontSize: FontSize(14),
    fontWeight: 'bold',
    color: 'black',
  },
  footerDataContainer: {
    //backgroundColor: 'yellow',
    width: Sizing.isTablet ? '92%' : '75%',
    height: 40,
    alignItems: 'center',
    justifyContent: 'center',
  },
  footerDataText: {
    fontSize: FontSize(14),
    color: 'black',
    //backgroundColor: 'red',
    width: '100%',
  },
  mainButtonsContainer: {
    alignItems: Sizing.isTablet ? 'flex-end' : 'center',
    justifyContent: 'space-between',
    padding: 10,
    flexDirection: 'row',
  },
  buttonContainer: {
    
  },
  buttonText: {
    //color: Colors.brandBlue,
    fontSize: FontSize(18),
    fontWeight: 'bold',
  },
});

export default OrdersScreen;
