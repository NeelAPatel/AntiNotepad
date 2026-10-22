# Function contract (M9-function-contract-separation, part a)

What the app does, independent of any shell. A UI or theme task reads this to know which commands a shell must expose and which owner to call. It never reimplements them. Seeded from `docs/product-checklist.md` (2026-10-21), which this replaces as the function list. The tracker stays the live status list.

## Rules
- Every user-facing function has one **command ID**. Shells bind IDs to controls. They never call Core, never save, never manage focus or navigation themselves.
- **Owner** says where the behaviour lives:
  - **Core**: `AntiQuickPad.Core`. No UI types.
  - **ViewModel**: `ViewModels/MainViewModel`. Commands and projections, no Avalonia.
  - **Controller**: the shell-agnostic resident controller. Today that is the `Views/Common/ResidentShellWindow` partials. Part b moves lifecycle, debounce, navigation protocol, focus cue and show/hide out of them. Shells only place controls and bind commands.
  - **Editor**: `Editor/Behaviour/` (input, edits via Core `Text/`, folding updates). `Editor/Presentation/` draws it from `Look`, and `EditorFactory` composes the two.
  - **Carousel**: `Carousel/CarouselMotion` (gesture policy and timing). `CarouselView` draws it, and `CarouselController` composes the two. Gestures become `note.select` / `note.next` / `note.previous`.
  - **Platform**: `Shell/` services (hotkey, tray, single instance, window ops) behind interfaces.
- **Reachable by** lists the routes a user has today. Route kinds:
  - `toolbar:<label>`: a button in `ShellLayoutContext.Buttons`
  - `settings:<label>`: an entry in the settings window (M11-settings-window; it replaced the More menu)
  - `hotkey:<gesture>`: a key handled by the resident host
  - `gesture:<name>`: a pointer or wheel input
  - `tray:<item>`: the tray icon menu
  - `system:<name>`: the OS or command line
- **Conformance** (part d): the ID list below is mirrored once in code. The smoke check asserts the doc table equals the code list. It then asserts each ID resolves to at least one live route in the active shell (Quiet is the active production shell).
- **Required**: `yes` = every shell must expose at least one route. `shared` = provided by the resident host for every shell, so a shell only has to keep the host (keys, gestures, tray, system).

## Commands

