# A42-padview-commands-impact

Read-only impact analysis, 2026-10-25. Branch verified: `reorg/A42-padview-commands`; initial `git status --short` was empty. Product: `product/AntiNotepad`. References below are relative to that directory unless prefixed with `docs/`.

- Read `AGENTS.md`, `.coord/STATUS.md`, `docs/agents.md`, and `docs/decisions.md`. The task's stricter write boundary overrides the general STATUS update instruction: only this report is written.
- No build, selftest, application launch, termination, or git mutation performed. Proposed test results below are source-derived, not execution evidence.
- PadView.cs has 1,189 lines: PadView ends at 932; NoteAnalysis occupies 934–1189. There are no other top-level helpers after NoteAnalysis. A39-motion-out-of-ui is assumed to land first. Existing menu/motion UI references are not A42 work.
- Recommendation: extract MathNotes and ListEditing pure helpers; move NoteAnalysis intact; use a document-command adapter for list mutations. Keep scheduling, visual adaptation, note binding, cache ownership, and timer actions in PadView. Do not equate window-free with dependency-free: NoteAnalysis takes TextDocument and Timers; Timers owns a DispatcherTimer (`Model/Timers.cs:29`).

## Inventory conventions and caller evidence

Classification describes current responsibility; mixed commands are classified by their dominant boundary and split below. `pure text logic` includes stateless numeric decisions where the four requested classes provide no narrower category. Hot means reachable during a typing/edit gesture or its redraw/debounce; `yes (Enter/Tab)` is conditional, not every character. State columns list reads and writes, not just writes. `stay` means PadView.

Caller abbreviations (whole-product `rg` searched both static and instance references):

| Key | Caller locations |
|---|---|
| ctor | PadView.cs:80–160 event subscriptions and visual setup |
| PV | PadView internal call sites, named in rows |
| WIN | UI/PadWindow.Navigation.cs:23–33,55–254,352–398; UI/PadWindow.cs:144,248–249,305,365–414; UI/PadWindow.Keys.cs:39,93,105,114,255 |
| LAYOUT | UI/PadWindow.Layout.cs:111–129 constructs/subscribes; :165 uses Mode/CodeLanguage |
| BLUR | UI/PadWindow.TopBlur.cs:157–180,241–288 subscribes Edited, observes Editor, applies backdrop mask |
| PREVIEW | UI/MotionPreview.cs:110–118,139,169–204 creates Private pane, binds, scrolls, focuses, releases, applies look |
| RENDER | Editor/Renderers.cs:47–155 PaperLayer, :157–333 ResultRenderer (ChipFor at :261), :335–364 DividerRenderer, :366 onward CheckboxRenderer |
| COLOR | Editor/Colorizers.cs:43–104 NoteColorizer; :184–187 MathColorizer |
| ELEM | Editor/Elements.cs:44–84 CheckboxGenerator (ToggleCheckbox at :74); :86–107 BulletGenerator; :109–165 LinkGenerator; :168–184 MarkerGenerator |
| STRUCT | Editor/Structure.cs:394–447 FenceColorizer; :450–482 IndentGuideRenderer |
| COMP | Editor/Completion.cs:53–78 OnTextEntered; :82–157 popup/items; :160–192 transforms, caret, toast |
| MOTION | Editor/Motion.cs:226–314 SmoothCaret uses Editor/Settings/Palette |
| GUTTER | Editor/ChevronGutter.cs:60–68 takes PadView; :98 reads Palette |
| TEST | SelfTest.cs:44–45 math helpers; :234 list number; :263–264 TimerKey; :821–962 NoteAnalysis equivalence/work counters |

Public members with no external caller are marked explicitly. Field “callers” means their readers/writers. Auto-property accessors and compiler-generated backing fields are included in their property row; positional-record generated members are identified separately below. Local functions/lambdas are accounted for with their enclosing member.

## PadView member classification

