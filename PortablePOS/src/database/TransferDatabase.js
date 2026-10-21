import { enablePromise, openDatabase } from 'react-native-sqlite-storage';

// Local storage of Transfer (works offline)
// TR_Info : one row, the download of the stock of the location we transfer from (counts, resume points)
// TR_Items / TR_Lots / TR_Barcodes / TR_Locations : the downloaded stock, replaced by every new download
// TR_Header : one row, the transfer being prepared (one or multiple locations, destination, deliver by, remarks)
// TR_Lines : one line per lot + destination, with its own copy of the item and lot (kept by a new download)
// Quantities are stored as whole thousandths (12.5 => 12500), so totals never drift

const dbName = 'transfer.db';

enablePromise(true);

// Version 2 : the single references of the items (a scan matches them like a barcode), filled by the next download
const schemaVersion = 2;

// Kept as a promise so screens loading at the same time share one connection
let lcDBPromise = null;

const getDB = () => {
  if (!lcDBPromise) {
    lcDBPromise = (async () => {
      const db = await openDatabase({ name: dbName, location: 'default' });
      await createTables(db);
      await upgradeTables(db);
      return db;
    })().catch(error => {
      lcDBPromise = null;
      throw error;
    });
  }
  return lcDBPromise;
};

const createTables = async (db) => {
  await db.executeSql(`CREATE TABLE IF NOT EXISTS TR_Info (
                         infoID INTEGER PRIMARY KEY,
                         server TEXT NOT NULL,
                         location TEXT NOT NULL,
                         locationDescription TEXT,
                         downloadDate TEXT,
                         decimals INTEGER,
                         currencyLC TEXT,
                         currencyCC TEXT,
                         itemsCount INTEGER,
                         lotsCount INTEGER,
                         barcodesCount INTEGER,
                         itemsStored INTEGER NOT NULL DEFAULT 0,
                         lotsStored INTEGER NOT NULL DEFAULT 0,
                         barcodesStored INTEGER NOT NULL DEFAULT 0,
                         lastItemRecID TEXT NOT NULL DEFAULT '',
                         lastLotRecID TEXT NOT NULL DEFAULT '',
                         lastBarcodeRecID TEXT NOT NULL DEFAULT '',
                         complete INTEGER NOT NULL DEFAULT 0 )`);

  await db.executeSql(`CREATE TABLE IF NOT EXISTS TR_Items (
                         itemRecID TEXT PRIMARY KEY,
                         itemReference TEXT,
                         itemReferenceUpper TEXT,
                         itemDescription TEXT,
                         itemDescriptionUpper TEXT,
                         itemUnit TEXT,
                         itemSingleRef1Upper TEXT,
                         itemSingleRef2Upper TEXT,
                         priceLC REAL,
                         priceCC REAL )`);
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_TR_Items_Reference ON TR_Items (itemReferenceUpper)');

  // lotAvailable : thousandths
  await db.executeSql(`CREATE TABLE IF NOT EXISTS TR_Lots (
                         lotRecID TEXT PRIMARY KEY,
                         itemRecID TEXT NOT NULL,
                         lotExpiry TEXT,
                         lotBatch TEXT,
                         lotColor TEXT,
                         lotGrid TEXT,
                         lotSize TEXT,
                         lotAvailable INTEGER NOT NULL DEFAULT 0 )`);
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_TR_Lots_Item ON TR_Lots (itemRecID)');

  await db.executeSql(`CREATE TABLE IF NOT EXISTS TR_Barcodes (
                         codeRecID TEXT PRIMARY KEY,
                         itemRecID TEXT NOT NULL,
                         codeValueUpper TEXT )`);
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_TR_Barcodes_Value ON TR_Barcodes (codeValueUpper)');

  await db.executeSql(`CREATE TABLE IF NOT EXISTS TR_Locations (
                         locationCode TEXT PRIMARY KEY,
                         locationDescription TEXT )`);

  // multipleLocations : 0 one destination (destination), 1 each lot has its own destinations
  await db.executeSql(`CREATE TABLE IF NOT EXISTS TR_Header (
                         headerID INTEGER PRIMARY KEY,
                         multipleLocations INTEGER NOT NULL DEFAULT 0,
                         destination TEXT NOT NULL DEFAULT '',
                         deliverBy TEXT NOT NULL DEFAULT '',
                         remarks TEXT NOT NULL DEFAULT '' )`);

  // state : 1 not uploaded, 2 uploaded (the upload comes later)
  await db.executeSql(`CREATE TABLE IF NOT EXISTS TR_Lines (
                         lineID INTEGER PRIMARY KEY AUTOINCREMENT,
                         lotRecID TEXT NOT NULL,
                         destination TEXT NOT NULL,
                         itemRecID TEXT NOT NULL,
                         itemReference TEXT,
                         itemDescription TEXT,
                         itemUnit TEXT,
                         lotExpiry TEXT,
                         lotBatch TEXT,
                         lotColor TEXT,
                         lotGrid TEXT,
                         lotSize TEXT,
                         priceLC REAL,
                         priceCC REAL,
                         qty INTEGER NOT NULL DEFAULT 0,
                         state INTEGER NOT NULL DEFAULT 1,
                         updatedAt TEXT )`);
  await db.executeSql('CREATE UNIQUE INDEX IF NOT EXISTS IX_TR_Lines_Key ON TR_Lines (lotRecID, destination)');

};

