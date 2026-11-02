import React, { useState, useEffect, useRef } from 'react';
import { View, Text, TextInput, StyleSheet, TouchableOpacity, FlatList, ScrollView, Image, Modal, Keyboard, Vibration } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import ChoiceField, { choiceRowStyle, choiceTitleTextStyle } from '../components/controls/ChoiceField';
import MainButton from '../components/controls/MainButton';
import ItemProductGroups from '../components/lists/stock/ItemProductGroups';
import Items from '../components/lists/stock/Items';
import ModalTitle from '../components/ModalTitle';
import ROSupplierPicker from '../components/lists/receivingOrder/ROSupplierPicker';
import ROChoiceModal from '../components/lists/receivingOrder/ROChoiceModal';
import RODateModal from '../components/lists/receivingOrder/RODateModal';
import { ConfirmAndSave, SavingModal, MeasTotalsTable, LineConsQty } from '../components/lists/receivingOrder/ROSave';
import ReceivingOrderData from '../data/ReceivingOrderData';
import FreeReceivingSource from '../data/FreeReceivingSource';
import FreeReceivingItemPicker from '../components/lists/freeReceiving/FreeReceivingItemPicker';
import { FreeSave } from '../components/lists/freeReceiving/FreeReceivingSave';
import { useReceivingOrderDraft, UpdateDraft as UpdateDraftOf, ClearDraft as ClearDraftOf, SetLines as SetLinesOf, NextID } from '../variables/ReceivingOrderDraft';
import { PlayScanOk, PlayScanError } from '../variables/PhysicalCountSounds';
import { useScanWithoutEnter } from '../variables/ScanInput';
import { MODE_TITLES, STATUS_LABELS, FormatISODate, FormatQty, Plural, LineQty, LineIssue, LineBarcodes, isLotItem, VatLabel, FormatCost, LineCost, TodayISO } from '../variables/ReceivingOrderRules';
import MessageBox from '../components/MessageBox';

// The stock search shows the product groups or searches the items directly : the last side used is kept
const searchModeStorageKey = '@app_roSearchItemsMode';
let searchItemsMode = false;
AsyncStorage.getItem(searchModeStorageKey).then(value => { searchItemsMode = value === 'Y'; }).catch(() => { });

const STATUS_COLORS = { N: Colors.brandGreen, F: Colors.brandBlue, R: 'rgb(200, 60, 60)' };

// Nothing to tell : the bar is hidden (the placeholder of the scan already says what to do)
const READY_TEXT = '';

