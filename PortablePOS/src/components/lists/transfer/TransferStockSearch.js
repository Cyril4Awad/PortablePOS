import React, { useState, useEffect, useRef, useMemo } from 'react';
import { View, StyleSheet, Text, TextInput, FlatList, Modal, TouchableOpacity, Image, ActivityIndicator } from 'react-native';
import SwitchSelector from 'react-native-switch-selector';
import Colors from '../../../variables/Colors';
import { FontSize } from '../../../variables/ResponsiveFont';
import ModalTitle from '../../ModalTitle';
import ItemProductGroup from '../stock/ItemProductGroup';
import TransferDatabase from '../../../database/TransferDatabase';
import TransferData from '../../../data/TransferData';
import TransferSettings from '../../../configuration/TransferSettings';
import { LotText, FormatQuantity } from '../../../variables/TransferRules';

// The stock search of Transfer, like the stock search of the Receiving Order : the stock of the location is read from the server
// (every lot with an available qty, shown before anything is typed), then searched here :
// Product Groups (then the items of the group) or Items, one row per lot of an item (expiry / lot) with the qty left
// - One location (checks = true) : the lots are checked (Check All / Uncheck All), Add adds them to the transfer
// - Multiple locations : the lot tapped is returned, its qty is then typed for each location
// A lot whose available qty is all in the transfer already cannot be picked
// props : visible, server, location, checks, onAdd(rows) (checks), onPick(row) (no checks), onClose
// rows : the lot rows of the server (TransferData) with transferQty, the qty of the lot already in the transfer

// The stock read last (read when the Transfer screen opens, PreloadTransferStock) : the search opens at once with it,
// then reads it again in the background (the first call to a server left idle can take several seconds)
let stockCache = { key: '', rows: null, promise: null };

const ReadStock = (server, location) => {
  const key = `${server}|${location}`;
  const promise = TransferData.GetTransferStock(server, location)
    .then(stock => {
      const rows = stock.map(row => ({
        ...row,
        searchText: `${row.itemReference} ${row.itemSingleRef1} ${row.itemSingleRef2} ${row.itemBarcode} ${row.itemDescription}`.toUpperCase(),
      }));
      if (stockCache.key === key) stockCache.rows = rows;
      return rows;
    })
    .finally(() => { if (stockCache.key === key && stockCache.promise === promise) stockCache.promise = null; });
  if (stockCache.key !== key) stockCache = { key, rows: null, promise };
  else stockCache.promise = promise;
  return promise;
};

export const PreloadTransferStock = (server, location) => {
  if (!server || !location) return;
  if (stockCache.key === `${server}|${location}` && stockCache.promise) return;
  ReadStock(server, location).catch(() => { });
};