| Member | Line | Class | Callers / readers-writers | State touched | Hot | Proposed destination |
|---|---:|---|---|---|---|---|
| PadView type | 29 | control/glue | LAYOUT, PREVIEW; all editor consumers above | Grid and pane lifecycle | yes | stay |
| Documents | 31 | control/glue | DocumentFor, Forget, Evict, CachedDocuments | static document/undo ownership | no | stay; optional later DocumentCache |
| ListPrefix | 32 | pure text logic | StripListMarker, IsListLine, TryListNumber(string), Renumber, ContinueList | compiled regex | yes | ListEditing.Prefix |
| _timers | 34 | control/glue | ctor, Analyse, ChipFor, timer/pointer commands, Release | shared Timers | yes | stay |
| _timerTick | 35 | control/glue | ctor, Release | subscription delegate | no (tick) | stay |
| _hint | 36 | control/glue | ctor, ClearBackgrounds indirectly, ApplyLook, Bind, Analyse, ApplyMargins | TextBlock | yes | stay |
| _results | 37 | control/glue | ctor, OnViewPressed, OnViewMoved | rendered hit regions | no (pointer) | stay |
| _caret | 38 | control/glue | ctor, ApplyLook, Release | SmoothCaret lifecycle | yes | stay |
| _analysis | 39 | analysis | Bind, Release, OnDocumentChanged, Analyse, accessors, timer commands | per-pane analysis | yes | field stays; type moves |
| _folding | 40 | control/glue | Bind, UpdateFolds, FoldAtCaret | FoldingManager | yes | stay |
| _paper | 41 | control/glue | ctor scroll/line callbacks, ApplyLook | PaperLayer | yes | stay |
| _gutter | 42 | control/glue | ctor pointer callbacks, Bind, ApplyMargins | ChevronGutter | yes | stay |
| _fences | 43 | control/glue | ctor, Bind, Analyse | colorizer/cache | yes | stay |
| _edgeFadeMask | 44 | control/glue | UpdateEdgeFade, ClearEdgeFade | immutable brush cache | yes (layout) | stay |
| _edgeFadeKey | 45 | control/glue | UpdateEdgeFade, ClearEdgeFade | mask geometry cache | yes (layout) | stay |
| _topBackdropOffset | 46 | control/glue | SetTopBackdropMask, UpdateEdgeFade | visual mask position | yes (layout) | stay |
| _topBackdropOpacity | 47 | control/glue | SetTopBackdropMask, UpdateEdgeFade | visual mask opacity | yes (layout) | stay |
| Structure | 48 | analysis | Analyse writes; InFence, UpdateFolds; RENDER, STRUCT, COMP | published NoteStructure | yes | stay as facade |
| _binding | 49 | control/glue | Bind, OnDocumentChanged | event suppression | yes | stay |
| _loadHint | 50 | control/glue | Bind, Analyse | storage failure/empty hint | yes | stay |
| _loadHintError | 51 | control/glue | Bind, ApplyLook | hint palette selection | no | stay |
| _arrive | 52 | control/glue | Arrive | animation handle | no | stay after A39 |
| _bindToken | 53 | control/glue | Bind posted callback | stale-scroll cancellation | no | stay |
| _lastCaretLine | 54 | control/glue | OnCaretMoved | redraw suppression | yes | stay |
| _observedLineCount | 55 | analysis | Bind, OnDocumentChanged | previous event line count | yes | stay with scheduler |
| _pendingFirstChanged | 56 | analysis | OnDocumentChanged, ClearPendingAnalysis, ctor tick | debounce union | yes | stay |
| _pendingLastChanged | 57 | analysis | same as above | debounce union | yes | stay |
| _pendingChangeCount | 58 | analysis | OnDocumentChanged, ClearPendingAnalysis | debounce event count | yes | stay |
| _pendingFullAnalysis | 59 | analysis | OnDocumentChanged, ClearPendingAnalysis, ctor tick | conservative fallback | yes | stay |
| Editor | 61 | control/glue | almost all PV; WIN, BLUR, PREVIEW, COMP, MOTION, RENDER | TextEditor/document/caret | yes | stay |
| Note | 62 | control/glue | Bind, Flush, Analyse, timer commands; WIN, PREVIEW | bound model | yes | stay |
| Palette | 63 | control/glue | ctor/ApplyLook write; visuals, COMP, MOTION, GUTTER | theme | yes | stay |
| Settings | 64 | control/glue | ctor/ApplyLook write; Analyse, layout, menu, caret, renderers/elements/STRUCT | shared settings | yes | stay; pass scalar flags to logic |
| Mode | 65 | analysis | Analyse writes; key/list/timer/highlight/chip logic, COLOR, ELEM, STRUCT, COMP, RENDER, LAYOUT | published mode | yes | stay facade |
| CodeLanguage | 66 | analysis | Analyse, ApplyCodeHighlighting; LAYOUT | published argument | yes | stay facade |
| ModeKeywordLength | 67 | analysis | Analyse writes; COLOR, ELEM, COMP.Transform | mode prefix length | yes | stay facade |
| Decorate | 68 | control/glue | ELEM, RENDER | Mode | yes | stay |
| CaretLine | 69 | control/glue | keys/list/pointer/caret commands; ELEM, STRUCT | editor caret | yes | stay |
| Edited | 71 | control/glue | OnDocumentChanged invokes; LAYOUT, BLUR subscribe | window/save/blur notification | yes | stay |
| ModeChanged | 72 | control/glue | Analyse invokes; LAYOUT subscribes | chrome notification | yes | stay |
| Toast | 73 | control/glue | OnViewPressed, StartTimerOnLine, RaiseToast invoke; LAYOUT subscribes | message event | no (commands) | stay |
| FocusedPane | 74 | control/glue | ctor GotFocus invokes; LAYOUT subscribes | active-pane notification | no | stay |
| Copy | 76 | control/glue | OnViewPressed invokes; LAYOUT subscribes | clipboard routing | no | stay |
| AppMenu | 78 | control/glue | LAYOUT assigns; ShowMenu invokes | menu factory | no | A39 contract; keep out of A42 |
| PadView constructor | 80 | control/glue | LAYOUT:113, PREVIEW:110 | editor, renderers, callbacks, _analyse/_timers | no; callbacks hot | stay |
| ClearBackgrounds | 163 | control/glue | ctor Loaded/TemplateApplied callbacks | visual descendants | no | stay |
| ApplyLook | 175 | control/glue | ctor; WIN Keys:105,255; PREVIEW:204 | Palette, Settings, editor, _hint, _caret, layout | no | stay |
| Recent | 198 | control/glue | DocumentFor, Forget, Evict | static LRU ids | no | stay |
| Owners | 199 | control/glue | DocumentFor, Forget, Evict | static model ownership | no | stay |
| DocumentFor | 201 | control/glue | Bind; WIN Navigation:198–199,310,383 | Documents/Owners/Recent, Note.Attach, TextChanged callback | no; callback yes | stay |
| IsBlank | 218 | document command | DocumentFor TextChanged lambda | doc text/length (256 threshold) | yes | stay with cache |
| FirstLine | 220 | document command | same callback | first DocumentLine | yes | stay with cache |
| Forget | 222 | control/glue | WIN Navigation:45,136,205; UI/SearchOverlay.cs:362 | all static caches, Note.Detach | no | stay |
| Evict | 232 | control/glue | WIN PadWindow.cs:371,391,408 | caches, Dirty/onScreen guard, Note.Detach | no | stay |
| CachedDocuments | 247 | control/glue | WIN PadWindow.cs:414, Keys:39 | Documents.Count | no | stay |
| _isPrivate | 253 | control/glue | Private accessors | preview isolation | no | stay |
| Private | 254 | control/glue | PREVIEW:110 init; Bind, ApplyMargins/UpdateTopInset; RENDER:122 | _isPrivate, top inset | no | stay |
| _private | 263 | control/glue | Bind, Release | preview Note -> TextDocument cache | no | stay |
| Bind | 265 | control/glue | WIN Navigation:23,28; PREVIEW:117,197 | note/cache, guards, subscriptions, analysis, folding, caret/scroll | no | stay |
| UpdateEdgeFade | 312 | control/glue | ctor layout/scroll events; margins, backdrop setter | Settings, bounds, brush cache | yes (layout) | stay |
| EdgeFadeVaries | 389 | pure text logic | UpdateEdgeFade | none; numeric input | yes (layout) | stay: visual math, not text scope |
| EdgeFadeAlphaAt | 393 | pure text logic | UpdateEdgeFade | none; numeric input | yes (layout) | stay: visual math, not text scope |
| SetTopBackdropMask | 403 | control/glue | BLUR:272–288 | offsets/opacity, fade cache | yes (layout) | stay |
| ClearEdgeFade | 412 | control/glue | UpdateEdgeFade, Release | brush/key/editor mask | no (conditional layout) | stay |
| Release | 420 | control/glue | PREVIEW:188 | unsubscribe document/timer, stop analysis/caret, clear private | no | stay |
| Flush | 433 | control/glue | Bind; WIN locations above | Note.Caret/Scroll from editor | no | stay |
| Arrive | 441 | control/glue | WIN Navigation:33,175; PREVIEW:118 | _arrive, RenderTransform, Opacity | no | stay after A39 |
| FocusText | 453 | control/glue | WIN PadWindow.cs:305, Navigation:398; PREVIEW:181 | editor focus | no | stay |
| OnDocumentChanged | 455 | control/glue | doc.Changed bound/unbound by Bind/Release | _binding, analysis invalidation, pending fields, timer, Edited | yes | stay, unchanged scheduler |
| _analyse | 491 | control/glue | ctor tick; Bind, Release, OnDocumentChanged | DispatcherTimer 140 ms | yes | stay |
| ChangedLineRange | 493 | analysis | OnDocumentChanged | document/e bounds, no mutation | yes | NoteAnalysis.ChangedLineRange |
| ClearPendingAnalysis | 502 | analysis | ctor tick, Bind, Release, OnDocumentChanged | four pending fields | yes | stay |
| Analyse() | 510 | control/glue | Bind | full-range request | no | stay |
| Analyse(int,int,bool) | 512 | control/glue | overload, OnDocumentChanged, ctor tick | Settings.InlineMath, Note.Id, _timers, analysis, published props, fences/folds/hint, events | yes | stay |
| EvaluateMath | 538 | pure text logic | NoteAnalysis.Update:1076; no external direct callers today | calculator context local to each pass | yes (Math) | MathNotes.EvaluateMath |
| BlankLabel | 568 | pure text logic | EvaluateMath; NoteAnalysis.Update:1066; TEST:44 | regex only | yes | MathNotes.BlankLabel |
| StripListMarker | 575 | pure text logic | EvaluateMath; NoteAnalysis.Update:1065; TEST:45 | ListPrefix only | yes | MathNotes.StripListMarker -> ListEditing prefix |
| Redraw | 581 | control/glue | ApplyLook, Analyse, OnCaretMoved | TextView | yes | stay |
| InFence | 583 | analysis | COLOR:52, ELEM:179, RENDER:349 | Structure.InFence | yes | stay facade |
| MathLine | 584 | analysis | ChipFor; COLOR:185 | _analysis.MathLines | yes | stay facade |
| InlineLine | 585 | analysis | InlineResult, ChipFor; no external direct caller | _analysis.InlineLines | yes | stay facade |
| InlineResult | 586 | analysis | ChipFor; COLOR:104 | InlineLine | yes | stay facade |
| TimerKey | 587 | pure text logic | TEST:263–264 only | forwards to NoteAnalysis | no | remove facade, TEST -> NoteAnalysis.TimerKey |
| ChipFor | 590 | control/glue | RENDER:261 | Mode, math/inline caches, Palette, TimerKeys, live Timers | yes (draw/tick) | stay palette adapter + ChipText decision |
| OnViewPressed | 618 | control/glue | ctor pointer handler | result hit regions, Timers, Copy/Toast, redraw | no | stay |
| OnViewMoved | 634 | control/glue | ctor pointer handler | hit regions, cursor/tooltip | no | stay |
| OnCaretMoved | 643 | control/glue | ctor caret subscription | _lastCaretLine, Settings, redraw | yes | stay |
| ToggleCheckbox | 650 | document command | OnKeyDown:683; ELEM:74 | Document mutation; CheckboxGenerator.Box | yes (Ctrl+Enter) | thin stay wrapper -> DocumentCommands.ToggleCheckbox/ListEditing |
| OnKeyDown | 671 | control/glue | ctor tunnel key handler | selection, Completion.IsOpen, modifiers, command dispatch, RunUpdate | yes | stay |
| ToggleTimerAtCaret | 700 | control/glue | OnKeyDown:683 | document text, TimerKeys, Timers, selection-layer invalidation | yes (Ctrl+Enter) | stay; no pure timer state machine needed |
| ResetDoneTimer | 710 | control/glue | OnViewPressed, ToggleTimerAtCaret | Timers.ClearDone/Ensure, TimerKeys, document, Note.Id | no (commands) | stay |
| StartTimerOnLine | 722 | control/glue | OnKeyDown:691 | Timers.Ensure/Start, TimerKeys, document, Toast | yes (Enter) | stay |
| ShowMenu | 734 | control/glue | ctor ContextRequested | Editor selection/undo, AppMenu, Palette/Settings | no | A39 inverted contract; stay glue |
| IsListLine | 757 | document command | OnKeyDown:693 | document line, regex/number parser | yes (Tab) | DocumentCommands.IsListLine -> ListEditing.IsListLine |
| TryListNumber(string,out int) | 763 | pure text logic | TEST:234 only | ListPrefix | no (TEST today) | ListEditing.TryListNumber |
| TryListNumber(Match,out int) | 765 | pure text logic | string overload, IsListLine, Renumber, ContinueList | regex groups, invariant parsing | yes (Enter/Tab) | private ListEditing overload |
| Indent | 773 | document command | OnKeyDown:696 | document line prefix | yes (Tab) | DocumentCommands.Indent + ListEditing.IndentEdit |
| Renumber | 791 | document command | OnKeyDown:689,696; ContinueList:841 | list-block scan, counts, per-number doc.Replace | yes (Enter/Tab) | DocumentCommands.Renumber + ListEditing.RenumberEdits |
| ContinueList | 819 | document command | OnKeyDown:689 | Mode, document/caret, prefix, Insert, nested Renumber | yes (Enter) | DocumentCommands.ContinueList + ListEditing.ContinueEdit |
| Insert | 859 | control/glue | ContinueList:830,855 | document.Insert at caret; BringCaretToView | yes (Enter) | insertion decision moves; scroll stays |
| UpdateFolds | 865 | control/glue | Analyse:525 | Structure.Folds, doc, _folding | yes | stay |
| StyleFoldingMargin | 873 | control/glue | Bind:296 | Palette, static FoldingElementGenerator.TextBrush, margins | no | stay |
| ApplyMargins | 885 | control/glue | ctor SizeChanged, ApplyLook, StyleFoldingMargin | Settings, font metrics/bounds, Private, gutter, hint/insets/mask | yes (layout) | stay |
| UpdateTopInset | 904 | control/glue | Private init, ApplyMargins | Settings.Bars, Private, specialized TextArea | no (layout) | stay |
| FoldAtCaret | 909 | document command | OnKeyDown:677 only; no external caller | doc line bounds, FoldingManager sections/IsFolded | yes (shortcut) | stay wrapper + FoldSelection.TargetIndex |
| ApplyCodeHighlighting | 920 | control/glue | ApplyLook, Analyse | Mode/CodeLanguage/Palette; CodeColors, editor highlight | yes (mode change) | stay |
| OnTextEntered | 929 | control/glue | ctor TextEntered | Completion popup/commands | yes | stay |
| RaiseToast | 931 | control/glue | COMP:184,191 | Toast event | no (commands) | stay |

