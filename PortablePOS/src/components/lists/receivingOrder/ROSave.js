import React from 'react';
import { View, Text, StyleSheet, Modal, ActivityIndicator, Alert } from 'react-native';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';
import ReceivingOrderData from '../../../data/ReceivingOrderData';
import { ClearDraft } from '../../../variables/ReceivingOrderDraft';
import { LineIssue, LineQty, SignedQty, FormatQty, Plural, isLotItem, MODE_TITLES, DatesIssue } from '../../../variables/ReceivingOrderRules';

// What is missing before the save ('' when the receiving order can be saved)
export const DraftIssue = (draft) => {
    if (draft.mode === 'CANCEL') return '';
    if (draft.mode === 'ADD') {
        if (!draft.supplier) return 'Choose the supplier';
        if (!draft.location) return 'Choose the receiving location';
        if (draft.roRef.trim() === '') return 'Type the RO Ref. # (the delivery note of the supplier)';
    }
    const dateIssue = DatesIssue(draft.mode, draft.roDate, draft.roActualDate);
    if (dateIssue !== '') return dateIssue;
    if (draft.lines.length === 0) return draft.mode === 'ADD' ? 'Add the items received' : 'Add the new lines';
    const wrong = draft.lines.find(line => LineIssue(line) !== '');
    if (wrong) return `${wrong.itemReference} : ${LineIssue(wrong)}`;
    return '';
};

// The total qty that goes in (normal and free) less the returns
export const DraftTotalQty = (lines) => lines.reduce((total, line) => total + SignedQty(line), 0);

const MEAS_LABELS = { P: 'Pcs', W: 'Kg', L: 'M' };

const STATUS_TOTAL_LABELS = { N: 'Normal', F: 'Free', R: 'Return' };

// The totals of FrmDO (RefQtyLbl) : normal, free and return apart (a return is not taken out), each by the measure type
// of the sub unit (pieces, weight, length), in sub unit qty (DOD_ConsQty), e.g. 'Normal Pcs 625  ·  Return Pcs 600'
export const DraftMeasTotals = (lines) => {
    const totals = {};
    lines.forEach(line => {
        const status = STATUS_TOTAL_LABELS[line.lineStatus] ? line.lineStatus : 'N';
        const type = MEAS_LABELS[line.itemMeasType] ? line.itemMeasType : 'P';
        const factor = line.itemConsFactor === undefined || line.itemConsFactor === null ? 1 : Number(line.itemConsFactor);
        const key = status + type;
        totals[key] = (totals[key] ?? 0) + LineQty(line) * factor;
    });
    const shown = [];
    Object.keys(STATUS_TOTAL_LABELS).forEach(status => Object.keys(MEAS_LABELS).forEach(type => {
        const total = totals[status + type];
        if (total !== undefined) shown.push(`${STATUS_TOTAL_LABELS[status]} ${MEAS_LABELS[type]} ${FormatQty(Math.round(total * 1000) / 1000)}`);
    }));
    return shown.length === 0 ? 'Pcs 0' : shown.join('  ·  ');
};

const Payload = (draft) => ({
    roRecID: draft.roRecID,
    supplierRecID: draft.supplier ? draft.supplier.supplierRecID.toString() : '',
    roRef: draft.roRef.trim(),
    roDate: draft.roDate,
    roActualDate: draft.roActualDate,
    locationCode: draft.location ? draft.location.locationCode : '',
    poRecID: draft.po ? draft.po.poRecID.toString() : '',
    lines: draft.mode === 'CANCEL' ? [] : draft.lines.map(line => ({
        itemRecID: line.itemRecID.toString(),
        // an item of the pending stock file : itemRecID is its pending record id, the item is created on the save
        pending: line.isPending === true,
        supplierRef: line.supplierRef.trim(),
        lineStatus: line.lineStatus,
        qty: LineQty(line),
        barcodes: isLotItem(line.itemClass) ? [] : (line.barcodes ?? []),
        lots: isLotItem(line.itemClass) ? line.lots.map(lot => ({
            expiry: lot.expiry, batch: lot.batch.trim(), production: lot.production ?? '', qty: Number(lot.qty), barcodes: lot.barcodes ?? [],
        })) : [],
    })),
});