// The receiving order on one screen : the header (supplier, location, RO ref, dates, purchase order) folds away,
// under it the items : scan or type a reference, a barcode or a supplier reference (Enter), the stock search (Search)
// finds them by description too. Every item opens the line editor, which comes back here
// New : the location is the one of the service (LocCode of its Web.config), it cannot change. Modify : new lines, only the actual date changes
// Cancel and View : read only. route.params.view : the receiving order viewed (the one being prepared stays)
// route.params.free : the receiving order of Free Receiving (its own draft), the stock of the server online, the stock file downloaded offline,
// saved on the device and uploaded (at once when online)
const ReceivingOrderEditScreen = ({ navigation, route }) => {

    const view = !!route.params?.view;
    const free = !!route.params?.free;
    const draft = useReceivingOrderDraft(free ? 'FREE' : view);
    const data = free ? FreeReceivingSource : ReceivingOrderData;
    const UpdateDraft = (changes) => UpdateDraftOf(changes, free ? 'FREE' : 'RO');
    const SetLines = (change) => SetLinesOf(change, free ? 'FREE' : 'RO');
    const ClearDraft = (viewed = false) => ClearDraftOf(free ? 'FREE' : viewed);
    const homeScreen = free ? 'FreeReceiving' : 'ReceivingOrder';

    const [headerOpen, setHeaderOpen] = useState(() => !!draft && draft.mode === 'ADD' && draft.lines.length === 0);
    const [supplierPickerVisible, setSupplierPickerVisible] = useState(false);
    const [dateField, setDateField] = useState(null);
    const [purchaseOrders, setPurchaseOrders] = useState(null);
    const [saving, setSaving] = useState(false);
    const [refWarning, setRefWarning] = useState('');

    const [scanText, setScanText] = useState('');
    const scanWithoutEnter = useScanWithoutEnter(() => HandleScan());
    const [typing, setTyping] = useState(false);
    const [status, setStatus] = useState({ text: READY_TEXT, isError: false });
    const [searchChoiceVisible, setSearchChoiceVisible] = useState(false);
    const [itemChoices, setItemChoices] = useState(null);
    // The items of the pending stock file to choose from (null : closed)
    const [pendingItems, setPendingItems] = useState(null);
    const [searchText, setSearchText] = useState(null);
    const [browseVisible, setBrowseVisible] = useState(false);
    const [searching, setSearching] = useState(false);

    const scanRef = useRef();

    const isNew = draft?.mode === 'ADD';
    const isModify = draft?.mode === 'MODIFY';
    const isCancel = draft?.mode === 'CANCEL';
    const isView = draft?.mode === 'VIEW';
    // Cancel and view : the saved lines only, nothing is scanned
    const readOnly = isCancel || isView;

    const focusScan = () => { if (!readOnly) setTimeout(() => scanRef.current?.focus(), 150); };

    // The message shown when there is nothing to tell
    const readyText = isView ? (draft?.roStatus === 'R' ? 'Reverse receiving order (cancel), view only' : 'The lines of the receiving order (view only)')
        : isCancel ? 'The lines of the receiving order to cancel' : READY_TEXT;

    useEffect(() => {
        if (!draft) return;
        navigation.setOptions({
            title: free ? (draft.mode === 'VIEW' ? `Free Receiving - RO # ${draft.roRecID}` : draft.orderID ? 'Free Receiving - Saved On The Device' : 'Free Receiving')
                : MODE_TITLES[draft.mode] + (draft.roRecID ? ` #${draft.roRecID}` : ''),
        });
        // A new RO receives to the location of the service
        if (isNew && !draft.location) {
            data.GetLocations()
                .then(rows => {
                    if (rows.length > 0) UpdateDraft({ location: rows[0] });
                    else setStatus({ text: 'The location of the server is not a receiving location', isError: true });
                })
                .catch(err => setStatus({ text: err.message, isError: true }));
        }
    }, [draft?.mode]);

    // A message stays 3.5 seconds (pressed : shown as an alert), then the ready message comes back
    useEffect(() => {
        if (status.text === readyText) return;
        const timer = setTimeout(() => setStatus({ text: readyText, isError: false }), 3500);
        return () => clearTimeout(timer);
    }, [status, readyText]);

    // Free Receiving : an item picked in the stock file of View Items (route.params.pickItem), looked up once the supplier and location are there
    const [pendingPick, setPendingPick] = useState(null);
    const pickRef = useRef(null);
    useEffect(() => {
        const pick = route.params?.pickItem;
        if (!free || !pick || !draft) return;
        navigation.setParams({ pickItem: undefined });
        setPendingPick(pick);
        if (!draft.supplier) {
            setHeaderOpen(true);
            setSupplierPickerVisible(true);
            setStatus({ text: `Choose the supplier to receive ${pick.itemReference}`, isError: false });
        }
    }, [route.params?.pickItem]);
    useEffect(() => {
        if (!pendingPick || !draft?.supplier || !draft?.location) return;
        const pick = pendingPick;
        setPendingPick(null);
        pickRef.current?.(pick);
    }, [pendingPick, draft?.supplier, draft?.location]);

    // Back from the line editor : the ready message and the scan gets the focus (not when the screen opens : nothing has it)
    const opened = useRef(false);
    useEffect(() => navigation.addListener('focus', () => {
        setStatus({ text: readyText, isError: false });
        if (opened.current) focusScan();
        opened.current = true;
    }), [navigation, readyText]);

    // The draft is cleared (saved or cleared) : back to the receiving order menu
    if (!draft) {
        return (
            <View style={styles.emptyContainer}>
                <Text style={styles.emptyText}>{view ? 'No receiving order is opened' : 'No receiving order is being prepared'}</Text>
                <MainButton title='Back' imageSource={require('../assets/icons/close.png')} onPress={() => navigation.navigate(homeScreen)} />
            </View>
        );
    }

    const savedLines = draft.savedLines.map(line => ({ ...line, saved: true }));
    const lines = readOnly ? savedLines : [...draft.lines, ...savedLines];
    // the lines saved by this screen (modify : the new ones)
    const newLines = readOnly ? draft.savedLines : draft.lines;
    const missing = readOnly ? 0 : draft.lines.filter(line => LineIssue(line) !== '').length;

    const ShowError = (text) => {
        PlayScanError();
        Vibration.vibrate([0, 60, 60, 60]);
        setStatus({ text, isError: true });
        focusScan();
    };

    const ReadyStatus = () => setStatus({ text: readyText, isError: false });

    //#region Supplier, dates

    const PickSupplier = () => {
        if (draft.lines.length > 0) {
            alert('The supplier cannot change once the receiving order has items, remove them first');
            return;
        }
        setSupplierPickerVisible(true);
    };

    const CheckRef = () => {
        const reference = draft.roRef.trim();
        if (reference === '') { setRefWarning(''); return; }
        data.RefExists(reference)
            .then(exists => setRefWarning(exists ? 'Already used, change it' : ''))
            .catch(() => setRefWarning(''));
    };

    // RO date : not after today, the actual date follows it when it was before ; actual date : not before the RO date
    const PickDate = (date) => {
        const field = dateField;
        setDateField(null);
        if (field === 'roDate') {
            if (date > TodayISO()) { MessageBox.alert('RO Date', "The RO date should be equal or less than today's date"); return; }
            UpdateDraft(current => ({ roDate: date, roActualDate: current.roActualDate < date ? date : current.roActualDate }));
            return;
        }
        if (date < draft.roDate) {
            MessageBox.alert('Actual Date', `The actual date should be greater or equal than the RO date (${FormatISODate(draft.roDate)})`);
            return;
        }
        UpdateDraft({ roActualDate: date });
    };

    //#endregion

    //#region Purchase order

    const PickPurchaseOrder = () => {
        if (!draft.supplier) {
            alert('Choose the supplier first');
            return;
        }
        setSearching(true);
        setStatus({ text: 'Loading the purchase orders...', isError: false });
        data.GetPurchaseOrders(draft.supplier.supplierRecID, draft.roDate)
            .then(rows => {
                ReadyStatus();
                if (rows.length === 0) alert('There is no purchase order to receive for this supplier');
                else setPurchaseOrders(rows);
            })
            .catch(err => ShowError(err.message))
            .finally(() => setSearching(false));
    };

    // The lines of the PO replace the lines imported from the previous PO, the items scanned stay
    const ImportPurchaseOrder = (po) => {

        if (po.poAfterRO === 1) { alert('Cannot import a PO with a date after the RO date'); return; }
        if (po.poLocked === 1) { alert(`PO #${po.poRecID} is opened in the ERP, try again later`); return; }
        setPurchaseOrders(null);

        const run = () => {
            setSearching(true);
            setStatus({ text: 'Importing the purchase order...', isError: false });
            data.GetPurchaseOrderLines(draft.supplier.supplierRecID, po.poRecID, draft.roDate)
                .then(result => {
                    const blocked = result.lines.filter(line => line.blockMessage !== '');
                    const imported = result.lines.filter(line => line.blockMessage === '').map(line => ({
                        lineID: NextID(),
                        itemRecID: line.itemRecID,
                        itemReference: line.itemReference,
                        itemDescription: line.itemDescription,
                        itemClass: line.itemClass,
                        itemUnit: line.itemUnit,
                        itemBarcode: line.itemBarcode,
                        itemQtyControl: line.itemQtyControl,
                        itemVatType: line.itemVatType,
                        itemCostCC: line.itemCostCC,
                        itemCurrency: line.itemCurrency,
                        itemSubUnit: line.itemSubUnit,
                        itemMeasType: line.itemMeasType,
                        itemConsFactor: line.itemConsFactor,
                        unitCost: line.unitCost,
                        isPending: line.isPending === 1,
                        supplierRef: line.supplierRef,
                        lineStatus: line.lineStatus,
                        // an expiry item gets its qty from the lots typed
                        qty: isLotItem(line.itemClass) ? 0 : line.remainingQty,
                        lots: [],
                        barcodes: [],
                        poQty: line.remainingQty,
                    }));
                    UpdateDraft(current => ({
                        po: { poRecID: po.poRecID, poDate: po.poDate },
                        lines: [...current.lines.filter(line => line.poQty === undefined), ...imported],
                    }));
                    ReadyStatus();
                    setHeaderOpen(false);
                    const notes = [];
                    // What was imported, then why some lines of the PO are not in the receiving order
                    const lotLines = imported.filter(line => isLotItem(line.itemClass)).length;
                    if (lotLines > 0) notes.push(`• ${Plural(lotLines, 'expiry item')} : open ${lotLines === 1 ? 'it' : 'each one'} to type the expiry date and lot, the qty is 0 until then.`);
                    if (blocked.length > 0) notes.push(`• ${Plural(blocked.length, 'line')} not imported, the item cannot be received today :\n${blocked.map(line => `   ${line.itemReference} : ${line.blockMessage}`).join('\n')}`);
                    if (result.skipped > 0) notes.push(`• ${Plural(result.skipped, 'line')} not imported : a fashion or consignment item, an item without its unit, or an item of a brand / item tag you are not allowed to purchase (your purchase privileges).`);
                    MessageBox.alert(`PO #${po.poRecID}`, `${Plural(imported.length, 'line')} imported with the qty still to receive.${notes.length > 0 ? '\n\n' + notes.join('\n\n') : ''}`);
                })
                .catch(err => ShowError(err.message))
                .finally(() => setSearching(false));
        };

        const fromPO = draft.lines.filter(line => line.poQty !== undefined).length;
        if (fromPO > 0) {
            MessageBox.alert('Change The PO', `The ${Plural(fromPO, 'line')} of PO #${draft.po?.poRecID} will be replaced by the lines of PO #${po.poRecID}.`, [
                { text: 'Cancel', style: 'cancel' },
                { text: 'Replace', onPress: run },
            ]);
        }
        else run();
    };

    const RemovePurchaseOrder = () => {
        if (!draft.po) return;
        const fromPO = draft.lines.filter(line => line.poQty !== undefined).length;
        MessageBox.alert('Remove The PO', `PO #${draft.po.poRecID} and its ${Plural(fromPO, 'line')} will be removed from the receiving order.`, [
            { text: 'Cancel', style: 'cancel' },
            { text: 'Remove', style: 'destructive', onPress: () => UpdateDraft(current => ({ po: null, lines: current.lines.filter(line => line.poQty === undefined) })) },
        ]);
    };

    //#endregion

    //#region Scan

    // The items need the supplier (its references) and the location (its stock) : the header opens to choose them
    const CanScan = () => {
        if (draft.supplier && draft.location) return true;
        ShowError(!draft.supplier ? 'Choose the supplier first' : 'The receiving location is not loaded');
        setHeaderOpen(true);
        return false;
    };

    // A new line of the item, the expiry / lot of a scanned barcode are proposed
    const OpenItem = (item) => {
        if (item.blockMessage) {
            ShowError(`${item.itemReference} : ${item.blockMessage}`);
            return;
        }
        PlayScanOk();
        // An item of the purchase order : its line is opened (the qty to receive is already there)
        const poLine = draft.lines.find(line => line.poQty !== undefined && line.itemRecID.toString() === item.itemRecID.toString() && !!line.isPending === (item.isPending === 1));
        if (poLine) {
            setStatus({ text: `${item.itemReference} : the line of the purchase order`, isError: false });
            navigation.navigate('ReceivingOrderLine', { free,  lineID: poLine.lineID });
            return;
        }
        setStatus({ text: `${item.itemReference} : type the qty`, isError: false });
        navigation.navigate('ReceivingOrderLine', { free,  item, prefillLot: item.codeExpiry ? { expiry: item.codeExpiry, batch: item.codeBatch } : null });
    };

    const Lookup = (text, itemRecID = null) => {
        const code = text.trim();
        if (code === '' || !CanScan()) return;
        setHeaderOpen(false);
        setSearching(true);
        setStatus({ text: `Searching ${code}...`, isError: false });
        data.FindItem(draft.supplier.supplierRecID, code, draft.roDate)
            .then(items => {
                const found = itemRecID ? items.filter(item => item.itemRecID.toString() === itemRecID.toString()) : items;
                if (found.length === 1) OpenItem(found[0]);
                else if (found.length > 1) {
                    setStatus({ text: `${code} : ${Plural(found.length, 'item')}, choose one`, isError: false });
                    setItemChoices(found);
                }
                // Not a code : the stock search (any part of the reference, barcode or description)
                else if (code.length >= 3 && !itemRecID) SearchStock(code);
                else NotFound(code);
            })
            .catch(err => ShowError(err.message))
            .finally(() => setSearching(false));
    };

    // Nothing : the error sound and a message, one item : its qty, several : the list to choose
    const SearchStock = (code) => {
        setSearching(true);
        setStatus({ text: `Searching "${code}" in the stock...`, isError: false });
        data.SearchItems(draft.supplier.supplierRecID, code)
            .then(items => {
                if (items.length === 0) NotFound(code);
                else if (items.length === 1) Lookup(items[0].itemReference, items[0].itemRecID);
                else {
                    setStatus({ text: `${Plural(items.length, 'item')} found for "${code}", choose one`, isError: false });
                    setSearchText(code);
                }
            })
            .catch(err => ShowError(err.message))
            .finally(() => setSearching(false));
    };

    const NotFound = (code) => {
        ShowError(`No item found for ${code}`);
        MessageBox.alert('Item Not Found', `No item of the receiving order is found for "${code}".`, [{ text: 'OK', onPress: focusScan }]);
    };

    const HandleScan = () => {
        const text = scanText;
        setScanText('');
        Lookup(text);
    };

    pickRef.current = (pick) => Lookup(pick.itemReference, pick.itemRecID);

    // An item picked in the stock search : its details for the receiving order (supplier ref, privileges, dates)
    const PickFromSearch = (itemRecID, itemReference) => {
        setSearchText(null);
        setBrowseVisible(false);
        Lookup(itemReference, itemRecID);
    };

    // Search : the stock (the text typed, or by product group), or the items of the pending stock file
    const OpenSearch = () => {
        if (CanScan()) setSearchChoiceVisible(true);
    };

    // Free Receiving offline : the stock search of the stock file downloaded
    const localSearch = free && !FreeReceivingSource.isOnline();

    const StockSearch = () => {
        setSearchChoiceVisible(false);
        if (scanText.trim().length >= 3) { const text = scanText; setScanText(''); setSearchText(text.trim()); }
        else setBrowseVisible(true);
    };

    const OpenPendingItems = () => {
        setSearching(true);
        setStatus({ text: 'Loading the pending stock items...', isError: false });
        data.GetPendingItems()
            .then(items => {
                if (items.length === 0) {
                    ReadyStatus();
                    MessageBox.alert('Pending Stock Items', 'There is no pending stock item to receive.', [{ text: 'OK', onPress: focusScan }]);
                    return;
                }
                setStatus({ text: `${Plural(items.length, 'pending item')}, choose one`, isError: false });
                setPendingItems(items);
            })
            .catch(err => ShowError(err.message))
            .finally(() => setSearching(false));
    };

    //#endregion

    const RemoveLine = (line) => {
        MessageBox.alert('Remove Line', `Do you want to remove ${line.itemReference} (${STATUS_LABELS[line.lineStatus]} ${FormatQty(LineQty(line))}) ?`, [
            { text: 'No', style: 'cancel' },
            { text: 'Remove', style: 'destructive', onPress: () => SetLines(rows => rows.filter(row => row.lineID !== line.lineID)) },
        ]);
    };

    const Clear = () => {
        MessageBox.alert('Clear', free
            ? `The receiving order and its ${draft.lines.length} item(s) will be cleared.`
            : `The ${MODE_TITLES[draft.mode].toLowerCase()} and its ${Plural(draft.lines.length, isModify ? 'new line' : 'line')} will be cleared.`, [
            { text: 'Cancel', style: 'cancel' },
            { text: 'Clear', style: 'destructive', onPress: () => { ClearDraft(); navigation.navigate(homeScreen); } },
        ]);
    };

    const LineRow = ({ item: line }) => {
        const issue = line.saved ? '' : LineIssue(line);
        const barcodes = LineBarcodes(line).length;
        const lotText = isLotItem(line.itemClass) && line.lots.length > 0
            ? line.lots.map(lot => `${lot.expiry} / ${lot.batch} : ${FormatQty(lot.qty)}`).join('  ·  ') : '';
        return (
            <TouchableOpacity style={[styles.lineContainer, line.saved && !readOnly ? styles.lineSaved : null]}
                onPress={() => navigation.navigate('ReceivingOrderLine', { free,  lineID: line.lineID, view })}>
                <View style={[styles.statusStripe, { backgroundColor: STATUS_COLORS[line.lineStatus] ?? 'gray' }]} />
                <View style={styles.lineTextContainer}>
                    <Text style={styles.lineReferenceText} numberOfLines={1}>{line.itemReference}</Text>
                    <Text style={styles.lineDescriptionText} numberOfLines={1}>{line.itemDescription}</Text>
                    <Text style={styles.lineInfoText} numberOfLines={1}>
                        {STATUS_LABELS[line.lineStatus]}  ·  Ref {line.supplierRef}{barcodes > 0 ? `  ·  ${Plural(barcodes, 'barcode')}` : ''}{line.saved && !readOnly ? '  ·  Saved' : ''}
                    </Text>
                    <Text style={styles.lineInfoText} numberOfLines={1}>
                        VAT {VatLabel(line.itemVatType)}  ·  Cost {FormatCost(LineCost(line))}{line.itemCurrency ? ` ${line.itemCurrency}` : ''}
                        {line.isPending ? <Text style={styles.linePendingText}>  ·  Pending Item</Text> : <></>}
                    </Text>
                    {lotText !== '' ? <Text style={styles.lineLotText} numberOfLines={2}>{lotText}</Text> : <></>}
                    {line.poQty !== undefined ? <Text style={styles.linePOText} numberOfLines={1}>PO : {FormatQty(line.poQty)} to receive</Text> : <></>}
                    {issue !== '' ? <Text style={styles.lineIssueText} numberOfLines={1}>{issue}</Text> : <></>}
                </View>
                <View style={styles.lineQtyContainer}>
                    <Text style={[styles.lineQtyText, { color: LineQty(line) > 0 ? 'black' : 'red' }]}>{FormatQty(LineQty(line))}</Text>
                    <Text style={styles.lineUnitText} numberOfLines={1}>{line.itemUnit}</Text>
                    {/* the consumed qty (in the sub unit) when it is not the qty */}
                    {LineConsQty(line) !== LineQty(line) ?
                        <Text style={styles.lineConsText} numberOfLines={1}>Cons. {FormatQty(LineConsQty(line))}{line.itemSubUnit ? ` ${line.itemSubUnit}` : ''}</Text> :
                        <></>}
                </View>
                {!line.saved ?
                    <TouchableOpacity style={styles.lineDeleteButton} onPress={() => RemoveLine(line)}>
                        <Text style={styles.lineDeleteText}>✕</Text>
                    </TouchableOpacity> :
                    <></>}
            </TouchableOpacity>
        );
    };

    const SearchChoiceModal = () => (
        <Modal visible={searchChoiceVisible} transparent={true} presentationStyle="overFullScreen"
            onRequestClose={() => { setSearchChoiceVisible(false); focusScan(); }}>
            <View style={styles.modalContainer}>
                <View style={styles.modalCard}>
                    <ModalTitle title='Search' onCloseButtonPress={() => { setSearchChoiceVisible(false); focusScan(); }} />
                    <View style={styles.choiceContainer}>
                        <TouchableOpacity style={styles.choiceButton} onPress={StockSearch}>
                            <Image source={require('../assets/icons/stock.png')} style={styles.choiceImage} resizeMode='contain' />
                            <View style={styles.choiceTextContainer}>
                                <Text style={styles.choiceTitleText}>Stock Search</Text>
                                <Text style={styles.choiceDescriptionText}>
                                    {scanText.trim().length >= 3 ? `Search "${scanText.trim()}" in the stock` : 'By product group, or the items directly (switch)'}
                                </Text>
                            </View>
                        </TouchableOpacity>
                        <TouchableOpacity style={styles.choiceButton} onPress={() => { setSearchChoiceVisible(false); OpenPendingItems(); }}>
                            <Image source={require('../assets/icons/missingItems.png')} style={styles.choiceImage} resizeMode='contain' />
                            <View style={styles.choiceTextContainer}>
                                <Text style={styles.choiceTitleText}>Pending Stock Items</Text>
                                <Text style={styles.choiceDescriptionText}>Items not created yet, created when the RO is saved</Text>
                            </View>
                        </TouchableOpacity>
                    </View>
                </View>
            </View>
        </Modal>
    );

    // Folded : one line with the supplier, the RO ref and the location
    const headerSummary = [
        draft.supplier ? draft.supplier.supplierName : 'No Supplier',
        draft.roRef.trim() !== '' ? `Ref ${draft.roRef.trim()}` : '',
        draft.location ? draft.location.locationCode : '',
    ].filter(text => text !== '').join('  ·  ');

    return (
        <View style={styles.container}>

            <SavingModal visible={saving} />

            {SearchChoiceModal()}

            <ROSupplierPicker
                visible={supplierPickerVisible}
                selectedRecID={draft.supplier?.supplierRecID}
                data={data}
                onPick={supplier => {
                    setSupplierPickerVisible(false);
                    UpdateDraft(current => ({ supplier, po: current.supplier?.supplierRecID === supplier.supplierRecID ? current.po : null }));
                }}
                onClose={() => setSupplierPickerVisible(false)}
            />

            <RODateModal
                visible={dateField !== null}
                title={dateField === 'roDate' ? 'RO Date' : 'Actual Delivery Date'}
                value={dateField ? draft[dateField] : ''}
                onPick={PickDate}
                onClose={() => setDateField(null)}
            />

            <ROChoiceModal
                visible={purchaseOrders !== null}
                title='Purchase Orders'
                rows={purchaseOrders ?? []}
                keyOf={po => po.poRecID}
                textsOf={po => ({
                    title: `PO #${po.poRecID}${po.poManualNo ? '  ·  ' + po.poManualNo : ''}`,
                    detail: `${po.poDate}  ·  Delivery ${po.poDeliveryDate || '-'}  ·  ${FormatQty(po.poTotal)} ${po.poCurrency}`,
                    note: po.poRemark,
                    warning: po.poAfterRO === 1 ? 'The PO date is after the RO date' : po.poLocked === 1 ? 'Opened in the ERP' : '',
                    badge: draft.po?.poRecID?.toString() === po.poRecID.toString() ? 'Imported' : '',
                })}
                onPick={ImportPurchaseOrder}
                onClose={() => setPurchaseOrders(null)}
            />

            <ROChoiceModal
                visible={itemChoices !== null}
                title='Choose The Item'
                rows={itemChoices ?? []}
                keyOf={item => item.itemRecID}
                textsOf={item => ({
                    title: item.itemReference,
                    detail: item.itemDescription,
                    note: `${isLotItem(item.itemClass) ? 'Expiry item' : 'Trading item'}  ·  ${item.itemUnit}${item.itemBarcode ? '  ·  ' + item.itemBarcode : ''}`,
                    warning: item.blockMessage,
                })}
                onPick={item => { setItemChoices(null); OpenItem(item); }}
                onClose={() => { setItemChoices(null); ReadyStatus(); focusScan(); }}
            />

            <ROChoiceModal
                visible={pendingItems !== null}
                title='Pending Stock Items'
                rows={pendingItems ?? []}
                keyOf={item => item.itemRecID}
                textsOf={item => ({
                    title: item.itemReference,
                    detail: item.itemDescription,
                    note: `${isLotItem(item.itemClass) ? 'Expiry item' : 'Trading item'}  ·  ${item.itemUnit}${item.itemProductGroupDesc ? '  ·  ' + item.itemProductGroupDesc : ''}`,
                    badge: 'Pending',
                })}
                onPick={item => { setPendingItems(null); OpenItem(item); }}
                onClose={() => { setPendingItems(null); ReadyStatus(); focusScan(); }}
            />

            <FreeReceivingItemPicker
                visible={localSearch && (searchText !== null || browseVisible)}
                searchText={searchText ?? ''}
                title='Search (Device Stock)'
                onPick={item => PickFromSearch(item.itemRecID, item.itemReference)}
                onClose={() => { setSearchText(null); setBrowseVisible(false); ReadyStatus(); focusScan(); }}
            />

            {/* The stock search with the conditions of the receiving order : by text, or by product group */}
            <Modal visible={searchText !== null && !localSearch} onRequestClose={() => { setSearchText(null); ReadyStatus(); focusScan(); }}>
                <Items
                    title='Receiving Order'
                    itemProductGroupCode=''
                    itemPromotionCriteria=''
                    searchString={searchText ?? ''}
                    searchFor='RO'
                    supplierRecID={draft.supplier?.supplierRecID}
                    setStockScreenVisible={() => setSearchText(null)}
                    onSelectItem={PickFromSearch}
                    onCloseButtonPress={() => { setSearchText(null); ReadyStatus(); focusScan(); }}
                />
            </Modal>

            <Modal visible={browseVisible && !localSearch} onRequestClose={() => { setBrowseVisible(false); ReadyStatus(); focusScan(); }}>
                <ItemProductGroups
                    searchFor='RO'
                    itemsSwitch={true}
                    initialItemsMode={searchItemsMode}
                    onItemsModeChange={itemsMode => {
                        searchItemsMode = itemsMode;
                        AsyncStorage.setItem(searchModeStorageKey, itemsMode ? 'Y' : 'N').catch(() => { });
                    }}
                    supplierRecID={draft.supplier?.supplierRecID}
                    setStockScreenVisible={() => { setBrowseVisible(false); ReadyStatus(); }}
                    onSelectItem={PickFromSearch}
                />
            </Modal>

            {/* The header : tap to fold or unfold it */}
            <TouchableOpacity style={styles.headerToggle} onPress={() => { Keyboard.dismiss(); setHeaderOpen(open => !open); }}>
                <Text style={[styles.headerToggleText, !draft.supplier ? styles.missingValue : null]} numberOfLines={1}>
                    {headerOpen ? 'Header' : headerSummary}
                </Text>
                <Text style={styles.headerToggleArrow}>{headerOpen ? '▲' : '▼'}</Text>
            </TouchableOpacity>

            {headerOpen ?
                <ScrollView style={styles.header} keyboardShouldPersistTaps='handled'>

                    <ChoiceField
                        title='Supplier'
                        value={draft.supplier ? `${draft.supplier.supplierCode} - ${draft.supplier.supplierName}` : 'Choose The Supplier'}
                        valueStyle={!draft.supplier ? styles.missingValue : null}
                        onPress={isNew ? PickSupplier : undefined}
                        style={{ borderRightWidth: 0 }}
                    />

                    <ChoiceField
                        title='Receiving Location'
                        value={draft.location ? `${draft.location.locationCode} - ${draft.location.locationDescription}` : 'Loading...'}
                        style={{ borderRightWidth: 0 }}
                    />

                    <View style={styles.refField}>
                        <View style={styles.refTitleRow}>
                            <Text style={choiceTitleTextStyle} numberOfLines={1}>RO Ref. #</Text>
                            {refWarning !== '' ? <Text style={styles.refWarningText} numberOfLines={1}>{refWarning}</Text> : <></>}
                        </View>
                        <TextInput
                            style={styles.refText}
                            value={draft.roRef}
                            editable={isNew}
                            placeholder='The delivery note # of the supplier'
                            maxLength={15}
                            autoCorrect={false}
                            autoCapitalize='characters'
                            onChangeText={text => { setRefWarning(''); UpdateDraft({ roRef: text }); }}
                            onEndEditing={CheckRef}
                        />
                    </View>

                    <View style={choiceRowStyle.row}>
                        <View style={styles.fieldHalf}>
                            <ChoiceField
                                title='RO Date'
                                value={FormatISODate(draft.roDate)}
                                onPress={isNew ? () => setDateField('roDate') : undefined}
                                style={{ borderBottomWidth: 0 }}
                            />
                        </View>
                        <View style={styles.fieldHalf}>
                            <ChoiceField
                                title='Actual Date'
                                value={FormatISODate(draft.roActualDate)}
                                onPress={!readOnly ? () => setDateField('roActualDate') : undefined}
                                style={{ borderRightWidth: 0, borderBottomWidth: 0 }}
                            />
                        </View>
                    </View>

                    {!free ?
                    <View style={choiceRowStyle.row}>
                        <ChoiceField
                            title='Purchase Order (Optional)'
                            value={draft.po ? `PO #${draft.po.poRecID}${draft.po.poDate ? '  ·  ' + draft.po.poDate : ''}` : isNew ? 'Import A Purchase Order' : 'None'}
                            onPress={isNew ? PickPurchaseOrder : undefined}
                            style={{ borderRightWidth: 0, borderBottomWidth: 0 }}
                        />
                        {isNew && draft.po ?
                            <TouchableOpacity style={styles.removePOButton} onPress={RemovePurchaseOrder}>
                                <Text style={styles.removePOText}>✕</Text>
                            </TouchableOpacity> :
                            <></>}
                    </View> :
                    <></>}

                </ScrollView> :
                <></>}

            {!readOnly ?
                <View style={styles.entryContainer}>
                    <TouchableOpacity style={styles.searchButton} onPress={OpenSearch} disabled={searching}>
                        <Image source={require('../assets/icons/stock.png')} style={styles.searchImage} resizeMode='contain' />
                        <Text style={styles.searchButtonText}>Search</Text>
                    </TouchableOpacity>
                    <TextInput
                        ref={scanRef}
                        style={styles.entryText}
                        value={scanText}
                        placeholder='Scan Or Type Ref / Barcode'
                        showSoftInputOnFocus={typing}
                        onPressIn={() => { setHeaderOpen(false); if (!typing) Keyboard.dismiss(); }}
                        autoCapitalize='characters'
                        autoCorrect={false}
                        blurOnSubmit={false}
                        editable={!searching}
                        maxLength={100}
                        onChangeText={text => { scanWithoutEnter.watch(text, scanText); setScanText(text); }}
                        onSubmitEditing={() => { scanWithoutEnter.cancel(); HandleScan(); }}
                    />
                    <TouchableOpacity style={[styles.keyboardButton, typing ? styles.keyboardButtonOn : null]} onPress={() => {
                        const next = !typing;
                        setTyping(next);
                        scanRef.current?.blur();
                        if (next) focusScan(); else Keyboard.dismiss();
                    }}>
                        <Image source={require('../assets/icons/keyboard.png')} style={styles.keyboardImage} resizeMode='contain' />
                    </TouchableOpacity>
                </View> :
                <></>}

            {status.text !== '' ?
                <TouchableOpacity style={[styles.statusContainer, { backgroundColor: status.isError ? 'red' : isCancel ? 'rgb(200, 60, 60)' : Colors.brandBlue }]}
                    onPress={() => { if (status.text !== readyText) MessageBox.alert(status.isError ? 'Error' : 'Receiving Order', status.text); }}>
                    <Text style={styles.statusText} numberOfLines={1}>{status.text}</Text>
                </TouchableOpacity> :
                <></>}

            <FlatList
                style={styles.list}
                data={lines}
                keyExtractor={line => line.lineID.toString()}
                renderItem={LineRow}
                keyboardShouldPersistTaps='handled'
                ListEmptyComponent={<Text style={styles.emptyListText}>{readOnly ? 'No line' : 'Scan the items received, or press Search to find them by description'}</Text>}
            />

            {/* Always at the bottom */}
            <View style={styles.bottomContainer}>
                {/* Lines | the totals box of FrmDO : consumed qty by measure, normal / free / return */}
                <View style={styles.summaryRow}>
                    <View style={styles.summaryLinesCell}>
                        <Text style={styles.summaryText} numberOfLines={1}>{Plural(newLines.length, isModify ? 'New Line' : 'Line')}</Text>
                        {missing > 0 ? <Text style={[styles.summaryText, styles.summaryMissingText]} numberOfLines={1}>{missing} To Complete</Text> : <></>}
                    </View>
                    <MeasTotalsTable lines={newLines} noReturn={free} />
                </View>
                <View style={styles.actionsContainer}>
                    {isView ?
                        <MainButton title='Close' imageSource={require('../assets/icons/close.png')}
                            onPress={() => { ClearDraft(true); navigation.navigate(homeScreen); }} /> :
                        <>
                            <MainButton title={isCancel ? 'Cancel RO' : 'Save'} imageSource={isCancel ? require('../assets/icons/ReturnOrders.png') : require('../assets/icons/upload.png')}
                                onPress={() => free
                                    ? FreeSave(draft, setSaving, () => navigation.navigate(homeScreen))
                                    : ConfirmAndSave(draft, setSaving, () => navigation.navigate(homeScreen))} />
                            <MainButton title='Clear' imageSource={require('../assets/icons/deleteLine.png')} onPress={Clear} />
                        </>}
                </View>
            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: Colors.bgColor,
    },
    emptyContainer: {
        flex: 1,
        alignItems: 'center',
        justifyContent: 'center',
        padding: 20,
        backgroundColor: Colors.bgColor,
    },
    emptyText: {
        color: 'gray',
        fontSize: FontSize(16),
        marginBottom: 15,
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
    missingValue: {
        color: 'red',
    },
    fieldHalf: {
        width: '50%',
    },
    refField: {
        height: 50,
        borderColor: 'silver',
        borderBottomWidth: 1,
        paddingLeft: 10,
        paddingRight: 8,
        justifyContent: 'center',
    },
    refTitleRow: {
        flexDirection: 'row',
        justifyContent: 'space-between',
    },
    refWarningText: {
        flexShrink: 1,
        color: 'rgb(220, 120, 0)',
        fontSize: FontSize(13),
        fontWeight: 'bold',
        marginLeft: 6,
    },
    refText: {
        height: 26,
        color: 'black',
        fontSize: FontSize(17),
        paddingVertical: 0,
        paddingLeft: 0,
    },
    removePOButton: {
        width: 50,
        alignItems: 'center',
        justifyContent: 'center',
        borderLeftWidth: 1,
        borderLeftColor: 'silver',
    },
    removePOText: {
        color: 'red',
        fontSize: FontSize(20),
        fontWeight: 'bold',
    },
    entryContainer: {
        backgroundColor: Colors.brandGreen,
        width: '100%',
        height: 52,
        padding: 5,
        flexDirection: 'row',
        alignItems: 'center',
    },
    searchButton: {
        height: '100%',
        flexDirection: 'row',
        alignItems: 'center',
        paddingHorizontal: 8,
        marginRight: 5,
        borderRadius: 15,
        borderWidth: 1,
        borderColor: Colors.buttonBorderColor,
        backgroundColor: 'white',
    },
    searchImage: {
        width: 26,
        height: 26,
        marginRight: 4,
    },
    searchButtonText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
        fontWeight: 'bold',
    },
    entryText: {
        flex: 1,
        height: '100%',
        backgroundColor: 'white',
        color: 'black',
        fontSize: FontSize(17),
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
    choiceContainer: {
        padding: 10,
    },
    choiceButton: {
        flexDirection: 'row',
        alignItems: 'center',
        minHeight: 74,
        marginBottom: 10,
        paddingHorizontal: 10,
        borderRadius: 20,
        borderWidth: 2,
        borderColor: Colors.buttonBorderColor,
        backgroundColor: Colors.buttonBackColor1,
    },
    choiceImage: {
        width: 42,
        height: 42,
        marginRight: 10,
    },
    choiceTextContainer: {
        flex: 1,
    },
    choiceTitleText: {
        color: Colors.buttonForeColor1,
        fontSize: FontSize(19),
    },
    choiceDescriptionText: {
        color: Colors.buttonForeColor2,
        fontSize: FontSize(13),
        fontStyle: 'italic',
        fontWeight: 'bold',
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
        fontSize: FontSize(15),
        fontWeight: 'bold',
    },
    list: {
        flex: 1,
    },
    emptyListText: {
        textAlign: 'center',
        color: 'gray',
        fontSize: FontSize(16),
        padding: 20,
    },
    lineSaved: {
        opacity: 0.6,
    },
    lineContainer: {
        flexDirection: 'row',
        alignItems: 'center',
        minHeight: 64,
        backgroundColor: Colors.listItemBackColor,
        marginBottom: 2,
        paddingRight: 5,
    },
    statusStripe: {
        width: 6,
        alignSelf: 'stretch',
        marginRight: 8,
    },
    lineTextContainer: {
        flex: 1,
        paddingVertical: 4,
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
    lineInfoText: {
        color: 'gray',
        fontSize: FontSize(13),
    },
    lineLotText: {
        color: 'gray',
        fontSize: FontSize(13),
        fontStyle: 'italic',
    },
    linePOText: {
        color: Colors.brandGreen,
        fontSize: FontSize(13),
        fontWeight: 'bold',
    },
    linePendingText: {
        color: 'rgb(220, 120, 0)',
        fontWeight: 'bold',
    },
    lineIssueText: {
        color: 'red',
        fontSize: FontSize(13),
        fontWeight: 'bold',
    },
    lineQtyContainer: {
        alignItems: 'flex-end',
        minWidth: 70,
        marginLeft: 6,
    },
    lineQtyText: {
        fontSize: FontSize(19),
        fontWeight: 'bold',
    },
    lineUnitText: {
        color: 'gray',
        fontSize: FontSize(12),
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
    bottomContainer: {
        borderTopWidth: 1,
        borderTopColor: 'silver',
        backgroundColor: Colors.bgColor,
        paddingHorizontal: 10,
        paddingTop: 6,
    },
    summaryRow: {
        flexDirection: 'row',
        alignItems: 'center',
        marginBottom: 6,
    },
    summaryLinesCell: {
        width: '28%',
        paddingRight: 4,
        borderRightWidth: 1,
        borderRightColor: 'silver',
    },
    lineConsText: {
        color: Colors.brandGreen,
        fontSize: FontSize(12),
        fontWeight: 'bold',
    },
    summaryText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
        fontWeight: 'bold',
        textAlign: 'center',
        marginBottom: 6,
    },
    summaryMissingText: {
        color: 'red',
    },
    actionsContainer: {
        flexDirection: 'row',
        justifyContent: Sizing.isTablet ? 'flex-end' : 'space-between',
    },
});

export default ReceivingOrderEditScreen;
