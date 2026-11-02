import { enablePromise, openDatabase } from 'react-native-sqlite-storage';

// Local storage of Free Receiving (works offline)
// RC_Info : one row, the download of the stock file (server + location, counts, resume points)
// RC_Items / RC_Codes / RC_Suppliers / RC_Lots : the downloaded stock file, replaced by every new download
// RC_Orders : the receiving orders prepared on the Receiving Order screens (the draft as it is) not uploaded yet, kept by a new download,
//             removed once uploaded. RC_OrderLines : their lines in rows (View Items, the totals)

const dbName = 'freeReceiving.db';

enablePromise(true);

// 2 : saved as a Receiving Order (location, item class, RO Ref #, expiry / lot, no promotion), the older tables are dropped
// 3 : the expiry / lot of the barcodes, the lots of the location (RC_Lots), the production date of a lot : the stock file is downloaded again,
//     the received lines are kept
// 4 : the receiving orders of the Receiving Order screens (RC_Orders), the columns of the Receiving Order items : all downloaded again
const schemaVersion = 4;

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
  const [version] = await db.executeSql('PRAGMA user_version');
  const currentVersion = version.rows.item(0).user_version;
  if (currentVersion < schemaVersion) {
    for (const table of ['RC_Info', 'RC_Items', 'RC_Codes', 'RC_Suppliers', 'RC_Lots', 'RC_Lines']) {
      await db.executeSql(`DROP TABLE IF EXISTS ${table}`);
    }
  }

  await db.executeSql(`CREATE TABLE IF NOT EXISTS RC_Info (
                         infoID INTEGER PRIMARY KEY,
                         server TEXT NOT NULL,
                         location TEXT NOT NULL DEFAULT '',
                         locationDescription TEXT NOT NULL DEFAULT '',
                         downloadDate TEXT,
                         decimals INTEGER,
                         currencyCC TEXT NOT NULL DEFAULT '',
                         itemsCount INTEGER,
                         barcodesCount INTEGER,
                         supplierRefsCount INTEGER,
                         suppliersCount INTEGER,
                         lotsCount INTEGER NOT NULL DEFAULT 0,
                         itemsStored INTEGER NOT NULL DEFAULT 0,
                         barcodesStored INTEGER NOT NULL DEFAULT 0,
                         supplierRefsStored INTEGER NOT NULL DEFAULT 0,
                         suppliersStored INTEGER NOT NULL DEFAULT 0,
                         lotsStored INTEGER NOT NULL DEFAULT 0,
                         lastItemRecID TEXT NOT NULL DEFAULT '',
                         lastBarcodeRecID TEXT NOT NULL DEFAULT '',
                         lastSupplierRefRecID TEXT NOT NULL DEFAULT '',
                         lastLotRecID TEXT NOT NULL DEFAULT '',
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
                         itemUnit TEXT,
                         itemClass TEXT,
                         itemSubUnit TEXT,
                         itemVatType TEXT,
                         itemCostCC REAL,
                         itemQtyControl TEXT,
                         itemMeasType TEXT,
                         itemConsFactor REAL,
                         itemHalted INTEGER,
                         itemBBFDate TEXT )`);
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_RC_Items_Reference ON RC_Items (itemReferenceUpper)');
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_RC_Items_Supplier ON RC_Items (itemSupplier, itemReferenceUpper)');
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_RC_Items_Category ON RC_Items (itemCategory, itemReferenceUpper)');
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_RC_Items_Serie ON RC_Items (itemSerie)');
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_RC_Items_SingleRef1 ON RC_Items (itemSingleRef1Upper)');
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_RC_Items_SingleRef2 ON RC_Items (itemSingleRef2Upper)');

  // codeKind : barcode | supplierRef, codeExpiry / codeBatch : the expiry and lot of a barcode ('' when it has none)
  await db.executeSql(`CREATE TABLE IF NOT EXISTS RC_Codes (
                         codeKind TEXT NOT NULL,
                         codeRecID TEXT NOT NULL,
                         itemRecID TEXT NOT NULL,
                         codeValue TEXT,
                         codeValueUpper TEXT,
                         supplierRecID TEXT,
                         codeExpiry TEXT NOT NULL DEFAULT '',
                         codeBatch TEXT NOT NULL DEFAULT '',
                         PRIMARY KEY (codeKind, codeRecID) )`);
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_RC_Codes_Value ON RC_Codes (codeKind, codeValueUpper)');
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_RC_Codes_Item ON RC_Codes (itemRecID)');

  await db.executeSql(`CREATE TABLE IF NOT EXISTS RC_Suppliers (
                         supplierRecID TEXT PRIMARY KEY,
                         supplierCode TEXT,
                         supplierName TEXT )`);

  // The lots of the expiry items (the lot window, GetROItemLots) : the location of the stock file with the qty available when downloaded,
  // and location 00 (the lots known, with their production date)
  await db.executeSql(`CREATE TABLE IF NOT EXISTS RC_Lots (
                         lotRecID TEXT PRIMARY KEY,
                         itemRecID TEXT NOT NULL,
                         locationCode TEXT,
                         lotExpiry TEXT,
                         lotBatch TEXT,
                         lotProduction TEXT,
                         lotAvailable REAL )`);
  await db.executeSql('CREATE INDEX IF NOT EXISTS IX_RC_Lots_Item ON RC_Lots (itemRecID)');

  // A receiving order saved on the device : draft = the receiving order of the screens (JSON), state 1 not uploaded, 2 uploaded (roRecID)
  await db.executeSql(`CREATE TABLE IF NOT EXISTS RC_Orders (
                         orderID INTEGER PRIMARY KEY AUTOINCREMENT,
                         supplierRecID TEXT,
                         supplierCode TEXT,
                         supplierName TEXT,
                         roRef TEXT,
                         roDate TEXT,
                         locationCode TEXT,
                         draft TEXT NOT NULL,
                         state INTEGER NOT NULL DEFAULT 1,
                         roRecID TEXT NOT NULL DEFAULT '',
                         savedAt TEXT )`);

  // The lines of the receiving orders in rows : the item, its status, qty and lots
  await db.executeSql(`CREATE TABLE IF NOT EXISTS RC_OrderLines (
                         orderID INTEGER NOT NULL,
                         lineNo INTEGER NOT NULL,
                         itemRecID TEXT,
                         itemReference TEXT,
                         itemDescription TEXT,
                         itemUnit TEXT,
                         lineStatus TEXT,
                         supplierRef TEXT,
                         qty REAL,
                         lotsText TEXT,
                         PRIMARY KEY (orderID, lineNo) )`);

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

