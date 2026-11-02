import { enablePromise, openDatabase } from 'react-native-sqlite-storage';

// Local storage of Transfer : the transfer being prepared (kept until it is saved on the server), the stock is read online
// TR_Info : one row, the location of the server and the decimals of the quantities (read from the server when the screen opens)
// TR_Locations : the destinations (read from the server when the screen opens, kept for a server not reachable)
// TR_Header : one row, the transfer being prepared (one or multiple locations, destination, save id)
// TR_Lines : one line per lot + destination, with its own copy of the item and lot and the available qty of the lot read last
// Quantities are stored as whole thousandths (12.5 => 12500), so totals never drift

const dbName = 'transfer.db';

enablePromise(true);

// Version 4 : the stock is online, the downloaded tables are removed, the lines keep the available qty of their lot
const schemaVersion = 4;

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
                         decimals INTEGER )`);

  await db.executeSql(`CREATE TABLE IF NOT EXISTS TR_Locations (
                         locationCode TEXT PRIMARY KEY,
                         locationDescription TEXT )`);

  // multipleLocations : 0 one destination (destination), 1 each lot has its own destinations
  await db.executeSql(`CREATE TABLE IF NOT EXISTS TR_Header (
                         headerID INTEGER PRIMARY KEY,
                         multipleLocations INTEGER NOT NULL DEFAULT 0,
                         destination TEXT NOT NULL DEFAULT '',
                         deliverBy TEXT NOT NULL DEFAULT '',
                         remarks TEXT NOT NULL DEFAULT '',
                         saveID TEXT NOT NULL DEFAULT '' )`);

  // lotAvailable : the available qty of the lot when it was read last (all the lines of a lot have the same)
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
                         lotAvailable INTEGER NOT NULL DEFAULT 0,
                         qty INTEGER NOT NULL DEFAULT 0,
                         state INTEGER NOT NULL DEFAULT 1,
                         updatedAt TEXT )`);
  await db.executeSql('CREATE UNIQUE INDEX IF NOT EXISTS IX_TR_Lines_Key ON TR_Lines (lotRecID, destination)');

};

