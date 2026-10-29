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
import Due from '../components/transactions/Due';
import Payment from '../components/transactions/Payment';
import CollectionOptions from '../components/transactions/CollectionOptions';
import LoadingData from '../components/LoadingData';
import ServerError from '../components/ServerError';
import ClientReceiptData from '../data/ClientReceiptData';
import Printing from '../components/printing/Printing';
import { getCurrentDate, getLoginUserCodeName } from '../variables/LoginInfo';

const ClientReceiptScreen = props => {

  //Control Screen
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  
  //Common
  const [localCurrency, setLocalCurrency] = useState('');
  const [controlCurrency, setControlCurrency] = useState('');
  const [operatingCurrency, setOperatingCurrency] = useState('');
  const [defaultCurrency, setDefaultCurrency] = useState('');
  const [rateLCCC, setRateLCCC] = useState(0);

  //Client Receipt
  const [receiptNumber, setReceiptNumber] = useState('Auto Increment');
  const [receiptRemark, setReceiptRemark] = useState('');

  //Collection
  const [collectionOptionsLCVisible, setCollectionOptionsLCVisible] = useState(false);
  const [collectionOptionsCCVisible, setCollectionOptionsCCVisible] = useState(false);
  
  //Payment
  const [paymentVisible, setPaymentVisible] = useState(false);
  const [paidAmountLC, setPaidAmountLC] = useState(0);
  const [paidAmountCC, setPaidAmountCC] = useState(0);

  //Dues
  const [duesList, setDuesList] = useState([]);

  const [totalDuesLC, setTotalDuesLC] = useState(0);
  const [totalDuesCC, setTotalDuesCC] = useState(0);
  const [totalToBePaidLC, setTotalToBePaidLC] = useState(0);
  const [totalToBePaidCC, setTotalToBePaidCC] = useState(0);

  //Client
  const [selectedClientRecID, setSelectedClientRecID] = useState('');
  const [selectedClientCode, setSelectedClientCode] = useState('');
  const [selectedClientName, setSelectedClientName] = useState('');
  const [selectedClientPriceCat, setSelectedClientPriceCat] = useState('');

  // The bluetooth permission is asked when printing (Printing.PrintSlipReport), not when the page opens
  useEffect( () => {

    return (() => {
      //Cleanup
      setDuesList([]);
    })

}, []);

  useEffect(() => {

    if (localCurrency === '') {

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

    calculateTotals();

  }, [duesList, collectionOptionsLCVisible, collectionOptionsCCVisible]);

  const calculateTotals = () => {
    var totDuesLC = 0;
    var totDuesCC = 0;
    var totToBePaidLC = 0;
    var totToBePaidCC = 0;

    for (i = 0; i < duesList.length; i++) {

      if (duesList[i].dueCurrency == localCurrency)
      {
        totDuesLC += duesList[i].dueValue;
        totToBePaidLC += duesList[i].amountToPay;
      }
      else if (duesList[i].dueCurrency == controlCurrency) {
        totDuesCC += duesList[i].dueValue;
        totToBePaidCC += duesList[i].amountToPay;
      }

    }

    console.log(duesList);

    setTotalDuesLC(totDuesLC);
    setTotalDuesCC(totDuesCC);
    setTotalToBePaidLC(totToBePaidLC);
    setTotalToBePaidCC(totToBePaidCC);
  }

  // const ChangeCurrency = () => {
  //   if (operatingCurrency === 'FC'&& duesList.length == 0) {

  //     setReceiptCurrency(receiptCurrency == localCurrency ? controlCurrency : localCurrency);

  //   }
  // };

  const LoadDues = () => {

    ClientReceiptData.GetClientDues(
      selectedClientRecID,
      { setDuesList },
      { setIsLoading },
      { setError }
    );

  }

  const ClearClient = () => {
    setSelectedClientRecID('');
    setSelectedClientCode('');
    setSelectedClientName('');
    setSelectedClientPriceCat('');
  };

  const ClearScreen = () => {
    setDuesList([]);
    ClearClient();
    
    setPaidAmountLC(0);
    setPaidAmountCC(0);

  }

  const PrintReceipt = async (numbers) => {

    console.log('TEst')
    console.log(selectedClientName.toString())

    try {

      const text =
        "[L]\n" +
        "[C]<u><font size='big'>" + numbers + "</font></u>\n" +
        "[L]\n" +
        "[L]Date : " + getCurrentDate() + "\n" +
        "[L]User : " + getLoginUserCodeName() + "\n" +
        "[L]\n" +
        "[C]==============================================\n" +
        "[L]\n" +
        "[L]<b>Client : </b> " + selectedClientName + "\n" +
        "[L]\n" +
        "[C]----------------------------------------------\n" +
        "[R]<b>Settled Dues " + localCurrency + " :</b>[R]" + Intl.NumberFormat("ar-de").format(totalToBePaidLC) + "\n" +
        "[R]<b>Settled Dues " + controlCurrency + " :</b>[R]" + Intl.NumberFormat("ar-de").format(totalToBePaidCC) + "\n" +
        "[L]\n" +
        "[C]----------------------------------------------\n" +
        "[R]<b>CASH " + localCurrency + " :</b>[R]" + Intl.NumberFormat("ar-de").format(paidAmountLC) + "\n" +
        "[R]<b>CASH " + controlCurrency + " :</b>[R]" + Intl.NumberFormat("ar-de").format(paidAmountCC) + "\n" +
        "[L]\n" +
        "[C]==============================================\n" +
        "[L]\n" +
        "[C]<qrcode size='20'>Receipts " + numbers + " / Client " + selectedClientCode + " - " + selectedClientName + " / Cash " + localCurrency + " " + Intl.NumberFormat("ar-de").format(totalToBePaidLC) + " / Cash " + controlCurrency + " " + Intl.NumberFormat("ar-de").format(totalToBePaidCC) + " </qrcode>\n" +
        "[L]\n";

      console.log(text)

      await Printing.PrintSlipReport(text);

      ClearScreen();

    } catch (error) {
      alert(error)
    }

  }

  const SaveTransaction = () => {

    if (selectedClientRecID == '') {
      alert('Please Fill Client');
    }
    else if (duesList.length == 0) {
      alert('No Dues to Save');
    }
    else if (totalToBePaidLC == 0 && totalToBePaidCC == 0) {
      alert('Amount to pay should be greater than Zero');
    }
    else {
      ClientReceiptData.SaveClientReceipt(selectedClientRecID,
        selectedClientName,
        totalToBePaidLC,
        totalToBePaidCC,
        JSON.stringify(duesList),
        rateLCCC,
        paidAmountLC,
        paidAmountCC,
        PrintReceipt);
    };

  };

  const DueItem = (itemData, index) => {

    return (
      <Due
        lineNumber={itemData.item.lineNumber}
        checked={itemData.item.checked}
        locationCode={itemData.item.locationCode}
        locationDescription={itemData.item.locationDescription}
        invoiceRecID={itemData.item.invoiceRecID}
        invoiceNumber={itemData.item.invoiceNumber}
        invoiceDate={itemData.item.invoiceDate}
        invoiceNetTotal={itemData.item.invoiceNetTotal}
        invoiceCurrency={itemData.item.invoiceCurrency}
        dueCashRegister={itemData.item.dueCashRegister}
        dueOperationNumber={itemData.item.dueOperationNumber}
        dueCounter={itemData.item.dueCounter}
        dueValueDate={itemData.item.dueValueDate}
        dueValueDateDisplay={itemData.item.dueValueDateDisplay}
        duePercentage={itemData.item.duePercentage}
        initDueValue={itemData.item.initDueValue}
        dueValue={itemData.item.dueValue}
        dueCurrency={itemData.item.dueCurrency}
        amountToPay={itemData.item.amountToPay}
        dueRemark={itemData.item.dueRemark}
      />
    )
  };

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

  return (
    <View style={styles.container}>

      <Modal
        visible={paymentVisible}
        transparent={true}
        presentationStyle="overFullScreen">
        <View style={styles.modalContainer}>
          <View style={styles.modalCard}> 
            <Payment 
              setPaymentVisible={setPaymentVisible}

              totalLC= {Number(Number(totalToBePaidLC) + ( Number(totalToBePaidCC) * Number(rateLCCC)))}
              totalCC= {Number(Number(totalToBePaidCC) + ( Number(totalToBePaidLC) / Number(rateLCCC)))}

              rateLCCC={rateLCCC}
              paidAmountLC={paidAmountLC}
              setPaidAmountLC={setPaidAmountLC}
              paidAmountCC={paidAmountCC}
              setPaidAmountCC={setPaidAmountCC}

              localCurrency={localCurrency}
              controlCurrency={controlCurrency}

              onSubmit={() => {
                SaveTransaction();
              }}

            />
          </View>
        </View>
      </Modal>

      <Modal
        visible={collectionOptionsLCVisible}
        transparent={true}
        presentationStyle="overFullScreen">
        <View style={styles.modalContainer}>
          <View style={styles.modalCard}> 
            
              <CollectionOptions 
                setCollectionOptionsVisible={setCollectionOptionsLCVisible}
                receiptCurrency={localCurrency}
                setDuesList={setDuesList}
                duesList={duesList}
                totalDues={totalDuesLC}
              />
            
          </View>
        </View>
      </Modal>

      <Modal
        visible={collectionOptionsCCVisible}
        transparent={true}
        presentationStyle="overFullScreen">
        <View style={styles.modalContainer}>
          <View style={styles.modalCard}> 
            
              <CollectionOptions 
                setCollectionOptionsVisible={setCollectionOptionsCCVisible}
                receiptCurrency={controlCurrency}
                setDuesList={setDuesList}
                duesList={duesList}
                totalDues={totalDuesCC}
              />
            
          </View>
        </View>
      </Modal>

      <TransactionHeader
        receiptNumber={receiptNumber}
        receiptRemark={receiptRemark}
        orderCurrency={''}
        onCurrencyClick={()=>{}}
        orderVatType={''}
        setReceiptRemark={setReceiptRemark}
      />

      <ClientSearchBox
        selectedClientName={selectedClientName}
        setSelectedClientRecID={setSelectedClientRecID}
        setSelectedClientCode={setSelectedClientCode}
        setSelectedClientName={setSelectedClientName}
        setSelectedClientPriceCat={setSelectedClientPriceCat}
        disabled={duesList.length !== 0}
      />

      <View style={styles.loadDuesButtonContainer}>
        <TouchableOpacity
          style={styles.loadDuesButton}
          disabled={selectedClientRecID == ''}
          onPress={() => {LoadDues()}}>
          <Text style={styles.loadDuesButtonText}>Load Dues</Text>
        </TouchableOpacity>
      </View>

      <FlatList
        keyExtractor={(item, index) => item.lineNumber.toString()}
        data={duesList}
        renderItem={DueItem}
        numColumns={1}
        style={{ width: '100%', backgroundColor: Colors.bgColor }}
        contentContainerStyle={{
          alignItems: 'center',
          justifyContent: 'center',
        }}
      />
      

      <View style={styles.mainButtonsContainer}>

        <TouchableOpacity
          style={styles.buttonContainer}
          onPress={() => {setCollectionOptionsLCVisible(true)}}
          >
          <Text style={styles.buttonText}>Collection Options {localCurrency}</Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={styles.buttonContainer}
          onPress={() => {setCollectionOptionsCCVisible(true)}}
          >
          <Text style={styles.buttonText}>Collection Options {controlCurrency}</Text>
        </TouchableOpacity>

      </View>
      

      <View style={styles.totalsContainer}>

        <View style={styles.totalsSection}>
          <Text style={styles.totalTitle}>Total Dues {localCurrency} </Text>
          <Text style={styles.totalDataText}>{Intl.NumberFormat("ar-de").format(totalDuesLC)}</Text>
        </View>

        <View style={styles.totalsSection}>
          <Text style={styles.totalTitle}>Total To be Paid {localCurrency} </Text>
          <Text style={styles.totalDataText}>{Intl.NumberFormat("ar-de").format(totalToBePaidLC)}</Text>
        </View>

        <View style={styles.totalsSection}>
          <Text style={styles.totalTitle}>Total Dues {controlCurrency} </Text>
          <Text style={styles.totalDataText}>{Intl.NumberFormat("ar-de").format(totalDuesCC)}</Text>
        </View>

        <View style={styles.totalsSection}>
          <Text style={styles.totalTitle}>Total To be Paid {controlCurrency} </Text>
          <Text style={styles.totalDataText}>{Intl.NumberFormat("ar-de").format(totalToBePaidCC)}</Text>
        </View>

      </View>

        <View style={styles.mainButtonsContainer}>

          {/* //to be replaced by payment later */}
        <TouchableOpacity
          style={styles.buttonContainer}
          onPress={() => {
            
            setPaymentVisible(true);

          }}
          >
          <Text style={styles.buttonText}>Save Receipt</Text> 
        </TouchableOpacity>

        <TouchableOpacity
          style={styles.buttonContainer}
           onPress={() => {
            ClearScreen();
             //alert('Reset Order');
           }}
          >
          <Text style={styles.buttonText}>Reset Receipt</Text>
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
  modalContainer: {
    width: '100%',
    height: '100%',
    backgroundColor: 'rgba(0,0,0,0.5)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  modalCard: {
    width: Sizing.windowsWidth * 0.90,
    height: Sizing.windowsHeight * 0.60,
    backgroundColor: 'white',
    borderRadius: 20,
    overflow: 'hidden',
    elevation: 10,
    borderWidth: 5,
    borderColor: Colors.brandBlue,
    alignItems: 'center',
    justifyContent: 'center',
  },
  loadDuesButtonContainer: {
    backgroundColor: Colors.brandBlue,
    alignItems: 'center',
    justifyContent: 'center',
    width: '100%',
    height: 25,
    elevation: 10,
    marginBottom: 10,
  },
  loadDuesButton: {
    backgroundColor: Colors.brandGreen,
    width: '100%',
    height: '100%',
    alignItems: 'center',
    justifyContent: 'center',
  },
  loadDuesButtonText: {
    color: 'white',
  },
  totalsContainer: {
    width: '100%',
    height: 100,
    backgroundColor: 'rgba(0,0,0,0.2)',
    alignItems: 'center',
    justifyContent: 'center',
    borderRadius: 5,
    //flexDirection: 'row',
    paddingRight: 10,
    //elevation : 10,
  },
  totalsSection: {
    //backgroundColor: 'red',
    width: '100%',
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-around'
  },
  totalTitle: {
    fontSize: FontSize(15),
    paddingRight: 10,
    fontWeight: 'bold',
    color: 'black',
    //backgroundColor: 'blue',
    width: '40%',
  },
  totalDataText: {
    fontSize: FontSize(20),
    fontWeight: 'bold',
    color: Colors.brandBlue,
    //backgroundColor: 'blue',
    width: '40%',
    textAlign: 'right'
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
    color: Colors.brandBlue,
    fontSize: FontSize(18),
    fontWeight: 'bold',
  },
});

export default ClientReceiptScreen;
