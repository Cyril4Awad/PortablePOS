import { callService } from './ServiceCall';
import { getLoginUserCode } from '../variables/LoginInfo';
import ServerInfo from '../configuration/ServerInfo';

// Free Receiving calls the server of the login screen
// The pages follow the record id : afterRecID = '' for the first page, else the last one stored

// First step of the download
// Returns { itemsCount, barcodesCount, supplierRefsCount, suppliersCount, lotsCount, decimals, currencyCC }
const GetReceivingInfo = (server, timeoutMs = 0) => {

    return callService(server, 'GetReceivingInfo', {}, timeoutMs, 'No response from the server')
        .then(strResult => JSON.parse(strResult));

};

// [{ itemRecID, itemReference, itemDescription, itemSupplier, itemCategory, itemSerie, itemSingleRef1, itemSingleRef2, itemUnit, itemClass }]
// [] after the last page
const GetReceivingItems = (server, afterRecID, pageSize) => {

    return callService(server, 'GetReceivingItems', { afterRecID: afterRecID, pageSize: pageSize })
        .then(strResult => strResult == '' ? [] : JSON.parse(strResult));

};

// codeKind : barcode | supplierRef
// [{ codeRecID, itemRecID, codeValue, supplierRecID, codeExpiry, codeBatch }], [] after the last page
const GetReceivingItemCodes = (server, codeKind, afterRecID, pageSize) => {

    return callService(server, 'GetReceivingItemCodes', { codeKind: codeKind, afterRecID: afterRecID, pageSize: pageSize })
        .then(strResult => strResult == '' ? [] : JSON.parse(strResult));

};

// The lots of the location of the server and of location 00 (the lot window)
// [{ lotRecID, itemRecID, locationCode, lotExpiry, lotBatch, lotProduction, lotAvailable }], [] after the last page
const GetReceivingLots = (server, afterRecID, pageSize) => {

    return callService(server, 'GetReceivingLots', { afterRecID: afterRecID, pageSize: pageSize })
        .then(strResult => strResult == '' ? [] : JSON.parse(strResult));

};

// [{ supplierRecID, supplierCode, supplierName }], all of them in one call
const GetReceivingSuppliers = (server) => {

    return callService(server, 'GetReceivingSuppliers', {})
        .then(strResult => strResult == '' ? [] : JSON.parse(strResult));

};

// The receiving orders of Free Receiving saved on the server of the login screen (by the user logged in), in one transaction :
// all of them or none (the error names the order that failed)
// orders : [{ orderName, receivingOrder }] receivingOrder = the receiving order of SaveReceivingOrder (Payload of ROSave)
// Returns [{ roRecID, already }] in the same order (already : it was saved by an upload that got no answer)
const SaveFreeReceiving = async (orders) => {

    const server = await ServerInfo.serverIP();
    if (!server) throw new Error('Please define the server on the login screen');
    return callService(server, 'SaveFreeReceiving', {
        userCode: getLoginUserCode(), receivingOrders: JSON.stringify(orders),
    }, 180000).then(strResult => JSON.parse(strResult));

};

export default {
    GetReceivingInfo: GetReceivingInfo,
    GetReceivingItems: GetReceivingItems,
    GetReceivingItemCodes: GetReceivingItemCodes,
    GetReceivingLots: GetReceivingLots,
    GetReceivingSuppliers: GetReceivingSuppliers,
    SaveFreeReceiving: SaveFreeReceiving,
}