// Starts a new download from the server of its location : the previous stock file is replaced, the received lines are kept
const StartDownload = (server, location, locationDescription, info) => executeInTransaction([
  ['DELETE FROM RC_Codes', []],
  ['DELETE FROM RC_Items', []],
  ['DELETE FROM RC_Suppliers', []],
  ['DELETE FROM RC_Lots', []],
  ['DELETE FROM RC_Info', []],
  [`INSERT INTO RC_Info (infoID, server, location, locationDescription, decimals, currencyCC, itemsCount, barcodesCount, supplierRefsCount, suppliersCount, lotsCount)
    VALUES (1, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)`,
    [server, location, locationDescription, parseInt(info.decimals, 10) || 0, info.currencyCC ?? '', info.itemsCount, info.barcodesCount, info.supplierRefsCount,
      info.suppliersCount, info.lotsCount ?? 0]],
]);

// Stores one page of items and moves the resume point to its last item
const SaveItemsPage = (items) => {
  const statements = items.map(item => [
    `INSERT OR REPLACE INTO RC_Items (itemRecID, itemReference, itemReferenceUpper, itemDescription, itemDescriptionUpper,
                                      itemSupplier, itemCategory, itemSerie, itemSingleRef1, itemSingleRef1Upper,
                                      itemSingleRef2, itemSingleRef2Upper, itemUnit, itemClass, itemSubUnit, itemVatType, itemCostCC,
                                      itemQtyControl, itemMeasType, itemConsFactor, itemHalted, itemBBFDate)
     VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)`,
    [item.itemRecID, item.itemReference, upper(item.itemReference), item.itemDescription, upper(item.itemDescription),
      upper(item.itemSupplier), upper(item.itemCategory), item.itemSerie, item.itemSingleRef1, upper(item.itemSingleRef1),
      item.itemSingleRef2, upper(item.itemSingleRef2), item.itemUnit, upper(item.itemClass), item.itemSubUnit, item.itemVatType,
      Number(item.itemCostCC) || 0, upper(item.itemQtyControl), upper(item.itemMeasType), Number(item.itemConsFactor ?? 1), Number(item.itemHalted) || 0,
      item.itemBBFDate ?? '']]);
  statements.push([
    'UPDATE RC_Info SET itemsStored = (SELECT COUNT(*) FROM RC_Items), lastItemRecID = ? WHERE infoID = 1',
    [items[items.length - 1].itemRecID]]);
  return executeInTransaction(statements);
};

