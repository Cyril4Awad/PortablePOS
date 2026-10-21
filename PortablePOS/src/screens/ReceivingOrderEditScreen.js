import React, { useState, useEffect } from 'react';
import { View, Text, TextInput, StyleSheet, TouchableOpacity, Alert } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import ChoiceField, { choiceRowStyle, choiceTitleTextStyle } from '../components/controls/ChoiceField';
import MainButton from '../components/controls/MainButton';
import PhysicalCountFilterPicker from '../components/lists/physicalCount/PhysicalCountFilterPicker';
import ROSupplierPicker from '../components/lists/receivingOrder/ROSupplierPicker';
import ROChoiceModal from '../components/lists/receivingOrder/ROChoiceModal';
import RODateModal from '../components/lists/receivingOrder/RODateModal';
import { ConfirmAndSave, SavingModal, DraftTotalQty } from '../components/lists/receivingOrder/ROSave';
import ReceivingOrderData from '../data/ReceivingOrderData';
import { useReceivingOrderDraft, UpdateDraft, ClearDraft, NextID } from '../variables/ReceivingOrderDraft';
import { MODE_TITLES, FormatISODate, FormatQty, Plural, LineIssue, isLotItem, TodayISO } from '../variables/ReceivingOrderRules';

const locationStorageKey = '@app_roLocation';

