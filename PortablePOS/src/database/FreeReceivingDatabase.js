import { enablePromise, openDatabase } from 'react-native-sqlite-storage';

// Local storage of Free Receiving (works offline)
// RC_Info : one row, the download of the stock file (counts, resume points)
// RC_Items / RC_Codes / RC_Suppliers : the downloaded stock file, replaced by every new download
// RC_Lines : the collected items, kept by a new download (they keep their own copy of the item reference and description)
// Quantities are stored as whole thousandths (received 12.5 => 12500), so totals never drift

const dbName = 'freeReceiving.db';

enablePromise(true);

const schemaVersion = 1;

// Kept as a promise so screens loading at the same time share one connection
let lcDBPromise = null;

const getDB = () => {
  if (!lcDBPromise) {
    lcDBPromise = (async () => {
      const db = await openDatabase({ name: dbName, location: 'default' });
      await createTables(db);
      return db;
    })().catch(error => {
      lcDBPromise = null;
      throw error;
    });
  }
  return lcDBPromise;
};

const createTables = async (db) => {
  await db.executeSql(`CREATE TABLE IF NOT EXISTS RC_Info (
                         infoID INTEGER PRIMARY KEY,
                         server TEXT NOT NULL,
                         downloadDate TEXT,
                         decimals INTEGER,
                         itemsCount INTEGER,
                         barcodesCount INTEGER,
                         supplierRefsCount INTEGER,
                         suppliersCount INTEGER,
                         itemsStored INTEGER NOT NULL DEFAULT 0,
                         barcodesStored INTEGER NOT NULL DEFAULT 0,
                         supplierRefsStored INTEGER NOT NULL DEFAULT 0,
                         suppliersStored INTEGER NOT NULL DEFAULT 0,
                         lastItemRecID TEXT NOT NULL DEFAULT '',
                         lastBarcodeRecID TEXT NOT NULL DEFAULT '',
                         lastSupplierRefRecID TEXT NOT NULL DEFAULT '',
                         complete INTEGER NOT NULL DEFAULT 0 )`);

  await db.executeSql(`CREATE TABLE IF NOT EXISTS RC_Items (
                         itemRecID TEXT PRIMARY KEY,
                         itemReference TEXT,
                         itemReferenceUpper TEXT,
                         itemDescription TEXT,
                         itemDescriptionUpper TEXT,
                         itemSupplier TEXT,
                         itemCategory TEXT,
                         itemSerie TEXT,
                         itemSingleRef1 TEXT,
                         itemSingleRef1Upper TEXT,
                         itemSingleRef2 TEXT,
                         itemSingleRef2Upper TEXT,
                         itemUnit TEXT )`);
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_RC_Items_Reference ON RC_Items (itemReferenceUpper)');
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_RC_Items_Supplier ON RC_Items (itemSupplier, itemReferenceUpper)');
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_RC_Items_Category ON RC_Items (itemCategory, itemReferenceUpper)');
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_RC_Items_Serie ON RC_Items (itemSerie)');
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_RC_Items_SingleRef1 ON RC_Items (itemSingleRef1Upper)');
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_RC_Items_SingleRef2 ON RC_Items (itemSingleRef2Upper)');

  // codeKind : barcode | supplierRef
  await db.executeSql(`CREATE TABLE IF NOT EXISTS RC_Codes (
                         codeKind TEXT NOT NULL,
                         codeRecID TEXT NOT NULL,
                         itemRecID TEXT NOT NULL,
                         codeValue TEXT,
                         codeValueUpper TEXT,
                         supplierRecID TEXT,
                         PRIMARY KEY (codeKind, codeRecID) )`);
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_RC_Codes_Value ON RC_Codes (codeKind, codeValueUpper)');
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_RC_Codes_Item ON RC_Codes (itemRecID)');

  await db.executeSql(`CREATE TABLE IF NOT EXISTS RC_Suppliers (
                         supplierRecID TEXT PRIMARY KEY,
                         supplierCode TEXT,
                         supplierName TEXT )`);

  // One line per supplier + invoice + item + type + linked item, a new scan adds its qty to it
  // receiveType : N normal, P promotion, F free - linkedItemRecID : the item a promotion is linked to ('' otherwise)
  // state : 1 not uploaded, 2 uploaded (the upload comes later)
  await db.executeSql(`CREATE TABLE IF NOT EXISTS RC_Lines (
                         lineID INTEGER PRIMARY KEY AUTOINCREMENT,
                         supplierRecID TEXT NOT NULL,
                         supplierCode TEXT,
                         supplierName TEXT,
                         invoiceNumber TEXT NOT NULL DEFAULT '',
                         itemRecID TEXT NOT NULL,
                         itemReference TEXT,
                         itemDescription TEXT,
                         itemUnit TEXT,
                         receiveType TEXT NOT NULL DEFAULT 'N',
                         linkedItemRecID TEXT NOT NULL DEFAULT '',
                         linkedItemReference TEXT,
                         linkedItemDescription TEXT,
                         qty INTEGER NOT NULL DEFAULT 0,
                         state INTEGER NOT NULL DEFAULT 1,
                         updatedAt TEXT )`);
  await db.executeSql(`CREATE UNIQUE INDEX IF NOT EXISTS IX_RC_Lines_Key
                         ON RC_Lines (supplierRecID, invoiceNumber, itemRecID, receiveType, linkedItemRecID)`);

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

//#region Download

const GetInfo = async () => {
  const rows = await query('SELECT * FROM RC_Info WHERE infoID = 1');
  return rows.length > 0 ? rows[0] : null;
};

// Starts a new download : the previous stock file is replaced, the collected lines are kept
const StartDownload = (server, info) => executeInTransaction([
  ['DELETE FROM RC_Codes', []],
  ['DELETE FROM RC_Items', []],
  ['DELETE FROM RC_Suppliers', []],
  ['DELETE FROM RC_Info', []],
  [`INSERT INTO RC_Info (infoID, server, decimals, itemsCount, barcodesCount, supplierRefsCount, suppliersCount)
    VALUES (1, ?, ?, ?, ?, ?, ?)`,
    [server, parseInt(info.decimals, 10) || 0, info.itemsCount, info.barcodesCount, info.supplierRefsCount, info.suppliersCount]],
]);

// Stores one page of items and moves the resume point to its last item
const SaveItemsPage = (items) => {
  const statements = items.map(item => [
    `INSERT OR REPLACE INTO RC_Items (itemRecID, itemReference, itemReferenceUpper, itemDescription, itemDescriptionUpper,
                                      itemSupplier, itemCategory, itemSerie, itemSingleRef1, itemSingleRef1Upper,
                                      itemSingleRef2, itemSingleRef2Upper, itemUnit)
     VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)`,
    [item.itemRecID, item.itemReference, upper(item.itemReference), item.itemDescription, upper(item.itemDescription),
      upper(item.itemSupplier), upper(item.itemCategory), item.itemSerie, item.itemSingleRef1, upper(item.itemSingleRef1),
      item.itemSingleRef2, upper(item.itemSingleRef2), item.itemUnit]]);
  statements.push([
    'UPDATE RC_Info SET itemsStored = (SELECT COUNT(*) FROM RC_Items), lastItemRecID = ? WHERE infoID = 1',
    [items[items.length - 1].itemRecID]]);
  return executeInTransaction(statements);
};

// codeKind : barcode | supplierRef
const SaveCodesPage = (codeKind, codes) => {
  const statements = codes.map(code => [
    'INSERT OR REPLACE INTO RC_Codes (codeKind, codeRecID, itemRecID, codeValue, codeValueUpper, supplierRecID) VALUES (?, ?, ?, ?, ?, ?)',
    [codeKind, code.codeRecID, code.itemRecID, code.codeValue, upper(code.codeValue), code.supplierRecID]]);
  const [storedColumn, lastColumn] = codeKind === 'barcode' ? ['barcodesStored', 'lastBarcodeRecID'] : ['supplierRefsStored', 'lastSupplierRefRecID'];
  statements.push([
    `UPDATE RC_Info SET ${storedColumn} = (SELECT COUNT(*) FROM RC_Codes WHERE codeKind = ?), ${lastColumn} = ? WHERE infoID = 1`,
    [codeKind, codes[codes.length - 1].codeRecID]]);
  return executeInTransaction(statements);
};

const SaveSuppliers = (suppliers) => {
  const statements = [['DELETE FROM RC_Suppliers', []]];
  suppliers.forEach(supplier => statements.push([
    'INSERT OR REPLACE INTO RC_Suppliers (supplierRecID, supplierCode, supplierName) VALUES (?, ?, ?)',
    [supplier.supplierRecID, supplier.supplierCode, supplier.supplierName]]));
  statements.push(['UPDATE RC_Info SET suppliersStored = (SELECT COUNT(*) FROM RC_Suppliers) WHERE infoID = 1', []]);
  return executeInTransaction(statements);
};

const CompleteDownload = (downloadDate) => execute(
  'UPDATE RC_Info SET complete = 1, downloadDate = ? WHERE infoID = 1', [downloadDate]);

//#endregion

//#region Stock file

const GetItem = async (itemRecID) => {
  const rows = await query('SELECT * FROM RC_Items WHERE itemRecID = ?', [itemRecID]);
  return rows.length > 0 ? rows[0] : null;
};

// Barcodes of the item (shown on the scan screen)
const GetItemBarcodes = async (itemRecID) => {
  const rows = await query("SELECT codeValue FROM RC_Codes WHERE codeKind = 'barcode' AND itemRecID = ? ORDER BY codeRecID", [itemRecID]);
  return rows.map(row => row.codeValue);
};

const ITEMS_ORDER = 'ORDER BY itemReferenceUpper LIMIT ?';

const itemsWithCode = (codeKind, condition) =>
  `SELECT * FROM RC_Items WHERE itemRecID IN (SELECT itemRecID FROM RC_Codes WHERE codeKind = '${codeKind}' AND ${condition})`;

// Items of a search field (see FreeReceivingRules.SEARCH_FIELDS), 'any' : reference, description, single references and codes
// exact = true : the whole value, false : the start of it (the description : any part of it)
const FindItems = (field, text, exact, limit) => {
  const value = upper((text ?? '').trim());
  const prefix = likeText(value) + '%';
  const part = '%' + likeText(value) + '%';
  const compare = exact ? '= ?' : "LIKE ? ESCAPE '!'";
  const param = exact ? value : prefix;

  switch (field) {
    case 'barcode':
      return query(
        `SELECT * FROM RC_Items
         WHERE itemSingleRef1Upper ${compare} OR itemSingleRef2Upper ${compare}
               OR itemRecID IN (SELECT itemRecID FROM RC_Codes WHERE codeKind = 'barcode' AND codeValueUpper ${compare})
         ${ITEMS_ORDER}`, [param, param, param, limit]);
    case 'supplierRef':
      return query(`${itemsWithCode('supplierRef', `codeValueUpper ${compare}`)} ${ITEMS_ORDER}`, [param, limit]);
    case 'serie':
      return query(`SELECT * FROM RC_Items WHERE itemSerie ${compare} ${ITEMS_ORDER}`, [param, limit]);
    case 'supplier':
      return query(`SELECT * FROM RC_Items WHERE itemSupplier ${compare} ${ITEMS_ORDER}`, [param, limit]);
    case 'category':
      return query(`SELECT * FROM RC_Items WHERE itemCategory ${compare} ${ITEMS_ORDER}`, [param, limit]);
    case 'reference':
      return query(`SELECT * FROM RC_Items WHERE itemReferenceUpper ${compare} ${ITEMS_ORDER}`, [param, limit]);
    case 'description':
      // the description is always searched by any part of it
      return query(`SELECT * FROM RC_Items WHERE itemDescriptionUpper LIKE ? ESCAPE '!' ${ITEMS_ORDER}`, [part, limit]);
    default:
      if (value === '') return query(`SELECT * FROM RC_Items ${ITEMS_ORDER}`, [limit]);
      return query(
        `SELECT * FROM RC_Items
         WHERE itemReferenceUpper LIKE ? ESCAPE '!' OR itemDescriptionUpper LIKE ? ESCAPE '!'
               OR itemSingleRef1Upper LIKE ? ESCAPE '!' OR itemSingleRef2Upper LIKE ? ESCAPE '!'
               OR itemRecID IN (SELECT itemRecID FROM RC_Codes WHERE codeValueUpper LIKE ? ESCAPE '!')
         ${ITEMS_ORDER}`, [prefix, part, prefix, prefix, prefix, limit]);
  }
};

const GetSuppliers = () => query('SELECT * FROM RC_Suppliers ORDER BY supplierCode');

const GetSupplier = async (supplierRecID) => {
  const rows = await query('SELECT * FROM RC_Suppliers WHERE supplierRecID = ?', [supplierRecID]);
  return rows.length > 0 ? rows[0] : null;
};

//#endregion

//#region Collected lines

// { lines, qty, pending } of all the collected lines
const GetLinesSummary = async () => {
  const rows = await query(
    'SELECT COUNT(*) lines, IFNULL(SUM(qty), 0) qty, IFNULL(SUM(CASE WHEN state = 1 THEN 1 ELSE 0 END), 0) pending FROM RC_Lines');
  return rows[0];
};

// Qty already received of the item on this supplier + invoice (all the types)
const GetReceivedQty = async (supplierRecID, invoiceNumber, itemRecID) => {
  const rows = await query(
    'SELECT IFNULL(SUM(qty), 0) qty FROM RC_Lines WHERE supplierRecID = ? AND invoiceNumber = ? AND itemRecID = ?',
    [supplierRecID, invoiceNumber, itemRecID]);
  return rows[0].qty;
};

// Adds the qty to the line of the same supplier + invoice + item + type + linked item (created when there is none)
// receipt : { supplier, invoiceNumber, item, receiveType, linkedItem (or null), qty }
// Returns the total of the line
const AddReceivedQty = async (receipt) => {
  const linkedItemRecID = receipt.linkedItem ? receipt.linkedItem.itemRecID : '';
  const key = [receipt.supplier.supplierRecID, receipt.invoiceNumber, receipt.item.itemRecID, receipt.receiveType, linkedItemRecID];
  const now = new Date().toISOString();

  await executeInTransaction([
    [`INSERT OR IGNORE INTO RC_Lines (supplierRecID, supplierCode, supplierName, invoiceNumber, itemRecID, itemReference, itemDescription, itemUnit,
                                      receiveType, linkedItemRecID, linkedItemReference, linkedItemDescription, qty, state, updatedAt)
      VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, 0, 1, ?)`,
      [receipt.supplier.supplierRecID, receipt.supplier.supplierCode, receipt.supplier.supplierName, receipt.invoiceNumber,
        receipt.item.itemRecID, receipt.item.itemReference, receipt.item.itemDescription, receipt.item.itemUnit,
        receipt.receiveType, linkedItemRecID, receipt.linkedItem?.itemReference ?? '', receipt.linkedItem?.itemDescription ?? '', now]],
    [`UPDATE RC_Lines SET qty = qty + ?, state = 1, updatedAt = ?
      WHERE supplierRecID = ? AND invoiceNumber = ? AND itemRecID = ? AND receiveType = ? AND linkedItemRecID = ?`,
      [receipt.qty, now, ...key]],
  ]);

  const rows = await query(
    'SELECT qty FROM RC_Lines WHERE supplierRecID = ? AND invoiceNumber = ? AND itemRecID = ? AND receiveType = ? AND linkedItemRecID = ?', key);
  return rows[0].qty;
};

// The last lines first, searched by the item, the linked item, the supplier or the invoice
const GetLines = (searchText, limit) => {
  const text = (searchText ?? '').trim();
  if (text === '') return query('SELECT * FROM RC_Lines ORDER BY updatedAt DESC, lineID DESC LIMIT ?', [limit]);
  const part = '%' + likeText(upper(text)) + '%';
  return query(
    `SELECT * FROM RC_Lines
     WHERE UPPER(itemReference) LIKE ? ESCAPE '!' OR UPPER(itemDescription) LIKE ? ESCAPE '!'
           OR UPPER(linkedItemReference) LIKE ? ESCAPE '!' OR UPPER(supplierCode) LIKE ? ESCAPE '!'
           OR UPPER(supplierName) LIKE ? ESCAPE '!' OR UPPER(invoiceNumber) LIKE ? ESCAPE '!'
     ORDER BY updatedAt DESC, lineID DESC LIMIT ?`, [part, part, part, part, part, part, limit]);
};

// The qty as typed (replaces the total of the line)
const SetLineQty = (lineID, qty) => execute(
  'UPDATE RC_Lines SET qty = ?, state = 1, updatedAt = ? WHERE lineID = ?', [qty, new Date().toISOString(), lineID]);

const DeleteLine = (lineID) => execute('DELETE FROM RC_Lines WHERE lineID = ?', [lineID]);

const DeleteAllLines = () => execute('DELETE FROM RC_Lines');

//#endregion

export default {
  GetInfo,
  StartDownload,
  SaveItemsPage,
  SaveCodesPage,
  SaveSuppliers,
  CompleteDownload,
  GetItem,
  GetItemBarcodes,
  FindItems,
  GetSuppliers,
  GetSupplier,
  GetLinesSummary,
  GetReceivedQty,
  AddReceivedQty,
  GetLines,
  SetLineQty,
  DeleteLine,
  DeleteAllLines,
}
