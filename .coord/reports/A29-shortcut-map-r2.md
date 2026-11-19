# A29-shortcut-map-r2

## 1 Summary

Fixed all five proven review findings. Shortcut routing now resolves saved overrides as a set, protects Shift+Backspace, reports the effective Open slot key, and has no dispatch interception hook in product code. Build, selftest, and the requested offline gate pass.

## 2 Change list

- `UI/Shortcuts.cs`: reject Shift+Backspace; detect collisions among valid overrides before assigning routes; restore defaults for invalid or duplicate overrides unless a valid override owns that default. Added summaries to collision and route helpers.
- `UI/PadWindow.Keys.cs`: removed `ShortcutDispatchOverride`. Default routes are checked through `ShortcutMap.TryGet`; the selftest runs New note, Keep on top, and Help through the real handler.
- `UI/PadWindow.Navigation.cs`: slot promotion reads the effective Open slot label and omits the hint when Open slot is unassigned.
- `UI/SettingsWindow.cs`: added the requested method summaries.
- `SelfTest/UI.cs`: covered Shift+Backspace and the other checked editing gestures, malformed and duplicate paired overrides, fallback routes, valid swaps, explicit clears, slot hints, and safe real actions.
- `docs/shortcuts.md`: documented editor-owned editing gestures. The shortcut table still matches the map.

## 3 Behavior changes (keys removed from defaults)

No further defaults were removed in this rework. The A29 map's removed defaults versus dev remain as reported in the first review: Ctrl+P/K search aliases; Ctrl+PageUp/Down note aliases; Ctrl+, Settings; Ctrl+0, Ctrl+NumPad0 and Ctrl+tilde stack routes; Ctrl+1–9 and Ctrl+NumPad1–9 Open slot routes; Ctrl+Shift+1–9 and Ctrl+Shift+NumPad1–9 Keep slot routes; Alt+Shift+Up/Down note moves; Ctrl+Shift+Backspace delete note; Ctrl+/ and Ctrl+Shift+/ Help; Ctrl+G Grid in Search; and Ctrl+T Trash in Search. Ctrl+T still pins outside overlays. First/latest note actions had no prior default.

The rework also refuses Shift+Backspace recording so the editor can keep character deletion. Existing text-editing reservations for Shift+Delete, Ctrl+Insert, Shift+Insert and Ctrl+Shift+Z remain in place. Exact modifier matching remains; extra-modifier routes removed by the original map stay removed.

## 4 Out-of-scope files

No source or documentation changes were made outside this round's Files list. The gate generated its normal `.coord/reports/A29-shortcut-map-r2.gate.md` result. A separate user-directed Search-toggle note appeared in the base task card during this run; it is preserved and was not edited as part of this round.

## 5 Plan deviations

None.

## 6 Contracts

The existing `shortcut-map` CONTRACTS row remains covered. Its selftest checks blocked gestures, route lookup for defaults, malformed and duplicate override recovery, valid swaps, explicit clears, Help/docs parity, and the no-allocation lookup. The docs table matched the map. Gate output: 36 contract rows, 35 pass, 1 manual.

## 7 Lens review

Independent lens review: **accept**; no proven findings. The reviewer confirmed the fallback behavior, editing-key reservation, effective slot hint, hook removal, selftest coverage, and scope.

## 8 Checks run

- Debug build to `build/A29-shortcut-map-check`: succeeded, 0 warnings, 0 errors (48.53 s).
- Isolated `dotnet AntiNotepad.dll --selftest`: exit 0, `SELFTEST PASS` (under 10 s; 180 s limit).
- `pwsh -NoProfile -File tools/gate.ps1 -Task A29-shortcut-map-r2 -Offline`: **GATE: PASS**. Build 0 warnings/0 errors; selftest passed; seam, tokens, format, scope, and LoopLint passed. Gate reported its Offline warning and Size warnings for `SelfTest/UI.cs` and `SettingsWindow.cs`; neither failed the gate.
- Gate cleanup removed its task build and data folders. The standalone `build/A29-shortcut-map-check` folder remains: the command policy rejected the authorized recursive deletion after its absolute path and parent were verified.

## 9 Hand checks

Not performed. The dev-copy hand checks remain: record, clear, reset one row, reset all, restart, take a conflicting key, and confirm F1 Help lists the current bindings.
