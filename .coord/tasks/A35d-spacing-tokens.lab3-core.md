# A35d-spacing-tokens lab, round 3, part core

Role: terra, the shared base every other run depends on; correctness of tokens and Fluent controls matters most.

First read `.coord/tasks/A35d-spacing-tokens.lab3-common.md` in full and follow it. Write in small saved steps.

**Your files:** `tools/playground/a35d-lab/core.css`, `tools/playground/a35d-lab/core.js`, plus `tools/playground/a35d-lab/kit-test.html` (a plain page that shows every kit builder once in both modes, for checking).
**Source to copy:** `Theme/*`, `Motion/MotionTokens.cs`, `UI/Kit.cs`, `UI/SettingsParts.cs` (the builders), `UI/PadBar.cs`, the toast code in `UI/PadWindow*.cs`.
**Captures to match:** the Settings captures for card, row, toggle, segments, dropdown, swatch and button looks; `pad-*` for icon buttons, pill, badge and meta text.
**Snap rows you wire:** all rows 1-19 as variables (both modes); row 19 in the Fluent classes.

**Must work:** every builder in the Core API section, with hover, press, focus and selected states matching the captures.

Report: `.coord/reports/A35d-spacing-tokens-lab3-core.md`.
