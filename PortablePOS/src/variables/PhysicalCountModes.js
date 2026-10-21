import { FormatQuantity } from './PhysicalCountRules';

// Counting modes of the physical count (Scan For Testing screen)
// Quantities are whole thousandths like the rest of the physical count (1 => 1000)
// Quantities are added, only Set Total replaces. One level of undo.

const ONE = 1000;

export const MODES = {
    PLUS_ONE: 'plusOne',
    SCAN_THEN_TYPE: 'scanThenType',
    TYPE_THEN_SCAN: 'typeThenScan',
    BUTTONS: 'buttons',
};

export const MODE_LABELS = {
    plusOne: '+1',
    scanThenType: 'Scan, then type',
    typeThenScan: 'Type, then scan',
    buttons: 'Buttons',
};

export const MODE_DESCRIPTIONS = {
    plusOne: 'Each scan adds 1',
    scanThenType: 'After each scan, type the quantity (1 is selected)',
    typeThenScan: 'Type the quantity first, then scan the item',
    buttons: 'After each scan, tap a quantity button',
};

export const MAX_QUICK_BUTTONS = 5;     // + the Other button = 6 buttons

export const DEFAULT_SETTINGS = {
    quickButtons: [1, 5, 10, 12, 24],
    sameItemWarningMs: 1000,
};

// A scanner types all its characters and Enter within a few milliseconds, a person cannot press a key and Enter that fast
export const SCAN_MAX_MS = 80;

// Letters, symbols or more than 9 digits => a barcode, otherwise a typed quantity
export const ClassifyFormat = (text) => {
    const value = (text ?? '').trim();
    if (value === '') return 'empty';
    return /^[0-9]{1,9}$/.test(value) ? 'quantity' : 'barcode';
};

// Input of a quantity box : a scan of the next item (scanner speed, or letters / symbols / more than 9 digits)
// or a typed quantity. firstCharTime : time of the first character, submitTime : time of Enter
export const IsScannedInput = (text, firstCharTime, submitTime, scanMaxMs = SCAN_MAX_MS) => {
    if (ClassifyFormat(text) === 'barcode') return true;
    return firstCharTime !== null && firstCharTime !== undefined && submitTime - firstCharTime < scanMaxMs;
};

// A typed quantity that cannot be counted : negative or 0 (otherwise '')
// Checked before the scan detection so "-5" is not searched as a barcode
export const QtyError = (text) => {
    const value = (text ?? '').trim();
    if (/^-\s*[0-9]+([.,][0-9]*)?$/.test(value)) return "Can't enter a negative amount";
    if (/^0+([.,]0*)?$/.test(value)) return "Can't enter 0";
    return '';
};

// Typed quantity (digits only) in thousandths
export const QtyFromText = (text) => parseInt(text.trim(), 10) * ONE;

//#region Session

// counts : { [lineRecID]: total } of the lines scanned in this screen (thousandths)
export const CreateSession = () => ({
    counts: {},
    pending: null,      // { lineRecID, qty } : scanned, quantity not confirmed yet (scan then type, buttons)
    lastEntry: null,    // { lineRecID, before, beforeState, qty, kind: 'add' | 'set' } : for undo and for the +1 quantity
    nextQty: ONE,       // type then scan : quantity of the next scan
    lastScan: null,     // { lineRecID, time } : same item warning
});

// The total and state of a line read from the database when it is scanned
export const KnowLine = (session, lineRecID, counted, state) => ({
    ...session,
    counts: { ...session.counts, [lineRecID]: counted },
    states: { ...(session.states ?? {}), [lineRecID]: state },
});

const totalOf = (session, lineRecID) => session.counts[lineRecID] ?? 0;
const stateOf = (session, lineRecID) => (session.states ?? {})[lineRecID] ?? 0;

// Every function returns { session, message, save } : save = { lineRecID, counted } to write to the database