## NoteAnalysis member classification (same file today)

All rows move to `Editor/NoteAnalysis.cs` with the same namespace/type identity unless stated. All mutable state remains per instance; do not introduce a static analysis cache. Classification `analysis` includes its private record storage.

| Member | Line | Class | Callers / readers-writers | State touched | Hot | Proposed destination |
|---|---:|---|---|---|---|---|
| NoteAnalysis type | 935 | analysis | PadView._analysis; TEST:821–962 | per-pane/headless analysis | yes | NoteAnalysis.cs |
| CachedLine record | 937 | analysis | ReadLines | immutable cached string | yes | same |
| CachedLine.Text (positional) | 937 | analysis | ReadLines:1100 | cached text | yes | same |
| TimerLine record | 938 | analysis | UpdateTimers | parsed timer snapshot | yes (Timer) | same |
| TimerLine.Duration (positional) | 938 | analysis | UpdateTimers Ensure | duration value | yes | same |
| TimerLine.Label (positional) | 938 | analysis | UpdateTimers Ensure | label value | yes | same |
| TimerLine.DurationText (positional) | 938 | analysis | UpdateTimers ordinal/key construction | identity text | yes | same |
| _lineText | 940 | analysis | Invalidate, Reset, ReadLines | ConditionalWeakTable<DocumentLine,CachedLine> | yes | same |
| _document | 941 | analysis | Invalidate, Reset, Update | identity guard | yes | same |
| _previousLineCount | 942 | analysis | Reset, Update | prior analyzed count | yes | same |
| _hasProcessed | 943 | analysis | Reset, Update | full/incremental eligibility | yes | same |
| _previousInlineMath | 944 | analysis | Reset, Update | setting-change detection | yes | same |
| _timerLines | 945 | analysis | Reset, UpdateTimers | parsed line-aligned snapshots | yes | same |
| _inlineLines | 946 | analysis | Reset, Update, ShiftInlineLines, InlineLines | 1-based results | yes | same |
| _timerKeys | 947 | analysis | Reset, UpdateTimers, TimerKeys | 1-based timer identities | yes | same |
| _mathLines | 948 | analysis | Reset, Update, MathLines | zero-based results including mode line | yes | same |
| Mode | 950 | analysis | Reset/Update write; PadView.Analyse, TEST | Modes.Detect result | yes | same |
| CodeLanguage | 951 | analysis | Reset/Update write; PadView.Analyse, TEST | mode argument | yes | same |
| ModeKeywordLength | 952 | analysis | Reset/Update write; PadView.Analyse, TEST | detected prefix | yes | same |
| Structure | 953 | analysis | Reset/Update write; PadView.Analyse, TEST | NoteStructure | yes | same |
| Lines | 954 | analysis | Reset/Update write; ReadLines consumers, shifts/timers, PadView.Analyse, TEST | string[] snapshot | yes | same |
| MathLines | 955 | analysis | PadView.MathLine, TEST | _mathLines | yes | same |
| InlineLines | 956 | analysis | PadView.InlineLine, TEST | _inlineLines | yes | same |
| TimerKeys | 957 | analysis | PadView.ChipFor/timer commands, TEST | _timerKeys | yes | same |
| LastCopiedLineCount | 959 | analysis | Reset/Update/ReadLines write; TEST:878,959–960 | GetText miss count | yes | same |
| LastAnalysedLineCount | 961 | analysis | Reset/Update write; TEST:959–960 | structure+inline+timer+math work | yes | same |
| Invalidate | 964 | analysis | PadView.OnDocumentChanged; TEST Refresh/burst | remove changed DocumentLine cache entries | yes | same |
| Reset | 975 | analysis | Update new doc; PadView.Bind/Release | all analysis state | no (bind/reset) | same |
| Update | 995 | analysis | PadView.Analyse; TEST Full/Refresh/initializations | document snapshot, mode/structure, all derived caches, Timers reconcile | yes | same; MathNotes calls replace PadView calls |
| ReadLines | 1088 | analysis | Update:1010 | CWT misses, Lines construction, copy counter | yes | same |
| ShiftInlineLines | 1105 | analysis | Update:1056 | removes changed range, shifts surviving keys | yes | same |
| UpdateTimers | 1125 | analysis | Update:1080 | timer-array shifting/reparse, keys, Timers.Ensure/Reconcile | yes (all modes reconcile; parse Timer only) | same |
| TimerKey | 1188 | pure text logic | UpdateTimers:1175, PadView facade:587 -> TEST | none | yes (Timer) | keep NoteAnalysis.TimerKey |

