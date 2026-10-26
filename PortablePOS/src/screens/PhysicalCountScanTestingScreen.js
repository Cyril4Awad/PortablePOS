import React, { useState, useEffect, useRef } from 'react';
import { View, Text, TextInput, StyleSheet, ScrollView, TouchableOpacity, Modal, Keyboard, Vibration, Image } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import DataField from '../components/controls/DataField';
import ModalTitle from '../components/ModalTitle';
import NumberPadModal from '../components/controls/NumberPadModal';
import MessageBox from '../components/MessageBox';
import PhysicalCountDifferences from '../components/lists/physicalCount/PhysicalCountDifferences';
import PhysicalCountItemPicker from '../components/lists/physicalCount/PhysicalCountItemPicker';
import PhysicalCountDatabase from '../database/PhysicalCountDatabase';
import { PlayScanOk, PlayScanError, PlayQtyConfirmed } from '../variables/PhysicalCountSounds';
import { FormatQuantity, LookupScan, LineFields, CodeDescription } from '../variables/PhysicalCountRules';
import {
    MODES, MODE_LABELS, MODE_DESCRIPTIONS, DEFAULT_SETTINGS, MAX_QUICK_BUTTONS, ClassifyFormat, IsScannedInput, QtyFromText, QtyError,
    CreateSession, KnowLine, AddEntry, SetTotal, StartPending, ConfirmPending,
    SetNextQty, ScanWithNextQty, Undo, UndoLabel, Preview, IsSameItemAgain, RememberScan,
} from '../variables/PhysicalCountModes';

// Scan For Testing : the scan screen of the physical count with 4 counting modes
// It counts on the downloaded count like the Scan Items screen (the lines are uploaded with the Upload button)

const modeStorageKey = '@app_scanForTestingMode';
const settingsStorageKey = '@app_scanForTestingSettings';

