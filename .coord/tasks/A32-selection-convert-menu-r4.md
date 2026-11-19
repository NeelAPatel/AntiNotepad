# A32-selection-convert-menu-r4: Numbered list icon matching the bullet icon

Rework round of `A32-selection-convert-menu` (`docs/loop.md`). Read the item card `.coord/tasks/A32-selection-convert-menu.md` first: its Plan, Files, Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Medium, same as the card.
Role: quick, because one icon in one file.

## Why this round
Hand test (user, 2026-11-19): Checklist and Bullet list icons are fine; the drawn "1 2 3" stack for Numbered list "looks weird". Segoe Fluent Icons has no numbered-list glyph (checked all 1,574 glyphs in E700-F8FF; only `\uE8FD` bullets, `\uE762` check list and `\uE71D`/`\uE8A4` plain lists exist). Redraw the Numbered list icon to look like the Bullet list glyph `\uE8FD`: three rows, each a small digit (1, 2, 3) in the place of the bullet dot on the left and a thin horizontal line to its right, same overall size (about 18 px), same stroke weight and `p.Ink` colour as the glyph, centred in the same 28 px icon column. Draw the lines as thin shapes or one path (not stacked text); the digits are small `TextBlock`s in the menu font with a `Typography` size token (add a token only if none fits; no new raw literals outside `Theme/`). Compare it side by side with the `\uE8FD` row so lines align on the same rows.

## Change to the plan
None.

## Files
- `product/AntiNotepad/UI/PadMenu.cs`

## Done means
- The Numbered list row shows digits-with-lines in the bullet icon's layout; the other icons are unchanged.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A32-selection-convert-menu-r4 -Offline` passes.

Report: `.coord/reports/A32-selection-convert-menu-r4.md`, in the card's report format.