Record constructors/equality/deconstruction are compiler-generated from the record rows, not additional handwritten behavior. PadView constructor lambdas include the debounce tick (:148–156), timer selection invalidation (:157), focus/menu/pointer/layout callbacks (:127–147). DocumentFor owns the significant TextChanged lambda (:209). Bind owns the token-checked posted scroll callback (:305). Renumber's local `Text`/:794 and `InBlock`/:796 move with its adapter/block scan.

## Proposed files, exact ownership, and call-site changes

| File/type | Existing members moved or retained | New seam required |
|---|---|---|
| Editor/MathNotes.cs / MathNotes | EvaluateMath, BlankLabel, StripListMarker | StripListMarker calls ListEditing prefix matcher; no PadView/renderer dependency |
| Editor/ListEditing.cs / ListEditing | ListPrefix -> private Prefix; both TryListNumber overloads; logic from IsListLine, ContinueList, Indent, Renumber, ToggleCheckbox | Pure IsListLine(string); ContinueEdit(text,mode,caretColumn), IndentEdit(text,deeper), CheckboxEdit(text); RenumberEdits(blockLines,targetIndex,fresh). Return offset/length/replacement decisions and handled/insertion/outdent flags. No TextDocument, TextEditor, controls, palette, timers, or PadView. Preserve streaming/block-local access; do not copy entire document per Enter/Tab. |
| Editor/DocumentCommands.cs / DocumentCommands | document portions of IsListLine, Indent, Renumber, ContinueList, ToggleCheckbox | Take TextDocument plus scalar line/caret/mode inputs. Apply minimal edits; return command outcome. Retain caller-owned update group. ContinueList needs a synchronous caret-offset supplier or insertion callback so Renumber uses the actual post-insert caret line without moving TextArea into this type. |
| Editor/NoteAnalysis.cs / NoteAnalysis | Entire existing class including CachedLine/TimerLine, fields, properties, Invalidate, Reset, Update, ReadLines, ShiftInlineLines, UpdateTimers, TimerKey; add ChangedLineRange from PadView | Keep Timers argument/reconciliation unchanged in this task. No event subscription, dispatcher scheduling, renderer, or PadView reference. |
| Editor/ChipText.cs / ChipText | text/kind/value decision portions of ChipFor | Pure value record (Text,Kind,Value,ColorRole). Given evaluated line/error, timer key, completed flag and an immutable timer snapshot (Done/Running/Paused/Remaining/Duration/Key), return descriptor. Read live Timers once in PadView; map ColorRole to Palette there. |
| Editor/FoldSelection.cs / FoldSelection | selection predicate/order from FoldAtCaret | TargetIndex((Start,End,Folded)[],lineStart,lineEnd,wantFold) -> index or -1. Preserve enumeration-order tie behavior; PadView sets target.IsFolded. |
| Editor/TextCommands.cs | Keep all current members: Marker, Done, Apply, Sort, Dedupe, ClearDone, Uncheck, AsList, Tidy, Plain, SelfTest | No need to merge distinct grammars or whole-note transforms into live list editing. |
| Editor/PadView.cs | All rows marked stay, public command facades, key handler/update groups, caret/scroll glue, palette adapter, cache ownership | Replace bodies/calls only. Timer actions remain orchestration; no new timer framework. |

