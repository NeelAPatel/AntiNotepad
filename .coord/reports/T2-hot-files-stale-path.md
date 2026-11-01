# T2-hot-files-stale-path: report

Built by the Claude builder session, no Codex (fast lane, three lines).

## Summary
Dropped the dead `product/AntiNotepad/SelfTest.cs` name from the loop hot-file lists and the card template.

## Changes
- `tools/catch-up.ps1`: removed `'product/AntiNotepad/SelfTest.cs',` from `$HotFiles`.
- `tools/files-overlap.ps1`: same line removed.
- `.coord/templates/item.md`: Files example now `` `product/AntiNotepad/SelfTest/<area>.cs` ``.

## Checks
- Both scripts parse with no errors (`Parser::ParseFile`).
- `pwsh -NoProfile -File tools/files-overlap.ps1 -Task T2-hot-files-stale-path`: PASS, exit 0.
- `tools/gate.ps1 -Task T2-hot-files-stale-path`: PASS (see card Check).