const PhysicalCountScanTestingScreen = ({ route }) => {

    // Opened with a lineRecID (from View Items) the line is shown and its Set Total opens
    const { server, location, lineRecID: openedLineRecID } = route.params;
    const countKey = { server, location };

    // Downloaded header : modes (trading, fashion, expiry, book) and available qty privilege
    const [header, setHeader] = useState(null);

    const [mode, setModeState] = useState(MODES.SCAN_THEN_TYPE);
    const modeRef = useRef(mode);
    const setMode = (value) => { modeRef.current = value; setModeState(value); };

    const [settings, setSettings] = useState(DEFAULT_SETTINGS);
    const [modePickerVisible, setModePickerVisible] = useState(false);
    const [differencesVisible, setDifferencesVisible] = useState(false);

    // The session is kept in a ref so fast scans always work on the latest totals
    const sessionRef = useRef(CreateSession());
    const [session, setSessionState] = useState(sessionRef.current);
    const setSession = (next) => { sessionRef.current = next; setSessionState(next); };

    const [line, setLine] = useState(null);
    const [resultText, setResultText] = useState('Ready to scan');
    const [resultIsError, setResultIsError] = useState(false);
    const [warningText, setWarningText] = useState('');

    const [scanText, setScanText] = useState('');
    const [qtyText, setQtyTextState] = useState('1');            // scan then type
    const qtyTextRef = useRef('1');
    const setQtyText = (value) => { qtyTextRef.current = value; setQtyTextState(value); };
    const [nextQtyText, setNextQtyText] = useState('');         // type then scan

    // Scanned text when it is the start of more than one reference
    const [pickSearch, setPickSearch] = useState(null);
    // true : pickSearch is a barcode of more than one line
    const [pickByBarcode, setPickByBarcode] = useState(false);

    // { title, onSubmit, preview }
    const [numberPad, setNumberPad] = useState(null);

    const scanRef = useRef();
    const qtyRef = useRef();
    const nextQtyRef = useRef();

    // Time of the first character typed in each quantity box : a scanner types its characters and Enter
    // within a few milliseconds, so a scan of the next item in a quantity box is not taken as a quantity
    const firstCharTime = useRef({ qty: null, nextQty: null });

    // Scans and buttons run one at a time, in order, so fast scans never lose a count
    const queue = useRef(Promise.resolve());

    //#region Loading, mode and settings

    useEffect(() => {

        PhysicalCountDatabase.GetHeader(countKey).then(setHeader);

        if (openedLineRecID) {
            enqueue(async () => {
                const openedLine = await PhysicalCountDatabase.GetLine(countKey, openedLineRecID);
                setSession(KnowLine(sessionRef.current, openedLineRecID, openedLine.counted, openedLine.state));
                setLine(openedLine);
                showResult('Scan to count, or Set Total');
                // Opened from View Items : Set Total opens directly on the line
                OpenSetTotal(openedLineRecID, openedLine.itemReference);
            });
        }

        AsyncStorage.getItem(settingsStorageKey).then(saved => {
            if (!saved) return;
            const savedSettings = { ...DEFAULT_SETTINGS, ...JSON.parse(saved) };
            setSettings({ ...savedSettings, quickButtons: savedSettings.quickButtons.slice(0, MAX_QUICK_BUTTONS) });
        }).catch(() => { });

        // The mode never changes by itself, the last one is remembered on the device
        AsyncStorage.getItem(modeStorageKey).then(saved => {
            const savedMode = saved && MODE_LABELS[saved] ? saved : MODES.SCAN_THEN_TYPE;
            setMode(savedMode);
            if (savedMode === MODES.TYPE_THEN_SCAN) FocusNextQty();
        }).catch(() => { });

        // Leaving the screen with a scan waiting for its quantity : it is saved with the quantity shown
        return () => {
            const current = sessionRef.current;
            if (!current.pending) return;
            // a negative or 0 quantity is not saved
            if (modeRef.current === MODES.SCAN_THEN_TYPE && QtyError(qtyTextRef.current) !== '') return;
            const typed = modeRef.current === MODES.SCAN_THEN_TYPE && ClassifyFormat(qtyTextRef.current) === 'quantity' ? QtyFromText(qtyTextRef.current) : undefined;
            const result = ConfirmPending(current, typed);
            if (result.save) PhysicalCountDatabase.SaveLineCounted(countKey, result.save.lineRecID, result.save.counted).catch(error => console.log(error));
        };

    }, []);

    const ChangeMode = (newMode) => {
        // a scan waiting for its quantity is confirmed with it before changing the mode
        enqueue(async () => {
            if (modeRef.current === MODES.SCAN_THEN_TYPE && sessionRef.current.pending && QtyError(qtyTextRef.current) !== '') {
                setModePickerVisible(false);
                QtyRejected(QtyError(qtyTextRef.current), qtyRef);
                return;
            }
            await ConfirmWaiting(modeRef.current === MODES.SCAN_THEN_TYPE ? TypedQty() : undefined);
            setMode(newMode);
            AsyncStorage.setItem(modeStorageKey, newMode).catch(() => { });
            setModePickerVisible(false);
            setNextQtyText('');
            if (newMode === MODES.TYPE_THEN_SCAN) FocusNextQty(); else focusField(scanRef);
        });
    };

    const SaveSettings = (newSettings) => {
        setSettings(newSettings);
        AsyncStorage.setItem(settingsStorageKey, JSON.stringify(newSettings)).catch(() => { });
    };

    //#endregion

    //#region Helpers

    const enqueue = (action) => {
        queue.current = queue.current.then(action).catch(error => {
            PlayScanError();
            showResult(error.message, true);
        });
    };

    // selectAll : the whole text is selected once, when the field gets the focus (selectTextOnFocus),
    // the selection is never forced afterwards (a forced selection selected the first digit while typing)
    const focusField = (ref, selectAll = false) => setTimeout(() => {
        if (selectAll && ref.current?.isFocused()) {
            ref.current.blur();
            setTimeout(() => ref.current?.focus(), 30);
            return;
        }
        ref.current?.focus();
    }, 80);

    // Type then scan : the quantity box gets the focus with 1 selected
    const FocusNextQty = () => {
        setNextQtyText('1');
        firstCharTime.current.nextQty = null;
        focusField(nextQtyRef, true);
    };

    const showResult = (text, isError = false) => {
        setResultText(text);
        setResultIsError(isError);
    };

    const describeLine = (lineRecID) => line?.lineRecID === lineRecID ? line.itemReference : lineRecID;

    // Typed quantity of the scan then type box (undefined when it is not a number)
    const TypedQty = () => ClassifyFormat(qtyTextRef.current) === 'quantity' ? QtyFromText(qtyTextRef.current) : undefined;

    // Saves a counting step on the downloaded count, feedback
    const ApplyResult = async (result) => {
        setSession(result.session);
        if (result.save) {
            await PhysicalCountDatabase.SaveLineCounted(countKey, result.save.lineRecID, result.save.counted);
            setLine(current => current && current.lineRecID === result.save.lineRecID ? { ...current, counted: result.save.counted, state: 1 } : current);
            PlayQtyConfirmed();
            Vibration.vibrate(15);
        }
        showResult(result.message);
    };

    // Confirms the scan waiting for its quantity (scan then type, buttons)
    const ConfirmWaiting = async (qty) => {
        if (!sessionRef.current.pending) return;
        await ApplyResult(ConfirmPending(sessionRef.current, qty));
    };

    const NotFound = (text) => {
        setLine(null);
        setWarningText('');
        PlayScanError();
        Vibration.vibrate([0, 60, 60, 60]);
        showResult(text, true);
    };

    // Negative or 0 quantity : same error beep as not found, the quantity box gets the focus again with 1 selected
    const QtyRejected = (message, ref) => {
        PlayScanError();
        Vibration.vibrate([0, 60, 60, 60]);
        showResult(message, true);
        if (ref === nextQtyRef) { FocusNextQty(); return; }
        if (ref === scanRef) { focusField(scanRef); return; }
        setQtyText('1');
        firstCharTime.current.qty = null;
        focusField(ref, true);
    };

    //#endregion

    //#region Scans

    // A line was found (scan or picker) : counted according to the mode
    const ProcessLine = async (lineRecID) => {

        const foundLine = await PhysicalCountDatabase.GetLine(countKey, lineRecID);
        let current = KnowLine(sessionRef.current, lineRecID, foundLine.counted, foundLine.state);

        const now = Date.now();
        setWarningText(IsSameItemAgain(current, lineRecID, now, settings.sameItemWarningMs) ? `Same item again : ${foundLine.itemReference}` : '');
        current = RememberScan(current, lineRecID, now);
        setSession(current);
        setLine(foundLine);
        PlayScanOk();
        Vibration.vibrate(15);

        switch (modeRef.current) {
            case MODES.PLUS_ONE:
                await ApplyResult(AddEntry(current, lineRecID, 1000));
                focusField(scanRef);
                break;
            case MODES.TYPE_THEN_SCAN:
                await ApplyResult(ScanWithNextQty(current, lineRecID));
                FocusNextQty();
                break;
            case MODES.SCAN_THEN_TYPE: {
                const result = StartPending(current, lineRecID);
                setSession(result.session);
                showResult(result.message);
                setQtyText('1');
                firstCharTime.current.qty = null;
                focusField(qtyRef, true);
                break;
            }
            case MODES.BUTTONS: {
                const result = StartPending(current, lineRecID);
                setSession(result.session);
                showResult(result.message);
                focusField(scanRef);
                break;
            }
        }
    };

    // Same search as the Scan Items screen : barcode, start of the reference (picker if several), QR
    const SearchAndProcess = async (scannedText) => {
        const result = await LookupScan(PhysicalCountDatabase, countKey, scannedText);
        switch (result.kind) {
            case 'found':
                await ProcessLine(result.lineRecID);
                break;
            case 'pick':
                setPickByBarcode(result.byBarcode === true);
                setPickSearch(result.value);
                break;
            case 'invalidQr':
                NotFound(`Invalid QR : ${scannedText.trim()}`);
                if (modeRef.current === MODES.TYPE_THEN_SCAN) FocusNextQty(); else focusField(scanRef);
                break;
            case 'notFound':
                NotFound(`Item Not Found : ${scannedText.trim()}`);
                if (modeRef.current === MODES.TYPE_THEN_SCAN) FocusNextQty(); else focusField(scanRef);
                break;
        }
    };

    const HandleScanField = () => {

        const text = scanText;
        setScanText('');

        enqueue(async () => {

            // The scan field is always searched (barcode, reference, QR), never taken as a quantity
            if (text.trim() === '') return;

            // The next scan confirms the waiting one, not with a negative or 0 quantity : the scan is not taken, fix the quantity then scan again
            if (modeRef.current === MODES.SCAN_THEN_TYPE && sessionRef.current.pending && QtyError(qtyTextRef.current) !== '') {
                QtyRejected(QtyError(qtyTextRef.current), qtyRef);
                return;
            }
            if (modeRef.current === MODES.SCAN_THEN_TYPE) await ConfirmWaiting(TypedQty());
            if (modeRef.current === MODES.BUTTONS) await ConfirmWaiting();

            await SearchAndProcess(text);

        });
    };

    // Scan then type : Enter in the quantity box
    const HandleQtyField = () => {

        const text = qtyTextRef.current;
        const error = QtyError(text);
        const scanned = IsScannedInput(text, firstCharTime.current.qty, Date.now());
        firstCharTime.current.qty = null;

        enqueue(async () => {

            if (!sessionRef.current.pending) { focusField(scanRef); return; }

            // checked first : "-5" would be searched as a barcode
            if (error !== '') { QtyRejected(error, qtyRef); return; }

            if (!scanned) {
                await ApplyResult(ConfirmPending(sessionRef.current, QtyFromText(text)));
                setQtyText('1');
                focusField(scanRef);
                return;
            }

            // Scanner speed, letters, symbols or more than 9 digits : the next item was scanned into the quantity box
            await ConfirmWaiting();
            setQtyText('1');
            await SearchAndProcess(text);

        });
    };

    // Type then scan : Enter in the quantity box sets the quantity of the next scan
    const HandleNextQtyField = () => {

        const text = nextQtyText;
        const error = QtyError(text);
        const scanned = IsScannedInput(text, firstCharTime.current.nextQty, Date.now());
        firstCharTime.current.nextQty = null;

        enqueue(async () => {

            if (text.trim() === '') { focusField(scanRef); return; }

            // checked first : "-5" would be searched as a barcode
            if (error !== '') { QtyRejected(error, nextQtyRef); return; }

            if (!scanned) {
                const qty = QtyFromText(text);
                setSession(SetNextQty(sessionRef.current, qty));
                showResult(`Next scan adds ${FormatQuantity(qty)}`);
                focusField(scanRef);
                return;
            }

            // The item was scanned straight into the quantity box : counted with the current next quantity
            await SearchAndProcess(text);

        });
    };

    // Buttons mode : quick quantity
    const HandleQuickQty = (qty) => {
        enqueue(async () => {
            if (!sessionRef.current.pending) { showResult('Scan an item first', true); return; }
            await ApplyResult(ConfirmPending(sessionRef.current, qty * 1000));
            focusField(scanRef);
        });
    };

    //#endregion

    //#region Set total and undo

    const HandleSetTotal = () => {
        const lineRecID = sessionRef.current.pending?.lineRecID ?? line?.lineRecID;
        if (!lineRecID) { showResult('Scan an item first', true); return; }
        OpenSetTotal(lineRecID, describeLine(lineRecID));
    };

    const OpenSetTotal = (lineRecID, lineText) => {
        setNumberPad({
            title: `Set total of ${lineText}`,
            preview: (value) => `Total will be ${value}`,
            onSubmit: (value) => {
                setNumberPad(null);
                enqueue(async () => {
                    await ApplyResult(SetTotal(sessionRef.current, lineRecID, value * 1000));
                    if (modeRef.current === MODES.TYPE_THEN_SCAN) FocusNextQty(); else focusField(scanRef);
                });
            },
        });
    };

    const HandleUndo = () => {
        enqueue(async () => {
            const result = Undo(sessionRef.current, describeLine);
            setSession(result.session);
            if (result.restore) {
                await PhysicalCountDatabase.RestoreLineCounted(countKey, result.restore.lineRecID, result.restore.counted, result.restore.state);
                setLine(current => current && current.lineRecID === result.restore.lineRecID
                    ? { ...current, counted: result.restore.counted, state: result.restore.state } : current);
            }
            showResult(result.message, result.message === 'Nothing to undo');
            setQtyText('1');
            if (modeRef.current === MODES.TYPE_THEN_SCAN) FocusNextQty(); else focusField(scanRef);
        });
    };

    //#endregion

    //#region Render

    const pending = session.pending;
    const fields = LineFields(header, line);
    const total = line ? (session.counts[line.lineRecID] ?? line.counted) : null;
    const undoLabel = UndoLabel(session);

    // "Total will be N" before a typed quantity is confirmed
    let previewText = '';
    if (mode === MODES.SCAN_THEN_TYPE && pending && ClassifyFormat(qtyText) === 'quantity') {
        previewText = `Total will be ${FormatQuantity(Preview(session, pending.lineRecID, QtyFromText(qtyText)))}`;
    }
    else if (mode === MODES.BUTTONS && pending) {
        previewText = `Total will be ${FormatQuantity(Preview(session, pending.lineRecID, 1000))} with +1`;
    }
    else if (mode === MODES.TYPE_THEN_SCAN && ClassifyFormat(nextQtyText) === 'quantity') {
        previewText = `Next scan will add ${FormatQuantity(QtyFromText(nextQtyText))} (Enter to set)`;
    }

    return (
        <View style={styles.container}>

            <NumberPadModal
                visible={numberPad !== null}
                title={numberPad?.title ?? ''}
                preview={numberPad?.preview}
                onSubmit={(value) => numberPad?.onSubmit(value)}
                onCancel={() => { setNumberPad(null); focusField(scanRef); }}
            />

            <Modal visible={pickSearch !== null}>
                <PhysicalCountItemPicker
                    countKey={countKey}
                    searchString={pickSearch ?? ''}
                    byBarcode={pickByBarcode}
                    header={header}
                    onPick={(recID) => {
                        setPickSearch(null);
                        enqueue(() => ProcessLine(recID));
                    }}
                    onCloseButtonPress={() => {
                        setPickSearch(null);
                        if (modeRef.current === MODES.TYPE_THEN_SCAN) FocusNextQty(); else focusField(scanRef);
                    }}
                />
            </Modal>

            {/* Mode picker : list with the app check boxes and the quick quantities of the buttons mode */}
            <Modal visible={modePickerVisible} transparent={true} presentationStyle="overFullScreen"
                onRequestClose={() => { setModePickerVisible(false); focusField(scanRef); }}>
                <View style={styles.modalContainer}>
                    <View style={styles.modalCard}>
                        <ModalTitle title='Counting Mode' onCloseButtonPress={() => { setModePickerVisible(false); focusField(scanRef); }} />
                        <ScrollView keyboardShouldPersistTaps='handled'>
                            {Object.values(MODES).map(value => (
                                <TouchableOpacity key={value} style={styles.modeRow} onPress={() => ChangeMode(value)}>
                                    <View style={styles.modeRowTextContainer}>
                                        <Text style={styles.modeRowTitleText}>{MODE_LABELS[value]}</Text>
                                        <Text style={styles.modeRowDescriptionText}>{MODE_DESCRIPTIONS[value]}</Text>
                                    </View>
                                    <Image
                                        source={value === mode ? require('../assets/icons/checked.png') : require('../assets/icons/unchecked.png')}
                                        style={styles.checkBoxImage}
                                        resizeMode='contain'
                                    />
                                </TouchableOpacity>
                            ))}

                            <View style={styles.settingsContainer}>
                                <Text style={styles.settingTitleText}>Buttons : quick quantities, at most {MAX_QUICK_BUTTONS} (comma separated)</Text>
                                <TextInput style={styles.settingText} keyboardType='numbers-and-punctuation'
                                    defaultValue={settings.quickButtons.join(',')}
                                    onEndEditing={event => {
                                        const values = event.nativeEvent.text.split(',').map(text => parseInt(text.trim(), 10)).filter(value => value > 0 && value < 1000000000);
                                        if (values.length > MAX_QUICK_BUTTONS) alert(`At most ${MAX_QUICK_BUTTONS} quantities, the first ${MAX_QUICK_BUTTONS} are kept`);
                                        if (values.length > 0) SaveSettings({ ...settings, quickButtons: values.slice(0, MAX_QUICK_BUTTONS) });
                                    }} />
                            </View>
                        </ScrollView>
                    </View>
                </View>
            </Modal>

            {/* Available qty - counted, view only : same privilege as the available qty */}
            <PhysicalCountDifferences
                visible={differencesVisible}
                countKey={countKey}
                header={header}
                onClose={() => { setDifferencesVisible(false); focusField(scanRef); }}
            />

            {/* Mode : tap to change, List Differences at its right with the available qty privilege */}
            <View style={styles.optionsContainer}>
                <TouchableOpacity style={[styles.modeButton, { flex: 1 }]} onPress={() => setModePickerVisible(true)}>
                    <Text style={styles.modeButtonText} numberOfLines={1}>Mode : {MODE_LABELS[mode]} ▾</Text>
                </TouchableOpacity>
                {header?.showAvailableQty === 'Y' ?
                    <TouchableOpacity style={[styles.modeButton, { marginLeft: 5 }]} onPress={() => setDifferencesVisible(true)}>
                        <Text style={styles.modeButtonText} numberOfLines={1}>List Differences</Text>
                    </TouchableOpacity> :
                    <></>}
            </View>

            {/* Type then scan : quantity of the next scan */}
            {mode === MODES.TYPE_THEN_SCAN ?
                <View style={styles.entryContainer}>
                    <Text style={styles.entryLabelText}>Qty</Text>
                    <TextInput
                        ref={nextQtyRef}
                        style={[styles.entryText, styles.entryQtyText]}
                        value={nextQtyText}
                        placeholder='1'
                        keyboardType='number-pad'
                        selectTextOnFocus={true}
                        maxLength={13}
                        blurOnSubmit={false}
                        onChangeText={text => {
                            if (firstCharTime.current.nextQty === null) firstCharTime.current.nextQty = Date.now();
                            setNextQtyText(text);
                        }}
                        onSubmitEditing={HandleNextQtyField}
                    />
                    <Text style={styles.entryInfoText} numberOfLines={1}>Next scan adds {FormatQuantity(session.nextQty)}</Text>
                </View> :
                <></>}

            {/* Scan field : same green bar as the Scan Items screen */}
            <View style={styles.entryContainer}>
                <Image
                    source={require('../assets/icons/barcode.png')}
                    style={styles.scannerImage}
                    resizeMode='contain'
                />
                <TextInput
                    ref={scanRef}
                    style={styles.entryText}
                    value={scanText}
                    placeholder='Scan Item'
                    showSoftInputOnFocus={false}
                    onPressIn={() => Keyboard.dismiss()}
                    autoFocus={true}
                    blurOnSubmit={false}
                    maxLength={100}
                    onChangeText={setScanText}
                    onSubmitEditing={HandleScanField}
                />
            </View>

            {/* Scan then type : quantity of the scanned item (1 selected) */}
            {mode === MODES.SCAN_THEN_TYPE ?
                <View style={styles.entryContainer}>
                    <Text style={styles.entryLabelText}>Qty</Text>
                    <TextInput
                        ref={qtyRef}
                        style={[styles.entryText, styles.entryQtyText, { opacity: pending ? 1 : 0.5 }]}
                        value={qtyText}
                        editable={pending !== null}
                        keyboardType='number-pad'
                        selectTextOnFocus={true}
                        maxLength={13}
                        blurOnSubmit={false}
                        onChangeText={text => {
                            if (firstCharTime.current.qty === null) firstCharTime.current.qty = Date.now();
                            setQtyText(text);
                        }}
                        onSubmitEditing={HandleQtyField}
                    />
                </View> :
                <></>}

            {/* Status : blue, red when not found, tap it to read the whole message */}
            <TouchableOpacity style={[styles.statusContainer, { backgroundColor: resultIsError ? 'red' : Colors.brandBlue }]}
                onPress={() => MessageBox.alert(resultIsError ? 'Error' : 'Message', resultText, [{ text: 'OK', onPress: () => focusField(scanRef) }])}>
                <Text style={styles.statusText} numberOfLines={1}>{resultText}</Text>
            </TouchableOpacity>

            {previewText !== '' || warningText !== '' ?
                <View style={styles.noteContainer}>
                    {previewText !== '' ? <Text style={styles.previewText} numberOfLines={1}>{previewText}</Text> : <></>}
                    {warningText !== '' ? <Text style={styles.warningText} numberOfLines={1}>{warningText}</Text> : <></>}
                </View> :
                <></>}

            {/* Item and its fields : not scrollable, the buttons below are always visible */}
            <View style={styles.detailsContainer}>

                {/* Reference with the line id at its right (small, gray) and the description under it */}
                <View style={styles.itemContainer}>
                    <View style={styles.itemReferenceRow}>
                        <Text style={styles.itemReferenceText} numberOfLines={1}>{line ? line.itemReference : 'Scan an item'}</Text>
                        {line ? <Text style={styles.itemLineText} numberOfLines={1}>Line {line.lineRecID}</Text> : <></>}
                    </View>
                    {line ? <Text style={styles.itemDescriptionText} numberOfLines={1}>{line.itemDescription}</Text> : <></>}
                    {fields.showBook ? <Text style={styles.itemTitleText} numberOfLines={1}>{line.bookTitle}</Text> : <></>}
                </View>

                {fields.showFashion ?
                    <>
                        <View style={styles.fieldsRow}>
                            <View style={styles.fieldHalf}><DataField title='Color' data={CodeDescription(line.colorCode, line.colorDescription) || '-'} /></View>
                            <View style={styles.fieldHalf}><DataField title='Grid' data={CodeDescription(line.gridCode, line.gridDescription) || '-'} /></View>
                        </View>
                        <View style={styles.fieldsRow}>
                            <View style={styles.fieldHalf}><DataField title='Size' data={CodeDescription(line.sizeOrder, line.sizeDescription) || '-'} /></View>
                        </View>
                    </> :
                    <></>}

                {fields.showExpiry ?
                    <View style={styles.fieldsRow}>
                        <View style={styles.fieldHalf}><DataField title='Expiry' data={line.expiry || '-'} /></View>
                        <View style={styles.fieldHalf}><DataField title='Lot' data={line.batch || '-'} /></View>
                    </View> :
                    <></>}

                <View style={styles.fieldsRow}>
                    {fields.showAvailableQty ?
                        <View style={styles.fieldHalf}><DataField title='Avlb Qty' data={line && line.systemQty !== null ? FormatQuantity(line.systemQty) : '-'} /></View> :
                        <></>}
                    <View style={styles.fieldHalf}><DataField title='Total Counted' data={total !== null ? FormatQuantity(total) : '-'} toolTip={pending ? 'Waiting' : ''} /></View>
                </View>

            </View>

            {/* Always visible at the bottom */}
            <View style={styles.bottomContainer}>

                {/* Buttons mode : quick quantities, same look as the main buttons of the app */}
                {mode === MODES.BUTTONS ?
                    <View style={styles.quickContainer}>
                        {settings.quickButtons.map(value => (
                            <TouchableOpacity key={value} style={[styles.quickButton, { opacity: pending ? 1 : 0.4 }]} onPress={() => HandleQuickQty(value)}>
                                <Text style={styles.quickButtonText}>+{value}</Text>
                            </TouchableOpacity>
                        ))}
                        <TouchableOpacity style={[styles.quickButton, { opacity: pending ? 1 : 0.4 }]} onPress={() => {
                            if (!sessionRef.current.pending) { showResult('Scan an item first', true); return; }
                            const lineRecID = sessionRef.current.pending.lineRecID;
                            setNumberPad({
                                title: `Quantity of ${describeLine(lineRecID)}`,
                                preview: (value) => `Total will be ${FormatQuantity(Preview(sessionRef.current, lineRecID, value * 1000))}`,
                                onSubmit: (value) => {
                                    setNumberPad(null);
                                    // the number pad has no minus key, 0 is the only quantity it can reject (the scan keeps waiting)
                                    if (value === 0) { QtyRejected("Can't enter 0", scanRef); return; }
                                    HandleQuickQty(value);
                                },
                            });
                        }}>
                            <Text style={styles.quickButtonText}>Other</Text>
                        </TouchableOpacity>
                    </View> :
                    <></>}

                {/* Set total and undo on one row : blue and red like the Scan Items screen */}
                <View style={styles.actionsContainer}>
                    <TouchableOpacity style={styles.actionButton} onPress={HandleSetTotal}>
                        <Text style={styles.actionButtonText}>Set Total</Text>
                    </TouchableOpacity>
                    <TouchableOpacity style={[styles.actionButton, { backgroundColor: 'red', opacity: undoLabel === '' ? 0.5 : 1 }]} onPress={HandleUndo}>
                        <Text style={styles.actionButtonText} numberOfLines={1}>Undo Last Entry{undoLabel !== '' ? ` (${undoLabel})` : ''}</Text>
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
    // Mode bar : green like the sales order scanner
    optionsContainer: {
        backgroundColor: Colors.brandGreen,
        height: 44,
        paddingHorizontal: 5,
        flexDirection: 'row',
        alignItems: 'center',
        borderBottomWidth: 1,
        borderBottomColor: 'white',
    },
    modeButton: {
        height: 34,
        borderRadius: 15,
        backgroundColor: 'white',
        borderWidth: 1,
        borderColor: Colors.buttonBorderColor,
        alignItems: 'center',
        justifyContent: 'center',
        paddingHorizontal: 8,
    },
    modeButtonText: {
        color: Colors.brandBlue,
        fontSize: FontSize(16),
        fontWeight: 'bold',
    },
    // Entry bars : same green bar and white field as the Scan Items screen
    entryContainer: {
        backgroundColor: Colors.brandGreen,
        width: '100%',
        height: 55,
        padding: 5,
        flexDirection: 'row',
        alignItems: 'center',
    },
    scannerImage: {
        width: 40,
        height: '90%',
        marginRight: 5,
    },
    entryLabelText: {
        width: 45,
        color: 'white',
        fontSize: FontSize(18),
        fontWeight: 'bold',
        textAlign: 'center',
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
    },
    entryQtyText: {
        flex: 0,
        width: 110,
        textAlign: 'center',
        paddingLeft: 0,
        fontSize: FontSize(22),
    },
    entryInfoText: {
        flex: 1,
        marginLeft: 10,
        color: 'white',
        fontSize: FontSize(16),
        fontWeight: 'bold',
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
    noteContainer: {
        backgroundColor: Colors.listItemBackColor,
        paddingHorizontal: 10,
        paddingVertical: 3,
    },
    previewText: {
        color: Colors.brandGreen,
        fontSize: FontSize(16),
        fontWeight: 'bold',
        textAlign: 'center',
    },
    warningText: {
        color: 'red',
        fontSize: FontSize(15),
        fontWeight: 'bold',
        textAlign: 'center',
    },
    // Quick quantities : main button look (white, blue border and text)
    quickContainer: {
        flexDirection: 'row',
        flexWrap: 'wrap',
        justifyContent: 'space-between',
        paddingHorizontal: 10,
        paddingTop: 8,
    },
    quickButton: {
        width: '31%',
        height: 44,
        marginBottom: 6,
        borderRadius: 15,
        backgroundColor: Colors.buttonBackColor1,
        borderWidth: 1,
        borderColor: Colors.buttonBorderColor,
        alignItems: 'center',
        justifyContent: 'center',
    },
    quickButtonText: {
        color: Colors.buttonForeColor1,
        fontSize: FontSize(20),
        fontWeight: 'bold',
    },
    actionsContainer: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        paddingHorizontal: 10,
        paddingVertical: 8,
    },
    // Item and fields : fill the space left, never scroll (the bottom buttons stay visible)
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
    itemLineText: {
        marginLeft: 8,
        color: 'gray',
        fontSize: FontSize(13),
    },
    itemDescriptionText: {
        color: Colors.brandBlue,
        fontSize: FontSize(16),
    },
    itemTitleText: {
        color: Colors.brandGreen,
        fontSize: FontSize(15),
        fontStyle: 'italic',
    },
    fieldsRow: {
        flexDirection: 'row',
    },
    fieldHalf: {
        width: '50%',
    },
    bottomContainer: {
        borderTopWidth: 1,
        borderTopColor: 'silver',
        backgroundColor: Colors.bgColor,
    },
    actionButton: {
        width: '48.5%',
        height: 45,
        borderRadius: 15,
        backgroundColor: Colors.brandBlue,
        alignItems: 'center',
        justifyContent: 'center',
        paddingHorizontal: 5,
    },
    actionButtonText: {
        color: 'white',
        fontSize: FontSize(16),
    },
    // Modals : same card as the other modals of the app
    modalContainer: {
        width: '100%',
        height: '100%',
        backgroundColor: 'rgba(0,0,0,0.5)',
        alignItems: 'center',
        justifyContent: 'center',
    },
    modalCard: {
        width: Sizing.windowsWidth * 0.92,
        maxHeight: Sizing.windowsHeight * 0.85,
        backgroundColor: 'white',
        borderRadius: 20,
        overflow: 'hidden',
        elevation: 10,
        borderWidth: 5,
        borderColor: Colors.brandBlue,
    },
    modeRow: {
        flexDirection: 'row',
        alignItems: 'center',
        minHeight: 60,
        paddingHorizontal: 12,
        paddingVertical: 6,
        backgroundColor: Colors.listItemBackColor,
        marginBottom: 2,
    },
    modeRowTextContainer: {
        flex: 1,
    },
    modeRowTitleText: {
        color: Colors.brandBlue,
        fontSize: FontSize(18),
        fontWeight: 'bold',
    },
    modeRowDescriptionText: {
        color: 'gray',
        fontSize: FontSize(13),
        fontStyle: 'italic',
    },
    checkBoxImage: {
        width: 30,
        height: 30,
    },
    settingsContainer: {
        padding: 12,
    },
    settingTitleText: {
        fontSize: FontSize(14),
        color: Colors.brandBlue,
        marginTop: 4,
    },
    settingText: {
        height: 42,
        borderWidth: 1,
        borderColor: 'silver',
        borderRadius: 4,
        paddingLeft: 10,
        fontSize: FontSize(17),
        color: 'black',
        marginBottom: 8,
    },
});

export default PhysicalCountScanTestingScreen;
