import ReceivingOrderData from './ReceivingOrderData';
import FreeReceivingDatabase from '../database/FreeReceivingDatabase';
import { LastIsOnline } from '../variables/ConnectionStatus';
import { ItemRecIDFromLink } from '../variables/ItemLink';
import { ISOToDate } from '../variables/ReceivingOrderRules';

// The data of the Receiving Order screens for Free Receiving : the same calls as ReceivingOrderData,
// online the server (the stock of the server), offline the stock file downloaded on the device (FreeReceivingDatabase)
// A call that cannot reach the server is answered by the device too

const OFFLINE_ONLY = 'needs the server, it is not available offline';

const isConnectionError = (error) => /not reachable|network request failed|no response|taking too long|define the server/i.test(error?.message ?? '');

// online() first when the server answers, offline() otherwise
const either = async (online, offline) => {
    if (LastIsOnline() !== false) {
        try {
            return await online();
        }
        catch (error) {
            if (!isConnectionError(error)) throw error;
        }
    }
    return offline();
};

const isOnline = () => LastIsOnline() !== false;

const DownloadedInfo = async () => {
    const info = await FreeReceivingDatabase.GetInfo();
    if (!info || info.complete !== 1) throw new Error('Offline : download the stock file of Free Receiving first');
    return info;
};

// Why an item cannot be received on roDate (ROItemBlock) : halted activity, BBF date after the RO date
const BlockMessage = (item, roDate) => {
    if (Number(item.itemHalted) === 1) return "Item's Activity Is Halted!";
    const bbf = ISOToDate(item.itemBBFDate);
    const date = ISOToDate(roDate);
    if (bbf && date && bbf > date) {
        const [year, month, day] = item.itemBBFDate.split('-');
        return `This Item Has A BBF Date > than Receiving Order's Date. BBF Date : ${day}/${month}/${year}`;
    }
    return '';
};

// An item of the stock file as the server sends it (ROItemJson) : supplierRef = the one given, else its single reference 1, else its reference
const ItemOf = (row, supplierRef, roDate, currencyCC) => ({
    itemRecID: row.itemRecID,
    itemReference: row.itemReference,
    itemSingleRef1: row.itemSingleRef1 ?? '',
    itemBarcode: row.itemSingleRef2 ?? '',
    itemDescription: row.itemDescription,
    itemClass: row.itemClass,
    itemUnit: row.itemUnit,
    itemSubUnit: row.itemSubUnit ?? '',
    itemVatType: row.itemVatType ?? '',
    itemCostCC: Number(row.itemCostCC) || 0,
    itemCurrency: currencyCC,
    itemQtyControl: row.itemQtyControl ?? 'N',
    itemMeasType: row.itemMeasType || 'P',
    itemConsFactor: row.itemConsFactor === null || row.itemConsFactor === undefined ? 1 : Number(row.itemConsFactor),
    supplierRef: (supplierRef || row.itemSingleRef1 || row.itemReference || '').trim(),
    codeExpiry: row.codeExpiry ?? '',
    codeBatch: row.codeBatch ?? '',
    isPending: 0,
    blockMessage: BlockMessage(row, roDate),
});

