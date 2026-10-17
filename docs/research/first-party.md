# Microsoft stack assessment

Checked 2026-10-17. Documentation/source review only; no Windows runtime or latency measurements. Capability gaps below are engineering inferences from the cited control APIs, not proof that custom implementations are impossible.

## Window and editor fit

| Option | What it supplies | What AntiQuickPad must add |
|---|---|---|
| WinUI 3 / Windows App SDK | `Window.SystemBackdrop` with `MicaBackdrop` or `DesktopAcrylicBackdrop`; `OverlappedPresenter.SetBorderAndTitleBar(false, false)` for a chrome-free window. | Drag/resize/accessibility affordances; a suitable editor. Extending content into the title bar alone retains caption buttons. Mica is wallpaper-tinted, not live desktop transparency; Acrylic blurs desktop content. Arbitrary per-pixel transparency is a separate requirement to probe. |
| WinUI `RichEditBox` | RTF/rich text, formatting, links/images and text-object APIs. | Source-preserving Markdown parsing/styles, task-checkbox rendering/hit testing and text edits, fenced-language highlighting and edit/undo synchronization. These are not documented built-ins. |
| WPF | `WindowStyle=None`, `AllowsTransparency=True`, transparent background; `RichTextBox` / `FlowDocument` supports rich content and embedded UI. | Markdown source-to-document mapping, fenced lexing, widget synchronization and undo/caret behavior. Modern backdrop integration needs Windows interop; a transparent layered window is not evidence that Acrylic/Mica works in that configuration. |
| WinUIEdit | Independent BreeceW Scintilla/Lexilla port, `HighlightingLanguage`, Scintilla API and editor infrastructure. **Not Microsoft first-party.** | A proven source-preserving Markdown/widget composition; a language property alone does not prove mixed fenced-language editing. Maintainer explicitly warns it is not production ready. |

Sources, checked 2026-10-17: Microsoft [materials](https://learn.microsoft.com/en-us/windows/apps/develop/ui/materials), [border/titlebar API](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.windowing.overlappedpresenter.setborderandtitlebar), [title bars](https://learn.microsoft.com/en-us/windows/apps/develop/title-bar?tabs=winui3), [RichEditBox](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.richeditbox?view=windows-app-sdk-1.8), [WPF windows](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/windows/), [WPF RichTextBox](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/richtextbox-overview); [WinUIEdit README](https://github.com/BreeceW/WinUIEdit).

## Channels and maintenance

Microsoft's [channel table](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/release-channels), updated 2026-09-29, reports Stable **2.5.1 (2026-09-16)**, Preview **2.0 Preview2 (2026-03-31)**, and Experimental **2.5.4-experimental (2026-09-29)**. These are the page's labels, not normalized version guesses. Stable is supported; Preview/Experimental are unsupported and may break/change. Nothing reviewed establishes that an experimental channel supplies the missing Markdown editor, so it offers no demonstrated reason to switch.

WinUIEdit's [NuGet package](https://www.nuget.org/packages/WinUIEdit) is **0.0.5-prerelease, updated 2026-07-04**. Its repository shows 349 commits, 18 open issues and 4 PRs at inspection, with no published GitHub releases displayed. Those counts show a project exists, not response-time or stability guarantees. Documentation offers setup/demo plus upstream Scintilla API references; its readiness warning outweighs the package's availability.

## Carousel

WinUI's [InteractionTracker](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.interactions.interactiontracker) with `VisualInteractionSource` is the strongest first-party candidate: bind visual offset to tracker position; observe Interacting → Inertia → Idle, use release velocity, and configure custom inertia/rest behavior. Standard [ScrollView](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.scrollview) supplies scrolling/snap infrastructure; bespoke rubber band/new-note thresholds remain application logic. Neither establishes runtime success for an embedded editor.

WPF's stock rich-text control supplies no equivalent carousel. Pointer drag is straightforward custom UI work; Windows precision-touchpad semantics need a separately validated native route. Microsoft documents [touchpad-capable registration](https://learn.microsoft.com/en-us/windows/win32/input-precisiontouchpad/registertouchpadcapable) for WM_POINTER delivery; do not assume it works on every Windows 11 build or can be bolted onto framework input without routing changes. Wheel deltas alone do not identify finger lift.

## Decision

Do not restart a WPF/WinUI custom-editor spike. The user's cost concern is supported by the missing editor integration, although research cannot validate their prior hours or quantify future effort. WinUI is credible for materials and input; the editor remains the concrete reason to consider third-party stacks. Retain InteractionTracker/native input as a targeted fallback if A/B cannot satisfy the mandatory carousel.
