import React from 'react';
import renderer, { act } from 'react-test-renderer';
import { useScanWithoutEnter } from '../src/variables/ScanInput';

// The hook in a component : box.type(text, msLater) changes the box like onChangeText, box.enter() like onSubmitEditing
const renderBox = () => {
    const scans = [];
    let scan;
    const Box = () => {
        scan = useScanWithoutEnter(text => scans.push(text));
        return null;
    };
    act(() => { renderer.create(<Box />); });
    let value = '';
    let now = 1000;
    const box = {
        scans,
        type: (text, msLater = 0) => {
            now += msLater;
            jest.setSystemTime(now);
            jest.advanceTimersByTime(msLater);
            const previous = value;
            value = text;
            scan.watch(text, previous);
        },
        clear: () => { value = ''; },
        enter: () => scan.cancel(),
        wait: (ms) => { now += ms; jest.setSystemTime(now); jest.advanceTimersByTime(ms); },
    };
    return box;
};

// A scanner typing one character every msPerChar
const typeChars = (box, text, msPerChar, start = '') => {
    for (let i = 1; i <= text.length; i++) box.type(start + text.substring(0, i), i === 1 ? 0 : msPerChar);
};

beforeEach(() => jest.useFakeTimers('modern'));
afterEach(() => jest.useRealTimers());

describe('scan without Enter', () => {
    test('a scanner typing fast is submitted when it stops', () => {
        const box = renderBox();
        typeChars(box, '6291041500213', 5);
        expect(box.scans).toEqual([]);
        box.wait(200);
        expect(box.scans).toEqual(['6291041500213']);
    });

    test('a scanner filling the empty box at once is submitted', () => {
        const box = renderBox();
        box.type('ABC-123');
        box.wait(200);
        expect(box.scans).toEqual(['ABC-123']);
    });

    test('a scanner that sends Enter is not submitted twice', () => {
        const box = renderBox();
        typeChars(box, '6291041500213', 5);
        box.enter();
        box.wait(500);
        expect(box.scans).toEqual([]);
    });

    test('typing on the keyboard is not submitted', () => {
        const box = renderBox();
        typeChars(box, 'CHOCOLATE', 180);
        box.wait(1000);
        expect(box.scans).toEqual([]);
    });

    test('one key in a box that has text is not submitted', () => {
        const box = renderBox();
        typeChars(box, 'CHO', 180);
        box.wait(1000);
        box.type('CHOC', 0);
        box.wait(1000);
        expect(box.scans).toEqual([]);
    });

    test('a word of the keyboard suggestions is not submitted', () => {
        const box = renderBox();
        typeChars(box, 'CH', 200);
        box.wait(1000);
        box.type('CHOCOLATE ');
        box.wait(1000);
        expect(box.scans).toEqual([]);
    });

    test('a deletion is not submitted', () => {
        const box = renderBox();
        box.type('ABC-123');
        box.enter();
        box.type('ABC-12');
        box.wait(1000);
        expect(box.scans).toEqual([]);
    });

    test('the next scan after the screen cleared the box is submitted, the same code too', () => {
        const box = renderBox();
        box.type('ABC-123');
        box.wait(200);
        box.clear();
        box.type('ABC-123');
        box.wait(200);
        expect(box.scans).toEqual(['ABC-123', 'ABC-123']);
    });

    test('one or two characters are not a scan', () => {
        const box = renderBox();
        box.type('12');
        box.wait(1000);
        expect(box.scans).toEqual([]);
    });
});
