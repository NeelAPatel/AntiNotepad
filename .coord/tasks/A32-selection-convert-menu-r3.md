# A32-selection-convert-menu-r3: submenu corners and list icons

Rework round of `A32-selection-convert-menu` (`docs/loop.md`). Read the item card `.coord/tasks/A32-selection-convert-menu.md` first: its Plan, Files, Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Medium, same as the card.
Role: terra, because two small visual fixes in the menu files.

## Why this round
Hand test (user, 2026-11-19): the conversion itself passes; two visual bugs in the submenu (screenshot: square window corners show behind the rounded border, and the rows have no icons).
1. **Corner glitches.** The submenu `Popup` in `UI/PadMenu.cs` is built without what the main popup in `PadMenu.Show` has: transparent window hint (`host.TransparencyLevelHint`, `host.Background = Transparent`), `DesktopPlatform.ConfigurePopupWindow(host)` on `Opened`, `WindowManagerAddShadowHint = false`, and the shadow/border styling of `BuildMenu`. Give the submenu the same window setup and the same border, radius, shadow and padding as the main menu; share one helper instead of copying (coding.md: no new literals; use `Shape`, `Paint` tokens).
2. **Icons.** Every row has an icon slot. Add `MenuGlyphs.Checklist = "\uE762"` (check list) and `MenuGlyphs.Bullets = "\uE8FD"` (bulleted list), in `Editor/MenuEntry.cs`, and use them for the Checklist and Bullet list rows. Segoe Fluent Icons has no numbered-list glyph, so the Numbered list row draws a small icon in `PadMenu.Icon`: a narrow stack of three short "1", "2", "3" text lines (use the menu font and a `Typography` size token, `p.Ink` colour, same 28 px icon column and centring as `GlyphIcon`). The "Convert to list" parent row uses `MenuGlyphs.Checklist`. Colours follow the existing `colored` handling (neutral `p.Ink`).

## Change to the plan
Visual polish only; behavior unchanged. Add the icons in `PadView.ConvertMenu` entries (`MenuEntry` already has an Icon field).

## Files
- `product/AntiNotepad/UI/PadMenu.cs`
- `product/AntiNotepad/Editor/MenuEntry.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/SelfTest/Lists.cs`

## Done means
- The submenu popup uses the same transparency, window configuration, border, radius and shadow as the main menu; all four rows (parent and three children) have icons; existing selftests still pass (update the menu factory check only if the entry shape changed).
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A32-selection-convert-menu-r3 -Offline` passes; run `dotnet format product/AntiNotepad --include <changed files>` before the gate.

Report: `.coord/reports/A32-selection-convert-menu-r3.md`, in the card's report format.
