import { useEffect, useState } from 'react';
import { TodayISO } from './ReceivingOrderRules';

// The receiving order being prepared, shared by its screens (header, items, line editor) until it is saved or cleared
// mode : ADD, MODIFY (new lines added to roRecID), CANCEL (roRecID is reversed)
// supplier : { supplierRecID, supplierCode, supplierName }, location : { locationCode, locationDescription }, po : { poRecID, poDate }
// lines : the new lines { lineID, itemRecID, itemReference, itemDescription, itemClass, itemUnit, itemQtyControl, supplierRef,
//         lineStatus, qty, lots: [{ lotID, expiry, batch, production, qty, barcodes }], barcodes, poQty }
// savedLines : the lines already saved (MODIFY, CANCEL, VIEW), read only
// Slots : RO the one being prepared, VIEW a receiving order opened to view (the one being prepared stays),
//         FREE the receiving order of Free Receiving (orderID : its number on the device once saved there)
// The functions take the slot, or true / false for VIEW / RO

const slots = { RO: null, VIEW: null, FREE: null };
const listeners = { RO: new Set(), VIEW: new Set(), FREE: new Set() };
let lastID = 0;

const slotOf = (slot) => slot === true ? 'VIEW' : (slot === false || slot === undefined || slot === null) ? 'RO' : slot;

const notify = (slot) => listeners[slot].forEach(listener => listener(slots[slot]));

export const NextID = () => ++lastID;

export const GetDraft = (slot = false) => slots[slotOf(slot)];

// slot : RO by default, a VIEW receiving order goes in the VIEW slot
export const StartDraft = (mode, receivingOrder = null, slot = 'RO') => {

    const savedLines = (receivingOrder?.lines ?? []).map(line => ({
        ...line,
        lineID: NextID(),
        lots: (line.lots ?? []).map(lot => ({ ...lot, lotID: NextID(), production: '' })),
    }));

    const started = {
        mode,
        roRecID: receivingOrder ? receivingOrder.roRecID.toString() : '',
        supplier: receivingOrder ? {
            supplierRecID: receivingOrder.supplierRecID, supplierCode: receivingOrder.supplierCode, supplierName: receivingOrder.supplierName,
        } : null,
        location: receivingOrder ? { locationCode: receivingOrder.locationCode, locationDescription: receivingOrder.locationDescription } : null,
        roRef: receivingOrder?.roRef ?? '',
        roStatus: receivingOrder?.roStatus ?? 'N',
        roDate: receivingOrder?.roDate ?? TodayISO(),
        // Modify keeps the actual date of the RO (the user can change it)
        roActualDate: receivingOrder?.roActualDate || receivingOrder?.roDate || TodayISO(),
        po: receivingOrder?.poRecID ? { poRecID: receivingOrder.poRecID, poDate: '' } : null,
        lines: [],
        savedLines,
    };
    const target = mode === 'VIEW' && slot === 'RO' ? 'VIEW' : slotOf(slot);
    slots[target] = started;
    notify(target);
    return started;
};

// A draft kept elsewhere (a receiving order saved on the device by Free Receiving) opened again : its ids are renewed
export const LoadDraft = (saved, slot) => {
    const target = slotOf(slot);
    const renew = (line) => ({ ...line, lineID: NextID(), lots: (line.lots ?? []).map(lot => ({ ...lot, lotID: NextID() })) });
    slots[target] = { ...saved, lines: (saved.lines ?? []).map(renew), savedLines: (saved.savedLines ?? []).map(renew) };
    notify(target);
    return slots[target];
};

export const ClearDraft = (slot = false) => {
    const target = slotOf(slot);
    slots[target] = null;
    notify(target);
};

export const UpdateDraft = (changes, slot = 'RO') => {
    const target = slotOf(slot);
    const current = slots[target];
    if (!current) return;
    slots[target] = { ...current, ...(typeof changes === 'function' ? changes(current) : changes) };
    notify(target);
};

export const SetLines = (change, slot = 'RO') => UpdateDraft(current => ({ lines: change(current.lines) }), slot);

// The draft of a slot in a component, refreshed on every change
export const useReceivingOrderDraft = (slot = false) => {
    const target = slotOf(slot);
    const [value, setValue] = useState(slots[target]);
    useEffect(() => {
        setValue(slots[target]);
        listeners[target].add(setValue);
        return () => listeners[target].delete(setValue);
    }, [target]);
    return value;
};
