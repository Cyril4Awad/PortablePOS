import React from 'react';
import { useState, useEffect } from 'react';
import { View, Text, ScrollView, StyleSheet, Button, FlatList, Modal } from 'react-native';
import ClientGroups from '../components/lists/clients/ClientGroups';
import Clients from '../components/lists/clients/Clients';
import { Calendar, CalendarList, Agenda } from 'react-native-calendars';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import LoadingData from '../components/LoadingData';
import ServerError from '../components/ServerError';
import GeoLocationInfo from '../variables/GeoLocationInfo';
import ClientsData from '../data/ClientsData';

const ClientsVisitsScreen = props => {

  const [clientsData, setClientsData] = useState([]);
  const [clientsDataFiltered, setClientsDataFiltered] = useState([]);

  const [markedDates, setMarkedDates] = useState({});
  const [modalVisible, setModalVisible] = useState(false);
  const [visitDate, setVisitDate] = useState(null);

  const [location, setLocation] = useState(null);

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const currentYear = new Date().getFullYear();
  const firstDayOfYear = new Date(currentYear, 0, 2); // January 1st of the current year
  const lastDayOfYear = new Date(currentYear, 11, 32); // December 31st of the current year

  // Format the dates to 'YYYY-MM-DD' format
  const formattedFirstDay = firstDayOfYear.toISOString().split('T')[0];
  const formattedLastDay = lastDayOfYear.toISOString().split('T')[0];

  const [currentMonth, setCurrentMonth] = useState('');

  useEffect(() => {
    // To fix rendering issue in March 2025 with duplicate date (29th), we can ensure correct date range.
    const today = new Date();
    const formattedMonth = today.getMonth() + 1; // Month is 0-indexed, so add 1
    const formattedYear = today.getFullYear();

    setCurrentMonth(`${formattedYear}-${formattedMonth < 10 ? `0${formattedMonth}` : formattedMonth}`);
  }, []);

  useEffect(() => {

    LoadClients();

    return(() => {
      //Cleanup
      setClientsData([]);
      setClientsDataFiltered([]);
      console.log('cleanupppppppppppppp222')
    })

  }, []);

  const processVisitData = (data) => {
    console.log('process visit data')
    let markedDates = {};
    data.forEach((visit) => {
      markedDates[visit.dr_Date] = { marked: true };
    });
    return markedDates;
  };

  useEffect(() => {
    //console.log(clientsData)
    if (clientsData.length > 0) {
      setMarkedDates(processVisitData(clientsData));
    }
  }, [ clientsData ]);



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

  const convertDate = (date) => {
    if (date === null) return; 
    const [year, month, day] = date.split('-');
    return `${day}/${month}/${year}`;
  };

  const LoadClients = () => {
    try {

      setIsLoading(true);

      GeoLocationInfo.GetLocation().then((loc) => {
        console.log('1')
        setLocation(loc);
        ClientsData.GetClients(
          '',
          '',
          '',
          loc ? loc.coords.latitude : -1,
          loc ? loc.coords.longitude : -1,
          { setClientsData },
          { setClientsDataFiltered },
          { setIsLoading },
          { setError },
          true,
          null
        )
      }
      );
   
    } catch (error) {
      console.log(error)  
    }
  }







  const onDayPress = (day) => {
    console.log(day);
    const selectedClients = clientsData.filter(visit => visit.dr_Date === day.dateString);
    if (selectedClients.length > 0) {
      setVisitDate(day.dateString);
      setModalVisible(true);
    }
  };

  return (
    <View style={styles.container}>

      <Modal visible={modalVisible}>
        <Clients
          // clientGroupType={selectedClientGroupType}
          // clientGroupCode={selectedClientGroupCode}
          clientGroupDesc={convertDate(visitDate)}
          searchString=''
          // setSelectedClientRecID={props.setSelectedClientRecID}
          // setSelectedClientCode={props.setSelectedClientCode}
          // setSelectedClientName={props.setSelectedClientName}
          // setSelectedClientPriceCat={props.setSelectedClientPriceCat}
          // setClientsScreenVisible={props.setClientsScreenVisible}
          onlyHavingVisits={true}
          visitDate={visitDate}
          visitsSummary={true}
          onCloseButtonPress={() => { setModalVisible(false) }} />
      </Modal>

      <CalendarList
        current={currentMonth}
        firstDay={1}
        minDate={formattedFirstDay} // Min date for March 2025
        maxDate={formattedLastDay} // Max date for March 2025
        style={{
          borderWidth: 0,
          borderColor: 'gray',
          width: Sizing.windowsWidth,
          height: '100%'
        }}
        onDayPress={onDayPress}
        markedDates={markedDates}
        theme={{
          backgroundColor: Colors.bgColor,
          calendarBackground: Colors.bgColor,
          textSectionTitleColor: Colors.brandGreen,
          // selectedDayBackgroundColor: Colors.brandBlue,
          // selectedDayTextColor: 'white',
          todayTextColor: 'red',
          dayTextColor: Colors.brandBlue,
          textDisabledColor: 'silver'
        }}
      />
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    width: '100%',
    height: '100%',
    alignItems: 'center',
    justifyContent: 'center'
  }
});

export default ClientsVisitsScreen;