# A23-paste-keep-structure-r2 report

## Files changed
- `product/AntiNotepad/Editor/PasteEngine.cs`: retain paragraph, heading, div and blockquote text alongside lists, validate balanced tags, and preserve the existing guards and word comparison.
- `product/AntiNotepad/SelfTest/Paste.cs`: add capture C and block, line-break, malformed and mismatch cases; captures A and B remain unchanged.

## Avalonia clipboard call
The existing paste hook uses `GetDataFormatsAsync`, `TryGetTextAsync`, and `TryGetValueAsync(DataFormat.CreateBytesPlatformFormat("HTML Format"))`; this rework did not change the hook.

## Checks
- `dotnet build product/AntiNotepad -o build/A23-check`: passed, 0 warnings and 0 errors.
- `--selftest`: SELFTEST PASS; the final offline gate also rebuilt and passed selftest after the last change.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A23-paste-keep-structure-r2 -Offline`: GATE PASS. It reported existing loop budget and PadView size warnings; NuGet audit was disabled for offline execution.
- Task build folder removed.

## Not verified
Real clipboard, UI behavior, and hand testing were not checked; offline-only scope. The item chat owns the follow-up hand test and review.
