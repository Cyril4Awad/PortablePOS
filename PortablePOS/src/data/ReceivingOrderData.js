import ServerInfo from "../configuration/ServerInfo";
import { getLoginUserCode } from "../variables/LoginInfo";
import { callService } from "./ServiceCall";

// Receiving Order works online : every call goes to the server of the login screen (FrmDO rules are checked again on the save)
// Dates : yyyy-MM-dd, expiry and production dates : DD.MM.YYYY (00 for the day = the whole month)

const timeoutMs = 20000;
const saveTimeoutMs = 120000;

const call = async (method, body, timeout = timeoutMs) => {
    const serverIP = await ServerInfo.serverIP();
    if (!serverIP) throw new Error('Please define the server IP');
    return callService(serverIP, method, body, timeout, 'The server is not reachable, check the connection and try again');
};

const parse = (result, empty) => result === '' ? empty : JSON.parse(result);

// [{ supplierRecID, supplierCode, supplierName, supplierForeign, supplierCountry, supplierCurrency }]
const GetSuppliers = async () => parse(await call('GetROSuppliers', { userCode: getLoginUserCode() }), []);

// [{ locationCode, locationDescription }]
const GetLocations = async () => parse(await call('GetROLocations', {}), []);

// A code scanned or typed : [{ item..., supplierRef, codeExpiry, codeBatch, blockMessage }], [] when nothing is found
const FindItem = async (supplierRecID, code, roDate) =>
    parse(await call('FindROItem', { userCode: getLoginUserCode(), supplierRecID: supplierRecID.toString(), code, roDate }), []);

// The stock search of the receiving order (any part of the reference, barcode, description) : [{ itemRecID, itemReference, itemDescription, ... }]
const SearchItems = async (supplierRecID, searchString) =>
    parse(await call('GetItems', {
        itemProductGroup: '', itemPromotionCriteria: '', searchString,
        searchFor: 'RO', userCode: getLoginUserCode(), supplierRecID: supplierRecID.toString(),
    }), []);

// The items of the pending stock file the user can receive (created in the stock file when the RO is saved) : [{ item..., isPending: 1 }]
const GetPendingItems = async () => parse(await call('GetROPendingItems', { userCode: getLoginUserCode() }), []);

// { lots: [{ lotExpiry, lotBatch, lotAvailable }] (stock of the location), knownLots: [{ lotExpiry, lotBatch, lotProduction }] }
const GetItemLots = async (itemRecID, locationCode) =>
    parse(await call('GetROItemLots', { itemRecID: itemRecID.toString(), locationCode }), { lots: [], knownLots: [] });

// 'New' (added to the item on save) or 'Existing', throws when it is the barcode of another item
const CheckBarcode = async (itemRecID, barcode) => call('CheckROBarcode', { itemRecID: itemRecID.toString(), barcode });

// A new barcode from the counter of the system parameters, with its check digit
const GenerateBarcode = async () => call('GenerateROBarcode', {});

// true when a receiving order already has this ref (a warning only)
const RefExists = async (roRef) => (await call('CheckRORef', { roRef })) === 'Exists';

// [{ poRecID, poDate, poManualNo, poDeliveryDate, poTotal, poCurrency, poRemark, poAfterRO, poLocked }]
const GetPurchaseOrders = async (supplierRecID, roDate) =>
    parse(await call('GetROPurchaseOrders', { userCode: getLoginUserCode(), supplierRecID: supplierRecID.toString(), roDate }), []);

// { lines: [{ poLineNo, supplierRef, lineStatus, remainingQty, unitCost, item... }], skipped }
const GetPurchaseOrderLines = async (supplierRecID, poRecID, roDate) =>
    parse(await call('GetROPurchaseOrderLines', {
        userCode: getLoginUserCode(), supplierRecID: supplierRecID.toString(), poRecID: poRecID.toString(), roDate,
    }), { lines: [], skipped: 0 });

// forChange : only the ones that can be modified or cancelled
const GetReceivingOrders = async (forChange, searchString) =>
    parse(await call('GetReceivingOrders', { userCode: getLoginUserCode(), forChange: forChange ? 'Y' : 'N', searchString }), []);

// { header..., lines: [{ lineRecID, item..., supplierRef, lineStatus, qty, costCC, lots: [{ expiry, batch, qty, barcodes }], barcodes }] }
const GetReceivingOrder = async (roRecID, forChange) =>
    parse(await call('GetReceivingOrder', { userCode: getLoginUserCode(), roRecID: roRecID.toString(), forChange: forChange ? 'Y' : 'N' }), null);

// saveMode : ADD, MODIFY (the new lines only), CANCEL (no line) - returns the RO # saved
const Save = async (saveMode, receivingOrder) => {
    const result = await call('SaveReceivingOrder', {
        userCode: getLoginUserCode(), saveMode, receivingOrder: JSON.stringify(receivingOrder),
    }, saveTimeoutMs);
    return result.split('|')[1];
};

export default {
    GetSuppliers,
    GetLocations,
    FindItem,
    SearchItems,
    GetPendingItems,
    GetItemLots,
    CheckBarcode,
    GenerateBarcode,
    RefExists,
    GetPurchaseOrders,
    GetPurchaseOrderLines,
    GetReceivingOrders,
    GetReceivingOrder,
    Save,
};
