# A22-wrap-hanging-indent

## Summary
Wrapped continuation lines now start at the measured text column after list markers or leading whitespace. The editor history audit found no prior fix or change that undid one.

## Change list
- Vendored AvaloniaEdit 12.0.0 and replaced the package reference with the local project.
- Added a `TextView` callback backed by `ListEditing`; applied its measured offset to wrap width, drawing, hit testing, caret, and selection geometry.
- Added wrap and fork-documentation contracts, upstream patch notes, dependency upgrade steps, and shipped the MIT license with the app.

## Behavior changes
Bullets at levels 1–3, top-level bullets, numbered items, checkboxes, and plain indented lines wrap under their first text character. Unindented lines keep their left edge. Document text is unchanged.

## Out-of-scope files
No files outside the item card's listed directories and files were changed. No `PadWindow*`, Settings, Store, STATUS, or item card files were touched. The gate's exact-file scope check flagged the vendored files under the approved `third_party/AvaloniaEdit/` directory.

## Plan deviations
The vendored project targets `net10.0`, pins Avalonia to the app's existing 12.1.4, and disables signing. These local build settings make the source project match the app runtime. Added a local ignore for its generated `bin/obj` folders.

## Contracts
- `wrap-indent-text-column`: passed across 96 layout combinations; validates text-column alignment, unchanged text, caret, selection, and hit testing.
- `avaloniaedit-fork-documented`: passed; checks the 12.0.0 pin, every source marker, project reference, and shipped license.
- All existing checks passed. `CONTRACTS`: 21 rows, 20 automated passes, 1 existing manual row.

## Checks run
- Offline Debug build to `build/A22-check`: 0 warnings, 0 errors.
- `build/A22-check/AntiNotepad.exe --selftest`: `SELFTEST PASS`; 6/6 pure offset cases and 96 layouts (8 line types, 2 fonts, 2 sizes, 3 widths).
- `tools/gate.ps1 -Task A22-wrap-hanging-indent -Offline`: build, selftest, platform seam, tokens, format, and size passed. Scope failed only because the gate compares the listed directory as an exact file path; it flagged 249 paths within the vendored tree. The gate was not changed.
- `dotnet clean` completed, but the copied files remain in `build/A22-check`; workspace policy rejected recursive deletion, so that temporary output could not be removed.

## Hand checks for the user
No app was launched and no screenshot was captured, per the Codex run constraints. After A21 merges and a dev build is available, compare the supplied nested-bullet example at a narrow width. Check levels 1–3, top-level bullets, numbered items, checkboxes, plain indented and unindented text across widths and fonts; move the caret, select wrapped text, and click to place the caret.
