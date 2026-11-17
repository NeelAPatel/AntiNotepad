# A21-save-delay-setting-r5: fix the three review findings

Rework round of `A21-save-delay-setting`. Read `.coord/tasks/A21-save-delay-setting-r3.md` (spec, edge cases; still applies), then the review `.coord/reports/A21-save-delay-setting-r4.review.md` (the three findings, each with a reproduction). Do not edit any card or brief.

Tier: High (same)
Role: worker, because shared save state and timing across Note, Store and the window.

## Fix
1. [major] Clearing a new note while its first write is queued loses the deletion (`Model/Note.cs`, `Model/Store.cs`, `UI/PadWindow.cs`). The blank-discard rule must not clear Dirty or drop the deadline while a write of older text is queued or in flight; the later empty state must still be written or the earlier write superseded, so reload never brings the old text back.
2. [major] Switching the active note inside split view, by shortcut or by clicking the other pane, must save the departing note at once (`UI/PadWindow.Navigation.cs`, `UI/PadWindow.Layout.cs`, `Editor/PadView.cs` if needed).
3. [minor] Label refresh interval must never exceed 1 s when `SavedAt` is in the future (backward clock change): treat as "just now" and keep the 1 s cadence (`UI/PadWindow.Layout.cs`).

Each fix gets a selftest that fails before and passes after, based on the review's reproduction. Paste the before/after result lines in the report. No other changes.

## Files
- `product/AntiNotepad/Model/Note.cs`
- `product/AntiNotepad/Model/Store.cs`
- `product/AntiNotepad/UI/PadWindow.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/Theme/ChromeTokens.cs`
- `product/AntiNotepad/SelfTest/Store.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest/Design.cs`

## Done means
Offline only; finish inside 35 minutes. Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A21-save-delay-setting-r5 -Offline` run by you; quote its FINAL line. Write a draft report after the first green selftest. Delete your own `build/A21-save-delay-setting-r5-*` folders at the end (use the Bash tool's rm if the sandbox allows, else list them). If a process hangs, report its PID and path.

Report: `.coord/reports/A21-save-delay-setting-r5.md` (card report format, plus "Hand checks for the user": split view, switch the active pane with an unsaved edit then quit).
