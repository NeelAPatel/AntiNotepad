# A29-shortcut-map-gatefix: make the A29 gate pass

Role: worker. Same branch and worktree as `A29-shortcut-map` (read its card `.coord/tasks/A29-shortcut-map.md` first; its Plan stands). Never commit, switch branches, launch or kill AntiNotepad, run `--quit`, or edit `.coord/STATUS.md` or either card. Offline only.

## State now
The build is 0 warnings and `--selftest` prints `SELFTEST PASS` (the earlier hang is gone; do not run a selftest without a 3-minute limit). The gate `pwsh -NoProfile -File tools/gate.ps1 -Task A29-shortcut-map -Offline` fails on three steps. Fix only these:

1. **Format:** `product/AntiNotepad/UI/Shortcuts.cs` uses LF endings and some multi-statement lines the formatter rewrites. Run `dotnet format whitespace product/AntiNotepad --include <file>` for each changed file (the gate names them), then rebuild. Other changed files may also report problems on changed lines.
2. **Tokens:** `pwsh tools/check-design-tokens.ps1` reports spacing literals up in `UI/HelpOverlay.cs` (6 to 10) and `UI/HotkeyRecorder.cs` (0 to 2). Replace the new literal `Thickness(...)`, `Margin` and `Padding` numbers with existing `ChromeTokens`/`Theme` spacing values where one matches (for example `IconSpacing`, `BadgeSpacing`, `MetaSpacing`), or add one named token in `Theme/` per needed value. Do not run `-Update` and do not add new literals. The Help and Settings layout must look the same as now.
3. **Design hash:** if the layout dump changes, `SelfTest/Design.cs` `DesignHash` needs the new hash (the selftest prints it). Keep it equal to the final dump.

Then re-run the gate until `GATE: PASS` (Scope is already fixed in the card). Delete `build/A29-shortcut-map-check*` folders when done.

## Report
`.coord/reports/A29-shortcut-map-gatefix.md`: 1 What failed and the fix per step; 2 Files changed; 3 Final gate and selftest lines; 4 Anything that still fails and why.
