# Antipad: Claude's product spike (branch `claude/product-spike`)

**Antipad** is AntiNote, rebuilt for Windows on Avalonia + AvaloniaEdit. I took full creative freedom here: it's a new app in `product/Antipad`, not a restyle of AntiQuickPad. This branch is never merged.

> **Stop the auto-resume watchdog when you're back:** create an empty file `spike-notes/STOP`.
> A hidden `pwsh` watchdog (`spike-notes/watchdog.ps1`, log `spike-notes/watchdog.log`) resumes this Claude session headless whenever the transcript has been idle for 25 minutes. It ends on its own at about 19:20 on 2026-10-22. If you sit in this session and leave it idle, it can start a run alongside you.

## Try it (2 minutes)

```bash
"F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/build/antipad/Antipad.exe"
```

1. **Ctrl+Alt+Shift+S** shows and hides it from anywhere. **Esc** also hides it.
2. You land on the welcome note. **Swipe sideways** on the touchpad (or Ctrl+[ / Ctrl+]) to reach the list note and the math note.
3. Go past the last note to get a blank page. Type `math`, press Enter, then `lunch 12 + 3`.
4. **Ctrl+F** searches; **Ctrl+G** switches to the grid. **Ctrl+,** opens settings. **F1** lists every shortcut.

It runs side by side with AntiQuickPad. It has its own instance ID, its own data folder (`%LOCALAPPDATA%\Antipad`), its own hotkey and its own Run-key name.

## v2 (2026-10-22, after your feedback). The v1 build is tagged `antipad-v1`

