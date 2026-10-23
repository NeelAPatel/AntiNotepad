# M9c-behaviour-presentation-split (M9-function-contract-separation, part c)

Director: Codex gpt-6.1-sol medium. Do it yourself; a Luna subagent only for a clearly separable read/check.
Branch: feat/function-contract-separation (the manager handles ALL git; you run no git commands that write).
Naming: always `<ID>-<slug>`, never a bare ID. Finish in ONE turn (90 min wall limit). End when the report is written, both sub-steps are done, or a gate fails twice on a sub-step.

## Goal
Split Editor and Carousel into **behaviour** (input handling, state machines, timing, document edits, VM calls) and **token-driven presentation** (drawing, brushes, fonts, geometry, visual tree), so a later UI task can restyle or replace the presentation by changing `Look` tokens or presentation classes only, without touching behaviour. **Observable behaviour and pixels must not change**: same key/pointer handling, same wheel gate (140 ms burst), same drag 1:1 and snap rule, same 280 ms slide and easing, same frame offsets, same colours, fonts, sizes and renderer order.

## Read
AGENTS.md, docs/function-contract.md (Ownership section), docs/architecture.md, `.coord/reports/M9b-resident-controller.md`, all of `src/AntiQuickPad/Editor/`, `src/AntiQuickPad/Carousel/` (incl. README.md), `Look.cs`, `Views/Common/ResidentShellWindow.Appearance.cs` (how they are created), `Views/Classic/ClassicShellWindow.cs` and `Design/DesignWindow.cs` (other callers; their call sites may change, their behaviour may not).

## Write only
`src/AntiQuickPad/Editor/`, `src/AntiQuickPad/Carousel/`, call sites in `Views/Common/ResidentShellWindow.Appearance.cs`, `Views/Classic/ClassicShellWindow.cs`, `Design/` (call-site changes only), `.coord/reports/M9c-behaviour-presentation-split.md`, snapshots under `.coord/scratch/M9c/`.
NOT: Core, ViewModels, Controller, Contract, Views/Quiet, Views/Card, `Look.cs` values (adding no new tokens unless a hard-coded presentation literal must become one; keep its current value as the default), docs/. No git, no network, never kill processes.

## Target shape (adapt names if something fits better; explain in the report)
- **Editor**
  - `Editor/Behaviour/`: `EditorBehaviour.Attach(TextEditor, MarkdownScanner, MainViewModel?)` installs checkbox click toggle, hand-cursor hit testing, Enter/Tab key handling, the document-change -> fold-update wiring, caret -> active-guide line tracking (state only), and returns a release action. `EditorOperations` and `MarkdownScanner` are behaviour/analysis and move here (or stay, if moving adds risk; say which).
  - `Editor/Presentation/`: colorizer, base typeface, active line, code panel, strike, checkbox, indent guide renderers, fold margin brush and `ChevronMargin` drawing. They read `Look` only. Where a class mixes hit testing with drawing (ChevronMargin, checkbox hit test), keep geometry in presentation and the action (toggle fold / toggle checkbox) in behaviour, joined by a small interface or delegate.
  - `EditorFactory.Create(look, vm, document)` keeps its signature and simply composes: presentation from `look`, then behaviour. `EditorFactory.Release` releases both. The proofs/smokes keep working.
- **Carousel**
  - `Carousel/CarouselMotion.cs` (behaviour, no Avalonia controls; `Vector` is fine): wheel gate, drag state (origin, last x, velocity), snap decision, target selection, animation progress/easing and the frame-sample list, selection callback. Unit-testable with plain numbers; move the existing `RunSmoke` maths onto it.
  - `Carousel/CarouselView.cs` (presentation): viewport grid, translate transforms, preview editor, grab strip (brush, glyph, cursor, tooltip), page decorator and stride, frame clock hookup. It renders whatever offset the motion reports and forwards pointer/wheel input to the motion.
  - `CarouselController` keeps its public surface (constructor, `View`, `IsBusy`, `Offset`, `Frames`, `RequestWheel`, `BeginDrag`, `MoveDrag`, `EndDrag`, `Cancel`, `MotionChanged`, `RunSmoke`) as a thin composition so callers and selftests do not change.

## Sub-steps (in order; each ends with the gate and a snapshot)
- **c1 editor**: the Editor split above.
- **c2 carousel**: the Carousel split above.

## Gate after EVERY sub-step (all must pass)
From src/AntiQuickPad: `dotnet bin/Release/net10.0/AntiQuickPad.dll --quit` (never kill); `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build -c Release --no-restore -p:UseSharedCompilation=false` = 0 warnings / 0 errors; `--selftest` (env `ANTIQUICKPAD_ALIGNMENT_OUTPUT` = `.coord/scratch/align`) exit 0 with `CONFORMANCE quiet PASS`, `CONFORMANCE card PASS`, all T013/T014 pixel proofs (headings, checkbox, strike, guides, alignment, frames monotonic) PASS and show-to-focus < 100 ms. Also run `--render-shell quiet dark .coord/scratch/M9c/render-c<N>` and `--render-shell card light .coord/scratch/M9c/render-card-c<N>`, and compare every PNG byte-for-byte (or pixel diff) against a pre-change render you make first into `.coord/scratch/M9c/render-before` and `render-card-before`. Any pixel difference must be explained in the report or fixed. Before starting, save `grep -rn "+=\|AddHandler" src/AntiQuickPad/Editor src/AntiQuickPad/Carousel` to `.coord/scratch/M9c/subscriptions-before.txt`; after each sub-step list where each subscription went. Do NOT run `--measure`.
- Gate passes: copy `src/AntiQuickPad` (excluding bin/ and obj/) to `.coord/scratch/M9c/c<N>/` and write `.coord/scratch/M9c/c<N>.done` with a one-line summary.
- Gate fails: up to two fix attempts. If it still fails, restore sources from the previous snapshot (or the start state for c1), write `.coord/scratch/M9c/c<N>.failed` with the reason, and STOP.

## Report `.coord/reports/M9c-behaviour-presentation-split.md`
Per sub-step: files moved/created, what is behaviour vs presentation, the joins between them, gate output, render diff result, subscriptions accounting, behaviour risks not covered by tests (wheel feel on a real mouse, drag capture, hover cursor), and a list for the user's physical check. End with a short "How to restyle the editor/carousel" paragraph naming the files a UI task may touch.
