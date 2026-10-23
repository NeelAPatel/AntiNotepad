# M9c-behaviour-presentation-split

Completed c1-editor, then c2-carousel. Both gates passed and both source snapshots are saved. The manager retains all git and coordination-board ownership. No git commands, network, process kills, desktop control or `--measure` were run. The explicit write-only task scope leaves `.coord/STATUS.md`, `.coord/HANDOFF.md` and docs unchanged; this report and the scratch evidence carry the milestones.

## Scope and composition

Only `src/AntiQuickPad/Editor/` and `src/AntiQuickPad/Carousel/` source files changed. No call-site changes were needed: namespaces, EditorFactory.Create/Release, CarouselController constructor, View, IsBusy, Offset, Frames, RequestWheel, BeginDrag, MoveDrag, EndDrag, Cancel, MotionChanged, RunSmoke, SnapIndex and EasedOffset remain available. Core, ViewModels, Controller, Contract, all shell layouts, Design and Look values are unchanged. The snapshot comparison is `.coord/scratch/M9c/source-audit.txt`.

Presentation remains driven by the existing Look inputs. Existing fixed drawing geometry stays in presentation classes at its original values; no new tokens were necessary. The renderer order remains base typeface, active line (when modern), Markdown colorizer, code panel, strike, checkbox, indent guides. The existing Selection/Background layers remain unchanged.

## c1-editor

Files moved or created:

- `Editor/Behaviour/EditorBehaviour.cs`: checkbox press, hover hit-test requests/cursor policy, Enter/Tab, document redraw and folding update subscriptions, caret-to-guide state tracking, fold toggle action, release actions.
- `Editor/Behaviour/EditorOperations.cs`: moved document edit planning/VM dispatch and undoable application from EditorFactory. Adds explicit binding removal during release.
- `Editor/Behaviour/MarkdownScanner.cs`: moved shared document analysis/cache and its dirty subscription from EditorFactory.
- `Editor/Behaviour/GuideState.cs`: extracted indentation depth and active-block analysis; drawing stays outside it.
- `Editor/Presentation/EditorPresentation.cs`: editor construction, base typeface, active line, ordered renderer composition, cursor assets/tooltip, checkbox hit geometry lookup, fold brush/chevron styling and visual teardown.
- `Editor/Presentation/MarkdownRenderers.cs`: moved Markdown colorizer, code panel and adjacent grouping from EditorFactory.
- `Editor/Presentation/AlignmentRenderers.cs`, `StrikeRenderer.cs`, `ChevronMargin.cs`: moved from Editor root. Guide analysis delegates to GuideState. Chevron drawing and hit geometry remain here; fold mutation moved out.
- `Editor/EditorSmoke.cs`: retained existing smoke body behind EditorFactory.RunSmoke. EditorViewport and all pixel proof sources remain unchanged.
- `Editor/EditorFactory.cs`: thin create/release composition, with retained proof helper forwards.

Joins: EditorBehaviour.Attach takes the editor, shared scanner, IEditorPresentation and optional VM and returns a release Action. The additional presentation interface argument is the intentional adaptation of the proposed Attach signature: behaviour requests hit offsets, cursor state, guide invalidation and fold styling without choosing brushes, fonts or drawing geometry. ChevronMargin.FoldingAt supplies the nearest hit section; EditorBehaviour.AttachChevron owns pointer handlers and toggles IsFolded. Its release Action is detached when replacing the margin and when releasing the editor. Existing ChevronMargin.Install callers remain compatible.

Release now detaches all named editor input/caret handlers, document redraw/fold handlers, scanner dirty handler and chevron handlers, unbinds the document VM, uninstalls folding and clears presentation before replacing the retired editor document. Appearance rebuilds still transfer the original document/undo to the new editor.

Gate: initial build failed with CS1503 because a lambda sender named `_` bound `out _` as object. Fix attempt 1 named the ignored hit offset, then the complete gate passed. No rollback was needed.

- `--quit`: exit 0.
- Release/no-restore/shared-compilation-disabled build: exit 0, **0 warnings, 0 errors**.
- `--selftest`: exit 0; `CONFORMANCE quiet PASS (24 commands)` and `CONFORMANCE card PASS (24 commands)`; all four shell/palette combinations passed six heading levels, checkbox hit/hover/undo/row pixels, strike typeface/coverage, active guides, fold targets and alignment raster proofs. Quiet/Card/Classic capability suites passed.
- Internal show-to-focus: **2.861–6.572 ms**, every sample below 100 ms.
- Live frame samples: Quiet 65, Card 61; timestamps and offsets monotonic, final offsets -780 and -840 respectively. Timestamp sampling depends on the frame clock; the formulas and duration are unchanged.
- Quiet dark and Card light renders: **28/28 PNGs byte-for-byte identical** to pre-change baseline, including both scales and closeups. No pixel differences to explain.
- Snapshot: `.coord/scratch/M9c/c1/` (all app source excluding bin/obj); marker `c1.done`.

