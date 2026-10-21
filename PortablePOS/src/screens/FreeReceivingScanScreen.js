import React, { useState, useEffect, useRef } from 'react';
import { View, Text, TextInput, StyleSheet, TouchableOpacity, Keyboard, Vibration, Image } from 'react-native';
import Colors from '../variables/Colors';
import { FontSize } from '../variables/ResponsiveFont';
import PhysicalCountFilterPicker from '../components/lists/physicalCount/PhysicalCountFilterPicker';
import FreeReceivingItemPicker from '../components/lists/freeReceiving/FreeReceivingItemPicker';
import FreeReceivingDatabase from '../database/FreeReceivingDatabase';
import FreeReceivingSettings from '../configuration/FreeReceivingSettings';
import { PlayScanOk, PlayScanError, PlayQtyConfirmed } from '../variables/PhysicalCountSounds';
import { ItemRecIDFromLink } from '../variables/ItemLink';
import {
  SEARCH_FIELDS, SEARCH_FIELD_LABELS, SEARCH_FIELD_OPTIONS, RECEIVE_TYPES, RECEIVE_TYPE_LABELS, RECEIVE_TYPE_COLORS,
  LookupScan, IsScannedQty, ParseReceivedQty, FormatQuantity, CheckInvoiceNumber, BarcodeToSearch,
} from '../variables/FreeReceivingRules';

// Compact field of the item details : the title takes the width it needs (the note at its right),
// the value stays on one line and shrinks to fit (a long barcode is never wrapped or hyphenated)
const InfoField = ({ title, data, toolTip }) => (
  <View style={styles.infoField}>
    <View style={styles.infoFieldHeader}>
      <Text style={styles.infoFieldTitleText} numberOfLines={1}>{title}</Text>
      {toolTip ? <Text style={styles.infoFieldToolTipText} numberOfLines={1}>{toolTip}</Text> : <></>}
    </View>
    <Text style={styles.infoFieldDataText} numberOfLines={1} adjustsFontSizeToFit={true} minimumFontScale={0.6}
      textBreakStrategy='simple' android_hyphenationFrequency='none'>{data}</Text>
  </View>
);

