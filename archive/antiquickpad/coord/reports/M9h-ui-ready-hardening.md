# M9h-ui-ready-hardening

Completed the three ordered sub-steps in one director turn, without delegation. All three gates passed; source snapshots and done markers are saved. No gate required a fix attempt or rollback. The deliberately failing boundary probe was a separate negative test, removed before the final gate.

No git commands, network, process kills, desktop control or `--measure` were used. The task's write-only list takes precedence over the general coordination instruction: STATUS, HANDOFF, tracker and docs are left to the manager. Source changes are limited to the authorized folders. Quiet/Card diffs contain only command-ID arguments; Editor/Presentation diffs only remove two unused CoreText aliases. Source audit and unified diff: `.coord/scratch/M9h/source-audit.txt`, `source-diff.txt`. The resident was quit for the gates and was not relaunched.

## M9h-ui-ready-hardening / h1-projections

Moved the footer position, save-line policy, empty-note input, neighbour availability and eight-dot pager calculation from the window/context into plain INPC MainViewModel. The formula is unchanged: `Math.Clamp(CurrentIndex - 3, 0, Math.Max(0, Notes.Count - 8))`, followed by up to eight consecutive indices. Existing Classic `CountText` remains `n / m`.

| New MainViewModel member | Type | Notification inputs |
| --- | --- | --- |
| PositionText | string | Navigation/Refresh: index or note count, formatted `n of m` |
| IsCurrentEmpty | bool | Navigation/Refresh and UpdateCurrent: current text length is zero |
| SaveLineText | string | Navigation/Refresh, UpdateCurrent and Status.PropertyChanged |
| SaveLineFailed | bool | Navigation/Refresh and Status.PropertyChanged |
| HasPrevious | bool | Navigation/Refresh: CurrentIndex > 0 |
| HasNext | bool | Navigation/Refresh: CurrentIndex + 1 < Notes.Count |
| PagerFirstIndex | int | Navigation/Refresh: exact clamp formula above |
| PagerDots | IReadOnlyList<PagerDot> | Navigation/Refresh; each immutable PagerDot has int Index and bool IsCurrent |
| SelectCommand | ICommand | Stable instance, executing an int parameter through the existing Select/navigation protocol |

SaveLineText preserves failure priority and existing strings: Status.Text when failed or saving (`Could not save. Check disk access.` / `Saving`), otherwise `Ready to write` for empty text, otherwise `All changes saved`. Status.PropertyChanged forwards notifications for both save-line projections. Refresh raises all dependent projection notifications; UpdateCurrent raises empty/text notifications after updating the projected current note. IndexCommand rejects non-int parameters, without introducing Avalonia into ViewModels.

ResidentShellWindow.Appearance now copies the VM values into the existing controls and chooses the existing brushes. Pager buttons copy Index/IsCurrent and execute SelectCommand with CommandParameter = Index (resolved through `note.select` after h2-command-ids). ShellLayoutContext.HasPrevious/HasNext delegate to the VM. RefreshHint still owns `!carousel.IsBusy` and reads VM.IsCurrentEmpty for the note input. All geometry, accessible pager names and visual styling remain unchanged.

Added a small control-free ProjectionSmoke in ViewModels, invoked by the resident selftest. It checks empty/edited/saved/saving/failed text, empty/save failure notifications, and all twelve positions of a twelve-note pager (eight visible indices, first-index clamp, selected dot, previous/next and navigation notifications). Existing shell smoke also passes hint insertion/deletion, navigation, appearance rebuild and motion/cancel checks.

Gate: quit/build/selftest exit 0; 0 warnings/errors; both 24-command conformance checks and all pixel proofs PASS. Quiet dark + Card light: 28/28 PNGs byte-identical. Snapshot `h1/`, marker `h1.done`.

## M9h-ui-ready-hardening / h2-command-ids

Binding API: `ShellLayoutContext.Button(string glyph, string label, string commandId)`. The label remains the tooltip and accessible name. The command resolves through ResidentShellWindow.Command(id) -> ResidentController.Command(id), which first calls FunctionContract.RequireCommand(id). Unknown IDs throw ArgumentException while constructing a layout, before a binding enters the route inventory. Known IDs without an implemented controller binding also throw. Added `note.select` to that controller map, returning the VM SelectCommand.

Quiet/Card call sites supply explicit IDs, for example `c.Button("\uE710", "New note", "note.create")`. Removed CommandFor and its label-to-ID switch entirely. Menu Item accepts label + command ID and resolves through the same controller path. Final theme/shell menu entries use explicit label/ID/preference tuples, so their IDs are not derived from visible labels (this final audit refinement was included in the h3-boundary-check gate).

