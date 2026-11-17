# A21-save-delay-setting-r2

## 1 Summary

The save-delay UI check now reuses the Avalonia application initialized by `shortened-link-opens`; its duplicate `SetupWithoutStarting()` call is removed. The design dump comparison confirmed the only addition is the new Saving group and row, so the expected hash now matches this build.

## 2 Change list

- `product/AntiNotepad/SelfTest/UI.cs:277` | Reuses the existing application and reports a clear ordering failure | Plan > Tests: save-delay timer and transition checks.
- `product/AntiNotepad/SelfTest/Contracts.cs:113` | Runs the save-delay UI contract after `shortened-link-opens` | Plan > Tests: timer interval follows setting and pending edits save at transitions.
- `product/AntiNotepad/SelfTest/Design.cs:22` | Updates the expected design hash to `7c2c76482fc04e6e4e3c933b72e6d5e42774f6a702844eb41cbf66db181ce607` | Plan > Gate: design snapshot check.

## 3 Behavior changes

None. This rework changes selftest setup order and the pinned design snapshot only.

## 4 Out-of-scope files

No out-of-scope files changed in this rework. `SelfTest/Store.cs` needed no change.

## 5 Plan deviations

None.

## 6 Contracts

`shortened-link-opens` and `save-delay-timer-and-boundaries` both pass. The gate reports 21 contract rows: 20 automated passes and one manual check.

## 7 Lens review

No separate lens review was run for this targeted selftest rework.

## 8 Checks run

- Design dumps: dev `build/dev` hash `183c64c8a607288e0dcd12e0d2d24584bccbee4d38efab50df283d9295bc46e2`; this build hash `7c2c76482fc04e6e4e3c933b72e6d5e42774f6a702844eb41cbf66db181ce607`. After accounting for the two shifted top-level child indexes, the diff had 248 added lines under the Saving group and row, zero removed lines, and no differences elsewhere.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A21-save-delay-setting-r2 -Offline` — PASS. Build: 0 warnings, 0 errors. Selftest: `SELFTEST PASS`. Seam, tokens, format, scope and size checks passed. Offline NuGet audit was disabled as expected.

## 9 Hand checks for the user

None for this selftest-only rework. The A21 Settings hand check remains for the dev copy.
