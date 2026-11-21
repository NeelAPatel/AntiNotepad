# A35-ux-pass-s2: icon inventory (stage 2)

Role: terra, read-only scan with judgment across several files (`docs/agents.md`).
Sandbox: read-only, offline. Never launch or kill AntiNotepad, never build, never edit or create files, never commit.
Your **final message is the report**. Goal: `.coord/tasks/A35-ux-pass.md` (whole-app UX audit). Stage 2 lists every icon
the app draws, so the item chat can render a contact sheet and propose one icon registry.

## Background
During `A29-shortcut-map` a Shortcuts icon rendered as a person with a do-not-disturb sign. The app uses two icon
fonts: system **Segoe Fluent Icons** (`Glyph.Font` in `UI/Kit.cs`, with `Segoe MDL2 Assets` as fallback) and the bundled
**Fluent UI System Icons** (`Glyph.Fluent`, `Assets/Fonts/FluentSystemIcons-Regular.ttf`). Their code tables differ.
See `.coord/tasks/A29-shortcut-map-r4.md` item 4 and `.coord/reports/A29-shortcut-map-r4.md`.

## Do
1. **Every icon use.** Search all of `product/AntiNotepad` (including `SelfTest/` for checks that touch glyphs) for
   `Glyph.`, `GlyphIcon(`, `char.ConvertFromUtf32(`, `\u` escapes and literal Private Use Area characters (U+E000 to
   U+F8FF; note `Glyph` constants in `Kit.cs` and `MenuEntry.cs` are literal PUA characters, so give their hex code),
   `0xE`/`0xF` codes passed as glyphs, and any other icon font use (dev badge `0xE0A0` in `PadWindow.Layout.cs`). Per use:
   file:line, screen (pad bar, pager, menu, Settings sidebar, Settings row, search, toast, card, help/shortcut map, dev
   badge), code (hex), font (Segoe or bundled, and how you know: default `GlyphIcon` font vs explicit `Glyph.Fluent`),
   name in code (constant name or nearby label), intended meaning, size (token name and value), weight argument, and what
   changes it at runtime (hover color, selected, disabled alpha, rotation).
2. **Raw codes.** List every use that passes a raw number or literal character instead of a named constant.
3. **Same meaning, different glyph** and **same glyph, different meaning**: list any.
4. **A29 cause.** From git history (`git log -p -S "f6ed"` and `-S "f4b8"` across `product/`) say which font 0xF6ED was drawn
   in, where it came from, and what replaced it. Do not guess what 0xF6ED looks like; the item chat renders it.
5. **Existing checks.** Does any selftest check that a glyph exists in its font (`ContainsGlyph`, `CharacterToGlyphMap`)?
   List them with file:line.

## Report format (final message)
```
# A35-ux-pass-s2: icon inventory
## Summary      (counts: uses, distinct codes per font, raw-code uses, mismatches)
## Icons        (table: file:line | screen | code | font | name | meaning | size | weight | runtime changes)
## Raw codes    (table)
## Mismatches   (table)
## A29 cause    (short, with commit hashes and file:line)
## Glyph checks (table or "none")
## Checks run
READY: yes | no - <what is missing>
```
