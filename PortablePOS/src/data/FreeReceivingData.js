import { callService } from './ServiceCall';

// Free Receiving calls the server of its own settings (FreeReceivingSettings), not the server of the login screen
// The pages follow the record id : afterRecID = '' for the first page, else the last one stored

// Connection test of the settings and first step of the download
// Returns { itemsCount, barcodesCount, supplierRefsCount, suppliersCount, decimals }
const GetReceivingInfo = (server, timeoutMs = 0) => {

    return callService(server, 'GetReceivingInfo', {}, timeoutMs, 'No response from the server')
        .then(strResult => JSON.parse(strResult));

};

// [{ itemRecID, itemReference, itemDescription, itemSupplier, itemCategory, itemSerie, itemSingleRef1, itemSingleRef2, itemUnit }]
// [] after the last page
const GetReceivingItems = (server, afterRecID, pageSize) => {

    return callService(server, 'GetReceivingItems', { afterRecID: afterRecID, pageSize: pageSize })
        .then(strResult => strResult == '' ? [] : JSON.parse(strResult));

};

// codeKind : barcode | supplierRef
// [{ codeRecID, itemRecID, codeValue, supplierRecID }], [] after the last page
const GetReceivingItemCodes = (server, codeKind, afterRecID, pageSize) => {

    return callService(server, 'GetReceivingItemCodes', { codeKind: codeKind, afterRecID: afterRecID, pageSize: pageSize })
        .then(strResult => strResult == '' ? [] : JSON.parse(strResult));

};

// [{ supplierRecID, supplierCode, supplierName }], all of them in one call
const GetReceivingSuppliers = (server) => {

    return callService(server, 'GetReceivingSuppliers', {})
        .then(strResult => strResult == '' ? [] : JSON.parse(strResult));

};

export default {
    GetReceivingInfo: GetReceivingInfo,
    GetReceivingItems: GetReceivingItems,
    GetReceivingItemCodes: GetReceivingItemCodes,
    GetReceivingSuppliers: GetReceivingSuppliers,
}
