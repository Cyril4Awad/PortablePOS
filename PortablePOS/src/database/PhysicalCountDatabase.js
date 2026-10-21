import { enablePromise, openDatabase } from 'react-native-sqlite-storage';

// Local storage of the physical count (works offline, like the old ERPDataCollector app)
// One downloaded count per server + location (the same location code can exist on several servers)
// header + lines + barcodes, all keyed by server + location
// Quantities are stored as whole thousandths (counted 12.5 => 12500), so totals never drift

const dbName = 'physicalCount.db';

// Database.js also enables it, but with inline requires it is only loaded when another screen uses it
enablePromise(true);

// Version 2 : the count is keyed by server + location (version 1 was keyed by location only)
// Version 3 : trading / fashion / expiry / book modes, fashion fields, available qty only with the privilege
// Version 4 : book title of the lines (added to the existing table, the downloaded counts are kept)
const schemaVersion = 4;

// Kept as a promise so screens loading at the same time share one connection
let lcDBPromise = null;

const getDB = () => {
  if (!lcDBPromise) {
    lcDBPromise = (async () => {
      const db = await openDatabase({ name: dbName, location: 'default' });
      await upgradeTables(db);
      await createTables(db);
      return db;
    })().catch(error => {
      lcDBPromise = null;
      throw error;
    });
  }
  return lcDBPromise;
};

// Before version 3 the downloaded counts are dropped (they can be downloaded again), the profiles are kept
// From version 3 the new columns are added, the downloaded counts are kept
const upgradeTables = async (db) => {
  const [result] = await db.executeSql('PRAGMA user_version');
  const version = result.rows.item(0).user_version;
  if (version >= schemaVersion) return;

  if (version < 3) {
    await db.executeSql('DROP TABLE IF EXISTS PC_Barcodes');
    await db.executeSql('DROP TABLE IF EXISTS PC_Lines');
    await db.executeSql('DROP TABLE IF EXISTS PC_Header');
  }
  else {
    const [columns] = await db.executeSql('PRAGMA table_info(PC_Lines)');
    const names = [];
    for (let index = 0; index < columns.rows.length; index++) names.push(columns.rows.item(index).name);
    if (names.length > 0 && !names.includes('bookTitle')) {
      await db.executeSql('ALTER TABLE PC_Lines ADD COLUMN bookTitle TEXT');
    }
  }

  // Scratch table of the first scan for testing (it now counts on the downloaded count)
  await db.executeSql('DROP TABLE IF EXISTS lab_lines');
  await db.executeSql(`PRAGMA user_version = ${schemaVersion}`);
};

