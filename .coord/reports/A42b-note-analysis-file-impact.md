# A42b-note-analysis-file-impact

Read-only re-audit, 2026-10-26. **Recommend proceeding with A42b-note-analysis-file as scoped, then stopping for the user gate.** A42a-math-notes has already removed the analysis-to-control calls. A42b makes that boundary visible and enforceable; it is organizational work, with no expected typing-speed improvement.

## Evidence and limits

- Read `AGENTS.md` first, `.coord/STATUS.md`, `.coord/tasks/A42-padview-commands-plan.md` (including r2 corrections and Recommendation on handling), and `.coord/reports/A42-padview-commands-impact-r2.md`; verified findings against current product sources.
- Branch `reorg/A42-padview-commands`, HEAD `2a2c791`, containing dev tip `037f4c4` and A42a merge `7f845be`. `git diff 037f4c4 HEAD -- product/AntiNotepad` is empty.
- Only this report was created. The explicit report-only instruction overrides STATUS-update and commit conventions. No product/plan/STATUS edits, git writes, application launch/termination, build, selftest, or deliberate violation. **No executed LAYERS/MATH NOTES/LIST EDITING tail exists for this audit.** All pass/zero-reference assessments below are source-derived expectations.
- Two pre-existing untracked reports (`A42-padview-commands-impact-r2.director.codex.md`, `A42-padview-commands-impact.director.codex.md`) were left untouched.
- References below are relative to `product/AntiNotepad/`. Current `SelfTest.cs:395-450` contains **56 LayerRule rows**, plus the settings-setter summary at :521, hence 57 LAYERS output lines before any new rules. The brief's 55-line count is stale.

## Exact move and dependencies

| Current source | Measured span / action |
|---|---|
| `Editor/PadView.cs:878-1132` | Entire top-level `internal sealed class NoteAnalysis`, 255 lines inclusive; include its summary at :877 for 256 lines. It is already outside PadView, whose closing brace is :875. |
| `Editor/PadView.cs:880` | Private nested sealed record `CachedLine(string Text)`; stays nested and private. |
| `Editor/PadView.cs:881` | Private nested sealed record `TimerLine(TimeSpan Duration, string Label, string DurationText)`; stays nested and private. |
| `Editor/PadView.cs:1131` | `internal static string TimerKey(...)`; a method, not a nested type. Move unchanged with its owner. |
| `Editor/PadView.cs:493-500` | Eight-line ChangedLineRange helper; move into NoteAnalysis with unchanged body/signature except `private` becomes `internal`. Keep `static`. |
| `Editor/PadView.cs:540` | Delete forwarding TimerKey shim. No runtime caller remains after test rewiring. |

New file namespace stays `AntiNotepad.Editor`; retain `internal sealed class NoteAnalysis`. No partial class, new abstraction, constructor, or state transfer is needed. Adding ChangedLineRange is the sole member/accessibility addition to the otherwise verbatim class.

The minimal explicit imports for `Editor/NoteAnalysis.cs` are:

```csharp
using System.Runtime.CompilerServices;
using AvaloniaEdit.Document;
using AntiNotepad.Calc;
using AntiNotepad.Model;
```

| Import / symbol family | Why needed; PadView disposition |
|---|---|
| `System.Runtime.CompilerServices` | ConditionalWeakTable at :883/:920. Remove this import from PadView (:3); no remaining use in its control body. |
| `AvaloniaEdit.Document` | TextDocument, DocumentLine, and moved DocumentChangeEventArgs. PadView still needs TextDocument/cache APIs and OnDocumentChanged's argument; keep :16. |
| `AntiNotepad.Calc` | LineEval, Calculator, CalcErrorReason (:889/:1009-1010). PadView still exposes LineEval/CalcResult (:537-539) and handles errors; keep :21. |
| `AntiNotepad.Model` | NoteMode, Modes, Timers (:893/:956/:938-939/:1105). PadView still uses Note, Settings, NoteMode, Timers; keep :22. |
| SDK implicit imports | System (Math, TimeSpan, Array, StringComparer), Collections.Generic (lists/dictionaries/sets), Linq (ToArray at :1057). `AntiNotepad.csproj` enables ImplicitUsings. No explicit imports needed for these. |

