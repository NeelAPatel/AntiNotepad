# A35d-spacing-tokens fix 3

Centered the Settings `_find` TextBox with `Padding = new Thickness(Space.S, 0)` and `VerticalContentAlignment = VerticalAlignment.Center`; kept its 32 DIP height and magnifier unchanged.

Design dump: 56 repeated Settings search-field entries changed, each only `Padding=8,8,8,0` -> `Padding=8,0,8,0`. No other dump lines changed. New `DesignHash`: `d21b66fc4c10df7ca18d119e4514b8c18dd8b29031999af819f02bc47c209a71`.

Checks: offline build, 0 warnings/errors; `--design-dump` before/after and line comparison; `--selftest` (`SELFTEST PASS`); requested offline gate (`GATE: PASS`).