const createTables = async (db) => {
  await db.executeSql(`CREATE TABLE IF NOT EXISTS PC_Profiles (
                         profileID INTEGER PRIMARY KEY AUTOINCREMENT,
                         location TEXT NOT NULL,
                         server TEXT NOT NULL,
                         selected INTEGER NOT NULL DEFAULT 0 )`);

  await db.executeSql(`CREATE TABLE IF NOT EXISTS PC_Header (
                         server TEXT NOT NULL,
                         location TEXT NOT NULL,
                         machineName TEXT NOT NULL,
                         countNumber TEXT NOT NULL,
                         locationDescription TEXT,
                         warehouse TEXT,
                         dateCount TEXT,
                         userToUpdate INTEGER,
                         decimals INTEGER,
                         tradingMode TEXT,
                         fashionMode TEXT,
                         expiryMode TEXT,
                         bookMode TEXT,
                         showAvailableQty TEXT,
                         linesCount INTEGER,
                         barcodesCount INTEGER,
                         linesStored INTEGER NOT NULL DEFAULT 0,
                         barcodesStored INTEGER NOT NULL DEFAULT 0,
                         complete INTEGER NOT NULL DEFAULT 0,
                         PRIMARY KEY (server, location) )`);

  // state : 0 not counted, 1 counted and not uploaded, 2 uploaded
  await db.executeSql(`CREATE TABLE IF NOT EXISTS PC_Lines (
                         server TEXT NOT NULL,
                         location TEXT NOT NULL,
                         lineRecID TEXT NOT NULL,
                         rowNumber INTEGER NOT NULL,
                         itemRecID TEXT,
                         itemReference TEXT,
                         itemReferenceUpper TEXT,
                         itemDescription TEXT,
                         colorCode TEXT,
                         colorDescription TEXT,
                         gridCode TEXT,
                         gridDescription TEXT,
                         sizeOrder TEXT,
                         sizeDescription TEXT,
                         bookTitle TEXT,
                         expiry TEXT,
                         batch TEXT,
                         systemQty INTEGER,
                         counted INTEGER NOT NULL DEFAULT 0,
                         state INTEGER NOT NULL DEFAULT 0,
                         PRIMARY KEY (server, location, lineRecID) )`);
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_PC_Lines_State ON PC_Lines (server, location, state, rowNumber)');
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_PC_Lines_Reference ON PC_Lines (server, location, itemReferenceUpper)');
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_PC_Lines_Qr ON PC_Lines (server, location, itemRecID, expiry, batch)');

  await db.executeSql(`CREATE TABLE IF NOT EXISTS PC_Barcodes (
                         server TEXT NOT NULL,
                         location TEXT NOT NULL,
                         rowNumber INTEGER NOT NULL,
                         barcode TEXT NOT NULL,
                         lineRecID TEXT NOT NULL,
                         PRIMARY KEY (server, location, rowNumber) )`);
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_PC_Barcodes_Barcode ON PC_Barcodes (server, location, barcode)');
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

//#region Profiles

const GetProfiles = () => query('SELECT * FROM PC_Profiles ORDER BY location, server');

const GetSelectedProfile = async () => {
  const rows = await query('SELECT * FROM PC_Profiles WHERE selected = 1 LIMIT 1');
  return rows.length > 0 ? rows[0] : null;
};

// profileID 0 = new profile
const SaveProfile = async (profileID, location, server, selected) => {
  const statements = [];
  if (selected) {
    statements.push(['UPDATE PC_Profiles SET selected = 0', []]);
  }
  if (profileID === 0) {
    statements.push(['INSERT INTO PC_Profiles (location, server, selected) VALUES (?, ?, ?)', [location, server, selected ? 1 : 0]]);
  }
  else {
    statements.push(['UPDATE PC_Profiles SET location = ?, server = ?, selected = ? WHERE profileID = ?', [location, server, selected ? 1 : 0, profileID]]);
  }
  await executeInTransaction(statements);
};

const SelectProfile = (profileID) => executeInTransaction([
  ['UPDATE PC_Profiles SET selected = 0', []],
  ['UPDATE PC_Profiles SET selected = 1 WHERE profileID = ?', [profileID]],
]);

// The downloaded count of the profile is deleted with it
const DeleteProfile = (profile) => executeInTransaction([
  ['DELETE FROM PC_Barcodes WHERE server = ? AND location = ?', [profile.server, profile.location]],
  ['DELETE FROM PC_Lines WHERE server = ? AND location = ?', [profile.server, profile.location]],
  ['DELETE FROM PC_Header WHERE server = ? AND location = ?', [profile.server, profile.location]],
  ['DELETE FROM PC_Profiles WHERE profileID = ?', [profile.profileID]],
]);

//#endregion

// countKey : any object with server and location (a profile or a downloaded header)

//#region Header and download

const GetHeader = async (countKey) => {
  const rows = await query('SELECT * FROM PC_Header WHERE server = ? AND location = ?', [countKey.server, countKey.location]);
  return rows.length > 0 ? rows[0] : null;
};

// Lines per state of the count : { notCounted, pending, uploaded, countedQty }
// countedQty : total qty of the counted lines (uploaded or not), thousandths
const GetLinesSummary = async (countKey) => {
  const rows = await query(
    'SELECT state, COUNT(*) total, IFNULL(SUM(counted), 0) qty FROM PC_Lines WHERE server = ? AND location = ? GROUP BY state',
    [countKey.server, countKey.location]);
  const summary = { notCounted: 0, pending: 0, uploaded: 0, countedQty: 0 };
  rows.forEach(row => {
    if (row.state === 0) summary.notCounted = row.total;
    if (row.state === 1) summary.pending = row.total;
    if (row.state === 2) summary.uploaded = row.total;
    if (row.state !== 0) summary.countedQty += row.qty;
  });
  return summary;
};

// Starts a new download : the previous count of the server + location is replaced
const StartDownload = (countKey, machineName, header) => executeInTransaction([
  ['DELETE FROM PC_Barcodes WHERE server = ? AND location = ?', [countKey.server, countKey.location]],
  ['DELETE FROM PC_Lines WHERE server = ? AND location = ?', [countKey.server, countKey.location]],
  ['DELETE FROM PC_Header WHERE server = ? AND location = ?', [countKey.server, countKey.location]],
  [`INSERT INTO PC_Header (server, location, machineName, countNumber, locationDescription, warehouse, dateCount,
                           userToUpdate, decimals, tradingMode, fashionMode, expiryMode, bookMode, showAvailableQty,
                           linesCount, barcodesCount)
    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)`,
    [countKey.server, countKey.location, machineName, header.countNumber, header.locationDescription, header.warehouse, header.dateCount,
      header.userToUpdate, header.decimals, header.tradingMode, header.fashionMode, header.expiryMode, header.bookMode, header.showAvailableQty,
      header.linesCount, header.barcodesCount]],
]);

// Stores one page of lines and moves the resume point
// systemQty is not sent by the server when the available qty is hidden, it is stored as NULL
const SaveLinesPage = (countKey, lines, toMilli) => {
  const statements = lines.map(line => [
    `INSERT OR REPLACE INTO PC_Lines (server, location, lineRecID, rowNumber, itemRecID, itemReference, itemReferenceUpper,
                                      itemDescription, colorCode, colorDescription, gridCode, gridDescription, sizeOrder, sizeDescription,
                                      bookTitle, expiry, batch, systemQty, counted, state)
     VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, 0)`,
    [countKey.server, countKey.location, line.lineRecID, line.rowNumber, line.itemRecID, line.itemReference, line.itemReference.toUpperCase(),
      line.itemDescription, line.colorCode, line.colorDescription, line.gridCode, line.gridDescription, line.sizeOrder, line.sizeDescription,
      line.bookTitle ?? '', line.expiry, line.batch, line.systemQty === undefined ? null : toMilli(line.systemQty), toMilli(line.counted)]]);
  statements.push([
    'UPDATE PC_Header SET linesStored = (SELECT COUNT(*) FROM PC_Lines WHERE server = ? AND location = ?) WHERE server = ? AND location = ?',
    [countKey.server, countKey.location, countKey.server, countKey.location]]);
  return executeInTransaction(statements);
};

const SaveBarcodesPage = (countKey, barcodes) => {
  const statements = barcodes.map(barcode => [
    'INSERT OR REPLACE INTO PC_Barcodes (server, location, rowNumber, barcode, lineRecID) VALUES (?, ?, ?, ?, ?)',
    [countKey.server, countKey.location, barcode.rowNumber, barcode.barcode, barcode.lineRecID]]);
  statements.push([
    'UPDATE PC_Header SET barcodesStored = (SELECT COUNT(*) FROM PC_Barcodes WHERE server = ? AND location = ?) WHERE server = ? AND location = ?',
    [countKey.server, countKey.location, countKey.server, countKey.location]]);
  return executeInTransaction(statements);
};

const CompleteDownload = (countKey) => execute(
  'UPDATE PC_Header SET complete = 1 WHERE server = ? AND location = ?', [countKey.server, countKey.location]);

//#endregion

//#region Scan lookup

const GetLine = async (countKey, lineRecID) => {
  const rows = await query(
    'SELECT * FROM PC_Lines WHERE server = ? AND location = ? AND lineRecID = ?',
    [countKey.server, countKey.location, lineRecID]);
  return rows.length > 0 ? rows[0] : null;
};

// The lines of a barcode : a barcode can belong to more than one item (or lot), the user then chooses the line
const FindLinesByBarcode = (countKey, barcode, limit) => query(
  `SELECT * FROM PC_Lines
   WHERE server = ? AND location = ?
         AND lineRecID IN (SELECT lineRecID FROM PC_Barcodes WHERE server = ? AND location = ? AND barcode = ?)
   ORDER BY rowNumber LIMIT ?`,
  [countKey.server, countKey.location, countKey.server, countKey.location, barcode, limit]);

// Lines whose reference starts with the text (case insensitive, % and _ are plain characters)
const FindLinesByReferencePrefix = (countKey, prefix, limit) => query(
  `SELECT * FROM PC_Lines
   WHERE server = ? AND location = ? AND substr(itemReferenceUpper, 1, ?) = ?
   ORDER BY rowNumber LIMIT ?`,
  [countKey.server, countKey.location, prefix.length, prefix.toUpperCase(), limit]);

// Typed search of the item list : the start of the reference, or any part of the description or of the book title
// (% and _ are plain characters)
const FindLinesBySearch = (countKey, text, limit) => {
  const value = text.toUpperCase().replace(/[!%_]/g, character => '!' + character);
  return query(
    `SELECT * FROM PC_Lines
     WHERE server = ? AND location = ?
           AND (itemReferenceUpper LIKE ? ESCAPE '!' OR UPPER(itemDescription) LIKE ? ESCAPE '!' OR UPPER(bookTitle) LIKE ? ESCAPE '!')
     ORDER BY rowNumber LIMIT ?`,
    [countKey.server, countKey.location, value + '%', '%' + value + '%', '%' + value + '%', limit]);
};

// The lines of an item (the link of a label QR) : one line per expiry / batch
const FindLinesByItem = (countKey, itemRecID, limit) => query(
  'SELECT * FROM PC_Lines WHERE server = ? AND location = ? AND itemRecID = ? ORDER BY rowNumber LIMIT ?',
  [countKey.server, countKey.location, itemRecID, limit]);

const FindLineByQr = async (countKey, itemRecID, expiry, batch) => {
  const rows = await query(
    'SELECT lineRecID FROM PC_Lines WHERE server = ? AND location = ? AND itemRecID = ? AND expiry = ? AND batch = ? ORDER BY rowNumber LIMIT 1',
    [countKey.server, countKey.location, itemRecID, expiry, batch]);
  return rows.length > 0 ? rows[0].lineRecID : null;
};

// Every change of the total marks the line as counted and not uploaded
const SaveLineCounted = (countKey, lineRecID, counted) => execute(
  'UPDATE PC_Lines SET counted = ?, state = 1 WHERE server = ? AND location = ? AND lineRecID = ?',
  [counted, countKey.server, countKey.location, lineRecID]);

// Undo : puts back the total and the state the line had before the last entry
const RestoreLineCounted = (countKey, lineRecID, counted, state) => execute(
  'UPDATE PC_Lines SET counted = ?, state = ? WHERE server = ? AND location = ? AND lineRecID = ?',
  [counted, state, countKey.server, countKey.location, lineRecID]);

//#endregion

//#region Items and upload

// counted = true : counted lines, uploaded or not (state 1 and 2, the line shows which), false : not counted lines (state 0)
// Uploaded lines stay in the counted list, scanning one again only puts it back to "to upload"
const GetLines = (countKey, counted, searchText) => {
  const stateCondition = counted ? 'state <> 0' : 'state = 0';
  if (searchText && searchText.trim() !== '') {
    const text = `%${searchText.trim().toUpperCase()}%`;
    return query(
      `SELECT * FROM PC_Lines
       WHERE server = ? AND location = ? AND ${stateCondition} AND (itemReferenceUpper LIKE ? OR UPPER(itemDescription) LIKE ?)
       ORDER BY rowNumber LIMIT 500`,
      [countKey.server, countKey.location, text, text]);
  }
  return query(
    `SELECT * FROM PC_Lines WHERE server = ? AND location = ? AND ${stateCondition} ORDER BY rowNumber LIMIT 500`,
    [countKey.server, countKey.location]);
};

const GetPendingLines = (countKey) => query(
  'SELECT lineRecID, counted FROM PC_Lines WHERE server = ? AND location = ? AND state = 1 ORDER BY rowNumber',
  [countKey.server, countKey.location]);

const MarkLinesUploaded = (countKey, lineRecIDs) => executeInTransaction(
  lineRecIDs.map(lineRecID => [
    'UPDATE PC_Lines SET state = 2 WHERE server = ? AND location = ? AND lineRecID = ? AND state = 1',
    [countKey.server, countKey.location, lineRecID]]));

// Sends again every uploaded line on the next upload
const RestoreUploaded = (countKey) => execute(
  'UPDATE PC_Lines SET state = 1 WHERE server = ? AND location = ? AND state = 2', [countKey.server, countKey.location]);

//#endregion

export default {
  GetProfiles,
  GetSelectedProfile,
  SaveProfile,
  SelectProfile,
  DeleteProfile,
  GetHeader,
  GetLinesSummary,
  StartDownload,
  SaveLinesPage,
  SaveBarcodesPage,
  CompleteDownload,
  GetLine,
  FindLinesByBarcode,
  FindLinesByReferencePrefix,
  FindLinesBySearch,
  FindLinesByItem,
  FindLineByQr,
  SaveLineCounted,
  RestoreLineCounted,
  GetLines,
  GetPendingLines,
  MarkLinesUploaded,
  RestoreUploaded,
}