- Required actual rename sites: `NoteAnalysis.Update` :1065–1066/:1076 -> MathNotes helpers; `SelfTest.cs:44` -> MathNotes.BlankLabel, :45 -> MathNotes.StripListMarker, :234 -> ListEditing.TryListNumber, :263–264 -> NoteAnalysis.TimerKey. PadView.EvaluateMath currently has no external direct caller; its single caller is NoteAnalysis. Remove PadView.TimerKey forwarding method once tests change.
- PadView.OnDocumentChanged :459 -> NoteAnalysis.ChangedLineRange. Use internal static visibility for SelfTest access without widening public API. Existing `Editor.NoteAnalysis` test references require no rename after its file move.
- Keep CheckboxGenerator.Box compatibility by moving the regex to ListEditing and having `Elements.cs:46` alias/use it. ToggleCheckbox must not depend on a visual generator. The Box regex is a separate grammar from ListPrefix; preserve group 2 for checked character. Existing visual and command callers keep `PadView.ToggleCheckbox`.
- Renderers, Colorizers, Elements, Structure, Completion, Motion, ChevronGutter, PadWindow*, SearchOverlay, MotionPreview keep existing PadView-facing methods/properties/events. RENDER:261 still calls ChipFor and receives the same tuple; no palette handling moves into ChipText. FoldAtCaret has no external caller today.
- Optional later task only: DocumentCache extraction would take Documents, Recent, Owners, DocumentFor, IsBlank, FirstLine, Forget, Evict, CachedDocuments together, and rename WIN/SearchOverlay static accesses listed above. It is not necessary to split text logic, and expands model/undo lifetime risk.

## Entangled commands and behavior to preserve

- ContinueList (:819–857): separate pure decision from application. Code mode always declines. In List mode, unmatched first/nonblank line inserts `\n[ ] `; unmatched blank non-first line declines. Empty marked item with at least two indentation characters removes exactly two characters then renumbers; otherwise removes the whole line content and consumes Return. Number limit 999,999,999 declines continuation. Checked prefixes reset to unchecked; markers and indentation survive. Insertion occurs at caret, even mid-line: do not append at line end or replace the whole line. Keep original literal LF insertion.
- Renumber (:791–815): find only the contiguous nonblank block of list lines or lines beginning with ASCII space. Pure planner receives that block, not the whole note. Keep independent counters per regex indentation length; discard deeper counters on shallower lines; nonnumber/invalid-number items reset current level; first number is retained unless `fresh && n == target`. Apply number-only edits in original traversal order with live offsets, or descending absolute offsets if planned from one snapshot; never apply stale ascending offsets when digit widths change.
- Indent (:773–783): deeper inserts two spaces at line start; shallower removes up to two ASCII spaces only. It does not shift a selected multiline region. Keep caret movement supplied by AvaloniaEdit document anchors, and renumber after indentation using the current caret line (:696).
- ToggleCheckbox (:650–668): pure decision returns one replace/insert span. Existing box toggles only its interior character; checked `x` and `X` both become space. Otherwise a leading bullet and all its following whitespace become `[ ] `, preserving indentation; unmarked text gains `[ ] ` after leading whitespace. No code/fence guard inside this command; do not introduce one during extraction.
- FoldAtCaret (:909–917): pure target selection takes offsets and folded flags. Matching is inclusive intersection with the entire caret line, not caret position containment. Choose highest StartOffset among sections whose state differs from requested state. This can skip an inner section already in the desired state and select an outer one. FoldingManager and mutation remain in PadView.
- ChipFor (:590–615): pure descriptor selection needs timer state sampled by glue, not Timers itself. Preserve Math vs Plain result precedence over mixed-currency error, Timer line >1 guard, completed-key lookup before Get, done/running/paused/ready ordering, and exact kind/value strings. Palette role mapping stays local; renderer uses kind for icon and click behavior (:608; Renderers.cs:261 onward).
- ToggleTimerAtCaret/ResetDoneTimer/StartTimerOnLine (:700–730): leave stateful shell intact. Parsing text is already Timers.TryParse. ClearDone occurs before reverse-key lookup/re-Ensure; normal Enter starts timer only after list continuation declined; start toast and selection-layer invalidation belong here. A pure decision describing “reset/toggle/start” adds little unless behavior can be frozen independently; avoid introducing stale timer snapshots into actions.

## Concrete risks and guardrails