// Columns added to the existing tables (the downloaded stock and the transfer are kept), then the indexes on them
const upgradeTables = async (db) => {
  const [result] = await db.executeSql('PRAGMA user_version');
  if (result.rows.item(0).user_version < 2) {
    const [columns] = await db.executeSql('PRAGMA table_info(TR_Items)');
    const names = [];
    for (let index = 0; index < columns.rows.length; index++) names.push(columns.rows.item(index).name);
    if (!names.includes('itemSingleRef1Upper')) await db.executeSql('ALTER TABLE TR_Items ADD COLUMN itemSingleRef1Upper TEXT');
    if (!names.includes('itemSingleRef2Upper')) await db.executeSql('ALTER TABLE TR_Items ADD COLUMN itemSingleRef2Upper TEXT');
  }
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_TR_Items_SingleRef1 ON TR_Items (itemSingleRef1Upper)');
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_TR_Items_SingleRef2 ON TR_Items (itemSingleRef2Upper)');
  await db.executeSql(`PRAGMA user_version = ${schemaVersion}`);
};

const query = async (sql, params = []) => {
  const db = await getDB();
  const [result] = await db.executeSql(sql, params);
  const rows = [];
  for (let index = 0; index < result.rows.length; index++) {
    rows.push(result.rows.item(index));
  }
  return rows;
};

const execute = async (sql, params = []) => {
  const db = await getDB();
  const [result] = await db.executeSql(sql, params);
  return result.rowsAffected;
};

// statements : [[sql, params], ...] in one transaction
const executeInTransaction = async (statements) => {
  const db = await getDB();
  await db.transaction(tx => {
    statements.forEach(([sql, params]) => tx.executeSql(sql, params));
  });
};

// LIKE text with % and _ as plain characters (used with ESCAPE '!')
const likeText = (text) => text.replace(/[!%_]/g, character => '!' + character);

const upper = (text) => (text ?? '').toString().toUpperCase();

const toMilli = (value) => Math.round(Number(value || 0) * 1000);

//#region Download

const GetInfo = async () => {
  const rows = await query('SELECT * FROM TR_Info WHERE infoID = 1');
  return rows.length > 0 ? rows[0] : null;
};

// Starts a new download : the previous stock is replaced, the transfer being prepared is kept
const StartDownload = (server, info, locations) => {
  const statements = [
    ['DELETE FROM TR_Barcodes', []],
    ['DELETE FROM TR_Lots', []],
    ['DELETE FROM TR_Items', []],
    ['DELETE FROM TR_Locations', []],
    ['DELETE FROM TR_Info', []],
    [`INSERT INTO TR_Info (infoID, server, location, locationDescription, decimals, currencyLC, currencyCC, itemsCount, lotsCount, barcodesCount)
      VALUES (1, ?, ?, ?, ?, ?, ?, ?, ?, ?)`,
      [server, info.locationCode, info.locationDescription, parseInt(info.decimals, 10) || 0, info.currencyLC, info.currencyCC,
        info.itemsCount, info.lotsCount, info.barcodesCount]],
  ];
  locations.forEach(location => statements.push([
    'INSERT OR REPLACE INTO TR_Locations (locationCode, locationDescription) VALUES (?, ?)',
    [location.locationCode, location.locationDescription]]));
  return executeInTransaction(statements);
};

