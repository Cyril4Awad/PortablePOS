import React from 'react';
import { useState, useEffect } from 'react';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { View, Text, FlatList, ScrollView, StyleSheet, Button, TouchableOpacity , TextInput, Keyboard } from 'react-native';
import SalesOrder from './SalesOrder';
import { FontSize } from '../../../variables/ResponsiveFont';
import SalesOrdersData from '../../../data/SalesOrdersData';
import LoadingData from '../../LoadingData';
import ServerError from '../../ServerError';

const SalesOrders = () => { 

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const [salesOrdersData, setSalesOrdersData] = useState([]);
  const [salesOrdersDataFiltered, setSalesOrdersDataFiltered] = useState([]);

  const [searchString, setSearchString] = useState('');

  useEffect(() => {

    SalesOrdersData.GetSalesOrders(
      { setSalesOrdersData },
      { setSalesOrdersDataFiltered },
      { setIsLoading },
      { setError }
    );

    return(() => {
      //Cleanup
      setSalesOrdersData([]);
      setSalesOrdersDataFiltered([]);
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

  var newData = salesOrdersData;
  const searchSplits = searchString.trim().split(' ')

  for (var i = 0; i < searchSplits.length; i++) {
    if (!searchSplits[i].trim() == '') {

      newData = newData.filter(item => {

        let searchResult = false;

        if (item.soSearchString.toString().toUpperCase().indexOf(searchSplits[i].trim().toUpperCase()) > -1) {

          searchResult = true;

        }

        return searchResult;

      });

    }
  };
  setSalesOrdersDataFiltered(newData);
};

  const SalesOrderItem = soData => {
    return (
      <SalesOrder
        soRowNumber={soData.item.soRowNumber}
        soKey={soData.item.soKey}
        soLocation={soData.item.soLocation}
        soRecID={soData.item.soRecID}
        soOfferReference={soData.item.soOfferReference}
        soDate={soData.item.soDate}
        soType={soData.item.soType}
        soClientCode={soData.item.soClientCode}
        soClientNumber={soData.item.soClientNumber}
        soClientName={soData.item.soClientName}
        soGeneralTotal={soData.item.soGeneralTotal}
        soCurrency={soData.item.soCurrency}
        soVATType={soData.item.soVATType}
        soSubjectRemarks={soData.item.soSubjectRemarks}
        soStatus={soData.item.soStatus}
      />
    );
  };

  const SearchTitle = () => {

    if (Sizing.isTablet) {
      return (
        <View style={styles.searchTitleContainer}>
          <Text style={styles.searchTitleText}>Search Sales Orders</Text>
        </View>
      )
    }
    else {
      return (<View></View>)
    }

  };

  return (
    <View style={styles.container}>

<View style={styles.searchContainer}>
          <SearchTitle />
          <TextInput
          style={styles.searchText}
          placeholder={Sizing.isTablet ? '' : 'Search Sales Orders'}
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
        data={salesOrdersDataFiltered}
        keyExtractor={(item) => item.soKey.toString()}
        renderItem={SalesOrderItem}
        refreshing={false}
        onRefresh={() => {
          SalesOrdersData.GetSalesOrders(
            { setSalesOrdersData },
            { setSalesOrdersDataFiltered },
            { setIsLoading },
            { setError }
          );
          setSearchString('');
        }
      }
      />

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
});

export default SalesOrders;