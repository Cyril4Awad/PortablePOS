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
import PhysicalCountDatabase from '../database/PhysicalCountDatabase';
import PhysicalCountData from '../data/PhysicalCountData';
import PhysicalCountSettings from '../configuration/PhysicalCountSettings';
import DataField from '../components/controls/DataField';
import { CheckLocation, CheckServer, CheckPageSize } from '../variables/PhysicalCountRules';
import { MachineNameMessage } from '../configuration/MachineName';
import MessageBox from '../components/MessageBox';

// Each location has its own server (the location can be on another server), the selected one is used by the physical count
const PhysicalCountSettingsScreen = props => {

  const [profiles, setProfiles] = useState([]);

  // Location form, profileID 0 = new location
  const [profileID, setProfileID] = useState(0);
  const [location, setLocation] = useState('');
  const [server, setServer] = useState('');
  const [selected, setSelected] = useState(true);

  const [machineName, setMachineName] = useState('');
  const [pageSize, setPageSize] = useState('');

  // Connection test of each location : { [profileID]: { state: 'checking' | 'connected' | 'error', text } }
  const [connections, setConnections] = useState({});

  // Tested automatically every time the list is loaded (opening the screen, selecting, saving or deleting a location)
  const TestConnections = (loadedProfiles) => {
    loadedProfiles.forEach(profile => {
      setConnections(current => ({ ...current, [profile.profileID]: { state: 'checking', text: 'Checking...' } }));
      PhysicalCountData.CheckPhysicalCountLocation(profile.server, profile.location)
        .then(locationDescription => setConnections(current => ({
          ...current, [profile.profileID]: { state: 'connected', text: `Connected - ${locationDescription}` }
        })))
        .catch(error => setConnections(current => ({
          ...current, [profile.profileID]: { state: 'error', text: `Not Connected - ${error.message}` }
        })));
    });
  };

  const LoadProfiles = () => {
    PhysicalCountDatabase.GetProfiles()
      .then(loadedProfiles => {
        setProfiles(loadedProfiles);
        TestConnections(loadedProfiles);
      })
      .catch(err => alert(err.message));
  };

  useEffect(() => {
    LoadProfiles();
    PhysicalCountSettings.machineName().then(setMachineName);
    PhysicalCountSettings.pageSize().then(value => setPageSize(value.toString()));
  }, []);

  const ClearProfile = () => {
    setProfileID(0);
    setLocation('');
    setServer('');
    setSelected(true);
  };

  const SaveProfile = async () => {

    const locationCode = location.trim().toUpperCase();
    const serverAddress = server.trim().toLowerCase();

    const errorMessage = CheckLocation(locationCode) || CheckServer(serverAddress);
    if (errorMessage !== '') {
      alert(errorMessage);
      return;
    }

    // The same location can be on several servers, but not twice on the same server
    if (profiles.some(profile => profile.location === locationCode && profile.server === serverAddress && profile.profileID !== profileID)) {
      alert(`Location ${locationCode} is already defined on server ${serverAddress}`);
      return;
    }

    const save = async () => {
      try {
        await PhysicalCountDatabase.SaveProfile(profileID, locationCode, serverAddress, selected);
        ClearProfile();
        LoadProfiles();
      } catch (error) {
        alert(error.message);
      }
    };

    // The downloaded count belongs to the server + location of the profile,
    // changing them would leave the counted lines that are not uploaded out of reach
    const editedProfile = profiles.find(profile => profile.profileID === profileID);
    if (editedProfile && (editedProfile.server !== serverAddress || editedProfile.location !== locationCode)) {
      const summary = await PhysicalCountDatabase.GetLinesSummary(editedProfile);
      if (summary.pending > 0) {
        alert(`Location ${editedProfile.location} has ${summary.pending} counted lines not uploaded to ${editedProfile.server}, please upload them first`);
        return;
      }
    }

    save();

  };

  const DeleteProfile = async () => {

    const deletedProfile = profiles.find(profile => profile.profileID === profileID);
    if (!deletedProfile) return;

    const summary = await PhysicalCountDatabase.GetLinesSummary(deletedProfile);
    if (summary.pending > 0) {
      alert(`Location ${deletedProfile.location} has ${summary.pending} counted lines not uploaded to ${deletedProfile.server}, please upload them first`);
      return;
    }

    MessageBox.alert('Delete Location', `Do you want to delete location ${deletedProfile.location} on ${deletedProfile.server} and its downloaded count ?`, [
      { text: 'No', style: 'cancel' },
      {
        text: 'Yes', onPress: () => {
          PhysicalCountDatabase.DeleteProfile(deletedProfile)
            .then(() => { ClearProfile(); LoadProfiles(); })
            .catch(err => alert(err.message));
        }
      },
    ]);

  };

  const SaveDeviceSettings = async () => {

    const errorMessage = CheckPageSize(pageSize);
    if (errorMessage !== '') {
      alert(errorMessage);
      return;
    }

    await PhysicalCountSettings.savePageSize(parseInt(pageSize, 10));
    alert('Settings Saved');

  };

  const RestoreUploaded = async () => {

    const profile = await PhysicalCountDatabase.GetSelectedProfile();
    if (!profile) {
      alert('Please select a location');
      return;
    }

    MessageBox.alert('Restore Uploaded', `All the uploaded lines of location ${profile.location} will be sent again on the next upload. Continue ?`, [
      { text: 'No', style: 'cancel' },
      {
        text: 'Yes', onPress: () => {
          PhysicalCountDatabase.RestoreUploaded(profile)
            .then(count => alert(`${count} Lines Restored`))
            .catch(err => alert(err.message));
        }
      },
    ]);

  };

  const ProfileItem = (profile) => {
    return (
      <View key={profile.profileID} style={styles.profileContainer}>

        <TouchableOpacity style={styles.profileInfo} onPress={() => {
          setProfileID(profile.profileID);
          setLocation(profile.location);
          setServer(profile.server);
          setSelected(profile.selected === 1);
        }}>
          <Text style={styles.profileLocationText}>{profile.location}</Text>
          <Text style={styles.profileServerText} numberOfLines={1}>{profile.server}</Text>
          <Text numberOfLines={1} style={[styles.profileConnectionText, {
            color: connections[profile.profileID]?.state === 'connected' ? Colors.brandGreen
              : connections[profile.profileID]?.state === 'error' ? 'red' : 'gray'
          }]}>
            {connections[profile.profileID]?.text ?? ''}
          </Text>
        </TouchableOpacity>

        <TouchableOpacity style={styles.profileCheckBox} onPress={() => {
          PhysicalCountDatabase.SelectProfile(profile.profileID)
            .then(LoadProfiles)
            .catch(err => alert(err.message));
        }}>
          <Image
            source={profile.selected === 1 ? require('../assets/icons/checked.png') : require('../assets/icons/unchecked.png')}
            style={styles.profileCheckBoxImage}
            resizeMode='contain'
          />
        </TouchableOpacity>

      </View>
    );
  };

  return (
    <View style={styles.container}>

      <ScreenHeader
        description='Locations, servers and machine name of the physical count.'
        imageSource={require('../assets/icons/settings.png')}
      />

      <ScrollView keyboardShouldPersistTaps='handled'>

        <MenuHeader title='LOCATIONS' />

        {profiles.length === 0 ?
          <Text style={styles.emptyText}>No Locations Defined</Text> :
          profiles.map(ProfileItem)}

        <View style={styles.formContainer}>

          <Text style={styles.formTitleText}>{profileID === 0 ? 'New Location' : `Edit Location ${location}`}</Text>

          <TextBoxField
            title='Location (2 Characters)'
            value={location}
            maxLength={2}
            multiline={false}
            numberOfLines={1}
            autoCapitalize='characters'
            autoCorrect={false}
            onChangeText={setLocation} />

          <TextBoxField
            title='Server (without http://)'
            value={server}
            maxLength={100}
            multiline={false}
            numberOfLines={1}
            autoCapitalize='none'
            autoCorrect={false}
            onChangeText={setServer} />

          <TouchableOpacity style={styles.selectedContainer} onPress={() => setSelected(!selected)}>
            <Text style={styles.selectedText}>In Use</Text>
            <Image
              source={selected ? require('../assets/icons/checked.png') : require('../assets/icons/unchecked.png')}
              style={styles.profileCheckBoxImage}
              resizeMode='contain'
            />
          </TouchableOpacity>

          <View style={styles.buttonsContainer}>
            <MainButton title='Save Location' onPress={() => SaveProfile()} />
            <MainButton title={profileID === 0 ? 'Clear' : 'New Location'} onPress={() => ClearProfile()} />
            {profileID !== 0 ? <MainButton title='Delete Location' onPress={() => DeleteProfile()} /> : <></>}
          </View>

        </View>

        <MenuHeader title='DEVICE' />

        <View style={styles.formContainer}>

          {/* The name of the device, changed in the Settings of the device (read only here) */}
          <DataField
            title='Machine Name'
            data={machineName === '' ? 'Not Valid' : machineName}
            toolTip='Device Name'
            onPress={() => alert(machineName === '' ? MachineNameMessage() : 'The machine name is the name of the device, change it in the Settings of the device : About phone > Device name.')}
          />

          <TextBoxNumericField
            title='Download Page Size'
            value={pageSize}
            maxLength={4}
            multiline={false}
            numberOfLines={1}
            onChangeText={setPageSize} />

          <View style={styles.buttonsContainer}>
            <MainButton title='Save Settings' onPress={() => SaveDeviceSettings()} />
            <MainButton title='Restore Uploaded' onPress={() => RestoreUploaded()} />
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
  emptyText: {
    textAlign: 'center',
    color: 'gray',
    fontSize: FontSize(16),
    padding: 15,
  },
  profileContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    height: 72,
    backgroundColor: Colors.listItemBackColor,
    marginBottom: 2,
    paddingLeft: 15,
    paddingRight: 10,
  },
  profileInfo: {
    flex: 1,
    height: '100%',
    justifyContent: 'center',
  },
  profileLocationText: {
    color: 'black',
    fontWeight: 'bold',
    fontSize: FontSize(18),
  },
  profileServerText: {
    color: Colors.brandBlue,
    fontSize: FontSize(15),
  },
  profileConnectionText: {
    fontSize: FontSize(13),
    fontWeight: 'bold',
  },
  profileCheckBox: {
    width: 45,
    height: '100%',
    alignItems: 'center',
    justifyContent: 'center',
  },
  profileCheckBoxImage: {
    width: 30,
    height: 30,
  },
  formContainer: {
    padding: 10,
  },
  formTitleText: {
    color: Colors.brandGreen,
    fontSize: FontSize(18),
    fontWeight: 'bold',
    marginBottom: 10,
  },
  selectedContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    height: 45,
    paddingLeft: 5,
    paddingRight: 5,
  },
  selectedText: {
    color: Colors.brandBlue,
    fontSize: FontSize(17),
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

export default PhysicalCountSettingsScreen;
