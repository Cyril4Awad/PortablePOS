import React from 'react';
import { useState, useEffect } from 'react';
import { View, StyleSheet, Text, FlatList, TextInput, Image, TouchableOpacity, Modal, Button, Keyboard, Alert } from 'react-native';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';
import SalesOrderItem from './SalesOrderItem';
import ModalTitle from '../../ModalTitle';
import LoadingData from '../../LoadingData';
import ServerError from '../../ServerError';
import SalesOrdersData from '../../../data/SalesOrdersData';
import SalesOrderScanner from './SalesOrderScanner';

const SalesOrderItems = props => {

  const [modalVisible, setModalVisible] = useState(false);

  const [isLoading, setIsLoading] = useState(false);
  const [isLoadingFake, setIsLoadingFake] = useState(false);
  const [error, setError] = useState(null);

  const [salesOrdersItems, setSalesOrderItems] = useState([]);
  const [salesOrdersItemsFiltered, setSalesOrderItemsFiltered] = useState([]);

  const [searchString, setSearchString] = useState('');

  useEffect(() => {

    SalesOrdersData.GetSalesOrderItems(
      props.selectedSoRecID.toString(),
      { setSalesOrderItems },
      { setSalesOrderItemsFiltered },
      { setIsLoading },
      { setError },
      true
    );

    return(() => {
      //Cleanup
      setSalesOrderItems([]);
      setSalesOrderItemsFiltered([]);
  })

  }, []);

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

  const simpleSearch = searchString => {

    var newData = salesOrdersItems;
    const searchSplits = searchString.trim().split(' ')

    for (var i = 0; i < searchSplits.length; i++) {
      if (!searchSplits[i].trim() == '') {

        newData = newData.filter(item => {

          let searchResult = false;

          if (item.sodSearchString.toString().toUpperCase().indexOf(searchSplits[i].trim().toUpperCase()) > -1) {

            searchResult = true;

          }

          return searchResult;

        });

      }
    };
    setSalesOrderItemsFiltered(newData);
  };

  const OrderItem = soData => {
    return (

      <SalesOrderItem
        sodRowNumber={soData.item.sodRowNumber}
        sodKey={soData.item.sodKey}
        sodLocation={soData.item.sodLocation}
        sodHRecid={soData.item.sodHRecid}
        sodCounter={soData.item.sodCounter}
        sodItemRecID={soData.item.sodItemRecID}
        sodItemReference={soData.item.sodItemReference}
        sodItemDescription={soData.item.sodItemDescription}
        sodQtyOffered={soData.item.sodQtyOffered}
        sodQtyDelivered={soData.item.sodQtyDelivered}
        sodQtyInvoiced={soData.item.sodQtyInvoiced}
        sodQtyNeeded={soData.item.sodQtyNeeded}
        sodQtyPrepared={soData.item.sodQtyPrepared}
        sodQtyPending={soData.item.sodQtyPending}
        sodUnit={soData.item.sodUnit}
        sodRemarks={soData.item.sodRemarks}
        refreshSalesOrderItems={() => {
          SalesOrdersData.GetSalesOrderItems(
            props.selectedSoRecID.toString(),
            { setSalesOrderItems },
            { setSalesOrderItemsFiltered },
            { setIsLoading },
            { setError },
            false
          );
        }}
      />

    );
  };

  const SearchTitle = () => {

    if (Sizing.isTablet) {
      return (
        <View style={styles.searchTitleContainer}>
          <Text style={styles.searchTitleText}>Search Items</Text>
        </View>
      )
    }
    else {
      return (<View></View>)
    }

  };

  return (

    <View style={styles.container}>

      <Modal visible={modalVisible}
        transparent={true}
        presentationStyle="overFullScreen"
      >
        <View style={styles.modalContainer}>
          <View style={styles.modalCard}>

            <SalesOrderScanner
              setModalVisible={setModalVisible}
              selectedHRecid={props.selectedSoRecID}
              refreshSalesOrderItems={() => {
                SalesOrdersData.GetSalesOrderItems(
                  props.selectedSoRecID.toString(),
                  { setSalesOrderItems },
                  { setSalesOrderItemsFiltered },
                  { setIsLoading },
                  { setError },
                  false
                );
              }}
            />

          </View>
        </View>
      </Modal>

      <ModalTitle
        title={props.selectedSoOfferReference}
        onCloseButtonPress={() => props.setModalVisible(false)}
        clearData={() => {
          setSalesOrderItems([]);
          SalesOrdersData.CloseSalesOrder(props.selectedSoRecID)
        }}
      />

      <View style={styles.dataClientContainer}>
        <Text numberOfLines={1} style={styles.dataClientText}>{props.selectedSoClientCode} / {props.selectedSoClientNumber} - {props.selectedSoClientName}</Text>
      </View>

      <View style={styles.searchContainer}>
        <SearchTitle />
        <TextInput
          style={styles.searchText}
          placeholder={Sizing.isTablet ? '' : 'Search Items'}
          maxLength={30}
          value={searchString}
          showSoftInputOnFocus={!Sizing.isPortableScanner}
          onPressIn={() => {
            if (Sizing.isPortableScanner) {
              Keyboard.dismiss();
            }
          }}
          //autoFocus={true}
          onChangeText={queryText => {
            simpleSearch(queryText);
            setSearchString(queryText)
          }}
        />
      </View>

      <FlatList
        data={salesOrdersItemsFiltered}
        keyExtractor={(item) => item.sodKey.toString()}
        renderItem={OrderItem}
        refreshing={false}
        onRefresh={() => {
          SalesOrdersData.GetSalesOrderItems(
            props.selectedSoRecID.toString(),
            { setSalesOrderItems },
            { setSalesOrderItemsFiltered },
            { setIsLoading },
            { setError },
            true
          );
          setSearchString('');
        }
        }
      />

      <View style={styles.scanItemsContainer}>

        <TouchableOpacity style={styles.scanItemsButton} onPress={() => {

          Alert.alert('Ready for Invoicing', 'Do you want to send the sales order to POS ?', [
            {
              text: 'No',
              onPress: () => { },
              style: 'cancel',
            },
            {
              text: 'Yes', onPress: () => {

                SalesOrdersData.SendSalesOrderToPOS(props.selectedSoRecID.toString());
                props.setModalVisible(false);
                setSalesOrderItems([]);
                SalesOrdersData.CloseSalesOrder(props.selectedSoRecID);

              }
            },
          ]);

        }}>
          <Text style={styles.scanItemsButtonText}>Ready for Invoicing</Text>
        </TouchableOpacity>

        <TouchableOpacity style={styles.scanItemsButton} onPress={() => setModalVisible(true)}>
          <Text style={styles.scanItemsButtonText}>Scan Items</Text>
        </TouchableOpacity>

      </View>

    </View>

  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.buttonBackColor1,
    padding: 0,
    //alignItems: 'center',
    justifyContent: 'center'
  },
  modalContainer: {
    width: '100%',
    height: '100%',
    backgroundColor: 'rgba(0,0,0,0.5)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  modalCard: {
    width: Sizing.isPortableScanner ? Sizing.windowsWidth * 0.9 : Sizing.windowsWidth * 0.9,
    height: Sizing.isPortableScanner ? Sizing.windowsHeight * 0.8 : Sizing.windowsHeight * 0.6,
    backgroundColor: 'white',
    borderRadius: 20,
    overflow: 'hidden',
    elevation: 10,
    borderWidth: 5,
    borderColor: Colors.brandBlue,
    alignItems: 'center',
    justifyContent: 'center',
  },
  dataClientContainer: {
    backgroundColor: Colors.brandBlue,
    alignItems: 'center',
    justifyContent: 'center',
    width: '100%',
    height: 60,
    padding: 5,
  },
  dataClientText: {
    fontSize: FontSize(26),
    color: Colors.brandGreen,
    fontWeight: 'bold',
  },
  searchContainer: {
    //backgroundColor: Colors.brandGreen,
    width: '100%',
    height: 60,
    alignItems: Sizing.isTablet ? 'flex-end' : 'center',
    justifyContent: 'center',
    padding: 5,
    paddingTop: 5,
    flexDirection: 'row',
  },
  searchTitleContainer: {
    width: '75%',
    height: '100%',
    alignItems: 'flex-end',
    justifyContent: 'center',
    paddingRight: 10,
  },
  searchTitleText: {
    fontSize: FontSize(24),
    //backgroundColor: 'red',
  },
  searchText: {
    width: Sizing.isTablet ? '25%' : '100%',
    height: '100%',
    //borderRadius: 5,
    borderWidth: 1,
    borderColor: 'silver',
    textAlign: 'left',
    paddingLeft: 25,
    fontSize: FontSize(23),
    backgroundColor: 'white',
    color: 'black',
  },
  scanItemsContainer: {
    backgroundColor: Colors.brandBlue,    
    height: 50,
    width: '100%',
    alignItems: 'center',
    justifyContent: 'space-around',
    flexDirection: 'row',
    padding: 10,
  },
  scanItemsButton: {  
    //backgroundColor: 'yellow',
    width: '40%',
    height: '100%',
    alignItems: 'center',
    justifyContent: 'center',
  },
  scanItemsButtonText: {
    fontSize : FontSize(18),
    color: 'white',
  },
});

export default SalesOrderItems;
