# A35-ux-pass-s2: icon inventory and contact sheet (stage 2)

Written by the item chat, 2026-11-20. Inventory scan by Codex terra, read-only ([raw report](A35-ux-pass-s2.codex.md));
font facts by two item-chat scripts that read the font files directly. Report only; no product file changed.

## Summary
- **A29 cause confirmed.** Code `U+F6ED` in the bundled Fluent System Icons font is
  `ic_fluent_person_subtract_16_regular`: a person with a minus disc, which reads as "do not disturb". Segoe Fluent Icons
  has no glyph at that code. The Settings sidebar draws its icons with the bundled font (`Fl(0x...)` and the sidebar
  helper), so the guessed code drew a real, wrong icon instead of a blank. The keyboard is `U+F4B8`
  (`ic_fluent_keyboard_20_regular`). Git history no longer holds the 0xF6ED line (Codex searched); the r4 brief records it.
- **56 icons from two fonts:** 20 Segoe Fluent Icons codes (pad bar, pager, menus, search, close) and 35 bundled Fluent
  codes (all of Settings), plus the dev badge (`U+E0A0` from Cascadia, with a text fallback) and 6 hand-drawn Settings
  pictures.
- **Every current code exists in the font it is drawn with** and its glyph name matches its meaning (bundled font names
  checked by script; Segoe has no glyph names in the file, so its 20 were checked by eye on the sheet).
- **Silent-failure risk is real for 32 of 55 codes.** All 20 Segoe codes also exist in the bundled font as unrelated icons
  (Close `E711` is a house, Delete `E74D` a one-handed keyboard), and 12 bundled codes exist in Segoe. Passing the wrong
  font draws a wrong icon, never a blank box. That is exactly the A29 failure.
- **44 raw-code uses**, all in Settings (`SettingsWindow.cs` sidebar and rows, `SettingsParts.cs` chevrons, settings
  search). The pad side uses named constants (`Glyph`, `MenuGlyphs`) but those hold literal PUA characters with no name
  check.
- **No selftest checks that a glyph exists or matches.** Only "the font file loads".

## Contact sheet
`build/a35-icon-sheet.html`, generated from the font files by
[`A35-ux-pass-s2-sheet.py`](A35-ux-pass-s2-sheet.py) and [the icon list](A35-ux-pass-s2-icons.tsv). Each card shows the
icon at its app size, at 48 px, and in red what the same code draws in the other font. Light/dark toggle and a
font filter. Open the file directly in a browser. Regenerate:
`python -I .coord/reports/A35-ux-pass-s2-sheet.py .coord/reports/A35-ux-pass-s2-icons.tsv product/AntiNotepad/Assets/Fonts/FluentSystemIcons-Regular.ttf C:/Windows/Fonts/SegoeIcons.ttf build/a35-icon-sheet.html`.
The page is **not committed**: it embeds Segoe Fluent Icons outlines, a Windows system font we do not redistribute.

## Findings
| # | Finding | Lane | Evidence | Sev | Fix (stage 4) |
|---|---|---|---|---|---|
| I-1 | Two icon fonts, codes not tied to a font; wrong font draws a wrong icon silently | icons | A29; 32 of 55 codes collide | 🔴 | Registry with font per entry (below) |
| I-2 | 44 raw codes in Settings, no names | icons | `SettingsWindow.cs:33-35, 205, 338-761`; `SettingsParts.cs:148, 357` | 🟡 | Registry names |
| I-3 | Same meaning, different glyph: Search (Segoe `E721` pad, bundled `F68F` Settings), Keep on top (Segoe pin `E718` vs bundled pin `F601`), Hide (Segoe dash `E921` vs bundled arrows `F165`) | icons, N4 | sheet | 🟡 | One glyph per meaning in the registry; Settings row icons match the pad's |
| I-4 | Same glyph, different meaning: trash can `E74D` = delete note, delete selection and open trash; `E76B` = previous note and leave trash; `F418` folder = Storage, Notes folder, Recovered notes; `F359` desktop = System, Taskbar icon, App data; `F4B8` keyboard on 5 rows | icons, N4, I5 | sheet | 🟢 | Accept trash and back reuse (same verb); give Recovered notes and App data their own glyphs |
| I-5 | Bundled 20 px designs drawn at 16 (Settings search) and 14/16 (chevrons) | visual, I2 | `SettingsWindow.cs:205`, `SettingsParts.cs:148, 357` | 🟢 | Use the `_16` variants, or draw at 20 |
| I-6 | Mixed styles: Settings uses Fluent System Icons (rounder, 20 px grid), the pad uses Segoe Fluent (16 px) | visual, I3 | sheet | 🟢 | No change; they never sit side by side. Recorded as a rule in the registry |
| I-7 | `Glyph.Restore` defined, unused | icons | `Kit.cs:18` | 🟢 | Delete |
| I-8 | No icon check in selftest or gate | icons, Encode lessons | Codex scan | 🔴 | Selftest and gate step (below) |

## Proposal: registry and checks (stage 4, needs Approve)
1. **One registry**, `UI/Icons.cs`: `record Icon(string Name, IconFont Font, int Code, string FontGlyphName, string
   Meaning)`, one static field per icon (`Icons.Keyboard`, `Icons.SettingsSearch`). `GlyphIcon` takes an `Icon`, not a
   string and font, so a code can never be drawn in the wrong font. `Glyph`, `MenuGlyphs`, `Fl(int)` and the sidebar's
   raw codes migrate in the same change and are deleted.
2. **Selftest, two checks per entry:** the code exists in its font (Avalonia `GlyphTypeface` character map), and, for the
   bundled font, the glyph name in the font's `post` table equals `FontGlyphName` (catches A29: `F6ED` exists but is named
   `person_subtract`). Segoe has no names in its file, so its entries check existence only, and the contact sheet stays
   the human check.
3. **Gate step:** the tokens scanner gains an `icon-code` category that fails on `char.ConvertFromUtf32(0x...)`, `\uE...`/
   `\uF...` escapes and literal PUA characters outside `Icons.cs`.
4. **Lesson** (`.coord/lessons.md`): "Look icon codes up by font glyph name (`ic_fluent_<name>_20_regular` or Microsoft's
   Segoe table); never guess a code; add it to `Icons.cs` and run the sheet."
5. **The sheet becomes the review page** for any icon change: the generator reads `Icons.cs` instead of the TSV.

## Checks run
Codex terra read-only scan; `A35-ux-pass-s2-fontcodes.py` on both fonts for every code; the sheet generator; the sheet
viewed in the browser pane (light theme; the A29 card shows the person-minus glyph).
