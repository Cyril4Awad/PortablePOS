import React, { useState, useEffect, useRef, useCallback } from 'react';
import { View, Text, TextInput, StyleSheet, FlatList, TouchableOpacity, Alert, Vibration } from 'react-native';
import SwitchSelector from 'react-native-switch-selector';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import LoadingData from '../components/LoadingData';
import PhysicalCountCreateLine from '../components/lists/physicalCount/PhysicalCountCreateLine';
import PhysicalCountData, { NewRequestID, CREATE_TIMEOUT_MESSAGE } from '../data/PhysicalCountData';
import { PlayScanOk, PlayScanError } from '../variables/PhysicalCountSounds';
import { HasGroups, CreateSelection, CheckAll, UncheckAll, IsChecked, ToggleItem } from '../variables/PhysicalCountCreateRules';

const PAGE_SIZE = 100;
const SEARCH_DELAY_MS = 500;

// Create : tries with the same request when the answer does not come (the server never creates the count twice)
const CREATE_TRIES = 4;
const CanTryAgain = (error) => error.message === CREATE_TIMEOUT_MESSAGE || /try again|network|failed to fetch/i.test(error.message);

const ALL_ITEMS = 0;
const CHECKED_ITEMS = 1;

// Create Physical Count, step 2 : check the items, then create the count for this machine
// The list is read from the server page by page (the device never holds the whole item file),
// the server works out the checked items from the filters and the items checked / unchecked here
// Scanning a barcode (or typing a reference and Enter) checks the item of that barcode / reference,
// when no item has it exactly the items containing the text are shown (nothing is checked)
const PhysicalCountCreateItemsScreen = ({ route, navigation }) => {

  const { server, location, machineName, filters } = route.params;

  // The checked brands, categories... check their items : the list opens on the checked items
  const [view, setView] = useState(HasGroups(filters) ? CHECKED_ITEMS : ALL_ITEMS);
  const [searchString, setSearchString] = useState('');

  // Kept in a ref too : checking an item works on the latest selection without redrawing every line
  const [selection, setSelectionState] = useState(CreateSelection());
  const selectionRef = useRef(selection);
  const setSelection = (value) => { selectionRef.current = value; setSelectionState(value); };

  const [items, setItems] = useState([]);
  const [totalItems, setTotalItems] = useState(0);
  // The last page was full : there may be more items after it
  const [hasMore, setHasMore] = useState(false);
  const [checkedCount, setCheckedCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [creating, setCreating] = useState(false);

  // Result of the last scan : { text, isError }
  const [status, setStatus] = useState(null);

  // Only the last request updates the list (search typed fast, Check All while a page loads...)
  const requestID = useRef(0);
  const loadingMore = useRef(false);
  const searchTimer = useRef(null);
  // The search box is emptied after a scan : the list keeps the scanned item, it is not read again
  const skipNextSearch = useRef(false);
  const searchRef = useRef();

  // after : null for the first page, else the last item shown (the server reads the page after it, by reference)
  // search / onlySelected : the values to read with (a scan reads all the items with its text)
  // exactMatch : a scan, only the item of this barcode or of this reference
  const LoadPage = async (after, currentSelection, reset, search = searchString.trim(), onlySelected = view === CHECKED_ITEMS, exactMatch = false) => {

    const id = ++requestID.current;
    setLoading(true);

    try {

      const result = await PhysicalCountData.GetPhysicalCountItems(server, location, filters, currentSelection,
        search, onlySelected, after, PAGE_SIZE, exactMatch);

      if (id !== requestID.current) return null;

      setItems(current => reset ? result.items : [...current, ...result.items]);
      setHasMore(result.items.length === PAGE_SIZE);
      // The totals come with the first page only (-1 after), the checked count not with a search (it does not change)
      if (result.totalItems >= 0) setTotalItems(result.totalItems);
      if (result.selectedItems >= 0) setCheckedCount(result.selectedItems);

      return result;

    } catch (error) {
      if (id === requestID.current) alert(error.message);
      return null;
    } finally {
      if (id === requestID.current) setLoading(false);
      loadingMore.current = false;
    }

  };

  // First page again when the search or the view changes (the search waits until the user stops typing)
  useEffect(() => {
    if (skipNextSearch.current) {
      skipNextSearch.current = false;
      return;
    }
    searchTimer.current = setTimeout(() => LoadPage(null, selectionRef.current, true), searchString === '' ? 0 : SEARCH_DELAY_MS);
    return () => clearTimeout(searchTimer.current);
  }, [searchString, view]);

  const LoadMore = () => {
    if (loading || loadingMore.current || !hasMore || items.length === 0) return;
    loadingMore.current = true;
    LoadPage(items[items.length - 1], selectionRef.current, false);
  };

  // Same function for every line (memo lines) : only the line that changes is redrawn
  const Toggle = useCallback((item) => {
    const current = selectionRef.current;
    setCheckedCount(count => count + (IsChecked(current, item) ? -1 : 1));
    setSelection(ToggleItem(current, item));
  }, []);

  const ChangeAll = (newSelection) => {
    setStatus(null);
    setSelection(newSelection);
    LoadPage(null, newSelection, true);
  };

  // Enter in the search box (a scanner sends it after the barcode) : the item of this barcode or reference is checked,
  // the box is emptied for the next scan. No exact item : the items containing the text are shown, nothing is checked
  const ScanSubmit = async () => {

    clearTimeout(searchTimer.current);

    const text = searchString.trim();
    if (text === '') return;

    let result = await LoadPage(null, selectionRef.current, true, text, false, true);
    if (result === null) return;

    if (result.items.length === 0) {
      result = await LoadPage(null, selectionRef.current, true, text, false, false);
      if (result === null) return;
      if (result.items.length > 0) {
        setStatus({ text: `No item has ${text}, ${result.totalItems} items contain it`, isError: true });
        PlayScanError();
        Vibration.vibrate([0, 60, 60, 60]);
        skipNextSearch.current = true;
        setSearchString('');
        setTimeout(() => searchRef.current?.focus(), 80);
        return;
      }
    }

    if (result.items.length === 1) {
      const item = result.items[0];
      if (IsChecked(selectionRef.current, item)) {
        setStatus({ text: `${item.itemReference} is already checked`, isError: false });
      }
      else {
        Toggle(item);
        setStatus({ text: `${item.itemReference} checked`, isError: false });
      }
      PlayScanOk();
      Vibration.vibrate(15);
    }
    else if (result.items.length === 0) {
      setStatus({ text: `Item Not Found : ${text}`, isError: true });
      PlayScanError();
      Vibration.vibrate([0, 60, 60, 60]);
    }
    else {
      setStatus({ text: `${result.totalItems} items have ${text}, tap the one to check`, isError: false });
    }

    skipNextSearch.current = true;
    setSearchString('');
    setTimeout(() => searchRef.current?.focus(), 80);

  };

  const Create = () => {

    if (checkedCount === 0) {
      alert('No Items Checked');
      return;
    }

    Alert.alert('Create Physical Count', `Create a physical count of ${checkedCount} items for machine ${machineName} ?`, [
      { text: 'No', style: 'cancel' },
      { text: 'Yes', onPress: () => RunCreate() },
    ]);

  };

  // The same request is sent again when the answer does not come in time (timeout, connection lost, stock busy) :
  // the server returns the count of the first try, so the count is never created twice and its number is always known
  const RunCreate = async () => {

    setCreating(true);

    const requestID = NewRequestID(machineName);
    let result = null;
    let lastError = null;

    for (let attempt = 1; attempt <= CREATE_TRIES && result === null; attempt++) {
      try {
        result = await PhysicalCountData.CreatePhysicalCount(server, location, machineName, filters, selectionRef.current, '', requestID);
      } catch (error) {
        lastError = error;
        if (!CanTryAgain(error)) break;
      }
    }

    setCreating(false);

    if (result === null) {
      alert(CanTryAgain(lastError)
        ? 'The server did not answer. Do not create the count again : open My Counts on the Create Physical Count page in a few minutes, the count will be there if it was created.'
        : lastError.message);
      return;
    }

    // Back to the Physical Count page, which offers to download the new count
    Alert.alert('Physical Count Created', `Physical Count ${result.countNumber}\n${result.itemsCount} Items - ${result.linesCount} Lines`, [
      {
        text: 'OK', onPress: () => navigation.reset({
          index: 1,
          routes: [{ name: 'Home' }, { name: 'PhysicalCount', params: { newCountNumber: result.countNumber } }],
        })
      },
    ], { cancelable: false });

  };

  const LineItem = ({ item }) => (
    <PhysicalCountCreateLine item={item} checked={IsChecked(selection, item)} onToggle={Toggle} />
  );

  return (
    <View style={styles.container}>

      <View style={styles.searchContainer}>
        <TextInput
          ref={searchRef}
          style={styles.searchText}
          placeholder='Search Items Or Scan'
          maxLength={200}
          value={searchString}
          blurOnSubmit={false}
          onChangeText={text => {
            setStatus(null);
            setSearchString(text);
          }}
          onSubmitEditing={ScanSubmit}
        />
      </View>

      {/* Checked items and Check All / Uncheck All : blue and red like the scan screens */}
      <View style={styles.checkContainer}>
        <Text style={styles.checkedText} numberOfLines={1}>{checkedCount} Checked</Text>
        <TouchableOpacity style={styles.smallButton} onPress={() => ChangeAll(CheckAll())}>
          <Text style={styles.smallButtonText}>Check All</Text>
        </TouchableOpacity>
        <TouchableOpacity style={[styles.smallButton, { backgroundColor: 'red' }]} onPress={() => ChangeAll(UncheckAll())}>
          <Text style={styles.smallButtonText}>Uncheck All</Text>
        </TouchableOpacity>
      </View>

      {/* Result of the last scan : blue, red when not found */}
      {status !== null ?
        <View style={[styles.statusContainer, { backgroundColor: status.isError ? 'red' : Colors.brandBlue }]}>
          <Text style={styles.statusText} numberOfLines={1}>{status.text}</Text>
        </View> :
        <></>}

      <FlatList
        data={items}
        keyExtractor={(item) => item.itemRecID}
        renderItem={LineItem}
        extraData={selection}
        onEndReached={LoadMore}
        onEndReachedThreshold={0.5}
        keyboardShouldPersistTaps='handled'
        windowSize={7}
        ListEmptyComponent={loading ? <></> : <Text style={styles.emptyText}>No Items Found</Text>}
        ListFooterComponent={items.length > 0 && hasMore ?
          <Text style={styles.limitText}>{items.length} / {totalItems}</Text> : <></>}
      />

      <View style={styles.bottomContainer}>

        <View style={styles.switchContainer}>
          <SwitchSelector
            initial={view === CHECKED_ITEMS ? 1 : 0}
            onPress={(value) => { setStatus(null); setView(value); }}
            textColor={Colors.brandBlue}
            selectedColor='white'
            buttonColor={Colors.brandBlue}
            borderColor={Colors.brandBlue}
            hasPadding={true}
            options={[
              { label: 'All Items', value: ALL_ITEMS },
              { label: 'Checked', value: CHECKED_ITEMS }
            ]}
          />
        </View>

        <TouchableOpacity style={[styles.createButton, { opacity: checkedCount === 0 || creating ? 0.5 : 1 }]} disabled={creating} onPress={Create}>
          <Text style={styles.createButtonText} numberOfLines={1}>Create Count ({checkedCount} Items)</Text>
        </TouchableOpacity>

      </View>

      {/* Last : over everything, nothing can be pressed while it shows */}
      {loading || creating ? <LoadingData /> : <></>}

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
  checkContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 10,
    paddingBottom: 6,
  },
  checkedText: {
    flex: 1,
    color: Colors.brandGreen,
    fontSize: FontSize(17),
    fontWeight: 'bold',
  },
  smallButton: {
    height: 34,
    borderRadius: 15,
    backgroundColor: Colors.brandBlue,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 12,
    marginLeft: 6,
  },
  smallButtonText: {
    color: 'white',
    fontSize: FontSize(15),
  },
  statusContainer: {
    width: '100%',
    height: 35,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 10,
  },
  statusText: {
    color: 'white',
    fontSize: FontSize(17),
    fontWeight: 'bold',
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
  bottomContainer: {
    borderTopWidth: 1,
    borderTopColor: 'silver',
    backgroundColor: Colors.bgColor,
    paddingBottom: 8,
  },
  switchContainer: {
    width: '100%',
    padding: 5,
    alignItems: 'center',
    justifyContent: 'center',
  },
  createButton: {
    marginHorizontal: 10,
    height: 45,
    borderRadius: 15,
    backgroundColor: Colors.brandBlue,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 5,
  },
  createButtonText: {
    color: 'white',
    fontSize: FontSize(17),
    fontWeight: 'bold',
  },
});

export default PhysicalCountCreateItemsScreen;
