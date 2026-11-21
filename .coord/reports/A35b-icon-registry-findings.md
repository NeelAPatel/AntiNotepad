# A35b-icon-registry: findings detail

Written by the item chat, 2026-11-20. Card: `.coord/tasks/A35b-icon-registry.md`. Base facts: `A35-ux-pass-s2.md`.

## Code today
- `UI/Kit.cs` `Glyph`: 14 Segoe string constants (`Restore` unused) plus `Font` and `Fluent` families.
- `Editor/MenuEntry.cs` `MenuGlyphs`: 12 Segoe string constants; `Delete`, `NewNote`, `Search`, `Split`, `Pin`,
  `Settings` duplicate `Glyph` codes. `MenuEntry.Icon` is `string?`.
- Settings: `KeyboardGlyph = 0xf4b8`, sidebar tuples with raw ints (`SettingsWindow.cs:33-35`), `Fl(0x...)` rows
  (`SettingsWindow.cs:338-761`, 38 raw codes), search box `0xf68f` (`:205`), sidebar face `ConvertFromUtf32(glyph)`
  (`:237`), chevrons `0xf2a3` (`SettingsParts.cs:148, 357`), `Fl(int)` (`SettingsParts.cs:558`).
- Constant comparisons that break when icons become records: `PadMenu.Icon` colors by
  `MenuGlyphs.NewNote or MenuGlyphs.Pin` (accent) and `MenuGlyphs.Delete` (error) (`PadMenu.cs:271-280`);
  `Kit.IconButton` adds the filled-pin layer when `glyph == Glyph.Pin` (`Kit.cs:115`).
- Not icons, must stay legal: `0xF120`, `0xFFFFFFFE`, `0xFFFF` masks (Platform), `0xf900`/`0xfe00` ranges (`PasteTable.cs`),
  `ContainsGlyph(0xE0A0)` (`PadWindow.Layout.cs:507`), dev badge `""` (`AppInfo.cs:61`, Cascadia branch symbol
  with a text fallback).
- Scanner: `tools/check-design-tokens.ps1` masks string contents before matching (only `font-family` keeps them), so a
  PUA character or `` escape inside a string is invisible to it today. Its `font-size` pattern matches the
  trailing numeric weight argument of `new GlyphIcon(...)`, so deleting the weight changes counts in the baseline TSV.

## Faux bold (user addition a)
Every `new GlyphIcon(` passes weight `0` or `ChromeTokens.PlainIconWeight` (= 0): `Kit.cs:112, 120`, `PadMenu.cs:280`,
`PadWindow.Bar.cs:66`, `SettingsParts.cs:148, 357, 558`, `SettingsWindow.cs:205, 237`. No caller uses the 0.15
default. The four-copy path never runs; deleting it changes no pixel.

## Look changes (lab: `build/a35b-icon-lab.html`, regenerate from `build/a35b-lab.tsv`, not committed)
| Where | Before | After | Why |
|---|---|---|---|
| Hide when I click away | `F165` arrow_minimize_20 | `EBD0` subtract_20 | I-3: pad Hide is a dash (Segoe `E921`) |
| Settings search box | `F68F` search_20 at 16 px | `EA7C` search_16 | I-5: 16 px design at 16 px |
| Expand / collapse chevrons | `F2A3` chevron_down_20 at 14/16 | `F2A2` chevron_down_16 | I-5 |
| Recovered notes | `F418` folder_20 (same as Notes folder) | `F47E` history_20 | I-4 |
| App data | `F359` desktop_20 (same as System) | `E542` document_settings_20 | I-4: log file and pointer |

Unchanged on purpose: Search and Keep on top already show the same picture in both places (magnifier, pin); only the
font style differs, which is accepted finding I-6 (the pad and Settings never sit side by side). Trash and back reuse
(same verb) stay per I-4. Keyboard on five Shortcuts rows stays (same meaning).

## Codicons (user addition b)
Sources read 2026-11-20: github.com/microsoft/vscode-codicons README, `LICENSE`, `src/template/mapping.json`,
`src/icons/add.svg`, `src/icons/trash.svg`.
- **License:** repo content CC BY 4.0, code MIT; the font is built from the SVG content. Bundling in a closed app is
  allowed with attribution (not legal advice).
- **Style:** 16x16 viewBox, single filled paths in a thin line look (VS Code's). Closer to Segoe Fluent (16 px) than to
  Fluent System Icons (20 px, rounder).
- **Coverage gaps for our meanings:** no cut, paste, select all, undo (only `discard`), sun or moon (light/dark), grid,
  font, line spacing, motion. It cannot replace the pad's Segoe set (editor menu needs cut, paste, undo, select all) nor
  the Settings set (palette, font, spacing, motion rows).
- **Collisions (measured, item chat's fontcodes script):** codes `U+EA60..ECFB`. 17 of 17 sampled codes (trash `EA81`,
  keyboard `EA65`, close `EA76`, add, search, pin, gear, list, redo, copy, folder, desktop, info, text-size, fold, link)
  exist in the bundled Fluent font as unrelated icons; 7 of 17 exist in Segoe. A third font would add a third colliding
  map, the I-1 risk again.
- **Verdict: not adopt.** It would add font mixing (a third set next to two) and cover fewer of our meanings than
  either current font. The registry makes a later swap a one-line change per icon if ever wanted.
