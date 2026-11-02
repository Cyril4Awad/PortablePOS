import React, { useState, useCallback, useEffect } from 'react';
import { View, Text, TextInput, StyleSheet, FlatList } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import SwitchSelector from 'react-native-switch-selector';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import FreeReceivingLine from '../components/lists/freeReceiving/FreeReceivingLine';
import FreeReceivingItem from '../components/lists/freeReceiving/FreeReceivingItem';
import FreeReceivingDatabase from '../database/FreeReceivingDatabase';
import { FormatQuantity, LIST_LIMIT } from '../variables/FreeReceivingRules';
import { GetDraft, LoadDraft, StartDraft } from '../variables/ReceivingOrderDraft';
import MessageBox from '../components/MessageBox';

const RECEIVED = 0;
const STOCK_FILE = 1;

// The items of the receiving orders saved on the device, not uploaded (tap one : its receiving order on the Receiving Order screen)
// and the downloaded stock file (tap one : received on the scan screen)
const FreeReceivingItemsScreen = ({ navigation }) => {

  const [view, setView] = useState(RECEIVED);
  const [searchString, setSearchString] = useState('');
  const [rows, setRows] = useState([]);
  const [summary, setSummary] = useState({ orders: 0, lines: 0, qty: 0, pending: 0, pendingOrders: 0 });

  const LoadRows = (shownView, text) => {
    const request = shownView === RECEIVED
      ? FreeReceivingDatabase.GetLines(text, LIST_LIMIT)
      : FreeReceivingDatabase.SearchItems(text, LIST_LIMIT);
    request.then(setRows).catch(err => alert(err.message));
    FreeReceivingDatabase.GetLinesSummary().then(setSummary).catch(() => { });
  };

  const Reload = () => LoadRows(view, searchString);

  useFocusEffect(
    useCallback(() => {
      LoadRows(view, searchString);
    }, [view, searchString])
  );

  useEffect(() => {
    LoadRows(view, searchString);
  }, [view, searchString]);

  // The receiving order of the line on the Receiving Order screen : not uploaded = it can change (Save keeps it on the device and uploads it),
  // uploaded = view only
  const OpenOrder = async (line) => {
    const order = await FreeReceivingDatabase.GetOrder(line.orderID);
    if (!order) { Reload(); return; }
    const open = () => {
      if (order.state === 2) {
        LoadDraft({ ...order.draft, mode: 'VIEW', roRecID: order.roRecID, savedLines: order.draft.lines, lines: [], orderID: order.orderID }, 'FREE');
      }
      else {
        LoadDraft({ ...order.draft, mode: 'ADD', orderID: order.orderID }, 'FREE');
      }
      navigation.navigate('ReceivingOrderEdit', { free: true });
    };
    // A new receiving order being prepared (not saved on the device yet) would be lost
    const current = GetDraft('FREE');
    if (current && !current.orderID && current.mode === 'ADD' && current.lines.length > 0) {
      MessageBox.alert('Free Receiving', `The receiving order being prepared (${current.lines.length} lines, not saved) will be cleared.`, [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Open', style: 'destructive', onPress: open },
      ]);
    }
    else open();
  };

  // An item of the stock file : the scan screen with it (the new receiving order being prepared, or a new one)
  const ReceiveItem = (item) => {
    const current = GetDraft('FREE');
    if (!current || current.orderID || current.mode === 'VIEW') StartDraft('ADD', null, 'FREE');
    navigation.navigate('ReceivingOrderEdit', { free: true, pickItem: { itemRecID: item.itemRecID, itemReference: item.itemReference } });
  };

  const RowItem = rowData => view === RECEIVED
    ? <FreeReceivingLine line={rowData.item} onPress={() => OpenOrder(rowData.item)} />
    : <FreeReceivingItem item={rowData.item} onPress={() => ReceiveItem(rowData.item)} />;

  return (
    <View style={styles.container}>

      <View style={styles.searchContainer}>
        <TextInput
          style={styles.searchText}
          placeholder={view === RECEIVED ? 'Search Received Items' : 'Search Stock File'}
          maxLength={30}
          value={searchString}
          autoCorrect={false}
          onChangeText={setSearchString}
        />
      </View>

      <FlatList
        data={rows}
        keyExtractor={(item) => view === RECEIVED ? `${item.orderID}-${item.lineNo}` : item.itemRecID}
        renderItem={RowItem}
        ListEmptyComponent={<Text style={styles.emptyText}>{view === RECEIVED ? 'No Received Items' : 'No Items Found'}</Text>}
      />

      {rows.length === LIST_LIMIT ?
        <Text style={styles.limitText}>Showing the first {LIST_LIMIT}, use the search</Text> :
        <></>}

      <View style={styles.footerContainer}>

        {view === RECEIVED ?
          <View style={styles.collectedRow}>
            <Text style={styles.summaryText} numberOfLines={1}>
              {summary.lines} Received  ·  Qty {FormatQuantity(Math.round(summary.qty * 1000))}
              {summary.pendingOrders > 0 ? <Text style={styles.pendingText}>  ·  to upload</Text> : ''}
            </Text>
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
            { label: 'Received Items', value: RECEIVED },
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
  pendingText: {
    color: 'red',
  },
});

export default FreeReceivingItemsScreen;