// codeKind : barcode | supplierRef
const SaveCodesPage = (codeKind, codes) => {
  const statements = codes.map(code => [
    'INSERT OR REPLACE INTO RC_Codes (codeKind, codeRecID, itemRecID, codeValue, codeValueUpper, supplierRecID, codeExpiry, codeBatch) VALUES (?, ?, ?, ?, ?, ?, ?, ?)',
    [codeKind, code.codeRecID, code.itemRecID, code.codeValue, upper(code.codeValue), code.supplierRecID, code.codeExpiry ?? '', code.codeBatch ?? '']]);
  const [storedColumn, lastColumn] = codeKind === 'barcode' ? ['barcodesStored', 'lastBarcodeRecID'] : ['supplierRefsStored', 'lastSupplierRefRecID'];
  statements.push([
    `UPDATE RC_Info SET ${storedColumn} = (SELECT COUNT(*) FROM RC_Codes WHERE codeKind = ?), ${lastColumn} = ? WHERE infoID = 1`,
    [codeKind, codes[codes.length - 1].codeRecID]]);
  return executeInTransaction(statements);
};

const SaveLotsPage = (lots) => {
  const statements = lots.map(lot => [
    `INSERT OR REPLACE INTO RC_Lots (lotRecID, itemRecID, locationCode, lotExpiry, lotBatch, lotProduction, lotAvailable)
     VALUES (?, ?, ?, ?, ?, ?, ?)`,
    [lot.lotRecID, lot.itemRecID, lot.locationCode, lot.lotExpiry, lot.lotBatch, lot.lotProduction, Number(lot.lotAvailable) || 0]]);
  statements.push([
    'UPDATE RC_Info SET lotsStored = (SELECT COUNT(*) FROM RC_Lots), lastLotRecID = ? WHERE infoID = 1',
    [lots[lots.length - 1].lotRecID]]);
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

// The items of a scanned code (FindROItem, V_CPP_ROItemCodes) : the reference, the single references 1 / 2, a barcode,
// a supplier reference of the supplier. One row per item : the supplier reference first, then the barcode (it can give the expiry / lot)
// Returns [{ ...item, codeExpiry, codeBatch }]
const FindItemsByCode = async (supplierRecID, text) => {
  const code = upper((text ?? '').trim());
  if (code === '') return [];
  const rows = await query(
    `SELECT I.*, C.codeOrder, C.codeExpiry, C.codeBatch
     FROM (SELECT itemRecID, 0 codeOrder, '' codeExpiry, '' codeBatch FROM RC_Codes WHERE codeKind = 'supplierRef' AND codeValueUpper = ? AND supplierRecID = ?
           UNION ALL SELECT itemRecID, 1, codeExpiry, codeBatch FROM RC_Codes WHERE codeKind = 'barcode' AND codeValueUpper = ?
           UNION ALL SELECT itemRecID, 2, '', '' FROM RC_Items WHERE itemReferenceUpper = ? OR itemSingleRef1Upper = ? OR itemSingleRef2Upper = ?) C
          INNER JOIN RC_Items I ON I.itemRecID = C.itemRecID
     ORDER BY I.itemRecID, C.codeOrder`, [code, supplierRecID, code, code, code, code]);
  const seen = new Set();
  return rows.filter(row => !seen.has(row.itemRecID) && seen.add(row.itemRecID));
};

// The stock search : any part of the reference, the description, the single references or a code, the first ones by reference
const SearchItems = (text, limit) => {
  const value = upper((text ?? '').trim());
  if (value === '') return query(`SELECT * FROM RC_Items ${ITEMS_ORDER}`, [limit]);
  const part = '%' + likeText(value) + '%';
  return query(
    `SELECT * FROM RC_Items
     WHERE itemReferenceUpper LIKE ? ESCAPE '!' OR itemDescriptionUpper LIKE ? ESCAPE '!'
           OR itemSingleRef1Upper LIKE ? ESCAPE '!' OR itemSingleRef2Upper LIKE ? ESCAPE '!'
           OR itemRecID IN (SELECT itemRecID FROM RC_Codes WHERE codeValueUpper LIKE ? ESCAPE '!')
     ${ITEMS_ORDER}`, [part, part, part, part, part, limit]);
};

// The lots of an item for the lot window : { lots: [{ lotExpiry, lotBatch, lotAvailable }] of the location, knownLots: [{ lotExpiry, lotBatch, lotProduction }] }
const GetItemLots = async (itemRecID, location) => {
  const lots = await query('SELECT lotExpiry, lotBatch, lotAvailable FROM RC_Lots WHERE itemRecID = ? AND locationCode = ?', [itemRecID, location]);
  const knownLots = await query("SELECT lotExpiry, lotBatch, lotProduction FROM RC_Lots WHERE itemRecID = ? AND locationCode = '00' AND lotBatch <> ''", [itemRecID]);
  return { lots, knownLots };
};

// The first supplier reference of the item for this supplier ('' when it has none)
const GetItemSupplierRef = async (itemRecID, supplierRecID) => {
  const rows = await query("SELECT codeValue FROM RC_Codes WHERE codeKind = 'supplierRef' AND itemRecID = ? AND supplierRecID = ? ORDER BY codeRecID LIMIT 1",
    [itemRecID, supplierRecID]);
  return rows.length > 0 ? rows[0].codeValue : '';
};

// The item that has this barcode (null : none)
const GetBarcodeItem = async (barcode) => {
  const rows = await query("SELECT itemRecID FROM RC_Codes WHERE codeKind = 'barcode' AND codeValueUpper = ? LIMIT 1", [upper((barcode ?? '').trim())]);
  return rows.length > 0 ? rows[0].itemRecID : null;
};

const GetSuppliers = () => query('SELECT * FROM RC_Suppliers ORDER BY supplierCode');

const GetSupplier = async (supplierRecID) => {
  const rows = await query('SELECT * FROM RC_Suppliers WHERE supplierRecID = ?', [supplierRecID]);
  return rows.length > 0 ? rows[0] : null;
};

//#endregion

//#region Receiving orders

// { orders, lines, qty, pending, pendingOrders } : the receiving orders on the device (not uploaded) and their lines
const GetLinesSummary = async () => {
  const rows = await query(
    `SELECT COUNT(DISTINCT O.orderID) orders, COUNT(L.lineNo) lines, IFNULL(SUM(L.qty), 0) qty,
            COUNT(L.lineNo) pending, COUNT(DISTINCT O.orderID) pendingOrders
     FROM RC_Orders O LEFT JOIN RC_OrderLines L ON L.orderID = O.orderID
     WHERE O.state = 1`);
  return rows[0];
};

// Saves the receiving order of the screens (orderID : the one saved before, changed) and its lines in rows, returns its orderID
// lines : [{ itemRecID, itemReference, itemDescription, itemUnit, lineStatus, supplierRef, qty, lotsText }]
const SaveOrder = async (orderID, draft, lines) => {
  const db = await getDB();
  let savedID = orderID;
  await db.transaction(tx => {
    const values = [draft.supplier?.supplierRecID?.toString() ?? '', draft.supplier?.supplierCode ?? '', draft.supplier?.supplierName ?? '',
      draft.roRef.trim(), draft.roDate, draft.location?.locationCode ?? '', JSON.stringify(draft), new Date().toISOString()];
    const saveLines = (id) => {
      tx.executeSql('DELETE FROM RC_OrderLines WHERE orderID = ?', [id]);
      lines.forEach((line, index) => tx.executeSql(
        `INSERT INTO RC_OrderLines (orderID, lineNo, itemRecID, itemReference, itemDescription, itemUnit, lineStatus, supplierRef, qty, lotsText)
         VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)`,
        [id, index + 1, line.itemRecID, line.itemReference, line.itemDescription, line.itemUnit, line.lineStatus, line.supplierRef, line.qty, line.lotsText]));
    };
    if (orderID) {
      tx.executeSql(`UPDATE RC_Orders SET supplierRecID = ?, supplierCode = ?, supplierName = ?, roRef = ?, roDate = ?, locationCode = ?, draft = ?, savedAt = ?
                     WHERE orderID = ? AND state = 1`, [...values, orderID]);
      saveLines(orderID);
    }
    else {
      tx.executeSql(`INSERT INTO RC_Orders (supplierRecID, supplierCode, supplierName, roRef, roDate, locationCode, draft, savedAt)
                     VALUES (?, ?, ?, ?, ?, ?, ?, ?)`, values, (_, result) => {
        savedID = result.insertId;
        saveLines(savedID);
      });
    }
  });
  return savedID;
};

// The receiving order saved on the device : { orderID, state, roRecID, draft (the receiving order of the screens) }
const GetOrder = async (orderID) => {
  const rows = await query('SELECT * FROM RC_Orders WHERE orderID = ?', [orderID]);
  return rows.length > 0 ? { ...rows[0], draft: JSON.parse(rows[0].draft) } : null;
};

// The receiving orders to upload, the first saved first
const GetPendingOrders = async () => {
  const rows = await query('SELECT * FROM RC_Orders WHERE state = 1 ORDER BY orderID');
  return rows.map(row => ({ ...row, draft: JSON.parse(row.draft) }));
};

// The receiving orders saved by the server (in one transaction) are removed from the device together,
// with any receiving order left uploaded by an older version
const DeleteUploadedOrders = (orderIDs) => {
  const marks = orderIDs.map(() => '?').join(', ');
  return executeInTransaction([
    [`DELETE FROM RC_OrderLines WHERE orderID IN (${marks}) OR orderID IN (SELECT orderID FROM RC_Orders WHERE state = 2)`, orderIDs],
    [`DELETE FROM RC_Orders WHERE orderID IN (${marks}) OR state = 2`, orderIDs],
  ]);
};

// A RO Ref # already used by another receiving order of the device ('' when it is not), exceptOrderID : the one being changed
const OrderOfRef = async (roRef, exceptOrderID) => {
  const rows = await query('SELECT orderID, supplierCode, roRecID FROM RC_Orders WHERE UPPER(roRef) = ? AND orderID <> ? LIMIT 1',
    [upper((roRef ?? '').trim()), exceptOrderID ?? 0]);
  return rows.length > 0 ? rows[0] : null;
};

// The lines of the receiving orders not uploaded (View Items), the last saved first, searched by the item, the supplier or the RO Ref #
const GetLines = (searchText, limit) => {
  const text = (searchText ?? '').trim();
  const select = `SELECT L.*, O.supplierCode, O.supplierName, O.roRef, O.state, O.roRecID FROM RC_OrderLines L INNER JOIN RC_Orders O ON O.orderID = L.orderID
                  WHERE O.state = 1`;
  if (text === '') return query(`${select} ORDER BY O.savedAt DESC, L.lineNo LIMIT ?`, [limit]);
  const part = '%' + likeText(upper(text)) + '%';
  return query(
    `${select}
       AND ( UPPER(L.itemReference) LIKE ? ESCAPE '!' OR UPPER(L.itemDescription) LIKE ? ESCAPE '!'
             OR UPPER(O.supplierCode) LIKE ? ESCAPE '!' OR UPPER(O.supplierName) LIKE ? ESCAPE '!' OR UPPER(O.roRef) LIKE ? ESCAPE '!' )
     ORDER BY O.savedAt DESC, L.lineNo LIMIT ?`, [part, part, part, part, part, limit]);
};

const DeleteOrder = (orderID) => executeInTransaction([
  ['DELETE FROM RC_OrderLines WHERE orderID = ?', [orderID]],
  ['DELETE FROM RC_Orders WHERE orderID = ?', [orderID]],
]);

const DeleteAllOrders = () => executeInTransaction([
  ['DELETE FROM RC_OrderLines', []],
  ['DELETE FROM RC_Orders', []],
]);

//#endregion

export default {
  GetInfo,
  StartDownload,
  SaveItemsPage,
  SaveCodesPage,
  SaveLotsPage,
  SaveSuppliers,
  CompleteDownload,
  GetItem,
  GetItemBarcodes,
  FindItemsByCode,
  SearchItems,
  GetItemLots,
  GetItemSupplierRef,
  GetBarcodeItem,
  GetSuppliers,
  GetSupplier,
  GetLinesSummary,
  SaveOrder,
  GetOrder,
  GetPendingOrders,
  DeleteUploadedOrders,
  OrderOfRef,
  GetLines,
  DeleteOrder,
  DeleteAllOrders,
}
