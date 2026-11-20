# A34-pill-bar-per-mode-r11

## Change list
- Added `UI/PadBar.cs` with per-pane containers, ordered `BarItem` registry, `BarContext`, `BarActions`, and the existing bar rendering and sizing.
- Moved window placement, fade, overlay, and popup handling to `_bars`; updated bar and UI selftests for registry order/visibility, delegate actions, a second bar, and moved controls.
- The parent item card already lists `UI/PadBar.cs` under `## Files`.

## Behavior changes
None expected. The visual tree hash remains `f08e0c448f3f7a400dd73fe7710bd51e05f6545782c40b3caa6ed5c24466aa79` (18,048 characters).

## Plan deviations
- The requested cleanup of `build/A34-check` was blocked by execution policy. The folder remains.
- No other source files were needed.

## Gate result
- `dotnet build` to `build/A34-check`: 0 warnings, 0 errors.
- `build/A34-check/AntiNotepad.exe --selftest`: PASS; design hash unchanged. Contracts: 40 rows, 39 pass, 1 manual.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A34-pill-bar-per-mode -Offline`: PASS; build, selftest, seam, tokens, format, scope, and size passed. Loop lint skipped because no loop files changed.
