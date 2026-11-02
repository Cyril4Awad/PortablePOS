import FreeReceivingData from '../../../data/FreeReceivingData';
import FreeReceivingSource from '../../../data/FreeReceivingSource';
import FreeReceivingDatabase from '../../../database/FreeReceivingDatabase';
import { ClearDraft } from '../../../variables/ReceivingOrderDraft';
import { LineQty, isLotItem, FormatQty } from '../../../variables/ReceivingOrderRules';
import { DraftIssue, DraftMeasTotals, Payload } from '../receivingOrder/ROSave';
import MessageBox from '../../MessageBox';

// The save of the receiving order of Free Receiving : like the Receiving Order (ConfirmAndSave), but it is kept on the device first
// (RC_Orders), then uploaded at once when the server answers, otherwise later with the Upload button of Free Receiving

// The lines in rows for View Items
const LinesOf = (draft) => draft.lines.map(line => ({
    itemRecID: line.itemRecID.toString(),
    itemReference: line.itemReference,
    itemDescription: line.itemDescription,
    itemUnit: line.itemUnit,
    lineStatus: line.lineStatus,
    supplierRef: line.supplierRef,
    qty: LineQty(line),
    lotsText: isLotItem(line.itemClass) ? line.lots.map(lot => `${lot.expiry} / ${lot.batch} : ${FormatQty(lot.qty)}`).join('  ·  ') : '',
}));

export const OrderName = (order) => `${order.supplierCode} - RO Ref # ${order.roRef}`;

// Uploads receiving orders saved on the device (FreeReceivingDatabase.GetOrder) in one call : the server saves all of them or none,
// they are removed from the device only when all are saved. Returns [{ roRecID, already }] in the same order
export const UploadOrders = async (orders) => {
    const results = await FreeReceivingData.SaveFreeReceiving(orders.map(order => ({ orderName: OrderName(order), receivingOrder: Payload(order.draft) })));
    if (results.length !== orders.length) throw new Error(`The server saved ${results.length} of ${orders.length} receiving orders, upload again`);
    await FreeReceivingDatabase.DeleteUploadedOrders(orders.map(order => order.orderID));
    return results;
};

// Asks, keeps it on the device, uploads it when online, then the draft is cleared and onSaved() is called
// setSaving(true / false) : shows the saving card (SavingModal)
export const FreeSave = async (draft, setSaving, onSaved) => {

    const issue = DraftIssue(draft);
    if (issue !== '') {
        alert(issue);
        return;
    }

    // A RO Ref # already used : by another receiving order of the device, or on the server (the upload checks it again)
    const reference = draft.roRef.trim();
    const deviceOrder = await FreeReceivingDatabase.OrderOfRef(reference, draft.orderID);
    if (deviceOrder) {
        MessageBox.alert('RO Ref. #', `The RO Ref. # ${reference} is already used by another receiving order of this device (${deviceOrder.supplierCode}${deviceOrder.roRecID ? `, RO # ${deviceOrder.roRecID}` : ''}), change it.`);
        return;
    }
    try {
        if (await FreeReceivingSource.RefExists(reference)) {
            MessageBox.alert('RO Ref. #', `The RO Ref. # ${reference} is already used by another receiving order, change it.`);
            return;
        }
    }
    catch (err) {
        MessageBox.alert('Not Saved', err.message);
        return;
    }

    const question = `${draft.supplier?.supplierName}\n${draft.lines.length} item(s), ${DraftMeasTotals(draft.lines)} in ${draft.location?.locationCode}.`;

    MessageBox.alert('Free Receiving', question, [
        { text: 'Not Now', style: 'cancel' },
        {
            text: 'Save', onPress: async () => {
                setSaving(true);
                let orderID;
                try {
                    orderID = await FreeReceivingDatabase.SaveOrder(draft.orderID ?? null, { ...draft, mode: 'ADD' }, LinesOf(draft));
                }
                catch (err) {
                    setSaving(false);
                    MessageBox.alert('Not Saved', err.message);
                    return;
                }
                ClearDraft('FREE');

                // Online : uploaded at once, offline (or the upload failed) : uploaded later
                let done = 'Saved on the device (offline), upload it from Free Receiving once online.';
                if (FreeReceivingSource.isOnline()) {
                    try {
                        const [result] = await UploadOrders([await FreeReceivingDatabase.GetOrder(orderID)]);
                        done = `Receiving order saved, RO # ${result.roRecID}`;
                    }
                    catch (err) {
                        done = `Saved on the device, not uploaded : ${err.message}\nUpload it from Free Receiving.`;
                    }
                }
                setSaving(false);
                MessageBox.alert('Saved', done, [{ text: 'OK', onPress: () => onSaved() }]);
            },
        },
    ]);
};
