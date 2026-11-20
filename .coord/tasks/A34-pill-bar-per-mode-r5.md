# A34-pill-bar-per-mode-r5: rebuild the lab as a faithful AntiNotepad mock

Role: terra at CODEX_EFFORT=high, a new lab page (docs/agents.md). Rewrite `tools/playground/a34-pill-bar-lab.html` from scratch (the old file is replaced; reuse its tested navigation, delete and new-note logic if useful). Single file, offline, no external requests, no top-level identifier named `top`, `node --check` clean. Touch no other file.
User (2026-11-20): "I want icons for Delete and New Note. The slot for date: Created date should be the date when the note was last 'not-empty' (delete all characters, 5 s soft save, new characters: now Created is different). Last modified is redundant so change it to an optional Created date slot. The preview lab is pretty unfriendly and poor in quality and not matching the design paradigms and settings of the app we have currently."
Earlier choices (keep): S1 or S3 for the Standard pill, P1 or P3 for the Permanent pill, T1 icon-only swap button top-right, New note bottom-right, Delete bottom-left.

## The problem with the old lab
Many small generic dark cards with walls of text and invented styling. Replace with ONE large faithful live window plus a small control column, like the app itself.

## Read the real source first (do not invent values)
`product/AntiNotepad/Theme/Palette.cs` (8 real palettes, use their exact hexes; light: paper, snow, sage; dark: ink, midnight, graphite; plus the two contrast ones), `Theme/ChromeTokens.cs`, `Theme/Typography.cs`, `Theme/Shape.cs`, `Theme/Paint.cs`, `UI/Kit.cs` (IconButton, Plain, Glyph codes), `UI/PadWindow.Layout.cs` (top row, `_meta` Saved text and chips, `_icons`, BuildPill, `_toast`, bar styles docked/overlay/float, reveal zones), `UI/PadMenu.cs` (`MenuGlyphs.NewNote`, `MenuGlyphs.Delete`: use the same glyphs for the two new buttons), `UI/SettingsWindow.cs` and `UI/SettingsParts.cs` (how Settings rows, toggles and segmented choices look), `Model/Settings.cs`.
Reference screenshots of the real app are in `docs/playground/shots/` (for example `pad-stack-docked-solid-dark-default.png`, `pad-stack1-float-bar-dark-default.png`, `pad-stack1-overlay-bar-dark-default.png`, `pad-stack1-docked-light-default.png`, `toast-copied-dark-default.png`, `search-results-dark-default.png`). Open them if you can and match: window corner radius, border, shadow, bar fades, pill, dot sizes, the "Saved 1:37 PM" text at top-left, icon spacing, toast shape.
Fonts and icons: UI text `Segoe UI Variable Text, Segoe UI`; note text `Cascadia Mono, Consolas`; icons are Segoe Fluent Icons code points (`font-family: "Segoe Fluent Icons"`), exactly the codes in `Glyph` and `MenuGlyphs`, at the token sizes (button 32, glyph 16, hover radius per token, hover and pressed washes as in Kit.IconButton). All px values come from the tokens; put them in one `const T = {...}` object at the top of the script with the source file named in a comment.

## Page layout
- Header: title and a one-line description only.
- Left (about 70 percent): ONE live window mock at real size (default 522 px wide, about 560 tall) with real chrome: top row (left: the meta slot; right: only the T1 swap button, icon-only, showing the current view; no other page icons), editable note text in the real mono font with a sample note, and the bottom bar for the chosen bar style. Below the window: a width switch (320 / 522 / Split: two panes at half width each with its own pager, as the app does) and a Theme palette picker (the 8 real palettes by their names).
- Right (about 30 percent): a "Settings" panel drawn like the app's Settings (same row, toggle and segment styling as `SettingsParts.cs`) holding these live rows. Treat each row as a proposed new setting:
  1. Bar style: Docked / Overlay / Float (real behavior: docked reserves a top row and a bottom strip; overlay bars slide in at the window edge, reveal zone from the tokens, hide while typing; float shows the pager in a popup below the window).
  2. Bottom bar layout: E1 corner actions (Delete bottom-left, pager center, New note bottom-right) / E3 one pill (Delete, pager, New note inside the pill) / E4 three pills. Pager style: S1 or S3 for Standard, P1 or P3 for Permanent (a compact choice).
  3. Show Delete button (on/off). Show New note button (on/off). Show Skip first / Skip last (Standard only, on/off; draws `|‹` and `›|` ends in the pager).
  4. Created date: Off / Top-left after Saved / Top-left instead of Saved / Bottom row / Next to Delete in the bottom-left corner. Format: "Created today 14:32", "Created Nov 18, 14:32", "Created Nov 18, 2025".
- Under the settings panel: a short "What this removes from today's bar" list (3 lines max) and the recommended combination with one sentence of reason each. No other prose walls.

## Behavior (real rules, keep exactly)
Shared model: stack of notes + 9 slots, viewSlot, current.
- Standard pager: ‹ count › ("n / N"). Count click opens Search in the real app (mock: toast "Search opens"). Arrows: no wrap, "No older Notes" at the start; past the last note a new empty note is appended with "New Note Created", or "Already at Latest" if the last note is empty; leaving an empty non-last note drops it ("Empty note removed").
- Permanent pager: 9 slot dots only (current = wide accent pill, filled = solid muted, empty = outline); a click opens that slot; a click on the current dot returns to the stack. The swap button switches views (Standard to Permanent opens the last-used or first filled slot; back returns to the stack at the previous note). Dot tooltips "Slot i · title".
- Delete (trash icon): a stack note goes to the trash with toast "Moved to trash" and a working Undo that restores it at the same index; a slot has its text cleared with toast "Slot n cleared" and Undo; an empty slot does nothing; deleting the last stack note leaves one blank note; after a delete, current becomes the previous note.
- New note (add icon): always opens the stack, landing on the trailing blank note or appending one. In Permanent view it leaves the slot view; add a toggle under the settings, "New note in Permanent: leaves to stack / hidden", so the user can feel both.
- Skip first/last: jump with no toast; disabled look at the ends.
- Created: each note has `created`. The textarea is live. When the text becomes empty start a 5 s timer (show "soft save in 5s" in a small debug strip under the window, with a "speed up x10" checkbox). When the timer expires with the text still empty, the note counts as emptied; the next characters typed then set `created = now`. Typing again before 5 s keeps the old date. The Created text updates immediately in its slot. Seed dates: one note from today, one from Nov 18, one from last year.
- Toast: the app's real toast style above the bar (`ToastPagerClearance`), 1.8 s, with an Undo button where relevant.
- Keyboard: every control is a real `<button>` with a focus ring, Enter and Space work, tooltips include shortcuts (Alt+Left, Alt+Right).
- Narrow 320: nothing may overflow; if the chosen layout does not fit, show the real fallback you propose (for example the date moves up) with a small caption.

## Done means
Valid HTML; the script parses (`node --check` on the extracted script if node exists); no external requests. Final message (6 lines max): what you matched to the source and anything you could not match or had to guess.
