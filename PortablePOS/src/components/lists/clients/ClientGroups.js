import React from 'react';
import { useState, useEffect } from 'react';
import { View, Text, ScrollView, StyleSheet, Button, FlatList, TextInput, Modal } from 'react-native';
import Colors from '../../../variables/Colors';

import ScreenTitle from '../../ScreenTitle';

import LoadingData from '../../LoadingData';
import ServerError from '../../ServerError';

import Clients from './Clients';

import ClientGroup from './ClientGroup';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';
import ClientsData from '../../../data/ClientsData';
import ListFooter from '../../ListFooter';

import SwitchSelector from 'react-native-switch-selector';

const ClientGroups = props => {

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const [clientGroupsData, setClientGroupsData] = useState([]);
  const [clientGroupsDataFiltered, setClientGroupsDataFiltered] = useState([]);

  const [searchString, setSearchString] = useState('');

  const [modalVisible, setModalVisible] = useState(false);

  const [selectedClientGroupType, setSelectedClientGroupType] = useState('CT'); // CT: Client Types - CCR : Client Country Region
  const [selectedClientGroupCode, setSelectedClientGroupCode] = useState('');
  const [selectedClientGroupDesc, setSelectedClientGroupDesc] = useState('');

  useEffect(() => {

    ClientsData.GetClientGroups(
      selectedClientGroupType, 
      { setClientGroupsData },
      { setClientGroupsDataFiltered },
      { setIsLoading },
      { setError }
    );

    return (() => {
      //Cleanup
      setClientGroupsData([]);
      setClientGroupsDataFiltered([]);
    })

  }, [selectedClientGroupType]);

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

    var newData = clientGroupsData;
    const searchSplits = searchString.trim().split(' ')

    for (var i = 0; i < searchSplits.length; i++) {
      if (!searchSplits[i].trim() == '') {

        newData = newData.filter(item => {

          let searchResult = false;

          if (item.clientGroupDesc.toString().toUpperCase().indexOf(searchSplits[i].trim().toUpperCase()) > -1) {

            searchResult = true;

          }

          return searchResult;

        });

      }
    };
    setClientGroupsDataFiltered(newData);
  };
 
  const ClientsSearch = () => {
    return (
      <Modal visible={modalVisible}>
        <Clients 
          clientGroupType={selectedClientGroupType}
          clientGroupCode={selectedClientGroupCode}
          clientGroupDesc={selectedClientGroupDesc}
          searchString=''
          setSelectedClientRecID={props.setSelectedClientRecID}
          setSelectedClientCode={props.setSelectedClientCode}
          setSelectedClientName={props.setSelectedClientName}
          setSelectedClientPriceCat={props.setSelectedClientPriceCat}
          setClientsScreenVisible={props.setClientsScreenVisible}
          onlyHavingVisits={props.onlyHavingVisits}
          visitDate={null}
          onCloseButtonPress={() => {setModalVisible(false)}} />
      </Modal>
    )
  };
 
  const ClientGroupItem = clientGroupData => {
    return (
      <ClientGroup
      clientGroupType={clientGroupData.item.clientGroupType}
      clientGroupCode={clientGroupData.item.clientGroupCode}
      clientGroupDesc={clientGroupData.item.clientGroupDesc}
        onPress={() => {
          setSelectedClientGroupType(clientGroupData.item.clientGroupType);
          setSelectedClientGroupCode(clientGroupData.item.clientGroupCode);
          setSelectedClientGroupDesc(clientGroupData.item.clientGroupDesc);
          setModalVisible(true);
        }}
      />
    );
  };

  const SearchTitle = () => {

    if (Sizing.isTablet) {
      return (
        <View style={styles.searchTitleContainer}>
          <Text style={styles.searchTitleText}>Search {selectedClientGroupType === 'CT' ? 'Client Types' : 'Country / Regions'}</Text>
        </View>
      )
    }
    else {
      return (<View></View>)
    }

  };

  const ClientGroupsListFooter = () => {

    if (typeof props.setClientsScreenVisible === 'undefined') {
      return(
        <View></View>
      )  
    }
    else {
      return(
        <ListFooter setSearchVisible={props.setClientsScreenVisible}/>
      )
    };
    
  }

  return (
    <View style={styles.container}>

      <ScreenTitle
        title={selectedClientGroupType === 'CT' ? 'Search Client Types' : 'Search Country / Regions'}
      />

      <ClientsSearch />

      <View style={styles.searchContainer}>
        <SearchTitle />
        <TextInput
          style={styles.searchText}
          placeholder={Sizing.isTablet ? '' : selectedClientGroupType === 'CT' ? 'Search Client Types' : 'Search Country / Regions'}
          maxLength={30}
          value={searchString}
          //autoFocus={true}
          onChangeText={queryText => {
            simpleSearch(queryText);
            setSearchString(queryText);
          }}
        />
      </View>

      <FlatList
        keyExtractor={(item, index) => item.clientGroupCode.toString()}
        data={clientGroupsDataFiltered}
        renderItem={ClientGroupItem}
        numColumns={Sizing.isTablet ? 5 : 2}
        style={{ width: '100%', backgroundColor: Colors.bgColor }}
        contentContainerStyle={{ alignItems: 'center', justifyContent: 'center' }}
        refreshing={false}
        onRefresh={() => {
          ClientsData.GetClientGroups(
            selectedClientGroupType,
            { setClientGroupsData },
            { setClientGroupsDataFiltered },
            { setIsLoading },
            { setError }
          );
          setSearchString('');
        }
        }
      />

      <View style={styles.switchContainer}>
        <SwitchSelector
          initial={selectedClientGroupType === 'CT' ? 0 : 1}
          onPress={(val) => {setSelectedClientGroupType(val)}}
          textColor={Colors.brandBlue}
          selectedColor='white'
          buttonColor={Colors.brandBlue}
          borderColor={Colors.brandBlue}
          hasPadding={true}
          disableValueChangeOnPress={true}
          options={[
            { label: "Client Type", value: "CT" }, //images.feminino = require('./path_to/assets/img/feminino.png')
            { label: "Country / Region", value: "CCR" } //images.masculino = require('./path_to/assets/img/masculino.png')
          ]}
        />
      </View>

      <ClientGroupsListFooter />

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
    height: 60,
    alignItems: Sizing.isTablet ? 'flex-end' : 'center',
    justifyContent: 'center',
    padding: 5,
    paddingTop: 10,
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
  switchContainer: {
    backgroundColor: Colors.bgColor,
    width: '100%',
    padding: 5,
    alignItems: 'center',
    justifyContent: 'center',
  }
});

export default ClientGroups;
