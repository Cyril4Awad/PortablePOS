import React, { useState, useRef, useEffect } from 'react';
import { View, Text, TextInput, StyleSheet, TouchableOpacity, FlatList, Image, Modal, Keyboard, Vibration, Alert } from 'react-native';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import MainButton from '../components/controls/MainButton';
import ItemProductGroups from '../components/lists/stock/ItemProductGroups';
import Items from '../components/lists/stock/Items';
import ROChoiceModal from '../components/lists/receivingOrder/ROChoiceModal';
import ModalTitle from '../components/ModalTitle';
import { ConfirmAndSave, SavingModal, DraftTotalQty } from '../components/lists/receivingOrder/ROSave';
import ReceivingOrderData from '../data/ReceivingOrderData';
import { useReceivingOrderDraft, SetLines } from '../variables/ReceivingOrderDraft';
import { PlayScanOk, PlayScanError } from '../variables/PhysicalCountSounds';
import { STATUS_LABELS, LineQty, LineIssue, LineBarcodes, FormatQty, Plural, isLotItem, VatLabel, FormatCost, LineCost } from '../variables/ReceivingOrderRules';

const STATUS_COLORS = { N: Colors.brandGreen, F: Colors.brandBlue, R: 'rgb(200, 60, 60)' };

// The items of the receiving order : scan or type a reference, a barcode or a supplier reference (Enter),
// the stock search (Search) finds them by description too. Every item opens the line editor
const READY_TEXT = 'Scan or type a reference / barcode';

