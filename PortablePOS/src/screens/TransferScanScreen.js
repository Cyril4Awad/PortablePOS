import React, { useState, useEffect, useRef, useCallback } from 'react';
import { View, Text, TextInput, StyleSheet, TouchableOpacity, Keyboard, Vibration, Image, FlatList, ScrollView, Alert } from 'react-native';
import { useNavigation, useFocusEffect } from '@react-navigation/native';
import Colors from '../variables/Colors';
import { FontSize } from '../variables/ResponsiveFont';
import ChoiceField, { choiceRowStyle } from '../components/controls/ChoiceField';
import PhysicalCountFilterPicker from '../components/lists/physicalCount/PhysicalCountFilterPicker';
import TransferItemPicker from '../components/lists/transfer/TransferItemPicker';
import TransferLotPicker from '../components/lists/transfer/TransferLotPicker';
import TransferDetailsModal from '../components/lists/transfer/TransferDetailsModal';
import { useTransferDownload } from '../components/lists/transfer/TransferDownload';
import TransferDatabase from '../database/TransferDatabase';
import TransferSettings from '../configuration/TransferSettings';
import { PlayScanOk, PlayScanError, PlayQtyConfirmed } from '../variables/PhysicalCountSounds';
import {
  SEARCH_FIELDS, SEARCH_FIELD_LABELS, ONE, LIST_LIMIT, LookupScan, LotText, FormatQuantity,
  CheckTransferQty, CheckLineQty, ParseTransferQty,
} from '../variables/TransferRules';

// Transfer on one screen : the header folds away (from location = the one of the server, server and machine open the settings,
// the stock downloaded, the way of work and the destination, deliver by / remarks), under it the scan and the items
// Scan a barcode or search a reference, the lot of the item is transferred
// - One location : every scan adds 1 to its line (the qty can be changed in the list), all the lines go to the destination
// - Multiple locations : one item at a time, its qty is typed for each destination
// The way of work is locked once the transfer has an item, until its items are cleared
// 1 item, 2 items
const Plural = (count, word) => `${count} ${word}${count === 1 ? '' : 's'}`;