ShellLayoutContext records label -> ID alongside label -> Button. The menu records label -> ID alongside its existing command dictionary. HasPresentationRoute receives the expected command ID from the controller; toolbar/menu routes are available only when both the label and bound ID match. Thus conformance still searches the documented route labels and also rejects a label bound to a different valid command.

Selftest proves an unknown ID is rejected synchronously during layout construction and never registered, and a differently named button still resolves `note.create` to the same VM CreateCommand. The temporary controls are never added to the visual tree and their route records are removed. Output: `COMMAND-BINDING PASS (unknown ID rejected during layout construction; label-independent command)`.

Gate: quit/build/selftest exit 0; 0 warnings/errors; both 24-command conformance checks and all pixel proofs PASS. Quiet dark + Card light: 28/28 PNGs byte-identical. Snapshot `h2/`, marker `h2.done`.

## M9h-ui-ready-hardening / h3-boundary-check

New Contract/BoundaryCheck.cs is called once, at the start of the resident selftest. It opens the running assembly with PEReader/System.Reflection.Metadata, using only BCL APIs and existing framework references. No package or project changes.

For every compiled TypeDef, it collects direct referenced types/members from fields, method signatures and IL token operands, including generic arguments/instantiations, local signatures, catch types, arrays, pointers, byrefs, modifiers and function pointers. It also inspects base/interface/property/event types. IL decoding handles operand sizes and switch tables rather than searching raw bytes for tokens. Nested types inherit the ownership of their outermost type, so generated closure/iterator/state-machine code is checked. Violations are deduplicated and sorted, and the exception lists every offending `source type -> referenced member/type`.

Exact enforced rules:

- Namespace roots AntiQuickPad.Views.Quiet and AntiQuickPad.Views.Card (including child namespaces and nested/generated types), plus marked editor presentation roots, may not reference AntiQuickPad.Core or its child namespaces.
- Those same shell/presentation types may read VM property getters and obtain ICommand projections. Any referenced ViewModels member that is not an actual metadata property-getter MethodDef is rejected: ordinary methods, constructors, setters, event accessors and field access. Getter calls are read operations even when the underlying property also has a setter; calling that setter remains forbidden. ICommand.Execute is allowed because it targets System.Windows.Input.ICommand, not a VM mutator. This rule applies to all ViewModels types, not only MainViewModel. Referencing a VM type as a field/parameter is allowed; accessing VM fields is not.
- AntiQuickPad.Controller and AntiQuickPad.ViewModels roots/children/nested types may not reference AntiQuickPad.Views or its child namespaces, AntiQuickPad.Look, any type whose full name ends in `.ShellPalette`, or Avalonia.Media and its child namespaces.
- AntiQuickPad.ViewModels additionally may not reference Avalonia or any child namespace. AvaloniaEdit is a separate namespace; no current VM references it.

Editor presentation marker: the explicit PresentationTypes manifest in BoundaryCheck contains the nine existing top-level types from Editor/Presentation: IEditorPresentation, EditorPresentation, MarkdownColorizer, CodePanelRenderer, AdjacentGrouping, CheckboxRenderer, IndentGuideRenderer, StrikeRenderer and ChevronMargin (all under AntiQuickPad.Editor). Every nested/generated type under each root is included automatically. The check also rejects a manifest entry missing from the assembly. New top-level editor presentation types must be added to this manifest; moved/renamed roots must update it. This explicit marker avoids unauthorized namespace/attribute edits in renderer files. Unused CoreText using aliases were removed from EditorPresentation.cs and MarkdownRenderers.cs; no renderer implementation changed.

This checks direct compiled dependencies, not a transitive call graph or reflection/string-based invocation. Unused source aliases have no compiled dependency. Common host adapters and the separately owned editor behaviour remain outside the presentation rule.

Known boundary violations: **none**. The named KnownViolations dictionary is empty. Its exact source -> target entries would print `BOUNDARY KNOWN` with their reason rather than being silently ignored; no such waiver was needed. The unrelated existing FunctionContract window.move route KNOWN-GAP remains visible in both conformance runs.

Negative proof: temporarily compiled Contract/BoundaryNegativeProbe.cs with types in Quiet, Controller and ViewModels. It included a generic Core field/type token, a VM Select call in a lambda, controller references to a Views type/Look/Avalonia.Media, and an Avalonia.Size reference from a VM. Negative build: exit 0, 0 warnings/errors. Selftest: expected exit 1, `BOUNDARY FAIL`, listing all offending targets, including the generated closure's MainViewModel.Select. A legal PositionText getter in the same fixture was not reported. The fixture source was copied to scratch and removed from product sources in finally; no environment-variable injection or probe remains in the final assembly.