No CultureInfo, Regex, Avalonia control/media/threading or AvaloniaEdit rendering/folding imports are needed by NoteAnalysis. PadView still needs `System.Globalization` for :757, `System.Text.RegularExpressions` for :618/:795, and its existing Avalonia/UI-editor imports for the retained control body. Keep every other existing PadView import in this task, including any pre-existing redundant import; do not turn the move into import cleanup. Same-namespace NoteStructure and MathNotes need no using; ListEditing is reached only through MathNotes, not directly from NoteAnalysis.

| Analysis dependencies | Current members / reference sites |
|---|---|
| AvaloniaEdit document model | Invalidate :907-914; Update :938; ReadLines :1031-1044; plus moved range helper :493-499. |
| Model mode detection | Modes.Detect :956 and detected Mode/Argument/KeywordLength :957-959; NoteMode checks throughout Update/UpdateTimers. |
| Editor structure | NoteStructure property :896; Build/Update :985/:990 and LastAnalysedLineCount. |
| Editor math helpers / Calc | MathNotes.StripListMarker/BlankLabel :1008-1009, Calculator.EvaluateInlineLine and result/error values :1009-1011, MathNotes.EvaluateMath :1019. |
| Model timers | UpdateTimers :1068-1128: TryParse, Reconcile, IsDone, Ensure; TimerKey at :1118/:1131. |
| Framework state | ConditionalWeakTable, arrays, dictionaries, HashSet, value tuples in the moved helper; no static PadView state/helper dependency. |

## Direct forbidden references and scanner meaning

Reading the complete class body, signatures and nested records yields:

| Target | Remaining direct references today | Would current scanner trip? |
|---|---|---|
| PadView, including its nested types | **0** after A42a; :1008-1019 calls MathNotes | New ExactFrom/ExactTo rule should report 0. PadView's own field of type NoteAnalysis is the allowed reverse direction. |
| `AntiNotepad.UI` | **0** | Existing Editor->UI rule already covers this class; proposed exact-source row also holds. |
| `Avalonia.*` | **0** | Requested exact bans below hold. Broad external-namespace strings do not work with this matcher. |
| `AvaloniaEdit.*` | DocumentLine and TextDocument, CWT/document signatures and method calls above; DocumentChangeEventArgs once helper moves | Document bans would fail. No direct TextEditor, TextArea, TextView, Rendering or Editing dependency. |

These are direct-reference findings, **not an executed IL count or transitive purity claim**. Timers owns a DispatcherTimer (`Model/Timers.cs:29`), and NoteStructure uses NoteColorizer.HeadingLevel and NewFolding (`Editor/Structure.cs:146/:238-246`). Those indirect dependencies do not cause a direct NoteAnalysis->Dispatcher/Control/TextView rule to fail.

Scanner evidence: `SelfTest.cs:472-501` enumerates assembly types and inspects base/interfaces, fields, properties, events, constructors, static initializers and methods. :685-702 expands array/element and generic argument/constraint types. :705-718 scans method signatures; :780-824 resolves IL type/member/field tokens and referenced-method declaring/signature types. It does not recursively scan callees as part of the caller, and :721-724 goes directly to IL bytes without collecting method-body local-variable types. Thus a bare unused import or optimized-away local is no proof of a guard. ExactTo matches exact/nested types, not all subclasses; namespace matching at :679-683 understands only `AntiNotepad.<segment>`.

## Callers and accessibility

Whole-product `*.cs` searches found only PadView and SelfTest using NoteAnalysis; no other ChangedLineRange or PadView.TimerKey caller.

| Caller / site | Required change |
|---|---|
| PadView field :38, Reset :272/:426, Invalidate :460, Update :517; state reads :518-522/:537-539/:558/:656/:666/:679 | None. Same namespace/type identity and instance lifecycle. |
| PadView.OnDocumentChanged :459 | Call `NoteAnalysis.ChangedLineRange(doc, e)`; make moved helper `internal static`. Leaving it private in its new owner would fail compilation. |
| PadView.TimerKey shim :540 | Remove. Its only callers are the two tests below. |
| SelfTest :1018-1019 | Replace `Editor.PadView.TimerKey` with `Editor.NoteAnalysis.TimerKey`; key fixtures and expected values unchanged. |
| SelfTest Equivalent :1576, Full :1607/:1609-1610, analysis :1631-1640, math :1680-1684, timers :1688-1693, burst :1701-1716 | No rename or visibility change. Already addresses `Editor.NoteAnalysis` directly. |
| NoteAnalysis's own private helpers/records and TimerKey :1118 | No change. Moving a top-level class between files changes no private-member boundary inside it. |