const SaveItemsPage = (items) => {
  const statements = items.map(item => [
    `INSERT OR REPLACE INTO TR_Items (itemRecID, itemReference, itemReferenceUpper, itemDescription, itemDescriptionUpper, itemUnit,
                                      itemSingleRef1Upper, itemSingleRef2Upper, priceLC, priceCC)
     VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)`,
    [item.itemRecID, item.itemReference, upper(item.itemReference), item.itemDescription, upper(item.itemDescription),
      item.itemUnit, upper(item.itemSingleRef1), upper(item.itemSingleRef2), Number(item.priceLC) || 0, Number(item.priceCC) || 0]]);
  statements.push([
    'UPDATE TR_Info SET itemsStored = (SELECT COUNT(*) FROM TR_Items), lastItemRecID = ? WHERE infoID = 1',
    [items[items.length - 1].itemRecID]]);
  return executeInTransaction(statements);
};

const SaveLotsPage = (lots) => {
  const statements = lots.map(lot => [
    `INSERT OR REPLACE INTO TR_Lots (lotRecID, itemRecID, lotExpiry, lotBatch, lotColor, lotGrid, lotSize, lotAvailable)
     VALUES (?, ?, ?, ?, ?, ?, ?, ?)`,
    [lot.lotRecID, lot.itemRecID, lot.lotExpiry, lot.lotBatch, lot.lotColor, lot.lotGrid, lot.lotSize, toMilli(lot.lotAvailable)]]);
  statements.push([
    'UPDATE TR_Info SET lotsStored = (SELECT COUNT(*) FROM TR_Lots), lastLotRecID = ? WHERE infoID = 1',
    [lots[lots.length - 1].lotRecID]]);
  return executeInTransaction(statements);
};

const SaveBarcodesPage = (barcodes) => {
  const statements = barcodes.map(barcode => [
    'INSERT OR REPLACE INTO TR_Barcodes (codeRecID, itemRecID, codeValueUpper) VALUES (?, ?, ?)',
    [barcode.codeRecID, barcode.itemRecID, upper(barcode.codeValue)]]);
  statements.push([
    'UPDATE TR_Info SET barcodesStored = (SELECT COUNT(*) FROM TR_Barcodes), lastBarcodeRecID = ? WHERE infoID = 1',
    [barcodes[barcodes.length - 1].codeRecID]]);
  return executeInTransaction(statements);
};

const CompleteDownload = (downloadDate) => execute(
  'UPDATE TR_Info SET complete = 1, downloadDate = ? WHERE infoID = 1', [downloadDate]);

//#endregion

//#region Stock

const GetItem = async (itemRecID) => {
  const rows = await query('SELECT * FROM TR_Items WHERE itemRecID = ?', [itemRecID]);
  return rows.length > 0 ? rows[0] : null;
};

// A barcode of the item, or one of its single references (an empty single reference never matches : the scan is never empty)
const FindItemsByBarcode = (barcode) => {
  const value = upper(barcode.trim());
  return query(
    `SELECT * FROM TR_Items
     WHERE itemSingleRef1Upper = ? OR itemSingleRef2Upper = ?
           OR itemRecID IN (SELECT itemRecID FROM TR_Barcodes WHERE codeValueUpper = ?)
     ORDER BY itemReferenceUpper LIMIT 50`, [value, value, value]);
};

// exact : the whole reference, else its start
const FindItemsByReference = (text, exact, limit) => {
  const value = upper(text.trim());
  return exact
    ? query('SELECT * FROM TR_Items WHERE itemReferenceUpper = ? ORDER BY itemReferenceUpper LIMIT ?', [value, limit])
    : query("SELECT * FROM TR_Items WHERE itemReferenceUpper LIKE ? ESCAPE '!' ORDER BY itemReferenceUpper LIMIT ?", [likeText(value) + '%', limit]);
};

