import { ParseTypedQuantity, FormatQuantity, CodeDescription } from './PhysicalCountRules';
import { ItemRecIDFromLink } from './ItemLink';

// Transfer rules : search fields, lots, quantities (whole thousandths like the physical count : 12.5 => 12500)

export const DEFAULT_PAGE_SIZE = 5000;
export const MAX_PAGE_SIZE = 5000;
export const PICK_LIMIT = 200;
export const LIST_LIMIT = 500;
export const ONE = 1000;

export { FormatQuantity, CodeDescription };

//#region Search fields

export const SEARCH_FIELDS = {
    BARCODE: 'barcode',
    REFERENCE: 'reference',
};

export const SEARCH_FIELD_LABELS = {
    barcode: 'Barcode',
    reference: 'Reference',
};

// database : TransferDatabase
// barcode : the whole barcode, reference : the whole reference, else its start (several items open the list)
// The link of a label QR (ItemLink) is the item itself, whatever the search field
// Returns { kind: 'found', itemRecID } | { kind: 'pick', value } | { kind: 'notFound', value } | { kind: 'empty' }
export const LookupScan = async (database, field, scannedText) => {
    const value = (scannedText ?? '').trim();
    if (value === '') return { kind: 'empty' };

    const linkRecID = ItemRecIDFromLink(value);
    if (linkRecID !== null) {
        const linkItem = await database.GetItem(linkRecID);
        return linkItem ? { kind: 'found', itemRecID: linkItem.itemRecID } : { kind: 'notFound', value };
    }

    if (field === SEARCH_FIELDS.BARCODE) {
        const byBarcode = await database.FindItemsByBarcode(value);
        if (byBarcode.length === 1) return { kind: 'found', itemRecID: byBarcode[0].itemRecID };
        if (byBarcode.length > 1) return { kind: 'pick', value };
        return { kind: 'notFound', value };
    }

    const exact = await database.FindItemsByReference(value, true, 2);
    if (exact.length === 1) return { kind: 'found', itemRecID: exact[0].itemRecID };

    const byStart = await database.FindItemsByReference(value, false, 2);
    if (byStart.length === 1) return { kind: 'found', itemRecID: byStart[0].itemRecID };
    if (byStart.length > 1) return { kind: 'pick', value };

    return { kind: 'notFound', value };
};

//#endregion

//#region Lots

// Expiry / lot and color / grid / size of a lot, '' when it has none
export const LotText = (lot) => {
    const parts = [];
    if ((lot.lotExpiry ?? '') !== '') parts.push(`Exp ${lot.lotExpiry}`);
    if ((lot.lotBatch ?? '') !== '') parts.push(`Lot ${lot.lotBatch}`);
    const fashion = [lot.lotColor, lot.lotGrid, lot.lotSize].filter(text => (text ?? '') !== '').join(' / ');
    if (fashion !== '') parts.push(fashion);
    return parts.join('   ');
};

//#endregion

//#region Quantity

// The qty to transfer of a lot : more than 0 and, with the qty of the other lines of the lot, not more than its available qty
// available / otherQty : thousandths - Returns { ok: true, value } or { ok: false, message }
export const CheckTransferQty = (qty, available, otherQty) => {
    if (available <= 0) return { ok: false, message: 'The available qty is 0' };
    if (qty <= 0) return { ok: false, message: "Can't enter 0" };
    const left = available - otherQty;
    if (qty > left) {
        return { ok: false, message: left <= 0 ? `All the available qty (${FormatQuantity(available)}) is already transferred` : `Only ${FormatQuantity(left)} available` };
    }
    return { ok: true, value: qty };
};

// New qty of an existing line : checked with the qty of the other lines of its lot
// database : TransferDatabase - Returns { ok: true, value } or { ok: false, message }
export const CheckLineQty = async (database, line, qty) => {
    const lot = await database.GetLot(line.lotRecID);
    // the lot is not in the last download any more : the qty can only go down
    if (!lot) return qty > 0 && qty <= line.qty ? { ok: true, value: qty } : { ok: false, message: 'The lot has no available qty any more' };
    return CheckTransferQty(qty, lot.lotAvailable, lot.transferQty - line.qty);
};

// Typed qty, '' or 0 allowed when allowZero (a destination left empty in multiple locations)
export const ParseTransferQty = (text, decimals, allowZero = false) => {
    if (allowZero && (text ?? '').trim() === '') return { ok: true, value: 0 };
    const result = ParseTypedQuantity(text, decimals);
    if (result.ok && result.value === 0 && !allowZero) return { ok: false, message: "Can't enter 0" };
    return result;
};

//#endregion

//#region Settings checks

export const CheckPageSize = (text) => {
    const value = parseInt(text, 10);
    return /^[0-9]{1,4}$/.test((text ?? '').trim()) && value >= 1 && value <= MAX_PAGE_SIZE ? '' : `The download size is a whole number from 1 to ${MAX_PAGE_SIZE}`;
};

export { CheckServer, CheckMachineName } from './PhysicalCountRules';

//#endregion
