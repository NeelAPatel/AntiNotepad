# A23-paste-keep-structure-r5 report

## Files changed
- Editor/PasteHtml.cs: scans tags by offset and rejects nesting beyond 64 levels before recursive walks.
- Editor/PasteEngine.cs: falls back for HTML over 1,000,000 characters.
- Editor/PasteTable.cs: measures emoji grapheme clusters as two columns.
- Editor/PadView.cs: queues paste requests and captures editor state after each request enters the queue.
- SelfTest/Paste.cs: checks deep nesting, the input budget, 4,000-row conversion speed, and emoji widths alongside existing cases.
- .coord/STATUS.md: records the rework state.

## Clipboard call
The existing hook uses Avalonia IClipboard.GetDataFormatsAsync, TryGetTextAsync, TryGetValueAsync for HTML Format, and TryGetFilesAsync.

## Checks
- Release build: 0 warnings, 0 errors; NuGet audit disabled offline.
- Selftest: SELFTEST PASS, including the previous paste checks and the new cases; the 4,000-row conversion completed within the one-second selftest limit.
- tools/gate.ps1 -Task A23-paste-keep-structure-r5 -Offline: GATE PASS. Existing loop budget and PadView size warnings remain.
- git diff --check and the three Paste files against dev: clean.
- Cleanup of build/A23-check was rejected by execution policy; the gate cleaned its own build folder.

## Not verified
Live clipboard, overlapping key presses, toast display and undo were not exercised under the offline-only instruction. The async paste queue depends on the editor and clipboard, so no offline pure selftest covers it. The existing normal-paste fallback delegates to AvaloniaEdit's asynchronous paste command. No commit was made.