No `file`-local types are involved. Public members of an internal class remain effectively assembly-internal; do not widen the class. TimerKey stays `internal static`; ChangedLineRange should not become public or a PadView forwarding shim.

## Per-keystroke / A29 path

1. `PadView.OnDocumentChanged` :455-460 ignores bind-time changes, takes the live document, computes the zero-based inclusive neighborhood from the post-change offset and insertion length (:493-499), then invalidates before scheduling. The helper includes a preceding/following line with clamping; preserve this arithmetic exactly.
2. `NoteAnalysis.Invalidate` :907-914 checks document identity and removes CWT entries keyed by affected DocumentLine objects. TextDocument/DocumentLine APIs remain one-based; analysis ranges remain zero-based.
3. PadView :461-486 tracks observed count. For **>1500** lines it unions pending ranges, counts changes, marks full fallback for a subsequent line-count change, and restarts the **140 ms** timer (:491). For shorter notes it stops/clears pending work and analyses immediately, forcing full work if earlier pending state or line-count changes require it. The tick :148-155 snapshots pending values, clears them and calls Analyse. All stays in PadView.
4. `Analyse` :510-531 passes Settings.InlineMath as a bool, note ID and the existing Timers instance to Update, then publishes mode/language/structure, folds, hint/highlighting and redraw. No scheduling, settings object, renderer, or folding ownership moves.
5. `NoteAnalysis.Update` :938-980 resets for a different document, ReadLines :1031-1045 reuses cached strings by DocumentLine identity, detects the first-line mode, validates the changed range/delta and chooses full fallback. Structure.Build/Update :982-992 precedes inline work. Plain inline math :994-1014 clears or ShiftInlineLines (:1048-1066), recomputes the changed band with MathNotes/Calculator, and retains one-based result keys. Math mode deliberately runs whole-note MathNotes.EvaluateMath (:1016-1021). UpdateTimers :1023/:1068-1128 preserves cached prefix/suffix timer parsing, document-order duplicate ordinals, one-based keys, Ensure/IsDone/Reconcile, and work counts. Previous state is committed at :1025-1028.

Only file location plus the range-helper owner/accessibility and call qualification change. No new wrapper, delegate, allocation, task, lock, RunUpdate boundary, document mutation or analysis call is introduced. ReadLines still visits/allocates an array for all lines; timers may scan all identities and math remains a full pass. Counters bound selected work, not total CPU.

Potential silent changes to exclude during implementation:

- No explicit static initializer/static data exists in NoteAnalysis; moving TimerKey does not move state. PadView's removed shim has no state. Keep instance field initialization and Reset ordering unchanged.
- Namespace, class/nested owners and signatures stay identical: `AntiNotepad.Editor.NoteAnalysis+CachedLine` and `+TimerLine` already have those names. Never nest NoteAnalysis inside PadView or change it to static/partial as part of this task.
- CWT object lifetime, DocumentLine key identity, `_document` reference comparison, cached strings and per-pane `_analysis` identity must remain unchanged. File paths are not CWT identity.
- Recompilation can reorder metadata tokens/debug paths and can yield normal JIT/layout/inlining variation; source inspection cannot prove cycle-identical timing. There is no algorithmic reason here for a typing-latency change. Removing a test-only shim does not affect the production typing path.
- Keep Documents/LRU, Bind/Release resets, undo limit/history, `_binding`, debounce/full fallback and command update groups unchanged. Do not fix live InlineMath/Folding refresh limitations incidentally.

## Existing tests and reflection

`SelfTest.IncrementalAnalysisChecks` is now :1561-1729, called at :99. Its current coverage is:

