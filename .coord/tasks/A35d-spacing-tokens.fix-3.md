# A35d-spacing-tokens fix 3: Settings search hint sits low

Role: quick. Hand-test finding from the user: in Settings, the "Find a setting" hint text sits low in its box. There is
about 12 DIP above the text and 4 DIP below it, while the magnifier icon is centered.

Cause (Claude's reading): `UI/SettingsWindow.cs`, the `_find` TextBox has `Height = SettingsSearchFieldHeight` (32) and
`Padding = new Thickness(Space.S, Space.S, Space.S, 0)`. The top-only padding pushes the text down.

Fix: center the text vertically. Use `Padding = new Thickness(Space.S, 0)` and `VerticalContentAlignment = VerticalAlignment.Center`.
If the placeholder or the caret does not follow `VerticalContentAlignment` in Avalonia 12's Fluent TextBox, pick the
smallest change that centers both the hint and the typed text, and say why in the report. Keep the height at 32 and the
icon as it is.

Rules: read `AGENTS.md` and `.claude/rules/coding.md`. Edit only `product/AntiNotepad/UI/SettingsWindow.cs` and, if the
dump moves, the `DesignHash` in `product/AntiNotepad/SelfTest/Design.cs`. Never switch, merge or commit. Never launch
or kill AntiNotepad, never run `--quit`, and never edit `.coord/STATUS.md` or the card. Use offline checks only.
Build into `build/A35d-spacing-tokens-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--design-dump` before and after,
and confirm that only the `_find` TextBox's lines changed. Re-pin `DesignHash`, run `--selftest`, then delete the folder.
Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A35d-spacing-tokens -Base 3b6ecb5 -Offline`. Done means 0 build
warnings, `SELFTEST PASS`, and the gate passing.

Report `.coord/reports/A35d-spacing-tokens-fix-3.md`, short. Include what you changed, the dump diff lines, the new hash,
and the checks you ran.