| Risk | Code spot | Required preservation / evidence |
|---|---|---|
| Undo grouping | PadView.cs:689,696 RunUpdate; :841 nested Renumber; :814 number Replace; :659–667 checkbox edits | Keep one caller-owned group around continuation+renumber and indent+renumber, including empty-item outdent. RunUpdate is the current API; no explicit BeginUpdate/EndUpdate/UndoGroup exists anywhere in current product (`rg`). Do not replace grouped number edits with Document.Text assignment. Add one headless document undo check per grouped command. |
| Other command undo conventions | Completion.cs:160–175,178–184 | Transform uses one whole-document Replace; Line restores clamped caret; PrettyJson assigns Text. These are separate existing paths, not reasons to normalize all mutations in A42. |
| Caret anchor timing | PadView.cs:689,696,830,841,855,859–862 | Renumber uses CaretLine after mutation, not the pre-command line. Insert relies on TextArea caret anchors and explicitly BringCaretToView; a headless arithmetic caret return alone does not prove identical live placement. Keep synchronous glue callback or validate actual AvaloniaEdit caret behavior later in dev copy. |
| Cache invalidation order | PadView.cs:459–460; NoteAnalysis:964–971,1088–1100 | Invalidate using post-change document lines before any pending scheduling. CWT keys are DocumentLine identity, not line number or text. Preserve first/last surrounding-line padding (:497–499); lines surviving insertion can shift without recopy. Changing Update without corresponding Invalidate risks stale text. |
| Index conventions | NoteAnalysis:1056–1068,1105–1122,1143–1177 | Changed bounds zero-based inclusive; inline/timer dictionaries one-based; math array includes first line. `oldLastChanged = lastChanged - delta` governs drops and shifts. Preserve all three conventions and timer-array prefix/suffix copies. |
| Debounce and fallback | PadView.cs:148–155,455–507 | >1500, not >=1500; restart 140 ms timer each edit; invalidate immediately. Pending range union; line-count change when pendingChangeCount >1 forces full. Below threshold any prior pending work or line-count change forces full. Tick snapshots bounds/forceFull then clears pending before Analyse. Bind/Release stop and clear. Do not relocate scheduler into NoteAnalysis. |
| Hidden O(n) work | NoteAnalysis:1088–1102,1128–1185; Structure.cs:39–73 | ReadLines still allocates an array and visits every line; work counters count text copies/signatures/etc., not total CPU. Timer keys/ordinals/reconcile are rebuilt across all timer lines; Math keeps full note passes. Counters alone cannot establish latency. Do not claim a timing improvement or add whole-doc copies for commands. |
| Static documents/undo ownership | PadView.cs:201–247,284–305; UI/PadWindow.cs:365–408 | Same TextDocument per note, undo SizeLimit 400, Note.Attach lazy text getter, TextChanged Touched callback; Evict skips dirty and on-screen notes and is called after saving. Forget/Detach must remain paired. Keep separate preview cache and unavailable-note empty document. |
| _binding and event ordering | PadView.cs:268–300,457 | Flush old note, set guard, detach old Changed, stop/reset, uninstall old folding before document swap, attach new Changed, initialize observed count/caret, clear guard, full Analyse. Private documents do not attach to shared Documents model callbacks. No async command/analysis crossing binds. |
| Regex grammar/init | PadView.cs:32,568–578,765–770; TextCommands.cs:8–9; Elements.cs:46,88 | Keep static compiled prefix once, not per call; Box likewise compiled. Moving initialization earlier/later may affect first-use cost but there is no visible dependency cycle today. Avoid static PadView forwarding properties that initialize controls. BlankLabel currently uses Regex.Match with a literal, not a compiled field; moving it must not silently change its pattern/options. |
| Internal access | PadView.cs:538,568,575,587,763; NoteAnalysis:1188; SelfTest.cs:44–45,234,263–264 | Rename tests in same change; preserve internal static accessibility. File move alone keeps NoteAnalysis tests valid. |
| Math two-pass semantics/offsets | PadView.cs:540–563; NoteAnalysis:1065–1076 | Preserve mode line, blank marker/label spans without shortening, retry whole note for later variable/context effects, merge existing successful first-pass results, retain ErrorReason/AssignedName and leading Comment token. Do not turn into independent line evaluation. Calculator.Clock (:163) means date expressions need controlled clock in deterministic tests. |
| Timers are side effects | NoteAnalysis:1133,1174–1185; Model/Timers.cs:86–119 | Label changes preserve duration-text identity; duplicates use document-order ordinal. Ensure refreshes label; Reconcile can stop/drop entries. Preserve null noteId/timers behavior and reconciliation even after leaving Timer mode. This extraction is not a claim that NoteAnalysis is pure. |
| Draw allocation | PadView.cs:590–615; Renderers.cs:261 | ChipFor runs for visible lines on redraw and timer ticks. Prefer readonly value descriptor/snapshot, not per-line service objects/closures. Keep results/chips from published analysis, no analysis in getters. |
| A39 integration | PadView.cs:24,78,441–450,734–755; SelfTest.cs currently no LAYERS match | Rebase on A39 and follow its actual menu/motion contracts and table schema. Do not restore Editor -> UI dependencies or count their removal as A42 work. A39 implementation is not present here, so exact API integration remains unverified. |

## Proposed small SelfTest checks (not run)

Use existing `Check(bool,string)` style (`SelfTest.cs:14`). Names below are proposed helper APIs, not claims that they compile today. For concision, pure test adapters `Continue(text,mode,column)` and `Renumber(lines,target,fresh)` apply the proposed edit decisions to strings; real implementation should test the returned edits directly or use a tiny local apply helper. `Toggle` means apply CheckboxEdit; `Indent` means apply IndentEdit. Inputs/expected behavior are derived from the cited current code.

