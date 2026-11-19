# A29-shortcut-map: key inventory (Findings evidence)

Read-only scout by the item chat, 2026-11-19, dev tip ece5337. Handlers grepped: every `KeyDown`, `Key.`, `KeyModifiers`, `HotKey` in `product/AntiNotepad`.

## Handlers (where keys are decided)
| Layer | File | What it handles |
|---|---|---|
| OS global | `Platform/GlobalHotkey.cs`, `HotkeyRegistrar.cs` | Show/hide (default Ctrl+Alt+Shift+S) and optional "new note" hotkey; both are user settings |
| Window | `UI/PadWindow.Keys.cs` `OnWindowKeyDown` | all app commands below; dev-only F11 (memory probe) and F12 (screenshot, needs env var) |
| Editor (tunnel) | `Editor/PadView.cs` `OnKeyDown` | Ctrl+V paste, Ctrl+Shift+[ ] fold, Ctrl+Enter, Enter list continue, Ctrl+[ ] list level, Tab / Shift+Tab |
| Editor scroll | `Editor/ScrollableTopInset.cs` | Ctrl+Home (+Shift) |
| Editor pointer | `Editor/Elements.cs`, `PadView.cs` | Ctrl+click opens a link |
| Slash popup | `Editor/Completion.cs` | Esc closes; Enter accepts a keyword |
| Search overlay | `UI/SearchOverlay.cs:326` | Up/Down/Left/Right, Enter, Ctrl+G grid, Ctrl+T trash, Ctrl+Space select, Ctrl+Delete, Ctrl+Backspace (empty query) |
| Save-failure overlay | `UI/PadWindow.Layout.cs:781` | Enter retry, Tab swap, swallows the rest |
| Popup menu | `UI/PadMenu.cs:46`; Settings `SettingsWindow.cs:84` | Esc closes |
| Settings recorder | `UI/HotkeyRecorder.cs` | records the global hotkeys (needs Ctrl/Alt/Shift) |
| AvaloniaEdit defaults | library | Ctrl+Z/Y undo, Ctrl+A/C/X, Ctrl+arrows, Home/End, Ctrl+Backspace/Delete word |

## Window-level command map today
| Key | Action | In help | In app menu |
|---|---|---|---|
| Esc | close overlay, else hide | yes | "Escape" |
| F1, Ctrl+? | help | F1 not listed | no |
| Ctrl+N | new note | yes | yes |
| Alt+Left / Right | previous / next note | yes | no |
| Ctrl+PageUp / PageDown | previous / next note | **no** | no |
| Alt+Shift+Up / Down | move note earlier / later | yes | no |
| Ctrl+F, Ctrl+P, Ctrl+K | search | only Ctrl+F | Ctrl+F |
| Ctrl+, | settings | **no** | "Ctrl+OemComma" |
| Ctrl+1..9 / Ctrl+Shift+1..9 | open slot / keep in slot | yes | no |
| Ctrl+0, Ctrl+~ | back to the stack | Ctrl+0 only | no |
| Ctrl+\ | split | yes | "Ctrl+OemBackslash" |
| Ctrl+T | pin (keep on top) | yes | yes |
| Ctrl+Shift+Backspace | delete note | yes | no |
| Ctrl+Shift+C | copy whole note | yes | no |
| Ctrl+Shift+O | OCR snip | yes | no |
| Ctrl+Plus / Minus | text size | yes | no |
| Ctrl+Shift+Q | quit | **no** | no |
| Ctrl+Z | undo the last toast action (delete, reset), else editor undo | no | no |

## Seeds, checked against code
1. **Alt+Shift+arrows:** code has only Alt+Shift+Up/Down = move the note earlier/later. There is no first/latest-note jump, so the seed is either "add a jump key" or a wording slip. CORRECTED by Codex audit: Alt+Shift+Left/Right also navigate notes today and Alt+Shift+arrows shadow rectangular selection.
2. **Duplicate Ctrl+T:** help lists Ctrl+T under Notes (as trash, inside search only) and under Writing (as pin). Different layers, same text in one sheet; also the Notes row crams three keys in one line.
3. **No move-note key in menus/rename:** Alt+Shift+Up/Down exists but is only in the sheet; the app menu has no entry; "move" has no word in the help beside the arrows.

## Drift found beyond the seeds
- The app menu prints raw enum names: `Ctrl+OemBackslash`, `Ctrl+OemComma`, `Escape`. Tooltips in Layout.cs:271 and SearchOverlay.cs:79,80 hand-type keys a third time.
- Keys with no help row: Ctrl+PageUp/Down, Ctrl+,, Ctrl+Shift+Q, Ctrl+Shift+[ ] (fold), Ctrl+P/K aliases, Ctrl+~, Ctrl+Z, Ctrl+Home, Ctrl+click link, Ctrl+Space and Ctrl+Delete in search, F1.
- Help sheet and handlers are separate hand-written lists in four places (`HelpOverlay.Sheet`, `PadWindow.Keys.cs`, `PadWindow.Layout.cs` menu and tooltips, `SearchOverlay.cs`); nothing checks they agree.
- The only key selftest is `SelfTest/UI.cs:84` (Ctrl+T pin) plus the hotkey recorder checks.

## Keys other apps own (reuse is fine if the habit transfers)
- Matches habit: Ctrl+N, Ctrl+F, Ctrl+PageUp/Down (browser tabs), Alt+Left/Right (back/forward), Ctrl+Plus/Minus, Ctrl+, (settings in VS Code and browsers), Ctrl+Enter, Ctrl+Shift+[ ] (VS Code fold).
- Different meaning elsewhere: Ctrl+T (new tab), Ctrl+P (print), Ctrl+K (insert link in Word), Ctrl+G (Notepad go to line, Word go to), Ctrl+0 (reset zoom; here back to the stack, zoom reset is a likely expectation), Ctrl+Shift+Q (quit in browsers, close all in Word), Ctrl+Shift+C (copy in terminals, dev tools in browsers), Ctrl+\ (VS Code split, matches).
- Windows: Ctrl+Shift+digit and Ctrl+Alt+digit are not system shortcuts; Alt+Shift alone switches input language only when pressed and released with no other key, so Alt+Shift+arrow is safe.

## Sibling items (key overlap)
- A31-list-default-type adds keywords only (`checklist`, `bullet`, `bulletlist`, `numberlist`); no keys. It touches Completion Enter handling, which A29 does not change.
- A32-selection-convert-menu adds context-menu entries; it names no shortcut. If it later wants keys, they would come from this map (nothing is claimed now).
- A29 must not move `Ctrl+[ ]`, Tab, Enter (A26) and should not touch menu code A32 edits except `AppMenu` key labels. Expect a small merge in `PadView.RequestMenu` area only if A29 changes it (planned: no).
