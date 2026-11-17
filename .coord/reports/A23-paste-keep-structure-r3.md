# A23-paste-keep-structure-r3 report

## Files changed
- Editor/PasteEngine.cs: reader table, clipboard guards and fragment extraction.
- Editor/PasteHtml.cs: parses HTML and converts blocks, lists, inline marks and links.
- Editor/PasteTable.cs: aligned tables and unsupported structure guards.
- SelfTest/Paste.cs: exact real-copy fixtures and safeguards. Capture C keeps its words and structure, with bold and code added as required by r3.

## Clipboard call
The existing paste hook uses Avalonia IClipboard.GetDataFormatsAsync and TryGetDataAsync; this round does not change it.

## Checks
- Build: 0 warnings, 0 errors.
- Selftest: SELFTEST PASS, including captures D and E.
- tools/gate.ps1 -Task A23-paste-keep-structure-r3 -Offline: GATE PASS. The gate reports loop budget and PadView size warnings; offline NuGet audit is disabled.
- Cleanup of the separate build/A23-check folder was rejected by the execution policy.

## Not verified
Live clipboard and UI behavior, undo, and hand tests were not run under the offline-only instruction. No commit was made.