const TransferStockSearch = props => {

  const [itemsMode, setItemsMode] = useState(false);
  // the stock of the location : null while it is read
  const [allRows, setAllRows] = useState(null);
  const [error, setError] = useState(null);
  const [groupText, setGroupText] = useState('');
  // { itemProductGroup, itemProductGroupDesc } : the items of a product group, null : the list of the groups
  const [group, setGroup] = useState(null);
  const [searchText, setSearchText] = useState('');
  // the text searched, a moment after the typing stops (the list is not filtered again on every letter)
  const [appliedText, setAppliedText] = useState('');
  useEffect(() => {
    const timer = setTimeout(() => setAppliedText(searchText), 250);
    return () => clearTimeout(timer);
  }, [searchText]);
  // the stock read in the background is shown when it arrives
  const [refreshing, setRefreshing] = useState(false);
  // lotRecID => row
  const [checked, setChecked] = useState({});
  const searchRef = useRef();

  // Opened : the stock read last at once (with the qty of each lot in the transfer), then read again from the server
  const WithTransfer = async (rows) => {
    const qtys = await TransferDatabase.GetTransferQtys();
    return rows.map(row => ({ ...row, transferQty: qtys[row.lotRecID] ?? 0 }));
  };

  const LoadStock = async () => {
    setError(null);
    const key = `${props.server}|${props.location}`;
    const cached = stockCache.key === key ? stockCache.rows : null;
    if (cached) setAllRows(await WithTransfer(cached));
    else setAllRows(null);
    setRefreshing(true);
    try {
      const read = stockCache.key === key && stockCache.promise ? stockCache.promise : ReadStock(props.server, props.location);
      setAllRows(await WithTransfer(await read));
    }
    catch (err) {
      if (!cached) setError(err.message);
    }
    finally {
      setRefreshing(false);
    }
  };

  useEffect(() => {
    if (!props.visible) return;
    setGroup(null);
    setGroupText('');
    setSearchText('');
    setChecked({});
    setAppliedText('');
    TransferSettings.searchItemsMode().then(setItemsMode);
    LoadStock();
  }, [props.visible]);

  const showItems = props.visible && (itemsMode || group !== null);

  // The product groups of the stock (computed once per stock read)
  const groups = useMemo(() => {
    const list = [];
    const groupIndex = {};
    (allRows ?? []).forEach(row => {
      if (groupIndex[row.itemProductGroup] === undefined) {
        groupIndex[row.itemProductGroup] = list.length;
        list.push({ itemProductGroup: row.itemProductGroup, itemProductGroupDesc: row.itemProductGroupDesc, items: new Set() });
      }
      list[groupIndex[row.itemProductGroup]].items.add(row.itemRecID);
    });
    list.forEach(row => { row.itemsCount = row.items.size; });
    return list.sort((first, second) => (first.itemProductGroupDesc || first.itemProductGroup).localeCompare(second.itemProductGroupDesc || second.itemProductGroup));
  }, [allRows]);

  // The lots of the group (or all the lots) with every word typed
  const rows = useMemo(() => {
    if (!showItems) return [];
    const words = appliedText.trim().toUpperCase().split(/\s+/).filter(word => word !== '');
    return (allRows ?? []).filter(row =>
      (itemsMode || row.itemProductGroup === group.itemProductGroup) && words.every(word => row.searchText.includes(word)));
  }, [allRows, showItems, itemsMode, group, appliedText]);

  const left = (row) => row.lotAvailable - row.transferQty;
  const checkedCount = Object.keys(checked).length;

  const Toggle = (row) => {
    const next = { ...checked };
    if (next[row.lotRecID]) delete next[row.lotRecID];
    else next[row.lotRecID] = row;
    setChecked(next);
  };

  // The rows shown that can still be transferred
  const CheckAll = () => {
    const next = { ...checked };
    rows.filter(row => left(row) > 0).forEach(row => { next[row.lotRecID] = row; });
    setChecked(next);
  };

  const Close = () => {
    setChecked({});
    props.onClose();
  };

  const GroupRow = ({ item: row }) => (
    <ItemProductGroup
      itemProductGroupCode={row.itemProductGroup === '' ? 'No Product Group' : row.itemProductGroup}
      itemProductGroupDesc={`${row.itemProductGroupDesc}  (${row.itemsCount})`}
      onPress={() => { setSearchText(''); setGroup(row); }}
    />
  );

  const LotRow = ({ item: row }) => {
    const rowLeft = left(row);
    const isChecked = !!checked[row.lotRecID];
    const lotText = LotText(row);
    return (
      <TouchableOpacity
        style={[styles.lotRow, isChecked ? styles.lotRowChecked : null, { opacity: rowLeft > 0 ? 1 : 0.45 }]}
        disabled={rowLeft <= 0}
        onPress={() => props.checks ? Toggle(row) : props.onPick(row)}>
        {props.checks ?
          <Image
            source={isChecked ? require('../../../assets/icons/checked.png') : require('../../../assets/icons/unchecked.png')}
            style={styles.checkImage}
            resizeMode='contain'
          /> :
          <></>}
        <View style={styles.lotTextContainer}>
          <Text style={styles.referenceText} numberOfLines={1}>{row.itemReference}</Text>
          <Text style={styles.descriptionText} numberOfLines={2}>{row.itemDescription}</Text>
          <Text style={styles.lotText} numberOfLines={1}>
            {lotText !== '' ? lotText + '   ·   ' : ''}Avlb {FormatQuantity(row.lotAvailable)}{row.transferQty > 0 ? `   ·   In Transfer ${FormatQuantity(row.transferQty)}` : ''}
          </Text>
        </View>
        <View style={styles.leftContainer}>
          <Text style={[styles.leftText, { color: rowLeft > 0 ? Colors.brandGreen : 'red' }]}>{FormatQuantity(Math.max(rowLeft, 0))}</Text>
          <Text style={styles.unitText} numberOfLines={1}>{row.itemUnit}</Text>
        </View>
      </TouchableOpacity>
    );
  };

  const groupFilter = groupText.trim().toUpperCase();
  const shownGroups = groupFilter === '' ? groups
    : groups.filter(row => `${row.itemProductGroup} ${row.itemProductGroupDesc}`.toUpperCase().includes(groupFilter));

  return (
    <Modal visible={props.visible} onRequestClose={() => group !== null && !itemsMode ? setGroup(null) : Close()}>
      <View style={styles.container}>

        <ModalTitle title='Items List - Transfer' onCloseButtonPress={Close} />

        {refreshing && allRows !== null ? <Text style={styles.refreshingText}>Updating the available qty...</Text> : <></>}

        <View style={styles.switchContainer}>
          <SwitchSelector
            key={itemsMode ? 'items' : 'groups'}
            initial={itemsMode ? 1 : 0}
            onPress={value => {
              setItemsMode(value === 1);
              TransferSettings.saveSearchItemsMode(value === 1);
            }}
            textColor={Colors.brandBlue}
            selectedColor='white'
            buttonColor={Colors.brandBlue}
            borderColor={Colors.brandBlue}
            hasPadding={true}
            options={[
              { label: 'Product Groups', value: 0 },
              { label: 'Items', value: 1 }
            ]}
          />
        </View>

        {/* The items of a product group : back to the groups */}
        {!itemsMode && group !== null ?
          <TouchableOpacity style={styles.groupBar} onPress={() => setGroup(null)}>
            <Text style={styles.groupBarText} numberOfLines={1}>◀  {group.itemProductGroupDesc || group.itemProductGroup || 'No Product Group'}</Text>
          </TouchableOpacity> :
          <></>}

        <View style={styles.searchContainer}>
          <TextInput
            ref={searchRef}
            style={styles.searchText}
            placeholder={showItems ? 'Search Items' : 'Search Product Groups'}
            maxLength={60}
            value={showItems ? searchText : groupText}
            autoCorrect={false}
            onChangeText={text => showItems ? setSearchText(text) : setGroupText(text)}
          />
        </View>

        {allRows === null ?
          // reading the stock, or it cannot be read : tap to read it again
          <TouchableOpacity style={styles.list} disabled={error === null} onPress={LoadStock}>
            {error === null ?
              <ActivityIndicator style={styles.loading} size='large' color={Colors.brandBlue} /> :
              <Text style={[styles.emptyText, { color: 'red' }]}>{error}{'\n\n'}Tap to try again</Text>}
          </TouchableOpacity> :
          showItems ?
          <FlatList
            style={styles.list}
            data={rows}
            keyExtractor={row => row.lotRecID}
            renderItem={LotRow}
            keyboardShouldPersistTaps='handled'
            removeClippedSubviews={false}
            initialNumToRender={15}
            ListEmptyComponent={<Text style={styles.emptyText}>No item with an available qty</Text>}
          /> :
          <FlatList
            style={styles.list}
            data={shownGroups}
            keyExtractor={row => row.itemProductGroup}
            renderItem={GroupRow}
            keyboardShouldPersistTaps='handled'
            removeClippedSubviews={false}
            contentContainerStyle={{ alignItems: 'center' }}
            ListEmptyComponent={<Text style={styles.emptyText}>No item with an available qty</Text>}
          />}

        {/* One location : check the lots, then add them */}
        {props.checks ?
          <View style={styles.bottomContainer}>
            <View style={styles.buttonsRow}>
              <TouchableOpacity style={[styles.button, { opacity: showItems && rows.length > 0 ? 1 : 0.5 }]} disabled={!showItems} onPress={CheckAll}>
                <Image source={require('../../../assets/icons/checked.png')} style={styles.buttonImage} resizeMode='contain' />
                <Text style={styles.buttonText} numberOfLines={1}>Check All</Text>
              </TouchableOpacity>
              <TouchableOpacity style={[styles.button, { opacity: checkedCount > 0 ? 1 : 0.5 }]} onPress={() => setChecked({})}>
                <Image source={require('../../../assets/icons/unchecked.png')} style={styles.buttonImage} resizeMode='contain' />
                <Text style={styles.buttonText} numberOfLines={1}>Uncheck All</Text>
              </TouchableOpacity>
            </View>
            <TouchableOpacity
              style={[styles.button, styles.addButton, { opacity: checkedCount > 0 ? 1 : 0.5 }]}
              disabled={checkedCount === 0}
              onPress={() => { const picked = Object.values(checked); setChecked({}); props.onAdd(picked); }}>
              <Image source={require('../../../assets/icons/add.png')} style={styles.buttonImage} resizeMode='contain' />
              <Text style={styles.buttonText} numberOfLines={1}>{checkedCount === 0 ? 'Add To Transfer' : `Add ${checkedCount} Item${checkedCount === 1 ? '' : 's'} To Transfer`}</Text>
            </TouchableOpacity>
          </View> :
          <></>}

      </View>
    </Modal>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.bgColor,
  },
  switchContainer: {
    width: '100%',
    paddingHorizontal: 5,
    paddingTop: 5,
  },
  groupBar: {
    height: 38,
    justifyContent: 'center',
    paddingHorizontal: 10,
    marginTop: 5,
    backgroundColor: Colors.listItemBackColor,
    borderBottomWidth: 1,
    borderBottomColor: 'silver',
  },
  groupBarText: {
    color: Colors.brandBlue,
    fontSize: FontSize(16),
    fontWeight: 'bold',
  },
  searchContainer: {
    width: '100%',
    height: 56,
    padding: 5,
  },
  searchText: {
    flex: 1,
    borderWidth: 1,
    borderColor: 'silver',
    paddingLeft: 15,
    paddingVertical: 0,
    fontSize: FontSize(20),
    backgroundColor: 'white',
    color: 'black',
  },
  list: {
    flex: 1,
  },
  loading: {
    marginTop: 40,
  },
  refreshingText: {
    color: 'gray',
    fontSize: FontSize(12),
    fontStyle: 'italic',
    textAlign: 'center',
    paddingTop: 2,
  },
  emptyText: {
    textAlign: 'center',
    color: 'gray',
    fontSize: FontSize(15),
    padding: 20,
  },
  lotRow: {
    flexDirection: 'row',
    alignItems: 'center',
    minHeight: 70,
    paddingHorizontal: 8,
    paddingVertical: 4,
    backgroundColor: Colors.listItemBackColor,
    marginBottom: 2,
  },
  lotRowChecked: {
    backgroundColor: 'rgb(220, 240, 255)',
  },
  checkImage: {
    width: 26,
    height: 26,
    marginRight: 8,
  },
  lotTextContainer: {
    flex: 1,
  },
  referenceText: {
    color: 'black',
    fontSize: FontSize(16),
    fontWeight: 'bold',
  },
  descriptionText: {
    color: Colors.brandBlue,
    fontSize: FontSize(15),
  },
  lotText: {
    color: 'gray',
    fontSize: FontSize(13),
    fontStyle: 'italic',
  },
  leftContainer: {
    alignItems: 'flex-end',
    marginLeft: 8,
    minWidth: 60,
  },
  leftText: {
    fontSize: FontSize(19),
    fontWeight: 'bold',
  },
  unitText: {
    color: 'gray',
    fontSize: FontSize(12),
  },
  bottomContainer: {
    borderTopWidth: 1,
    borderTopColor: 'silver',
    paddingHorizontal: 8,
    paddingTop: 8,
  },
  buttonsRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  // the look of MainButton (white, blue border, icon)
  button: {
    width: '49%',
    height: 50,
    marginBottom: 8,
    borderRadius: 15,
    borderWidth: 1,
    borderColor: Colors.buttonBorderColor,
    backgroundColor: Colors.buttonBackColor1,
    flexDirection: 'row',
    alignItems: 'center',
    paddingLeft: 8,
    paddingRight: 5,
    overflow: 'hidden',
  },
  addButton: {
    width: '100%',
    justifyContent: 'center',
  },
  buttonImage: {
    width: 32,
    height: 32,
    marginRight: 6,
  },
  buttonText: {
    fontSize: FontSize(18),
    color: Colors.buttonForeColor1,
    flexShrink: 1,
  },
});

export default TransferStockSearch;
