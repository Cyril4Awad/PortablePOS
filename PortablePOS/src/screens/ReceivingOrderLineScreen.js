import React, { useState, useEffect, useRef } from 'react';
import { View, Text, TextInput, StyleSheet, ScrollView, TouchableOpacity, Keyboard, Alert, Modal, Image } from 'react-native';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import MainButton from '../components/controls/MainButton';
import ReceivingOrderData from '../data/ReceivingOrderData';
import ROChoiceModal from '../components/lists/receivingOrder/ROChoiceModal';
import { useReceivingOrderDraft, GetDraft, SetLines, NextID } from '../variables/ReceivingOrderDraft';
import {
    STATUS_LABELS, STATUS_ORDER, isLotItem, ParseQty, FormatQty, LineQty, LineBarcodes, Plural,
    MaskLotDate, CheckExpiry, CheckProduction, BarcodeCheckDigitOk, isReadOnlyMode, VatLabel, FormatCost, LineCost,
} from '../variables/ReceivingOrderRules';

const STATUS_COLORS = { N: Colors.brandGreen, F: Colors.brandBlue, R: 'rgb(200, 60, 60)' };

const EMPTY_LOT = { expiry: '', batch: '', production: '', qty: '', barcodes: [] };

// One line of the receiving order (FrmDO rules) : the status (normal, free, return), the supplier ref,
// the qty (trading item) or the expiry / lots (expiry item), the barcodes
// route.params : { item, prefillLot } for a new line, { lineID } for a line of the list (a saved line is read only), view : the RO viewed
const ReceivingOrderLineScreen = ({ route, navigation }) => {

    // view : a line of the receiving order viewed
    const view = !!route.params?.view;
    const draft = useReceivingOrderDraft(view);

    // The line being edited, or a new one for the item
    const existing = route.params?.lineID
        ? [...(GetDraft(view)?.lines ?? []), ...(GetDraft(view)?.savedLines ?? [])].find(line => line.lineID === route.params.lineID)
        : null;
    const readOnly = !!existing && (GetDraft(view)?.savedLines ?? []).some(line => line.lineID === existing.lineID) || isReadOnlyMode(GetDraft(view)?.mode);
    const item = existing ?? route.params?.item ?? {};
    const lotItem = isLotItem(item.itemClass);
    // An item of the pending stock file : created in the stock file when the RO is saved (no stock, no return, cost 0)
    const isPending = existing ? existing.isPending === true : item.isPending === 1;

    const [lineStatus, setLineStatus] = useState(existing?.lineStatus ?? 'N');
    const [supplierRef, setSupplierRef] = useState(existing?.supplierRef ?? item.supplierRef ?? '');
    const [qtyText, setQtyText] = useState(existing && !lotItem ? FormatQty(existing.qty) : lotItem ? '' : '1');
    const [lots, setLots] = useState(existing?.lots ?? []);
    const [barcodes, setBarcodes] = useState(existing?.barcodes ?? []);
    // The lot being typed (expiry item)
    const [lotForm, setLotForm] = useState(null);
    const [barcodeText, setBarcodeText] = useState('');
    // null until the stock of the location is loaded
    const [stock, setStock] = useState(null);
    const [message, setMessage] = useState({ text: '', isError: false });
    const [busy, setBusy] = useState(false);
    // The lots of the item to choose from in the lot window
    const [lotLookupVisible, setLotLookupVisible] = useState(false);

    const barcodeRef = useRef();
    const qtyRef = useRef();

    useEffect(() => {
        navigation.setOptions({ title: readOnly ? 'Receiving Order - Line' : existing ? 'Receiving Order - Change Line' : 'Receiving Order - New Line' });
        if (!item.itemRecID || !draft?.location) return;
        if (isPending) setStock({ lots: [], knownLots: [] });
        else LoadStock().catch(err => setMessage({ text: `Stock not loaded : ${err.message}`, isError: true }));
        // A barcode scanned with its expiry / lot : the lot is ready to be completed
        if (!existing && lotItem) {
            const prefill = route.params?.prefillLot;
            setLotForm({ ...EMPTY_LOT, lotID: null, expiry: prefill?.expiry ?? '', batch: prefill?.batch ?? '' });
        }
    }, []);

    if (!draft || !item.itemRecID) {
        return (
            <View style={styles.emptyContainer}>
                <Text style={styles.emptyText}>The line is not available any more</Text>
                <MainButton title='Back' imageSource={require('../assets/icons/close.png')} onPress={() => navigation.goBack()} />
            </View>
        );
    }

    // An error is an alert (seen over the lot window), an information is shown under the item
    const ShowMessage = (text, isError = false) => {
        if (isError) Alert.alert(item.itemReference, text);
        else setMessage({ text, isError });
    };

    //#region Stock checks

    // The stock is loaded again when it was not there yet (a return is checked with it)
    const LoadStock = async () => {
        const loaded = await ReceivingOrderData.GetItemLots(item.itemRecID, draft.location.locationCode);
        setStock(loaded);
        return loaded;
    };

    // Available qty of the location for a return (STK_QTYCTRL items, QtyCntrEc of FrmDO) : null when the item has none there
    const AvailableOf = (expiry, batch, stockData = stock) => {
        if (!stockData) return null;
        const rows = stockData.lots.filter(lot => lot.lotExpiry === expiry && lot.lotBatch === batch);
        if (rows.length === 0) return null;
        return rows.reduce((total, lot) => total + Number(lot.lotAvailable), 0);
    };

    // The qty already returned by the other new lines of the same item and lot
    const ReturnedByOthers = (expiry, batch) => (GetDraft(view)?.lines ?? [])
        .filter(line => line.lineStatus === 'R' && line.itemRecID.toString() === item.itemRecID.toString() && line.lineID !== existing?.lineID)
        .reduce((total, line) => total + (lotItem
            ? line.lots.filter(lot => lot.expiry === expiry && lot.batch === batch).reduce((sum, lot) => sum + Number(lot.qty), 0)
            : Number(line.qty)), 0);

    // '' when the return can take this qty (stockData : the stock of the location, loaded)
    const ReturnIssue = (expiry, batch, qty, stockData) => {
        if (lineStatus !== 'R' || item.itemQtyControl !== 'Y') return '';
        const available = AvailableOf(expiry, batch, stockData);
        const lotText = expiry ? ` (${expiry} / ${batch})` : '';
        if (available === null) return `${item.itemReference}${lotText} does not have quantity in ${draft.location.locationCode}`;
        const total = qty + ReturnedByOthers(expiry, batch);
        if (total > available) return `${item.itemReference}${lotText} has ${FormatQty(available)} available quantity in ${draft.location.locationCode}`;
        return '';
    };

    //#endregion

    //#region Barcodes

    // Every barcode already used in the receiving order, except the ones of this line
    const OtherBarcodes = () => [...(GetDraft(view)?.lines ?? []), ...(GetDraft(view)?.savedLines ?? [])]
        .filter(line => line.lineID !== existing?.lineID)
        .reduce((all, line) => all.concat(LineBarcodes(line)), []);

    // The barcodes of the trading line, or of the lot being typed
    const currentBarcodes = lotItem ? (lotForm?.barcodes ?? []) : barcodes;
    const setCurrentBarcodes = (list) => lotItem ? setLotForm(form => ({ ...form, barcodes: list })) : setBarcodes(list);
    const barcodeLimit = () => {
        const parsed = ParseQty(lotItem ? lotForm?.qty : qtyText, true);
        return parsed.ok ? parsed.value : 0;
    };

    const AddBarcode = (text) => {
        const barcode = (text ?? '').replace(/[\r\n]/g, '').trim();
        setBarcodeText('');
        if (barcode === '') return;
        if (barcode.length > 20) { ShowMessage('The barcode should be less than 20 characters', true); return; }
        const lineBarcodes = lotItem ? [...lots.filter(lot => lot.lotID !== lotForm?.lotID).reduce((all, lot) => all.concat(lot.barcodes), []), ...currentBarcodes] : currentBarcodes;
        if (lineBarcodes.includes(barcode) || OtherBarcodes().includes(barcode)) { ShowMessage(`${barcode} is already in the receiving order`, true); return; }
        if (currentBarcodes.length >= barcodeLimit()) { ShowMessage(`The barcodes cannot be more than the qty (${FormatQty(barcodeLimit())})`, true); return; }
        setBusy(true);
        ReceivingOrderData.CheckBarcode(item.itemRecID, barcode)
            .then(result => {
                setCurrentBarcodes([...currentBarcodes, barcode]);
                const checkDigit = BarcodeCheckDigitOk(barcode) ? '' : '  (the check digit looks wrong)';
                ShowMessage(`${barcode} added${result === 'New' ? ', it will be added to the item' : ''}${checkDigit}`, checkDigit !== '');
            })
            .catch(err => ShowMessage(err.message, true))
            .finally(() => { setBusy(false); setTimeout(() => barcodeRef.current?.focus(), 100); });
    };

    const GenerateBarcode = () => {
        if (currentBarcodes.length >= barcodeLimit()) { ShowMessage(`Type the qty first, the barcodes cannot be more than the qty (${FormatQty(barcodeLimit())})`, true); return; }
        setBusy(true);
        ReceivingOrderData.GenerateBarcode()
            .then(barcode => {
                setCurrentBarcodes([...currentBarcodes, barcode]);
                ShowMessage(`${barcode} generated`);
            })
            .catch(err => ShowMessage(err.message, true))
            .finally(() => setBusy(false));
    };

    const BarcodesSection = (title) => (
        <View style={styles.section}>
            <View style={styles.sectionTitleRow}>
                <Text style={styles.sectionTitleText}>{title}</Text>
                <Text style={styles.sectionCountText}>{currentBarcodes.length} / {FormatQty(barcodeLimit())}</Text>
            </View>
            {!readOnly ?
                <View style={styles.barcodeEntryRow}>
                    <TextInput
                        ref={barcodeRef}
                        style={styles.barcodeText}
                        value={barcodeText}
                        placeholder='Scan Or Type A Barcode'
                        autoCorrect={false}
                        blurOnSubmit={false}
                        maxLength={40}
                        editable={!busy}
                        onChangeText={setBarcodeText}
                        onSubmitEditing={() => AddBarcode(barcodeText)}
                    />
                    <TouchableOpacity style={styles.smallButton} onPress={GenerateBarcode} disabled={busy}>
                        <Text style={styles.smallButtonText}>Generate</Text>
                    </TouchableOpacity>
                </View> :
                <></>}
            {currentBarcodes.length === 0 ?
                <Text style={styles.hintText}>No barcode (optional)</Text> :
                <View style={styles.chipsContainer}>
                    {currentBarcodes.map(barcode => (
                        <View key={barcode} style={styles.chip}>
                            <Text style={styles.chipText}>{barcode}</Text>
                            {!readOnly ?
                                <TouchableOpacity onPress={() => setCurrentBarcodes(currentBarcodes.filter(code => code !== barcode))}>
                                    <Text style={styles.chipRemoveText}>  ✕</Text>
                                </TouchableOpacity> :
                                <></>}
                        </View>
                    ))}
                </View>}
        </View>
    );

    //#endregion

    //#region Lots (expiry item)

    const lotsTotal = lots.reduce((total, lot) => total + Number(lot.qty || 0), 0);

    // The production date of a lot already known (location 00)
    const KnownProduction = (expiry, batch) => (stock?.knownLots ?? []).find(lot => lot.lotExpiry === expiry && lot.lotBatch === batch)?.lotProduction ?? '';

    const UpdateLotForm = (changes) => setLotForm(form => {
        const next = { ...form, ...changes };
        if ((changes.expiry !== undefined || changes.batch !== undefined) && next.production === '') {
            next.production = KnownProduction(next.expiry, next.batch.trim());
        }
        return next;
    });

    const SaveLot = async () => {
        const expiry = lotForm.expiry;
        const batch = lotForm.batch.trim();
        const production = lotForm.production;
        const expiryCheck = CheckExpiry(expiry, draft.roDate);
        if (!expiryCheck.ok) { ShowMessage(expiryCheck.message, true); return; }
        if (batch === '') { ShowMessage('Type the lot #', true); return; }
        const productionCheck = CheckProduction(production, draft.roDate);
        if (!productionCheck.ok) { ShowMessage(productionCheck.message, true); return; }
        const parsed = ParseQty(lotForm.qty);
        if (!parsed.ok) { ShowMessage(parsed.message, true); return; }
        if (lots.some(lot => lot.lotID !== lotForm.lotID && lot.expiry === expiry && lot.batch.toUpperCase() === batch.toUpperCase())) {
            ShowMessage('This expiry date and lot # are already in the line', true);
            return;
        }
        if (lotForm.barcodes.length > parsed.value) { ShowMessage('The barcodes cannot be more than the qty of the lot', true); return; }
        if (lineStatus === 'R' && item.itemQtyControl === 'Y') {
            let stockData = stock;
            try { stockData = stock ?? await LoadStock(); }
            catch (err) { ShowMessage(`The stock cannot be checked : ${err.message}`, true); return; }
            const returnIssue = ReturnIssue(expiry, batch, parsed.value, stockData);
            if (returnIssue !== '') { ShowMessage(returnIssue, true); return; }
        }

        const lot = { lotID: lotForm.lotID ?? NextID(), expiry, batch, production, qty: parsed.value, barcodes: lotForm.barcodes };
        setLots(current => lotForm.lotID ? current.map(row => row.lotID === lot.lotID ? lot : row) : [...current, lot]);
        setLotForm(null);
        Keyboard.dismiss();
        ShowMessage(`Lot ${expiry} / ${batch} : ${FormatQty(lot.qty)}`);
    };

    const LotCard = (lot) => (
        <TouchableOpacity key={lot.lotID} style={styles.lotCard} disabled={readOnly}
            onPress={() => setLotForm({ ...lot, qty: FormatQty(lot.qty), production: lot.production ?? '' })}>
            <View style={styles.lotCardText}>
                <Text style={styles.lotTitleText}>{lot.expiry}  ·  Lot {lot.batch}</Text>
                <Text style={styles.lotDetailText}>
                    {lot.production ? `Production ${lot.production}  ·  ` : ''}{Plural((lot.barcodes ?? []).length, 'barcode')}
                </Text>
            </View>
            <Text style={styles.lotQtyText}>{FormatQty(lot.qty)}</Text>
            {!readOnly ?
                <TouchableOpacity style={styles.lotRemoveButton} onPress={() => setLots(current => current.filter(row => row.lotID !== lot.lotID))}>
                    <Text style={styles.removeText}>✕</Text>
                </TouchableOpacity> :
                <></>}
        </TouchableOpacity>
    );

    // A return picks one of the lots in stock in the location
    // The lots of the item : the ones known (with their production date) and the qty of each one in the location
    const itemLots = (() => {
        const rows = {};
        (stock?.knownLots ?? []).forEach(lot => {
            rows[`${lot.lotExpiry}|${lot.lotBatch}`] = { expiry: lot.lotExpiry, batch: lot.lotBatch, production: lot.lotProduction, available: null };
        });
        (stock?.lots ?? []).filter(lot => lot.lotExpiry !== '').forEach(lot => {
            const key = `${lot.lotExpiry}|${lot.lotBatch}`;
            rows[key] = { expiry: lot.lotExpiry, batch: lot.lotBatch, production: rows[key]?.production ?? '', available: Number(lot.lotAvailable) };
        });
        return Object.values(rows);
    })();

    // A lot chosen fills the lot window (its barcodes and qty are typed after)
    const PickLot = (lot) => {
        setLotLookupVisible(false);
        setLotForm(form => ({ ...form, expiry: lot.expiry, batch: lot.batch, production: lot.production ?? '' }));
        const check = CheckExpiry(lot.expiry, draft.roDate);
        if (!check.ok) ShowMessage(check.message, true);
    };

    // The expiry is checked as soon as it is complete
    const TypeExpiry = (text) => {
        const expiry = MaskLotDate(text);
        UpdateLotForm({ expiry });
        if (expiry.length === 10) {
            const check = CheckExpiry(expiry, draft.roDate);
            if (!check.ok) ShowMessage(check.message, true);
        }
    };

    // The lot is typed in a small window over the line
    const LotFormModal = () => (
        <Modal visible={lotForm !== null} transparent={true} presentationStyle="overFullScreen" onRequestClose={() => setLotForm(null)}>
            <View style={styles.modalContainer}>
                <View style={styles.modalCard}>
                    <ScrollView keyboardShouldPersistTaps='handled'>
                        {lotForm ? LotFormCard() : <></>}
                    </ScrollView>
                </View>
            </View>
        </Modal>
    );

    const LotFormCard = () => (
        <View style={styles.lotFormCard}>
            <Text style={styles.lotFormTitleText}>{lotForm.lotID ? 'Change The Lot' : 'New Lot'}  ·  {item.itemReference}</Text>

            {itemLots.length > 0 ?
                <TouchableOpacity style={styles.lotLookupButton} onPress={() => setLotLookupVisible(true)}>
                    <Text style={styles.lotLookupText}>Choose An Existing Lot ({itemLots.length})  ▼</Text>
                </TouchableOpacity> :
                <></>}

            <ROChoiceModal
                visible={lotLookupVisible}
                title='Lots Of The Item'
                rows={itemLots}
                keyOf={lot => `${lot.expiry}|${lot.batch}`}
                textsOf={lot => ({
                    title: `${lot.expiry}  ·  Lot ${lot.batch}`,
                    detail: `In stock in ${draft.location?.locationCode} : ${lot.available === null ? '0' : FormatQty(lot.available)}`,
                    note: lot.production ? `Production ${lot.production}` : '',
                    warning: CheckExpiry(lot.expiry, draft.roDate).ok ? '' : 'Expired',
                })}
                onPick={PickLot}
                onClose={() => setLotLookupVisible(false)}
            />

            <View style={styles.fieldRow}>
                <View style={styles.fieldHalf}>
                    <Text style={styles.fieldTitleText}>Expiry Date</Text>
                    <TextInput style={styles.fieldText} value={lotForm.expiry} placeholder='DD.MM.YYYY' keyboardType='numeric' maxLength={10}
                        autoFocus={lotForm.expiry === ''}
                        onChangeText={TypeExpiry} />
                </View>
                <View style={styles.fieldHalf}>
                    <Text style={styles.fieldTitleText}>Lot #</Text>
                    <TextInput style={styles.fieldText} value={lotForm.batch} placeholder='Lot #' maxLength={15} autoCapitalize='characters' autoCorrect={false}
                        onChangeText={text => UpdateLotForm({ batch: text })} />
                </View>
            </View>

            <View style={styles.fieldRow}>
                <View style={styles.fieldHalf}>
                    <Text style={styles.fieldTitleText}>Production (Optional)</Text>
                    <TextInput style={styles.fieldText} value={lotForm.production} placeholder='DD.MM.YYYY' keyboardType='numeric' maxLength={10}
                        onChangeText={text => setLotForm(form => ({ ...form, production: MaskLotDate(text) }))} />
                </View>
                <View style={styles.fieldHalf}>
                    <Text style={styles.fieldTitleText}>Qty ({item.itemUnit})</Text>
                    <TextInput style={[styles.fieldText, styles.qtyFieldText]} value={lotForm.qty} placeholder='0' keyboardType='numeric' maxLength={14}
                        onChangeText={text => setLotForm(form => ({ ...form, qty: text }))} />
                </View>
            </View>
            <Text style={styles.hintText}>Type the digits, the dots are added : 25072027 = 25.07.2027. Day 00 = the whole month (00.07.2027)</Text>

            {BarcodesSection('Barcodes Of The Lot')}

            <View style={styles.lotFormButtons}>
                <TouchableOpacity style={[styles.lotButton, styles.lotButtonCancel]} onPress={() => { setLotForm(null); Keyboard.dismiss(); }}>
                    <Image source={require('../assets/icons/close.png')} style={styles.lotButtonImage} resizeMode='contain' />
                    <Text style={[styles.lotButtonText, { color: Colors.brandBlue }]}>Cancel</Text>
                </TouchableOpacity>
                <TouchableOpacity style={[styles.lotButton, styles.lotButtonOk]} onPress={SaveLot}>
                    <Image source={require('../assets/icons/ok.png')} style={styles.lotButtonImage} resizeMode='contain' />
                    <Text style={[styles.lotButtonText, { color: 'white' }]}>OK</Text>
                </TouchableOpacity>
            </View>
        </View>
    );

    //#endregion

    //#region Save line

    const SaveLine = async () => {

        if (lotForm) { ShowMessage('Press OK to keep the lot being typed, or Cancel', true); return; }
        const reference = supplierRef.replace(/[\r\n]/g, '').trim();
        if (reference === '') { ShowMessage('Type the supplier reference', true); return; }

        let qty = 0;
        if (lotItem) {
            if (lots.length === 0) { ShowMessage('Add the expiry date and lot of the qty received', true); return; }
            qty = lotsTotal;
        }
        else {
            const parsed = ParseQty(qtyText);
            if (!parsed.ok) { ShowMessage(parsed.message, true); qtyRef.current?.focus(); return; }
            qty = parsed.value;
            if (barcodes.length > qty) { ShowMessage('The barcodes cannot be more than the qty', true); return; }
        }

        // A return of a quantity controlled item : not more than the stock of the location (the status may have changed after the lots)
        if (lineStatus === 'R' && item.itemQtyControl === 'Y') {
            let stockData = stock;
            try { stockData = stock ?? await LoadStock(); }
            catch (err) { ShowMessage(`The stock cannot be checked : ${err.message}`, true); return; }
            const checks = lotItem ? lots.map(lot => [lot.expiry, lot.batch, Number(lot.qty)]) : [['', '', qty]];
            for (const [expiry, batch, checkQty] of checks) {
                const returnIssue = ReturnIssue(expiry, batch, checkQty, stockData);
                if (returnIssue !== '') { ShowMessage(returnIssue, true); return; }
            }
        }

        const line = {
            lineID: existing?.lineID ?? NextID(),
            itemRecID: item.itemRecID,
            itemReference: item.itemReference,
            itemDescription: item.itemDescription,
            itemClass: item.itemClass,
            itemUnit: item.itemUnit,
            itemBarcode: item.itemBarcode,
            itemQtyControl: item.itemQtyControl,
            itemMeasType: item.itemMeasType,
            itemConsFactor: item.itemConsFactor,
            itemVatType: item.itemVatType,
            itemCostCC: item.itemCostCC,
            itemCurrency: item.itemCurrency,
            isPending,
            ...(existing?.unitCost !== undefined ? { unitCost: existing.unitCost } : {}),
            supplierRef: reference,
            lineStatus,
            qty,
            lots: lotItem ? lots : [],
            barcodes: lotItem ? [] : barcodes,
            ...(existing?.poQty !== undefined ? { poQty: existing.poQty } : {}),
        };

        let note = '';
        SetLines(current => {
            if (existing) return current.map(row => row.lineID === line.lineID ? line : row);
            // The same item, status and supplier ref already has a new line : the qty goes in it
            const same = current.find(row => row.itemRecID.toString() === line.itemRecID.toString() && !!row.isPending === isPending
                && row.lineStatus === line.lineStatus && row.supplierRef === line.supplierRef);
            if (!same) return [...current, line];
            note = `${line.itemReference} : added to the line already in the receiving order`;
            const merged = { ...same };
            if (lotItem) {
                merged.lots = [...same.lots];
                line.lots.forEach(lot => {
                    const sameLot = merged.lots.find(row => row.expiry === lot.expiry && row.batch.toUpperCase() === lot.batch.toUpperCase());
                    if (sameLot) merged.lots = merged.lots.map(row => row === sameLot ? { ...row, qty: Number(row.qty) + Number(lot.qty), barcodes: [...row.barcodes, ...lot.barcodes] } : row);
                    else merged.lots.push(lot);
                });
                merged.qty = LineQty(merged);
            }
            else {
                merged.qty = Number(same.qty) + line.qty;
                merged.barcodes = [...same.barcodes, ...line.barcodes];
            }
            return current.map(row => row === same ? merged : row);
        });

        navigation.goBack();
        if (note !== '') setTimeout(() => alert(note), 300);
    };

    //#endregion

    const ChangeQty = (step) => {
        const parsed = ParseQty(qtyText, true);
        const value = Math.max(0, (parsed.ok ? parsed.value : 0) + step);
        setQtyText(FormatQty(value));
    };

    const available = item.itemQtyControl === 'Y' && !lotItem ? AvailableOf('', '') : null;

    return (
        <View style={styles.container}>

            <ScrollView keyboardShouldPersistTaps='handled' contentContainerStyle={styles.scrollContent}>

                {/* The item */}
                <View style={styles.itemCard}>
                    <Text style={styles.itemReferenceText} numberOfLines={1}>{item.itemReference}</Text>
                    <Text style={styles.itemDescriptionText} numberOfLines={2}>{item.itemDescription}</Text>
                    <Text style={styles.itemInfoText} numberOfLines={1}>
                        {lotItem ? 'Expiry Item' : 'Trading Item'}  ·  {item.itemUnit}{item.itemBarcode ? `  ·  ${item.itemBarcode}` : ''}
                    </Text>
                    <Text style={styles.itemInfoText} numberOfLines={1}>
                        VAT {VatLabel(item.itemVatType)}  ·  Cost {FormatCost(LineCost(item))}{item.itemCurrency ? ` ${item.itemCurrency}` : ''}{existing?.unitCost !== undefined ? ' (PO)' : ''}
                    </Text>
                    {isPending ? <Text style={styles.itemPendingText}>Pending item : created in the stock file when the RO is saved</Text> : <></>}
                    {existing?.poQty !== undefined ? <Text style={styles.itemPOText}>Purchase order : {FormatQty(existing.poQty)} to receive</Text> : <></>}
                    {lineStatus === 'R' && item.itemQtyControl === 'Y' && !lotItem ?
                        <Text style={styles.itemPOText}>In stock in {draft.location?.locationCode} : {stock === null ? 'checking...' : available === null ? 'none' : FormatQty(available)}</Text> :
                        <></>}
                </View>

                {message.text !== '' ?
                    <View style={[styles.messageContainer, message.isError ? styles.messageError : null]}>
                        <Text style={styles.messageText}>{message.text}</Text>
                    </View> :
                    <></>}

                {/* Normal / Free / Return */}
                <View style={styles.statusRow}>
                    {STATUS_ORDER.map(code => (
                        <TouchableOpacity key={code} disabled={readOnly || (isPending && code === 'R')}
                            style={[styles.statusButton, lineStatus === code ? { backgroundColor: STATUS_COLORS[code], borderColor: STATUS_COLORS[code] } : null,
                                isPending && code === 'R' ? styles.statusButtonDisabled : null]}
                            onPress={() => setLineStatus(code)}>
                            <Text style={[styles.statusButtonText, lineStatus === code ? styles.statusButtonTextOn : null]}>{STATUS_LABELS[code]}</Text>
                        </TouchableOpacity>
                    ))}
                </View>

                <View style={styles.section}>
                    <Text style={styles.fieldTitleText}>Supplier Reference</Text>
                    <TextInput style={styles.fieldText} value={supplierRef} editable={!readOnly} maxLength={30} autoCorrect={false}
                        onChangeText={setSupplierRef} />
                </View>

                {!lotItem ?
                    <>
                        <View style={styles.section}>
                            <Text style={styles.fieldTitleText}>Qty ({item.itemUnit})</Text>
                            <View style={styles.qtyRow}>
                                {!readOnly ?
                                    <TouchableOpacity style={styles.qtyStepButton} onPress={() => ChangeQty(-1)}>
                                        <Text style={styles.qtyStepText}>−</Text>
                                    </TouchableOpacity> :
                                    <></>}
                                <TextInput
                                    ref={qtyRef}
                                    style={styles.qtyText}
                                    value={qtyText}
                                    editable={!readOnly}
                                    keyboardType='numeric'
                                    selectTextOnFocus={true}
                                    maxLength={14}
                                    onChangeText={setQtyText}
                                />
                                {!readOnly ?
                                    <TouchableOpacity style={styles.qtyStepButton} onPress={() => ChangeQty(1)}>
                                        <Text style={styles.qtyStepText}>+</Text>
                                    </TouchableOpacity> :
                                    <></>}
                            </View>
                        </View>
                        {BarcodesSection('Barcodes')}
                    </> :
                    <View style={styles.section}>
                        <View style={styles.sectionTitleRow}>
                            <Text style={styles.sectionTitleText}>Expiry Dates And Lots</Text>
                            <Text style={styles.sectionCountText}>Total {FormatQty(lotsTotal)} {item.itemUnit}</Text>
                        </View>
                        {/* On top : reached without scrolling when the line has many lots */}
                        {!readOnly ?
                            <View style={styles.addLotContainer}>
                                <MainButton title='Add Lot' imageSource={require('../assets/icons/add.png')}
                                    onPress={() => setLotForm({ ...EMPTY_LOT, lotID: null })} />
                            </View> :
                            <></>}
                        {lots.length === 0 ? <Text style={styles.hintText}>Add the expiry date and lot of the qty received</Text> : <></>}
                        {[...lots].reverse().map(lot => LotCard(lot))}
                    </View>}

            </ScrollView>

            {LotFormModal()}

            <View style={styles.bottomContainer}>
                {readOnly ?
                    <MainButton title='Back' imageSource={require('../assets/icons/close.png')} onPress={() => navigation.goBack()} /> :
                    <>
                        <MainButton title='Cancel' imageSource={require('../assets/icons/close.png')} onPress={() => navigation.goBack()} />
                        <MainButton title={existing ? 'OK' : 'Add Line'} imageSource={require('../assets/icons/ok.png')} onPress={SaveLine} />
                    </>}
            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: Colors.bgColor,
    },
    scrollContent: {
        paddingBottom: 20,
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
    itemCard: {
        backgroundColor: Colors.listItemBackColor,
        paddingHorizontal: 12,
        paddingVertical: 8,
    },
    itemReferenceText: {
        color: 'black',
        fontSize: FontSize(18),
        fontWeight: 'bold',
    },
    itemDescriptionText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
    },
    itemInfoText: {
        color: 'gray',
        fontSize: FontSize(13),
    },
    itemPOText: {
        color: Colors.brandGreen,
        fontSize: FontSize(14),
        fontWeight: 'bold',
    },
    messageContainer: {
        backgroundColor: Colors.brandBlue,
        paddingHorizontal: 10,
        paddingVertical: 6,
    },
    messageError: {
        backgroundColor: 'red',
    },
    messageText: {
        color: 'white',
        fontSize: FontSize(15),
        fontWeight: 'bold',
        textAlign: 'center',
    },
    statusRow: {
        flexDirection: 'row',
        padding: 10,
        paddingBottom: 4,
    },
    statusButton: {
        flex: 1,
        height: 44,
        marginHorizontal: 3,
        borderRadius: 15,
        borderWidth: 1,
        borderColor: Colors.buttonBorderColor,
        backgroundColor: 'white',
        alignItems: 'center',
        justifyContent: 'center',
    },
    statusButtonText: {
        color: Colors.brandBlue,
        fontSize: FontSize(16),
        fontWeight: 'bold',
    },
    statusButtonTextOn: {
        color: 'white',
    },
    section: {
        paddingHorizontal: 12,
        paddingTop: 8,
    },
    sectionTitleRow: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center',
        marginBottom: 4,
    },
    sectionTitleText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
        fontWeight: 'bold',
    },
    sectionCountText: {
        color: 'gray',
        fontSize: FontSize(14),
    },
    fieldTitleText: {
        color: Colors.brandBlue,
        fontSize: FontSize(14),
        fontWeight: 'bold',
        marginBottom: 2,
    },
    fieldText: {
        height: 44,
        backgroundColor: 'white',
        color: 'black',
        fontSize: FontSize(17),
        borderWidth: 1,
        borderColor: 'silver',
        borderRadius: 4,
        paddingHorizontal: 8,
        paddingVertical: 0,
    },
    qtyFieldText: {
        textAlign: 'center',
        fontWeight: 'bold',
    },
    fieldRow: {
        flexDirection: 'row',
        marginBottom: 6,
    },
    fieldHalf: {
        flex: 1,
        marginHorizontal: 3,
    },
    qtyRow: {
        flexDirection: 'row',
        alignItems: 'center',
    },
    qtyStepButton: {
        width: 60,
        height: 54,
        borderRadius: 15,
        borderWidth: 1,
        borderColor: Colors.buttonBorderColor,
        backgroundColor: 'white',
        alignItems: 'center',
        justifyContent: 'center',
    },
    qtyStepText: {
        color: Colors.brandBlue,
        fontSize: FontSize(28),
        fontWeight: 'bold',
    },
    qtyText: {
        flex: 1,
        height: 54,
        marginHorizontal: 8,
        backgroundColor: 'white',
        color: 'black',
        fontSize: FontSize(24),
        fontWeight: 'bold',
        textAlign: 'center',
        borderWidth: 1,
        borderColor: 'silver',
        borderRadius: 4,
        paddingVertical: 0,
    },
    barcodeEntryRow: {
        flexDirection: 'row',
        alignItems: 'center',
        marginBottom: 6,
    },
    barcodeText: {
        flex: 1,
        height: 44,
        backgroundColor: 'white',
        color: 'black',
        fontSize: FontSize(16),
        borderWidth: 1,
        borderColor: 'silver',
        borderRadius: 4,
        paddingHorizontal: 8,
        paddingVertical: 0,
        marginRight: 6,
    },
    smallButton: {
        height: 44,
        paddingHorizontal: 12,
        borderRadius: 15,
        borderWidth: 1,
        borderColor: Colors.buttonBorderColor,
        backgroundColor: 'white',
        alignItems: 'center',
        justifyContent: 'center',
    },
    smallButtonGray: {
        borderColor: 'silver',
    },
    smallButtonOk: {
        backgroundColor: Colors.brandGreen,
        borderColor: Colors.brandGreen,
    },
    smallButtonText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
        fontWeight: 'bold',
    },
    chipsContainer: {
        flexDirection: 'row',
        flexWrap: 'wrap',
        marginBottom: 4,
    },
    chip: {
        flexDirection: 'row',
        alignItems: 'center',
        paddingHorizontal: 10,
        paddingVertical: 5,
        marginRight: 6,
        marginBottom: 6,
        borderRadius: 12,
        backgroundColor: 'rgb(220, 240, 255)',
    },
    chipText: {
        color: 'black',
        fontSize: FontSize(14),
    },
    chipRemoveText: {
        color: 'red',
        fontSize: FontSize(15),
        fontWeight: 'bold',
    },
    lotLookupButton: {
        height: 40,
        borderRadius: 15,
        borderWidth: 1,
        borderColor: Colors.buttonBorderColor,
        backgroundColor: 'rgb(220, 240, 255)',
        alignItems: 'center',
        justifyContent: 'center',
        marginBottom: 8,
    },
    lotLookupText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
        fontWeight: 'bold',
    },
    itemPendingText: {
        color: 'rgb(220, 120, 0)',
        fontSize: FontSize(13),
        fontWeight: 'bold',
    },
    statusButtonDisabled: {
        opacity: 0.3,
    },
    stockLotChip: {
        paddingHorizontal: 10,
        paddingVertical: 6,
        marginRight: 6,
        marginBottom: 6,
        borderRadius: 12,
        borderWidth: 1,
        borderColor: 'rgb(200, 60, 60)',
        backgroundColor: 'white',
    },
    stockLotChipText: {
        color: 'rgb(200, 60, 60)',
        fontSize: FontSize(13),
        fontWeight: 'bold',
    },
    hintText: {
        color: 'gray',
        fontSize: FontSize(13),
        fontStyle: 'italic',
        marginBottom: 6,
    },
    lotCard: {
        flexDirection: 'row',
        alignItems: 'center',
        minHeight: 54,
        backgroundColor: Colors.listItemBackColor,
        marginBottom: 3,
        paddingLeft: 10,
        borderRadius: 6,
    },
    lotCardText: {
        flex: 1,
    },
    lotTitleText: {
        color: 'black',
        fontSize: FontSize(15),
        fontWeight: 'bold',
    },
    lotDetailText: {
        color: 'gray',
        fontSize: FontSize(13),
    },
    lotQtyText: {
        color: 'black',
        fontSize: FontSize(18),
        fontWeight: 'bold',
        marginLeft: 6,
    },
    lotRemoveButton: {
        width: 40,
        height: 44,
        alignItems: 'center',
        justifyContent: 'center',
    },
    removeText: {
        color: 'red',
        fontSize: FontSize(19),
        fontWeight: 'bold',
    },
    modalContainer: {
        width: '100%',
        height: '100%',
        backgroundColor: 'rgba(0,0,0,0.5)',
        alignItems: 'center',
        justifyContent: 'center',
    },
    modalCard: {
        width: Sizing.windowsWidth * 0.94,
        maxHeight: Sizing.windowsHeight * 0.9,
        backgroundColor: 'white',
        borderRadius: 20,
        borderWidth: 5,
        borderColor: Colors.brandBlue,
        overflow: 'hidden',
    },
    lotFormCard: {
        padding: 10,
    },
    lotFormTitleText: {
        color: Colors.brandBlue,
        fontSize: FontSize(16),
        fontWeight: 'bold',
        marginBottom: 6,
    },
    lotFormButtons: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        marginTop: 12,
    },
    lotButton: {
        width: '48.5%',
        height: 52,
        borderRadius: 15,
        borderWidth: 1,
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
    },
    lotButtonCancel: {
        borderColor: Colors.buttonBorderColor,
        backgroundColor: 'white',
    },
    lotButtonOk: {
        borderColor: Colors.brandGreen,
        backgroundColor: Colors.brandGreen,
    },
    lotButtonImage: {
        width: 28,
        height: 28,
        marginRight: 8,
    },
    lotButtonText: {
        fontSize: FontSize(18),
        fontWeight: 'bold',
    },
    addLotContainer: {
        marginBottom: 4,
    },
    bottomContainer: {
        flexDirection: 'row',
        justifyContent: Sizing.isTablet ? 'flex-end' : 'space-between',
        borderTopWidth: 1,
        borderTopColor: 'silver',
        paddingHorizontal: 10,
        paddingTop: 8,
        backgroundColor: Colors.bgColor,
    },
});

export default ReceivingOrderLineScreen;
