import { SCAN_MAX_MS } from './PhysicalCountModes';
import { ParseTypedQuantity, FormatQuantity } from './PhysicalCountRules';
import { ItemRecIDFromLink } from './ItemLink';

// Free Receiving rules : search fields, receive types, quantities (whole thousandths like the physical count : 12.5 => 12500)

export const DEFAULT_PAGE_SIZE = 5000;
export const MAX_PAGE_SIZE = 5000;
export const PICK_LIMIT = 200;
export const LIST_LIMIT = 500;

export { FormatQuantity };

//#region Search fields

export const SEARCH_FIELDS = {
    BARCODE: 'barcode',
    SERIE: 'serie',
    SUPPLIER: 'supplier',
    CATEGORY: 'category',
    REFERENCE: 'reference',
    DESCRIPTION: 'description',
    SUPPLIER_REF: 'supplierRef',
};

export const SEARCH_FIELD_LABELS = {
    barcode: 'Barcode',
    serie: 'Serial No',
    supplier: 'Supplier Code',
    category: 'Category Code',
    reference: 'Reference',
    description: 'Description',
    supplierRef: 'Supplier Ref',
};

export const SEARCH_FIELD_DESCRIPTIONS = {
    barcode: 'Barcodes and single references 1 / 2',
    serie: 'Serial number of the item',
    supplier: 'Items of a supplier code',
    category: 'Items of a category code',
    reference: 'The reference, or the start of it',
    description: 'Any part of the description',
    supplierRef: 'References of the item at its suppliers',
};

// Search fields as options of the list picker
export const SEARCH_FIELD_OPTIONS = Object.values(SEARCH_FIELDS).map(field => ({
    code: field,
    description: `${SEARCH_FIELD_LABELS[field]}  ·  ${SEARCH_FIELD_DESCRIPTIONS[field]}`,
}));

// Remove check digit (settings) : a scanned barcode of digits loses its last digit
export const BarcodeToSearch = (value, removeCheckDigit) =>
    removeCheckDigit && /^[0-9]{2,}$/.test(value) ? value.slice(0, -1) : value;

// database : FreeReceivingDatabase
// Exact match first, then the start / part of the text : one item is picked, more open the list
// The link of a label QR (ItemLink) is the item itself, whatever the search field
// Returns { kind: 'found', itemRecID } | { kind: 'pick', field, value } | { kind: 'notFound', value } | { kind: 'empty' }
export const LookupScan = async (database, field, scannedText, removeCheckDigit) => {
    let value = (scannedText ?? '').trim();
    if (value === '') return { kind: 'empty' };

    const linkRecID = ItemRecIDFromLink(value);
    if (linkRecID !== null) {
        const linkItem = await database.GetItem(linkRecID);
        return linkItem ? { kind: 'found', itemRecID: linkItem.itemRecID } : { kind: 'notFound', value };
    }

    if (field === SEARCH_FIELDS.BARCODE) value = BarcodeToSearch(value, removeCheckDigit);

    const exact = await database.FindItems(field, value, true, 2);
    if (exact.length === 1) return { kind: 'found', itemRecID: exact[0].itemRecID };
    if (exact.length > 1) return { kind: 'pick', field, value, exact: true };

    const similar = await database.FindItems(field, value, false, 2);
    if (similar.length === 1) return { kind: 'found', itemRecID: similar[0].itemRecID };
    if (similar.length > 1) return { kind: 'pick', field, value, exact: false };

    return { kind: 'notFound', value };
};

//#endregion

//#region Receive types

export const RECEIVE_TYPES = {
    NORMAL: 'N',
    PROMOTION: 'P',
    FREE: 'F',
};

export const RECEIVE_TYPE_LABELS = {
    N: 'Normal',
    P: 'Promotion',
    F: 'Free',
};

export const RECEIVE_TYPE_COLORS = {
    N: 'rgb(8, 98, 170)',
    P: 'darkorange',
    F: 'rgb(64, 184, 112)',
};

//#endregion

//#region Quantity

// A typed quantity : digits with an optional decimal part, at most 9 whole digits (a longer number is a barcode)
const TYPED_QTY = /^[0-9]{1,9}([.,][0-9]*)?$/;

// The scanner typed into the quantity box : letters / symbols / long numbers, or typed at scanner speed
export const IsScannedQty = (text, firstCharTime, submitTime) => {
    const value = (text ?? '').trim();
    if (value === '') return false;
    if (!TYPED_QTY.test(value)) return true;
    return firstCharTime !== null && firstCharTime !== undefined && submitTime - firstCharTime < SCAN_MAX_MS;
};

// Returns { ok: true, value } (thousandths) or { ok: false, message }
export const ParseReceivedQty = (text, decimals) => {
    const result = ParseTypedQuantity(text, decimals);
    if (result.ok && result.value === 0) return { ok: false, message: "Can't enter 0" };
    return result;
};

//#endregion

//#region Settings checks

export const CheckPageSize = (text) => {
    const value = parseInt(text, 10);
    return /^[0-9]{1,4}$/.test((text ?? '').trim()) && value >= 1 && value <= MAX_PAGE_SIZE ? '' : `The download size is a whole number from 1 to ${MAX_PAGE_SIZE}`;
};

export const CheckInvoiceNumber = (text) => (text ?? '').trim().length <= 30 ? '' : 'The supplier invoice number is at most 30 characters';

export { CheckServer, CheckMachineName } from './PhysicalCountRules';

//#endregion
