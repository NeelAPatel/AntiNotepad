# A23-paste-keep-structure-r4 report

## Files changed
- Editor/PasteEngine.cs: exposes the conversion outcome and shared preformatted guard.
- Editor/PadView.cs: uses the existing toast for converted or fallback structured pastes.
- SelfTest/Paste.cs: checks the requested outcomes and guarded code and markdown cases.

## Clipboard call
The existing hook uses Avalonia IClipboard.GetDataFormatsAsync, TryGetTextAsync, TryGetValueAsync for HTML Format, and TryGetFilesAsync.

## Checks
- Build: 0 warnings, 0 errors. Cleanup of build/A23-check was rejected by the execution policy.
- Selftest: SELFTEST PASS, including earlier paste checks and the new outcome cases.
- tools/gate.ps1 -Task A23-paste-keep-structure-r4 -Offline: GATE FAIL solely on scope: the existing branch change tools/playground/a23-paste-pill-lab.html is absent from the item and rework Files lists.
- The same gate with -LooseScope: GATE PASS, with that scope warning; loop budget and PadView size warnings also remain. The generated gate report records this loose-scope run. Offline NuGet audit was disabled.

## Not verified
Live clipboard, toast display and timing, undo, and hand tests were not run under the offline-only instruction. No commit was made.
