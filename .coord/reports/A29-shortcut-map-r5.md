# A29-shortcut-map-r5

## Summary

Updated both built-in example notes to tell users to click a checkbox.

## Change list

- `Model/Welcome.cs`: changed the checklist example instruction to “Click the box to check off this example.”
- `Model/Showcase.cs`: changed the example line to “[ ] Click a box to tick it.”
- No selftest pins either sentence; no test changes were needed. `Ctrl+Enter` remains in shortcut behavior tests and guidance, but no longer appears in built-in note text.

## Checks run

- Build to `build/A29-shortcut-map-check`: 0 errors; 4 `NU1900` warnings because the vulnerability feed was unreachable. Output folder deleted.
- `--selftest`: `SELFTEST PASS`.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A29-shortcut-map-r5 -Offline`: `GATE: PASS`; gate build had 0 warnings and 0 errors. Existing size and budget notices remain non-blocking.
- Confirmed no `Ctrl+Enter` match under `product/AntiNotepad/Model`.