// The header of the receiving order : supplier, location, RO ref, dates, purchase order (new RO), then the items
// Modify : only the actual date changes, Cancel : read only
const ReceivingOrderEditScreen = ({ navigation }) => {

    const draft = useReceivingOrderDraft();

    const [supplierPickerVisible, setSupplierPickerVisible] = useState(false);
    const [locations, setLocations] = useState([]);
    const [locationPickerVisible, setLocationPickerVisible] = useState(false);
    const [dateField, setDateField] = useState(null);
    const [purchaseOrders, setPurchaseOrders] = useState(null);
    const [busy, setBusy] = useState('');
    const [saving, setSaving] = useState(false);
    const [refWarning, setRefWarning] = useState('');

    const isNew = draft?.mode === 'ADD';
    const isCancel = draft?.mode === 'CANCEL';
    const isView = draft?.mode === 'VIEW';
    // the saved lines are the ones shown (view, cancel)
    const readOnly = isCancel || isView;

    useEffect(() => {
        if (!draft) return;
        navigation.setOptions({ title: MODE_TITLES[draft.mode] + (draft.roRecID ? ` #${draft.roRecID}` : '') });
        // A new RO takes the location used last
        if (isNew && !draft.location) {
            AsyncStorage.getItem(locationStorageKey)
                .then(saved => { if (saved) UpdateDraft(current => current.location ? {} : { location: JSON.parse(saved) }); })
                .catch(() => { });
        }
    }, [draft?.mode]);

    // The draft is cleared (saved or cleared) : back to the receiving order menu
    if (!draft) {
        return (
            <View style={styles.emptyContainer}>
                <Text style={styles.emptyText}>No receiving order is being prepared</Text>
                <MainButton title='Back' imageSource={require('../assets/icons/close.png')} onPress={() => navigation.navigate('ReceivingOrder')} />
            </View>
        );
    }

    const shownLines = readOnly ? draft.savedLines : draft.lines;
    const missing = draft.lines.filter(line => LineIssue(line) !== '').length;

    //#region Supplier, location, dates

    const PickSupplier = () => {
        if (!isNew) return;
        if (draft.lines.length > 0) {
            alert('The supplier cannot change once the receiving order has items, remove them first');
            return;
        }
        setSupplierPickerVisible(true);
    };

    const PickLocation = () => {
        if (!isNew) return;
        if (locations.length > 0) {
            setLocationPickerVisible(true);
            return;
        }
        setBusy('Loading the locations...');
        ReceivingOrderData.GetLocations()
            .then(rows => { setLocations(rows); setLocationPickerVisible(true); })
            .catch(err => alert(err.message))
            .finally(() => setBusy(''));
    };

    const CheckRef = () => {
        const reference = draft.roRef.trim();
        if (reference === '') { setRefWarning(''); return; }
        ReceivingOrderData.RefExists(reference)
            .then(exists => setRefWarning(exists ? 'A receiving order already has this ref' : ''))
            .catch(() => setRefWarning(''));
    };

    //#endregion

    //#region Purchase order

    const PickPurchaseOrder = () => {
        if (!isNew) return;
        if (!draft.supplier) {
            alert('Choose the supplier first');
            return;
        }
        setBusy('Loading the purchase orders...');
        ReceivingOrderData.GetPurchaseOrders(draft.supplier.supplierRecID, draft.roDate)
            .then(rows => {
                if (rows.length === 0) alert('There is no purchase order to receive for this supplier');
                else setPurchaseOrders(rows);
            })
            .catch(err => alert(err.message))
            .finally(() => setBusy(''));
    };

    // The lines of the PO replace the lines imported from the previous PO, the items scanned stay
    const ImportPurchaseOrder = (po) => {

        if (po.poAfterRO === 1) { alert('Cannot import a PO with a date after the RO date'); return; }
        if (po.poLocked === 1) { alert(`PO #${po.poRecID} is opened in the ERP, try again later`); return; }
        setPurchaseOrders(null);

        const run = () => {
            setBusy('Importing the purchase order...');
            ReceivingOrderData.GetPurchaseOrderLines(draft.supplier.supplierRecID, po.poRecID, draft.roDate)
                .then(result => {
                    const blocked = result.lines.filter(line => line.blockMessage !== '');
                    const lines = result.lines.filter(line => line.blockMessage === '').map(line => ({
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
                        lines: [...current.lines.filter(line => line.poQty === undefined), ...lines],
                    }));
                    const notes = [];
                    if (lines.some(line => isLotItem(line.itemClass))) notes.push('Type the expiry dates and lots of the expiry items.');
                    if (blocked.length > 0) notes.push(`${Plural(blocked.length, 'line')} cannot be received (${blocked[0].itemReference} : ${blocked[0].blockMessage}).`);
                    if (result.skipped > 0) notes.push(`${Plural(result.skipped, 'line')} left out (items you have no privilege for).`);
                    Alert.alert(`PO #${po.poRecID}`, `${Plural(lines.length, 'line')} imported with the qty still to receive.${notes.length > 0 ? '\n\n' + notes.join('\n') : ''}`);
                })
                .catch(err => alert(err.message))
                .finally(() => setBusy(''));
        };

        const fromPO = draft.lines.filter(line => line.poQty !== undefined).length;
        if (fromPO > 0) {
            Alert.alert('Change The PO', `The ${Plural(fromPO, 'line')} of PO #${draft.po?.poRecID} will be replaced by the lines of PO #${po.poRecID}.`, [
                { text: 'Cancel', style: 'cancel' },
                { text: 'Replace', onPress: run },
            ]);
        }
        else run();
    };

    const RemovePurchaseOrder = () => {
        if (!isNew || !draft.po) return;
        const fromPO = draft.lines.filter(line => line.poQty !== undefined).length;
        Alert.alert('Remove The PO', `PO #${draft.po.poRecID} and its ${Plural(fromPO, 'line')} will be removed from the receiving order.`, [
            { text: 'Cancel', style: 'cancel' },
            { text: 'Remove', style: 'destructive', onPress: () => UpdateDraft(current => ({ po: null, lines: current.lines.filter(line => line.poQty === undefined) })) },
        ]);
    };

    //#endregion

    const Clear = () => {
        Alert.alert('Clear', `The ${MODE_TITLES[draft.mode].toLowerCase()} and its ${Plural(draft.lines.length, 'new line')} will be cleared.`, [
            { text: 'Cancel', style: 'cancel' },
            { text: 'Clear', style: 'destructive', onPress: () => { ClearDraft(); navigation.navigate('ReceivingOrder'); } },
        ]);
    };

    // The items need the supplier (its references) and the location (its stock)
    const GoToItems = () => {
        if (!draft.supplier) { alert('Choose the supplier first'); return; }
        if (!draft.location) { alert('Choose the receiving location first'); return; }
        navigation.navigate('ReceivingOrderItems');
    };

    // RO date : not after today, the actual date follows it when it was before ; actual date : not before the RO date
    const PickDate = (date) => {
        const field = dateField;
        setDateField(null);
        if (field === 'roDate') {
            if (date > TodayISO()) { Alert.alert('RO Date', "The RO date should be equal or less than today's date"); return; }
            UpdateDraft(current => ({ roDate: date, roActualDate: current.roActualDate < date ? date : current.roActualDate }));
            return;
        }
        if (date < draft.roDate) {
            Alert.alert('Actual Date', `The actual date should be greater or equal than the RO date (${FormatISODate(draft.roDate)})`);
            return;
        }
        UpdateDraft({ roActualDate: date });
    };

    return (
        <View style={styles.container}>

            <SavingModal visible={saving} />

            <ROSupplierPicker
                visible={supplierPickerVisible}
                selectedRecID={draft.supplier?.supplierRecID}
                onPick={supplier => {
                    setSupplierPickerVisible(false);
                    UpdateDraft(current => ({ supplier, po: current.supplier?.supplierRecID === supplier.supplierRecID ? current.po : null }));
                }}
                onClose={() => setSupplierPickerVisible(false)}
            />

            <PhysicalCountFilterPicker
                visible={locationPickerVisible}
                title='Receiving Location'
                options={locations.map(row => ({ code: row.locationCode, description: `${row.locationCode} - ${row.locationDescription}` }))}
                selected={draft.location ? [draft.location.locationCode] : []}
                multiple={false}
                onSubmit={codes => {
                    setLocationPickerVisible(false);
                    const location = locations.find(row => row.locationCode === codes[0]);
                    if (!location) return;
                    UpdateDraft({ location });
                    AsyncStorage.setItem(locationStorageKey, JSON.stringify(location)).catch(() => { });
                }}
                onCancel={() => setLocationPickerVisible(false)}
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

            {busy !== '' ?
                <View style={styles.busyContainer}>
                    <Text style={styles.busyText}>{busy}</Text>
                </View> :
                <></>}

            {/* Everything fits on the screen without scrolling */}
            <View style={styles.content}>

                <View style={[styles.modeContainer, isCancel ? styles.modeCancel : draft.mode === 'MODIFY' || isView ? styles.modeModify : null]}>
                    <Text style={styles.modeText} numberOfLines={1}>
                        {isView ? (draft.roStatus === 'R' ? 'Reverse receiving order (cancel), view only' : 'View only, nothing can change') : isCancel ? 'The whole receiving order will be reversed' : draft.mode === 'MODIFY' ? 'The new lines are added, the saved lines stay' : 'New receiving order'}
                    </Text>
                </View>

                <ChoiceField
                    title='Supplier'
                    value={draft.supplier ? `${draft.supplier.supplierCode} - ${draft.supplier.supplierName}` : 'Choose The Supplier'}
                    valueStyle={!draft.supplier ? styles.missingValue : null}
                    onPress={isNew ? PickSupplier : undefined}
                    style={{ borderRightWidth: 0 }}
                />

                <ChoiceField
                    title='Receiving Location'
                    value={draft.location ? `${draft.location.locationCode} - ${draft.location.locationDescription}` : 'Choose The Location'}
                    valueStyle={!draft.location ? styles.missingValue : null}
                    onPress={isNew ? PickLocation : undefined}
                    style={{ borderRightWidth: 0 }}
                />

                <View style={styles.refField}>
                    <View style={styles.refTitleRow}>
                        <Text style={choiceTitleTextStyle}>RO Ref. #</Text>
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
                </View>

                {/* Lines | total qty | missing */}
                <TouchableOpacity style={styles.linesRow} onPress={GoToItems}>
                    <View style={styles.linesCell}>
                        <Text style={styles.linesNumberText}>{shownLines.length}</Text>
                        <Text style={styles.linesLabelText}>{readOnly ? 'Lines' : draft.mode === 'MODIFY' ? `New Lines (${draft.savedLines.length} saved)` : 'Lines'}</Text>
                    </View>
                    <View style={[styles.linesCell, styles.linesCellSeparator]}>
                        <Text style={styles.linesNumberText}>{FormatQty(DraftTotalQty(shownLines))}</Text>
                        <Text style={styles.linesLabelText}>Total Qty</Text>
                    </View>
                    <View style={[styles.linesCell, styles.linesCellSeparator]}>
                        <Text style={[styles.linesNumberText, { color: missing > 0 ? 'red' : Colors.brandGreen }]}>{missing}</Text>
                        <Text style={styles.linesLabelText}>To Complete</Text>
                    </View>
                </TouchableOpacity>

                <View style={styles.buttonsContainer}>

                    <MainButton title={readOnly ? 'View Items' : 'Items'} imageSource={require('../assets/icons/scanItems.png')}
                        onPress={GoToItems} />

                    {isView ?
                        <MainButton title='Close' imageSource={require('../assets/icons/close.png')}
                            onPress={() => { ClearDraft(); navigation.navigate('ReceivingOrder'); }} /> :
                        <>
                            <MainButton title={isCancel ? 'Cancel RO' : 'Save'} imageSource={isCancel ? require('../assets/icons/ReturnOrders.png') : require('../assets/icons/upload.png')}
                                onPress={() => ConfirmAndSave(draft, setSaving, () => navigation.navigate('ReceivingOrder'))} />

                            <MainButton title='Clear' imageSource={require('../assets/icons/deleteLine.png')} onPress={Clear} />
                        </>}

                </View>

            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.bgColor,
        height: '100%',
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
    busyContainer: {
        backgroundColor: Colors.brandBlue,
        height: 30,
        alignItems: 'center',
        justifyContent: 'center',
    },
    busyText: {
        color: 'white',
        fontSize: FontSize(15),
        fontWeight: 'bold',
    },
    modeContainer: {
        backgroundColor: Colors.brandGreen,
        height: 30,
        alignItems: 'center',
        justifyContent: 'center',
        paddingHorizontal: 10,
    },
    modeModify: {
        backgroundColor: Colors.brandBlue,
    },
    modeCancel: {
        backgroundColor: 'rgb(200, 60, 60)',
    },
    modeText: {
        color: 'white',
        fontSize: FontSize(15),
        fontWeight: 'bold',
    },
    missingValue: {
        color: 'red',
    },
    refField: {
        height: 58,
        paddingLeft: 10,
        paddingRight: 8,
        borderColor: 'silver',
        borderBottomWidth: 1,
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
        marginLeft: 8,
    },
    refText: {
        height: 34,
        color: 'black',
        fontSize: FontSize(17),
        paddingVertical: 0,
        paddingLeft: 0,
    },
    fieldHalf: {
        width: '50%',
    },
    content: {
        flex: 1,
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
    linesRow: {
        flexDirection: 'row',
        height: 56,
        borderColor: 'silver',
        borderBottomWidth: 1,
    },
    linesCell: {
        flex: 1,
        alignItems: 'center',
        justifyContent: 'center',
        paddingHorizontal: 4,
    },
    linesCellSeparator: {
        borderLeftWidth: 1,
        borderLeftColor: 'silver',
    },
    linesNumberText: {
        color: 'black',
        fontSize: FontSize(19),
        fontWeight: 'bold',
    },
    linesLabelText: {
        color: Colors.brandBlue,
        fontSize: FontSize(13),
    },
    buttonsContainer: {
        flexDirection: 'row',
        flexWrap: 'wrap',
        alignItems: 'center',
        justifyContent: Sizing.isTablet ? 'flex-start' : 'space-between',
        width: '100%',
        padding: 10,
    },
});

export default ReceivingOrderEditScreen;