| ID | Function | Owner | Reachable by | Required |
| --- | --- | --- | --- | --- |
| `app.toggle` | Show if hidden, hide if shown (the speed promise, < 100 ms show-to-focus) | Controller (+ Platform hotkey) | hotkey:Ctrl+Alt+Shift+A (override `ANTIQUICKPAD_HOTKEY`); tray:Show / Hide | shared |
| `app.show` | Show the pad and land focus in the editor | Controller | tray:icon click; system:second launch | shared |
| `app.hide` | Hide (save first; a failed save keeps the pad visible); closes settings | Controller | toolbar:Hide; hotkey:Esc; system:window Close | yes |
| `app.quit` | Save and exit (a failed save aborts and shows the pad) | Controller | settings:Quit AntiQuickPad; tray:Quit; system:`--quit` | yes |
| `window.minimize` | Minimize the window (closes settings) | Controller (Platform) | settings:Minimize | yes |
| `window.pin` | Toggle always-on-top; persisted | ViewModel (`PinCommand`, `Pinned`) | toolbar:Pin | yes |
| `window.move` | Drag, double-click maximize, snap | Platform (native title-bar hit test) | gesture:title-bar drag region | yes |
| `settings.toggle` | Open or close the settings window (owned by the pad; closes on hide, minimize, quit, hotkey; Esc closes it) | Controller | toolbar:Settings; hotkey:Esc (close) | yes |
| `note.create` | New note after the current one; navigates to it | ViewModel (`CreateCommand`) -> Core | toolbar:New note; settings:New note; hotkey:Ctrl+N | yes |
| `note.delete` | Delete the current note; keeps at least one | ViewModel (`DeleteCommand`) -> Core | settings:Delete note; hotkey:Ctrl+Shift+Delete | yes |
| `note.next` | Go to the next note | ViewModel (`NextCommand`) | toolbar:Next; gesture:wheel notch; gesture:grab-strip drag | yes |
| `note.previous` | Go to the previous note | ViewModel (`PreviousCommand`) | toolbar:Previous; gesture:wheel notch; gesture:grab-strip drag | yes |
| `note.select` | Go to note n | ViewModel (`Select(index)`) | gesture:pager dot (Quiet); gesture:drag snap | shared |
| `note.autosave` | Save text and caret 350 ms after the last edit; save before any navigation, hide or quit | Controller (debounce) -> ViewModel `Save` -> Core | implicit (typing, caret move) | shared |
| `theme.system` | Follow the Windows app theme; persisted | Controller (ThemeService) | settings:System | yes |
| `theme.light` | Light palette; persisted | Controller (ThemeService) | settings:Light | yes |
| `theme.dark` | Dark palette; persisted | Controller (ThemeService) | settings:Dark | yes |
| `settings.material` | Window material: acrylic, mica or solid (parameter string); persisted | Controller -> ViewModel `Preferences` | settings:Material | yes |
| `settings.tint` | Tint strength 0.5-1 over the material (parameter double); persisted | Controller -> ViewModel `Preferences` | settings:Tint | yes |
| `settings.bars` | Bar reveal: hover or always (parameter string); persisted | Controller -> ViewModel `Preferences` | settings:Bars | yes |
| `settings.hotkey` | Change the global hotkey (parameter string); keeps the old one if the new one cannot register; `ANTIQUICKPAD_HOTKEY` overrides; persisted | Controller (+ Platform hotkey) | settings:Hotkey | yes |
| `settings.hotkey-capture` | Suspend (true) or restore (false) the global hotkey while the capture box listens | Controller (+ Platform hotkey) | settings:Hotkey capture | shared |
| `settings.startup` | Start with Windows on or off (parameter bool); mirrors the real per-user Run entry | Controller (+ Platform startup) | settings:Start with Windows | yes |
| `settings.font-size` | Editor font size 11-22 (parameter double); persisted | Controller -> ViewModel `Preferences` | settings:Font size | yes |
| `settings.marker-dim` | Markdown marker opacity 0.2-1 (parameter double); persisted | Controller -> ViewModel `Preferences` | settings:Marker dimness | yes |
| `settings.strike` | Strike thickness 0.05-0.25 em (parameter double); persisted | Controller -> ViewModel `Preferences` | settings:Strike thickness | yes |
| `editor.list-continue` | Enter continues `-`, `*`, `1.`, `- [ ]` lists; an empty item ends the list | Editor -> Core Text | hotkey:Enter | shared |
| `editor.indent` | Tab indents a list item or selection (Word-like on a fresh item) | Editor -> Core Text | hotkey:Tab | shared |
| `editor.outdent` | Shift+Tab outdents | Editor -> Core Text | hotkey:Shift+Tab | shared |
| `editor.checkbox-toggle` | Toggle `- [ ]` / `- [x]` (undoable) | Editor -> Core Text | gesture:checkbox click | shared |
| `editor.fold-toggle` | Fold or unfold a heading section or fence | Editor (fold margin) -> Core MarkdownScanner | gesture:fold chevron click | shared |

## State a shell displays (read-only projections)

A shell may show these. It reads them, never computes them.

| Projection | Source | Shown today as |
| --- | --- | --- |
| Current index and count | ViewModel `CurrentIndex`, `Notes.Count` | footer "n of m"; Card dot window (max 8) |
| Has previous / next | ViewModel `CurrentIndex` vs `Notes.Count` | Card neighbour peeks |
| Save state and error | ViewModel `Status` (`Saving`, `Failed`, `Text`) | footer "All changes saved" / error text in accent |
| Pinned | ViewModel `Pinned` | Pin button highlight |
| Empty note | editor document length = 0 and no carousel motion | "A fresh thought." hint |
| Theme preference, shell ID | Controller (ThemeService preference, shell ID) | settings window selection |

## Guarantees a shell must not break (behaviour contract)
1. **Navigation protocol.** Before a note change, the controller does four things in order: cancel carousel motion, capture editor text and caret, save, remember scroll. A failed save cancels the navigation. After the change it binds the new note, restores its scroll, clears editor undo, saves the selection (except after a delete) and restores focus.
2. **Debounce.** Edits and caret moves update the ViewModel at once and save 350 ms after the last change. Hide, quit, navigation and shell switch save immediately.
3. **Show/hide.** Show activates the window, shows the accent focus cue for 130 ms and focuses the editor. Show-to-focus is measured and must stay < 100 ms. Hide saves first. A failed save keeps the pad visible. Close = hide; only quit exits.
4. **Appearance change** (theme or shell) keeps the same document (including undo), caret and scroll.
5. **Persistence.** Notes are plain `.md` files with atomic writes. Window bounds, pin, shell ID and theme go to `settings.json`. Last note and caret restore on relaunch.

