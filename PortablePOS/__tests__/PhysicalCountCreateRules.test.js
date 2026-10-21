import {
    EmptyFilters, HasGroups, GroupSummary, NotCountedSummary, CheckDate, CheckDateRange,
    CreateSelection, CheckAll, UncheckAll, IsChecked, ToggleItem, AvailableQtyText,
} from '../src/variables/PhysicalCountCreateRules';
import { FormatQuantity } from '../src/variables/PhysicalCountRules';

const item = (itemRecID, selected, extra = {}) => ({ itemRecID, selected, ...extra });

describe('selection', () => {
    test('an item keeps the server value until it is toggled', () => {
        const selection = CreateSelection();
        expect(IsChecked(selection, item('1', 1))).toBe(true);
        expect(IsChecked(selection, item('2', 0))).toBe(false);
    });

    test('toggling checks an unchecked item and unchecks a checked one', () => {
        let selection = ToggleItem(CreateSelection(), item('2', 0));
        expect(selection.include).toEqual(['2']);
        expect(IsChecked(selection, item('2', 0))).toBe(true);

        selection = ToggleItem(selection, item('2', 0));
        expect(selection.include).toEqual([]);
        expect(selection.exclude).toEqual(['2']);
        expect(IsChecked(selection, item('2', 0))).toBe(false);
    });

    test('an item checked by its brand can be unchecked by hand', () => {
        const selection = ToggleItem(CreateSelection(), item('7', 1));
        expect(selection.exclude).toEqual(['7']);
        expect(IsChecked(selection, item('7', 1))).toBe(false);
    });

    test('check all / uncheck all start again from nothing typed by hand', () => {
        expect(CheckAll()).toEqual({ all: true, groups: true, include: [], exclude: [] });
        expect(UncheckAll()).toEqual({ all: false, groups: false, include: [], exclude: [] });
    });
});

describe('filters', () => {
    test('groups and summaries', () => {
        const filters = EmptyFilters();
        expect(HasGroups(filters)).toBe(false);
        expect(HasGroups({ ...filters, categories: ['C1'] })).toBe(true);
        expect(GroupSummary([{ code: 'B1', description: 'Nike' }], [])).toBe('-');
        expect(GroupSummary([{ code: 'B1', description: 'Nike' }], ['B1', 'B9'])).toBe('2 Checked : Nike, B9');
        expect(NotCountedSummary(null)).toBe('Off');
        expect(NotCountedSummary({ from: '', to: '' })).toBe('Not in an open count');
        expect(NotCountedSummary({ from: '01/10/2026', to: '' })).toBe('01/10/2026  -  ...');
    });

    test('dates are dd/MM/yyyy, optional, and from before to', () => {
        expect(CheckDate('')).toBe('');
        expect(CheckDate('08/10/2026')).toBe('');
        expect(CheckDate('2026-10-08')).toBe('Type the date as dd/MM/yyyy');
        expect(CheckDate('31/02/2026')).toBe('This date does not exist');
        expect(CheckDateRange('01/10/2026', '08/10/2026')).toBe('');
        expect(CheckDateRange('', '08/10/2026')).toBe('');
        expect(CheckDateRange('09/10/2026', '08/10/2026')).toBe('The From date is after the To date');
        expect(CheckDateRange('99/10/2026', '')).toBe('From : This date does not exist');
    });
});

describe('available qty', () => {
    test('shown only when the server sends it (privilege) and it is not 0', () => {
        expect(AvailableQtyText(item('1', 0), FormatQuantity)).toBe('');
        expect(AvailableQtyText(item('1', 0, { systemQty: 0 }), FormatQuantity)).toBe('');
        expect(AvailableQtyText(item('1', 0, { systemQty: 12.5 }), FormatQuantity)).toBe('Avlb 12.5');
        expect(AvailableQtyText(item('1', 0, { systemQty: -3 }), FormatQuantity)).toBe('Avlb -3');
    });
});
