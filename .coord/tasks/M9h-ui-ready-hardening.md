# M9h-ui-ready-hardening (M9-function-contract-separation follow-up)

Director: Codex gpt-6.1-sol medium. Do it yourself.
Branch: feat/function-contract-separation (the manager handles ALL git; you run no git commands that write).
Naming: always `<ID>-<slug>`, never a bare ID. Finish in ONE turn (90 min wall limit). End when the report is written, all three sub-steps are done, or a gate fails twice on a sub-step.

## Why
The next phase is a full UI overhaul (new layouts, new look). It must be able to replace `Views/Quiet`, `Views/Card`, the menu and palettes by binding to ready-made commands and projections only. Three gaps remain: display logic still computed in the window, controls bound by button label instead of command ID, and the folder boundaries enforced only by convention. **Observable behaviour and pixels must not change.**

## Read
AGENTS.md, docs/function-contract.md (Commands, projections, Ownership), docs/architecture.md, `.coord/reports/M9b-resident-controller.md`, `.coord/reports/M9c-behaviour-presentation-split.md`, `src/AntiQuickPad/Views/Common/*.cs`, `Views/Quiet`, `Views/Card`, `Controller/*.cs`, `Contract/FunctionContract.cs`, `ViewModels/*.cs`.

## Write only
`src/AntiQuickPad/ViewModels/`, `Controller/`, `Contract/`, `Views/Common/`, `Views/Quiet/`, `Views/Card/` (binding call sites only: no geometry, colour, glyph, spacing or text-content changes), `Editor/Presentation/` (only to drop the unused `CoreText` using aliases), `.coord/reports/M9h-ui-ready-hardening.md`, snapshots under `.coord/scratch/M9h/`.
NOT: Core, Editor/Behaviour, Carousel, Shell, Look/ShellPalette values, docs/. No git, no network, never kill processes.

## Sub-steps (in order; each ends with the gate and a snapshot)
- **h1 projections**: move display-state computation out of `ResidentShellWindow.Appearance.cs` into `MainViewModel` (plain INPC, no Avalonia): position text "n of m" (keep existing `CountText` "n / m" untouched for Classic), save-line text ("Could not save..."/"Saving" when failed or saving, else "Ready to write" for an empty current note, else "All changes saved") plus a failed flag, the pager dot window (first index and the visible indices, max 8, current-3 clamped exactly as today) with an `IsCurrent` per dot, `HasPrevious`/`HasNext`, and a `SelectCommand` taking the note index. Raise change notifications whenever the inputs change (navigation, text edits via `UpdateCurrent`, status). The window then only copies these values into controls and picks brushes. `ShellLayoutContext.HasPrevious/HasNext` read the VM. The empty-hint rule stays in the window (it depends on carousel motion), but takes its "note is empty" input from the VM.
- **h2 bind by command ID**: `ShellLayoutContext.Button(glyph, label)` becomes `Button(glyph, label, commandId)`, with the command resolved via the controller's `Command(id)`. Quiet and Card layouts pass the ID (for example `c.Button("", "New note", "note.create")`); the label stays the tooltip and accessible name. Menu items bind by ID too. Remove label -> ID mapping from `ResidentController.CommandFor`, or keep it only as an obsolete path that nothing in Views uses. Every bound ID must exist in `FunctionContract`; an unknown ID throws at build-time of the layout (fail fast in the selftest). The conformance check keeps working (toolbar/menu routes are still found by label; resolve them however is cleanest, e.g. the context records label -> ID).
- **h3 boundary check**: add a selftest check (new `Contract/BoundaryCheck.cs`, called once from the selftest) that inspects the compiled assembly with `System.Reflection.Metadata` (BCL only, no new packages). For every type, it collects the namespaces of the types and members its method bodies and fields reference, and asserts:
  - types in `AntiQuickPad.Views.Quiet`, `AntiQuickPad.Views.Card`, and `Editor/Presentation` types (namespace or a clear marker; Codex picks and documents) never reference `AntiQuickPad.Core`, and never call ViewModel mutators other than through `ICommand` (simplest rule: Quiet/Card layouts reference no `MainViewModel` members except read-only projections; document the exact rule);
  - types in `AntiQuickPad.Controller` and `AntiQuickPad.ViewModels` never reference `AntiQuickPad.Views.*`, `ShellPalette`, `Look`, or `Avalonia.Media` (brushes/colours);
  - `AntiQuickPad.ViewModels` never references any `Avalonia` namespace.
  Print `BOUNDARY PASS (<n> types checked)` or throw listing every violation (type -> referenced member). Prove it can fail: temporarily add a forbidden reference in a scratch build or behind a test-only env var, show FAIL, remove. If current code violates a rule, fix the code when it is a trivial move; otherwise record it in a named known-violation list that prints `BOUNDARY KNOWN` and explain why in the report.

## Gate after EVERY sub-step (all must pass)
From src/AntiQuickPad: `dotnet bin/Release/net10.0/AntiQuickPad.dll --quit` (never kill); `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build -c Release --no-restore -p:UseSharedCompilation=false` = 0 warnings / 0 errors; `--selftest` (env `ANTIQUICKPAD_ALIGNMENT_OUTPUT` = `.coord/scratch/align`) exit 0 with `CONFORMANCE quiet PASS`, `CONFORMANCE card PASS`, all pixel proofs PASS, show-to-focus < 100 ms (and from h3 on, `BOUNDARY PASS`). Make pre-change renders first (`--render-shell quiet dark .coord/scratch/M9h/render-before`, `--render-shell card light .coord/scratch/M9h/render-card-before`) and after each sub-step render again and compare PNGs byte-for-byte; any difference must be fixed or explained. Do NOT run `--measure`.
- Gate passes: copy `src/AntiQuickPad` (excluding bin/ and obj/) to `.coord/scratch/M9h/h<N>/` and write `.coord/scratch/M9h/h<N>.done` with a one-line summary.
- Gate fails: up to two fix attempts; then restore from the previous snapshot (or the start state), write `.coord/scratch/M9h/h<N>.failed`, and STOP.

## Report `.coord/reports/M9h-ui-ready-hardening.md`
Per sub-step: what moved, new VM members (name, type, when it changes), binding API, boundary rules exactly as enforced, proof the check fails, known violations, gate output, render diff result. End with "What a new shell binds to": the projection names and the command-ID binding call, in a short list a UI task can follow.