```csharp
Check(MathNotes.BlankLabel("spent 12.50 + 8.25 =") == "      12.50 + 8.25 =", "label offsets");
Check(MathNotes.BlankLabel("rent = 12") == "rent = 12", "assignment not blanked");
Check(MathNotes.BlankLabel("spent -12 + 3 =") == "spent -12 + 3 =", "minus not label retry trigger");
Check(MathNotes.StripListMarker("- 19.99 * 3 =") == "  19.99 * 3 =", "bullet blanked");
Check(MathNotes.StripListMarker("2. 19 + 3 =") == "2. 19 + 3 =", "numbered marker preserved");
Check(MathNotes.StripListMarker("  - [x] 3 + 4 =") == "        3 + 4 =", "checkbox marker offsets");
Check(ListEditing.TryListNumber("999999999. a", out var n) && n == 999999999, "nine digits accepted");
Check(!ListEditing.TryListNumber("1000000000. a", out _), "ten digits rejected");
Check(Toggle("  - [X] task") == "  - [ ] task", "checked box clears");
Check(Toggle("  -   task") == "  [ ] task", "bullet whitespace replaced");
Check(Toggle("  task") == "  [ ] task", "indent preserved");
Check(Toggle("[ ]task") == "[x]task", "no trailing space required for toggle");
Check(Indent(" x", false) == "x" && Indent("\tx", false) == "\tx", "outdent ASCII spaces only");
Check(Continue("- [x] task", NoteMode.Plain, 10) == "- [x] task\n- [ ] ", "continued box unchecked");
Check(Continue("  - ", NoteMode.Plain, 4) == "- ", "empty nested item outdents");
Check(Continue("- ", NoteMode.Plain, 2) == "", "empty root item exits");
Check(Continue("abc", NoteMode.List, 3) == "abc\n[ ] ", "list-mode fallback");
Check(ListEditing.ContinueEdit("999999999. a", NoteMode.Plain, 12).Handled == false, "max marker Return falls through");
Check(ListEditing.ContinueEdit("- a", NoteMode.Code, 3).Handled == false, "code mode bypasses continuation");
Check(Renumber(["5. a", "9. b"], 1, false).SequenceEqual(["5. a", "6. b"]), "keep starting number");
Check(Renumber(["1. a", "  9. b", "  4. c", "8. d"], 1, true)
    .SequenceEqual(["1. a", "  1. b", "  2. c", "2. d"]), "fresh nested level and parent counter");
Check(Renumber(["1. a", "- b", "7. c"], 0, false)
    .SequenceEqual(["1. a", "- b", "7. c"]), "bullet resets same-level counter");
var math = MathNotes.EvaluateMath(["math", "rate = 10", "cost = rate * 3", "lunch 2 + 4"]);
Check(math[2]?.Result?.Value == 30 && math[3]?.Result?.Value == 6, "label retry preserves variables");
Check(math[3]?.Tokens.First() == new Calc.CalcToken(0, 6, Calc.TokenKind.Comment), "label token span");
Check(MathNotes.EvaluateMath(["math", "- 2 + 3"])[1]?.Result?.Value == 5, "bullet math stays positive");
Check(NoteAnalysis.TimerKey("note", "5m", 2) == "note:5m:2", "timer identity");
Check(FoldSelection.TargetIndex([(0, 20, false), (10, 15, true)], 12, 14, true) == 0,
    "already-folded inner section skipped");
Check(FoldSelection.TargetIndex([(0, 20, false), (10, 15, false)], 15, 17, true) == 1,
    "inclusive line intersection, highest start");
Check(FoldSelection.TargetIndex([(0, 10, false)], 11, 12, true) == -1, "no intersecting fold");
```

- Continue adapter columns are zero-based string offsets. `Renumber` targets above are zero-based within the supplied block; convert to original one-based document line in the adapter. The current public command boundary still uses one-based lines.
- Add a real document-only check: `var d = new TextDocument("5. a\n9. b"); using(d.RunUpdate()) { DocumentCommands.Indent(d,2,true); DocumentCommands.Renumber(d,2,true); } Check(d.Text == "5. a\n  1. b", "indent plus renumber"); d.UndoStack.Undo(); Check(d.Text == "5. a\n9. b", "single undo restores both");` No TextArea/window required. Clear or mark baseline undo state explicitly if helper setup inserts text rather than using constructor.
- Add Continue+Renumber document check at end-of-line and mid-line; assert resulting text, handled flag, and callback-observed caret line. Keep live caret/scroll check for manager/user: headless TextDocument has no actual editor caret anchoring.
- Fold creation already has a no-window path: `var d = new TextDocument("# H\nbody\n\ntail"); var s = NoteStructure.Build(["# H","body","","tail"]); Check(s.Folds(d,["# H","body","","tail"],false).Select(f => (f.StartOffset,f.EndOffset)).SequenceEqual([(3,14)]), "heading spans single blank");` Two blanks fixture `"# H\nbody\n\ntail"` -> `"# H\nbody\n\n\ntail"` must end heading fold at offset 8. This tests existing Structure logic, distinct from FoldSelection.
- ChipText proposed concrete calls using scalar/value inputs: `Check(ChipText.ForResult(new Calc.CalcResult("$8",8,"$")) == new ChipDescriptor("$8","result","$8",ChipColorRole.Result), "result chip"); Check(ChipText.MixedCurrencies == new ChipDescriptor("mixed currencies","error","",ChipColorRole.Error), "error chip"); Check(ChipText.ForTimer("n:5m:1", completed:true, snapshot:null) == new ChipDescriptor("done","timer:done","n:5m:1",ChipColorRole.Confirm), "completed key before lookup");` Timer-ready snapshot `{Key="k", Done=false, Running=false, Paused=null, Remaining=5m, Duration=5m}` -> `("5:00","timer:ready","k",Muted)`; running with Remaining=65s -> `("1:05","timer:running","k",Number)`; paused with Remaining=65s -> `("1:05","timer:paused","k",Muted)`; missing snapshot and completed=false -> null. Verify `Model/Timers.cs:202` formatting remains shared (or pass its formatted string from glue).
- Retain the existing A29-incremental-analysis checks (`SelfTest.cs:821–962`): full equivalence across edits, math variable propagation, duplicate timer identities, 5,000-line two-edit work bound `LastCopiedLineCount <= 8 && LastAnalysedLineCount < 32`. They exercise NoteAnalysis directly, not PadView's 140 ms scheduler. Add only focused line insertion/deletion inline-key and timer-array-shift fixtures if the extraction changes those methods; file relocation alone needs no extra coverage.

## Proposed A39-style LAYERS rules and present status

No `LAYERS`/layer test was found in current SelfTest.cs. Use A39's actual table and checking mechanism after integration, not an invented parallel framework. Rules should inspect relevant type/file bodies or symbols; a whole Editor-directory ban is wrong because it contains controls/renderers. A namespace/import scan alone can miss fully qualified references and cannot establish transitive purity.

