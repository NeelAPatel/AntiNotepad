# A35d-spacing-tokens build step 1

## 1 Summary

Added the spacing scale, stroke tokens, named layout sizes, and the specified derived and exception tokens. Replaced same-value spacing, size and stroke literals across the listed product files. No snap values changed; the design hash stayed byte-identical.

## 2 Change list

- `product/AntiNotepad/Theme/Space.cs:1-30` | spacing scale, editor derives, and named exceptions | step line 16, 21-22
- `product/AntiNotepad/Theme/Shape.cs:1-18` | thin/thick strokes and checkbox drawing exceptions | step line 17, 22
- `product/AntiNotepad/Theme/Typography.cs:22-25` | caps tracking and editor chip scale | step line 22
- `product/AntiNotepad/Theme/ChromeTokens.cs:20-118` | scale aliases, derived overlay margins, and named layout sizes | step line 18-22
- `product/AntiNotepad/Editor/Completion.cs:99,119,146` | popup sizes and text-fit columns | step line 20-22
- `product/AntiNotepad/Editor/PadView.cs:100,131,1026-1040` | named insets and hint derives | step line 20-22
- `product/AntiNotepad/Editor/Renderers.cs:254,399-424` | named chip and checkbox exceptions | step line 22
- `product/AntiNotepad/Platform/TaskbarPresence.cs:143-147` | hidden helper size token | step line 22
- `product/AntiNotepad/UI/HelpOverlay.cs:20-61` | derived bottom margin and text-fit key width | step line 21-22
- `product/AntiNotepad/UI/Kit.cs:186-235` | spacing and stroke aliases; caps tracking | step line 20-22
- `product/AntiNotepad/UI/MotionPreview.cs:108-226` | spacing, panel size and sample dimensions | step line 20-22
- `product/AntiNotepad/UI/PadMenu.cs:101-303` | spacing, strokes, menu sizes and derived icon box | step line 20-22
- `product/AntiNotepad/UI/PadWindow.cs:80-81` | named pad minimum size | step line 20
- `product/AntiNotepad/UI/PadWindow.Layout.cs:49,696-778` | toast and save card sizes, spacing and stroke | step line 20
- `product/AntiNotepad/UI/SearchOverlay.cs:47-257` | spacing, strokes, result size and derived bottom margin | step line 20-21
- `product/AntiNotepad/UI/SettingsParts.cs:31-667` | row, dropdown, toggle and sample tokens | step line 20-22
- `product/AntiNotepad/UI/SettingsWindow.cs:47-698` | window, dialog, row and sample sizes; spacing aliases | step line 20

## 3 Behavior changes

None intended. Token substitutions retain their existing values. Search and help bottom margins, editor hints, and the toggle travel distance are expressed as derives with the same results.

## 4 Out-of-scope files

No out-of-scope product or tooling files changed. The step report and gate report are required check records under `.coord/reports/`.

## 5 Deviations

- The gate’s default `dev` base and the item-parent base included previously approved lab files, so their Scope checks failed. The final gate used `-Base 808e021`, the approved step-start commit, and passed Scope.
- The requested check build was created at `build/A35d-spacing-tokens-check` and tested. Its recursive cleanup was rejected by execution policy after the target was verified inside this worktree; that check folder remains. The successful gate removed its own build and data folders.

## 6 Checks run

- Build: `dotnet build product/AntiNotepad/AntiNotepad.csproj -c Debug -o build/A35d-spacing-tokens-check -p:NuGetAudit=false`, with `AVALONIA_TELEMETRY_OPTOUT=1` — succeeded, 0 warnings, 0 errors.
- Self-test: `CONTRACTS: 42 rows, 40 pass, 1 manual, 1 deferred`; `SELFTEST PASS`.
- DESIGN dump: `DESIGN dump f08e0c448f3f7a400dd73fe7710bd51e05f6545782c40b3caa6ed5c24466aa79 18048`.
- Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A35d-spacing-tokens -Base 808e021 -Offline` — `GATE: PASS`; build 0 warnings/errors; Selftest, Seam, Tokens, Format and Scope passed.
- TOKENS lines: `TOKENS baseline can be lowered: run -Update`; `TOKENS PASS`.