Raw evidence: `c1-quit.log`, `c1-build.log`, `c1-selftest.log`, `c1-render.log`, `c1-render-card.log`, `c1-render-diff.txt`, `subscriptions-c1.txt` under `.coord/scratch/M9c/`. The build log is the successful retry. The render comparison helper initially expected ten files and an unavailable Python imaging package; it was corrected to compare the actual fourteen-file inventory per shell directly as bytes. No packages were installed and no product source fix was required for comparison.

Remaining c1-editor risks: bitmap proofs exercise geometry and programmatic hover/hit helpers, not real pointer delivery, cursor transitions across child controls, physical fold clicks or held keyboard input. The existing shell/theme smoke exercises document/undo transfer, but it is not a long-running leak test.

## c2-carousel

Files created/changed:

- `Carousel/CarouselMotion.cs`: WheelGate moved unchanged; owns drag origin/last sample/velocity, 80 ms stale-velocity rule, 1:1/clamped drag and edge quarter resistance, target selection, half-width/.55 velocity snap, 140 ms burst gate, 280 ms slide, smoothstep wheel curve, cubic settling curve, cancellation versions, first-frame origin, sampled offsets and selection callback. It contains no Avalonia types, controls or wall clock.
- `Carousel/CarouselView.cs`: owns viewport/host, transforms, real read-only neighbour preview, page decoration/stride, grab strip brush/glyph/font/cursor/tooltip, input event adapters, pointer capture, size-change forwarding, preview scroll restore and TopLevel frame-clock hookup.
- `Carousel/CarouselController.cs`: stable composition and forwards. Frames still returns the same list and retained nested FrameSample value type, now populated by motion. Keeping that value type on the controller preserves the existing internal API without copying or projecting samples.
- `Carousel/README.md`: documents the boundary and corrects its stale 220 ms prose to the existing runtime value, 280 ms.

Joins: ICarouselSurface carries only numbers and callbacks. Motion requests preview, read-only state, reset, pointer release and frame scheduling; View supplies page stride and applies current/preview offsets. View delivers timestamps to Motion.Advance and only reschedules when it requests another frame. MotionChanged forwards directly, preserving callback order. Motion owns preview target/visibility policy; View owns the editor and wrapper visibility. All existing wheel/pointer routing strategies, handled flags, capture/release sequencing and scroll restoration remain in place.

The existing RunSmoke maths moved to CarouselMotion. A small control-free surface additionally drives the actual extracted motion state machine: exact drag/settling offsets (-400, -400, -750, -800), selection after completion, normal slide midpoint, stale-frame rejection after cancel, and idle boundary at exactly 140 versus 141 ms. This establishes the model can run using plain numbers without creating Avalonia controls. Existing 60/120/144 Hz curve checks and integrated shell capability checks remain.

Gate: passed on the first attempt; no source fix or rollback needed.

- `--quit`: exit 0.
- Release/no-restore/shared-compilation-disabled build: exit 0, **0 warnings, 0 errors**.
- `--selftest`: exit 0; both 24-command conformance checks, all four shell/palette heading/checkbox/strike/guide/fold/alignment proof sets, retained Classic suite and numeric motion smoke PASS.
- Internal show-to-focus: **2.449–6.688 ms**, every sample below 100 ms.
- Live frame samples: Quiet 68, Card 62; timestamps and offsets monotonic, final offsets -780 and -840. Existing 60/120/144 Hz curve maxima remain 71.3341 / 35.6924 / 29.7473.
- Quiet dark and Card light renders: **28/28 PNGs byte-for-byte identical** to the original pre-change baseline. No pixel differences to explain.
- Snapshot: `.coord/scratch/M9c/c2/` (all app source excluding bin/obj); marker `c2.done`.

Raw evidence: `c2-quit.log`, `c2-build.log`, `c2-selftest.log`, `c2-render.log`, `c2-render-card.log`, `c2-render-diff.txt`, `subscriptions-c2.txt` under `.coord/scratch/M9c/`. `before/` holds the start source snapshot; `render-before/` and `render-card-before/` hold baseline PNGs. `gate.ps1`, `compare-renders.py` and `audit.py` record the local verification commands. Alignment output used the required absolute repository `.coord/scratch/align` directory. Hidden-window checks also passed. The conformance suite still reports its pre-existing native window.move route known-gap before its PASS; this task does not change that check.

Remaining c2-carousel risks: deterministic timestamps establish math, callback state and frame offsets, not subjective wheel feel on a real mouse. Real pointer capture outside the grab strip, capture loss on deactivation, device event cadence and compositor presentation at physical DPI/refresh rates remain unverified. No performance claim beyond the scripted internal focus checks is made.

## Subscription accounting after each sub-step

