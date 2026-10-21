import React, { useState, useCallback, useLayoutEffect } from 'react';
import { View, Text, StyleSheet, Modal, TouchableOpacity, Alert } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import ScreenHeader from '../components/ScreenHeader';
import ModalTitle from '../components/ModalTitle';
import DataField from '../components/controls/DataField';
import ChoiceField, { choiceRowStyle } from '../components/controls/ChoiceField';
import TextBoxField from '../components/controls/TextBoxField';
import MainButton from '../components/controls/MainButton';
import LoadingData from '../components/LoadingData';
import PhysicalCountFilterPicker from '../components/lists/physicalCount/PhysicalCountFilterPicker';
import PhysicalCountMachineCounts from '../components/lists/physicalCount/PhysicalCountMachineCounts';
import PhysicalCountData from '../data/PhysicalCountData';
import PhysicalCountDatabase from '../database/PhysicalCountDatabase';
import PhysicalCountSettings from '../configuration/PhysicalCountSettings';
import {
  BOOKS_OPTIONS, GROUPS, EmptyFilters, GroupSummary, NotCountedSummary, CheckDateRange,
} from '../variables/PhysicalCountCreateRules';

// Create Physical Count, step 1 : filters, like the count module of the ERP
// The brands, categories, product groups and identifications check their items (the items are chosen on the next screen)
// The count is created for the location and the machine of the physical count settings
const PhysicalCountCreateScreen = props => {

  const [profile, setProfile] = useState(null);
  const [machineName, setMachineName] = useState('');

  // { locationDescription, workingDay, bookMode, showAvailableQty, brands, categories, productGroups, identifications }
  const [serverData, setServerData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [errorText, setErrorText] = useState('');

  const [filters, setFilters] = useState(EmptyFilters());

  // Picker open : a key of GROUPS or 'books'
  const [pickerKey, setPickerKey] = useState(null);

  // Not counted between : { from, to } while the modal is open
  const [dateRange, setDateRange] = useState(null);

  // My Counts : the counts of this machine, and the count downloaded on the device (marked in the list)
  const [myCountsVisible, setMyCountsVisible] = useState(false);
  const [deviceCountNumber, setDeviceCountNumber] = useState(null);

  const LoadFilters = async () => {

    setErrorText('');

    try {

      const selectedProfile = await PhysicalCountDatabase.GetSelectedProfile();
      setProfile(selectedProfile);
      setMachineName(await PhysicalCountSettings.machineName());

      if (!selectedProfile) {
        setServerData(null);
        setDeviceCountNumber(null);
        return;
      }

      const deviceHeader = await PhysicalCountDatabase.GetHeader(selectedProfile);
      setDeviceCountNumber(deviceHeader ? deviceHeader.countNumber : null);

      setLoading(true);
      setServerData(await PhysicalCountData.GetPhysicalCountFilters(selectedProfile.server, selectedProfile.location));

    } catch (error) {
      setServerData(null);
      setErrorText(error.message);
    } finally {
      setLoading(false);
    }

  };

  // Clear is in the title bar : Show Items and My Counts fit on one row, the screen of the portable scanners has no room for a second one
  useLayoutEffect(() => {
    props.navigation.setOptions({
      headerRight: () => (
        <TouchableOpacity style={styles.headerButton} onPress={() => setFilters(EmptyFilters())}>
          <Text style={styles.headerButtonText}>Clear</Text>
        </TouchableOpacity>
      ),
    });
  }, [props.navigation]);

  // Reload when coming back from the settings
  useFocusEffect(
    useCallback(() => {
      LoadFilters();
    }, [])
  );

  const ShowItems = () => {

    if (!profile) {
      alert('Please select a location in the physical count settings');
    }
    else if (machineName === '') {
      alert('Please fill the machine name in the physical count settings');
    }
    else if (serverData === null) {
      alert(errorText !== '' ? errorText : 'The location is not loaded yet');
    }
    else {
      props.navigation.navigate('PhysicalCountCreateItems', {
        server: profile.server,
        location: profile.location,
        machineName: machineName,
        showAvailableQty: serverData.showAvailableQty,
        filters: filters,
      });
    }

  };

  const OpenMyCounts = () => {

    if (!profile) {
      alert('Please select a location in the physical count settings');
    }
    else if (machineName === '') {
      alert('Please fill the machine name in the physical count settings');
    }
    else {
      setMyCountsVisible(true);
    }

  };

  // An open count of My Counts : downloaded on the Physical Count page (which warns about lines not uploaded)
  const PickCount = (countNumber) => {

    const onDevice = deviceCountNumber === countNumber;

    Alert.alert('Download Physical Count',
      onDevice ? `Physical count ${countNumber} is already on this device. Download it again ?` : `Download physical count ${countNumber} now ?`, [
      { text: 'No', style: 'cancel' },
      {
        text: 'Download', onPress: () => {
          setMyCountsVisible(false);
          props.navigation.reset({
            index: 1,
            routes: [{ name: 'Home' }, { name: 'PhysicalCount', params: { downloadCountNumber: countNumber } }],
          });
        }
      },
    ]);

  };

  const pickerGroup = GROUPS.find(group => group.key === pickerKey);

  return (
    <View style={styles.container}>

      <PhysicalCountMachineCounts
        visible={myCountsVisible}
        server={profile?.server}
        location={profile?.location}
        machineName={machineName}
        deviceCountNumber={deviceCountNumber}
        onPick={PickCount}
        onClose={() => setMyCountsVisible(false)}
      />

      <PhysicalCountFilterPicker
        visible={pickerKey !== null}
        title={pickerKey === 'books' ? 'Books' : pickerGroup?.title ?? ''}
        options={pickerKey === 'books' ? BOOKS_OPTIONS : pickerGroup && serverData ? serverData[pickerGroup.key] : []}
        selected={pickerKey === 'books' ? [filters.books] : pickerGroup ? filters[pickerGroup.key] : []}
        multiple={pickerKey !== 'books'}
        onSubmit={(codes) => {
          setFilters(current => pickerKey === 'books' ? { ...current, books: codes[0] } : { ...current, [pickerKey]: codes });
          setPickerKey(null);
        }}
        onCancel={() => setPickerKey(null)}
      />

      {/* Not counted between : items that are not in an open count of the location between the dates (both dates are optional) */}
      <Modal visible={dateRange !== null} transparent={true} presentationStyle="overFullScreen" onRequestClose={() => setDateRange(null)}>
        <View style={styles.modalContainer}>
          <View style={styles.modalCard}>
            <ModalTitle title='Not Counted Between' onCloseButtonPress={() => setDateRange(null)} />
            <View style={styles.modalBody}>
              <Text style={styles.modalHintText}>Leave out the items that are in an open count of this location between these dates (dd/MM/yyyy, both optional)</Text>
              <TextBoxField title='From' value={dateRange?.from ?? ''} maxLength={10}
                onChangeText={text => setDateRange(current => ({ ...current, from: text }))} />
              <TextBoxField title='To' value={dateRange?.to ?? ''} maxLength={10}
                onChangeText={text => setDateRange(current => ({ ...current, to: text }))} />
            </View>
            <View style={styles.footer}>
              <TouchableOpacity style={[styles.button, { backgroundColor: 'red' }]} onPress={() => {
                setFilters(current => ({ ...current, notCounted: null }));
                setDateRange(null);
              }}>
                <Text style={styles.buttonText}>Off</Text>
              </TouchableOpacity>
              <TouchableOpacity style={styles.button} onPress={() => {
                const from = dateRange.from.trim(), to = dateRange.to.trim();
                const errorMessage = CheckDateRange(from, to);
                if (errorMessage !== '') { alert(errorMessage); return; }
                setFilters(current => ({ ...current, notCounted: { from, to } }));
                setDateRange(null);
              }}>
                <Text style={styles.buttonText}>OK</Text>
              </TouchableOpacity>
            </View>
          </View>
        </View>
      </Modal>

      {!Sizing.isPortableScanner ?
        <ScreenHeader
          description='Choose the items, then create a count for this machine.'
          imageSource={require('../assets/icons/stock.png')}
        /> :
        <></>}

      <View>

        <ChoiceField
          title='Location'
          value={profile ? `${profile.location}${serverData?.locationDescription ? ' - ' + serverData.locationDescription : ''}` : 'Not Selected'}
          onPress={() => props.navigation.navigate('PhysicalCountSettings')}
          style={{ borderRightWidth: 0 }}
        />

        <View style={choiceRowStyle.row}>
          <View style={styles.fieldHalf}>
            <ChoiceField
              title='Machine'
              value={machineName === '' ? 'Not Defined' : machineName}
              onPress={() => props.navigation.navigate('PhysicalCountSettings')}
              style={{ borderBottomWidth: 0 }}
            />
          </View>
          <View style={styles.fieldHalf}>
            <DataField title='Count Date' data={serverData?.workingDay ?? '-'} />
          </View>
        </View>

        {errorText !== '' ?
          <Text style={styles.errorText}>{errorText}</Text> :
          <></>}

        {GROUPS.map(group => (
          <DataField key={group.key} title={group.title}
            data={GroupSummary(serverData ? serverData[group.key] : [], filters[group.key])}
            toolTip='Check Items'
            onPress={() => serverData ? setPickerKey(group.key) : alert(errorText !== '' ? errorText : 'The location is not loaded yet')} />
        ))}

        {serverData?.bookMode === 'Y' ?
          <DataField title='Books' data={BOOKS_OPTIONS.find(option => option.code === filters.books).description} toolTip='Filter'
            onPress={() => setPickerKey('books')} /> :
          <></>}

        <DataField title='Not Counted Between' data={NotCountedSummary(filters.notCounted)} toolTip='Filter'
          onPress={() => setDateRange({ from: filters.notCounted?.from ?? '', to: filters.notCounted?.to ?? '' })} />

        <View style={styles.buttonsContainer}>

          <MainButton title='Show Items' imageSource={require('../assets/icons/stock.png')} onPress={() => ShowItems()} />

          <MainButton title='My Counts' imageSource={require('../assets/icons/download.png')} onPress={() => OpenMyCounts()} />

        </View>

      </View>

      {/* Last : over everything, nothing can be pressed while it shows */}
      {loading ? <LoadingData /> : <></>}

    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    backgroundColor: Colors.bgColor,
    height: '100%',
  },
  headerButton: {
    paddingHorizontal: 12,
    height: '100%',
    justifyContent: 'center',
  },
  headerButtonText: {
    color: Colors.navTitleColor,
    fontSize: FontSize(14),
    fontWeight: 'bold',
  },
  fieldsRow: {
    flexDirection: 'row',
  },
  fieldHalf: {
    width: '50%',
  },
  errorText: {
    color: 'red',
    fontSize: FontSize(15),
    fontWeight: 'bold',
    paddingHorizontal: 15,
    paddingVertical: 6,
  },
  buttonsContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    alignItems: 'center',
    justifyContent: Sizing.isTablet ? 'flex-end' : 'space-between',
    width: '100%',
    padding: 10,
  },
  modalContainer: {
    width: '100%',
    height: '100%',
    backgroundColor: 'rgba(0,0,0,0.5)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  modalCard: {
    width: Sizing.windowsWidth * 0.92,
    backgroundColor: 'white',
    borderRadius: 20,
    overflow: 'hidden',
    elevation: 10,
    borderWidth: 5,
    borderColor: Colors.brandBlue,
  },
  modalBody: {
    padding: 12,
  },
  modalHintText: {
    color: 'gray',
    fontSize: FontSize(14),
    fontStyle: 'italic',
    marginBottom: 10,
  },
  footer: {
    backgroundColor: 'gainsboro',
    height: 60,
    width: '100%',
    alignItems: 'center',
    justifyContent: 'space-evenly',
    flexDirection: 'row',
  },
  button: {
    backgroundColor: Colors.brandBlue,
    width: 110,
    height: 40,
    alignItems: 'center',
    justifyContent: 'center',
    borderRadius: 15,
  },
  buttonText: {
    fontSize: FontSize(15),
    color: 'white',
  },
});

export default PhysicalCountCreateScreen;