| Lines | Capability preserved unchanged |
|---|---|
| :1569-1604 | Eval result/error/assignment/token signatures; mode/language/keyword, Lines, fence/indent/InFence, math, inline keys, timer keys and fold equivalence. |
| :1607-1643 | Fresh force-full baseline vs incremental Invalidate/Update; initial line-copy count and inline result. |
| :1646-1677 | Fence body/opening/language/open/close, heading insertion/deletion, list-run indentation. |
| :1679-1685 | Variable edit affects later two-pass math result (36). |
| :1687-1697 | Timer duration edit and duplicate ordinals stay document ordered. Timers instance is null here. |
| :1699-1717 | Two invalidated same-count edits in a 5,000-line document, union update, `LastCopiedLineCount <= 8 && LastAnalysedLineCount < 32`, full equivalence. |

These A29 tests need no source change and are expected to pass; they do **not** invoke PadView.OnDocumentChanged, ChangedLineRange or its DispatcherTimer, nor prove GUI latency/undo isolation or live timer reconciliation. Only the two existing TimerKey test call sites need rewiring. Existing MATH NOTES and LIST EDITING checks should remain unchanged.

No reflection-by-name lookup for NoteAnalysis/CachedLine/TimerLine/ChangedLineRange or PadView nested types was found in product sources. Reflection in SelfTest is the general layer/settings scanner and opcode-map machinery (:466-583/:685-834). Existing rows naming PadView are target bans for HotkeyRegistrar, ClipboardCapturePolicy, MathNotes and ListEditing (:404/:414/:429/:441); none assumes analysis is nested there.

The IL scanner **is nested-type aware**, independent of source file: MatchesSource/MatchesRule :566-576 use IsExactOrNested :579-583, which walks DeclaringType. A NoteAnalysis ExactFrom rule includes both private records and generated nested implementation types. Assembly.GetTypes includes them and their generated signatures/methods are scanned. Moving files preserves this behavior and their full names; no special relocation test is justified.

## Proposed A42b-note-analysis-file LAYERS rows

Literal additions to the existing table, matching the A42a style (`SelfTest.cs:409-439`). **Every row below holds today by direct source inspection**, including the PadView row; they are proposed additions, not executed output. Adding the range helper preserves every row. Sixteen additions give 72 table rules and 73 LAYERS lines including the settings-setter summary, if no other changes land.

```csharp
new("NoteAnalysis", "PadView", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Editor.PadView)),
new("NoteAnalysis", "UI", ExactFrom: typeof(Editor.NoteAnalysis)),
new("NoteAnalysis", "Platform", ExactFrom: typeof(Editor.NoteAnalysis)),
new("NoteAnalysis", "Motion", ExactFrom: typeof(Editor.NoteAnalysis)),
new("NoteAnalysis", "Theme", ExactFrom: typeof(Editor.NoteAnalysis)),
new("NoteAnalysis", "PadWindow", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(PadWindow)),
new("NoteAnalysis", "Program", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Program)),
new("NoteAnalysis", "Native", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Native)),
new("NoteAnalysis", "Window", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Avalonia.Controls.Window)),
new("NoteAnalysis", "Control", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Avalonia.Controls.Control)),
new("NoteAnalysis", "Dispatcher", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Avalonia.Threading.Dispatcher)),
new("NoteAnalysis", "Settings", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Settings)),
new("NoteAnalysis", "SettingsService", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(SettingsService)),
new("NoteAnalysis", "TextEditor", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(AvaloniaEdit.TextEditor)),
new("NoteAnalysis", "TextArea", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(AvaloniaEdit.Editing.TextArea)),
new("NoteAnalysis", "TextView", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(AvaloniaEdit.Rendering.TextView)),
```

Do **not** prohibit Timers or TextDocument: both are required direct dependencies today and after the move. DocumentLine and the moved DocumentChangeEventArgs likewise remain allowed. Do not add `"Avalonia.*"` or `"AvaloniaEdit.*"` namespace strings to this schema; they would silently fail to express the intended ban. Exact type rows are useful but do not establish broad external-namespace purity.

## Measured size and manager gates

An in-memory line-array projection (no product file written) measured the following exact layout:

| File | Calculation | Expected lines |
|---|---|---:|
| PadView.cs | 1132 - 257 (:876 blank plus :877-1132) - 9 (:493-500 helper plus :501 blank) - 1 (:540 shim) - 1 (:3 CWT import) | **864** |
| NoteAnalysis.cs | 7 header lines (four imports, blank, namespace, blank) + 256 original summary/class lines + 9 helper lines (eight plus separator) | **272** |

