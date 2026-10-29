import { callService } from './ServiceCall';
import { getLoginUserCode } from '../variables/LoginInfo';

// The physical count calls the server of the login screen, on the location of that server (LocCode of its web.config),
// the downloaded count keeps the server + location it was downloaded from

// Create a count : the lists answer in a second when the server is free, they never keep the user waiting without end
const LIST_TIMEOUT_MS = 90000;
const CREATE_TIMEOUT_MS = 180000;

// Location of the server : { location, locationDescription }, from "01 - Warehouse Zouk Mosbeh"
const GetServerLocation = (server, timeoutMs = 8000) => {

    return callService(server, 'GetLocationPretty', {}, timeoutMs, 'No response from the server')
        .then(strResult => {
            const [locationCode, ...description] = strResult.split(' - ');
            return { location: locationCode.trim().toUpperCase(), locationDescription: description.join(' - ').trim() };
        });

};

const GetPhysicalCountHeader = (server, location, countNumber, machineName) => {

    return callService(server, 'GetPhysicalCountHeader', {
        location: location,
        countNumber: countNumber,
        machineName: machineName,
        userCode: getLoginUserCode()
    })
        .then(strResult => JSON.parse(strResult)[0]);

};

// startFromRow is 1 based, returns [] after the last row
const GetPhysicalCountDetails = (server, location, countNumber, machineName, startFromRow, pageSize) => {

    return callService(server, 'GetPhysicalCountDetails', {
        location: location,
        countNumber: countNumber,
        machineName: machineName,
        startFromRow: startFromRow,
        pageSize: pageSize,
        userCode: getLoginUserCode()
    })
        .then(strResult => strResult == '' ? [] : JSON.parse(strResult));

};

const GetPhysicalCountBarcodes = (server, location, countNumber, machineName, startFromRow, pageSize) => {

    return callService(server, 'GetPhysicalCountBarcodes', {
        location: location,
        countNumber: countNumber,
        machineName: machineName,
        startFromRow: startFromRow,
        pageSize: pageSize
    })
        .then(strResult => strResult == '' ? [] : JSON.parse(strResult));

};

// Negations of the logged in user on the count of the location (tbl_CPP_NegationButton) : { showAvailableQty, showListDifferences } ('Y' / 'N')
const GetPhysicalCountNegations = (server, location, userCode, timeoutMs = 8000) => {

    return callService(server, 'GetPhysicalCountNegations', { location: location, userCode: userCode }, timeoutMs)
        .then(strResult => JSON.parse(strResult));

};

// lines : [{ lineRecID, counted }] - counted is the absolute total (replaces the server value)
const SavePhysicalCountLines = (server, location, countNumber, machineName, lines) => {

    return callService(server, 'SavePhysicalCountLines', {
        location: location,
        countNumber: countNumber,
        machineName: machineName,
        lines: JSON.stringify(lines)
    });

};

// logs : [{ logID, lineRecID, itemRecID, qty, total, mode, action, unit, userCode, dateStamp }] - the counting steps (tbl_CPP_PhysicalCountLogs)
// a log sent twice is saved once on the server
const SavePhysicalCountLogs = (server, location, countNumber, machineName, logs) => {

    return callService(server, 'SavePhysicalCountLogs', {
        location: location,
        countNumber: countNumber,
        machineName: machineName,
        logs: JSON.stringify(logs)
    });

};

//#region Create a count

// { location, locationDescription, workingDay, bookMode, showAvailableQty, brands, categories, productGroups, identifications }
// each list : [{ code, description }]
const GetPhysicalCountFilters = (server, location) => {

    return callService(server, 'GetPhysicalCountFilters', { location: location, userCode: getLoginUserCode() }, LIST_TIMEOUT_MS)
        .then(strResult => JSON.parse(strResult));

};

// filters : { books, notCounted, brands, categories, productGroups, identifications }
// selection : { all, groups, include, exclude } (see PhysicalCountCreateRules)
// after : null for the first page, else the last item shown ({ itemReference, itemRecID }) : the page starts after it
// exactMatch : a scan, only the item of this barcode or of this reference (not the items containing the text)
// Returns { totalItems, selectedItems, items: [{ itemRecID, itemReference, itemDescription, ..., systemQty (with the privilege only), selected }] }
// totalItems / selectedItems : first page only (-1 on the next pages), selectedItems -1 with a search
const GetPhysicalCountItems = (server, location, filters, selection, searchText, onlySelected, after, pageSize, exactMatch = false) => {

    return callService(server, 'GetPhysicalCountItems', {
        location: location,
        filters: JSON.stringify(filters),
        selection: JSON.stringify(selection),
        searchText: searchText,
        onlySelected: onlySelected,
        afterReference: after ? after.itemReference : '',
        afterItemRecID: after ? after.itemRecID : '',
        pageSize: pageSize,
        exactMatch: exactMatch,
        userCode: getLoginUserCode()
    }, LIST_TIMEOUT_MS)
        .then(strResult => JSON.parse(strResult));

};

// requestID : one per Create (NewRequestID), sent again as is when the answer did not come in time :
// the server returns the count of the first try instead of creating a second count
// Returns { countNumber, itemsCount, linesCount, barcodesCount, existing } (existing : the count of the first try)
const CreatePhysicalCount = (server, location, machineName, filters, selection, memo, requestID) => {

    return callService(server, 'CreatePhysicalCount', {
        location: location,
        machineName: machineName,
        filters: JSON.stringify(filters),
        selection: JSON.stringify(selection),
        memo: memo,
        requestID: requestID
    }, CREATE_TIMEOUT_MS, CREATE_TIMEOUT_MESSAGE)
        .then(strResult => JSON.parse(strResult));

};

export const CREATE_TIMEOUT_MESSAGE = 'The server is taking too long to create the count';

// Letters, digits and dashes, at most 40 : machine-time-random
export const NewRequestID = (machineName) =>
    `${machineName}-${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 8)}`.replace(/[^A-Za-z0-9-]/g, '').slice(0, 40);

// Counts of this machine on the location (the last 50) : [{ countNumber, dateCount, nbrItems, status: open | cancelled | finalized, createdOnPortable }]
const GetMachinePhysicalCounts = (server, location, machineName) => {

    return callService(server, 'GetMachinePhysicalCounts', { location: location, machineName: machineName }, LIST_TIMEOUT_MS)
        .then(strResult => JSON.parse(strResult));

};

//#endregion

export default {
    GetPhysicalCountFilters: GetPhysicalCountFilters,
    GetPhysicalCountItems: GetPhysicalCountItems,
    CreatePhysicalCount: CreatePhysicalCount,
    GetMachinePhysicalCounts: GetMachinePhysicalCounts,
    GetServerLocation: GetServerLocation,
    GetPhysicalCountHeader: GetPhysicalCountHeader,
    GetPhysicalCountDetails: GetPhysicalCountDetails,
    GetPhysicalCountBarcodes: GetPhysicalCountBarcodes,
    SavePhysicalCountLines: SavePhysicalCountLines,
    SavePhysicalCountLogs: SavePhysicalCountLogs,
    GetPhysicalCountNegations: GetPhysicalCountNegations,
}
