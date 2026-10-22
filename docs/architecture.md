# AntiQuickPad architecture

T-013 adds production Quiet and Card shells on the T-012 Core/ViewModel boundaries. Both product projects target net10.0; Core has no package references or Avalonia types. The small Core.Smoke console is a disposable capability check.

```text
App / Program (composition and CLI entry points)
    constructs Core NotesSession + SettingsStore -> MainViewModel
    constructs platform services -> chosen IShellView

Views/Quiet, Views/Card -> Views/Common resident host -> Controller/ResidentController
Views/Classic, Design (IEditorHost), Editor, Carousel
                         |
                         v
ViewModels: MainViewModel -> NoteViewModel / StatusViewModel
                         |
                         v
AntiQuickPad.Core: NotesSession, Note, SettingsStore, Text

Resident shell -> Shell interfaces -> Avalonia / Windows implementations
Measurement / smoke harnesses exercise these boundaries independently.
```

Dependency rule: **Views -> Controller -> ViewModels -> Core; nothing points up.** The controller holds no layout, palette or brush types; it reaches the window through `IResidentSurface`. Function vs UI ownership by folder: `docs/function-contract.md`. Core never references UI, dispatcher, window, editor, or platform APIs. ViewModels use plain INPC and ICommand helpers and never reference Avalonia. Core DTOs are used by the editor adapter for text edit plans; the adapter calls ViewModel methods, which call Core. App alone constructs dependencies by hand. There is no service locator or dependency injection package.

| Feature | Owner |
| --- | --- |
| Ordered Markdown notes, IDs, current selection, caret, keep-one-note deletion | Core/NotesSession and Note |
| Atomic Markdown/manifest writes; explicit Save/Flush and save-state events | Core/NotesSession |
| Pin, normal window bounds, shell/look identifiers, atomic settings replacement | Core/SettingsStore and WindowSettings |
| Markdown/Python classification, heading/fence fold spans | Core/Text/MarkdownScanner |
| Checkbox toggle, list continuation/exit, Tab/outdent and ordered-list numbering | Core/Text/TextOperations returns granular edit plans |
| Notes projection, commands, current index/count, status/error, pin state, settings adapter | ViewModels/MainViewModel, NoteViewModel, StatusViewModel |
| Existing toolbar/status layout, editor hosting, focus cue, Close=hide, lifecycle and 350 ms debounce | Views/Classic/ClassicShellWindow |
| Quiet/Card production layouts, unified toolbar and floating pager | Views/Quiet/QuietLayout and Views/Card/CardLayout |
| Resident lifecycle, 350 ms autosave debounce, navigation protocol and scroll memory, show/hide/quit, focus and 130 ms focus cue, hotkey/tray install, key map, contract command map (ID -> ICommand) | Controller/ResidentController partials (M9 part b); talks to the window only through Controller/IResidentSurface |
| Quiet/Card window host: layout and menu building, appearance rebuild, palettes, focus-cue brush, IResidentSurface implementation | Views/Common/ResidentShellWindow partials |
| Function contract mirror and conformance check | Contract/FunctionContract (doc: docs/function-contract.md); route resolver ResidentShellWindow.Contract via ResidentController.Contract |
| System/Light/Dark preference and live Windows app-theme notifications | Views/Common/ThemeService (Avalonia IPlatformSettings.ColorValuesChanged); ShellPalette owns the four designed palettes |
| Editor input and edits (Enter/Tab, checkbox toggle, hand-cursor hit policy, fold updates, caret line state) | Editor/Behaviour (M9 part c) |
| Editor drawing: colors/fonts, colorizer, code panel, strike, checkbox, indent guides, active line, chevron margin | Editor/Presentation, configured by Look; joined to behaviour by IEditorPresentation; EditorFactory composes both |
| Editor creation, undo groups, scroll offsets | Editor, configured by Look (ShellPalette supplies production looks; DesignSpec supplies the original light design tokens) |
| Wheel burst gate, drag state/snap, target selection, slide easing and frame samples (no controls) | Carousel/CarouselMotion; selection requests go to MainViewModel |
| Carousel viewport, transforms, preview editor, grab strip, page decorator/stride | Carousel/CarouselView via ICarouselSurface; CarouselController composes both |
| Global hotkey, tray, singleton signals and platform window operations | Shell interfaces and their concrete implementations |
| Isolated render-design/probe-backdrop fixtures | Design; shares IEditorHost and EditorFactory without opening resident notes |
| --measure, --selftest and isolated Core smoke | Measurement, ResidentShellWindow.Smoke plus retained ClassicShellWindow.Smoke, Core.Smoke |

