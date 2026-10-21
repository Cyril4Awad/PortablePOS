import { callService } from './ServiceCall';

// Transfer calls the server of its own settings (TransferSettings), not the server of the login screen
// The pages follow the record id : afterRecID = '' for the first page, else the last one stored

const listResult = (strResult) => strResult == '' ? [] : JSON.parse(strResult);

// The locations of the settings and the destinations (not the head office), also the connection test
// [{ locationCode, locationDescription }]
const GetTransferLocations = (server, timeoutMs = 0) => {

    return callService(server, 'GetTransferLocations', {}, timeoutMs, 'No response from the server')
        .then(listResult);

};

// { locationCode, locationDescription, itemsCount, lotsCount, barcodesCount, decimals, currencyLC, currencyCC }
const GetTransferInfo = (server, location) => {

    return callService(server, 'GetTransferInfo', { location: location })
        .then(strResult => JSON.parse(strResult));

};

// [{ itemRecID, itemReference, itemDescription, itemUnit, priceLC, priceCC }], [] after the last page
const GetTransferItems = (server, location, afterRecID, pageSize) => {

    return callService(server, 'GetTransferItems', { location: location, afterRecID: afterRecID, pageSize: pageSize })
        .then(listResult);

};

// The lots of the location with an available qty
// [{ lotRecID, itemRecID, lotExpiry, lotBatch, lotColor, lotGrid, lotSize, lotAvailable }], [] after the last page
const GetTransferLots = (server, location, afterRecID, pageSize) => {

    return callService(server, 'GetTransferLots', { location: location, afterRecID: afterRecID, pageSize: pageSize })
        .then(listResult);

};

// [{ codeRecID, itemRecID, codeValue }], [] after the last page
const GetTransferBarcodes = (server, afterRecID, pageSize) => {

    return callService(server, 'GetTransferBarcodes', { afterRecID: afterRecID, pageSize: pageSize })
        .then(listResult);

};

export default {
    GetTransferLocations: GetTransferLocations,
    GetTransferInfo: GetTransferInfo,
    GetTransferItems: GetTransferItems,
    GetTransferLots: GetTransferLots,
    GetTransferBarcodes: GetTransferBarcodes,
}
