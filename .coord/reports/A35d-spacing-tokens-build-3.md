# A35d-spacing-tokens build step 3

## 1 Summary

Applied snap rows 1–19 with the existing tokens and a shared Fluent setup. Re-pinned `DesignHash` to `40aa4d67373fa5f983d3a8d5a1a40f4f9403c6009ef8157d20c90ad507c24df0`.

## 2 Change list

`file:lines | what | step line`

- `product/AntiNotepad/UI/SettingsWindow.cs:182,193,203,258,346,530,713` | Settings margins and row gaps | step 3, rows 1–5
- `product/AntiNotepad/UI/SettingsParts.cs:361,446,609,678` | dropdown and swatch gaps; picture pens | step 3, rows 6–8
- `product/AntiNotepad/UI/SearchOverlay.cs:47,78,172,201` | overlay, field/empty-state margins and result gap | step 3, rows 9–11
- `product/AntiNotepad/UI/HelpOverlay.cs:20,22` | overlay and panel margins | step 3, rows 12–13
- `product/AntiNotepad/Editor/Completion.cs:116,143` | completion row gaps | step 3, row 14
- `product/AntiNotepad/UI/PadWindow.Layout.cs:777-780` | save-failure action and content gaps | step 3, row 15
- `product/AntiNotepad/UI/MotionPreview.cs:85,107,223,271-272` | preview gaps and sample pen widths | step 3, rows 16–17
- `product/AntiNotepad/Editor/Renderers.cs:101` | folded block highlight radius | step 3, row 18
- `product/AntiNotepad/Program.cs:207-220`; `product/AntiNotepad/SelfTest/Design.cs:21,138`; `product/AntiNotepad/SelfTest/UI.cs:33,270,603,885` | shared Fluent resource setup and hash | step 3, row 19
- `tools/design-tokens-baseline.tsv:1-18` | lowered counts after snapped literals were removed | step 3

## 3 Behavior changes

Settings, search, help, completion, save-failure, motion-preview and folded-block values now use the approved After values. Fluent buttons use `Shape.Control` (4 DIP) through the same setup in the app and selftests.

## 4 Out-of-scope files

None. Source changes and the baseline update are in the card's Files; this report is required by the build brief.

## 5 Deviations

The automatic command policy rejected recursive deletion of `build/A35d-spacing-tokens-check` as “blocked by policy.” The folder remains. No other deviations.

## 6 Checks run

- Before dump: `build/A35d-before.txt`, SHA-256 `f08e0c448f3f7a400dd73fe7710bd51e05f6545782c40b3caa6ed5c24466aa79`, 18,048 lines.
- After dump: `build/A35d-after.txt`, SHA-256 `40aa4d67373fa5f983d3a8d5a1a40f4f9403c6009ef8157d20c90ad507c24df0`, 18,048 lines. Gate output: `DESIGN dump 40aa4d67373fa5f983d3a8d5a1a40f4f9403c6009ef8157d20c90ad507c24df0 18048`.
- Dump diff: same 18,048 control paths; 2,080 changed lines (200 `Margin`, 1,880 `Button.CornerRadius`), no other properties, no added or removed paths, and zero unmapped lines. All 1,880 corner lines are `3,3,3,3` → `4,4,4,4` across 235 logical button paths and eight palette snapshots.

| Snap row | Changed dump control | Before → after | Lines |
|---|---|---:|---:|
| 1 | Settings title `TextBlock.Margin` | `18,0,0,0` → `16,0,0,0` | 56 |
| 2 | Settings nav `StackPanel.Margin` | `10,4,10,10` → `12,4,12,12` | 56 |
| 4 | Settings page `StackPanel.Margin` | `16,2,28,32` → `16,2,32,32` | 56 |
| 9 | `SearchOverlay.Margin` | `14,14,14,46` → `16,16,16,46` | 8 |
| 10 | Search tools `StackPanel.Margin` | `6,0,0,0` → `8,0,0,0` | 8 |
| 12 | `HelpOverlay.Margin` | `10,14,10,40` → `12,16,12,40` | 8 |
| 13 | Help panel `StackPanel.Margin` | `8,0,14,12` → `8,0,16,12` | 8 |
| 19 | Fluent `Button.CornerRadius` | `3,3,3,3` → `4,4,4,4` | 1,880 |

Rows 3, 5–8, 11, 14–18 are not represented in the dump (tokenized gaps, pen widths, drawn radius, or the save-failure card that the dump does not instantiate); the source diff contains their approved values. The search empty-state margin is also absent from this dump fixture.

- Check build: 0 warnings, 0 errors; after-dump command and `--selftest` exited 0; `SELFTEST PASS`.
- Baseline update: `TOKENS baseline updated`. Category totals, old → new: spacing 27 → 4; radius 21 → 19; stroke 6 → 2; color 5 → 5; font-size 4 → 4; opacity 1 → 1; timer 12 → 12.
- Scanner: `TOKENS PASS`.
- Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A35d-spacing-tokens -Base 3b6ecb5 -Offline` → `GATE: PASS`; build 0 warnings/errors, `SELFTEST PASS`, `TOKENS PASS`, format and scope pass.

## 7 Hand checks

| Screen | Look at | Expected |
|---|---|---|
| Settings navigation and page | Title inset, nav list, search icon, page right edge | Title and search icon sit 16 and 12 DIP from their edges; nav list margin is 12/4/12/12 DIP; page right margin is 32 DIP. |
| Settings rows, dropdown, swatches and controls | Caret/shortcut rows, dropdown items, swatch row, Fluent buttons | Row gaps are 8 DIP, dropdown gaps 2 DIP, swatch gaps 4 DIP, picture strokes 1 DIP, and Fluent buttons have 4 DIP corners. |
| Search overlay | Overlay edges, toolbar, results and empty state | Top and side margins are 16 DIP, pager clearance stays 46 DIP, toolbar/empty-state left margin is 8 DIP, and result-line gaps are 4 DIP. |
| Help overlay (F1) | Outer edges and shortcut panel | Outer margins are 12/16/12 DIP with 40 DIP bottom clearance; panel right margin is 16 DIP. |
| Completion popup | Suggestion row spacing and text-fit columns | Rows have 12 DIP gaps; mode and command columns still fit their text. |
| Settings motion preview | Legend, preview stack and sample strokes | Gaps are 16, 8 and 4 DIP at the three snapped sites; sample strokes are 2 DIP. |
| Editor with a folded block | Highlight around the folded text | Highlight radius is 8 DIP and still clears the fold chevrons. |
| Save-failure card | Message, action row and buttons | Vertical gaps, action top margin and button gap are 8 DIP; the card remains readable. |