export const AddEntry = (session, lineRecID, qty) => {
    const before = totalOf(session, lineRecID);
    const total = before + qty;
    return {
        session: {
            ...session,
            counts: { ...session.counts, [lineRecID]: total },
            states: { ...(session.states ?? {}), [lineRecID]: 1 },
            lastEntry: { lineRecID, before, beforeState: stateOf(session, lineRecID), qty, kind: 'add' },
        },
        message: `Added ${FormatQuantity(qty)} · total ${FormatQuantity(total)}`,
        save: { lineRecID, counted: total },
    };
};

// Set Total : replaces the total of the line
export const SetTotal = (session, lineRecID, total) => {
    const before = totalOf(session, lineRecID);
    return {
        session: {
            ...session,
            pending: null,
            counts: { ...session.counts, [lineRecID]: total },
            states: { ...(session.states ?? {}), [lineRecID]: 1 },
            lastEntry: { lineRecID, before, beforeState: stateOf(session, lineRecID), qty: total, kind: 'set' },
        },
        message: `Total set to ${FormatQuantity(total)} (was ${FormatQuantity(before)})`,
        save: { lineRecID, counted: total },
    };
};

// Scan then type / buttons : the scan waits for its quantity (default 1)
export const StartPending = (session, lineRecID) => ({
    session: { ...session, pending: { lineRecID, qty: ONE } },
    message: `Quantity? Total will be ${FormatQuantity(totalOf(session, lineRecID) + ONE)}`,
    save: null,
});

export const ConfirmPending = (session, qty) => {
    if (!session.pending) return { session, message: '', save: null };
    return AddEntry({ ...session, pending: null }, session.pending.lineRecID, qty ?? session.pending.qty);
};

// Type then scan : quantity of the next scan, back to 1 after it
export const SetNextQty = (session, qty) => ({ ...session, nextQty: qty });

export const ScanWithNextQty = (session, lineRecID) => {
    const result = AddEntry(session, lineRecID, session.nextQty);
    return { ...result, session: { ...result.session, nextQty: ONE } };
};

// One level : cancels the pending scan, or puts back the total (and state) before the last entry
// restore = { lineRecID, counted, state } to write to the database
export const Undo = (session, describeLine = (id) => id) => {
    if (session.pending) {
        return {
            session: { ...session, pending: null },
            message: `Undo : cancelled the scan of ${describeLine(session.pending.lineRecID)}`,
            restore: null,
        };
    }
    const last = session.lastEntry;
    if (!last) return { session, message: 'Nothing to undo', restore: null };
    const current = totalOf(session, last.lineRecID);
    return {
        session: {
            ...session,
            counts: { ...session.counts, [last.lineRecID]: last.before },
            states: { ...(session.states ?? {}), [last.lineRecID]: last.beforeState },
            lastEntry: null,
        },
        message: `Undo : ${UndoLabel(session)} on ${describeLine(last.lineRecID)} (total ${FormatQuantity(current)} → ${FormatQuantity(last.before)})`,
        restore: { lineRecID: last.lineRecID, counted: last.before, state: last.beforeState },
    };
};

// What the Undo button will undo : "+15", "= 7" (set total), "scan" (waiting scan) or ''
export const UndoLabel = (session) => {
    if (session.pending) return 'scan';
    const last = session.lastEntry;
    if (!last) return '';
    return last.kind === 'set' ? `= ${FormatQuantity(last.qty)}` : `+${FormatQuantity(last.qty)}`;
};

// "Total will be N" before a typed quantity is confirmed
export const Preview = (session, lineRecID, qty) => totalOf(session, lineRecID) + qty;

// Same code twice within the window : warning only
export const IsSameItemAgain = (session, lineRecID, time, windowMs = DEFAULT_SETTINGS.sameItemWarningMs) =>
    session.lastScan !== null && session.lastScan.lineRecID === lineRecID && time - session.lastScan.time <= windowMs;

export const RememberScan = (session, lineRecID, time) => ({ ...session, lastScan: { lineRecID, time } });

//#endregion
