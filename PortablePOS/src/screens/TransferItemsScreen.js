import React, { useState, useCallback, useEffect } from 'react';
import { View, Text, TextInput, StyleSheet, FlatList, TouchableOpacity, Alert } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import SwitchSelector from 'react-native-switch-selector';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import TransferLine from '../components/lists/transfer/TransferLine';
import TransferItem from '../components/lists/transfer/TransferItem';
import TransferLineEditor from '../components/lists/transfer/TransferLineEditor';
import TransferDatabase from '../database/TransferDatabase';
import { FormatQuantity, LIST_LIMIT, CheckLineQty } from '../variables/TransferRules';

const TRANSFER = 0;
const STOCK = 1;

// The lines of the transfer (tap one to change its qty or delete it, or clear them all) and the stock of the location
const TransferItemsScreen = () => {

  const [view, setView] = useState(TRANSFER);
  const [searchString, setSearchString] = useState('');
  const [rows, setRows] = useState([]);
  const [summary, setSummary] = useState({ lines: 0, lots: 0, qty: 0, destinations: 0 });
  const [info, setInfo] = useState(null);
  const [editedLine, setEditedLine] = useState(null);

  const LoadRows = (shownView, text) => {
    const request = shownView === TRANSFER
      ? TransferDatabase.GetLines(text, LIST_LIMIT)
      : TransferDatabase.SearchItems(text, true, LIST_LIMIT);
    request.then(setRows).catch(err => alert(err.message));
    TransferDatabase.GetSummary().then(setSummary).catch(() => { });
  };

  const Reload = () => LoadRows(view, searchString);

  useFocusEffect(
    useCallback(() => {
      TransferDatabase.GetInfo().then(setInfo).catch(() => { });
    }, [])
  );

  useEffect(() => {
    LoadRows(view, searchString);
  }, [view, searchString]);

  const ClearTransfer = () => {

    if (summary.lines === 0) {
      alert('No Items In The Transfer');
      return;
    }

    Alert.alert('Clear Transfer', `Do you want to remove the ${summary.lines} lines of the transfer ? The way of work can be changed again afterwards.`, [
      { text: 'No', style: 'cancel' },
      {
        text: 'Clear', style: 'destructive', onPress: () => {
          TransferDatabase.ClearLines()
            .then(Reload)
            .catch(err => alert(err.message));
        }
      },
    ]);

  };

  const DeleteLine = (line) => {
    Alert.alert('Remove Line', `Do you want to remove ${line.itemReference} → ${line.destination} (${FormatQuantity(line.qty)}) ?`, [
      { text: 'No', style: 'cancel' },
      {
        text: 'Remove', style: 'destructive', onPress: () => {
          setEditedLine(null);
          TransferDatabase.DeleteLine(line.lineID)
            .then(Reload)
            .catch(err => alert(err.message));
        }
      },
    ]);
  };

  const RowItem = rowData => view === TRANSFER
    ? <TransferLine line={rowData.item} showDestination={true} onPress={() => setEditedLine(rowData.item)} />
    : <TransferItem item={rowData.item} currencyLC={info?.currencyLC} />;

  return (
    <View style={styles.container}>

      <TransferLineEditor
        line={editedLine}
        decimals={info?.decimals ?? 0}
        onSave={async (qty) => {
          const check = await CheckLineQty(TransferDatabase, editedLine, qty);
          if (!check.ok) return check.message;
          await TransferDatabase.SetLineQty(editedLine.lineID, check.value);
          setEditedLine(null);
          Reload();
          return '';
        }}
        onDelete={() => DeleteLine(editedLine)}
        onClose={() => setEditedLine(null)}
      />

      <View style={styles.searchContainer}>
        <TextInput
          style={styles.searchText}
          placeholder={view === TRANSFER ? 'Search Transfer Items' : 'Search Stock'}
          maxLength={30}
          value={searchString}
          autoCorrect={false}
          onChangeText={setSearchString}
        />
      </View>

      <FlatList
        data={rows}
        keyExtractor={(item) => view === TRANSFER ? item.lineID.toString() : item.itemRecID}
        renderItem={RowItem}
        ListEmptyComponent={<Text style={styles.emptyText}>{view === TRANSFER ? 'No Items In The Transfer' : 'No Items Found'}</Text>}
      />

      {rows.length === LIST_LIMIT ?
        <Text style={styles.limitText}>Showing the first {LIST_LIMIT}, use the search</Text> :
        <></>}

      <View style={styles.footerContainer}>

        {view === TRANSFER ?
          <View style={styles.transferRow}>
            <Text style={styles.summaryText} numberOfLines={1}>
              {summary.lots} Items  ·  Qty {FormatQuantity(summary.qty)}
            </Text>
            <TouchableOpacity style={[styles.clearButton, { opacity: summary.lines === 0 ? 0.5 : 1 }]} onPress={ClearTransfer}>
              <Text style={styles.clearButtonText}>Clear Transfer</Text>
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
            { label: 'Transfer Items', value: TRANSFER },
            { label: info ? `Stock Of ${info.location}` : 'Stock', value: STOCK }
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
  transferRow: {
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
  clearButton: {
    height: 40,
    paddingHorizontal: 15,
    borderRadius: 15,
    backgroundColor: 'red',
    alignItems: 'center',
    justifyContent: 'center',
  },
  clearButtonText: {
    color: 'white',
    fontSize: FontSize(15),
  },
});

export default TransferItemsScreen;
