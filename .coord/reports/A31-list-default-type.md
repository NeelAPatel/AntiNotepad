# A31-list-default-type report

## 1 Summary
Implemented the approved list default and direct list keywords. The required gate does not pass: its scope parser accepts only the first backticked path on each Files bullet, and four necessary files are genuinely outside that list.

## 2 Change list
- Added normalized and persisted `ListDefault` and the Editor settings pills.
- Added list keyword kinds, popup descriptions, and default-aware continuation and completion.
- Added list, persistence, popup-name, and contract checks; updated Settings field count and approved design hash.

## 3 Behavior changes
`list` defaults to bullets and follows the setting. `todo` and `checklist` use checkboxes; `bullet` and `bulletlist` use bullets; `numberlist` uses numbers. Existing prefixed lines retain their style. Plain lines in existing `list` notes follow the setting.

## 4 Out-of-scope files
- `Editor/DocumentCommands.cs` and `Editor/PadView.cs` carry the setting and first-line keyword to continuation.
- `SelfTest/UI.cs` updates the Settings field count; `SelfTest/Design.cs` updates the design hash for the approved control.
- The gate also flags Completion.cs, Settings.cs, SettingsService.cs, Contracts.cs, and Store.cs: these appear on the card but are not first paths on their bullets, so its parser misses them.

## 5 Plan deviations
`ModeKeyword` and `Modes.ListKindOf` use the persisted string kind, mapped to the existing `ListEditing.ListKind` in the editor. This preserves the enforced Model-to-Editor dependency boundary and adds no second enum. Four files above were needed beyond the listed paths. The requested dedicated build folder could not be deleted: the execution policy rejected the recursive removal command. The failed gate also retained its own build and data folders.

## 6 Contracts
Added `list-default-kind`: `list` follows `ListDefault`; `todo` always creates checkboxes. Focused checks cover all direct keywords and all defaults.

## 7 Checks run
- Offline build in `build/A31-list-default-type-check`: 0 warnings, 0 errors.
- `--selftest`: SELFTEST PASS; 36 automated contracts pass, one manual.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A31-list-default-type -Offline`: FAIL at Scope; Build, Selftest, Seam, Tokens, Format and Size pass. Offline audit disabled for this run.
- `git diff --check`: pass.

## 8 Hand checks
Not performed. In the slot dev copy, change each New list lines choice, type and accept `list` and each direct keyword, check the popup default description, and verify continuation and existing prefixed lines. Check the settings layout and fixed Enter behavior against `docs/shortcuts.md`.
