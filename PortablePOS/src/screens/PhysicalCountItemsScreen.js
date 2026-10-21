import React, { useState, useCallback } from 'react';
import { View, Text, TextInput, StyleSheet, FlatList } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import SwitchSelector from 'react-native-switch-selector';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import PhysicalCountLine from '../components/lists/physicalCount/PhysicalCountLine';
import PhysicalCountDatabase from '../database/PhysicalCountDatabase';

const COUNTED = 1;
const NOT_COUNTED = 0;

// Counted (uploaded or not, each line shows it) and not counted lines, tapping a line opens it in the Scan For Testing screen
const PhysicalCountItemsScreen = ({ route, navigation }) => {

  const { server, location } = route.params;
  const countKey = { server, location };

  const [state, setState] = useState(COUNTED);
  const [searchString, setSearchString] = useState('');
  const [lines, setLines] = useState([]);
  // Downloaded header : modes and available qty privilege used to show the line fields
  const [header, setHeader] = useState(null);

  const LoadLines = (lineState, text) => {
    PhysicalCountDatabase.GetLines(countKey, lineState === COUNTED, text)
      .then(setLines)
      .catch(err => alert(err.message));
  };

  // Reload when coming back from the scan screen
  useFocusEffect(
    useCallback(() => {
      PhysicalCountDatabase.GetHeader(countKey).then(setHeader);
      LoadLines(state, searchString);
    }, [state, searchString])
  );

  const LineItem = lineData => {
    return (
      <PhysicalCountLine
        line={lineData.item}
        header={header}
        onPress={() => navigation.navigate('PhysicalCountScanTesting', { server: server, location: location, lineRecID: lineData.item.lineRecID })}
      />
    );
  };

  return (
    <View style={styles.container}>

      <View style={styles.searchContainer}>
        <TextInput
          style={styles.searchText}
          placeholder='Search Items'
          maxLength={30}
          value={searchString}
          onChangeText={setSearchString}
        />
      </View>

      <FlatList
        data={lines}
        keyExtractor={(item) => item.lineRecID}
        renderItem={LineItem}
        ListEmptyComponent={<Text style={styles.emptyText}>No Items Found</Text>}
      />

      {lines.length === 500 ?
        <Text style={styles.limitText}>Showing the first 500 items, use the search</Text> :
        <></>}

      <View style={styles.switchContainer}>
        <SwitchSelector
          initial={0}
          onPress={(value) => setState(value)}
          textColor={Colors.brandBlue}
          selectedColor='white'
          buttonColor={Colors.brandBlue}
          borderColor={Colors.brandBlue}
          hasPadding={true}
          options={[
            { label: 'Counted', value: COUNTED },
            { label: 'Not Counted', value: NOT_COUNTED }
          ]}
        />
      </View>

    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.bgColor,
  },
  searchContainer: {
    width: '100%',
    height: 60,
    alignItems: Sizing.isTablet ? 'flex-end' : 'center',
    justifyContent: 'center',
    padding: 5,
  },
  searchText: {
    width: Sizing.isTablet ? '25%' : '100%',
    height: '100%',
    borderWidth: 1,
    borderColor: 'silver',
    paddingLeft: 25,
    fontSize: FontSize(23),
    backgroundColor: 'white',
    color: 'black',
  },
  emptyText: {
    textAlign: 'center',
    color: 'gray',
    fontSize: FontSize(17),
    padding: 20,
  },
  limitText: {
    textAlign: 'center',
    color: 'gray',
    fontSize: FontSize(13),
    padding: 5,
  },
  switchContainer: {
    backgroundColor: Colors.bgColor,
    width: '100%',
    padding: 5,
    alignItems: 'center',
    justifyContent: 'center',
  },
});

export default PhysicalCountItemsScreen;
