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
import PhysicalCountFilterPicker from '../components/lists/physicalCount/PhysicalCountFilterPicker';
import TransferData from '../data/TransferData';
import TransferDatabase from '../database/TransferDatabase';
import TransferSettings from '../configuration/TransferSettings';
import { CheckServer, CheckMachineName, CheckPageSize } from '../variables/TransferRules';

const CONNECTION_TIMEOUT_MS = 8000;

// Server, location we transfer from (the locations of the server, not the head office), machine name and download size
const TransferSettingsScreen = () => {

  const [server, setServer] = useState('');
  const [location, setLocation] = useState(null);
  const [machineName, setMachineName] = useState('');
  const [pageSize, setPageSize] = useState('');

  // The locations of the server : loaded by the connection test
  const [locations, setLocations] = useState([]);
  const [locationPickerVisible, setLocationPickerVisible] = useState(false);

  // { state: 'checking' | 'connected' | 'error', text }
  const [connection, setConnection] = useState(null);

  const TestConnection = (serverAddress) => {
    if (serverAddress === '') {
      setConnection(null);
      setLocations([]);
      return Promise.resolve([]);
    }
    setConnection({ state: 'checking', text: 'Checking...' });
    return TransferData.GetTransferLocations(serverAddress, CONNECTION_TIMEOUT_MS)
      .then(loadedLocations => {
        setLocations(loadedLocations);
        setConnection({ state: 'connected', text: `Connected - ${loadedLocations.length} Locations` });
        return loadedLocations;
      })
      .catch(error => {
        setLocations([]);
        setConnection({ state: 'error', text: `Not Connected - ${error.message}` });
        return [];
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

  const OpenLocations = async () => {
    const serverAddress = server.trim().toLowerCase();
    const errorMessage = CheckServer(serverAddress);
    if (errorMessage !== '') {
      alert(errorMessage);
      return;
    }
    const loadedLocations = locations.length > 0 ? locations : await TestConnection(serverAddress);
    if (loadedLocations.length === 0) {
      alert('The locations of the server cannot be read, check the server');
      return;
    }
    setLocationPickerVisible(true);
  };

  // The transfer being prepared belongs to the location : it cannot change while the transfer has lines
  const ChooseLocation = async (code) => {
    setLocationPickerVisible(false);
    const picked = locations.find(row => row.locationCode === code);
    if (!picked || picked.locationCode === location?.locationCode) return;

    const summary = await TransferDatabase.GetSummary();
    const info = await TransferDatabase.GetInfo();
    if (summary.lines > 0 && info && info.location !== picked.locationCode) {
      alert(`The transfer has ${summary.lines} lines from location ${info.location}, upload or clear them first`);
      return;
    }
    setLocation(picked);
  };

  const SaveSettings = async () => {

    const serverAddress = server.trim().toLowerCase();
    const machine = machineName.trim().toUpperCase();

    const errorMessage = CheckServer(serverAddress) || CheckMachineName(machine) || CheckPageSize(pageSize)
      || (location === null ? 'Choose the location to transfer from' : '');
    if (errorMessage !== '') {
      alert(errorMessage);
      return;
    }

    await TransferSettings.saveServer(serverAddress);
    await TransferSettings.saveLocation(location);
    await TransferSettings.saveMachineName(machine);
    await TransferSettings.savePageSize(parseInt(pageSize, 10));
    setServer(serverAddress);
    setMachineName(machine);
    TestConnection(serverAddress);

    const info = await TransferDatabase.GetInfo();
    alert(info && info.location !== location.locationCode
      ? `Settings Saved - Download the stock of location ${location.locationCode}`
      : 'Settings Saved');

  };

  return (
    <View style={styles.container}>

      <PhysicalCountFilterPicker
        visible={locationPickerVisible}
        title='Transfer From'
        options={locations.map(row => ({ code: row.locationCode, description: `${row.locationCode} - ${row.locationDescription}` }))}
        selected={location ? [location.locationCode] : []}
        multiple={false}
        onSubmit={(codes) => ChooseLocation(codes[0])}
        onCancel={() => setLocationPickerVisible(false)}
      />

      {!Sizing.isPortableScanner ?
        <ScreenHeader
          description='Server, location to transfer from and machine name of the transfer.'
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
            onChangeText={text => { setServer(text); setLocations([]); }} />

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
          value={location ? `${location.locationCode} - ${location.locationDescription}` : 'Not Selected'}
          onPress={OpenLocations}
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
