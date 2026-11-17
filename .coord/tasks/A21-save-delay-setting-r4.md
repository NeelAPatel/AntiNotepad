# A21-save-delay-setting-r4: finish r3 (it timed out at 45 min, gate failing)

Continuation of `A21-save-delay-setting-r3`. Read `.coord/tasks/A21-save-delay-setting-r3.md` (the full spec, edge cases and Files list; it still applies unchanged), then the card. Do not edit the card.

Tier: High (same)
Role: worker, because the work is half done in shared timer and test state.

## State you inherit
The r3 run was stopped by the 45-minute limit. Its edits are uncommitted in this worktree (`git diff` shows 11 files, 647 added lines); there is no r3 report. Treat the diff as your own unfinished work: read it first, keep what is right, fix what is not. Do not start over.

## Gate result at the stop (`.coord/reports/A21-save-delay-setting-r3.gate.md`)
Build 0 warnings; Seam, Tokens, Format, Scope pass. Selftest FAILS:
- contract `pinned-accent-ring`: "Parameter count mismatch." A reflection call in a selftest no longer matches a method whose signature the r3 edit changed (likely in `PadWindow.Layout.cs` or `PadWindow.cs`). Find it; fix the test call, or keep the old signature.
- contracts `save-delay-settings-persist` and `save-delay-timer-and-boundaries` are listed in the failing block; find out what they miss (run the selftest, read the lines) and fix.
- Design dump hash line: update the expected hash in `SelfTest/Design.cs` only after confirming the dump change is the intended one.
- Size WARN: say in the report which files crossed 600 lines; do not split them now.

## Done means
Offline only, and finish inside 40 minutes: stop adding new tests once the spec's edge cases have one check each. Build 0 warnings, `SELFTEST PASS`, run `pwsh -NoProfile -File tools/gate.ps1 -Task A21-save-delay-setting-r4 -Offline` and quote its FINAL line in the report. Write the report FIRST as a draft after your first green selftest, then update it. If a process hangs, tell me the PID and path.

Report: `.coord/reports/A21-save-delay-setting-r4.md`: card report format, plus a final section "Hand checks for the user" (new blank note shows "-"; type at 5 s; nonstop typing writes every N s; switch, hide and minimize save at once; bars stay still on a save; label counts just now, 1s, 2s).

## Files

- `product/AntiNotepad/Model/Note.cs`
- `product/AntiNotepad/Model/Store.cs`
- `product/AntiNotepad/UI/PadWindow.cs`
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/Kit.cs`
- `product/AntiNotepad/Theme/ChromeTokens.cs`
- `product/AntiNotepad/SelfTest/Store.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
