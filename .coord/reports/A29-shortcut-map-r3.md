# A29-shortcut-map-r3

## Summary
Search shortcuts now toggle the global search overlay.

## Change list
- Route Search and both Search aliases through `ToggleSearch()`.
- Extend the real window-handler selftest to open, close while Search is focused, and reopen Search with Ctrl+F.
- Keep the documented shortcut table aligned with the canonical action name.

## Checks run
- Debug build: passed; initial standalone restore reported offline NU1900 metadata warnings.
- `--selftest`: `SELFTEST PASS` (bounded to 3 minutes).
- `pwsh -NoProfile -File tools/gate.ps1 -Task A29-shortcut-map-r3 -Offline`: passed, build 0 warnings, selftest passed; gate completed cleanup.