| You asked | What changed |
|---|---|
| Memory with many and long notes | Each note is now its own file (`index.json` plus `notes/<id>.md`); the v1 `antipad.json` migrates on first launch and is kept as `.v1-backup`. Text loads only when a note is opened, and only changed notes are saved. Editor documents stay cached for the 6 most recent notes. 45 s after hiding, everything else is released, the heap is compacted and the working set is trimmed. Very long notes (over 1,500 lines) are analysed after a typing pause instead of on every keystroke. |
| | **Measured** (Release build, 71 notes plus a 6,000-line note): hidden working set **152 → 22 MB**; shown **168 → 89 MB**. The rest is the framework floor: the managed heap is about 5 MB. Picking **Solid** switches to software rendering after a restart, about **40 MB** private instead of about 125 MB. Glass needs the GPU. |
| Fenced code, indent guides, coloured headers, folding | ```` ```python ```` blocks get real syntax colours on a tinted panel. Indent guides are faint, with the caret's level drawn a little stronger. H1, H2 and H3 each have their own colour and size. Folding covers headings, fences, nested lists and code, with quiet chevrons that appear on hover (Ctrl+Shift+[ / ] at the caret). |
| Scrolling as a detent | Horizontal input now drags the page with rubber-band resistance. When input stops, it commits only past a distance or speed threshold; otherwise it springs back. One gentle notch springs back; a flick moves on. |
| Answer bubble beside "=" | Answers sit right after the line, so a narrow window still works. |
| Toolbar fade symmetry | It fades in and out over the same 180 ms. |
| Toasts for implicit actions | "New note" when swiping past the end; "Empty note removed" when leaving a blank note. |
| Thin icons | Stroke icons with a 1.7 px pen, a weight heavier than Segoe Fluent Icons. |
| Filled pin when pinned | Filled in the accent colour. |
| No close buttons | Search, Settings and Shortcuts each have an ✕ at the top right. |
| Square grey hover in Settings | Fluent's template fill is turned off; hover uses the same rounded pill as the selected option. |
| UI scale | Settings › UI scale: 90–150 %, scaling chrome, panels and text together. |
| Smooth scrolling and caret | The wheel and touchpad glide; the caret glides between positions and blinks only when idle. |

Also: Solid's hint in Settings says when a restart applies the renderer change, and dev builds run beside your copy (`build/dev`, instance "Antipad-dev", Ctrl+Alt+Shift+D).

## What I built

| Area | What you get |
|---|---|
| **Window** | One rounded, chrome-free Acrylic window. The top icons and the bottom pill appear when the mouse moves and fade while you type. No taskbar button; there's a tray icon. Opens where you left it, centred, under the mouse, above the tray, or **drops down from the top edge** like a console (setting). |
| **Speed** | Hotkey to focused editor: **23–30 ms** in the default position (many presses, logged). First show after `--hidden` start: **87 ms**, thanks to an invisible pre-warm render. In drop-down mode keyboard focus lands in **13 ms**, while the slide itself takes 150 ms. Windows' high-contrast mode switches to the contrast palettes automatically. |
| **Note stack** | Swipe, Ctrl+[ / ], or Alt+← / →. Going past the newest note creates one. Empty notes you leave remove themselves. Each note keeps its own caret, scroll and undo history. Alt+Shift+↑ / ↓ reorders notes. |
| **After a break** | After 30 minutes away (setting), the hotkey opens a fresh page; your last note is one swipe back. |
| **9 slots** | Ctrl+1…9 open permanent notes. Ctrl+Shift+N moves the current note into slot N. Dots on the floating pill show which slots are filled. |
| **Split view** | Ctrl+\ puts a slot beside your note. The window widens, and the right pane can use its own palette. |
| **Search** | Ctrl+F filters as you type (all words must match), in a list or card grid. Arrow keys and Enter work. Ctrl+click or Ctrl+Space selects several cards; Ctrl+Delete trashes them. Matches are highlighted. Drag a card to reorder the stack. |
| **Trash** | Deleting a note sends it to the trash (Ctrl+Shift+Backspace). **Ctrl+Z** or the toast's Undo brings it back, in the pad or in search, until you type. Restore from the trash view in search. Items expire after 30 days. |
| **Writing** | Markdown colouring: headings, **bold**, *italic*, ++underline++, ~~strike~~, `code`, fences, `//` comments, `>` quotes, #tags. **Clean markdown** hides the markers except on the line you're editing. Bullets render as dots. Return continues a list; Return on an empty item ends it. Tab and Shift+Tab indent. |
| **Checklists** | `[ ]` renders as a drawn checkbox; click it or press Ctrl+Enter to toggle. Done items dim and get struck through. |
| **Links** | Long URLs display as `host/…/tail` and expand on the caret line. The full URL stays in the text and on the clipboard. Ctrl+click opens a link. |
| **Inline answers** | In any note, a line ending in `=` shows its answer, e.g. `spent 12.50 + 8.25 =` shows 20.75. Click an answer to copy it. |
| **Line-1 modes** | Typing a keyword on line 1 offers completion: **math**, **list**/**todo**, **timer**, **code py** (or js, cs, json, sql…), **clipboard**. |
| **math** | Variables, reactive updates, `sum`/`total`/`avg`, `prev`, %, units (`5 km in mi`, `72 F in C`, `2.5 GB in MB`), functions, currency, labels (`lunch 12 + 3`), and **dates**: `days until dec 25`, `today + 30 days`, `launch = dec 25` then `launch - 2 weeks`, `weeks until 2027-01-01`. Answers are right-aligned in their own colour. |
| **timer** | One timer per line (`25m focus`, `1h 30m`, `05:00 eggs`); Return starts it. Click a chip to pause or resume. When a timer finishes you hear a chime and see a toast; the pad appears without stealing focus. |
| **clipboard** | While the note exists, everything you copy is appended, even with the pad hidden. Password-manager copies (clipboard history opt-out formats) are skipped. |
| **/commands** | `/date /time /now /week /todo /uuid /sort /dedupe /reverse /checklist /bullets /numbered /plain /uncheck /tidy /upper /lower /title /json /count /clear /ocr /lorem`. Whole-note commands are a single undo step and never touch a line-1 mode keyword. |
| **OCR** | `/ocr` or Ctrl+Shift+O reads the copied image, or opens Windows' snipping overlay and reads the snip. It uses Windows' built-in OCR, so nothing leaves the PC. |
| **Look** | 8 palettes (Paper, Snow, Sage, Ink, Midnight, Graphite, and high-contrast light and dark), chosen separately for light and dark. The theme maker sets an accent and a paper tone, with a live preview. Acrylic, Mica or Solid; glass tint; paper (blank, lined, dotted, grid, large grid at 3 strengths); font; size; reduced motion. |
| **Tray** | Click to show. Menu: show, new note, the nine slots by title, settings, quit. |
| **Reliability** | Each save writes a temp file and atomically replaces `antipad.json`, keeping a `.bak` copy. A broken file falls back to the `.bak` (covered by the self-test). |

