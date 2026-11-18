# A21b-save-scheduler-extract report

## 1 Summary
Implemented the approved shrink and pure timing move. PadWindow.cs went from 740 to 680 physical lines (the card's 739-line baseline counts differently).

## 2 Change list
- Moved SaveTiming unchanged to UI/SaveTiming.cs and updated selftest references.
- Added Note.ResolveEmptyEdit and used it at both save boundaries; navigation's discard calls remain unchanged.
- Added no-window empty-edit cases and pending-edit checks through Quit and PrepareForSessionEnd.

## 3 Behavior changes
None intended. Save delays, timer glue, per-note schedules and saved label are unchanged.

## 4 Out-of-scope files
No product files outside the approved Files list were changed. The offline gate wrote its standard report.

## 5 Plan deviations
The move and model change are uncommitted because Codex cannot commit; the item chat must make the planned separate move commit. Independent Codex and design reviews, merge catch-up and slot hand test remain with the item chat.

## 6 Contracts
No new contracts. Gate selftest reports 32 rows: 31 pass, 1 manual.

## 7 Lens review
Independent lens review not run in this implementation session; required check remains pending.

## 8 Checks run
- Restored from local NuGet cache offline. Build: 0 warnings, 0 errors.
- Baseline and pure-move selftests passed; outputs matched after omitting timing lines and normalizing the save timestamp. Sorted moved lines equal.
- Final --selftest: SELFTEST PASS, including the new model, quit and session-end checks.
- pwsh -NoProfile -File tools/gate.ps1 -Task A21b-save-scheduler-extract -Offline: GATE PASS. NuGet audit disabled by offline gate; tokens baseline can be lowered.
- git diff --check: pass.

## 9 Hand checks
Not run: brief prohibits launching the app. Item chat should type, wait, switch and hide in the slot dev copy.
