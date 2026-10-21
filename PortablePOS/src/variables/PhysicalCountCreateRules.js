// Create a count : same rules as the count module of the ERP (Show then Save)
// - filters : books only / no books, not counted between two dates
// - brands, categories, product groups, identifications : their items are checked (OR between them, like the module)
// - the user checks / unchecks items, Check All / Uncheck All
// The server works out the checked items from the filters and the selection, the device never holds the whole item file

export const BOOKS_OPTIONS = [
    { code: 'all', description: 'All Items' },
    { code: 'books', description: 'Books Only' },
    { code: 'noBooks', description: 'No Books' },
];

export const GROUPS = [
    { key: 'brands', title: 'Brands' },
    { key: 'categories', title: 'Categories' },
    { key: 'productGroups', title: 'Product Groups' },
    { key: 'identifications', title: 'Identifications' },
];

// notCounted : null (off) or { from: 'dd/MM/yyyy' or '', to: 'dd/MM/yyyy' or '' }
export const EmptyFilters = () => ({
    books: 'all',
    notCounted: null,
    brands: [],
    categories: [],
    productGroups: [],
    identifications: [],
});

export const HasGroups = (filters) => GROUPS.some(group => filters[group.key].length > 0);

// '-' or "2 Checked : Nike, Adidas"
export const GroupSummary = (list, codes) => {
    if (codes.length === 0) return '-';
    const names = codes.map(code => list.find(row => row.code === code)?.description || code);
    return `${codes.length} Checked : ${names.join(', ')}`;
};

export const NotCountedSummary = (notCounted) => {
    if (notCounted === null) return 'Off';
    if (notCounted.from === '' && notCounted.to === '') return 'Not in an open count';
    return `${notCounted.from || '...'}  -  ${notCounted.to || '...'}`;
};

// '' when valid (empty is allowed), else the message
export const CheckDate = (text) => {
    const value = (text ?? '').trim();
    if (value === '') return '';
    const match = /^([0-9]{2})\/([0-9]{2})\/([0-9]{4})$/.exec(value);
    if (!match) return 'Type the date as dd/MM/yyyy';
    const [day, month, year] = [parseInt(match[1], 10), parseInt(match[2], 10), parseInt(match[3], 10)];
    const date = new Date(year, month - 1, day);
    if (date.getFullYear() !== year || date.getMonth() !== month - 1 || date.getDate() !== day) return 'This date does not exist';
    return '';
};

const DateValue = (text) => {
    const [day, month, year] = text.split('/').map(part => parseInt(part, 10));
    return new Date(year, month - 1, day).getTime();
};

// '' when valid, else the message
export const CheckDateRange = (from, to) => {
    const fromError = CheckDate(from);
    if (fromError !== '') return 'From : ' + fromError;
    const toError = CheckDate(to);
    if (toError !== '') return 'To : ' + toError;
    if (from.trim() !== '' && to.trim() !== '' && DateValue(from.trim()) > DateValue(to.trim())) return 'The From date is after the To date';
    return '';
};

//#region Selection

// all : every item is checked, groups : the checked brands / categories... check their items,
// include / exclude : items checked / unchecked by hand (itemRecID)
export const CreateSelection = () => ({ all: false, groups: true, include: [], exclude: [] });

export const CheckAll = () => ({ all: true, groups: true, include: [], exclude: [] });

// The checked brands / categories... do not check their items anymore
export const UncheckAll = () => ({ all: false, groups: false, include: [], exclude: [] });

// item.selected : value of the server when the page was read (with the selection of that time)
export const IsChecked = (selection, item) => {
    if (selection.exclude.includes(item.itemRecID)) return false;
    if (selection.include.includes(item.itemRecID)) return true;
    return item.selected === 1 || item.selected === true;
};

export const ToggleItem = (selection, item) => {
    const id = item.itemRecID;
    if (IsChecked(selection, item)) {
        return { ...selection, include: selection.include.filter(value => value !== id), exclude: [...selection.exclude, id] };
    }
    return { ...selection, exclude: selection.exclude.filter(value => value !== id), include: [...selection.include, id] };
};

//#endregion

// The available qty is shown only with the privilege (the server does not send it otherwise), and never when it is 0
export const AvailableQtyText = (item, formatQuantity) => {
    if (item.systemQty === undefined || item.systemQty === null) return '';
    const milli = Math.round(Number(item.systemQty) * 1000);
    if (isNaN(milli) || milli === 0) return '';
    return `Avlb ${formatQuantity(milli)}`;
};