Screenshots (from the Release build) are in `spike-notes/shots/`: welcome, math, list, a mixed note, search grid, search highlight, settings, theme maker, shortcuts, and split with a slot. They come from in-app captures, so the Acrylic backdrop and the bottom pill and toast contents do not appear.

## Against the spec's acceptance criteria

| # | Criterion (ChatGPT AntiNote spec) | Antipad |
|---|---|---|
| 1 | Capture a note without clicking a single control | Hotkey, type, Esc. Focus lands in about 25 ms. |
| 2 | Navigate and create notes without opening a menu | Swipe, Ctrl+[ / ], or Ctrl+N; going past the last note starts a new one. |
| 3 | Enter and dismiss without losing focus or text | Autosaves within 400 ms, and saves on every hide and quit. Windows hands focus back to the previous app. |
| 4 | Markdown and syntax colours without cursor misalignment | Colouring only restyles text. Hidden markers and checkboxes are single caret stops that never change line height. |
| 5 | Resize, change themes and use translucency without breaking readability | The glass tint keeps text contrast. Paper is drawn in text space, so it stays aligned while scrolling. A high-contrast fallback is included. |
| 6 | Search and restore notes without permanent organisational clutter | Search and trash are temporary panels inside the pad; there's no sidebar. |
| 7 | Minimal resource use while hidden | Measured hidden: **0.2 % of one core** over 30 s, 156 MB working set (Debug build). Nothing ticks unless a timer is running. |

## Design choices

- **Built from scratch.** AntiQuickPad's contracts, smoke tests and shell variants exist for a different process. A small codebase with one window is easier to judge as a product. Hotkey, single-instance and tray code are adapted from AntiQuickPad and renamed so the two apps can't collide.
- **Settings, search and help open inside the pad.** No second window, so the pad never loses its place.
- **Chrome that gets out of the way.** Controls exist only while the mouse moves. Typing hides them, and moving the mouse brings them back.
- **Semantic colours.** Keywords, variables, numbers, results, links, comments and done items each have their own role in every palette, as the spec asks.
- **Windows, not macOS.** Native rounded corners, Acrylic or Mica, Windows shortcuts and the snipping overlay; no traffic lights.

## Checked live (computer use plus in-app captures)

- Data paths, checked against `antipad.json`:
  - delete then undo
  - promote into an empty slot and into a filled slot (the notes swap)
  - clear a slot
  - batch delete in search then undo
  - restore from the trash view
  - quit and relaunch, with everything persisted
- Looks: Mica, Solid, dotted and bold-grid paper, high-contrast dark, and a split pane in its own palette.

- Hotkey hides and shows the pad, including after `--hidden` start.
- Swipe moves one page per gesture, and an abandoned blank note drops itself.
- Math, list, code and timer notes all work (an 8-second timer counted down to done).
- Clipboard capture works while the pad is hidden.
- OCR read a test image exactly.
- Search list, grid and drag-reorder work.
- Settings and the light theme work.
- Lined paper, split view and slots work.
- Clean markdown works.
- Esc with a completion popup open closes only the popup.
- `Antipad.exe --selftest` passes: 168 calculator checks (including 43 for dates) plus store, modes, timers and links.

## Not done or not verified

- **Timer focus:** I couldn't fully check that a finished timer leaves focus with another app, because computer use could only drive Antipad. The code uses a non-activating show.
- **Untested hardware:** multi-monitor and high-DPI mixing, a real touchpad (only synthetic horizontal wheel events were tested), IME input and screen readers.
- **Not built:** sync, JS extensions and import/export (the last two you ruled out).
- **Tray slot menu:** not clicked through, because computer use can't reach the taskbar.


## Files

- `product/Antipad/`: the app. Main pieces:
  - `UI/PadWindow*.cs`: window, layout, navigation, keys
  - `Editor/`: pane, colourizers, inline elements, completion
  - `Calc/Calculator.cs`: math engine
  - `Model/`: store, modes, timers, welcome notes
  - `Platform/`: hotkey, single instance, tray, OCR, Win32
  - `Theme/Palette.cs`
- `spike-notes/PROGRESS.md`: running log.
- `spike-notes/dev.sh`: rebuild and launch against throwaway test data. F12 captures; Ctrl+F12 saves a numbered gallery shot.
- `spike-notes/press-hotkey.ps1`: dev aid that presses the hotkey from a script.
