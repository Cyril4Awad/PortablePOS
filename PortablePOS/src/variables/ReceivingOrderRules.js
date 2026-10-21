// Receiving Order rules (FrmDO "Receiving Order"), checked on the device to tell the user at once,
// the server checks them all again on the save

export const STATUS_LABELS = { N: 'Normal', F: 'Free', R: 'Return' };
export const STATUS_ORDER = ['N', 'F', 'R'];

export const MODE_TITLES = { ADD: 'New Receiving Order', MODIFY: 'Modify Receiving Order', CANCEL: 'Cancel Receiving Order', VIEW: 'View Receiving Order' };

// Read only : nothing can change (view), or the whole RO is reversed (cancel)
export const isReadOnlyMode = (mode) => mode === 'VIEW' || mode === 'CANCEL';

export const VAT_LABELS = { T: 'Taxable', N: 'Not Taxable', E: 'Taxable Embedded' };

export const VatLabel = (vatType) => VAT_LABELS[(vatType ?? '').toUpperCase()] ?? (vatType || '-');

// The cost of a line : the PO cost of a line imported from a PO, else the cost of the item (the saved cost of a saved line)
export const LineCost = (line) => line.unitCost ?? line.costCC ?? line.itemCostCC ?? 0;

// A cost with its 3 decimals
export const FormatCost = (value) => Number(value ?? 0).toFixed(3);

export const isLotItem = (itemClass) => (itemClass ?? '').toUpperCase() === 'E';

//#region Dates

const twoDigits = (value) => value.toString().padStart(2, '0');

// yyyy-MM-dd (the dates sent to the server)
export const ToISODate = (date) => `${date.getFullYear()}-${twoDigits(date.getMonth() + 1)}-${twoDigits(date.getDate())}`;

export const TodayISO = () => ToISODate(new Date());

export const ISOToDate = (iso) => {
    const parts = (iso ?? '').split('-');
    if (parts.length !== 3) return null;
    return new Date(Number(parts[0]), Number(parts[1]) - 1, Number(parts[2]));
};

// yyyy-MM-dd => dd/MM/yyyy
export const FormatISODate = (iso) => {
    const parts = (iso ?? '').split('-');
    return parts.length === 3 ? `${parts[2]}/${parts[1]}/${parts[0]}` : '';
};

// The dates of the header (txtDate_Validating of FrmDO) : the RO date not after today (new RO), the actual date not before the RO date
export const DatesIssue = (mode, roDate, roActualDate) => {
    if (mode === 'ADD' && roDate > TodayISO()) return 'The RO date should be equal or less than today\'s date';
    if (mode !== 'CANCEL' && roActualDate < roDate) return `The actual date (${FormatISODate(roActualDate)}) should be greater or equal than the RO date (${FormatISODate(roDate)})`;
    return '';
};

// What is typed for an expiry or production date, with the dots put while typing : 25 => 25, 2507 => 25.07, 25072027 => 25.07.2027
export const MaskLotDate = (text) => {
    const digits = (text ?? '').replace(/\D/g, '').slice(0, 8);
    if (digits.length <= 2) return digits;
    if (digits.length <= 4) return `${digits.slice(0, 2)}.${digits.slice(2)}`;
    return `${digits.slice(0, 2)}.${digits.slice(2, 4)}.${digits.slice(4)}`;
};

// DD.MM.YYYY with 00 for the day (the whole month) : the last day the date covers, null when it is not a date
export const LotDateLastDay = (text) => {
    const match = /^(\d{2})\.(\d{2})\.(\d{4})$/.exec(text ?? '');
    if (!match) return null;
    const day = Number(match[1]);
    const month = Number(match[2]);
    const year = Number(match[3]);
    if (month < 1 || month > 12 || year < 1800) return null;
    const daysInMonth = new Date(year, month, 0).getDate();
    if (day === 0) return new Date(year, month - 1, daysInMonth);
    if (day > daysInMonth) return null;
    return new Date(year, month - 1, day);
};