| From -> forbidden To | Holds today? | Post-split expectation |
|---|---|---|
| TextCommands -> Avalonia.*, AvaloniaEdit.*, PadView, UI, Theme | Yes, TextCommands.cs:1–86 has only regex/BCL dependencies | Keep |
| MathNotes -> Avalonia.*, AvaloniaEdit.*, PadView, UI, Theme, Timers, Settings | Proposed type absent; extracted bodies :538–578 contain no such types except current ownership/shared prefix | Enforce; Calc + ListEditing/BCL allowed |
| ListEditing -> Avalonia.*, AvaloniaEdit.*, PadView, UI, Theme, Timers, Settings | Proposed type absent; current commands mix document/control references | Enforce; Model.NoteMode value allowed |
| ChipText -> Avalonia.*, AvaloniaEdit.*, PadView, Palette, UI, Timers instance/Entry | Proposed type absent; current ChipFor violates via Color/Palette/live timers | Enforce scalar/value snapshots; BCL/Calc allowed |
| FoldSelection -> Avalonia.*, AvaloniaEdit.*, PadView, UI | Proposed type absent; FoldAtCaret currently needs FoldingManager/editor | Enforce offsets/value tuples only |
| DocumentCommands -> Avalonia.Controls, AvaloniaEdit.Rendering, AvaloniaEdit.Editing, PadView, UI, Theme | Proposed type absent; current mixed commands refer to Editor/CheckboxGenerator | Enforce; AvaloniaEdit.Document and pure helpers allowed; caret provided as scalar/callback |
| NoteAnalysis -> PadView | No, :1065–1066/:1076 | Enforce after MathNotes call renames |
| NoteAnalysis -> Avalonia.Controls, AvaloniaEdit.Rendering, UI | Yes for class body :935–1189 direct references; whole PadView.cs import scan fails | Enforce in new file; allow AvaloniaEdit.Document, Model, Calc, NoteStructure |
| NoteAnalysis -> Avalonia.Threading / DispatcherTimer directly | Yes for class body | Keep; transitive dependency through Timers still exists |
| NoteStructure -> all colorizers/renderers | No: Structure.cs:146 calls NoteColorizer.HeadingLevel; Folds returns NewFolding (:238 onward) | Do not assert this rule in A42; optional separate structural purity task |
| Editor -> UI | No today (PadView.cs:24, Motion.cs imports UI); A39 is stipulated prerequisite | Preserve A39's rule; no A42 plan to remove those references again |
| Calc -> Editor/PadView/UI/Avalonia | Yes, Calculator.cs imports only BCL | Keep; MathNotes belongs in Editor to avoid Calc -> Editor cycle |

## Size and safest ordering

| Step | Expected PadView.cs size | Reason / verification gate for implementation |
|---|---:|---|
| Baseline | 1,189 | 932-line control plus 257 lines including separator/helper |
| Extract MathNotes and ListEditing prefix/number parser; rename tests | about 1,125 | ~46 math/helper lines, ~10 numeric parser lines, regex; small glue replacements. Keep regex patterns exact. |
| Move NoteAnalysis verbatim, then ChangedLineRange | about 860–875 | ~257 helper lines + ~8 range lines removed; run existing A29 checks after implementation |
| Checkbox then Indent document adapters/pure edits | about 830–850 | Replace ~42 lines with short wrappers; headless mutation/undo checks |
| ContinueList/Renumber/IsListLine command split | about 745–790 | ~110 lines replaced by ~15–30 glue lines; preserve group/caret sequencing and block-local work |
| Optional small ChipText/FoldSelection seams | about 730–775 | Keep palette/timer sampling/folding mutation, remove only pure decisions |

- Final planning target: roughly **750 lines (730–790)**, excluding whatever A39 independently changes. This is a source-line estimate, not a generated patch measurement. Moving cache ownership or edge-fade math is excluded. NoteAnalysis extraction accounts for most of the reduction; splitting commands introduces new adapter/value-type code, so total product LOC may grow.
- Prefer the first two steps first: static text helpers, then the unchanged analysis file move, then range helper. Run targeted existing selftests between implementation milestones, not new broad harnesses. Next checkbox (single edit), indentation, then coupled continuation/renumber, then chip/fold decisions if useful. Last remove obsolete forwarding statics and enforce layers. Manager gates build/selftest and dev-copy caret/undo checks; no live validation was performed for this report.

## Surprises, duplication, and uncertainties

- `ListPrefix` and `TextCommands.Marker` are intentionally not interchangeable: PadView single bullet/number suffix is exactly one `\s`, TextCommands accepts `\s+`; PadView numbers use ASCII `[0-9]`, TextCommands `\d`; TryListNumber additionally bounds digit count/invariant parsing. StripListMarker explicitly does not blank numbered markers. `CheckboxGenerator.Box` accepts `[ ]task` without trailing whitespace; do not force it through list-prefix recognition.
- BlankLabel is already a reusable pure function but sits on a Grid type; NoteAnalysis's dependency on it is the clearest layering reversal. The inline retry is `EvaluateInlineLine(marked) ?? EvaluateInlineLine(BlankLabel(marked))`, so a non-null error result prevents fallback; math retry has different merge rules. Do not unify them casually.
- Structure is not fully UI-free transitively: NoteStructure calls NoteColorizer.HeadingLevel and exposes AvaloniaEdit NewFolding; Structure.cs also contains CodeColors/FenceColorizer/IndentGuideRenderer. A file split of NoteAnalysis does not fix those existing dependencies.
- Whole-note numbering (`TextCommands.AsList`, :33–49) globally increments through body items; live Renumber maintains counters by indentation and retains starting number. Completion.Transform is deliberately whole-note; its Line/PrettyJson/Count commands have additional text logic but are outside the requested PadView extraction. Future extraction should not change their undo/caret conventions by analogy.
- Redundant code: ContinueList :843–844 returns true on both branches; OnCaretMoved :645–646 repeats the comment. PadView.TimerKey is only a SelfTest forwarding shim; TryListNumber(string) is only directly called by SelfTest. No substantive dead PadView method found by whole-product search. Public InlineLine and FoldAtCaret have only internal PadView consumers today.
- InFence/MathLine assume positive line inputs (:583–584); MathLine can index -1 for zero and InFence can index negative. Existing callers supply valid document lines. Record this precondition; silently adding bounds fixes would exceed behavior-neutral scope.
- IsBlank (:218) deliberately declines whitespace-only documents of length >=256; cache extraction must preserve this inexpensive heuristic rather than replace it with unconditional doc.Text copies on every edit.
- Uncertain until implementation/live gate: actual TextArea anchor/caret movement under extracted adapters; timing/first-use regex cost; A39's final LAYERS/menu/motion APIs. Source tracing establishes current intent and dependencies, not measured latency or installed-app behavior.