## Ownership (M9 part e rule; AGENTS.md links here)
A task is either a **function task** or a **UI task**, never both. If it needs both, split it into two tasks: function first, then UI.

| Kind | May change | Must not change |
| --- | --- | --- |
| Function task (new or changed behaviour, settings, persistence, commands) | `src/AntiQuickPad.Core/`, `ViewModels/`, `Controller/` (resident lifecycle, debounce, navigation, focus, show/hide, command map), `Contract/FunctionContract.cs` + this doc, `Shell/` services, `Editor/Behaviour/`, `Carousel/CarouselMotion.cs` | `Views/Quiet/`, `Views/Card/`, any future shell layout folder, `ShellPalette`, `Look` tokens, design assets |
| UI task (look, layout, theme, motion presentation) | Shell layout folders, `ShellLayoutContext` placement helpers, `Tokens/` (design tokens, docs/design/tokens.md), `ShellPalette`, `Look` tokens, settings window and toolbar placement in `Views/Common`, `Editor/Presentation/`, `Carousel/CarouselView.cs` | Core, ViewModels, `Controller/`, `Contract/`, `Editor/Behaviour/`, `Carousel/CarouselMotion.cs` |

- A UI task binds controls to command IDs from the table above, through the controller's command map. It never calls Core, saves, focuses or navigates itself.
- A new user-facing function adds its row to this table and to `Contract/FunctionContract.cs` in the same task. The conformance check (in `--selftest`) then fails until every shell exposes it.
- Editor and Carousel are split (part c). A UI task may change `Editor/Presentation/` (renderers, chevron drawing, fonts, colours) and `Carousel/CarouselView.cs` (visual tree, transforms, grab strip), plus `Look` tokens. It never changes `Editor/Behaviour/` (keys, checkbox toggle, folding updates, edits) or `Carousel/CarouselMotion.cs` (wheel gate, drag, snap, slide timing). The joins `IEditorPresentation` and `ICarouselSurface` stay as they are.

## Building a new shell (UI overhaul guide)
The UI overhaul is a UI task. It replaces layouts, palettes, tokens, settings window look and the editor and carousel drawing. It never changes behaviour.

1. **Place controls** in a new `Views/<Name>/<Name>Layout.cs` built from `ShellLayoutContext`, the way `QuietLayout` and `CardLayout` do.
2. **Bind every action by command ID**: `c.Button(glyph, label, "note.create")`; settings window entries register their label and command ID the same way. The label is only the tooltip, accessible name and route name. Every ID must be in the Commands table above; an unknown ID fails when the layout is built.
3. **Show state from ViewModel projections only**:
   - `PositionText` ("n of m")
   - `SaveLineText` + `SaveLineFailed`
   - `IsCurrentEmpty` (combined with carousel motion for the empty hint)
   - `HasPrevious` / `HasNext`
   - `PagerFirstIndex` + `PagerDots` (`Index`, `IsCurrent`; bind a dot to `note.select` with the index as parameter)
   - `Pinned`
   - theme preference and `Preferences` (material, tint, bars, hotkey, startup, font size, marker opacity, strike) for the settings window
4. **Restyle** through `Look` / `ShellPalette` tokens, `Editor/Presentation/` and `Carousel/CarouselView.cs`.
5. **Register** the shell ID in `App` (a new shell also needs a `shell.<name>` command row + `FunctionContract` entry, which is a function change: add it in its own small task first; with one shell there is no shell command).
6. **Run `--selftest`**. Three checks guard the split:
   - `CONFORMANCE <shell> PASS`: every required command has a live route in the shell.
   - `BOUNDARY PASS`: layouts and presentation do not reach Core, and the controller and ViewModels do not reach views, palettes, `Look` or brushes.
   - The pixel proofs.

   Then run `--measure`; the median must be 30 ms or less.

Do not touch to restyle: `Controller/`, `ViewModels/`, `Contract/`, `Core`, `Editor/Behaviour/`, `Carousel/CarouselMotion.cs`. If a design needs new behaviour (a new gesture, setting or command), stop and split it into a function task first.

## Not built yet (function phase backlog, from the checklist)
- M7-settings: hotkey configurable, start with Windows, data folder, font size, marker dimness (F9), strike thickness (F17). These will get command IDs `settings.*` when built.
- F44-delete-placement-and-undo: undo for delete (`note.undo-delete`).
- M10-search-across-notes: not in scope.
