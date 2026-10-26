import { ParseTypedQuantity, FormatQuantity, CodeDescription } from './PhysicalCountRules';

// Transfer rules : lots, quantities (whole thousandths like the physical count : 12.5 => 12500)

export const LIST_LIMIT = 500;
export const ONE = 1000;

export { FormatQuantity, CodeDescription };

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

// The lot rows of the server for one item : { item, lots } (lots : the rows with a lot, none when its available qty is 0)
export const ItemOfRows = (rows) => ({
    item: rows[0],
    lots: rows.filter(row => row.lotRecID !== ''),
});

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

// New qty of an existing line : checked with the available qty of its lot (read last) and the qty of the other lines of the lot
// database : TransferDatabase - Returns { ok: true, value } or { ok: false, message }
export const CheckLineQty = async (database, line, qty) => {
    const transferQty = await database.GetLotTransferQty(line.lotRecID);
    return CheckTransferQty(qty, line.lotAvailable, transferQty - line.qty);
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

export { CheckServer, CheckMachineName } from './PhysicalCountRules';

//#endregion
