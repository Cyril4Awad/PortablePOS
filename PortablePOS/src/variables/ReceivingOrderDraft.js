import { useEffect, useState } from 'react';
import { TodayISO } from './ReceivingOrderRules';

// The receiving order being prepared, shared by its screens (header, items, line editor) until it is saved or cleared
// mode : ADD, MODIFY (new lines added to roRecID), CANCEL (roRecID is reversed)
// supplier : { supplierRecID, supplierCode, supplierName }, location : { locationCode, locationDescription }, po : { poRecID, poDate }
// lines : the new lines { lineID, itemRecID, itemReference, itemDescription, itemClass, itemUnit, itemQtyControl, supplierRef,
//         lineStatus, qty, lots: [{ lotID, expiry, batch, production, qty, barcodes }], barcodes, poQty }
// savedLines : the lines already saved (MODIFY, CANCEL, VIEW), read only
// A receiving order opened to view (VIEW) is kept apart (view = true) : the one being prepared stays

let draft = null;
let viewed = null;
let lastID = 0;
const listeners = new Set();
const viewListeners = new Set();

const notify = () => listeners.forEach(listener => listener(draft));
const notifyView = () => viewListeners.forEach(listener => listener(viewed));

export const NextID = () => ++lastID;

export const GetDraft = (view = false) => view ? viewed : draft;

export const StartDraft = (mode, receivingOrder = null) => {

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
    if (mode === 'VIEW') {
        viewed = started;
        notifyView();
    }
    else {
        draft = started;
        notify();
    }
    return started;
};

export const ClearDraft = (view = false) => {
    if (view) {
        viewed = null;
        notifyView();
        return;
    }
    draft = null;
    notify();
};

export const UpdateDraft = (changes) => {
    if (!draft) return;
    draft = { ...draft, ...(typeof changes === 'function' ? changes(draft) : changes) };
    notify();
};

export const SetLines = (change) => UpdateDraft(current => ({ lines: change(current.lines) }));

// The draft (view : the receiving order viewed) in a component, refreshed on every change
export const useReceivingOrderDraft = (view = false) => {
    const [value, setValue] = useState(GetDraft(view));
    useEffect(() => {
        const set = view ? viewListeners : listeners;
        setValue(GetDraft(view));
        set.add(setValue);
        return () => set.delete(setValue);
    }, [view]);
    return value;
};
