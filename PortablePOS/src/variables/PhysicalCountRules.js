// Physical count rules, same behaviour as the old ERPDataCollector app
// Quantities are whole thousandths (milli) : 12.5 => 12500

import { ItemRecIDFromLink } from './ItemLink';
import { getLoginUserCode } from './LoginInfo';

const SCALE = 1000;
const MAX_DECIMALS = 3;
export const MAX_QUANTITY = 999999999999999; // 999,999,999,999.999
export const DEFAULT_PAGE_SIZE = 5000;
export const MAX_PAGE_SIZE = 5000;
// The whole upload is sent in one call and saved in one transaction on the server (all or nothing)

export const ToMilli = (value) => Math.round(Number(value || 0) * SCALE);

// Display without trailing zeros : 12, 12.5, 0.125
export const FormatQuantity = (milli) => {
    const negative = milli < 0;
    const abs = Math.abs(milli);
    const whole = Math.floor(abs / SCALE);
    const fraction = (abs % SCALE).toString().padStart(3, '0').replace(/0+$/, '');
    return `${negative ? '-' : ''}${whole}${fraction ? '.' + fraction : ''}`;
};

// Sent to the server : plain decimal with a dot
export const FormatForUpload = (milli) => FormatQuantity(milli);

// Allowed decimals from SP_DecNo : its value capped at 3, anything unreadable means whole numbers
export const AllowedDecimals = (decimals) => {
    const value = parseInt(decimals, 10);
    return isNaN(value) || value < 0 ? 0 : Math.min(value, MAX_DECIMALS);
};

// Quantity typed by the user ('.' or ',' as decimal separator)
// Returns { ok: true, value } or { ok: false, message }
export const ParseTypedQuantity = (text, decimals) => {
    const value = (text ?? '').toString().trim();
    if (value === '') return { ok: false, message: 'Enter a quantity' };
    if (value.startsWith('-')) return { ok: false, message: 'The quantity cannot be negative' };

    const match = /^([0-9]*)(?:[.,]([0-9]*))?$/.exec(value);
    if (!match || !/[0-9]/.test(value)) return { ok: false, message: 'Not a number' };

    const whole = match[1] || '0';
    const fraction = (match[2] ?? '').replace(/0+$/, '');
    if (fraction.length > decimals) return { ok: false, message: decimals === 0 ? 'Whole numbers only' : `At most ${decimals} decimals` };
    if (whole.replace(/^0+/, '').length > 12) return { ok: false, message: 'The quantity is too large' };

    return { ok: true, value: parseInt(whole, 10) * SCALE + parseInt((fraction + '000').slice(0, 3), 10) };
};

const CheckTotal = (total) => {
    if (total < 0) return { ok: false, message: 'The total cannot be negative' };
    if (total > MAX_QUANTITY) return { ok: false, message: 'The total is too large' };
    return { ok: true, value: total };
};

//#region Count rules (the 3 buttons work from the fields on screen, like the old app)

// Scan (or pick from the list) : stored total + 1, Last Scanned = 1
export const ScanCount = (storedMilli) => {
    const total = CheckTotal(storedMilli + SCALE);
    if (!total.ok) return total;
    return { ok: true, total: total.value, lastScanned: SCALE, lastScanEnabled: true };
};

// Update Last Scan Qty : Total + (Last Scanned - 1), then the last scan buttons lock
export const UpdateLastScanQty = (totalText, lastScannedText, decimals) => {
    const total = ParseTypedQuantity(totalText, decimals);
    if (!total.ok) return { ok: false, message: 'Total : ' + total.message };
    const last = ParseTypedQuantity(lastScannedText, decimals);
    if (!last.ok) return { ok: false, message: 'Last Scanned : ' + last.message };
    const result = CheckTotal(total.value + (last.value - SCALE));
    if (!result.ok) return result;
    return { ok: true, total: result.value, lastScanned: last.value, lastScanEnabled: false };
};

// Cancel Last Scan : Total - Last Scanned, then the last scan buttons lock
export const CancelLastScan = (totalText, lastScannedText, decimals) => {
    const total = ParseTypedQuantity(totalText, decimals);
    if (!total.ok) return { ok: false, message: 'Total : ' + total.message };
    const last = ParseTypedQuantity(lastScannedText, decimals);
    if (!last.ok) return { ok: false, message: 'Last Scanned : ' + last.message };
    const result = CheckTotal(total.value - last.value);
    if (!result.ok) return result;
    return { ok: true, total: result.value, lastScanned: last.value, lastScanEnabled: false };
};

// Update Total Scanned : the total as typed, the last scan buttons keep their state
export const UpdateTotalScanned = (totalText, decimals) => {
    const total = ParseTypedQuantity(totalText, decimals);
    if (!total.ok) return { ok: false, message: 'Total : ' + total.message };
    return { ok: true, total: total.value };
};

//#endregion

//#region Fields shown for a line

// The modes come from stk04_SysParam (downloaded with the count header), the fields depend on the scanned item :
// - Expiry / Lot : when SP_Expiry = Y and the item has them (a trading item without expiry and lot shows none)
// - Color / Grid / Size : when SP_Fashion = Y and the item has them
// - Title : when SP_Book = Y and the item is a book (Bibl1_Book.BO_Title)
// - Avlb Qty : only with the privilege (otherwise the server does not send it)
export const LineFields = (header, line) => {

    const hasExpiry = line !== null && ((line.expiry ?? '') !== '' || (line.batch ?? '') !== '');
    const hasFashion = line !== null && ((line.colorCode ?? '') !== '' || (line.gridCode ?? '') !== '' || (line.sizeOrder ?? '') !== '');
    const hasBook = line !== null && (line.bookTitle ?? '') !== '';

    return {
        showExpiry: header?.expiryMode === 'Y' && hasExpiry,
        showFashion: header?.fashionMode === 'Y' && hasFashion,
        showBook: header?.bookMode === 'Y' && hasBook,
        showAvailableQty: NegationAllowed(header, 'showAvailableQty'),
    };
};