// The items with their available qty in the location (the lots downloaded) and the qty already in the transfer
// onlyAvailable : the items that have an available qty, text : start of the reference or barcode, any part of the description
const SearchItems = (text, onlyAvailable, limit) => {
  const value = upper((text ?? '').trim());
  const conditions = [];
  const params = [];
  if (value !== '') {
    conditions.push(`(itemReferenceUpper LIKE ? ESCAPE '!' OR itemDescriptionUpper LIKE ? ESCAPE '!'
                      OR itemSingleRef1Upper LIKE ? ESCAPE '!' OR itemSingleRef2Upper LIKE ? ESCAPE '!'
                      OR itemRecID IN (SELECT itemRecID FROM TR_Barcodes WHERE codeValueUpper LIKE ? ESCAPE '!'))`);
    params.push(likeText(value) + '%', '%' + likeText(value) + '%', likeText(value) + '%', likeText(value) + '%', likeText(value) + '%');
  }
  if (onlyAvailable) conditions.push('itemRecID IN (SELECT itemRecID FROM TR_Lots)');
  params.push(limit);
  return query(
    `SELECT TR_Items.*,
            IFNULL((SELECT SUM(lotAvailable) FROM TR_Lots WHERE TR_Lots.itemRecID = TR_Items.itemRecID), 0) available,
            IFNULL((SELECT SUM(qty) FROM TR_Lines WHERE TR_Lines.itemRecID = TR_Items.itemRecID), 0) transferQty
     FROM   TR_Items
     ${conditions.length > 0 ? 'WHERE ' + conditions.join(' AND ') : ''}
     ORDER BY itemReferenceUpper LIMIT ?`, params);
};

// The lots of the item with the qty already in the transfer (all the destinations)
const LOT_SELECT = `SELECT TR_Lots.*, IFNULL((SELECT SUM(qty) FROM TR_Lines WHERE TR_Lines.lotRecID = TR_Lots.lotRecID), 0) transferQty FROM TR_Lots`;

const GetItemLots = (itemRecID) => query(`${LOT_SELECT} WHERE itemRecID = ? ORDER BY lotExpiry, lotRecID`, [itemRecID]);

const GetLot = async (lotRecID) => {
  const rows = await query(`${LOT_SELECT} WHERE lotRecID = ?`, [lotRecID]);
  return rows.length > 0 ? rows[0] : null;
};

// The destinations : the locations downloaded with the stock, except the one we transfer from
const GetDestinations = (fromLocation) => query(
  'SELECT * FROM TR_Locations WHERE locationCode <> ? ORDER BY locationCode', [fromLocation]);

//#endregion

//#region Header

const GetHeader = async () => {
  const rows = await query('SELECT * FROM TR_Header WHERE headerID = 1');
  if (rows.length > 0) return rows[0];
  await execute('INSERT OR IGNORE INTO TR_Header (headerID) VALUES (1)');
  return { headerID: 1, multipleLocations: 0, destination: '', deliverBy: '', remarks: '' };
};

// The mode can only change while the transfer has no line (checked by the screen too)
const SetMultipleLocations = async (multiple) => {
  await GetHeader();
  const [{ lines }] = await query('SELECT COUNT(*) lines FROM TR_Lines');
  if (lines > 0) throw new Error('Clear the items of the transfer to change the way of work');
  await execute('UPDATE TR_Header SET multipleLocations = ? WHERE headerID = 1', [multiple ? 1 : 0]);
};

// One location : the destination of the transfer and of all its lines
const SetDestination = async (destination) => {
  await GetHeader();
  await executeInTransaction([
    ['UPDATE TR_Header SET destination = ? WHERE headerID = 1', [destination]],
    ['UPDATE TR_Lines SET destination = ?, state = 1 WHERE destination <> ?', [destination, destination]],
  ]);
};

const SaveHeaderTexts = async (deliverBy, remarks) => {
  await GetHeader();
  await execute('UPDATE TR_Header SET deliverBy = ?, remarks = ? WHERE headerID = 1', [deliverBy, remarks]);
};

//#endregion

//#region Lines

// { lines, lots, qty, destinations, pending }
const GetSummary = async () => {
  const rows = await query(
    `SELECT COUNT(*) lines, COUNT(DISTINCT lotRecID) lots, IFNULL(SUM(qty), 0) qty, COUNT(DISTINCT destination) destinations,
            IFNULL(SUM(CASE WHEN state = 1 THEN 1 ELSE 0 END), 0) pending
     FROM TR_Lines`);
  return rows[0];
};

