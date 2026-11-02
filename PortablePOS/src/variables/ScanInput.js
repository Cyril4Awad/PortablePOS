import { useCallback, useEffect, useRef } from 'react';

// Scan without Enter : some scanners (the Newland MT67) type the barcode without the Enter suffix, so onSubmitEditing never comes.
// A scanner types all its characters within a few milliseconds then stops, a person cannot type that fast :
// when the text stops changing, characters typed at scanner speed are submitted as if Enter was pressed.

// Wait after the last character before submitting (a scanner that sends Enter submits before, the wait is cancelled)
const IDLE_MS = 150;
// Average time between two characters of a scan (a scanner filling the box at once : 0)
const SCAN_CHAR_MS = 40;
// Fewer characters at scanner speed are not a scan (one key pressed)
const MIN_LENGTH = 3;

// onScan(text) : what Enter does in this box, called with the text after the box re-rendered (it can read its state)
// Returns { watch, cancel } :
// watch(text, previousText) in onChangeText, previousText : the value of the box before this change (its state)
// cancel() first thing in onSubmitEditing (Enter already submitted)
export const useScanWithoutEnter = (onScan) => {

  const onScanRef = useRef(onScan);
  onScanRef.current = onScan;

  const timer = useRef(null);
  // the characters added since the first change of the burst, its first and last change
  const burst = useRef({ added: 0, firstTime: 0, lastTime: 0 });

  const cancel = useCallback(() => {
    clearTimeout(timer.current);
    timer.current = null;
  }, []);

  useEffect(() => cancel, [cancel]);

  const watch = useCallback((text, previousText) => {

    const value = text ?? '';
    const previous = previousText ?? '';
    const inBurst = timer.current !== null;
    cancel();

    // characters at the end only : one key, the burst going on, or a scanner filling the empty box at once
    // otherwise (a deletion, an edit in the middle, a word of the keyboard suggestions) : typed
    if (value.trim() === '' || !value.startsWith(previous)) return;
    const added = value.length - previous.length;
    if (added === 0) return;
    if (added > 1 && previous !== '' && !inBurst) return;

    const now = Date.now();
    if (inBurst) {
      burst.current.added += added;
      burst.current.lastTime = now;
    } else {
      burst.current = { added, firstTime: now, lastTime: now };
    }

    timer.current = setTimeout(() => {
      timer.current = null;
      const { added: count, firstTime, lastTime } = burst.current;
      if (count < MIN_LENGTH) return;
      if (lastTime - firstTime > (count - 1) * SCAN_CHAR_MS) return;
      onScanRef.current(value);
    }, IDLE_MS);

  }, [cancel]);

  return { watch, cancel };
};
