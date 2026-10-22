import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView } from 'react-native';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import ScreenHeader from '../components/ScreenHeader';
import MenuHeader from '../components/menu/MenuHeader';
import TextBoxField from '../components/controls/TextBoxField';
import TextBoxNumericField from '../components/controls/TextBoxNumericField';
import ChoiceField from '../components/controls/ChoiceField';
import MainButton from '../components/controls/MainButton';
import TransferData from '../data/TransferData';
import TransferDatabase from '../database/TransferDatabase';
import TransferSettings from '../configuration/TransferSettings';
import { CheckServer, CheckMachineName, CheckPageSize } from '../variables/TransferRules';

const CONNECTION_TIMEOUT_MS = 8000;

// Server, machine name and download size. The location we transfer from is the one of the server (LocCode of its Web.config),
// read by the connection test, it cannot be changed
const TransferSettingsScreen = () => {

  const [server, setServer] = useState('');
  const [location, setLocation] = useState(null);
  const [machineName, setMachineName] = useState('');
  const [pageSize, setPageSize] = useState('');

  // { state: 'checking' | 'connected' | 'error', text }
  const [connection, setConnection] = useState(null);

  const TestConnection = (serverAddress) => {
    if (serverAddress === '') {
      setConnection(null);
      return Promise.resolve(null);
    }
    setConnection({ state: 'checking', text: 'Checking...' });
    return TransferData.GetTransferLocations(serverAddress, CONNECTION_TIMEOUT_MS)
      .then(loadedLocations => {
        const current = loadedLocations.find(row => Number(row.isCurrent) === 1);
        if (!current) {
          setConnection({ state: 'error', text: 'Connected - The location of the server is not a transfer location' });
          return null;
        }
        const serverLocation = { locationCode: current.locationCode, locationDescription: current.locationDescription };
        setLocation(serverLocation);
        setConnection({ state: 'connected', text: `Connected - ${loadedLocations.length} Locations` });
        return serverLocation;
      })
      .catch(error => {
        setConnection({ state: 'error', text: `Not Connected - ${error.message}` });
        return null;
      });
  };

  useEffect(() => {
    (async () => {
      const savedServer = await TransferSettings.server();
      setServer(savedServer);
      setLocation(await TransferSettings.location());
      setMachineName(await TransferSettings.machineName());
      setPageSize((await TransferSettings.pageSize()).toString());
      TestConnection(savedServer);
    })().catch(error => alert(error.message));
  }, []);

  const SaveSettings = async () => {

    const serverAddress = server.trim().toLowerCase();
    const machine = machineName.trim().toUpperCase();

    const errorMessage = CheckServer(serverAddress) || CheckMachineName(machine) || CheckPageSize(pageSize);
    if (errorMessage !== '') {
      alert(errorMessage);
      return;
    }

    // The location of the server (it may be another server than the one saved)
    const serverLocation = await TestConnection(serverAddress);
    if (serverLocation === null) {
      alert('The location of the server cannot be read, check the server');
      return;
    }

    // The transfer being prepared belongs to its location : it cannot change while the transfer has lines
    const summary = await TransferDatabase.GetSummary();
    const info = await TransferDatabase.GetInfo();
    if (summary.lines > 0 && info && info.location !== serverLocation.locationCode) {
      alert(`The transfer has ${summary.lines} lines from location ${info.location}, upload or clear them before changing the server`);
      return;
    }

    await TransferSettings.saveServer(serverAddress);
    await TransferSettings.saveLocation(serverLocation);
    await TransferSettings.saveMachineName(machine);
    await TransferSettings.savePageSize(parseInt(pageSize, 10));
    setServer(serverAddress);
    setMachineName(machine);

    alert(info && info.location !== serverLocation.locationCode
      ? `Settings Saved - Download the stock of location ${serverLocation.locationCode}`
      : 'Settings Saved');

  };

  return (
    <View style={styles.container}>

      {!Sizing.isPortableScanner ?
        <ScreenHeader
          description='Server and machine name of the transfer, the location is the one of the server.'
          imageSource={require('../assets/icons/settings.png')}
        /> :
        <></>}

      {/* Scrolls only when the keyboard is open on a small screen */}
      <ScrollView keyboardShouldPersistTaps='handled'>

        <MenuHeader title='SERVER' />

        <View style={styles.formContainer}>

          <TextBoxField
            title='Server (without http://)'
            value={server}
            maxLength={100}
            multiline={false}
            numberOfLines={1}
            autoCapitalize='none'
            autoCorrect={false}
            onChangeText={setServer} />

          {connection ?
            <Text numberOfLines={1} style={[styles.connectionText, {
              color: connection.state === 'connected' ? Colors.brandGreen : connection.state === 'error' ? 'red' : 'gray'
            }]}>
              {connection.text}
            </Text> :
            <></>}

        </View>

        <ChoiceField
          title='Transfer From Location'
          value={location ? `${location.locationCode} - ${location.locationDescription}` : 'Test the connection'}
          style={{ borderRightWidth: 0, borderTopWidth: 1 }}
        />

        <MenuHeader title='DEVICE' />

        <View style={styles.formContainer}>

          <View style={styles.fieldsRow}>
            <View style={styles.fieldHalf}>
              <TextBoxField
                title='Machine Name'
                value={machineName}
                maxLength={5}
                multiline={false}
                numberOfLines={1}
                autoCapitalize='characters'
                autoCorrect={false}
                onChangeText={setMachineName} />
            </View>
            <View style={styles.fieldHalf}>
              <TextBoxNumericField
                title='Download Size'
                value={pageSize}
                maxLength={4}
                multiline={false}
                numberOfLines={1}
                onChangeText={setPageSize} />
            </View>
          </View>

          <View style={styles.buttonsContainer}>
            <MainButton title='Save Settings' onPress={() => SaveSettings()} />
            <MainButton title='Test Connection' onPress={() => TestConnection(server.trim().toLowerCase())} />
          </View>

        </View>

      </ScrollView>

    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    backgroundColor: Colors.bgColor,
    height: '100%',
  },
  formContainer: {
    padding: 10,
  },
  connectionText: {
    fontSize: FontSize(14),
    fontWeight: 'bold',
    marginTop: -4,
    paddingLeft: 5,
  },
  fieldsRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  fieldHalf: {
    width: '48.5%',
  },
  buttonsContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    alignItems: 'center',
    justifyContent: Sizing.isTablet ? 'flex-end' : 'space-between',
    width: '100%',
    paddingTop: 10,
  },
});

export default TransferSettingsScreen;