const TransferScanScreen = () => {

  // The settings : { server, location, machineName }
  const [settings, setSettings] = useState({ server: '', location: null, machineName: '' });
  const [headerOpen, setHeaderOpen] = useState(false);
  const firstLoad = useRef(true);

  const [info, setInfo] = useState(null);
  const [header, setHeader] = useState({ multipleLocations: 0, destination: '', deliverBy: '', remarks: '' });
  const [destinations, setDestinations] = useState([]);
  const [summary, setSummary] = useState({ lines: 0, lots: 0, qty: 0, destinations: 0 });
  const [lines, setLines] = useState([]);

  const [searchField, setSearchField] = useState(SEARCH_FIELDS.BARCODE);
  const [scanText, setScanText] = useState('');
  const [typing, setTyping] = useState(false);

  const [status, setStatus] = useState({ text: 'Ready to scan', isError: false });

  // Multiple locations : the lot being dispatched and the typed qty of each destination
  const [current, setCurrent] = useState(null);
  const [destinationQtys, setDestinationQtys] = useState({});

  const [destinationPickerVisible, setDestinationPickerVisible] = useState(false);
  const [detailsVisible, setDetailsVisible] = useState(false);
  const [itemPickText, setItemPickText] = useState(null);
  // { item, lots }
  const [lotPick, setLotPick] = useState(null);

  const scanRef = useRef();
  const queue = useRef(Promise.resolve());

  // One location : the qty box of each line, the line whose qty box gets the focus after the next load,
  // the line whose qty was just confirmed with Enter (its end of editing is not saved twice)
  const lineQtyRefs = useRef({});
  const focusLineRequest = useRef(null);
  const submittedLine = useRef(null);

  // Multiple locations : the qty box of each destination, the typed texts (read by the checks), the last one typed
  const destinationRefs = useRef({});
  const destinationQtysRef = useRef({});
  const lastDestination = useRef(null);
  const setDestinationTexts = (texts) => { destinationQtysRef.current = texts; setDestinationQtys(texts); };

  // Typing a qty with the soft keyboard : the bars and buttons are hidden so the list keeps some height
  // (on the small scanners the keyboard leaves it no room, Android then drops the focus of the qty box and closes the keyboard).
  // The keyboard shrinks the screen before it says it is shown, so the bars are hidden when a qty box is touched or focused,
  // and shown again when the keyboard closes (hardware Enter, back key) : the qty box keeps the focus, Save stays reachable
  const [compact, setCompact] = useState(false);
  const qtyFocused = useRef(false);
  const qtyBlurTimer = useRef(null);
  const setCompactState = (value) => setCompact(value);

  // Moving from one qty box to the next one blurs then focuses : the layout does not change in between
  const QtyFocus = () => {
    clearTimeout(qtyBlurTimer.current);
    qtyFocused.current = true;
    setCompactState(true);
  };
  const QtyBlur = () => {
    clearTimeout(qtyBlurTimer.current);
    qtyBlurTimer.current = setTimeout(() => {
      qtyFocused.current = false;
      setCompactState(false);
    }, 200);
  };

  // The bars come back a moment after the keyboard closes : a hardware Enter closes the keyboard while the focus
  // moves to the next qty box, changing the layout at that same moment loses the focus (tested on the NLS scanner)
  const keyboardHideTimer = useRef(null);
  useEffect(() => {
    const shown = Keyboard.addListener('keyboardDidShow', () => {
      clearTimeout(keyboardHideTimer.current);
      if (qtyFocused.current) setCompactState(true);
    });
    const hidden = Keyboard.addListener('keyboardDidHide', () => {
      clearTimeout(keyboardHideTimer.current);
      keyboardHideTimer.current = setTimeout(() => setCompactState(false), 500);
    });
    return () => {
      shown.remove();
      hidden.remove();
      clearTimeout(qtyBlurTimer.current);
      clearTimeout(keyboardHideTimer.current);
    };
  }, []);

  // Leaving with the qtys of an item not saved (multiple locations) : asked first
  const navigation = useNavigation();
  const currentRef = useRef(null);
  useEffect(() => { currentRef.current = current; }, [current]);
  useEffect(() => navigation.addListener('beforeRemove', event => {
    if (!currentRef.current) return;
    event.preventDefault();
    Alert.alert('Quantities Not Saved', `The quantities of ${currentRef.current.item.itemReference} are not saved.`, [
      { text: 'Stay', style: 'cancel' },
      { text: 'Leave', style: 'destructive', onPress: () => { currentRef.current = null; navigation.dispatch(event.data.action); } },
    ]);
  }), [navigation]);

  const multiple = header.multipleLocations === 1;
  const decimals = info?.decimals ?? 0;
  const destinationOf = (code) => destinations.find(row => row.locationCode === code);

  //#region Loading

  const Reload = async () => {
    setHeader(await TransferDatabase.GetHeader());
    setSummary(await TransferDatabase.GetSummary());
    const loadedLines = await TransferDatabase.GetLines('', LIST_LIMIT);
    setLines(loadedLines);
    return loadedLines;
  };

  // The qty box of the requested line gets the focus once the list shows it (the whole qty selected)
  useEffect(() => {
    const lineID = focusLineRequest.current;
    if (lineID === null) return;
    focusLineRequest.current = null;
    setTimeout(() => {
      const input = lineQtyRefs.current[lineID];
      if (!input) return;
      if (input.isFocused()) input.blur();
      setTimeout(() => lineQtyRefs.current[lineID]?.focus(), 30);
    }, 80);
  }, [lines]);

  // The downloaded stock is the one of the location of the server
  const IsStockReady = (loadedInfo, location) => loadedInfo !== null && location !== null && loadedInfo.location === location.locationCode && loadedInfo.complete === 1;

  // On every visit (back from the settings) and after a download
  const LoadAll = async () => {
    const loadedSettings = {
      server: await TransferSettings.server(),
      location: await TransferSettings.location(),
      machineName: await TransferSettings.machineName(),
    };
    setSettings(loadedSettings);
    const loadedInfo = await TransferDatabase.GetInfo();
    setInfo(loadedInfo);
    setDestinations(loadedInfo ? await TransferDatabase.GetDestinations(loadedInfo.location) : []);
    await Reload();

    const ready = IsStockReady(loadedInfo, loadedSettings.location);
    if (!firstLoad.current) return;
    firstLoad.current = false;

    const savedField = await TransferSettings.searchField();
    if (SEARCH_FIELD_LABELS[savedField]) setSearchField(savedField);

    // The header is open until the stock is downloaded
    if (!ready) {
      setHeaderOpen(true);
      showStatus('Download the stock first', true);
      return;
    }
    const loadedHeader = await TransferDatabase.GetHeader();
    if (loadedHeader.multipleLocations === 0 && loadedHeader.destination === '') {
      showStatus('Choose the location to transfer to');
      setDestinationPickerVisible(true);
    }
  };

  useFocusEffect(
    useCallback(() => {
      LoadAll().catch(error => showStatus(error.message, true));
    }, [])
  );

  const { Download, ProgressModal } = useTransferDownload(() => LoadAll().catch(error => showStatus(error.message, true)));

  const stockReady = IsStockReady(info, settings.location);

  //#endregion

  //#region Helpers

  const enqueue = (action) => {
    queue.current = queue.current.then(action).catch(error => {
      PlayScanError();
      showStatus(error.message, true);
    });
  };

  // The bars are shown again first (a hidden scan box cannot take the focus)
  const focusScan = () => {
    clearTimeout(qtyBlurTimer.current);
    setCompactState(false);
    setTimeout(() => scanRef.current?.focus(), 80);
  };

  const showStatus = (text, isError = false) => setStatus({ text, isError });

  // focus = false : the focus stays where it is (a qty box being typed)
  const ShowError = (text, focus = true) => {
    PlayScanError();
    Vibration.vibrate([0, 60, 60, 60]);
    showStatus(text, true);
    if (focus) focusScan();
  };

  const Confirmed = (text) => {
    PlayQtyConfirmed();
    Vibration.vibrate(15);
    showStatus(text);
  };

  //#endregion

  //#region Scan

  const HandleScan = () => {
    const text = scanText;
    setScanText('');
    ScanText(text);
  };

  // A scan of the scan field, or a barcode scanned into a qty box
  const ScanText = (text) => {

    enqueue(async () => {
      if (text.trim() === '') return;
      if (!stockReady) {
        ShowError(info !== null && settings.location !== null && info.location !== settings.location.locationCode
          ? `The stock downloaded is the one of location ${info.location}, download location ${settings.location.locationCode}`
          : 'Download the stock first');
        setHeaderOpen(true);
        return;
      }
      setHeaderOpen(false);
      if (!multiple && header.destination === '') {
        ShowError('Choose the location to transfer to');
        setDestinationPickerVisible(true);
        return;
      }
      const result = await LookupScan(TransferDatabase, searchField, text);
      switch (result.kind) {
        case 'found':
          await ProcessItem(result.itemRecID);
          break;
        case 'pick':
          setItemPickText(result.value);
          break;
        case 'notFound':
          ShowError(`${SEARCH_FIELD_LABELS[searchField]} Not Found : ${result.value}`);
          break;
      }
    });

  };

  // The lots of the item : none = no available qty (blocked), one is taken, several open the list
  const ProcessItem = async (itemRecID) => {
    const item = await TransferDatabase.GetItem(itemRecID);
    const lots = await TransferDatabase.GetItemLots(itemRecID);

    if (lots.length === 0) {
      ShowError(`${item.itemReference} : the available qty in ${info.location} is 0`);
      return;
    }
    if (!lots.some(lot => lot.lotAvailable - lot.transferQty > 0)) {
      ShowError(`${item.itemReference} : all the available qty is already in the transfer`);
      return;
    }

    PlayScanOk();
    if (lots.length === 1) {
      await UseLot(item, lots[0]);
    }
    else {
      showStatus(`${item.itemReference} : choose the lot`);
      setLotPick({ item, lots });
    }
  };

  const UseLot = async (item, lot) => {

    if (!multiple) {
      const check = CheckTransferQty(ONE, lot.lotAvailable, lot.transferQty);
      if (!check.ok) { ShowError(`${item.itemReference} : ${check.message}`); return; }
      const total = await TransferDatabase.AddLotQty(item, lot, header.destination, ONE);
      // its qty box gets the focus : the qty is typed, Enter goes back to the scan
      const loadedLines = await TransferDatabase.GetLines('', LIST_LIMIT);
      focusLineRequest.current = loadedLines.find(row => row.lotRecID === lot.lotRecID)?.lineID ?? null;
      await Reload();
      Confirmed(`${item.itemReference} = ${FormatQuantity(total)} : type the qty, then Enter`);
      return;
    }

    await OpenLotEditor(item, lot);
  };

  // Multiple locations : every destination with the qty already given to it, to be changed
  const OpenLotEditor = async (item, lot) => {
    const qtys = await TransferDatabase.GetLotQtys(lot.lotRecID);
    const texts = {};
    Object.keys(qtys).forEach(code => { texts[code] = FormatQuantity(qtys[code]); });
    const existingQty = Object.values(qtys).reduce((sum, qty) => sum + qty, 0);
    setDestinationTexts(texts);
    lastDestination.current = null;
    // allowed : the available qty of the lot less the qty of its lines that are not in this card (none, all its lines are here)
    setCurrent({ item, lot, existingQtys: qtys, existingQty, allowed: lot.lotAvailable - (lot.transferQty - existingQty) });
    showStatus(`${item.itemReference} : type the qty of each location`);
  };

  // An item of the list (multiple locations) : its lot is opened with all the destinations
  // A lot that is not in the last download any more keeps its qty (it can only go down)
  const EditTransferLot = (group) => {
    enqueue(async () => {
      const line = group.lines[0];
      const item = await TransferDatabase.GetItem(line.itemRecID) ?? {
        itemRecID: line.itemRecID, itemReference: line.itemReference, itemDescription: line.itemDescription,
        itemUnit: line.itemUnit, priceLC: line.priceLC, priceCC: line.priceCC,
      };
      const lot = await TransferDatabase.GetLot(line.lotRecID) ?? {
        lotRecID: line.lotRecID, itemRecID: line.itemRecID, lotExpiry: line.lotExpiry, lotBatch: line.lotBatch,
        lotColor: line.lotColor, lotGrid: line.lotGrid, lotSize: line.lotSize, lotAvailable: group.qty, transferQty: group.qty,
      };
      await OpenLotEditor(item, lot);
    });
  };

  //#endregion

  //#region One location

  const ChangeDestination = (code) => {
    enqueue(async () => {
      await TransferDatabase.SetDestination(code);
      await Reload();
      const destination = destinationOf(code);
      showStatus(`Transfer to ${code}${destination ? ' - ' + destination.locationDescription : ''}`);
      focusScan();
    });
  };

  // fromEnter : the qty was confirmed with Enter, the scan gets the focus back when it is saved
  // More than the available qty : the line goes back to 1 and its qty box keeps the focus to type it again
  const SaveLineQty = (line, text, fromEnter) => {
    enqueue(async () => {
      const parsed = ParseTransferQty(text, decimals);
      const check = parsed.ok ? await CheckLineQty(TransferDatabase, line, parsed.value) : parsed;
      if (!check.ok) {
        let message = check.message;
        if (parsed.ok) {
          const one = await CheckLineQty(TransferDatabase, line, ONE);
          if (one.ok) {
            await TransferDatabase.SetLineQty(line.lineID, ONE);
            message += ', reset to 1';
          }
        }
        focusLineRequest.current = line.lineID;
        await Reload();
        ShowError(`${line.itemReference} : ${message}`, false);
        return;
      }
      if (check.value !== line.qty) {
        await TransferDatabase.SetLineQty(line.lineID, check.value);
        await Reload();
        Confirmed(`${line.itemReference} = ${FormatQuantity(check.value)}`);
      }
      if (fromEnter) focusScan();
    });
  };

  const DeleteLine = (line) => {
    Alert.alert('Remove Item', `Do you want to remove ${line.itemReference} (${FormatQuantity(line.qty)}) from the transfer ?`, [
      { text: 'No', style: 'cancel' },
      {
        text: 'Remove', style: 'destructive', onPress: () => enqueue(async () => {
          await TransferDatabase.DeleteLine(line.lineID);
          await Reload();
          showStatus(`${line.itemReference} removed`);
        })
      },
    ]);
  };

  //#endregion

  //#region Multiple locations

  // The total of the typed qtys cannot go over the allowed qty : the destination typed goes back to 0
  // Returns true when the qty of the destination is kept
  const CheckDestination = (code) => {
    if (!current) return true;
    const texts = destinationQtysRef.current;
    const parsed = ParseTransferQty(texts[code] ?? '', decimals, true);
    if (!parsed.ok) {
      setDestinationTexts({ ...texts, [code]: '0' });
      ShowError(`${code} : ${parsed.message}, reset to 0`, false);
      return false;
    }
    const total = Object.keys(texts).reduce((sum, key) => {
      const value = ParseTransferQty(texts[key], decimals, true);
      return sum + (value.ok ? value.value : 0);
    }, 0);
    if (total > current.allowed) {
      setDestinationTexts({ ...texts, [code]: '0' });
      ShowError(`${code} : only ${FormatQuantity(current.allowed)} available in total, reset to 0`, false);
      return false;
    }
    return true;
  };

  const focusDestination = (code) => setTimeout(() => {
    const input = destinationRefs.current[code];
    if (!input) return;
    if (input.isFocused()) input.blur();
    setTimeout(() => destinationRefs.current[code]?.focus(), 30);
  }, 60);

  // Enter : the next destination gets the focus (the last one closes the keyboard),
  // a qty over the available one is reset to 0 and stays to be typed again
  const SubmitDestination = (code) => {
    if (!CheckDestination(code)) {
      focusDestination(code);
      return;
    }
    const index = destinations.findIndex(row => row.locationCode === code);
    const next = destinations[index + 1];
    if (next) focusDestination(next.locationCode);
    else Keyboard.dismiss();
  };

  const SaveDestinationQtys = () => {

    if (!current) return;

    enqueue(async () => {

      const qtys = {};
      let total = 0;
      for (const destination of destinations) {
        const text = destinationQtysRef.current[destination.locationCode] ?? '';
        const parsed = ParseTransferQty(text, decimals, true);
        if (!parsed.ok) { ShowError(`${destination.locationCode} : ${parsed.message}`); return; }
        // only the destinations typed or already in the transfer are saved (a cleared one is removed)
        if (parsed.value > 0 || current.existingQtys[destination.locationCode] !== undefined) qtys[destination.locationCode] = parsed.value;
        total += parsed.value;
      }

      const lot = await TransferDatabase.GetLot(current.lot.lotRecID);
      if (total > 0) {
        const check = CheckTransferQty(total, lot.lotAvailable, lot.transferQty - current.existingQty);
        if (!check.ok) {
          // the last destination typed goes back to 0
          if (lastDestination.current) {
            setDestinationTexts({ ...destinationQtysRef.current, [lastDestination.current]: '0' });
            ShowError(`${current.item.itemReference} : ${check.message}, ${lastDestination.current} reset to 0`, false);
          }
          else ShowError(`${current.item.itemReference} : ${check.message}`, false);
          return;
        }
      }

      await TransferDatabase.SetLotQtys(current.item, current.lot, qtys);
      await Reload();
      Confirmed(`${current.item.itemReference} : ${FormatQuantity(total)} to ${Plural(Object.values(qtys).filter(qty => qty > 0).length, 'location')}`);
      setCurrent(null);
      setDestinationTexts({});
      focusScan();

    });

  };

  //#endregion

  //#region Save transfer

  const SaveTransfer = () => {
    if (summary.lines === 0) {
      ShowError('No items in the transfer');
      return;
    }
    if (!multiple && header.destination === '') {
      ShowError('Choose the location to transfer to');
      setDestinationPickerVisible(true);
      return;
    }
    Alert.alert('Save Transfer', `${Plural(summary.lots, 'item')}, qty ${FormatQuantity(summary.qty)}. The saving of the transfer to the server is not available yet.`, [
      { text: 'OK', onPress: focusScan },
    ]);
  };

  // The way of work can be changed again afterwards
  const ClearTransfer = () => {
    if (summary.lines === 0) {
      ShowError('No items in the transfer');
      return;
    }
    Alert.alert('Clear Transfer', `Do you want to remove the ${summary.lines} lines of the transfer ?`, [
      { text: 'No', style: 'cancel', onPress: focusScan },
      {
        text: 'Clear', style: 'destructive', onPress: () => enqueue(async () => {
          await TransferDatabase.ClearLines();
          setCurrent(null);
          setDestinationTexts({});
          await Reload();
          showStatus('Transfer cleared');
          focusScan();
        })
      },
    ]);
  };

  // The stock row : download it again, or look at it (and at the lines of every destination)
  const StockChoice = () => {
    Alert.alert('Stock', stockReady ? `Stock of ${info.location} downloaded ${info.downloadDate}` : 'The stock is not downloaded', [
      { text: 'Cancel', style: 'cancel' },
      ...(info !== null ? [{ text: 'View Items', onPress: () => navigation.navigate('TransferItems') }] : []),
      { text: 'Download', onPress: () => Download({ ...settings, info, lines: summary.lines }) },
    ]);
  };

  //#endregion

  //#region Way of work and details

  const ToggleMultiple = () => {

    const change = () => enqueue(async () => {
      await TransferDatabase.SetMultipleLocations(!multiple);
      setCurrent(null);
      setDestinationTexts({});
      await Reload();
      showStatus(!multiple ? 'Multiple locations : scan an item, then type its qty per location' : 'One location : choose it, then scan the items');
      if (multiple && header.destination === '') setDestinationPickerVisible(true);
      focusScan();
    });

    if (summary.lines === 0) {
      change();
      return;
    }

    Alert.alert('Change The Way Of Work', `The transfer has ${summary.lines} lines. The way of work can only change once its items are cleared.`, [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Clear Items', style: 'destructive', onPress: () => enqueue(async () => {
          await TransferDatabase.ClearLines();
          change();
        })
      },
    ]);

  };

  //#endregion

  //#region Render

  const LineRow = ({ item: line }) => (
    <View style={styles.lineContainer}>
      <View style={styles.lineTextContainer}>
        <Text style={styles.lineReferenceText} numberOfLines={1}>{line.itemReference}</Text>
        <Text style={styles.lineDescriptionText} numberOfLines={1}>{line.itemDescription}</Text>
        {LotText(line) !== '' ? <Text style={styles.lineLotText} numberOfLines={1}>{LotText(line)}</Text> : <></>}
      </View>
      <TextInput
        key={`${line.lineID}-${line.qty}`}
        ref={input => { lineQtyRefs.current[line.lineID] = input; }}
        onPressIn={QtyFocus}
        onFocus={QtyFocus}
        onBlur={QtyBlur}
        style={styles.lineQtyText}
        defaultValue={FormatQuantity(line.qty)}
        keyboardType='numeric'
        selectTextOnFocus={true}
        blurOnSubmit={false}
        maxLength={16}
        onSubmitEditing={event => {
          submittedLine.current = line.lineID;
                const text = event.nativeEvent.text.trim();
          // letters, symbols or more than 9 digits : the next item was scanned into the qty box, the qty is kept
          if (text !== '' && !/^[0-9]{1,9}([.,][0-9]*)?$/.test(text)) {
            lineQtyRefs.current[line.lineID]?.setNativeProps({ text: FormatQuantity(line.qty) });
            ScanText(text);
            return;
          }
          SaveLineQty(line, text, true);
        }}
        onEndEditing={event => {
          // already saved by Enter
          if (submittedLine.current === line.lineID) { submittedLine.current = null; return; }
          SaveLineQty(line, event.nativeEvent.text, false);
        }}
      />
      <TouchableOpacity style={styles.lineDeleteButton} onPress={() => DeleteLine(line)}>
        <Text style={styles.lineDeleteText}>✕</Text>
      </TouchableOpacity>
    </View>
  );

  const DestinationRow = (destination) => (
    <View key={destination.locationCode} style={styles.destinationRow}>
      <View style={styles.lineTextContainer}>
        <Text style={styles.destinationCodeText} numberOfLines={1}>{destination.locationCode}</Text>
        <Text style={styles.lineDescriptionText} numberOfLines={1}>{destination.locationDescription}</Text>
      </View>
      <TextInput
        style={styles.lineQtyText}
        ref={input => { destinationRefs.current[destination.locationCode] = input; }}
        onPressIn={QtyFocus}
        onFocus={QtyFocus}
        onBlur={QtyBlur}
        value={destinationQtys[destination.locationCode] ?? ''}
        placeholder='0'
        keyboardType='numeric'
        selectTextOnFocus={true}
        blurOnSubmit={false}
        returnKeyType='next'
        maxLength={16}
        onChangeText={text => {
          lastDestination.current = destination.locationCode;
          setDestinationTexts({ ...destinationQtysRef.current, [destination.locationCode]: text });
        }}
        onSubmitEditing={() => SubmitDestination(destination.locationCode)}
        onEndEditing={() => CheckDestination(destination.locationCode)}
      />
    </View>
  );

  // Multiple locations : one row per lot with the qty of each destination
  const transferLots = [];
  if (multiple) {
    const groups = {};
    lines.forEach(line => {
      if (!groups[line.lotRecID]) {
        groups[line.lotRecID] = { lotRecID: line.lotRecID, lines: [], qty: 0 };
        transferLots.push(groups[line.lotRecID]);
      }
      groups[line.lotRecID].lines.push(line);
      groups[line.lotRecID].qty += line.qty;
    });
  }

  const TransferLotRow = ({ item: group }) => {
    const line = group.lines[0];
    const lotText = LotText(line);
    const destinationsText = [...group.lines]
      .sort((first, second) => first.destination.localeCompare(second.destination))
      .map(row => `${row.destination} ${FormatQuantity(row.qty)}`).join('  ·  ');
    return (
      <TouchableOpacity style={styles.lineContainer} onPress={() => EditTransferLot(group)}>
        <View style={styles.lineTextContainer}>
          <Text style={styles.lineReferenceText} numberOfLines={1}>{line.itemReference}</Text>
          <Text style={styles.lineDescriptionText} numberOfLines={1}>{line.itemDescription}</Text>
          {lotText !== '' ? <Text style={styles.lineLotText} numberOfLines={1}>{lotText}</Text> : <></>}
          <Text style={styles.lotDestinationsText} numberOfLines={2}>{destinationsText}</Text>
        </View>
        <View style={styles.lotTotalContainer}>
          <Text style={styles.lotTotalText}>{FormatQuantity(group.qty)}</Text>
          <Text style={styles.lotTotalLabelText}>{Plural(group.lines.length, 'Location')}</Text>
        </View>
      </TouchableOpacity>
    );
  };

  const typedTotal = Object.values(destinationQtys).reduce((sum, text) => {
    const parsed = ParseTransferQty(text, 3, true);
    return sum + (parsed.ok ? parsed.value : 0);
  }, 0);
  const destination = destinationOf(header.destination);

  const location = settings.location;
  let stockText = 'Not Downloaded';
  if (info !== null && location !== null && info.location !== location.locationCode) stockText = `Location ${info.location}, download ${location.locationCode}`;
  else if (info !== null && info.complete === 0) stockText = 'Download Not Completed';
  else if (info !== null) stockText = `${info.itemsStored} Items  ·  ${info.lotsStored} Lots  ·  ${info.downloadDate}`;

  // Folded : from → to, or what is missing
  const headerSummary = !stockReady ? 'Stock not downloaded'
    : `${location.locationCode} → ${multiple ? 'Multiple Locations' : header.destination === '' ? 'Choose The Location' : `${header.destination}${destination ? ' - ' + destination.locationDescription : ''}`}`;

  return (
    <View style={styles.container}>

      {ProgressModal()}

      <PhysicalCountFilterPicker
        visible={destinationPickerVisible}
        title='Transfer To'
        options={destinations.map(row => ({ code: row.locationCode, description: `${row.locationCode} - ${row.locationDescription}` }))}
        selected={header.destination !== '' ? [header.destination] : []}
        multiple={false}
        onSubmit={(codes) => {
          setDestinationPickerVisible(false);
          if (codes[0] !== header.destination) ChangeDestination(codes[0]);
          else focusScan();
        }}
        onCancel={() => { setDestinationPickerVisible(false); focusScan(); }}
      />

      <TransferDetailsModal
        visible={detailsVisible}
        deliverBy={header.deliverBy}
        remarks={header.remarks}
        onSave={(deliverBy, remarks) => {
          setDetailsVisible(false);
          enqueue(async () => {
            await TransferDatabase.SaveHeaderTexts(deliverBy, remarks);
            await Reload();
            showStatus('Transfer details saved');
            focusScan();
          });
        }}
        onClose={() => { setDetailsVisible(false); focusScan(); }}
      />

      <TransferItemPicker
        visible={itemPickText !== null}
        searchText={itemPickText ?? ''}
        currencyLC={info?.currencyLC}
        onPick={(item) => {
          setItemPickText(null);
          enqueue(() => ProcessItem(item.itemRecID));
        }}
        onClose={() => { setItemPickText(null); focusScan(); }}
      />

      <TransferLotPicker
        item={lotPick?.item ?? null}
        lots={lotPick?.lots ?? []}
        onPick={(lot) => {
          const item = lotPick.item;
          setLotPick(null);
          enqueue(() => UseLot(item, lot));
        }}
        onClose={() => { setLotPick(null); focusScan(); }}
      />

      {/* The header : tap to fold or unfold it */}
      <TouchableOpacity style={[styles.headerToggle, compact ? styles.hidden : null]} onPress={() => { Keyboard.dismiss(); setHeaderOpen(open => !open); }}>
        <Text style={[styles.headerToggleText, !stockReady ? { color: 'red' } : null]} numberOfLines={1}>
          {headerOpen ? 'Header' : headerSummary}
        </Text>
        <Text style={styles.headerToggleArrow}>{headerOpen ? '▲' : '▼'}</Text>
      </TouchableOpacity>

      {headerOpen ?
        <ScrollView style={[styles.header, compact ? styles.hidden : null]} keyboardShouldPersistTaps='handled'>

          {/* The location of the server, it cannot change */}
          <ChoiceField
            title='From Location'
            value={location ? `${location.locationCode} - ${location.locationDescription}` : 'Open the settings'}
            valueStyle={!location ? { color: 'red' } : null}
            onPress={!location ? () => navigation.navigate('TransferSettings') : undefined}
            style={{ borderRightWidth: 0 }}
          />

          <View style={choiceRowStyle.row}>
            <View style={styles.fieldHalf}>
              <ChoiceField
                title='Server'
                value={settings.server === '' ? 'Not Defined' : settings.server}
                onPress={() => navigation.navigate('TransferSettings')}
                style={{ borderBottomWidth: 0 }}
              />
            </View>
            <View style={styles.fieldHalf}>
              <ChoiceField
                title='Machine'
                value={settings.machineName === '' ? 'Not Defined' : settings.machineName}
                onPress={() => navigation.navigate('TransferSettings')}
                style={{ borderRightWidth: 0, borderBottomWidth: 0 }}
              />
            </View>
          </View>

          <ChoiceField
            title='Stock'
            value={stockText}
            valueStyle={!stockReady ? { color: 'red' } : null}
            onPress={StockChoice}
            style={{ borderRightWidth: 0 }}
          />

          <ChoiceField
            title='Deliver By / Remarks'
            value={[header.deliverBy, header.remarks].filter(text => text !== '').join('  ·  ') || 'None'}
            onPress={() => setDetailsVisible(true)}
            style={{ borderRightWidth: 0 }}
          />

          {/* Way of work, destination (one location) */}
          <View style={styles.barContainer}>
            <TouchableOpacity style={[styles.pillButton, styles.multipleButton, multiple ? { flex: 1 } : null]} onPress={ToggleMultiple}>
              <Image
                source={multiple ? require('../assets/icons/checked.png') : require('../assets/icons/unchecked.png')}
                style={[styles.checkImage, { opacity: summary.lines > 0 ? 0.5 : 1 }]}
                resizeMode='contain'
              />
              <Text style={styles.pillButtonText} numberOfLines={1}>{multiple ? 'Multiple Locations' : 'Multiple'}</Text>
            </TouchableOpacity>
            {!multiple ?
              <TouchableOpacity style={[styles.pillButton, styles.destinationButton]} onPress={() => setDestinationPickerVisible(true)}>
                <Text style={styles.pillButtonText} numberOfLines={1}>
                  {header.destination === '' ? 'Transfer To' : `To ${header.destination}${destination ? ' - ' + destination.locationDescription : ''}`} ▾
                </Text>
              </TouchableOpacity> :
              <></>}
          </View>

        </ScrollView> :
        <></>}

      {/* Search field (tap to switch barcode / reference) + scan */}
      <View style={[styles.entryContainer, compact ? styles.hidden : null]}>
        <TouchableOpacity style={[styles.pillButton, styles.fieldButton]} onPress={() => {
          const next = searchField === SEARCH_FIELDS.BARCODE ? SEARCH_FIELDS.REFERENCE : SEARCH_FIELDS.BARCODE;
          setSearchField(next);
          TransferSettings.saveSearchField(next);
          showStatus(`Scan or type the ${SEARCH_FIELD_LABELS[next].toLowerCase()}`);
          focusScan();
        }}>
          <Text style={styles.pillButtonText} numberOfLines={1}>{SEARCH_FIELD_LABELS[searchField]} ⇄</Text>
        </TouchableOpacity>
        <TextInput
          ref={scanRef}
          style={styles.entryText}
          value={scanText}
          placeholder={`Scan ${SEARCH_FIELD_LABELS[searchField]}`}
          showSoftInputOnFocus={typing}
          onPressIn={() => { setHeaderOpen(false); if (!typing) Keyboard.dismiss(); }}
          autoCapitalize='characters'
          autoCorrect={false}
          blurOnSubmit={false}
          maxLength={100}
          onChangeText={setScanText}
          onSubmitEditing={HandleScan}
        />
        <TouchableOpacity style={[styles.keyboardButton, typing ? styles.keyboardButtonOn : null]} onPress={() => {
          const next = !typing;
          setTyping(next);
          scanRef.current?.blur();
          if (next) focusScan(); else Keyboard.dismiss();
        }}>
          <Image source={require('../assets/icons/keyboard.png')} style={styles.keyboardImage} resizeMode='contain' />
        </TouchableOpacity>
      </View>

      <View style={[styles.statusContainer, { backgroundColor: status.isError ? 'red' : Colors.brandBlue }]}>
        <Text style={styles.statusText} numberOfLines={1}>{status.text}</Text>
      </View>

      {!multiple ?
        // One location : the lines of the transfer, the qty is changed in the list
        <FlatList
          style={styles.list}
          data={lines}
          keyExtractor={(line) => line.lineID.toString()}
          renderItem={LineRow}
          keyboardShouldPersistTaps='handled'
          removeClippedSubviews={false}
          ListEmptyComponent={<Text style={styles.emptyText}>Scan the items to transfer</Text>}
        /> :
        current ?
          // Multiple locations : the scanned lot and the qty of each destination
          <View style={styles.list}>
            <View style={[styles.currentContainer, compact ? styles.currentContainerCompact : null]}>
              {!compact ?
                <>
                  <Text style={styles.lineReferenceText} numberOfLines={1}>{current.item.itemReference}</Text>
                  <Text style={styles.lineDescriptionText} numberOfLines={1}>{current.item.itemDescription}</Text>
                </> :
                <></>}
              <View style={styles.currentInfoRow}>
                <Text style={compact ? styles.lineReferenceText : styles.lineLotText} numberOfLines={1}>
                  {compact ? current.item.itemReference : LotText(current.lot) || 'No Expiry / Lot'}
                </Text>
                <Text style={[styles.currentAvailableText, { color: typedTotal > current.allowed ? 'red' : Colors.brandGreen }]}>
                  {FormatQuantity(typedTotal)} / {FormatQuantity(current.allowed)} {current.item.itemUnit}
                </Text>
              </View>
            </View>
            {/* Not a FlatList : when the keyboard shrinks it, it drops the row being typed (the focus and the keyboard are lost) */}
            <ScrollView keyboardShouldPersistTaps='handled'>
              {destinations.map(DestinationRow)}
            </ScrollView>
          </View> :
          // Multiple locations : the items of the transfer, tap one to change its qty in every location
          <FlatList
            style={styles.list}
            data={transferLots}
            keyExtractor={(group) => group.lotRecID}
            renderItem={TransferLotRow}
            keyboardShouldPersistTaps='handled'
            ListEmptyComponent={<Text style={styles.emptyText}>Scan an item, then type its qty for each location</Text>}
          />}

      {/* Always visible at the bottom */}
      <View style={[styles.bottomContainer, compact ? styles.hidden : null]}>
        <Text style={styles.summaryText} numberOfLines={1}>
          {Plural(summary.lots, 'Item')}  ·  Qty {FormatQuantity(summary.qty)}{multiple ? `  ·  ${Plural(summary.destinations, 'Location')}` : ''}
        </Text>
        {!(multiple && current) ?
          <View style={styles.actionsContainer}>
            <TouchableOpacity style={[styles.actionButton, styles.saveTransferButton, { opacity: summary.lines === 0 ? 0.5 : 1 }]} onPress={SaveTransfer}>
              <Text style={styles.actionButtonText}>Save Transfer</Text>
            </TouchableOpacity>
            <TouchableOpacity style={[styles.actionButton, styles.clearTransferButton, { opacity: summary.lines === 0 ? 0.5 : 1 }]} onPress={ClearTransfer}>
              <Text style={styles.actionButtonText}>Clear</Text>
            </TouchableOpacity>
          </View> :
          <></>}
        {multiple && current ?
          <View style={styles.actionsContainer}>
            <TouchableOpacity style={[styles.actionButton, { backgroundColor: 'gray' }]} onPress={() => { setCurrent(null); setDestinationTexts({}); showStatus('Ready to scan'); focusScan(); }}>
              <Text style={styles.actionButtonText}>Cancel</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.actionButton} onPress={SaveDestinationQtys}>
              <Text style={styles.actionButtonText}>Save Quantities</Text>
            </TouchableOpacity>
          </View> :
          <></>}
      </View>

    </View>
  );

  //#endregion
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.bgColor,
  },
  barContainer: {
    backgroundColor: Colors.brandGreen,
    height: 48,
    paddingHorizontal: 5,
    flexDirection: 'row',
    alignItems: 'center',
    borderBottomWidth: 1,
    borderBottomColor: 'white',
  },
  pillButton: {
    height: 38,
    borderRadius: 15,
    backgroundColor: 'white',
    borderWidth: 1,
    borderColor: Colors.buttonBorderColor,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 8,
  },
  pillButtonText: {
    color: Colors.brandBlue,
    fontSize: FontSize(15),
    fontWeight: 'bold',
  },
  multipleButton: {
    flexDirection: 'row',
    marginRight: 5,
  },
  checkImage: {
    width: 22,
    height: 22,
    marginRight: 5,
  },
  destinationButton: {
    flex: 1,
  },
  detailsButton: {
    width: 44,
    marginLeft: 5,
    paddingHorizontal: 0,
  },
  detailsButtonFilled: {
    backgroundColor: 'rgb(220, 240, 255)',
  },
  fieldButton: {
    width: 115,
    marginRight: 5,
  },
  entryContainer: {
    backgroundColor: Colors.brandGreen,
    width: '100%',
    height: 52,
    padding: 5,
    flexDirection: 'row',
    alignItems: 'center',
  },
  entryText: {
    flex: 1,
    height: '100%',
    backgroundColor: 'white',
    color: 'black',
    fontSize: FontSize(18),
    borderWidth: 1,
    borderColor: 'gainsboro',
    borderRadius: 4,
    paddingLeft: 10,
    paddingVertical: 0,
  },
  keyboardButton: {
    width: 42,
    height: '100%',
    marginLeft: 5,
    borderRadius: 4,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: 'rgba(255,255,255,0.35)',
  },
  keyboardButtonOn: {
    backgroundColor: 'white',
  },
  keyboardImage: {
    width: 30,
    height: 30,
  },
  statusContainer: {
    width: '100%',
    height: 32,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 10,
  },
  statusText: {
    color: 'white',
    fontSize: FontSize(16),
    fontWeight: 'bold',
  },
  list: {
    flex: 1,
  },
  emptyContainer: {
    justifyContent: 'center',
  },
  emptyText: {
    textAlign: 'center',
    color: 'gray',
    fontSize: FontSize(16),
    padding: 20,
  },
  lineContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    minHeight: 64,
    backgroundColor: Colors.listItemBackColor,
    marginBottom: 2,
    paddingLeft: 10,
    paddingRight: 5,
    paddingVertical: 4,
  },
  lineTextContainer: {
    flex: 1,
  },
  lineReferenceText: {
    color: 'black',
    fontSize: FontSize(16),
    fontWeight: 'bold',
  },
  lineDescriptionText: {
    color: Colors.brandBlue,
    fontSize: FontSize(14),
  },
  lineLotText: {
    flexShrink: 1,
    color: 'gray',
    fontSize: FontSize(13),
    fontStyle: 'italic',
  },
  lineQtyText: {
    width: 80,
    height: 42,
    marginLeft: 8,
    backgroundColor: 'white',
    color: 'black',
    fontSize: FontSize(19),
    borderWidth: 1,
    borderColor: 'silver',
    borderRadius: 4,
    textAlign: 'center',
    paddingVertical: 0,
  },
  lineDeleteButton: {
    width: 36,
    height: 42,
    alignItems: 'center',
    justifyContent: 'center',
  },
  lineDeleteText: {
    color: 'red',
    fontSize: FontSize(20),
    fontWeight: 'bold',
  },
  hidden: {
    display: 'none',
  },
  lotDestinationsText: {
    color: Colors.brandGreen,
    fontSize: FontSize(13),
    fontWeight: 'bold',
  },
  lotTotalContainer: {
    alignItems: 'flex-end',
    justifyContent: 'center',
    marginLeft: 8,
    minWidth: 70,
  },
  lotTotalText: {
    color: 'black',
    fontSize: FontSize(19),
    fontWeight: 'bold',
  },
  lotTotalLabelText: {
    color: 'gray',
    fontSize: FontSize(12),
  },
  saveTransferButton: {
    backgroundColor: Colors.brandGreen,
  },
  clearTransferButton: {
    backgroundColor: 'red',
  },
  headerToggle: {
    flexDirection: 'row',
    alignItems: 'center',
    height: 38,
    paddingLeft: 10,
    paddingRight: 8,
    backgroundColor: Colors.listItemBackColor,
    borderBottomWidth: 1,
    borderBottomColor: 'silver',
  },
  headerToggleText: {
    flex: 1,
    color: Colors.brandBlue,
    fontSize: FontSize(16),
    fontWeight: 'bold',
  },
  headerToggleArrow: {
    color: Colors.brandBlue,
    fontSize: FontSize(14),
    marginLeft: 6,
  },
  // folds the fields away when the screen is too small for the header and the scan
  header: {
    flexGrow: 0,
    flexShrink: 1,
  },
  fieldHalf: {
    width: '50%',
  },
  currentContainerCompact: {
    paddingVertical: 3,
  },
  currentContainer: {
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderBottomWidth: 1,
    borderBottomColor: 'silver',
  },
  currentInfoRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  currentAvailableText: {
    fontSize: FontSize(16),
    fontWeight: 'bold',
    marginLeft: 8,
  },
  destinationRow: {
    flexDirection: 'row',
    alignItems: 'center',
    minHeight: 54,
    backgroundColor: Colors.listItemBackColor,
    marginBottom: 2,
    paddingLeft: 12,
    paddingRight: 10,
  },
  destinationCodeText: {
    color: 'black',
    fontSize: FontSize(16),
    fontWeight: 'bold',
  },
  bottomContainer: {
    borderTopWidth: 1,
    borderTopColor: 'silver',
    backgroundColor: Colors.bgColor,
    paddingHorizontal: 10,
    paddingVertical: 6,
  },
  summaryText: {
    color: Colors.brandBlue,
    fontSize: FontSize(15),
    fontWeight: 'bold',
    textAlign: 'center',
  },
  actionsContainer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginTop: 6,
  },
  actionButton: {
    width: '48.5%',
    height: 45,
    borderRadius: 15,
    backgroundColor: Colors.brandBlue,
    alignItems: 'center',
    justifyContent: 'center',
  },
  actionButtonText: {
    color: 'white',
    fontSize: FontSize(16),
    fontWeight: 'bold',
  },
});

export default TransferScanScreen;
