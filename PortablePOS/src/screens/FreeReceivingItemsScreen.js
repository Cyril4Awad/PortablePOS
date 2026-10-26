import React, { useState, useCallback, useEffect } from 'react';
import { View, Text, TextInput, StyleSheet, FlatList, TouchableOpacity } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import SwitchSelector from 'react-native-switch-selector';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import FreeReceivingLine from '../components/lists/freeReceiving/FreeReceivingLine';
import FreeReceivingItem from '../components/lists/freeReceiving/FreeReceivingItem';
import FreeReceivingLineEditor from '../components/lists/freeReceiving/FreeReceivingLineEditor';
import FreeReceivingDatabase from '../database/FreeReceivingDatabase';
import { FormatQuantity, LIST_LIMIT } from '../variables/FreeReceivingRules';
import MessageBox from '../components/MessageBox';

const COLLECTED = 0;
const STOCK_FILE = 1;

// Collected items (tap one to change its qty or delete it, or delete them all) and the downloaded stock file
const FreeReceivingItemsScreen = () => {

  const [view, setView] = useState(COLLECTED);
  const [searchString, setSearchString] = useState('');
  const [rows, setRows] = useState([]);
  const [summary, setSummary] = useState({ lines: 0, qty: 0, pending: 0 });
  const [decimals, setDecimals] = useState(0);
  const [editedLine, setEditedLine] = useState(null);

  const LoadRows = (shownView, text) => {
    const request = shownView === COLLECTED
      ? FreeReceivingDatabase.GetLines(text, LIST_LIMIT)
      : FreeReceivingDatabase.FindItems('any', text, false, LIST_LIMIT);
    request.then(setRows).catch(err => alert(err.message));
    FreeReceivingDatabase.GetLinesSummary().then(setSummary).catch(() => { });
  };

  const Reload = () => LoadRows(view, searchString);

  useFocusEffect(
    useCallback(() => {
      FreeReceivingDatabase.GetInfo().then(info => setDecimals(info?.decimals ?? 0)).catch(() => { });
    }, [])
  );

  useEffect(() => {
    LoadRows(view, searchString);
  }, [view, searchString]);

  const DeleteAll = () => {

    if (summary.lines === 0) {
      alert('No Collected Items Found');
      return;
    }

    const message = summary.pending > 0
      ? `${summary.pending} of the ${summary.lines} collected lines are not uploaded. Do you want to delete all the collected items ?`
      : `Do you want to delete the ${summary.lines} collected lines ?`;

    MessageBox.alert('Delete Collected Items', message, [
      { text: 'No', style: 'cancel' },
      {
        text: 'Delete', style: 'destructive', onPress: () => {
          FreeReceivingDatabase.DeleteAllLines()
            .then(Reload)
            .catch(err => alert(err.message));
        }
      },
    ]);

  };

  const DeleteLine = (line) => {
    MessageBox.alert('Delete Line', `Do you want to delete ${line.itemReference} (${FormatQuantity(line.qty)}) ?`, [
      { text: 'No', style: 'cancel' },
      {
        text: 'Delete', style: 'destructive', onPress: () => {
          setEditedLine(null);
          FreeReceivingDatabase.DeleteLine(line.lineID)
            .then(Reload)
            .catch(err => alert(err.message));
        }
      },
    ]);
  };

  const RowItem = rowData => view === COLLECTED
    ? <FreeReceivingLine line={rowData.item} onPress={() => setEditedLine(rowData.item)} />
    : <FreeReceivingItem item={rowData.item} />;

  return (
    <View style={styles.container}>

      <FreeReceivingLineEditor
        line={editedLine}
        decimals={decimals}
        onSave={(qty) => {
          const lineID = editedLine.lineID;
          setEditedLine(null);
          FreeReceivingDatabase.SetLineQty(lineID, qty)
            .then(Reload)
            .catch(err => alert(err.message));
        }}
        onDelete={() => DeleteLine(editedLine)}
        onClose={() => setEditedLine(null)}
      />

      <View style={styles.searchContainer}>
        <TextInput
          style={styles.searchText}
          placeholder={view === COLLECTED ? 'Search Collected Items' : 'Search Stock File'}
          maxLength={30}
          value={searchString}
          autoCorrect={false}
          onChangeText={setSearchString}
        />
      </View>

      <FlatList
        data={rows}
        keyExtractor={(item) => view === COLLECTED ? item.lineID.toString() : item.itemRecID}
        renderItem={RowItem}
        ListEmptyComponent={<Text style={styles.emptyText}>{view === COLLECTED ? 'No Collected Items' : 'No Items Found'}</Text>}
      />

      {rows.length === LIST_LIMIT ?
        <Text style={styles.limitText}>Showing the first {LIST_LIMIT}, use the search</Text> :
        <></>}

      <View style={styles.footerContainer}>

        {view === COLLECTED ?
          <View style={styles.collectedRow}>
            <Text style={styles.summaryText} numberOfLines={1}>
              {summary.lines} Lines  ·  Qty {FormatQuantity(summary.qty)}
            </Text>
            <TouchableOpacity style={[styles.deleteButton, { opacity: summary.lines === 0 ? 0.5 : 1 }]} onPress={DeleteAll}>
              <Text style={styles.deleteButtonText}>Delete Collected</Text>
            </TouchableOpacity>
          </View> :
          <></>}

        <SwitchSelector
          initial={0}
          onPress={(value) => { setRows([]); setView(value); }}
          textColor={Colors.brandBlue}
          selectedColor='white'
          buttonColor={Colors.brandBlue}
          borderColor={Colors.brandBlue}
          hasPadding={true}
          options={[
            { label: 'Collected Items', value: COLLECTED },
            { label: 'Stock File', value: STOCK_FILE }
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
    fontSize: FontSize(21),
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
  footerContainer: {
    backgroundColor: Colors.bgColor,
    width: '100%',
    padding: 5,
    borderTopWidth: 1,
    borderTopColor: 'silver',
  },
  collectedRow: {
    flexDirection: 'row',
    alignItems: 'center',
    height: 44,
    marginBottom: 5,
    paddingLeft: 5,
  },
  summaryText: {
    flex: 1,
    color: Colors.brandBlue,
    fontSize: FontSize(15),
    fontWeight: 'bold',
  },
  deleteButton: {
    height: 40,
    paddingHorizontal: 15,
    borderRadius: 15,
    backgroundColor: 'red',
    alignItems: 'center',
    justifyContent: 'center',
  },
  deleteButtonText: {
    color: 'white',
    fontSize: FontSize(15),
  },
});

export default FreeReceivingItemsScreen;
