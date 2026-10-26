import React from 'react';
import { useState, useEffect } from 'react';
import { View, Text, ScrollView, StyleSheet, Button, FlatList, TextInput, Modal, SectionList } from 'react-native';
import Colors from '../../../variables/Colors';
import { FontSize } from '../../../variables/ResponsiveFont';
import ModalTitle from '../../ModalTitle';

import LoadingData from '../../LoadingData';
import ServerError from '../../ServerError';

import Item from './Item';
import ItemDetails from './ItemDetails';
import Sizing from '../../../variables/Sizing';
import StockData from '../../../data/StockData';
import { ItemRecIDFromLink } from '../../../variables/ItemLink';

// searchFor = 'RO' (optional) : the items of the Receiving Order (FrmDO conditions), supplierRecID : its supplier
// embedded (optional) : shown inside another list (the Items side of ItemProductGroups), without its title bar
const Items = props => {

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const [itemsData, setItemsData] = useState([]);
  const [itemsDataFiltered, setItemsDataFiltered] = useState([]);

  const [searchString, setSearchString] = useState('');

  const [modalVisible, setModalVisible] = useState(false);
  const [selectedItem, setSelectedItem] = useState([]);

  const selectItemForParent = item => {
    props.setSelectedItemRecID(item.itemRecID);
    props.setSelectedItemReference(item.itemReference);
    props.setSelectedItemDescription(item.itemDescription);
    props.setSelectedItemVatType(item.itemVatType);
    props.setSelectedItemVatPerc(item.itemVatPerc);
    props.setSelectedItemPromotionCriteria(item.itemPromotionCriteria);
    props.setStockScreenVisible(false);
  };
 
  useEffect(() => {

    StockData.GetItems(
      props.itemProductGroupCode,
      props.itemPromotionCriteria,
      props.searchString,
      { setItemsData },
      { setItemsDataFiltered },
      { setIsLoading },
      { setError },
      { searchFor: props.searchFor, supplierRecID: props.supplierRecID }
    );

    return (() => {
      //Cleanup
      setItemsData([]);
      setItemsDataFiltered([]);
    })

  }, []);

  useEffect(() => {
    if (
      props.autoSelectSingleResult &&
      typeof props.setSelectedItemRecID !== 'undefined' &&
      !isLoading &&
      !error &&
      itemsDataFiltered.length === 1
    ) {
      selectItemForParent(itemsDataFiltered[0]);
    }
  }, [isLoading, itemsDataFiltered, error, props.autoSelectSingleResult]);

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

    // The QR of a label (link ending with the record id) : the item itself
    const linkRecID = ItemRecIDFromLink(searchString);
    if (linkRecID !== null) {
      setItemsDataFiltered(itemsData.filter(item => item.itemRecID.toString() === linkRecID));
      return;
    }

    var newData = itemsData;
    const searchSplits = searchString.trim().split(' ')

    for (var i = 0; i < searchSplits.length; i++) {
      if (!searchSplits[i].trim() == '') {

        newData = newData.filter(item => {

          let searchResult = false;

          if (item.itemSearchString.toString().toUpperCase().indexOf(searchSplits[i].trim().toUpperCase()) > -1) {

            searchResult = true;

          }

          return searchResult;

        });

      }
    };
    setItemsDataFiltered(newData);
  };

  const ItemItem = itemData => {
    return (
      <Item
        itemRecID={itemData.item.itemRecID}
        itemReference={itemData.item.itemReference}
        itemBarcode={itemData.item.itemBarcode}
        itemDescription={itemData.item.itemDescription}
        itemProductGroup={itemData.item.itemProductGroup}
        itemUnit={itemData.item.itemUnit}
        itemPrice={itemData.item.itemPrice}
        itemCurrency={itemData.item.itemCurrency}
        itemAvailableQty={itemData.item.itemAvailableQty}
        itemImageBase64={itemData.item.itemImageBase64}
        infoText={props.searchFor === 'RO' ? itemData.item.itemUnit : undefined}
        setSelectedItemQty={props.setSelectedItemQty}
        selectedItemQty={props.selectedItemQty}
        onPress={() => {
          if (typeof props.setSelectedItemRecID !== 'undefined') {
            selectItemForParent(itemData.item);
          }
          else if (typeof props.onSelectItem !== 'undefined') {
            props.setStockScreenVisible(false);
            props.onSelectItem(itemData.item.itemRecID, itemData.item.itemReference, itemData.item.itemDescription, itemData.item.itemPrice);
          }
          else {
            setSelectedItem(itemsDataFiltered.find(item =>
            {return (item.itemRecID == itemData.item.itemRecID)}
            ));
            setModalVisible(true);
          }
        }} 
        onLongPress={() => {
          if (typeof props.setSelectedItemRecID !== 'undefined') {
            setSelectedItem(itemsDataFiltered.find(item => 
              {return (item.itemRecID == itemData.item.itemRecID)}
              ));
              setModalVisible(true);
          }
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

  const ItemDetailsModal = () => {
    return (
      <Modal style={styles.modalStyle} visible={modalVisible} animationType='fade'>
        <ItemDetails 
          selectedItem={selectedItem}
          onCloseButtonPress={() => {setModalVisible(false)}} 
          />
      </Modal>
    )
  };

  return (
    <View style={props.embedded ? styles.embeddedContainer : styles.container}>

      {!props.embedded ?
        <ModalTitle
          title={'Items List - ' + props.title}
          onCloseButtonPress={props.onCloseButtonPress}
          clearData={() => {
            setItemsData([]);
            setItemsDataFiltered([]);
          }}
        /> :
        <></>}

      <ItemDetailsModal />

      <View style={styles.searchContainer}>
        <SearchTitle />
        <TextInput
          style={styles.searchText}
          placeholder={Sizing.isTablet ? '' : 'Search Items'}
          maxLength={200}
          value={searchString}
          //autoFocus={true}
          onChangeText={queryText => {
            simpleSearch(queryText);
            setSearchString(queryText)
          }}
        />
      </View>

      <FlatList
        keyExtractor={(item, index) => item.itemRecID.toString()}
        data={itemsDataFiltered}
        renderItem={ItemItem}
        numColumns={1}
        style={[{ width: '100%' , backgroundColor: Colors.bgColor }, props.embedded ? { flex: 1 } : null]}
        contentContainerStyle={{ alignItems: 'center', justifyContent: 'center' }}
        refreshing={false}
        onRefresh={() => {
          StockData.GetItems(
            props.itemProductGroupCode,
            props.itemPromotionCriteria,
            props.searchString,
            { setItemsData },
            { setItemsDataFiltered },
            { setIsLoading },
            { setError },
            { searchFor: props.searchFor, supplierRecID: props.supplierRecID }
          );
          setSearchString('');
        }
        }
      />

    </View>
  );
};

const styles = StyleSheet.create({
  // inside the Items side of ItemProductGroups : the whole height left, the list from the top
  embeddedContainer: {
    flex: 1,
    width: '100%',
  },
  container: {
    //backgroundColor: Colors.bgColor,
    //backgroundColor: 'yellow',
    //padding: 10,
    width: '100%',
    height: '100%',
    alignItems: 'center',
    justifyContent: 'center',
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
});

export default Items;
