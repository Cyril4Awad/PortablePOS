import { callService } from './ServiceCall';

// Transfer calls the server of its own settings (TransferSettings), not the server of the login screen
// The stock is read online : the scan, the stock search and the available qty come from the server, the transfer is saved on the server

const listResult = (strResult) => strResult == '' ? [] : JSON.parse(strResult);

// The available qty in whole thousandths like the lines of the transfer (12.5 => 12500)
const toMilli = (value) => Math.round(Number(value || 0) * 1000);

// A lot row of the server : { lotRecID ('' = the item has no available qty), itemRecID, itemReference, itemDescription, itemUnit,
// itemSingleRef1, itemSingleRef2, itemProductGroup, itemProductGroupDesc, itemFourthParty, itemBarcode,
// lotExpiry, lotBatch, lotColor, lotGrid, lotSize, lotAvailable (thousandths) }
const lotRows = (strResult) => listResult(strResult).map(row => ({ ...row, lotAvailable: toMilli(row.lotAvailable) }));

// The locations of the settings and the destinations (not the head office), also the connection test
// [{ locationCode, locationDescription, isCurrent }]
const GetTransferLocations = (server, timeoutMs = 0) => {

    return callService(server, 'GetTransferLocations', {}, timeoutMs, 'No response from the server')
        .then(listResult);

};

// { locationCode, locationDescription, decimals }
const GetTransferInfo = (server, location) => {

    return callService(server, 'GetTransferInfo', { location: location }, 20000, 'No response from the server')
        .then(strResult => JSON.parse(strResult));

};

// The stock search : every lot of the location with an available qty
const GetTransferStock = (server, location) => {

    return callService(server, 'GetTransferStock', { location: location }, 60000, 'No response from the server')
        .then(lotRows);

};

// The code scanned : the lots of the items with this barcode / single reference / reference (else the references starting with it)
const FindTransferItem = (server, location, code) => {

    return callService(server, 'FindTransferItem', { location: location, code: code }, 20000, 'No response from the server')
        .then(lotRows);

};

// The available qty now of lots : { [lotRecID]: qty } (thousandths), a lot without available qty is not in the result
const GetTransferLotsAvailable = (server, location, lotRecIDs) => {

    return callService(server, 'GetTransferLotsAvailable', { location: location, lotRecIDs: lotRecIDs.join(',') }, 20000, 'No response from the server')
        .then(strResult => {
            const available = {};
            listResult(strResult).forEach(row => { available[row.lotRecID] = toMilli(row.lotAvailable); });
            return available;
        });

};

// Saves the transfer in the upload table of the server (one transfer per destination), all the lines or none
// saveID : the id of this transfer on the device, sent again after a failure it is not saved twice
// lines : [{ itemRecID, lotRecID, destination, qty }] (qty : text, the decimal point is a dot)
// { alreadySaved, transfers: [{ transferID, destination, lines, qty }] }
const SaveTransfer = (server, location, machineName, saveID, lines) => {

    return callService(server, 'SaveTransfer', { location: location, machineName: machineName, saveID: saveID, transfer: JSON.stringify(lines) },
        120000, 'No answer from the server, press Save Transfer again (the transfer is not saved twice)')
        .then(strResult => JSON.parse(strResult));

};

export default {
    GetTransferLocations: GetTransferLocations,
    GetTransferInfo: GetTransferInfo,
    GetTransferStock: GetTransferStock,
    FindTransferItem: FindTransferItem,
    GetTransferLotsAvailable: GetTransferLotsAvailable,
    SaveTransfer: SaveTransfer,
}
