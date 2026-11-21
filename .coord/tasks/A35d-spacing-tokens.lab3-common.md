# A35d-spacing-tokens lab, round 3: shared rules and contracts (read in full)

The user's goal: a before/after mockup of the A35d spacing snaps that is **pretty much indistinguishable from the
real AntiNotepad**. Every button and control can be clicked, and every hover and press state works. Deep functions can be
faked; the look cannot. Rounds 1 and 2 failed: round 1 was loose, and round 2 tried to write whole pages in one command
and never saved. This round splits the lab into small files, each written by its own run.

## How to write (hard rules)
- **Write in small steps.** Create your file with a skeleton first, then add one section per command. Keep each write
  under about 300 lines. Save after every section, so a stopped run still leaves working parts.
- Touch **only your own files** (named in your brief). Never edit another run's file, product code, `.coord/STATUS.md`
  or the item card. Do not commit. Never launch AntiNotepad.
- Offline only: no server, port or browser. Check with `node --check` on each `.js` file, and on scripts extracted from
  `.html`, when Node is available. Check every DOM id you use by reading the file.

## Inputs
- The snap list: `.coord/reports/A35d-spacing-tokens-snaps.md`. Rows 1-19 are the only things After changes. Derive and
  exception rows never change.
- The source code is the truth for every value: `product/AntiNotepad/Theme/*`, `Motion/MotionTokens.cs`, `UI/*`,
  `Editor/*`, `Model/Settings.cs`. The demo note text is in the repo; find it and use it word for word.
- **Real captures of today's app (look at the ones for your area):** `build/shots/a35d/`. These are exact-pixel grabs,
  1 DIP = 1 px, with a 1 px window border, in the default dark theme (Ink) on an Acrylic backdrop. Older captures and
  measurements are in `docs/playground/shots/` and `docs/playground/measure.md`.
  - `settings-<Page>-<n>-*.png`: each Settings page, scrolled one page further per `n` (0 is the top).
  - `pad-*`, `help-top-*`, `help-down-*`, `search-empty-*`, `search-results-*` (typed "the"), `split-*` (922 wide).
  - Menu: `docs/playground/shots/menu-*`.
- Fluent controls look like Avalonia 12's Fluent theme (dark). Use the standard Fluent dark values: control fill
  `#FFFFFF0F`, hover `#FFFFFF15`, pressed `#FFFFFF08`, control stroke `#FFFFFF12` with a bottom stroke `#FFFFFF18`, and
  text box focus underline in the accent color. Then adjust each value to match the captures where they show the control.
- Fonts: `'Segoe UI Variable Text', 'Segoe UI'` for the UI, `'Segoe Fluent Icons'` for glyphs (code points from the
  source), and the editor font from the code. All are installed. No web fonts, no network, no external scripts.

## Files (all in `tools/playground/a35d-lab/`)
| File | Written by | Holds |
|---|---|---|
| `core.css`, `core.js` | run 0, first and alone | tokens, palettes, mode, show-changes, backdrop, Fluent controls, both kits |
| `pad.html` | run 1 | the pad window: top bar, editor, pill bar, pager, split, menu, toast, save-failure card, completion |
| `help.js` | run 2 | the F1 help overlay |
| `search.js` | run 3 | the search overlay |
| `settings.html`, `settings-a.js` | run 4 | the Settings window shell and nav, Theme and UI pages |
| `settings-b.js` | run 5 | Editor and Feel pages (Feel has the motion preview) |
| `settings-c.js` | run 6 | System, Shortcuts, Storage and About pages |
| `index.html` | run 4 | the host: both windows, the Before / After / Flip switch, Show changes, theme picker |

Runs 1 to 6 run at the same time and use only the core API below. If a run needs something core lacks, it adds a small
local helper in its own file and names it in its report.

## Core API (run 0 builds it exactly; runs 1 to 6 rely on it)
- `LAB.T`: all tokens, each group with a comment naming its source file.
- `LAB.palettes`, `LAB.setTheme(id)`: sets CSS variables `--canvas --ink --muted --faint --surface --accent` and any
  others the code uses, on `:root`.
- `LAB.mode` ('before' or 'after'), `LAB.onMode(fn)`, `LAB.snap(row, before, after)` returns the value for the current
  mode. CSS reads `--snap-<row>-<what>` variables that `core.js` sets for every row 1-19 in both modes.
- `LAB.mark(el, row)`: registers an element that a snap row changes, for Show changes (magenta outline plus a tag).
- `LAB.glyph(codepoint, size, color)` returns a span.
- `LAB.backdrop(el)`: the Acrylic look (dark blurred gradient close to the captures, with the code's tint).
- Fluent classes: `.fl-button`, `.fl-textbox`, `.fl-combo`, `.fl-scroll`. Their corner radius reads `--snap-19-radius`
  (3 Before, 4 After). Controls that set a radius from `Shape` in the code keep their own radius.
- `LAB.kit.settings`: one builder per control that `UI/SettingsParts.cs` and `UI/Kit.cs` build for Settings, named
  after the C# methods (for example `section`, `card`, `row`, `expander`, `toggle`, `segments`, `dropdown`, `swatches`,
  `textButton`, `textBox`, `pictureChoice`). Each returns a DOM element with working states.
- `LAB.kit.pad`: `iconButton`, `pill`, `toast`, `badge`, `metaText`, the window frame, and the overlay panel header
  (`Kit.PanelHeader`).
- Messages between frames: `{lab:'mode', mode}`, `{lab:'changes', on}`, `{lab:'theme', id}`, `{lab:'open-settings'}`.
  `core.js` handles the first three. Each page posts `open-settings` to its parent.
- When a page has no parent, core shows a small floating lab switch (Before | After | Flip, Show changes), and the key
  `B` toggles the mode. It stays out of the app's area.

## Report
Write `.coord/reports/A35d-spacing-tokens-lab3-<your part>.md`, short, with these sections:
1. Files written.
2. Controls that work.
3. Snap rows wired (row, element, before, after).
4. Per capture you matched: what still differs ("none" only when true).
5. Helpers you added because core lacked them.
6. Checks run.

## Added after the core run
- Icon fonts: the app uses two. `Glyph.Fluent` in the code is the bundled `FluentSystemIcons-Regular.ttf` (now in
  `a35d-lab/`); call `LAB.glyph(code, size, color, 'fluent')` for those. Other glyphs use Segoe Fluent Icons; call
  `LAB.glyph(code, size, color)`. Check which family each icon uses in the code.
- Core's API as built is in `.coord/reports/A35d-spacing-tokens-lab3-core.md` (signatures and the snap variable names);
  read it and use those names. `kit-test.html` shows every builder.
