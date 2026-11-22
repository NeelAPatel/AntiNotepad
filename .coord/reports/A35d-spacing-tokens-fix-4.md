# A35d-spacing-tokens fix 4: Open at tile reflow

Changed `SettingsWindow.cs:643` from a five-column `UniformGrid` to a left-aligned `WrapPanel` with `Space.S` item and row spacing. Tile width remains 104 DIP. Wrapped rows keep their tile widths and do not stretch to fill the row. No new size token was needed.

Changed `SelfTest/Design.cs:21` to pin the new design hash: `2ec30e61e4a5deb801f4069db20750a787af9755db65a315b0a01f34d8344e21`.

Design dump diff: before `d21b66fc4c10df7ca18d119e4514b8c18dd8b29031999af819f02bc47c209a71`; after the hash above. The `Open at` subtree changed from `UniformGrid` to `WrapPanel`; the textual diff has 288 removed and 288 added lines because the control path changes for each descendant.

Checks: offline build to `build/A35d-spacing-tokens-check` (0 warnings); `--design-dump` before and after; `--selftest` via the gate (`SELFTEST PASS`); `pwsh -NoProfile -File tools/gate.ps1 -Task A35d-spacing-tokens -Base 3b6ecb5 -Offline` (`GATE: PASS`). Removed the check folder afterward.
