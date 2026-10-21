import React from 'react';
import { useState, useEffect } from 'react';
import { View, Text, ScrollView, StyleSheet, Image, FlatList, TextInput, TouchableOpacity, Modal} from 'react-native';
import Colors from '../../../variables/Colors';
import { FontSize } from '../../../variables/ResponsiveFont';

import ScreenTitle from '../../ScreenTitle';

import LoadingData from '../../LoadingData';
import ServerError from '../../ServerError';

import Items from './Items';

import ItemProductGroup from './ItemProductGroup';
import Sizing from '../../../variables/Sizing';
import StockData from '../../../data/StockData';
import ListFooter from '../../ListFooter';

import QRCodeScanner from 'react-native-qrcode-scanner';
import { RNCamera } from 'react-native-camera';

import Promotions from '../promotions/Promotions';

// searchFor = 'RO' (optional) : the product groups and items of the Receiving Order, supplierRecID : its supplier
// onSelectItem(itemRecID, itemReference, itemDescription, itemPrice) (optional) : called with the item picked
const ItemProductGroups = props => {

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const [itemProductGroupsData, setItemProductGroupsData] = useState([]);
  const [itemProductGroupsDataFiltered, setItemProductGroupsDataFiltered] = useState([]);

  const [searchString, setSearchString] = useState('');

  const [modalVisible, setModalVisible] = useState(false);

  const [selectedItemProductGroupCode, setSelectedItemProductGroupCode] = useState('');
  const [selectedItemProductGroupDesc, setSelectedItemProductGroupDesc] = useState('');


  const [barcodeScannerOpened, setBarcodeScannerOpened] = useState(false);

  const [promotionModalVisible, setPromotionModalVisible] = useState(false);

  useEffect(() => {

    StockData.GetItemProductGroups(
      {setItemProductGroupsData},
      {setItemProductGroupsDataFiltered},
      {setIsLoading},
      {setError},
      { searchFor: props.searchFor }
    );

    return(() => {
      //Cleanup
      setItemProductGroupsData([]);
      setItemProductGroupsDataFiltered([]);
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

    var newData = itemProductGroupsData;
    const searchSplits = searchString.trim().split(' ')

    for (var i = 0; i < searchSplits.length; i++) {
      if (!searchSplits[i].trim() == '') {

        newData = newData.filter(item => {

          let searchResult = false;

          if (item.itemProductGroupDesc.toString().toUpperCase().indexOf(searchSplits[i].trim().toUpperCase()) > -1) {

            searchResult = true;

          }

          return searchResult;

        });

      }
    };
    setItemProductGroupsDataFiltered(newData);
  };
 
  const ItemsSearch = () => {
    return (
      <Modal visible={modalVisible}>
        <Items 
          title={selectedItemProductGroupDesc}
          itemProductGroupCode={selectedItemProductGroupCode}
          itemPromotionCriteria=''
          searchString=''
          setSelectedItemRecID={props.setSelectedItemRecID}
          selectedItemQty={props.selectedItemQty}
          setSelectedItemQty={props.setSelectedItemQty}
          setSelectedItemReference={props.setSelectedItemReference}
          setSelectedItemDescription={props.setSelectedItemDescription}
          setSelectedItemVatType={props.setSelectedItemVatType}
          setSelectedItemVatPerc={props.setSelectedItemVatPerc}
          setSelectedItemPromotionCriteria={props.setSelectedItemPromotionCriteria}
          setStockScreenVisible={props.setStockScreenVisible}
          onSelectItem={props.onSelectItem}
          searchFor={props.searchFor}
          supplierRecID={props.supplierRecID}
          onCloseButtonPress={() => {setModalVisible(false)}} 
          />
      </Modal>
    )
  };

  const ItemProductGroupItem = itemProductGroupData => {
    return (
      <ItemProductGroup 
        itemProductGroupCode={itemProductGroupData.item.itemProductGroupCode} 
        itemProductGroupDesc={itemProductGroupData.item.itemProductGroupDesc}
        onPress={() => {
          setSelectedItemProductGroupCode(itemProductGroupData.item.itemProductGroupCode);
          setSelectedItemProductGroupDesc(itemProductGroupData.item.itemProductGroupDesc);
          setModalVisible(true);
        }} />
    );
  };

  const SearchTitle = () => {

    if (Sizing.isTablet) {
      return(
        <View style={styles.searchTitleContainer}>
          <Text style={styles.searchTitleText}>Search Product Groups</Text>
        </View>
      )
    }
    else {
      return(<View></View>)
    }

  };

  const ItemProductGroupsListFooter = () => {

    if (typeof props.setStockScreenVisible === 'undefined') {
      return(
        <View></View>
      )  
    }
    else {
      return(
        <ListFooter setSearchVisible={props.setStockScreenVisible}/>
      )
    };
    
  }

  return (
    <View style={styles.container}>

      <ScreenTitle
        title='Product Groups List'
      />    

      <ItemsSearch />

      <Modal visible={promotionModalVisible} style={styles.modal}>
        <Promotions  onCloseButtonPress={() => setPromotionModalVisible(false)}/>
      </Modal>

      {/* start */}
      <Modal visible={barcodeScannerOpened} 
                   style={styles.modal}>
                
                <View style={styles.cameraContainer}>
                    <View style={styles.camera}>

                        <QRCodeScanner
                            onRead={(e) => { onBarCodeRead(e) }}
                            flashMode={RNCamera.Constants.FlashMode.auto}
                            showMarker={true}
                            markerStyle={
                                {
                                    borderColor:Colors.prColor,
                                    borderRadius: 15,
                                }
                            }
                        />

                    </View>
                </View>

                <View style={styles.modalFooter}>
                    <TouchableOpacity
                        style={styles.modalCloseButton}
                        onPress={() => setBarcodeScannerOpened(false)}
                    >
                        <Text style={styles.modalCloseButtonText}>CANCEL</Text>
                    </TouchableOpacity>
                </View>

            </Modal>
{/* end */}

      <View style={styles.searchContainer}>

        <View style={styles.barcodeScanButtonContainer}>
          {/* <TouchableOpacity style={styles.barcodeScanButton} onPress={() => setBarcodeScannerOpened(true)} disabled={true}>
            <Image
              source={require('../../../assets/icons/barcode.png')}
              style={styles.barcodeScanButtonImage}
              resizeMode="contain"
            />
            <Text style={styles.barcodeScanButtonText}>Scan Barcode</Text>
            <Image
              source={require('../../../assets/icons/barcode.png')}
              style={styles.barcodeScanButtonImage}
              resizeMode="contain"
            />
          </TouchableOpacity> */}
        </View>

        <SearchTitle />

        <TextInput
            style={styles.searchText}
            placeholder={Sizing.isTablet ? '' : 'Search Product Groups'}
            maxLength={30}
            value={searchString}
            //autoFocus={true}
            onChangeText={queryText => {
              simpleSearch(queryText);
              setSearchString(queryText)}}
          />
      </View>

      <FlatList
        keyExtractor={(item, index) => item.itemProductGroupCode.toString()}
        data={itemProductGroupsDataFiltered}
        renderItem={ItemProductGroupItem}
        numColumns={Sizing.isTablet ? 4 : 1}
        style={{ width: '100%' , backgroundColor: Colors.bgColor }}
        contentContainerStyle={{ alignItems: 'center', justifyContent: 'center' }}
        refreshing={false}
        onRefresh={() => {
          StockData.GetItemProductGroups(
            { setItemProductGroupsData },
            { setItemProductGroupsDataFiltered },
            { setIsLoading },
            { setError },
            { searchFor: props.searchFor }
          );
          setSearchString('');}
        }
      />

      <View style={styles.promotionSectionContainer}>
        <TouchableOpacity onPress={() => setPromotionModalVisible(true)}>
          <Text style={styles.promotionButtonText}>View promotions</Text>
        </TouchableOpacity>
      </View>

      <ItemProductGroupsListFooter />

    </View>
  );
};

const styles = StyleSheet.create({
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
    height: Sizing.isTablet ? 60 : 60, //110 if barcode activated
    alignItems: Sizing.isTablet ? 'flex-end' : 'center',
    justifyContent: 'center',
    padding: 5,
    // paddingTop: 5,
    flexDirection: Sizing.isTablet ? 'row' : 'column',
  },
  barcodeScanButtonContainer: {
    //backgroundColor: 'red',
    width: Sizing.isTablet ? '20%' : '100%',
    height: Sizing.isTablet ? '100%' : 0, //50% if barcode activated
    alignItems: 'center',
    justifyContent: 'flex-end',
    marginBottom: Sizing.isTablet ? 0 : 5,
  },
  barcodeScanButton: {
    width: '100%',
    height: Sizing.isTablet? 45 : 38,
    borderRadius: 5,
    borderColor: 'silver',
    borderWidth: 1,
    alignItems: 'center',
    justifyContent: 'center',
    elevation: 10,
    backgroundColor: Colors.brandGreen,
    flexDirection: 'row',
  },
  barcodeScanButtonText: {
    color: 'white',
    fontSize: FontSize(23),
    paddingLeft: 10,
    paddingRight: 10,
    //backgroundColor: 'red',
  },
  barcodeScanButtonImage: {
    width: 40,
    height: '70%',
    overflow: 'hidden',
    resizeMode: 'contain',
    //backgroundColor: 'red',
  },
  searchTitleContainer: {
    width: '55%',
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
    height: 45,
    //borderRadius: 5,
    borderWidth: 1,
    borderColor: 'silver',
    textAlign: 'left',
    paddingLeft: 25,
    fontSize: FontSize(23),
    backgroundColor: 'white',
    color: 'black',
  },
//start
 //Barcode Scanner
 modal :{ 
  height : Sizing.WindowsHeight,
   alignItems: 'center', 
   justifyContent: 'center' 
},
modalHeaderContainer : {
  width: '100%',
  height: '8%',
  backgroundColor: 'rgba(0,0,0,0.1)',
  flexDirection:'row',
},
modalHeaderImageContainer: {
  height: '100%',
  width : '20%',
  alignItems:'center',
  justifyContent:'center',
},
modalHeaderImage: {
  resizeMode:'contain',
  width:'100%',
  height:'90%',
},
modalHeaderTextContainer: {
  height: '100%',
  width : '80%',
  alignItems:'flex-start',
  justifyContent:'center',
},
modalHeaderText: {
  fontSize: 15,
  fontWeight:'bold',
  color: Colors.prColor,
},
cameraContainer: {
  width: '100%',
  height: '85%',
  alignItems: 'center',
  justifyContent: 'center',
},
camera: {
  width: '90%',
  height: '90%',
  alignItems: 'center',
  justifyContent: 'center',
},
modalFooter: {
  alignItems: 'center',
  justifyContent: 'center',
  width: Sizing.WindowsWidth,
  height: '5%',
},
modalCloseButton: {
  backgroundColor: Colors.prColor,
  padding: 8,
  marginRight: 5,
  borderRadius: 5,
},
modalCloseButtonText: {
  fontWeight: 'bold',
  color: 'red'
},
promotionSectionContainer: {
  //backgroundColor: 'red',
  width: '100%',
  height: 30,
  alignItems: 'center',
  justifyContent: 'center',
},
promotionButtonText: {
  color: Colors.brandBlue,
  fontSize: FontSize(17),
  fontStyle: 'italic',
},
});

export default ItemProductGroups;