// Negation of the downloaded count (showAvailableQty, showListDifferences) : only for the user it was read for,
// another user logged in offline sees neither (they are read again for him when the server answers)
export const NegationAllowed = (header, negation) =>
    header?.[negation] === 'Y' && (header?.negationUser ?? '').toUpperCase() === getLoginUserCode().toUpperCase();

// Code - Description, or only one of them when the other is empty
export const CodeDescription = (code, description) => [code, description].filter(text => (text ?? '') !== '').join(' - ');

//#endregion

//#region Scan lookup : exact barcode -> reference prefix -> QR

// Same as the Java split of the old app : trailing empty parts are removed
const SplitParts = (text, separator) => {
    const parts = text.split(separator);
    while (parts.length > 0 && parts[parts.length - 1] === '') parts.pop();
    return parts;
};

// QR sticker : itemRecID|DD/MM/YY|Batch => expiry DD.MM.YY (MM.YY when the day is 00)
export const ParseQr = (value) => {
    if (!value.includes('|')) return { kind: 'notQr' };

    const parts = SplitParts(value, '|');
    if (parts.length < 3) return { kind: 'invalidQr' };

    const dateParts = SplitParts(parts[1], '/');
    if (dateParts.length < 3 || dateParts[0] === '' || dateParts[1] === '') return { kind: 'invalidQr' };

    const [day, month, year] = dateParts;
    return {
        kind: 'qr',
        itemRecID: parts[0],
        expiry: day === '00' ? `${month}.${year}` : `${day}.${month}.${year}`,
        batch: parts[2],
    };
};

// database : PhysicalCountDatabase, countKey : { server, location } of the downloaded count
// Returns { kind: 'found', lineRecID } | { kind: 'pick', value, byBarcode } | { kind: 'invalidQr' } | { kind: 'notFound' } | { kind: 'empty' }
// pick : the barcode belongs to more than one line (byBarcode), or the text starts more than one reference
// The link of a label QR (ItemLink) : the line of the item, the list of its lines (by its reference) when it has several (expiry / batch)
export const LookupScan = async (database, countKey, scannedText) => {
    const value = (scannedText ?? '').trim();
    if (value === '') return { kind: 'empty' };

    const linkRecID = ItemRecIDFromLink(value);
    if (linkRecID !== null) {
        const byItem = await database.FindLinesByItem(countKey, linkRecID, 2);
        if (byItem.length === 1) return { kind: 'found', lineRecID: byItem[0].lineRecID };
        if (byItem.length > 1) return { kind: 'pick', value: byItem[0].itemReference, byBarcode: false };
        return { kind: 'notFound' };
    }

    const byBarcode = await database.FindLinesByBarcode(countKey, value, 2);
    if (byBarcode.length === 1) return { kind: 'found', lineRecID: byBarcode[0].lineRecID };
    if (byBarcode.length > 1) return { kind: 'pick', value, byBarcode: true };

    const byReference = await database.FindLinesByReferencePrefix(countKey, value, 2);
    if (byReference.length === 1) return { kind: 'found', lineRecID: byReference[0].lineRecID };
    if (byReference.length > 1) return { kind: 'pick', value, byBarcode: false };

    const qr = ParseQr(value);
    if (qr.kind === 'invalidQr') return { kind: 'invalidQr' };
    if (qr.kind === 'qr') {
        const byQr = await database.FindLineByQr(countKey, qr.itemRecID, qr.expiry, qr.batch);
        if (byQr !== null) return { kind: 'found', lineRecID: byQr };
    }

    return { kind: 'notFound' };
};

//#endregion

//#region Settings checks

export const CheckLocation = (text) => /^[A-Za-z0-9]{1,2}$/.test(text) ? '' : 'The location code is 1 or 2 letters or digits';

export const CheckMachineName = (text) => /^[A-Za-z0-9]{1,5}$/.test(text) ? '' : 'The machine name is 1 to 5 letters or digits';

// host or host:port without a path, optionally after http:// or https:// (https://assets.example.com for the devices on GSM)
export const CheckServer = (text) => {
    if (text === '') return 'Enter the server address';
    const host = text.replace(/^https?:\/\//i, '');
    if (/^[a-z][a-z0-9+.-]*:\/\//i.test(host)) return 'The server starts with http:// or https://, or with the address (for example 0.0.0.0 or server:8080)';
    if (!/^[A-Za-z0-9.-]{1,253}(:[0-9]{1,5})?$/.test(host)) return 'The server may only contain letters, digits, dots, dashes and an optional :port';
    return '';
};

export const CheckCountNumber = (text) => /^[0-9]{1,18}$/.test((text ?? '').trim()) ? '' : 'The physical count number is digits only';

export const CheckPageSize = (text) => {
    const value = parseInt(text, 10);
    return /^[0-9]{1,4}$/.test((text ?? '').trim()) && value >= 1 && value <= MAX_PAGE_SIZE ? '' : `The download page size is a whole number from 1 to ${MAX_PAGE_SIZE}`;
};

//#endregion