// The tables of the versions before : the columns added, the downloaded stock removed (the transfer being prepared is kept)
const upgradeTables = async (db) => {
  const [result] = await db.executeSql('PRAGMA user_version');
  const version = result.rows.item(0).user_version;
  const columnsOf = async (table) => {
    const [columns] = await db.executeSql(`PRAGMA table_info(${table})`);
    const names = [];
    for (let index = 0; index < columns.rows.length; index++) names.push(columns.rows.item(index).name);
    return names;
  };
  if (version < 4) {
    if (!(await columnsOf('TR_Header')).includes('saveID')) await db.executeSql("ALTER TABLE TR_Header ADD COLUMN saveID TEXT NOT NULL DEFAULT ''");
    if (!(await columnsOf('TR_Lines')).includes('lotAvailable')) {
      await db.executeSql('ALTER TABLE TR_Lines ADD COLUMN lotAvailable INTEGER NOT NULL DEFAULT 0');
      // the available qty of the lot downloaded, read again from the server when the screen opens
      const [tables] = await db.executeSql("SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'TR_Lots'");
      if (tables.rows.length > 0) {
        await db.executeSql('UPDATE TR_Lines SET lotAvailable = IFNULL((SELECT lotAvailable FROM TR_Lots WHERE TR_Lots.lotRecID = TR_Lines.lotRecID), 0)');
      }
    }
    await db.executeSql('DROP TABLE IF EXISTS TR_Barcodes');
    await db.executeSql('DROP TABLE IF EXISTS TR_Lots');
    await db.executeSql('DROP TABLE IF EXISTS TR_Items');
    await db.executeSql('DROP TABLE IF EXISTS TR_Info');
    await createTables(db);
  }
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

//#region Server info and destinations

const GetInfo = async () => {
  const rows = await query('SELECT * FROM TR_Info WHERE infoID = 1');
  return rows.length > 0 ? rows[0] : null;
};

// The location of the server and the destinations, read from the server
// info : { locationCode, locationDescription, decimals }, locations : [{ locationCode, locationDescription }]
const SaveServerInfo = (server, info, locations) => {
  const statements = [
    ['DELETE FROM TR_Locations', []],
    ['DELETE FROM TR_Info', []],
    ['INSERT INTO TR_Info (infoID, server, location, locationDescription, decimals) VALUES (1, ?, ?, ?, ?)',
      [server, info.locationCode, info.locationDescription, parseInt(info.decimals, 10) || 0]],
  ];
  locations.forEach(location => statements.push([
    'INSERT OR REPLACE INTO TR_Locations (locationCode, locationDescription) VALUES (?, ?)',
    [location.locationCode, location.locationDescription]]));
  return executeInTransaction(statements);
};

// The destinations : the locations of the server, except the one we transfer from
const GetDestinations = (fromLocation) => query(
  'SELECT * FROM TR_Locations WHERE locationCode <> ? ORDER BY locationCode', [fromLocation]);

//#endregion

//#region Header

const GetHeader = async () => {
  const rows = await query('SELECT * FROM TR_Header WHERE headerID = 1');
  if (rows.length > 0) return rows[0];
  await execute('INSERT OR IGNORE INTO TR_Header (headerID) VALUES (1)');
  return { headerID: 1, multipleLocations: 0, destination: '', deliverBy: '', remarks: '', saveID: '' };
};

// The mode can only change while the transfer has no line (checked by the screen too)
const SetMultipleLocations = async (multiple) => {
  await GetHeader();
  const [{ lines }] = await query('SELECT COUNT(*) lines FROM TR_Lines');
  if (lines > 0) throw new Error('Clear the items of the transfer to change the way of work');
  await execute('UPDATE TR_Header SET multipleLocations = ? WHERE headerID = 1', [multiple ? 1 : 0]);
};

// One location : the destination of the transfer (it cannot change while the transfer has lines, checked by the screen)
const SetDestination = async (destination) => {
  await GetHeader();
  await execute('UPDATE TR_Header SET destination = ? WHERE headerID = 1', [destination]);
};

// The id of the save of this transfer : kept until the server answers, so a save sent again is not saved twice
const GetSaveID = async () => {
  const header = await GetHeader();
  if (header.saveID !== '') return header.saveID;
  const saveID = Date.now().toString(36).toUpperCase() + Math.floor(Math.random() * 1679616).toString(36).toUpperCase().padStart(4, '0');
  await execute('UPDATE TR_Header SET saveID = ? WHERE headerID = 1', [saveID]);
  return saveID;
};

// The transfer is saved on the server : its lines are removed, the next transfer gets a new save id
const CompleteSave = () => executeInTransaction([
  ['DELETE FROM TR_Lines', []],
  ["UPDATE TR_Header SET saveID = '', deliverBy = '', remarks = '' WHERE headerID = 1", []],
]);

//#endregion

//#region Lines

// { lines, lots, qty, destinations }
const GetSummary = async () => {
  const rows = await query(
    `SELECT COUNT(*) lines, COUNT(DISTINCT lotRecID) lots, IFNULL(SUM(qty), 0) qty, COUNT(DISTINCT destination) destinations
     FROM TR_Lines`);
  return rows[0];
};

// The line of a lot for a destination (null : none)
const GetLineID = async (lotRecID, destination) => {
  const rows = await query('SELECT lineID FROM TR_Lines WHERE lotRecID = ? AND destination = ?', [lotRecID, destination]);
  return rows.length > 0 ? rows[0].lineID : null;
};

// All the lines (sent by Save Transfer), in the order they were added
const GetAllLines = () => query('SELECT * FROM TR_Lines ORDER BY lineID');

// The items added last first : by the first line of the lot (a qty typed or a destination added does not move the item),
// searched by the item reference / description, the lot or the destination
const LINES_ORDER = 'ORDER BY ( SELECT MIN(L2.lineID) FROM TR_Lines L2 WHERE L2.lotRecID = TR_Lines.lotRecID ) DESC, lineID';

const GetLines = (searchText, limit) => {
  const text = (searchText ?? '').trim();
  if (text === '') return query(`SELECT * FROM TR_Lines ${LINES_ORDER} LIMIT ?`, [limit]);
  const part = '%' + likeText(upper(text)) + '%';
  return query(
    `SELECT * FROM TR_Lines
     WHERE UPPER(itemReference) LIKE ? ESCAPE '!' OR UPPER(itemDescription) LIKE ? ESCAPE '!'
           OR UPPER(lotBatch) LIKE ? ESCAPE '!' OR UPPER(destination) LIKE ? ESCAPE '!'
     ${LINES_ORDER} LIMIT ?`, [part, part, part, part, limit]);
};

// The qty of a lot in the transfer (all the destinations)
const GetLotTransferQty = async (lotRecID) => {
  const rows = await query('SELECT IFNULL(SUM(qty), 0) qty FROM TR_Lines WHERE lotRecID = ?', [lotRecID]);
  return rows[0].qty;
};

// The qty of every lot in the transfer : { [lotRecID]: qty }
const GetTransferQtys = async () => {
  const rows = await query('SELECT lotRecID, SUM(qty) qty FROM TR_Lines GROUP BY lotRecID');
  const qtys = {};
  rows.forEach(row => { qtys[row.lotRecID] = row.qty; });
  return qtys;
};

// The qty of a lot per destination : { [destination]: qty }
const GetLotQtys = async (lotRecID) => {
  const rows = await query('SELECT destination, qty FROM TR_Lines WHERE lotRecID = ?', [lotRecID]);
  const qtys = {};
  rows.forEach(row => { qtys[row.destination] = row.qty; });
  return qtys;
};

// The lots of the transfer
const GetLotRecIDs = async () => (await query('SELECT DISTINCT lotRecID FROM TR_Lines')).map(row => row.lotRecID);

// The available qty read from the server : { [lotRecID]: qty }, a lot not in it has none any more
const SetLotsAvailable = (available) => executeInTransaction([
  ['UPDATE TR_Lines SET lotAvailable = 0', []],
  ...Object.keys(available).map(lotRecID => ['UPDATE TR_Lines SET lotAvailable = ? WHERE lotRecID = ?', [available[lotRecID], lotRecID]]),
]);

// The available qty of one lot read from the server
const SetLotAvailable = (lotRecID, qty) => execute('UPDATE TR_Lines SET lotAvailable = ? WHERE lotRecID = ?', [qty, lotRecID]);

// item : { itemRecID, itemReference, itemDescription, itemUnit }
// lot : { lotRecID, lotExpiry, lotBatch, lotColor, lotGrid, lotSize, lotAvailable }
const lineInsert = (item, lot, destination, now) => [
  `INSERT OR IGNORE INTO TR_Lines (lotRecID, destination, itemRecID, itemReference, itemDescription, itemUnit,
                                   lotExpiry, lotBatch, lotColor, lotGrid, lotSize, priceLC, priceCC, lotAvailable, qty, state, updatedAt)
   VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, 0, 0, ?, 0, 1, ?)`,
  [lot.lotRecID, destination, item.itemRecID, item.itemReference, item.itemDescription, item.itemUnit,
    lot.lotExpiry, lot.lotBatch, lot.lotColor, lot.lotGrid, lot.lotSize, lot.lotAvailable, now]];

// The available qty just read for the lot, on all its lines
const lotAvailableUpdate = (lot) => ['UPDATE TR_Lines SET lotAvailable = ? WHERE lotRecID = ?', [lot.lotAvailable, lot.lotRecID]];

// One location : adds the qty to the line of the lot (created when there is none), returns its total
const AddLotQty = async (item, lot, destination, qty) => {
  const now = new Date().toISOString();
  await executeInTransaction([
    lineInsert(item, lot, destination, now),
    ['UPDATE TR_Lines SET qty = qty + ?, state = 1, updatedAt = ? WHERE lotRecID = ? AND destination = ?', [qty, now, lot.lotRecID, destination]],
    lotAvailableUpdate(lot),
  ]);
  const rows = await query('SELECT qty FROM TR_Lines WHERE lotRecID = ? AND destination = ?', [lot.lotRecID, destination]);
  return rows[0].qty;
};

// One location : adds the qty to the line of each lot (the items checked in the stock search)
// rows : [{ item, lot }]
const AddLotsQty = (rows, destination, qty) => {
  const now = new Date().toISOString();
  const statements = [];
  rows.forEach(({ item, lot }) => {
    statements.push(lineInsert(item, lot, destination, now));
    statements.push(['UPDATE TR_Lines SET qty = qty + ?, state = 1, updatedAt = ? WHERE lotRecID = ? AND destination = ?', [qty, now, lot.lotRecID, destination]]);
    statements.push(lotAvailableUpdate(lot));
  });
  return executeInTransaction(statements);
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
  statements.push(lotAvailableUpdate(lot));
  return executeInTransaction(statements);
};

const SetLineQty = (lineID, qty) => execute(
  'UPDATE TR_Lines SET qty = ?, state = 1, updatedAt = ? WHERE lineID = ?', [qty, new Date().toISOString(), lineID]);

const DeleteLine = (lineID) => execute('DELETE FROM TR_Lines WHERE lineID = ?', [lineID]);

// Multiple locations : the item (its lot) is removed from every location
const DeleteLot = (lotRecID) => execute('DELETE FROM TR_Lines WHERE lotRecID = ?', [lotRecID]);

// Clears the items : the way of work can be changed again (a new transfer, a new save id)
const ClearLines = () => executeInTransaction([
  ['DELETE FROM TR_Lines', []],
  ["UPDATE TR_Header SET saveID = '' WHERE headerID = 1", []],
]);

//#endregion

export default {
  GetLineID,
  GetInfo,
  SaveServerInfo,
  GetDestinations,
  GetHeader,
  SetMultipleLocations,
  SetDestination,
  GetSaveID,
  CompleteSave,
  GetSummary,
  GetAllLines,
  GetLines,
  GetLotTransferQty,
  GetTransferQtys,
  GetLotQtys,
  GetLotRecIDs,
  SetLotsAvailable,
  SetLotAvailable,
  AddLotQty,
  AddLotsQty,
  SetLotQtys,
  SetLineQty,
  DeleteLine,
  DeleteLot,
  ClearLines,
}
