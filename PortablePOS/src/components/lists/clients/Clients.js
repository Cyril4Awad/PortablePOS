import React from 'react';
import { useState, useEffect } from 'react';
import { View, Text, ScrollView, StyleSheet, Button, FlatList, TextInput, Modal } from 'react-native';
import Colors from '../../../variables/Colors';
import ModalTitle from '../../ModalTitle';

import LoadingData from '../../LoadingData';
import ServerError from '../../ServerError';

import Client from './Client';
import ClientGeoLocation from './ClientGeoLocation';
import Sizing from '../../../variables/Sizing';
import ClientsData from '../../../data/ClientsData';
import { FontSize } from '../../../variables/ResponsiveFont';

import GeoLocationInfo from '../../../variables/GeoLocationInfo';
import ClientStatement from './ClientStatement';

const Clients = props => {

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const [clientsData, setClientsData] = useState([]);
  const [clientsDataFiltered, setClientsDataFiltered] = useState([]);

  const [searchString, setSearchString] = useState('');

  const [location, setLocation] = useState(null);

  const [selectedClientRecId, setSelectedClientRecId] = useState(0);
  const [selectedClientName, setSelectedClientName] = useState('');


  const [clientClientStatementModalVisible, setClientStatementModalVisible] = useState(false);
  const [geoLocationModalVisible, setGeoLocationModalVisible] = useState(false);

  const [noOfExpectedVisits, setNoOfExpectedVisits] = useState(0);
  const [noOfExecutedVisits, setNoOfExecutedVisits] = useState(0);

  useEffect(() => {

    console.log('after data loading')

    const visitedSum = clientsData.reduce((sum, record) => sum + (record.clientVisited === "Y" ? 1 : 0), 0);
    setNoOfExecutedVisits(visitedSum);
    setNoOfExpectedVisits(clientsData.length)

  }, [clientsData]);

  useEffect(() => {

    LoadClients();

    console.log('props.visitDate');
    console.log(props.visitDate);

    return(() => {
      //Cleanup
      setClientsData([]);
      setClientsDataFiltered([]);
      console.log('cleanupppppppppppppp222')
    })

  }, []);

  useEffect(() => {

    LoadClients();

    return(() => {
      //Cleanup
      setClientsData([]);
      setClientsDataFiltered([]);
    })

  }, [geoLocationModalVisible]);

  
  //While Loading Or Searching Data
  if (isLoading) {
    return (
      <LoadingData />
    );
  };

  //In case of error
  if (error) {
    return (
      <ServerError errorMessage={error.toString()} setError={setError} />
    );
  };


  const LoadClients = () => {
    try {

      setIsLoading(true);

      GeoLocationInfo.GetLocation().then((loc) => {
        console.log('1')
        setLocation(loc);
        ClientsData.GetClients(
          props.clientGroupType === 'CT' ? props.clientGroupCode : '',
          props.clientGroupType === 'CCR' ? props.clientGroupCode : '',
          props.searchString,
          loc ? loc.coords.latitude : -1,
          loc ? loc.coords.longitude : -1,
          { setClientsData },
          { setClientsDataFiltered },
          { setIsLoading },
          { setError },
          props.onlyHavingVisits !== true ? false : true,
          props.visitDate
        )
      }
      );
   
    } catch (error) {
      console.log(error)  
    }
  }

  const simpleSearch = searchString => {

    var newData = clientsData;
    const searchSplits = searchString.trim().split(' ')

    for (var i = 0; i < searchSplits.length; i++) {
      if (!searchSplits[i].trim() == '') {

        newData = newData.filter(item => {

          let searchResult = false;

          if (item.clientSearchString.toString().toUpperCase().indexOf(searchSplits[i].trim().toUpperCase()) > -1) {

            searchResult = true;

          }

          return searchResult;

        });

      }
    };
    setClientsDataFiltered(newData);
  };

  const VisitsSummary = () => {
    if (props.visitsSummary) {
      return (
        <View style={styles.TotalVisitedContainer}>
            <Text style={styles.TotalVisitedText}>Visited Clients : {noOfExecutedVisits} / {noOfExpectedVisits}</Text>
        </View>
      )
    }
    else  {
      return (<></>)
    }
  }

  const ClientItem = clientData => {
    return (
      <Client
        clientCode={clientData.item.clientCode}
        clientName={clientData.item.clientName}
        clientGroup={clientData.item.clientGroup}
        clientAccountExecutive={clientData.item.clientAccountExecutive}
        clientVisited={clientData.item.clientVisited}
        clientGeoStatus={clientData.item.clientGeoStatus}
        onPress={() => {
          if (typeof props.setSelectedClientRecID !== 'undefined') {

            // if (clientData.item.clientGeoStatus === 1) {
            //   props.setSelectedClientRecID(clientData.item.clientRecID);
            //   props.setSelectedClientCode(clientData.item.clientCode);
            //   props.setSelectedClientName(clientData.item.clientName);
            //   props.setSelectedClientPriceCat(clientData.item.clientPriceCat);
            //   props.setClientsScreenVisible(false);
            // }
            // else if (clientData.item.clientGeoStatus === -1) {
            //   alert('Client Geo Location is not defined')
            // }
            // else {
            //   alert('You are outside client area')
            // }

            props.setSelectedClientRecID(clientData.item.clientRecID);
            props.setSelectedClientCode(clientData.item.clientCode);
            props.setSelectedClientName(clientData.item.clientName);
            props.setSelectedClientPriceCat(clientData.item.clientPriceCat);
            props.setClientsScreenVisible(false);

          }
          else {
            setSelectedClientRecId(clientData.item.clientRecID)
            setSelectedClientName(clientData.item.clientName)
            setClientStatementModalVisible(true);
          }
        }}
        onLongPress={() => {
          if (typeof props.setSelectedClientRecID !== 'undefined') {
            setSelectedClientRecId(clientData.item.clientRecID)
            setSelectedClientName(clientData.item.clientName)
            setClientStatemeneModalVisible(true);
          }
        }}
        onGeoLocationPress={() => {
          setSelectedClientRecId(clientData.item.clientRecID)
          setSelectedClientName(clientData.item.clientName)
          setGeoLocationModalVisible(true);
        }}
      />
    );
  };

  const SearchTitle = () => {

    if (Sizing.isTablet) {
      return (
        <View style={styles.searchTitleContainer}>
          <Text style={styles.searchTitleText}>Search Clients</Text>
        </View>
      )
    }
    else {
      return (<View></View>)
    }

  };

  return (
    <View style={styles.container}>

      {/* Last Purchases */}
      <Modal
        style={styles.modalStyle}
        visible={clientClientStatementModalVisible}
        animationType='fade'
        transparent={true}
        presentationStyle='overFullScreen'>

        <View style={styles.modalContainer}>
          <View style={styles.modalCard}>
            <ClientStatement
              clientRecID={selectedClientRecId}
              clientName={selectedClientName}
              setClientStatementModalVisible={setClientStatementModalVisible}
            />
          </View>
        </View>

      </Modal>

      {/* Geo Location */}
      <Modal
        style={styles.modalStyle}
        visible={geoLocationModalVisible}
        animationType='fade'
        transparent={true}
        presentationStyle='overFullScreen'>

        <View style={styles.modalContainer}>
          <View style={styles.modalCard}>
            <ClientGeoLocation 
                clientRecID={selectedClientRecId} 
                clientName={selectedClientName} 
                setGeoLocationModalVisible={setGeoLocationModalVisible}
            />
          </View>
        </View>

      </Modal>

      <ModalTitle
        title={'Clients List - ' + props.clientGroupDesc}
        onCloseButtonPress={props.onCloseButtonPress}
        clearData={() => {
          setClientsData([]);
          setClientsDataFiltered([]);
        }}
      />

      <View style={styles.searchContainer}>
        <SearchTitle />
        <TextInput
          style={styles.searchText}
          placeholder={Sizing.isTablet ? '' : 'Search Clients'}
          maxLength={30}
          value={searchString}
          //autoFocus={true}
          onChangeText={queryText => {
            simpleSearch(queryText);
            setSearchString(queryText)
          }}
        />
      </View>

      <FlatList
        keyExtractor={(item, index) => item.clientRecID.toString()}
        data={clientsDataFiltered}
        renderItem={ClientItem}
        numColumns={1}
        style={{ width: '100%', backgroundColor: Colors.bgColor }}
        contentContainerStyle={{ alignItems: 'center', justifyContent: 'center' }}
        refreshing={false}
        onRefresh={() => {
          LoadClients();
          setSearchString('');
        }
        }
      />

      <VisitsSummary />

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
  TotalVisitedContainer: {
    backgroundColor: Colors.brandGreen,
    width: '100%',
    height: 30,
    alignItems: 'center',
    justifyContent: 'center'
  },
  TotalVisitedText: {
    fontSize: FontSize(18),
    color: 'white'
  }
});

export default Clients;
