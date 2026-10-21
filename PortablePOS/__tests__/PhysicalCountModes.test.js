import {
    ClassifyFormat, IsScannedInput, QtyFromText, QtyError, CreateSession, KnowLine, AddEntry, SetTotal, StartPending, ConfirmPending,
    SetNextQty, ScanWithNextQty, Undo, UndoLabel, Preview, IsSameItemAgain, RememberScan,
} from '../src/variables/PhysicalCountModes';

// Quantities are thousandths : 1 => 1000
const known = (counted, state = 0) => KnowLine(CreateSession(), 'A', counted, state);

describe('adding', () => {
    test('+1 adds to the total of the line read from the database', () => {
        const session = AddEntry(known(3000), 'A', 1000).session;
        const result = AddEntry(session, 'A', 1000);
        expect(result.session.counts.A).toBe(5000);
        expect(result.message).toBe('Added 1 · total 5');
        expect(result.save).toEqual({ lineRecID: 'A', counted: 5000 });
    });

    test('scan then type : pending scan confirmed with the typed quantity', () => {
        const session = StartPending(known(2000), 'A').session;
        const result = ConfirmPending(session, 6000);
        expect(result.session.counts.A).toBe(8000);
        expect(result.session.pending).toBeNull();
    });

    test('scan then type : confirming without typing adds 1', () => {
        const session = StartPending(known(0), 'A').session;
        expect(ConfirmPending(session).session.counts.A).toBe(1000);
    });

    test('type then scan : next scan adds N then resets to 1', () => {
        let session = SetNextQty(known(0), 12000);
        session = ScanWithNextQty(session, 'A').session;
        expect(session.counts.A).toBe(12000);
        expect(session.nextQty).toBe(1000);
        expect(ScanWithNextQty(session, 'A').session.counts.A).toBe(13000);
    });

    test('set total replaces', () => {
        const result = SetTotal(known(40000), 'A', 7000);
        expect(result.session.counts.A).toBe(7000);
        expect(result.message).toBe('Total set to 7 (was 40)');
    });

    test('typed quantities', () => {
        expect(QtyFromText('30')).toBe(30000);
        expect(QtyFromText(' 7 ')).toBe(7000);
    });
});

describe('undo', () => {
    test('undo puts back the total and the state before the last entry', () => {
        const session = AddEntry(known(8000, 0), 'A', 5000).session;
        expect(UndoLabel(session)).toBe('+5');
        const result = Undo(session, () => 'REF-A');
        expect(result.session.counts.A).toBe(8000);
        expect(result.message).toBe('Undo : +5 on REF-A (total 13 → 8)');
        expect(result.restore).toEqual({ lineRecID: 'A', counted: 8000, state: 0 });
    });

    test('one level only', () => {
        let session = AddEntry(known(0), 'A', 1000).session;
        session = AddEntry(session, 'A', 1000).session;
        session = Undo(session).session;
        expect(session.counts.A).toBe(1000);
        expect(UndoLabel(session)).toBe('');
        expect(Undo(session).message).toBe('Nothing to undo');
    });

    test('undo of set total', () => {
        const session = SetTotal(known(40000, 2), 'A', 7000).session;
        expect(UndoLabel(session)).toBe('= 7');
        const result = Undo(session, () => 'A');
        expect(result.session.counts.A).toBe(40000);
        expect(result.restore).toEqual({ lineRecID: 'A', counted: 40000, state: 2 });
    });

    test('undo cancels a pending scan without touching the database', () => {
        const session = StartPending(known(0), 'A').session;
        expect(UndoLabel(session)).toBe('scan');
        const result = Undo(session, () => 'REF-A');
        expect(result.session.pending).toBeNull();
        expect(result.restore).toBeNull();
    });
});

describe('preview, same item, format', () => {
    test('total will be', () => {
        expect(Preview(known(3000), 'A', 10000)).toBe(13000);
        expect(Preview(CreateSession(), 'B', 4000)).toBe(4000);
    });

    test('same code within 1 second warns', () => {
        const session = RememberScan(CreateSession(), 'A', 1000);
        expect(IsSameItemAgain(session, 'A', 1800)).toBe(true);
        expect(IsSameItemAgain(session, 'A', 2100)).toBe(false);
        expect(IsSameItemAgain(session, 'B', 1100)).toBe(false);
    });

    test('quantity box : scanner speed or barcode format is a scan, slower digits are a quantity', () => {
        expect(IsScannedInput('0', 1000, 1015)).toBe(true);             // barcode 0 from the scanner
        expect(IsScannedInput('12345', 1000, 1030)).toBe(true);
        expect(IsScannedInput('30', 1000, 1600)).toBe(false);            // typed by a person
        expect(IsScannedInput('5285002192105', 1000, 5000)).toBe(true);  // more than 9 digits
        expect(IsScannedInput('A/FP/JB1/3/F', 1000, 5000)).toBe(true);   // reference
        expect(IsScannedInput('1', null, 1000)).toBe(false);             // nothing typed : the selected 1
    });

    test('letters, symbols or more than 9 digits are barcodes', () => {
        expect(ClassifyFormat('12')).toBe('quantity');
        expect(ClassifyFormat('999999999')).toBe('quantity');
        expect(ClassifyFormat('5285002192105')).toBe('barcode');
        expect(ClassifyFormat('A/FP/JB1/3/F')).toBe('barcode');
        expect(ClassifyFormat('1.5')).toBe('barcode');
        expect(ClassifyFormat('  ')).toBe('empty');
    });
});

describe('quantity errors', () => {
    test('negative quantities are rejected, not searched as a barcode', () => {
        expect(QtyError('-5')).toBe("Can't enter a negative amount");
        expect(QtyError(' -12 ')).toBe("Can't enter a negative amount");
        expect(QtyError('-1.5')).toBe("Can't enter a negative amount");
    });

    test('0 is rejected', () => {
        expect(QtyError('0')).toBe("Can't enter 0");
        expect(QtyError('000')).toBe("Can't enter 0");
    });

    test('positive quantities and barcodes are not quantity errors', () => {
        expect(QtyError('1')).toBe('');
        expect(QtyError('10')).toBe('');
        expect(QtyError('ABC-123')).toBe('');
        expect(QtyError('6291041500213')).toBe('');
        expect(QtyError('')).toBe('');
    });
});