Evidence: `.coord/scratch/M9h/boundary-negative-probe.cs`, `h3-negative-build.log`, `h3-negative-selftest.log`, `h3-negative-exit.txt`. Representative rejected edges:

```text
AntiQuickPad.Views.Quiet.BoundaryNegativeShell -> AntiQuickPad.Core.NotesSession
AntiQuickPad.Views.Quiet.BoundaryNegativeShell+<>c__DisplayClass5_0 -> AntiQuickPad.ViewModels.MainViewModel.Select
AntiQuickPad.Controller.BoundaryNegativeController -> AntiQuickPad.Look
AntiQuickPad.Controller.BoundaryNegativeController -> Avalonia.Media.Colors.get_Red
AntiQuickPad.ViewModels.BoundaryNegativeViewModel -> Avalonia.Size..ctor
```

Final gate after removing the probe: quit/build/selftest exit 0; 0 warnings/errors; `BOUNDARY PASS (218 types checked)`; both 24-command conformance checks and all pixel proofs PASS. Quiet dark + Card light: 28/28 PNGs byte-identical. Snapshot `h3/`, marker `h3.done`.

## M9h-ui-ready-hardening gate evidence

Every gate ran from src/AntiQuickPad: Release DLL `--quit`, `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build -c Release --no-restore -p:UseSharedCompilation=false`, then Release DLL `--selftest` with ANTIQUICKPAD_ALIGNMENT_OUTPUT set to the absolute repository `.coord/scratch/align` path. Every successful selftest emitted `CONFORMANCE quiet PASS (24 commands)`, `CONFORMANCE card PASS (24 commands)` and `SELFTEST PASS: Quiet + Card production shells and retained Classic capability suite.`

| Sub-step | Build | Selftest | Quiet internal focus ms | Card internal focus ms | Classic internal focus ms | PNG comparison |
| --- | --- | --- | --- | --- | --- | --- |
| h1-projections | exit 0; 0 warnings/errors | exit 0 | 5.199, 5.264, 5.613 | 6.885, 5.612, 6.868 | 5.360, 2.155, 3.788 | 28/28 byte-identical |
| h2-command-ids | exit 0; 0 warnings/errors | exit 0 | 6.118, 5.804, 4.992 | 6.119, 5.688, 5.954 | 3.876, 2.263, 2.273 | 28/28 byte-identical |
| h3-boundary-check | exit 0; 0 warnings/errors | exit 0; boundary PASS | 6.662, 7.587, 5.145 | 6.844, 6.098, 8.762 | 3.706, 2.806, 2.771 | 28/28 byte-identical |

All internal focus samples are below 100 ms. Every gate passed Quiet/Card light/dark heading, checkbox, strike, active-guide, fold-target and footer/menu alignment pixel proofs, carousel/navigation/persistence capability checks, and hidden-window assertions. These are scripted hidden-window focus and render proofs; physical hotkey-to-paint/foreground acquisition and real mouse/keyboard feel were not newly measured.

Pre-change captures: `render-before/` (Quiet dark), `render-card-before/` (Card light). After each sub-step: `render-h1/` through `render-h3/`, and corresponding `render-card-h1/` through `render-card-h3/`. Each shell has fourteen PNGs, covering six states at 1x/1.5x plus closeups. All after-images compare byte-for-byte against the original baseline; no differences to explain.

All evidence is under `.coord/scratch/M9h/`: per-step `hN-quit.log`, `hN-build.log`, `hN-selftest.log`, `hN-render.log`, `hN-render-card.log`, `hN-render-diff.txt`; reproducible `gate.ps1`, `compare.py`, `snapshot.py`; start snapshot `before/`; successful snapshots `h1/`, `h2/`, `h3/` (entire src/AntiQuickPad excluding bin/obj), and sibling done markers. The preliminary clean boundary run is `h3-preprobe-selftest.log`. The intentionally failing probe is separate from the three passing gates.

## What a new shell binds to

- PositionText (string); retain CountText only for the existing Classic `n / m` format.
- SaveLineText (string) and SaveLineFailed (bool); the shell chooses brushes.
- IsCurrentEmpty (bool); combine with carousel motion for the visual hint.
- HasPrevious / HasNext (bool) for neighbour visibility.
- PagerFirstIndex (int), PagerDots (IReadOnlyList<PagerDot> with Index/IsCurrent); bind dot Command to `Command("note.select")` and CommandParameter to Index.
- Pinned (bool) for pin presentation; commands execute through their IDs.
- Toolbar: `c.Button(glyph, label, "note.create")` (or another FunctionContract ID). Menu: `Item(label, "theme.dark", selected)`; both resolve through `ResidentController.Command(id)`. Labels are display/accessibility/route metadata, never command selectors.
