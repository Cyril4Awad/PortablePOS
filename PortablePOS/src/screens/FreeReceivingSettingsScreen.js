import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Image } from 'react-native';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import ScreenHeader from '../components/ScreenHeader';
import MenuHeader from '../components/menu/MenuHeader';
import TextBoxField from '../components/controls/TextBoxField';
import TextBoxNumericField from '../components/controls/TextBoxNumericField';
import MainButton from '../components/controls/MainButton';
import FreeReceivingData from '../data/FreeReceivingData';
import FreeReceivingSettings from '../configuration/FreeReceivingSettings';
import DataField from '../components/controls/DataField';
import { CheckServer, CheckPageSize } from '../variables/FreeReceivingRules';
import { MachineNameMessage } from '../configuration/MachineName';

const CONNECTION_TIMEOUT_MS = 8000;

// Server, machine name, download size and remove check digit of Free Receiving
const FreeReceivingSettingsScreen = () => {

  const [server, setServer] = useState('');
  const [machineName, setMachineName] = useState('');
  const [pageSize, setPageSize] = useState('');
  const [removeCheckDigit, setRemoveCheckDigit] = useState(false);

  // { state: 'checking' | 'connected' | 'error', text }
  const [connection, setConnection] = useState(null);

  const TestConnection = (serverAddress) => {
    if (serverAddress === '') {
      setConnection(null);
      return;
    }
    setConnection({ state: 'checking', text: 'Checking...' });
    FreeReceivingData.GetReceivingInfo(serverAddress, CONNECTION_TIMEOUT_MS)
      .then(info => setConnection({ state: 'connected', text: `Connected - ${info.itemsCount} Items, ${info.suppliersCount} Suppliers` }))
      .catch(error => setConnection({ state: 'error', text: `Not Connected - ${error.message}` }));
  };

  useEffect(() => {
    (async () => {
      const savedServer = await FreeReceivingSettings.server();
      setServer(savedServer);
      setMachineName(await FreeReceivingSettings.machineName());
      setPageSize((await FreeReceivingSettings.pageSize()).toString());
      setRemoveCheckDigit(await FreeReceivingSettings.removeCheckDigit());
      TestConnection(savedServer);
    })().catch(error => alert(error.message));
  }, []);

  const SaveSettings = async () => {

    const serverAddress = server.trim().toLowerCase();
    const errorMessage = CheckServer(serverAddress) || CheckPageSize(pageSize);
    if (errorMessage !== '') {
      alert(errorMessage);
      return;
    }

    await FreeReceivingSettings.saveServer(serverAddress);
    await FreeReceivingSettings.savePageSize(parseInt(pageSize, 10));
    await FreeReceivingSettings.saveRemoveCheckDigit(removeCheckDigit);
    setServer(serverAddress);
    TestConnection(serverAddress);
    alert('Settings Saved');

  };

  return (
    <View style={styles.container}>

      {!Sizing.isPortableScanner ?
        <ScreenHeader
          description='Server, machine name and scanning options of free receiving.'
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

        <MenuHeader title='DEVICE' />

        <View style={styles.formContainer}>

          <View style={styles.fieldsRow}>
            <View style={styles.fieldHalf}>
              {/* The name of the device, changed in the Settings of the device (read only here) */}
              <DataField
                title='Machine Name'
                data={machineName === '' ? 'Not Valid' : machineName}
                toolTip='Device Name'
                onPress={() => alert(machineName === '' ? MachineNameMessage() : 'The machine name is the name of the device, change it in the Settings of the device : About phone > Device name.')}
              />
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

          <TouchableOpacity style={styles.checkContainer} onPress={() => setRemoveCheckDigit(!removeCheckDigit)}>
            <View style={styles.checkTextContainer}>
              <Text style={styles.checkTitleText}>Remove Check Digit</Text>
              <Text style={styles.checkDescriptionText}>A scanned barcode loses its last digit before the search</Text>
            </View>
            <Image
              source={removeCheckDigit ? require('../assets/icons/checked.png') : require('../assets/icons/unchecked.png')}
              style={styles.checkBoxImage}
              resizeMode='contain'
            />
          </TouchableOpacity>

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
  checkContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    minHeight: 50,
    paddingLeft: 5,
    paddingRight: 5,
  },
  checkTextContainer: {
    flex: 1,
  },
  checkTitleText: {
    color: Colors.brandBlue,
    fontSize: FontSize(17),
  },
  checkDescriptionText: {
    color: 'gray',
    fontSize: FontSize(13),
    fontStyle: 'italic',
  },
  checkBoxImage: {
    width: 30,
    height: 30,
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

export default FreeReceivingSettingsScreen;