// Asks, saves, then the receiving order is cleared and onSaved(roRecID) is called
// setSaving(true / false) : shows the saving card (SavingModal)
export const ConfirmAndSave = async (draft, setSaving, onSaved) => {

    const issue = DraftIssue(draft);
    if (issue !== '') {
        alert(issue);
        return;
    }

    // A new RO cannot take a ref already used (the save checks it again)
    if (draft.mode === 'ADD') {
        try {
            if (await ReceivingOrderData.RefExists(draft.roRef.trim())) {
                Alert.alert('RO Ref. #', `The RO Ref. # ${draft.roRef.trim()} is already used by another receiving order, change it.`);
                return;
            }
        }
        catch (err) {
            Alert.alert('Not Saved', err.message);
            return;
        }
    }

    const lines = draft.mode === 'CANCEL' ? draft.savedLines : draft.lines;
    const question = draft.mode === 'CANCEL'
        ? `Receiving order #${draft.roRecID} will be cancelled : a reverse receiving order takes its ${Plural(lines.length, 'line')} (${DraftMeasTotals(lines)}) out of the stock of ${draft.location?.locationCode}.`
        : `${draft.supplier?.supplierName}\n${Plural(lines.length, draft.mode === 'MODIFY' ? 'new line' : 'line')}, ${DraftMeasTotals(lines)} in ${draft.location?.locationCode}.`;

    Alert.alert(draft.mode === 'CANCEL' ? 'Cancel Receiving Order' : MODE_TITLES[draft.mode], question, [
        { text: 'Not Now', style: 'cancel' },
        {
            text: draft.mode === 'CANCEL' ? 'Cancel RO' : 'Save', style: draft.mode === 'CANCEL' ? 'destructive' : 'default',
            onPress: () => {
                setSaving(true);
                ReceivingOrderData.Save(draft.mode, Payload(draft))
                    .then(roRecID => {
                        setSaving(false);
                        ClearDraft();
                        const done = draft.mode === 'CANCEL'
                            ? `Receiving order #${draft.roRecID} cancelled, reverse RO # ${roRecID}`
                            : draft.mode === 'MODIFY' ? `Receiving order #${roRecID} modified` : `Receiving order saved, RO # ${roRecID}`;
                        Alert.alert('Saved', done, [{ text: 'OK', onPress: () => onSaved(roRecID) }]);
                    })
                    .catch(err => {
                        setSaving(false);
                        Alert.alert('Not Saved', err.message);
                    });
            },
        },
    ]);
};

export const SavingModal = ({ visible }) => (
    <Modal visible={visible} transparent={true} presentationStyle="overFullScreen" onRequestClose={() => { }}>
        <View style={styles.modalContainer}>
            <View style={styles.modalCard}>
                <ActivityIndicator size='large' color={Colors.brandBlue} />
                <Text style={styles.text}>Saving The Receiving Order...</Text>
            </View>
        </View>
    </Modal>
);

const styles = StyleSheet.create({
    modalContainer: {
        width: '100%',
        height: '100%',
        backgroundColor: 'rgba(0,0,0,0.5)',
        alignItems: 'center',
        justifyContent: 'center',
    },
    modalCard: {
        width: Sizing.windowsWidth * 0.8,
        backgroundColor: 'white',
        borderRadius: 20,
        borderWidth: 5,
        borderColor: Colors.brandBlue,
        alignItems: 'center',
        padding: 25,
    },
    text: {
        marginTop: 15,
        color: Colors.brandBlue,
        fontSize: FontSize(17),
        fontWeight: 'bold',
    },
});