// What the user types for an expiry or production date, to DD.MM.YYYY :
// 25072027, 25.07.2027, 25/07/2027, 25-7-2027 (the day and month on 1 or 2 digits), 072027 or 07/2027 (the whole month : day 00)
export const NormalizeLotDate = (text) => {
    const value = (text ?? '').trim();
    if (value === '') return '';
    if (/^\d{8}$/.test(value)) return `${value.slice(0, 2)}.${value.slice(2, 4)}.${value.slice(4)}`;
    if (/^\d{6}$/.test(value)) return `00.${value.slice(0, 2)}.${value.slice(2)}`;
    const parts = value.split(/[./\-\s]+/);
    if (parts.length === 3 && parts.every(part => /^\d+$/.test(part)) && parts[2].length === 4) {
        return `${parts[0].padStart(2, '0')}.${parts[1].padStart(2, '0')}.${parts[2]}`;
    }
    if (parts.length === 2 && parts.every(part => /^\d+$/.test(part)) && parts[1].length === 4) {
        return `00.${parts[0].padStart(2, '0')}.${parts[1]}`;
    }
    return value;
};

// The expiry of a lot received on roDate (yyyy-MM-dd) : a valid date, not expired (LstExpiry_BeforeColUpdate of FrmDO)
export const CheckExpiry = (expiry, roDate) => {
    const lastDay = LotDateLastDay(expiry);
    if (!lastDay) return { ok: false, message: 'Invalid expiry date, type it as DD.MM.YYYY (00.MM.YYYY for the whole month)' };
    if (lastDay < ISOToDate(roDate)) return { ok: false, message: `Item expired ! The expiry date ${expiry} is before the RO date ${FormatISODate(roDate)}` };
    return { ok: true };
};

// The production date (optional) : a valid date, not after roDate
export const CheckProduction = (production, roDate) => {
    if ((production ?? '') === '') return { ok: true };
    const lastDay = LotDateLastDay(production);
    if (!lastDay) return { ok: false, message: 'Invalid production date, type it as DD.MM.YYYY' };
    if (lastDay > ISOToDate(roDate)) return { ok: false, message: 'The production date cannot be after the receiving order date' };
    return { ok: true };
};

//#endregion

//#region Quantities

// A typed qty : more than 0, 3 decimals at most
export const ParseQty = (text, allowZero = false) => {
    const value = (text ?? '').toString().trim().replace(',', '.');
    if (value === '') return allowZero ? { ok: true, value: 0 } : { ok: false, message: 'Type the qty' };
    if (!/^\d{1,10}(\.\d{0,3})?$/.test(value)) return { ok: false, message: 'Invalid qty (3 decimals at most)' };
    const number = Number(value);
    if (!allowZero && number <= 0) return { ok: false, message: 'The qty must be more than 0' };
    return { ok: true, value: number };
};

export const FormatQty = (value) => {
    const number = Number(value ?? 0);
    return Number.isInteger(number) ? number.toString() : number.toFixed(3).replace(/0+$/, '').replace(/\.$/, '');
};

// The qty of a line : the total of its lots for an expiry item
export const LineQty = (line) => isLotItem(line.itemClass)
    ? line.lots.reduce((total, lot) => total + Number(lot.qty || 0), 0)
    : Number(line.qty || 0);

// What is missing for the line to be saved ('' when nothing)
export const LineIssue = (line) => {
    if ((line.supplierRef ?? '').trim() === '') return 'Supplier ref missing';
    if (isLotItem(line.itemClass) && line.lots.length === 0) return 'Add the expiry / lots';
    if (LineQty(line) <= 0) return 'Qty missing';
    return '';
};

// N and F add to the stock, R takes out
export const SignedQty = (line) => (line.lineStatus === 'R' ? -1 : 1) * LineQty(line);

//#endregion

//#region Barcodes

// EAN-13 / UPC-A check digit (CheckEAN13 / CheckUPCA of the ERP) : a warning only, other barcodes are not checked
export const BarcodeCheckDigitOk = (barcode) => {
    const value = (barcode ?? '').trim();
    if (!/^\d+$/.test(value) || (value.length !== 13 && value.length !== 12)) return true;
    const digits = value.split('').map(Number);
    const body = digits.slice(0, -1);
    const total = body.reduce((sum, digit, index) => {
        // EAN-13 : x1, x3 from the left ; UPC-A : x3, x1 from the left
        const weight = value.length === 13 ? (index % 2 === 0 ? 1 : 3) : (index % 2 === 0 ? 3 : 1);
        return sum + digit * weight;
    }, 0);
    return (10 - total % 10) % 10 === digits[digits.length - 1];
};

// Every barcode of a line (the ones of its lots for an expiry item)
export const LineBarcodes = (line) => isLotItem(line.itemClass)
    ? line.lots.reduce((all, lot) => all.concat(lot.barcodes ?? []), [])
    : (line.barcodes ?? []);

//#endregion

export const Plural = (count, word) => `${count} ${word}${count === 1 ? '' : 's'}`;
