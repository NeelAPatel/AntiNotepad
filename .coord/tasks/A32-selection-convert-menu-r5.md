# A32-selection-convert-menu-r5: Numbered list icon drawn like Word's

Rework round of `A32-selection-convert-menu` (`docs/loop.md`). Read the item card `.coord/tasks/A32-selection-convert-menu.md` first: its Plan, Files, Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Medium, same as the card.
Role: quick, because one icon in one file.

## Why this round
Hand test (user, 2026-11-19): corners are fixed. The r4 Numbered list icon is still wrong; the user wants the Word "numbered list" icon (digits 1, 2, 3 stacked in a narrow left column, each with a short horizontal line to its right). Cause found: `NumberedIcon` in `UI/PadMenu.cs` puts 12 px `TextBlock` digits (`Typography.Micro`) into rows only 16/3 px high, so the digits overlap and clip, and 2 px lines are too heavy.

## Change
Replace `NumberedIcon` with one small custom `Control` (or a `Canvas` of `Path` shapes) that draws everything as geometry, no `TextBlock`s, inside the same 28 px column and `Typography.IconMedium` (16 px) square:
- three rows with centres at about 3, 8 and 13 px; each row has a line from x about 7 to 16, about 1 px thick;
- to the left of each line a tiny hand-drawn digit about 3 px wide and 4-5 px tall (1: a short vertical stroke with a small flag; 2: top arc, diagonal, base; 3: two small arcs), stroke about 1 px, round caps;
- all in `p.Ink`, stroke widths scaled so it reads like the Segoe Fluent icons next to it.
Numbers like 1 px and 3 px are icon geometry, not theme tokens; keep them as named private constants in the file. Remove the now-unused text and `Shape.Hairline` usage there. Other icons are unchanged. Compare against the bullet row (`\uE8FD`) so the three lines sit on the same rows.

## Files
- `product/AntiNotepad/UI/PadMenu.cs`

## Done means
- The Numbered list row looks like Word's numbered-list icon; digits do not overlap or clip.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A32-selection-convert-menu-r5 -Offline` passes.
- Never launch AntiNotepad. Offline checks only.

Report: `.coord/reports/A32-selection-convert-menu-r5.md`, in the card's report format.