// Free Receiving scan : choose the supplier (and its invoice number), scan by the chosen field,
// the item is shown and the qty box gets the focus, Enter (or Add) adds the qty as Normal, Promotion or Free
// The scanner types the code then Enter (keyboard wedge), the keyboard button opens the soft keyboard to type
const FreeReceivingScanScreen = () => {

  const [decimals, setDecimals] = useState(0);
  const [removeCheckDigit, setRemoveCheckDigit] = useState(false);

  const [suppliers, setSuppliers] = useState([]);
  const [supplier, setSupplier] = useState(null);
  const [invoiceNumber, setInvoiceNumber] = useState('');
  const [searchField, setSearchField] = useState(SEARCH_FIELDS.BARCODE);

  const [scanText, setScanText] = useState('');
  const [typing, setTyping] = useState(false);

  // The scanned item : { ...item, barcodes, receivedQty }
  const [item, setItem] = useState(null);
  const [qtyText, setQtyText] = useState('1');
  const [receiveType, setReceiveType] = useState(RECEIVE_TYPES.NORMAL);
  const [linkedItem, setLinkedItem] = useState(null);
  const [linkText, setLinkText] = useState('');

  const [status, setStatus] = useState({ text: 'Ready to scan', isError: false });
  const [warningText, setWarningText] = useState('');

  const [supplierPickerVisible, setSupplierPickerVisible] = useState(false);
  const [fieldPickerVisible, setFieldPickerVisible] = useState(false);
  // { purpose: 'item' | 'link', field, value, exact }
  const [itemPick, setItemPick] = useState(null);

  const scanRef = useRef();
  const qtyRef = useRef();
  const linkRef = useRef();

  // Time of the first character typed in the qty box : a scan of the next item there is not taken as a qty
  const qtyFirstCharTime = useRef(null);

  // Scans and buttons run one at a time, in order
  const queue = useRef(Promise.resolve());

  //#region Loading

  useEffect(() => {

    (async () => {
      const info = await FreeReceivingDatabase.GetInfo();
      setDecimals(info?.decimals ?? 0);
      setRemoveCheckDigit(await FreeReceivingSettings.removeCheckDigit());

      const loadedSuppliers = await FreeReceivingDatabase.GetSuppliers();
      setSuppliers(loadedSuppliers);

      // The supplier, invoice and search field of the last time
      const saved = await FreeReceivingSettings.scanState();
      const savedSupplier = saved.supplierRecID ? loadedSuppliers.find(row => row.supplierRecID === saved.supplierRecID) : null;
      setSupplier(savedSupplier ?? null);
      setInvoiceNumber(savedSupplier ? (saved.invoiceNumber ?? '') : '');
      if (saved.searchField && SEARCH_FIELD_LABELS[saved.searchField]) setSearchField(saved.searchField);

      if (!savedSupplier) {
        showStatus('Choose the supplier first');
        setSupplierPickerVisible(true);
      }
    })().catch(error => showStatus(error.message, true));

  }, []);

  const SaveScanState = (changes) => {
    FreeReceivingSettings.saveScanState({
      supplierRecID: supplier?.supplierRecID,
      invoiceNumber: invoiceNumber,
      searchField: searchField,
      ...changes,
    });
  };

  //#endregion

  //#region Helpers

  const enqueue = (action) => {
    queue.current = queue.current.then(action).catch(error => {
      PlayScanError();
      showStatus(error.message, true);
    });
  };

  const focusField = (ref, selectAll = false) => setTimeout(() => {
    if (selectAll && ref.current?.isFocused()) {
      ref.current.blur();
      setTimeout(() => ref.current?.focus(), 30);
      return;
    }
    ref.current?.focus();
  }, 80);

  const showStatus = (text, isError = false) => setStatus({ text, isError });

  const ShowError = (text, ref = scanRef) => {
    PlayScanError();
    Vibration.vibrate([0, 60, 60, 60]);
    showStatus(text, true);
    focusField(ref);
  };

  const ResetEntry = () => {
    setQtyText('1');
    qtyFirstCharTime.current = null;
    setReceiveType(RECEIVE_TYPES.NORMAL);
    setLinkedItem(null);
    setLinkText('');
  };

  //#endregion

  //#region Scan

  // A found item is shown and its qty box gets the focus
  const ShowItem = async (itemRecID) => {
    const foundItem = await FreeReceivingDatabase.GetItem(itemRecID);
    if (!foundItem) throw new Error('Item Not Found');

    const barcodes = await FreeReceivingDatabase.GetItemBarcodes(itemRecID);
    const receivedQty = supplier ? await FreeReceivingDatabase.GetReceivedQty(supplier.supplierRecID, invoiceNumber.trim(), itemRecID) : 0;

    setItem({ ...foundItem, barcodes, receivedQty });
    ResetEntry();

    // Item of another supplier : received anyway, with a warning
    setWarningText(supplier && foundItem.itemSupplier !== '' && foundItem.itemSupplier !== supplier.supplierCode.toUpperCase()
      ? `Item of supplier ${foundItem.itemSupplier}, not ${supplier.supplierCode}` : '');

    PlayScanOk();
    Vibration.vibrate(15);
    showStatus(`${foundItem.itemReference} : enter the received qty`);
    focusField(qtyRef, true);
  };

  const SearchAndShow = async (text) => {
    const result = await LookupScan(FreeReceivingDatabase, searchField, text, removeCheckDigit);
    switch (result.kind) {
      case 'found':
        await ShowItem(result.itemRecID);
        break;
      case 'pick':
        setItemPick({ purpose: 'item', field: searchField, value: result.value, exact: result.exact });
        break;
      case 'notFound':
        setItem(null);
        setWarningText('');
        ShowError(`${SEARCH_FIELD_LABELS[searchField]} Not Found : ${result.value}`);
        break;
    }
  };

  const HandleScan = () => {

    const text = scanText;
    setScanText('');

    enqueue(async () => {
      if (text.trim() === '') return;
      if (!supplier) {
        ShowError('Choose the supplier first');
        setSupplierPickerVisible(true);
        return;
      }
      await SearchAndShow(text);
    });

  };

  //#endregion

  //#region Promotion link

  const HandleLink = () => {

    const text = linkText.trim();

    enqueue(async () => {
      if (text === '') {
        setItemPick({ purpose: 'link', field: 'any', value: '', exact: false });
        return;
      }

      // The QR of a label (link ending with the record id) : the item itself
      const linkRecID = ItemRecIDFromLink(text);
      if (linkRecID !== null) {
        const linkItem = await FreeReceivingDatabase.GetItem(linkRecID);
        if (linkItem) LinkItem(linkItem);
        else ShowError(`Linked Item Not Found : ${text}`, linkRef);
        return;
      }

      // Any code of the item : barcode, references, supplier references (the check digit as in the settings)
      const exactBarcode = await FreeReceivingDatabase.FindItems(SEARCH_FIELDS.BARCODE, BarcodeToSearch(text, removeCheckDigit), true, 2);
      const exact = exactBarcode.length > 0 ? exactBarcode : await FreeReceivingDatabase.FindItems(SEARCH_FIELDS.REFERENCE, text, true, 2);
      const matches = exact.length > 0 ? exact : await FreeReceivingDatabase.FindItems('any', text, false, 2);

      if (matches.length === 1) {
        LinkItem(matches[0]);
      }
      else if (matches.length > 1) {
        setItemPick({ purpose: 'link', field: 'any', value: text, exact: false });
      }
      else {
        ShowError(`Linked Item Not Found : ${text}`, linkRef);
      }
    });

  };

  const LinkItem = (pickedItem) => {
    setLinkedItem(pickedItem);
    setLinkText('');
    showStatus(`Promotion linked to ${pickedItem.itemReference}`);
    focusField(qtyRef, true);
  };

  //#endregion

  //#region Add

  const HandleAdd = () => {

    const text = qtyText;

    enqueue(async () => {

      if (!item) { ShowError('Scan an item first'); return; }
      if (!supplier) { ShowError('Choose the supplier first'); setSupplierPickerVisible(true); return; }

      const invoiceError = CheckInvoiceNumber(invoiceNumber);
      if (invoiceError !== '') { ShowError(invoiceError); return; }

      const qty = ParseReceivedQty(text, decimals);
      if (!qty.ok) {
        ShowError(qty.message, qtyRef);
        setQtyText('1');
        focusField(qtyRef, true);
        return;
      }

      const total = await FreeReceivingDatabase.AddReceivedQty({
        supplier,
        invoiceNumber: invoiceNumber.trim(),
        item,
        receiveType,
        linkedItem: receiveType === RECEIVE_TYPES.PROMOTION ? linkedItem : null,
        qty: qty.value,
      });

      const receivedQty = await FreeReceivingDatabase.GetReceivedQty(supplier.supplierRecID, invoiceNumber.trim(), item.itemRecID);
      setItem(current => current && current.itemRecID === item.itemRecID ? { ...current, receivedQty } : current);

      PlayQtyConfirmed();
      Vibration.vibrate(15);
      showStatus(`${item.itemReference} +${FormatQuantity(qty.value)} ${RECEIVE_TYPE_LABELS[receiveType]} = ${FormatQuantity(total)}`);

      // The next item starts again as Normal
      ResetEntry();
      focusField(scanRef);

    });

  };

  // Enter in the qty box : a typed qty is added, a scan there is the next item (the shown one is not added)
  const HandleQtySubmit = () => {

    const text = qtyText;
    const scanned = IsScannedQty(text, qtyFirstCharTime.current, Date.now());
    qtyFirstCharTime.current = null;

    if (!scanned) {
      HandleAdd();
      return;
    }

    setQtyText('1');
    enqueue(() => SearchAndShow(text));

  };

  //#endregion

  //#region Render

  const ReceiveTypeButton = (type) => {
    const selected = receiveType === type;
    return (
      <TouchableOpacity key={type}
        style={[styles.typeButton, selected ? { backgroundColor: RECEIVE_TYPE_COLORS[type], borderColor: RECEIVE_TYPE_COLORS[type] } : null]}
        onPress={() => {
          setReceiveType(type);
          if (type === RECEIVE_TYPES.PROMOTION) focusField(linkRef);
          else focusField(qtyRef, true);
        }}>
        <Text style={[styles.typeButtonText, selected ? { color: 'white', fontWeight: 'bold' } : null]}>{RECEIVE_TYPE_LABELS[type]}</Text>
      </TouchableOpacity>
    );
  };

  const isPromotion = receiveType === RECEIVE_TYPES.PROMOTION;

  return (
    <View style={styles.container}>

      <PhysicalCountFilterPicker
        visible={supplierPickerVisible}
        title='Supplier'
        options={suppliers.map(row => ({ code: row.supplierRecID, description: `${row.supplierCode} - ${row.supplierName}` }))}
        selected={supplier ? [supplier.supplierRecID] : []}
        multiple={false}
        onSubmit={(codes) => {
          const picked = suppliers.find(row => row.supplierRecID === codes[0]) ?? null;
          setSupplierPickerVisible(false);
          if (picked && picked.supplierRecID !== supplier?.supplierRecID) {
            // another supplier : another invoice
            setSupplier(picked);
            setInvoiceNumber('');
            setItem(null);
            setWarningText('');
            SaveScanState({ supplierRecID: picked.supplierRecID, invoiceNumber: '' });
            showStatus(`Receiving from ${picked.supplierCode} - ${picked.supplierName}`);
          }
          focusField(scanRef);
        }}
        onCancel={() => { setSupplierPickerVisible(false); focusField(scanRef); }}
      />

      <PhysicalCountFilterPicker
        visible={fieldPickerVisible}
        title='Search By'
        options={SEARCH_FIELD_OPTIONS}
        selected={[searchField]}
        multiple={false}
        onSubmit={(codes) => {
          setFieldPickerVisible(false);
          setSearchField(codes[0]);
          SaveScanState({ searchField: codes[0] });
          showStatus(`Scan or type the ${SEARCH_FIELD_LABELS[codes[0]].toLowerCase()}`);
          focusField(scanRef);
        }}
        onCancel={() => { setFieldPickerVisible(false); focusField(scanRef); }}
      />

      <FreeReceivingItemPicker
        visible={itemPick !== null}
        field={itemPick?.field ?? 'any'}
        searchText={itemPick?.value ?? ''}
        exact={itemPick?.exact}
        title={itemPick?.purpose === 'link' ? 'Link Promotion To' : 'Choose Item'}
        onPick={(pickedItem) => {
          const purpose = itemPick.purpose;
          setItemPick(null);
          if (purpose === 'link') LinkItem(pickedItem);
          else enqueue(() => ShowItem(pickedItem.itemRecID));
        }}
        onClose={() => {
          const purpose = itemPick?.purpose;
          setItemPick(null);
          focusField(purpose === 'link' ? linkRef : scanRef);
        }}
      />

      {/* Supplier and its invoice number */}
      <View style={styles.barContainer}>
        <TouchableOpacity style={[styles.pillButton, styles.supplierButton]} onPress={() => setSupplierPickerVisible(true)}>
          <Text style={styles.pillButtonText} numberOfLines={1}>{supplier ? `${supplier.supplierCode} - ${supplier.supplierName}` : 'Choose Supplier'} ▾</Text>
        </TouchableOpacity>
        <TextInput
          style={styles.invoiceText}
          value={invoiceNumber}
          placeholder='Supplier Inv No'
          maxLength={30}
          autoCapitalize='characters'
          autoCorrect={false}
          onChangeText={setInvoiceNumber}
          onEndEditing={() => SaveScanState({ invoiceNumber: invoiceNumber.trim() })}
          onSubmitEditing={() => focusField(scanRef)}
        />
      </View>

      {/* Search field + scan */}
      <View style={styles.entryContainer}>
        <TouchableOpacity style={[styles.pillButton, styles.fieldButton]} onPress={() => setFieldPickerVisible(true)}>
          <Text style={styles.pillButtonText} numberOfLines={1}>{SEARCH_FIELD_LABELS[searchField]} ▾</Text>
        </TouchableOpacity>
        <TextInput
          ref={scanRef}
          style={styles.entryText}
          value={scanText}
          placeholder={`Scan ${SEARCH_FIELD_LABELS[searchField]}`}
          showSoftInputOnFocus={typing}
          onPressIn={() => { if (!typing) Keyboard.dismiss(); }}
          autoFocus={true}
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
          if (next) focusField(scanRef); else Keyboard.dismiss();
        }}>
          <Image source={require('../assets/icons/keyboard.png')} style={styles.keyboardImage} resizeMode='contain' />
        </TouchableOpacity>
      </View>

      <View style={[styles.statusContainer, { backgroundColor: status.isError ? 'red' : Colors.brandBlue }]}>
        <Text style={styles.statusText} numberOfLines={1}>{status.text}</Text>
      </View>

      {warningText !== '' ?
        <View style={styles.noteContainer}>
          <Text style={styles.warningText} numberOfLines={1}>{warningText}</Text>
        </View> :
        <></>}

      {/* Item details : not scrollable, the qty entry below is always visible */}
      <View style={styles.detailsContainer}>

        <View style={styles.itemContainer}>
          <View style={styles.itemReferenceRow}>
            <Text style={styles.itemReferenceText} numberOfLines={1}>{item ? item.itemReference : 'Scan an item'}</Text>
            {item && item.itemUnit !== '' ? <Text style={styles.itemUnitText} numberOfLines={1}>{item.itemUnit}</Text> : <></>}
          </View>
          {item ? <Text style={styles.itemDescriptionText} numberOfLines={1}>{item.itemDescription}</Text> : <></>}
        </View>

        {item ?
          <>
            <View style={styles.fieldsRow}>
              <View style={styles.fieldThird}><InfoField title='Supplier' data={item.itemSupplier || '-'} /></View>
              <View style={styles.fieldThird}><InfoField title='Category' data={item.itemCategory || '-'} /></View>
              <View style={styles.fieldThird}><InfoField title='Serial' data={item.itemSerie || '-'} /></View>
            </View>
            <View style={styles.fieldsRow}>
              <View style={styles.fieldHalf}><InfoField title='Barcode' data={item.barcodes[0] ?? '-'} toolTip={item.barcodes.length > 1 ? `+${item.barcodes.length - 1} more` : ''} /></View>
              <View style={styles.fieldHalf}><InfoField title='Received' data={FormatQuantity(item.receivedQty)} toolTip={invoiceNumber.trim() !== '' ? 'this invoice' : ''} /></View>
            </View>
            <View style={styles.fieldsRow}>
              <View style={styles.fieldHalf}><InfoField title='Single Ref 1' data={item.itemSingleRef1 || '-'} /></View>
              <View style={styles.fieldHalf}><InfoField title='Single Ref 2' data={item.itemSingleRef2 || '-'} /></View>
            </View>
          </> :
          <></>}

      </View>

      {/* Always visible at the bottom : type, promotion link, qty */}
      <View style={styles.bottomContainer}>

        <View style={styles.typeContainer}>
          {Object.values(RECEIVE_TYPES).map(ReceiveTypeButton)}
        </View>

        {isPromotion ?
          <View style={styles.linkContainer}>
            <Text style={styles.linkLabelText}>Link</Text>
            {linkedItem ?
              <TouchableOpacity style={styles.linkedItemContainer} onPress={() => { setLinkedItem(null); focusField(linkRef); }}>
                <Text style={styles.linkedItemText} numberOfLines={1}>{linkedItem.itemReference}  ·  {linkedItem.itemDescription}</Text>
                <Text style={styles.linkedItemClearText}>✕</Text>
              </TouchableOpacity> :
              <TextInput
                ref={linkRef}
                style={styles.linkText}
                value={linkText}
                placeholder='Scan or type the linked item (optional)'
                autoCapitalize='characters'
                autoCorrect={false}
                blurOnSubmit={false}
                maxLength={100}
                onChangeText={setLinkText}
                onSubmitEditing={HandleLink}
              />}
            <TouchableOpacity style={styles.linkSearchButton} onPress={() => setItemPick({ purpose: 'link', field: 'any', value: linkText.trim(), exact: false })}>
              <Image source={require('../assets/icons/stock.png')} style={styles.linkSearchImage} resizeMode='contain' />
            </TouchableOpacity>
          </View> :
          <></>}

        <View style={styles.qtyContainer}>
          <Text style={styles.qtyLabelText}>Qty</Text>
          <TextInput
            ref={qtyRef}
            style={[styles.qtyText, { opacity: item ? 1 : 0.5 }]}
            value={qtyText}
            editable={item !== null}
            keyboardType='numeric'
            selectTextOnFocus={true}
            maxLength={16}
            blurOnSubmit={false}
            onChangeText={text => {
              if (qtyFirstCharTime.current === null) qtyFirstCharTime.current = Date.now();
              setQtyText(text);
            }}
            onSubmitEditing={HandleQtySubmit}
          />
          <TouchableOpacity style={[styles.addButton, { backgroundColor: RECEIVE_TYPE_COLORS[receiveType], opacity: item ? 1 : 0.5 }]} onPress={HandleAdd}>
            <Text style={styles.addButtonText} numberOfLines={1}>Add {RECEIVE_TYPE_LABELS[receiveType]}</Text>
          </TouchableOpacity>
        </View>

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
  // Green bars like the Physical Count scan screens
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
  supplierButton: {
    flex: 1.6,
  },
  fieldButton: {
    width: 120,
    marginRight: 5,
  },
  pillButtonText: {
    color: Colors.brandBlue,
    fontSize: FontSize(15),
    fontWeight: 'bold',
  },
  invoiceText: {
    flex: 1,
    height: 38,
    marginLeft: 5,
    backgroundColor: 'white',
    color: 'black',
    fontSize: FontSize(16),
    borderWidth: 1,
    borderColor: 'gainsboro',
    borderRadius: 4,
    paddingLeft: 8,
    paddingVertical: 0,
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
  noteContainer: {
    backgroundColor: Colors.listItemBackColor,
    paddingHorizontal: 10,
    paddingVertical: 3,
  },
  warningText: {
    color: 'red',
    fontSize: FontSize(15),
    fontWeight: 'bold',
    textAlign: 'center',
  },
  // Item and fields : fill the space left, never scroll (the qty entry stays visible)
  detailsContainer: {
    flex: 1,
    overflow: 'hidden',
  },
  itemContainer: {
    paddingHorizontal: 15,
    paddingVertical: 6,
    borderBottomWidth: 1,
    borderBottomColor: 'silver',
  },
  itemReferenceRow: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  itemReferenceText: {
    flex: 1,
    color: 'black',
    fontSize: FontSize(18),
    fontWeight: 'bold',
  },
  itemUnitText: {
    marginLeft: 8,
    color: 'gray',
    fontSize: FontSize(14),
  },
  itemDescriptionText: {
    color: Colors.brandBlue,
    fontSize: FontSize(16),
  },
  fieldsRow: {
    flexDirection: 'row',
  },
  infoField: {
    height: 50,
    paddingHorizontal: 10,
    justifyContent: 'center',
    borderColor: 'silver',
    borderBottomWidth: 1,
    overflow: 'hidden',
  },
  infoFieldHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    height: 20,
  },
  infoFieldTitleText: {
    flexShrink: 0,
    color: Colors.brandBlue,
    fontSize: FontSize(15),
    fontWeight: 'bold',
  },
  infoFieldToolTipText: {
    flex: 1,
    marginLeft: 6,
    textAlign: 'right',
    color: 'gray',
    fontSize: FontSize(13),
    fontStyle: 'italic',
  },
  infoFieldDataText: {
    fontSize: FontSize(17),
    color: 'black',
  },
  fieldHalf: {
    width: '50%',
  },
  fieldThird: {
    width: '33.33%',
  },
  bottomContainer: {
    borderTopWidth: 1,
    borderTopColor: 'silver',
    backgroundColor: Colors.bgColor,
    paddingHorizontal: 10,
    paddingTop: 8,
    paddingBottom: 4,
  },
  // Normal / Promotion / Free : segmented buttons, the selected one in the color of its type
  typeContainer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: 8,
  },
  typeButton: {
    width: '32%',
    height: 38,
    borderRadius: 15,
    backgroundColor: Colors.buttonBackColor1,
    borderWidth: 1,
    borderColor: Colors.buttonBorderColor,
    alignItems: 'center',
    justifyContent: 'center',
  },
  typeButtonText: {
    color: Colors.buttonForeColor1,
    fontSize: FontSize(16),
  },
  linkContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    height: 44,
    marginBottom: 8,
  },
  linkLabelText: {
    width: 45,
    color: 'darkorange',
    fontSize: FontSize(16),
    fontWeight: 'bold',
  },
  linkText: {
    flex: 1,
    height: '100%',
    backgroundColor: 'white',
    color: 'black',
    fontSize: FontSize(15),
    borderWidth: 1,
    borderColor: 'darkorange',
    borderRadius: 4,
    paddingLeft: 8,
    paddingVertical: 0,
  },
  linkedItemContainer: {
    flex: 1,
    height: '100%',
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: 'rgb(255, 240, 220)',
    borderWidth: 1,
    borderColor: 'darkorange',
    borderRadius: 4,
    paddingHorizontal: 8,
  },
  linkedItemText: {
    flex: 1,
    color: 'black',
    fontSize: FontSize(15),
    fontWeight: 'bold',
  },
  linkedItemClearText: {
    color: 'darkorange',
    fontSize: FontSize(16),
    fontWeight: 'bold',
    marginLeft: 6,
  },
  linkSearchButton: {
    width: 44,
    height: '100%',
    marginLeft: 5,
    alignItems: 'center',
    justifyContent: 'center',
    borderRadius: 4,
    borderWidth: 1,
    borderColor: 'darkorange',
  },
  linkSearchImage: {
    width: 30,
    height: 30,
  },
  qtyContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    height: 48,
  },
  qtyLabelText: {
    width: 45,
    color: Colors.brandBlue,
    fontSize: FontSize(18),
    fontWeight: 'bold',
  },
  qtyText: {
    width: 110,
    height: '100%',
    backgroundColor: 'white',
    color: 'black',
    fontSize: FontSize(22),
    borderWidth: 1,
    borderColor: 'silver',
    borderRadius: 4,
    textAlign: 'center',
    paddingVertical: 0,
  },
  addButton: {
    flex: 1,
    height: '100%',
    marginLeft: 10,
    borderRadius: 15,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 5,
  },
  addButtonText: {
    color: 'white',
    fontSize: FontSize(17),
    fontWeight: 'bold',
  },
});

export default FreeReceivingScanScreen;