The requested pre-change grep-equivalent is `subscriptions-before.txt` (rg with `\+=|AddHandler`). There are **19 actual subscription sites**: 12 editor/scanner/chevron and 7 carousel. The other three matched lines are arithmetic += expressions, not subscriptions. Named handlers retain the original event registration order; detach counterparts now exist for every editor/scanner/chevron handler.

| Original subscription | After c1-editor | After c2-carousel |
| --- | --- | --- |
| EditorFactory caret PositionChanged | EditorBehaviour.Attach / caretChanged | Same |
| EditorFactory tunnel PointerPressed | EditorBehaviour.Attach / pressed | Same |
| EditorFactory document Changed / redraw | EditorBehaviour.Attach / redraw | Same |
| EditorFactory tunnel PointerMoved | EditorBehaviour.Attach / moved | Same |
| EditorFactory editor PointerExited | EditorBehaviour.Attach / exited | Same |
| EditorFactory TextView PointerExited | EditorBehaviour.Attach / exited | Same |
| EditorFactory tunnel KeyDown | EditorBehaviour.Attach / key | Same |
| EditorFactory document Changed / folding update | EditorBehaviour.Attach / update | Same |
| MarkdownScanner document Changed / dirty | Behaviour/MarkdownScanner constructor | Same |
| ChevronMargin PointerMoved | EditorBehaviour.AttachChevron / moved | Same |
| ChevronMargin PointerExited | EditorBehaviour.AttachChevron / exited | Same |
| ChevronMargin PointerPressed | EditorBehaviour.AttachChevron / pressed | Same |
| CarouselController preview PropertyChanged | Unchanged in CarouselController | CarouselView constructor, wrapper visibility |
| CarouselController tunnel PointerWheelChanged | Unchanged in CarouselController | CarouselView.Attach / Wheel |
| CarouselController grab PointerPressed | Unchanged in CarouselController | CarouselView.Attach, forwards BeginDrag then captures |
| CarouselController grab PointerMoved | Unchanged in CarouselController | CarouselView.Attach, forwards MoveDrag |
| CarouselController grab PointerReleased | Unchanged in CarouselController | CarouselView.Attach, forwards MoveDrag/EndDrag then releases |
| CarouselController grab PointerCaptureLost | Unchanged in CarouselController | CarouselView.Attach, forwards Cancel while dragging |
| CarouselController viewport SizeChanged | Unchanged in CarouselController | CarouselView.Attach, forwards Cancel while busy |

No original subscription was dropped or duplicated. The new CarouselController.MotionChanged add/remove accessor delegates the caller's subscription to motion; it does not create an extra subscriber. CarouselView handlers continue to live with the carousel visual tree, as before; Cancel releases pointer capture and invalidates pending frames. Preview EditorFactory cleanup is unchanged in lifetime policy (no new carousel disposal API).

## User physical checks

Perform these in Quiet and Card, with light and dark appearance and at least one scrolled note:

1. Type immediately after hotkey show; Enter continues/exits lists, Tab/Shift+Tab indent/outdent, selection edits and Undo still behave as before.
2. Hover and click checked/unchecked boxes, including nested and wrapped rows. Confirm hand cursor only over the box, text cursor returns on exit, toggle preserves caret and one Undo restores it.
3. Hover and click heading/fence fold chevrons. Confirm identical hover rectangle, fold/unfold, nearby hit targeting and scroll layout. Move the caret among nested lists/code and confirm active guide blocks.
4. Switch theme/shell repeatedly while editing; confirm caret, scroll, document/undo and folds behave as before, with no duplicate toggles or key edits.
5. Use real horizontal wheel and Shift+vertical wheel: one burst selects one neighbour, busy input queues nothing, a later burst works, and ordinary vertical scrolling stays in the editor. Check first/last note boundaries.
6. Drag the strip slowly in both directions: 1:1 tracking, short return, half-page commit, fast short flick, pause before release, edge resistance, identical landing and preview scroll. Release outside the strip; check capture loss, resize and window deactivation cancellation.
7. Confirm the grab cursor/tooltip, checkbox hand cursor and fold hover remain correct after repeated navigation, theme/shell replacement and cancel/release sequences. Check real monitor DPI and refresh-rate changes visually.

How to restyle the editor/carousel: a UI task may change Look presentation inputs (in a separately authorized scope), `Editor/Presentation/EditorPresentation.cs`, `MarkdownRenderers.cs`, `AlignmentRenderers.cs`, `StrikeRenderer.cs`, `ChevronMargin.cs`, and `Carousel/CarouselView.cs`; caller-supplied page decorators/stride may change in their authorized shell presentation scope. Preserve the IEditorPresentation/ICarouselSurface joins, renderer layering/order where required, and leave `Editor/Behaviour/`, `Carousel/CarouselMotion.cs` and VM/selection/timing policy untouched. No edits to behaviour are needed for colours, fonts, geometry, grab-strip styling or replacing the drawing classes.