const ReceivingOrderItemsScreen = ({ navigation }) => {

    const draft = useReceivingOrderDraft();

    const [scanText, setScanText] = useState('');
    const [typing, setTyping] = useState(false);
    const [status, setStatus] = useState({ text: READY_TEXT, isError: false });
    const [searchChoiceVisible, setSearchChoiceVisible] = useState(false);
    const [itemChoices, setItemChoices] = useState(null);
    // The items of the pending stock file to choose from (null : closed)
    const [pendingItems, setPendingItems] = useState(null);
    const [searchText, setSearchText] = useState(null);
    const [browseVisible, setBrowseVisible] = useState(false);
    const [saving, setSaving] = useState(false);
    const [searching, setSearching] = useState(false);

    const scanRef = useRef();

    const isCancel = draft?.mode === 'CANCEL';
    // View and cancel : the saved lines only, nothing is scanned
    const readOnly = isCancel || draft?.mode === 'VIEW';

    const focusScan = () => setTimeout(() => scanRef.current?.focus(), 150);

    // The message shown when there is nothing to tell
    const readyText = isCancel ? 'The lines of the receiving order to cancel' : readOnly ? 'The lines of the receiving order (view only)' : READY_TEXT;

    // A message stays 3.5 seconds (pressed : shown as an alert), then the ready message comes back
    useEffect(() => {
        if (status.text === readyText) return;
        const timer = setTimeout(() => setStatus({ text: readyText, isError: false }), 3500);
        return () => clearTimeout(timer);
    }, [status, readyText]);

    // Back from the line editor : the ready message and the scan gets the focus
    useEffect(() => navigation.addListener('focus', () => {
        setStatus({ text: readyText, isError: false });
        focusScan();
    }), [navigation, readyText]);

    if (!draft) {
        return (
            <View style={styles.emptyContainer}>
                <Text style={styles.emptyText}>No receiving order is being prepared</Text>
                <MainButton title='Back' imageSource={require('../assets/icons/close.png')} onPress={() => navigation.navigate('ReceivingOrder')} />
            </View>
        );
    }

    const ShowError = (text) => {
        PlayScanError();
        Vibration.vibrate([0, 60, 60, 60]);
        setStatus({ text, isError: true });
        focusScan();
    };

    //#region Scan

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
            navigation.navigate('ReceivingOrderLine', { lineID: poLine.lineID });
            return;
        }
        setStatus({ text: `${item.itemReference} : type the qty`, isError: false });
        navigation.navigate('ReceivingOrderLine', { item, prefillLot: item.codeExpiry ? { expiry: item.codeExpiry, batch: item.codeBatch } : null });
    };

    const Lookup = (text, itemRecID = null) => {
        const code = text.trim();
        if (code === '') return;
        setSearching(true);
        setStatus({ text: `Searching ${code}...`, isError: false });
        ReceivingOrderData.FindItem(draft.supplier.supplierRecID, code, draft.roDate)
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
        ReceivingOrderData.SearchItems(draft.supplier.supplierRecID, code)
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
        Alert.alert('Item Not Found', `No item of the receiving order is found for "${code}".`, [{ text: 'OK', onPress: focusScan }]);
    };

    const ReadyStatus = () => setStatus({ text: readyText, isError: false });

    const HandleScan = () => {
        const text = scanText;
        setScanText('');
        Lookup(text);
    };

    // An item picked in the stock search : its details for the receiving order (supplier ref, privileges, dates)
    const PickFromSearch = (itemRecID, itemReference) => {
        setSearchText(null);
        setBrowseVisible(false);
        Lookup(itemReference, itemRecID);
    };

    //#endregion

    const RemoveLine = (line) => {
        Alert.alert('Remove Line', `Do you want to remove ${line.itemReference} (${STATUS_LABELS[line.lineStatus]} ${FormatQty(LineQty(line))}) ?`, [
            { text: 'No', style: 'cancel' },
            { text: 'Remove', style: 'destructive', onPress: () => SetLines(lines => lines.filter(row => row.lineID !== line.lineID)) },
        ]);
    };

    const LineRow = ({ item: line }) => {
        const issue = line.saved ? '' : LineIssue(line);
        const barcodes = LineBarcodes(line).length;
        const lotText = isLotItem(line.itemClass) && line.lots.length > 0
            ? line.lots.map(lot => `${lot.expiry} / ${lot.batch} : ${FormatQty(lot.qty)}`).join('  ·  ') : '';
        return (
            <TouchableOpacity style={[styles.lineContainer, line.saved ? styles.lineSaved : null]}
                onPress={() => navigation.navigate('ReceivingOrderLine', { lineID: line.lineID })}>
                <View style={[styles.statusStripe, { backgroundColor: STATUS_COLORS[line.lineStatus] ?? 'gray' }]} />
                <View style={styles.lineTextContainer}>
                    <Text style={styles.lineReferenceText} numberOfLines={1}>{line.itemReference}</Text>
                    <Text style={styles.lineDescriptionText} numberOfLines={1}>{line.itemDescription}</Text>
                    <Text style={styles.lineInfoText} numberOfLines={1}>
                        {STATUS_LABELS[line.lineStatus]}  ·  Ref {line.supplierRef}{barcodes > 0 ? `  ·  ${Plural(barcodes, 'barcode')}` : ''}{line.saved ? '  ·  Saved' : ''}
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
                </View>
                {!line.saved ?
                    <TouchableOpacity style={styles.lineDeleteButton} onPress={() => RemoveLine(line)}>
                        <Text style={styles.lineDeleteText}>✕</Text>
                    </TouchableOpacity> :
                    <></>}
            </TouchableOpacity>
        );
    };

    // The supplier and the location are needed to search the items (its references) and check the stock
    if (!readOnly && (!draft.supplier || !draft.location)) {
        return (
            <View style={styles.emptyContainer}>
                <Text style={styles.emptyText}>Choose the {!draft.supplier ? 'supplier' : 'receiving location'} first</Text>
                <MainButton title='Header' imageSource={require('../assets/icons/orders.png')} onPress={() => navigation.navigate('ReceivingOrderEdit')} />
            </View>
        );
    }

    const lines = readOnly
        ? draft.savedLines.map(line => ({ ...line, saved: true }))
        : [...draft.lines, ...draft.savedLines.map(line => ({ ...line, saved: true }))];
    const newLines = readOnly ? draft.savedLines : draft.lines;

    // Search : the stock (the text typed, or by product group), or the items of the pending stock file
    const StockSearch = () => {
        setSearchChoiceVisible(false);
        if (scanText.trim().length >= 3) { const text = scanText; setScanText(''); setSearchText(text.trim()); }
        else setBrowseVisible(true);
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
                                    {scanText.trim().length >= 3 ? `Search "${scanText.trim()}" in the stock` : 'The items by product group or description'}
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

    const OpenPendingItems = () => {
        setSearching(true);
        setStatus({ text: 'Loading the pending stock items...', isError: false });
        ReceivingOrderData.GetPendingItems()
            .then(items => {
                if (items.length === 0) {
                    ReadyStatus();
                    Alert.alert('Pending Stock Items', 'There is no pending stock item to receive.', [{ text: 'OK', onPress: focusScan }]);
                    return;
                }
                setStatus({ text: `${Plural(items.length, 'pending item')}, choose one`, isError: false });
                setPendingItems(items);
            })
            .catch(err => ShowError(err.message))
            .finally(() => setSearching(false));
    };

    return (
        <View style={styles.container}>

            <SavingModal visible={saving} />

            {SearchChoiceModal()}

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

            {/* The stock search with the conditions of the receiving order : by text, or by product group */}
            <Modal visible={searchText !== null} onRequestClose={() => { setSearchText(null); ReadyStatus(); focusScan(); }}>
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

            <Modal visible={browseVisible} onRequestClose={() => { setBrowseVisible(false); ReadyStatus(); focusScan(); }}>
                <ItemProductGroups
                    searchFor='RO'
                    supplierRecID={draft.supplier?.supplierRecID}
                    setStockScreenVisible={() => { setBrowseVisible(false); ReadyStatus(); }}
                    onSelectItem={PickFromSearch}
                />
            </Modal>

            {!readOnly ?
                <View style={styles.entryContainer}>
                    <TouchableOpacity style={styles.searchButton} onPress={() => setSearchChoiceVisible(true)} disabled={searching}>
                        <Image source={require('../assets/icons/stock.png')} style={styles.searchImage} resizeMode='contain' />
                        <Text style={styles.searchButtonText}>Search</Text>
                    </TouchableOpacity>
                    <TextInput
                        ref={scanRef}
                        style={styles.entryText}
                        value={scanText}
                        placeholder='Scan Or Type Ref / Barcode'
                        showSoftInputOnFocus={typing}
                        onPressIn={() => { if (!typing) Keyboard.dismiss(); }}
                        autoFocus={true}
                        autoCapitalize='characters'
                        autoCorrect={false}
                        blurOnSubmit={false}
                        editable={!searching}
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
                </View> :
                <></>}

            <TouchableOpacity style={[styles.statusContainer, { backgroundColor: status.isError ? 'red' : isCancel ? 'rgb(200, 60, 60)' : Colors.brandBlue }]}
                onPress={() => { if (status.text !== readyText) Alert.alert(status.isError ? 'Error' : 'Receiving Order', status.text); }}>
                <Text style={styles.statusText} numberOfLines={1}>{status.text}</Text>
            </TouchableOpacity>

            <FlatList
                style={styles.list}
                data={lines}
                keyExtractor={line => line.lineID.toString()}
                renderItem={LineRow}
                keyboardShouldPersistTaps='handled'
                ListEmptyComponent={<Text style={styles.emptyListText}>Scan the items received, or press Search to find them by description</Text>}
            />

            <View style={styles.bottomContainer}>
                <Text style={styles.summaryText} numberOfLines={1}>
                    {Plural(newLines.length, readOnly ? 'Line' : draft.mode === 'MODIFY' ? 'New Line' : 'Line')}  ·  Total Qty {FormatQty(DraftTotalQty(newLines))}
                </Text>
                <View style={styles.actionsContainer}>
                    <MainButton title='Header' imageSource={require('../assets/icons/orders.png')} onPress={() => navigation.navigate('ReceivingOrderEdit')} />
                    {draft.mode !== 'VIEW' ?
                        <MainButton title={isCancel ? 'Cancel RO' : 'Save'} imageSource={isCancel ? require('../assets/icons/ReturnOrders.png') : require('../assets/icons/upload.png')}
                            onPress={() => ConfirmAndSave(draft, setSaving, () => navigation.navigate('ReceivingOrder'))} /> :
                        <></>}
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
    lineContainer: {
        flexDirection: 'row',
        alignItems: 'center',
        minHeight: 64,
        backgroundColor: Colors.listItemBackColor,
        marginBottom: 2,
        paddingRight: 5,
    },
    lineSaved: {
        opacity: 0.6,
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
    summaryText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
        fontWeight: 'bold',
        textAlign: 'center',
        marginBottom: 6,
    },
    actionsContainer: {
        flexDirection: 'row',
        justifyContent: Sizing.isTablet ? 'flex-end' : 'space-between',
    },
});

export default ReceivingOrderItemsScreen;