Navigation has explicit before/after events: the shell cancels animation, captures editor text/caret, saves and remembers scroll before the VM changes selection. A failed save cancels navigation. After a change the shell binds the selected note, restores scroll, clears the editor undo stack, saves the new selection and restores focus. Scroll vectors, editor documents, animation and focus never enter Core. Core events are synchronous on the caller thread; the UI invokes operations on its dispatcher thread. Save failures remain visible until a successful save.

IShellView exposes the resident Window host, editor, MainViewModel and show/hide/quit hooks. Its smaller IEditorHost contract is also implemented by the isolated design window; the fixture host deliberately has no resident ViewModel or services. Classic's smoke checks are kept in a separate partial file. Platform factories are constructor-injected through ShellServices. ISingleInstanceService is owned by Program; the command-only --quit path opens the existing channel and returns before Avalonia startup.

Existing notes.json, .md files and settings.json remain readable. Core and ViewModels are unchanged in T-013. Their existing ShellId/LookId settings carry `quiet|card` and `system|light|dark`; the settings store contains identifiers, never renderer objects. App defaults fresh installs and legacy `classic/Ink` to Quiet/System. Explicit `--shell classic` and unknown shell IDs select the Classic fallback. Other loaded shell choices survive bounds saves. `--shell quiet|card|classic` overrides startup selection, including measurement children.

Quiet and Card share one resident Window, services, VM and navigation contract. A runtime appearance change rebuilds the visual editor/carousel with the same TextDocument (including undo), caret and scroll; EditorFactory.Release removes the old scanner/folding/render subscriptions before transferring the document. Ordinary show/hide keeps the same editor instance. The menu calls the existing VM create/delete/navigation/pin commands; UI-only shell/theme/quit actions live in the resident host. Theme preferences persist in LookId, avoiding schema and backend changes. System follows Avalonia's Windows platform color notifications; the smoke injects a simulated source update through the same theme application path.

The shells extend the client area with BorderOnly decorations and native TitleBar hit-test roles on empty toolbar regions. Windows owns drag/double-click maximize and edge snapping. Caption actions are client controls; Close hides. Win+Z/snap-layout UI and physical multi-monitor behavior still require desktop verification. Minimum Quiet/Card size is 600x480 DIPs to preserve the fixed toolbar/footer targets.

Card's optional CarouselController page decorator wraps both real editors with the rounded card surface. The same wheel gate, direct drag and 220 ms completion/VM selection path move those full surfaces; Quiet moves flat editor pages. Card retains a transparent top-edge grab region, conditional adjacent surfaces, and an interactive sliding window of at most eight dots plus the exact total count. Neighbour peeks are edge surfaces, not additional editable documents.

## How to add a new shell

1. Add a layout under Views/<name> consuming ShellLayoutContext. Quiet/Card share ResidentShellWindow's IShellView implementation; a substantially different lifecycle may implement IShellView separately.
2. Bind controls through the context's VM command map and status projections. Keep all note/store access behind MainViewModel.
3. Supply UI-only palette tokens, optional carousel page decoration and empty-state geometry. Reuse resident navigation, focus, debounce, platform services and save-failure handling.
4. Register the ID in App and the host's runtime layout switch/menu; persist through the existing settings adapter. Add neither UI types to Core nor Avalonia dependencies to VMs.
5. Run the offline Release build and --selftest; render the real resident shell at both scales, then run --measure --shell <name> and physical hotkey/tray/editor/carousel/DPI checks on the desktop.

## Local verification

From src/AntiQuickPad: run `dotnet bin/Release/net10.0/AntiQuickPad.dll --quit`, then `$env:AVALONIA_TELEMETRY_OPTOUT='1'; dotnet build -c Release --no-restore -p:UseSharedCompilation=false` and `dotnet bin/Release/net10.0/AntiQuickPad.dll --selftest`.

From the repository root: build `src/AntiQuickPad.Core.Smoke` with the same Release/no-restore flags and run `dotnet src/AntiQuickPad.Core.Smoke/bin/Release/net10.0/AntiQuickPad.Core.Smoke.dll`. Its only project reference is Core; it checks both assembly references and actually loaded assemblies as well as persistence and legacy/settings round trips. New project assets can be generated with `dotnet restore <project> --source src` (local directory plus installed package cache; no network feed).

Scripted focus checks establish internal show-to-focus only. Physical hotkey latency, presentation, normal-desktop resource use and subjective rendering remain desktop measurements.

`--render-shell <quiet|card> <light|dark> <outdir>` constructs the real resident VM/window with isolated data and skips tray/hotkey ownership. It invokes actual commands, binds actual notes, opens the production in-tree menu, and freezes a real carousel drag between two editor/card trees. Five states at 1x/1.5x capture the client tree; DWM/window borders, OS snap flyouts and physical monitor DPI changes are outside this proof. `--render-design` still retains T-011's separate review host.
