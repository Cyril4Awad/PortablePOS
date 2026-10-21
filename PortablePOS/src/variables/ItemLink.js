// QR of the labels (Print Label, named data source QR) : a link ending with the record id of the item,
// LabelQRDefaultPath of the server + "/" + record id, e.g. http://site/item/12345
// Any link ending with "/<number>" is read the same way : the labels printed before a change of the path still scan
// (a barcode or a reference never contains "://")
const ITEM_LINK = /^[a-z][a-z0-9+.-]*:\/\/.*\/([0-9]+)\/?$/i;

// The record id of the item of a scanned label link, null when the text is not a link (a barcode, a reference...)
export const ItemRecIDFromLink = (scannedText) => {
    const match = ITEM_LINK.exec((scannedText ?? '').trim());
    return match ? match[1] : null;
};