const offline = {

    // [{ supplierRecID, supplierCode, supplierName, supplierForeign, supplierCountry, supplierCurrency }] : all the suppliers downloaded
    GetSuppliers: async () => {
        await DownloadedInfo();
        const rows = await FreeReceivingDatabase.GetSuppliers();
        return rows.map(row => ({ ...row, supplierForeign: 0, supplierCountry: '', supplierCurrency: '' }));
    },

    // The location of the stock file
    GetLocations: async () => {
        const info = await DownloadedInfo();
        return [{ locationCode: info.location, locationDescription: info.locationDescription }];
    },

    // The items of a code (FindROItem) : a label QR, the reference, single references, a barcode (with its expiry / lot), a supplier reference of the supplier
    FindItem: async (supplierRecID, code, roDate) => {
        const info = await DownloadedInfo();
        const linkRecID = ItemRecIDFromLink(code);
        const rows = linkRecID !== null
            ? [await FreeReceivingDatabase.GetItem(linkRecID)].filter(row => row)
            : await FreeReceivingDatabase.FindItemsByCode(supplierRecID.toString(), code);
        const result = [];
        for (const row of rows) {
            // the code itself when it is a supplier reference, else the first supplier reference of the item for this supplier
            const supplierRef = row.codeOrder === 0 ? code.trim() : await FreeReceivingDatabase.GetItemSupplierRef(row.itemRecID, supplierRecID.toString());
            result.push(ItemOf(row, supplierRef, roDate, info.currencyCC));
        }
        return result;
    },

    // The stock search : any part of the reference, description, single references or a code
    SearchItems: async (supplierRecID, searchString) => {
        await DownloadedInfo();
        return FreeReceivingDatabase.SearchItems(searchString, 200);
    },

    GetPendingItems: async () => { throw new Error(`The pending stock items ${OFFLINE_ONLY}`); },

    GetItemLots: async (itemRecID, locationCode) => FreeReceivingDatabase.GetItemLots(itemRecID.toString(), locationCode),

    // 'New' or 'Existing', the barcode of another item is refused (checked again on the server by the upload)
    CheckBarcode: async (itemRecID, barcode) => {
        const owner = await FreeReceivingDatabase.GetBarcodeItem(barcode);
        if (owner && owner.toString() !== itemRecID.toString()) throw new Error(`The Barcode ${barcode} Already Exist For Another Item.`);
        return owner ? 'Existing' : 'New';
    },

    GenerateBarcode: async () => { throw new Error(`Generating a barcode ${OFFLINE_ONLY}`); },

    RefExists: async () => false,

    GetPurchaseOrders: async () => { throw new Error(`Importing a purchase order ${OFFLINE_ONLY}`); },

    GetPurchaseOrderLines: async () => { throw new Error(`Importing a purchase order ${OFFLINE_ONLY}`); },
};

// Online : only the items sent to the POS are received (the stock file downloaded has only these)
const SentToPOS = (items) => items.map(item => item.itemSendToPOS === 'N' && !item.blockMessage
    ? { ...item, blockMessage: 'The Item Is Not Sent To The POS' } : item);

export default {
    isOnline,
    GetSuppliers: () => either(() => ReceivingOrderData.GetSuppliers(), offline.GetSuppliers),
    GetLocations: () => either(() => ReceivingOrderData.GetLocations(), offline.GetLocations),
    FindItem: (supplierRecID, code, roDate) => either(async () => SentToPOS(await ReceivingOrderData.FindItem(supplierRecID, code, roDate)), () => offline.FindItem(supplierRecID, code, roDate)),
    SearchItems: (supplierRecID, searchString) => either(() => ReceivingOrderData.SearchItems(supplierRecID, searchString), () => offline.SearchItems(supplierRecID, searchString)),
    GetPendingItems: () => either(() => ReceivingOrderData.GetPendingItems(), offline.GetPendingItems),
    GetItemLots: (itemRecID, locationCode) => either(() => ReceivingOrderData.GetItemLots(itemRecID, locationCode), () => offline.GetItemLots(itemRecID, locationCode)),
    CheckBarcode: (itemRecID, barcode) => either(() => ReceivingOrderData.CheckBarcode(itemRecID, barcode), () => offline.CheckBarcode(itemRecID, barcode)),
    GenerateBarcode: () => either(() => ReceivingOrderData.GenerateBarcode(), offline.GenerateBarcode),
    RefExists: (roRef) => either(() => ReceivingOrderData.RefExists(roRef), offline.RefExists),
    GetPurchaseOrders: (supplierRecID, roDate) => either(() => ReceivingOrderData.GetPurchaseOrders(supplierRecID, roDate), offline.GetPurchaseOrders),
    GetPurchaseOrderLines: (supplierRecID, poRecID, roDate) => either(() => ReceivingOrderData.GetPurchaseOrderLines(supplierRecID, poRecID, roDate), offline.GetPurchaseOrderLines),
};
