# A31-list-default-type-r2: fix gate Format whitespace

Rework round of `A31-list-default-type` (`docs/loop.md`). Read the item card `.coord/tasks/A31-list-default-type.md` first: its Plan, Files, Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Medium, same as the card
Role: quick, because it is a three-spot whitespace fix

## Why this round
Gate Format fails on changed lines (code cause), everything else passes (Build, Selftest, Seam, Tokens, Scope, Size):
- `Editor/Completion.cs:87` WHITESPACE
- `Editor/ListEditing.cs:94` WHITESPACE
- `Editor/ListEditing.cs:100` WHITESPACE
Fix with `dotnet format whitespace --include <file>` on those two files, and check the diff changed only whitespace.

## Change to the plan
None.

## Files
- `product/AntiNotepad/Editor/Completion.cs`
- `product/AntiNotepad/Editor/ListEditing.cs`

## Done means
- The gate's Format step passes; no behavior change.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A31-list-default-type-r2 -Offline` passes. Delete any `build/A31-list-default-type-*` folders you can; if the policy blocks deletion, say so.

Report: `.coord/reports/A31-list-default-type-r2.md`, short (Summary, Change list, Checks run).
