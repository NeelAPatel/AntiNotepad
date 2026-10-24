# A9-input-limits-and-timers

## Changes

- **A1-4-list-marker-overflow — `Editor/PadView.cs`:** The list marker uses ASCII digits. `TryListNumber` accepts at most nine digits with invariant `TryParse`; oversized markers take plain Enter/Tab behavior and are skipped by renumbering. Renumbering also avoids creating a ten-digit successor.
- **A1-5-timer-parser-can-throw — `Model/Timers.cs`:** Timer patterns accept ASCII digits only. Clock fields use invariant `TryParse`; part durations reject non-finite values and totals over 99 days before converting to `TimeSpan`. Start and toggle use a checked `DateTime` end-time helper.
- **A1-6-calculator-unbounded-recursion — `Calc/Calculator.cs`:** Parentheses, chained unary signs, nested functions and recursive power parsing share a depth limit of 64. Deeper lines produce the same invalid result as other parse errors.
- **A1-19-mixed-currency-addition — `Calc/Calculator.cs`:** Add/subtract reject two different explicit currencies, including the percent-add path. Function and note aggregates reject mismatched currencies too; same-currency and number-plus-money behavior remains.
- **A1-3-preview-timer-retains-settings — `UI/MotionPreview.cs`, `Editor/PadView.cs`, `Model/Timers.cs`:** The preview owns a private timer collection. Release unsubscribes the pad’s tick handler and stops and clears only that collection; the resident pad’s shared collection is unaffected.
- **A1-16-timer-history-never-pruned — `Editor/PadView.cs`, `Model/Timers.cs`, `UI/PadWindow.cs`:** Timer keys are `noteId:durationText:ordinal`, where the ordinal counts parsed timer lines in that note. The label is excluded, so label edits retain the running timer. Each analysis reconciles keys for that note; removed lines stop and remove their entries. The shared timer collection checks live stack and slot note IDs, excluding trash. Finished entries leave `_entries`; a reconciled done marker keeps the existing done chip until its line or note is removed.

## Verification

- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a9-check`: **0 warnings, 0 errors** when run with `NuGetAudit=false` in the environment. The ordinary restore could not reach nuget.org and emitted `NU1900` for vulnerability data; disabling that online audit avoided the network-only warning.
- `build/a9-check/AntiNotepad.exe --selftest`: **SELFTEST PASS**.
- Removing `build/a9-check` was attempted after resolving it inside the worktree, but the execution policy rejected recursive deletion; the check output remains.
