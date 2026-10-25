# Decisions (locked unless the user reopens them)

Newest first. History before 2026-10-23 (overnight build log and write-up) is in git history and the tags `v0.1`,
`antipad-v*` and `baseline/m8c-v1-compact`.

## 2026-10-24
- **Taskbar icon** (A20-taskbar-icon-setting, A22-taskbar-button-fixes): Defaults to Never; Settings offers While visible, Always and Never. One stable minimized stand-in supplies the pad's button in both visible and hidden states (Always keeps it while hidden); the real pad stays off the taskbar to avoid Avalonia's owner-change hide/re-show. Clicking the button hides the focused pad or shows/focuses it otherwise. Settings keeps its own button; the tray stays available in every mode.

## 2026-10-23
- **Notes location and recovery** (A7-notes-folder-and-recovery, decision 1): app home remains `%LOCALAPPDATA%\AntiNotepad`
  for the log and `location.json`; notes live in the user-visible folder `Documents\AntiNotepad` by default, using
  `SpecialFolder.MyDocuments` so OneDrive redirection is followed. Settings can move notes to another folder. Moves copy and
  verify the index, backup index, note bodies and recovered files before updating the pointer, then retain the previous copy.
  Unknown Markdown bodies are moved to `recovered\`; uninstall keeps the notes folder even when app home is removed.
- **Branches** (C2-stale-branch-cleanup): only `main` and `dev` are kept long-term; old spike, lab and feature branches are
  deleted. The Icon Lab (`--iconlab`) is gone with its branch and gets remade if needed; the N3 release plan is kept as tag
  `proposal/N3-release-plan`.
- **Archive removed** (C1-remove-archive): the `archive/` folder (old AntiQuickPad project, Antipad spike notes) is deleted
  from the tree; reach it through git history or the tags above.
- **Project**: AntiNotepad replaces AntiQuickPad. The old app, spikes A/B, demos, UI-audit kit and old docs moved to
  `archive/antiquickpad/` (since removed, see above).
- **Icons**: Segoe Fluent Icons, 16 px, palette Ink, drawn once (no faux-bold), snapped to whole pixels. Toolbar and pager
  buttons 32 px square with a 4 px rounded hover. The newer Fluent UI System Icons font stays bundled for Settings icons.
- **Right-click menu**: AntiNotepad's own popup in Windows 11 style (designed in the Icon Lab): 8 px rounded panel, 4 px insets,
  32 px rows, icon column, dimmed shortcuts, content width. Order: Undo, Redo, Select all | New note, Search notes, Split view,
  Keep on top | Settings, Hide AntiNotepad; Cut, Copy, Paste, Delete as an action row on top. Settings: Menu action row (on),
  Colored menu icons (off). The popup window has no system rounding, border or shadow (DWM attributes).
- **Codex does the implementation**; the Claude manager briefs, gates, commits and relays.

## 2026-10-22
- **Bar styles**: Docked (default), Overlay (bars slide in within 20 px of the top/bottom edge), Float below (pager in its own
  popup under the window, 8 px trigger). With overlay/float the first line gets a 30 px gap that scrolls away.
- **Edge fade** on by default (40 px). Text badge and "Saved <time>" label fade with the toolbar icons.
- **Markdown**: `__x__` is underline (`**` bold, `++` also underline); headings h1-h6 in a rainbow; `---`/`***`/`___` draws a
  rule; numbered lists renumber per level on Return/Tab; a heading's fold also ends at two blank lines.
- **Memory**: Settings' live trace runs only while something moves; closing Settings compacts the heap and trims the working set.
- **Scrolling** (user's mouse: MX Master 3S, whole clicks only): vertical modes Lines (eased glide), Free, Rate-matched (default),
  Wheel-driven; note paging by Detent or Progress swipe (default; 220 px of input = one note, fires at 0.35, re-arms after 110 ms).