// The last lines first, searched by the item reference / description, the lot or the destination
const GetLines = (searchText, limit) => {
  const text = (searchText ?? '').trim();
  if (text === '') return query('SELECT * FROM TR_Lines ORDER BY updatedAt DESC, lineID DESC LIMIT ?', [limit]);
  const part = '%' + likeText(upper(text)) + '%';
  return query(
    `SELECT * FROM TR_Lines
     WHERE UPPER(itemReference) LIKE ? ESCAPE '!' OR UPPER(itemDescription) LIKE ? ESCAPE '!'
           OR UPPER(lotBatch) LIKE ? ESCAPE '!' OR UPPER(destination) LIKE ? ESCAPE '!'
     ORDER BY updatedAt DESC, lineID DESC LIMIT ?`, [part, part, part, part, limit]);
};

// The qty of a lot per destination : { [destination]: qty }
const GetLotQtys = async (lotRecID) => {
  const rows = await query('SELECT destination, qty FROM TR_Lines WHERE lotRecID = ?', [lotRecID]);
  const qtys = {};
  rows.forEach(row => { qtys[row.destination] = row.qty; });
  return qtys;
};

const lineInsert = (item, lot, destination, now) => [
  `INSERT OR IGNORE INTO TR_Lines (lotRecID, destination, itemRecID, itemReference, itemDescription, itemUnit,
                                   lotExpiry, lotBatch, lotColor, lotGrid, lotSize, priceLC, priceCC, qty, state, updatedAt)
   VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, 0, 1, ?)`,
  [lot.lotRecID, destination, item.itemRecID, item.itemReference, item.itemDescription, item.itemUnit,
    lot.lotExpiry, lot.lotBatch, lot.lotColor, lot.lotGrid, lot.lotSize, item.priceLC, item.priceCC, now]];

// One location : adds the qty to the line of the lot (created when there is none), returns its total
const AddLotQty = async (item, lot, destination, qty) => {
  const now = new Date().toISOString();
  await executeInTransaction([
    lineInsert(item, lot, destination, now),
    ['UPDATE TR_Lines SET qty = qty + ?, state = 1, updatedAt = ? WHERE lotRecID = ? AND destination = ?', [qty, now, lot.lotRecID, destination]],
  ]);
  const rows = await query('SELECT qty FROM TR_Lines WHERE lotRecID = ? AND destination = ?', [lot.lotRecID, destination]);
  return rows[0].qty;
};

// Multiple locations : the qty of the lot for each destination (0 removes the line)
// qtys : { [destination]: qty }
const SetLotQtys = (item, lot, qtys) => {
  const now = new Date().toISOString();
  const statements = [];
  Object.keys(qtys).forEach(destination => {
    if (qtys[destination] > 0) {
      statements.push(lineInsert(item, lot, destination, now));
      statements.push(['UPDATE TR_Lines SET qty = ?, state = 1, updatedAt = ? WHERE lotRecID = ? AND destination = ? AND qty <> ?',
        [qtys[destination], now, lot.lotRecID, destination, qtys[destination]]]);
    }
    else {
      statements.push(['DELETE FROM TR_Lines WHERE lotRecID = ? AND destination = ?', [lot.lotRecID, destination]]);
    }
  });
  return executeInTransaction(statements);
};

const SetLineQty = (lineID, qty) => execute(
  'UPDATE TR_Lines SET qty = ?, state = 1, updatedAt = ? WHERE lineID = ?', [qty, new Date().toISOString(), lineID]);

const DeleteLine = (lineID) => execute('DELETE FROM TR_Lines WHERE lineID = ?', [lineID]);

// Clears the items : the way of work can be changed again
const ClearLines = () => execute('DELETE FROM TR_Lines');

//#endregion

export default {
  GetInfo,
  StartDownload,
  SaveItemsPage,
  SaveLotsPage,
  SaveBarcodesPage,
  CompleteDownload,
  GetItem,
  FindItemsByBarcode,
  FindItemsByReference,
  SearchItems,
  GetItemLots,
  GetLot,
  GetDestinations,
  GetHeader,
  SetMultipleLocations,
  SetDestination,
  SaveHeaderTexts,
  GetSummary,
  GetLines,
  GetLotQtys,
  AddLotQty,
  SetLotQtys,
  SetLineQty,
  DeleteLine,
  ClearLines,
}