Projection inserts the helper immediately inside the class, before CachedLine. Whitespace placement alone can shift these counts; with the exact layout above they are measured, not the old ~855-875 estimate. Total is 1136, four lines more than the original combined file. Keeping the unused CWT import gives PadView 865 instead. No runtime result depends on these counts.

Future writable implementation / manager gate sequence:

1. Cut the original summary/class verbatim to the new same-namespace file with the four imports. Move ChangedLineRange, change only private->internal, qualify its PadView call. Delete the shim, re-point SelfTest :1018-1019, remove the CWT import. Add the sixteen rows. No scheduler/command/cache changes.
2. Review the move ignoring file location: original class text identical apart from inserted helper; helper body byte-for-byte equivalent. Confirm no old shim callers, no class in PadView, and no accidental state/visibility/namespace change. Do not require new generic test machinery.
3. Manager builds the approved isolated dev/check output per dev-loop instructions with `AVALONIA_TELEMETRY_OPTOUT=1`: **0 warnings, 0 errors**. Run its `--selftest`: **SELFTEST PASS**, existing A29 bounds/equivalence and MATH NOTES/LIST EDITING intact, new NoteAnalysis rows 0 refs, scanner error-free. Record the exact LAYERS/MATH NOTES/LIST EDITING output in the implementation evidence.
4. In the future writable implementation only, prove the new NoteAnalysis->PadView rule: temporarily add a private field `private PadView? _layerProbe = null;` to NoteAnalysis (a real reflected field type, not merely an import). Build/run selftest, require nonzero failure naming `NoteAnalysis->PadView` and `AntiNotepad.Editor.NoteAnalysis -> AntiNotepad.Editor.PadView`. A negative-test compiler warning is acceptable for this temporary probe only. Revert the probe, rebuild at 0 warnings, rerun passing selftest and verify no probe remains. The current audit did not perform this mutation.
5. User checks the **dev copy**: switch notes and undo independently; edit first-line modes, fences, headings, lists and inline/math results; timer durations/duplicate ordinals and existing timer state; type/pause/insert/delete on both sides of the 1500-line threshold and in a 5,000+ line note (including multiple line-count changes before the pause). Check Enter/Tab and one-step undo remain unchanged. Never target the installed copy. Manager obtains the hand-test gate before committing/merging through the established flow.

## Recommendation and later gate

Proceed with A42b-note-analysis-file unchanged in scope, with the explicit `internal static` range-helper adjustment, minimal imports and targeted guards above. Drop nothing required by the brief; add no behavior fix, document-cache extraction, scheduler extraction or purity rewrite. A42a already achieved the dependency removal; A42b's value is file ownership, reduced control-file size and preventing that reverse dependency from returning.

For a verbatim move, typing-latency and undo/analysis risks are essentially absent at the algorithm level. The practical risks are an imperfect cut/paste, changed range arithmetic/order, an unintended constructor/reset/CWT identity change, or scope creep into scheduling and commands. Build/selftest address source compatibility and capability; live responsiveness, scheduler behavior, actual reconciliation and GUI undo remain unverified here.

After the move, about 864 lines remain in PadView: control construction/look/margins, document caching/binding/release, event scheduling/analysis publication, result/timer rendering facades, menu/hotkey glue, checkbox/list document mutations, folding and highlighting. It is reasonable to stop at this size.

| Later optional task | Gate should weigh |
|---|---|
| A42c-checkbox-indent | Concrete headless value for checkbox variants and two-space indent/outdent vs extra planner/adapter/Box-regex ownership surface. Current ToggleCheckbox :603, Indent :716 and caller-owned update group :649 should retain minimal mutations, caret and single undo. Decide from useful capability checks, not a smaller LOC target. |
| A42d-list-commands | Highest risk/value tradeoff: current IsListLine :710, Renumber :734, ContinueList :762 and RunUpdate :642/:649 coordinate live caret/offsets and post-edit renumbering. Require block-local work, checked/empty/nested cases, 9->10 digit-width changes, end/mid-line insertion, one-step undo and long-list response. Do it only if that testable contract justifies added machinery. |

Keep ChipText/FoldSelection deferred. A42b does not create a new reason to touch draw/timer sampling or the small fold selector. The user decision after A42b remains the stopping gate.
