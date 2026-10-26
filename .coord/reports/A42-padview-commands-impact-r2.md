# A42-padview-commands-impact-r2

Read-only re-audit, 2026-10-26. Recommendation: do **A42a-math-notes**, then **A42b-note-analysis-file**; gate before deciding on **A42c-checkbox-indent** and **A42d-list-commands**. Defer ChipText and FoldSelection. The earlier plan remains technically viable, but its approximately 750-line target should not drive scope.

## Evidence and boundaries

- Read AGENTS.md first, then .coord/STATUS.md, docs/agents.md, docs/dev-loop.md, docs/decisions.md, the first impact report and the plan. The user's report-only restriction overrides AGENTS.md's STATUS-update instruction. Neither STATUS nor the plan was edited.
- Audited branch `reorg/A42-padview-commands`, HEAD `66e5340645e0400a2670beda3c48be93c28b21b9`: brief commit after merge `54d41ca`, incorporating dev `34e5dee`.
- Ran `git diff b54b476 HEAD -- product/AntiNotepad/Editor`, the corresponding SelfTest.cs and PadWindow*.cs diffs, and source/caller searches. Whole-product diff statistics were used only for orientation; findings concern Editor, tests and PadWindow's PadView boundary.
- Starting status already contained untracked `.coord/reports/A42-padview-commands-impact.director.codex.md`; left untouched.
- Only this report was created. No product edits, git writes/commit, application launch/termination, build or selftest. Optional build was skipped; there is **no executed baseline tail** to quote. All guard/test assessments below are source-derived.
- References are relative to `product/AntiNotepad/` unless explicitly prefixed with `.coord/` or `docs/`. PadView.cs is now **1,190 lines**: PadView ends at 933, NoteAnalysis declaration is 936 and its closing brace is 1190.

## A39-A41b delta

| Change | Current evidence | Impact on A42 |
|---|---|---|
| A39-motion-out-of-ui: Editor no longer imports UI | PadView.cs:23-24 imports Motion/Theme; Elements.cs removed UI import (all subsequent lines -1); Editor/Motion.cs:10 imports Motion | Do not repeat this work or restore Editor -> UI |
| Menu ownership inversion | PadView.cs:78 retains AppMenu; :79 adds ShowMenu delegate; :142 calls RequestMenu; :734-755 renames old private ShowMenu to RequestMenu and invokes host delegate | Keep both properties and RequestMenu in control glue |
| Menu value types/glyphs | new Editor/MenuEntry.cs:4-14 contains MenuEntry, Separator, MenuGlyphs; PadWindow.Layout.cs:120-130 supplies host ShowMenu and AppMenu | AppMenu's source type is now Editor.MenuEntry; no UI menu references belong in extracted types |
| Motion/theme relocation | Anim moved UI -> Motion/Anim.cs; ChromeTokens moved UI/Chrome.cs -> Theme/ChromeTokens.cs | PadView._arrive :52 and Arrive :442-451 resolve Motion.Anim; margins/fade still resolve ChromeTokens, now Theme |
| A40-settings-change-event | Model/SettingsService.cs:119-128 Current/Changed/Update; Program.cs:349 subscribes window.HandleSettingsChanged; PadWindow.Keys.cs:269-283 ordered reaction; PadWindow.cs:475-496 appearance -> pane ApplyLook | **No PadView body change** from A40 in this diff; ApplyLook already calls SmoothCaret.SettingsChanged (:179). Preserve scalar settings input to analysis |
| A41a-hotkeys | PadWindow.cs:30,45-50 registrar field/construction; :451-453 forwarding registration members; Platform/HotkeyRegistrar.cs | No new PadView command caller or behavior change |
| A41b-clipboard-policy | new Editor/ClipboardCapturePolicy.cs:3-35; PadWindow.Navigation.cs:287-315 replaces window dedupe/format state | Window still calls DocumentFor then Insert (:312-313); document cache/lifecycle remains out of scope |
| SelfTest infrastructure | SelfTest.cs:19-36 invokes layer/settings/hotkey/clipboard checks; :100-108 LayerRule; :343-370 table; :385-446 scanner | Extend this table; no separate guard framework. Existing A42 helper and A29 checks remain |

PadView's exact diff is **12 added / 11 removed lines**, with net +1 solely from ShowMenu property. Lines 1-78 retain their positions (using replacement has zero net change); **every old line >=79 maps to old+1**, including all NoteAnalysis members. Only the menu handler body and its constructor subscription change beyond imports/property addition. No math, list, checkbox, document-cache, debounce or NoteAnalysis algorithm changed. SelfTest net +674 (677 added/3 removed) includes Welcome's settings-service migration; its first helper calls shift +19, while checks after the added infrastructure shift +674.

## Current member classification: changed rows

This is a delta to the complete inventories in `.coord/reports/A42-padview-commands-impact.md` (PadView and NoteAnalysis tables). **Every other member's classification, state, hot-path flag and destination stands**, with the universal line mapping above. Auto-accessors, nested record generated members and lambdas remain accounted for by their enclosing rows as before. Caller references involving the window/settings path should use the current references below rather than old line numbers.

| Member/current line | Class | Current callers / state | Hot path | Destination |
|---|---|---|---|---|
| ShowMenu property :79 (new) | control/glue | assigned PadWindow.Layout.cs:120; RequestMenu invokes; holds host Action<Control,IReadOnlyList<MenuEntry>> | no, menu gesture | stay |
| old ShowMenu -> RequestMenu :734 | control/glue | constructor :142; Editor selection/undo, AppMenu, MenuGlyphs, host ShowMenu; no direct Palette/Settings or PadMenu access now | no | stay |
| AppMenu :78 | control/glue | PadWindow.Layout.cs:121-131 assigns; RequestMenu reads; Editor.MenuEntry factory | no | stay |
| constructor :81 | control/glue | PadWindow.Layout.cs:115; UI/MotionPreview.cs:110; same visual/timer/analysis setup, new RequestMenu subscription | setup no; callbacks yes | stay |
| _arrive :52 / Arrive :442 | control/glue | same navigation/preview callers; handle/animation resolves Motion.Anim instead of UI.Anim | no (arrival) | stay |
| Settings :64 / ApplyLook :176 | control/glue | Settings is still pane-held object; host now obtains Current from SettingsService, calls ApplyAppearance -> pane ApplyLook; constructor/preview/reset/zoom callers remain | look update no; Settings reads hot | stay; no service subscription in helpers |
| DocumentFor :202 | control/glue | Bind :287; Navigation.cs:202-203 clear/undo, :312 clipboard, :384 OCR; owns Documents/Owners/Recent/Attach/400 undo limit | lookup no; TextChanged callback yes | stay; clipboard policy changes call preparation only |

The numeric visual helpers EdgeFadeVaries/EdgeFadeAlphaAt retain the first report's “pure text logic” bucket only because the requested four-way taxonomy has no numeric-visual class; they are visual math and **not extraction candidates**.

### Spot-verification of unchanged rows

Read the bodies/declarations of every unchanged member listed below, not just their names (more than the requested 15). Hot means editing, analysis or layout/redraw reachability, as in the first report.

| Members/current lines | Class; state; callers; hot; destination confirmed |
|---|---|
| Documents :31, Recent :199, Owners :200 | control/glue; static cache/LRU/model ownership; DocumentFor/Forget/Evict; no (except model callback); stay |
| ListPrefix :32 | pure text logic; compiled regex; marker stripping/list recognition/numbering/continuation; yes; ListEditing |
| Bind :266 | control/glue; binding/cache/folding/caret/scroll/analysis; navigation and preview; no; stay |
| Release :421 | control/glue; detaches doc/timer and resets analysis/private cache; preview; no; stay |
| Flush :434 | control/glue; Note caret/scroll; bind/window; no; stay |
| OnDocumentChanged :456 | control/glue; invalidation/pending fields/debounce/Edited; document event; yes; stay |
| _analyse :492 | control/glue; 140 ms DispatcherTimer; constructor/Bind/Release/change handler; yes; stay |
| ChangedLineRange :494 | analysis; document/event bounds only; change handler; yes; NoteAnalysis static |
| ClearPendingAnalysis :503 | analysis; four pending fields; tick/Bind/Release/change; yes; stay |
| Analyse() :511 / Analyse(int,int,bool) :513 | control/glue; published mode/structure/hints/folds and analysis; Bind/change/tick; yes for ranged; stay |
| EvaluateMath :539 / BlankLabel :569 / StripListMarker :576 | pure text logic; local arrays/calculator context/regex; NoteAnalysis.Update, tests (last two); yes; MathNotes |
| TimerKey :588 | pure text logic forwarding shim; no state; SelfTest only; no; remove, tests -> NoteAnalysis |
| ChipFor :591 | control/glue with optional pure decision seam; analysis/Palette/live Timers; Renderers.cs:261; yes redraw; stay (defer ChipText) |
| ToggleCheckbox :651 | document command; minimal document edits/Box regex; OnKeyDown and Elements.cs:73; yes gesture; optional DocumentCommands/ListEditing |
| OnKeyDown :672 | control/glue; selection/completion/RunUpdate/dispatch; ctor; yes; stay |
| IsListLine :758 | document command; document line/prefix; OnKeyDown; yes Tab; optional DocumentCommands -> ListEditing |
| TryListNumber(string) :764 / TryListNumber(Match) :766 | pure text logic; regex groups/invariant number; test / commands; no / yes gesture; ListEditing |
| Indent :774 / Renumber :792 / ContinueList :820 | document command; document lines/prefix/caret/mode; key handler and nested Renumber; yes Enter/Tab; optional DocumentCommands/ListEditing |
| Insert :860 | control/glue; document caret insertion/BringCaretToView; ContinueList; yes Enter; retain caret/scroll glue |
| ApplyMargins :886 | control/glue; Settings/font/bounds/insets/hint/gutter/mask; layout/ApplyLook; yes layout; stay |
| FoldAtCaret :910 | document command; FoldingManager sections/mutation/document line; key handler; yes shortcut; stay (defer FoldSelection) |

Also read NoteAnalysis :936-1190 in full: its nested records, all fields/properties and Invalidate/Reset/Update/ReadLines/ShiftInlineLines/UpdateTimers/TimerKey retain the first report's analysis classification, per-instance state, callers, hot flags and file-move destination. No new member exists there. Same-namespace file relocation does not rename the type.

## Re-evaluation of each move

| Move | Current judgment and preserved contract |
|---|---|
| MathNotes | Clear value, low risk. :539-580 still pure string/calculator work. Rewire NoteAnalysis :1066-1067/:1077 and tests :63-64. Preserve two whole-note passes, successful first-pass result precedence, Comment spans, AssignedName/ErrorReason, string lengths and variables. Calculator remains in Calc; MathNotes belongs in Editor. |
| ListEditing | Prefix/number parser extraction is low risk and needed by MathNotes. Remaining pure edits are useful only with precise edit/result contracts. Prefix, Box and TextCommands.Marker are separate grammars. Do not route every key through whole-note strings. |
| DocumentCommands | Still viable, medium/high editing risk. Take TextDocument/scalars, apply minimal edits, caller owns RunUpdate. Checkbox/indent first; continuation/renumber coupled later. Keep insertion callback/post-edit caret read synchronous. No SettingsService, editor controls, UI or timer ownership. |
| NoteAnalysis.cs | Highest LOC benefit at low risk if moved verbatim after helper rewiring. Move same namespace/type identity; add ChangedLineRange as a separate small change within that milestone. Keep scheduler and published pane facades. Not pure: needs TextDocument, NoteStructure, Timers and timer reconciliation. |
| ChipText | Defer. ChipFor :591-617 is short and on visible-line draw/timer ticks. Pure snapshot/descriptor layer adds allocations/contracts and can change live timer sampling. If later done, readonly values, sample once in PadView, pass formatted strings from Timers.Format; never let ChipText depend on Timers. |
| FoldSelection | Defer: approximately ten lines of current selection plus mutation buys little. Later TargetIndex must preserve stable ties, inclusive line intersection and highest StartOffset among sections whose state differs, not simply innermost caret-containing section. |

### Risks and guards still required

| Risk | Current source and required guard |
|---|---|
| Undo grouping | PadView.cs:690 and :697 still caller-owned RunUpdate; empty nested item also Renumber inside ContinueList :842. Keep all edits in same group, synchronous. Real TextDocument single Undo for continue+renumber and indent+renumber; no Text assignment replacement. |
| Caret after commands | :690/:697 read CaretLine **after** mutation; :831/:856 insert at current caret, :860-864 BringCaretToView. Pure planner may decide insertion text, but headless scalar caret arithmetic cannot prove AvaloniaEdit anchors/scroll. Keep callback/live caret read and hand-check middle/end of line and viewport bottom. |
| Renumber offsets | :792-816 retrieves live line offsets for each Replace, including digit-width changes. Plan snapshot edits descending by absolute offset or apply with refreshed live offsets. Scan only contiguous nonblank list/ASCII-space block (:797-801), not entire note. |
| A29 invalidation | :460-461 invalidate post-change DocumentLine keys before scheduling; :494-501 surrounding range. NoteAnalysis :965-973 CWT invalidation, :1089-1103 cached text. Preserve zero-based inclusive changes, one-based inline/timer keys and delta shifting :1106-1124/:1142-1156. |
| Debounce | :149-157 tick snapshots then clears; :465 >1500 (not >=); :477-487 union/count/full fallback; :492 140 ms. Bind/Release stop/reset/clear. Leave all scheduling in PadView. |
| Work/latency | ReadLines still allocates/visits every line; math whole-note passes and timer identity/reconcile remain O(n). A29 counters measure selected work, not total CPU. No claim of latency gain; reject new whole-document copies on Enter/Tab. Hand-check 5,000+ lines. |
| Documents/Evict/DocumentFor | :202-248 remains static cache + 400 undo limit + lazy Note.Attach + Touched callback, dirty/on-screen eviction protection and Detach. Private preview cache :264/:285-287 separate. Keep unchanged; document-command adapter accepts caller's document, never creates/caches another. |
| Binding | :268-306 Flush/guard/detach/stop/reset/uninstall old folding/swap/attach/observed count/caret/un-guard/full analysis order remains. No async extracted command crossing a bind. |
| Regex grammars | Prefix :32 uses ASCII [0-9], bullet/number one whitespace, optional checkbox suffix; Box Elements.cs:45 allows no trailing whitespace and group 2 is checked character; TextCommands.cs:8 uses \d and \s+. Preserve all patterns/options and compiled statics; BlankLabel :571 stays literal Regex.Match. Toggle bullet cleanup :666 uses \s+. No consolidation. |
| Guard proof | During future implementation only, introduce/revert deliberate forbidden references and prove guards fail with offender names. This read-only audit did not inject references. Merely adding a using or a null local optimized away is not a reliable IL violation. |
| Existing bounds quirks | InFence/MathLine :584-585 retain positive-line preconditions; ContinueList redundant true branches :844-845 remain. No incidental fixes in behavior-neutral extraction. |

### SettingsService interaction

The service owns mutations and emits one Changed event (Model/SettingsService.cs:119-128); Program subscribes the window at Program.cs:349. Window HandleSettingsChanged (:269-283 in Keys) orders policy/taskbar/appearance/persistence; ApplyAppearance (PadWindow.cs:475-496) calls pane ApplyLook. PadView.ApplyLook :178-195 replaces pane Settings/Palette, calls _caret.SettingsChanged **after** assigning Settings, applies fonts, line spacing, margins, syntax and redraw. SmoothCaret (Editor/Motion.cs:226-249) reads _pad.Settings; it has no service subscription.

Keep service and event handling out of MathNotes/ListEditing/DocumentCommands/NoteAnalysis. Analyse :518 passes **Settings.InlineMath as bool**, not Settings/SettingsService. NoteAnalysis :1008/:1020/:1055 detects flag changes on the next Update and refreshes inline results; ApplyLook itself does not call Analyse. Folding manager installation is still Bind-only (:295). Consequently live InlineMath/Folding refresh limitations are pre-existing and must not be silently “fixed” here, even though the service flags them Analysis (SettingsService.cs:64,68).

Other pane reads stay in UI glue: EdgeFade :318, OnCaretMoved :648, ApplyMargins TextWidth/FontSize/Bars :889-899, UpdateTopInset Bars :906-907. Helpers receive only the mode/flag/text they need. The existing setter guard should remain unchanged; extracted helpers should neither write settings nor join its allowlist.

### ClipboardCapturePolicy / shared document interaction

Policy owns own-copy/last-captured strings and format/dedupe only (ClipboardCapturePolicy.cs:14-35). Window keeps target choice, async clipboard read, private-clipboard test and actual mutation (Navigation.cs:295-315). Decide records last capture **before** DocumentFor/Insert, as currently; do not change that ordering here.

DocumentFor still supplies the note's shared live TextDocument. Visible target edits reach normal OnDocumentChanged; hidden targets retain Note.Attach/Touched and are fully analysed on a later bind. FormatAppend produces bullet + indented multiline text; it is not interactive list continuation or renumbering. Do not redirect capture through A42 list commands, add RunUpdate/renumber, reset policy on bind, or unify its format grammar with ListPrefix. Existing Elements checkbox callback still calls PadView.ToggleCheckbox; facades keep this caller stable.

## Concrete LAYERS rows

SelfTest.cs:100-108 is the exact schema; :485-503 selects exact source and nested types. Namespace rows match **AntiNotepad.<segment> only** (:598-602). Thus `new("MathNotes", "Avalonia.Controls", ExactFrom: ...)` would silently match nothing: do not use it. ExactTo checks exact/nested identity, not all subclasses. The scanner collects signatures/base/interfaces/generic types/IL tokens (:398-420/:604-737); it is a direct-reference guard, not a transitive dependency proof.

The following are literal proposed table entries using the present mechanism. New types are absent today, so their rows are **planned, not compilable/executed today**. Within each block every row has the same status given after the blocks. Theme rows extend the earlier plan; SettingsService rows explicitly prevent event-service coupling.

### MathNotes

```csharp
new("MathNotes", "UI", ExactFrom: typeof(Editor.MathNotes)),
new("MathNotes", "Platform", ExactFrom: typeof(Editor.MathNotes)),
new("MathNotes", "Motion", ExactFrom: typeof(Editor.MathNotes)),
new("MathNotes", "Theme", ExactFrom: typeof(Editor.MathNotes)),
new("MathNotes", "PadWindow", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(PadWindow)),
new("MathNotes", "PadView", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Editor.PadView)),
new("MathNotes", "Program", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Program)),
new("MathNotes", "Native", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Native)),
new("MathNotes", "Window", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Avalonia.Controls.Window)),
new("MathNotes", "Control", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Avalonia.Controls.Control)),
new("MathNotes", "Dispatcher", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Avalonia.Threading.Dispatcher)),
new("MathNotes", "TextDocument", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(AvaloniaEdit.Document.TextDocument)),
new("MathNotes", "Timers", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Timers)),
new("MathNotes", "Settings", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Settings)),
new("MathNotes", "SettingsService", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(SettingsService)),
```

### ListEditing

```csharp
new("ListEditing", "UI", ExactFrom: typeof(Editor.ListEditing)),
new("ListEditing", "Platform", ExactFrom: typeof(Editor.ListEditing)),
new("ListEditing", "Motion", ExactFrom: typeof(Editor.ListEditing)),
new("ListEditing", "Theme", ExactFrom: typeof(Editor.ListEditing)),
new("ListEditing", "PadWindow", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(PadWindow)),
new("ListEditing", "PadView", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Editor.PadView)),
new("ListEditing", "Program", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Program)),
new("ListEditing", "Native", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Native)),
new("ListEditing", "Window", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Avalonia.Controls.Window)),
new("ListEditing", "Control", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Avalonia.Controls.Control)),
new("ListEditing", "Dispatcher", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Avalonia.Threading.Dispatcher)),
new("ListEditing", "TextDocument", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(AvaloniaEdit.Document.TextDocument)),
new("ListEditing", "Timers", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Timers)),
new("ListEditing", "Settings", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Settings)),
new("ListEditing", "SettingsService", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(SettingsService)),
```

### DocumentCommands

```csharp
new("DocumentCommands", "UI", ExactFrom: typeof(Editor.DocumentCommands)),
new("DocumentCommands", "Platform", ExactFrom: typeof(Editor.DocumentCommands)),
new("DocumentCommands", "Motion", ExactFrom: typeof(Editor.DocumentCommands)),
new("DocumentCommands", "Theme", ExactFrom: typeof(Editor.DocumentCommands)),
new("DocumentCommands", "PadWindow", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(PadWindow)),
new("DocumentCommands", "PadView", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(Editor.PadView)),
new("DocumentCommands", "Program", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(Program)),
new("DocumentCommands", "Native", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(Native)),
new("DocumentCommands", "Window", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(Avalonia.Controls.Window)),
new("DocumentCommands", "Control", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(Avalonia.Controls.Control)),
new("DocumentCommands", "Dispatcher", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(Avalonia.Threading.Dispatcher)),
new("DocumentCommands", "Timers", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(Timers)),
new("DocumentCommands", "Settings", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(Settings)),
new("DocumentCommands", "SettingsService", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(SettingsService)),
```

### NoteAnalysis

```csharp
new("NoteAnalysis", "UI", ExactFrom: typeof(Editor.NoteAnalysis)),
new("NoteAnalysis", "Platform", ExactFrom: typeof(Editor.NoteAnalysis)),
new("NoteAnalysis", "Motion", ExactFrom: typeof(Editor.NoteAnalysis)),
new("NoteAnalysis", "Theme", ExactFrom: typeof(Editor.NoteAnalysis)),
new("NoteAnalysis", "PadWindow", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(PadWindow)),
new("NoteAnalysis", "PadView", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Editor.PadView)),
new("NoteAnalysis", "Program", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Program)),
new("NoteAnalysis", "Native", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Native)),
new("NoteAnalysis", "Window", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Avalonia.Controls.Window)),
new("NoteAnalysis", "Control", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Avalonia.Controls.Control)),
new("NoteAnalysis", "Dispatcher", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Avalonia.Threading.Dispatcher)),
new("NoteAnalysis", "Settings", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Settings)),
new("NoteAnalysis", "SettingsService", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(SettingsService)),
```

### ChipText

```csharp
new("ChipText", "UI", ExactFrom: typeof(Editor.ChipText)),
new("ChipText", "Platform", ExactFrom: typeof(Editor.ChipText)),
new("ChipText", "Motion", ExactFrom: typeof(Editor.ChipText)),
new("ChipText", "Theme", ExactFrom: typeof(Editor.ChipText)),
new("ChipText", "PadWindow", ExactFrom: typeof(Editor.ChipText), ExactTo: typeof(PadWindow)),
new("ChipText", "PadView", ExactFrom: typeof(Editor.ChipText), ExactTo: typeof(Editor.PadView)),
new("ChipText", "Program", ExactFrom: typeof(Editor.ChipText), ExactTo: typeof(Program)),
new("ChipText", "Native", ExactFrom: typeof(Editor.ChipText), ExactTo: typeof(Native)),
new("ChipText", "Window", ExactFrom: typeof(Editor.ChipText), ExactTo: typeof(Avalonia.Controls.Window)),
new("ChipText", "Control", ExactFrom: typeof(Editor.ChipText), ExactTo: typeof(Avalonia.Controls.Control)),
new("ChipText", "Dispatcher", ExactFrom: typeof(Editor.ChipText), ExactTo: typeof(Avalonia.Threading.Dispatcher)),
new("ChipText", "TextDocument", ExactFrom: typeof(Editor.ChipText), ExactTo: typeof(AvaloniaEdit.Document.TextDocument)),
new("ChipText", "Timers", ExactFrom: typeof(Editor.ChipText), ExactTo: typeof(Timers)),
new("ChipText", "Settings", ExactFrom: typeof(Editor.ChipText), ExactTo: typeof(Settings)),
new("ChipText", "SettingsService", ExactFrom: typeof(Editor.ChipText), ExactTo: typeof(SettingsService)),
```

### FoldSelection

```csharp
new("FoldSelection", "UI", ExactFrom: typeof(Editor.FoldSelection)),
new("FoldSelection", "Platform", ExactFrom: typeof(Editor.FoldSelection)),
new("FoldSelection", "Motion", ExactFrom: typeof(Editor.FoldSelection)),
new("FoldSelection", "Theme", ExactFrom: typeof(Editor.FoldSelection)),
new("FoldSelection", "PadWindow", ExactFrom: typeof(Editor.FoldSelection), ExactTo: typeof(PadWindow)),
new("FoldSelection", "PadView", ExactFrom: typeof(Editor.FoldSelection), ExactTo: typeof(Editor.PadView)),
new("FoldSelection", "Program", ExactFrom: typeof(Editor.FoldSelection), ExactTo: typeof(Program)),
new("FoldSelection", "Native", ExactFrom: typeof(Editor.FoldSelection), ExactTo: typeof(Native)),
new("FoldSelection", "Window", ExactFrom: typeof(Editor.FoldSelection), ExactTo: typeof(Avalonia.Controls.Window)),
new("FoldSelection", "Control", ExactFrom: typeof(Editor.FoldSelection), ExactTo: typeof(Avalonia.Controls.Control)),
new("FoldSelection", "Dispatcher", ExactFrom: typeof(Editor.FoldSelection), ExactTo: typeof(Avalonia.Threading.Dispatcher)),
new("FoldSelection", "TextDocument", ExactFrom: typeof(Editor.FoldSelection), ExactTo: typeof(AvaloniaEdit.Document.TextDocument)),
new("FoldSelection", "Timers", ExactFrom: typeof(Editor.FoldSelection), ExactTo: typeof(Timers)),
new("FoldSelection", "Settings", ExactFrom: typeof(Editor.FoldSelection), ExactTo: typeof(Settings)),
new("FoldSelection", "SettingsService", ExactFrom: typeof(Editor.FoldSelection), ExactTo: typeof(SettingsService)),
```

| Block | Holds today / extraction check |
|---|---|
| MathNotes | Type absent. All rows expected to hold after verbatim math helper extraction + ListEditing prefix access. No Timer/Settings/UI/document needs in bodies :539-580. |
| ListEditing | Type absent. All rows expected after pure decisions extraction. Must move Box ownership from CheckboxGenerator; a direct generator dependency defeats the pure boundary even though Control exact-type guard alone may miss it. |
| DocumentCommands | Type absent. All listed rows expected; deliberately **no TextDocument prohibition**. Retain doc/DocumentLine/RunUpdate as needed; no TextArea/caret-control dependency. |
| NoteAnalysis | Type exists. UI/Platform/Motion/Theme/PadWindow/Program/Native/Window/Control/Dispatcher/Settings/SettingsService rows hold by current direct-reference source inspection. **PadView row fails today** at :1066-1067/:1077; holds only after MathNotes rewiring. No Timers/TextDocument rows: both are required at :941-942/:996-997/:1126-1184. Timers indirectly uses DispatcherTimer; NoteStructure indirectly uses colorizer/folding. Direct Dispatcher ban does not imply transitive purity. |
| ChipText | Type absent. Rows hold only with pure descriptor values, formatted strings and scalar snapshots supplied by PadView. Calling Timers.Format inside ChipText would violate the proposed Timers row; pass its output from glue instead. |
| FoldSelection | Type absent. Rows hold only with scalar tuple ranges and index result. Passing FoldingSection/FoldingManager/DocumentLine would undermine the proposed pure seam. |

These rows cover the requested existing-pattern targets, but **do not fully enforce** the earlier “no Avalonia.*, no AvaloniaEdit.*” promise. Keep targeted exact bans in the same table, plus source review of all helper parameter/result dependencies; the current schema cannot express broad external namespace bans. If stronger automated bans are desired, separately review a narrow matcher extension rather than presenting these rows as equivalent.

Useful additional exact rows (same mechanism, all expected after intended extraction; NoteAnalysis currently has no direct Rendering/Editing controls):
```csharp
new("ListEditing", "CheckboxGenerator", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Editor.CheckboxGenerator)),
new("DocumentCommands", "TextEditor", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(AvaloniaEdit.TextEditor)),
new("DocumentCommands", "TextArea", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(AvaloniaEdit.Editing.TextArea)),
new("DocumentCommands", "TextView", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(AvaloniaEdit.Rendering.TextView)),
new("NoteAnalysis", "TextEditor", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(AvaloniaEdit.TextEditor)),
new("NoteAnalysis", "TextArea", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(AvaloniaEdit.Editing.TextArea)),
new("NoteAnalysis", "TextView", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(AvaloniaEdit.Rendering.TextView)),
```

Do not add namespace-wide Editor -> Motion/Theme/Settings/Timers/Control bans: PadView and renderers need those. Existing Editor -> UI plus the current 25 LayerRule rows and settings setter scan stay. Current rows are source-audited, **not** an executed “0 refs” result.

## Recheck of first-report selftest sketches

All helper APIs in the first report remain **proposals**, not current compiling symbols. The unchanged algorithm bodies support the expected outputs below; none was run here.

| Sketch group | Current verification / action at extraction |
|---|---|
| Existing BlankLabel / StripListMarker calls | SelfTest.cs:63-64 still call Editor.PadView; rename to Editor.MathNotes in same change. :569-580 confirms “spent ” -> six spaces, assignment/minus-label input unchanged; bullet -> equal-length spaces; numbered marker unchanged; “  - [x] ” -> eight spaces. |
| Existing TryListNumber | SelfTest.cs:908 still rejects 12-digit marker via PadView; :764-771 supports proposed 9-digit accepted / 10-digit rejected checks. Rename to ListEditing. |
| Existing TimerKey | SelfTest.cs:937-938 still calls PadView shim :588; actual NoteAnalysis.TimerKey :1189 same identity string. Rename tests to NoteAnalysis when removing shim. |
| Checkbox cases | :657-668 + Elements.cs:45 supports [X] -> space, bullet+all suffix whitespace replaced, indentation preserved, “[ ]task” -> “[x]task”. Preserve Box group 2. |
| Indent cases | :778-783 confirms exactly two inserted spaces; outdent up to two ASCII spaces; tab unchanged. |
| Continue cases | :825-856 confirms Code declines, max number declines, checked marker resets, empty nested outdents, empty root clears, List-mode unmatched first/nonblank fallback. First report's zero-based columns (10,4,2,3,12,3) match fixture lengths. Adapter must preserve one-based document vs zero-based planner indices. |
| Renumber cases | :797-815 supports retained starting number, fresh nested start, bullet counter reset and digit-width 9 -> 10. Fresh nested sketch target=1 means second block line (one-based doc line 2). |
| Math cases | :541-564 confirms full-note retry preserves variable evaluation and Comment span (0,6) for “lunch ”; bullet “- 2 + 3” becomes positive 5. Fixed numeric fixtures avoid Calculator.Clock concerns. Inline fallback :1067 is still null-coalescing, not “retry on any error”. |
| Real-document undo | First report's TextDocument indent+renumber fixture still matches :690/:697/:774-815. Add a real document check per grouped command when implementing; construct initial text without adding baseline undo edits. Headless checks cannot prove TextArea caret/scroll. |
| Fold sketches | :913-917 confirms inclusive intersection, stable OrderByDescending start and skip sections already requested state. Existing NoteStructure.Folds Structure.cs:238-248 yields first report's end offsets 14 for one blank and 8 for two blanks; separate from target-selection helper. |
| Chip sketches | :596-613 confirms result/error precedence, completed-key lookup first, done/running/paused/ready ordering; Timers.cs:202-206 yields 5:00 and 1:05. APIs remain hypothetical; skip new chip checks if ChipText deferred. |
| A29 capability checks | SelfTest.cs:1480-1648 retains full-equivalence/fence/heading/list/math/timer/inline/fold checks; :1618-1635 retains 5,000-line two-edit bounds **LastCopiedLineCount <= 8 && LastAnalysedLineCount < 32**. No body change in diff. These exercise NoteAnalysis directly, not PadView's debounce scheduler. |

The guard-negative tests from the first report are still appropriate **in a future writable implementation**, with two corrections: assert actual IL references rather than unused imports, and use the real ExactFrom/ExactTo schema above. No broad harness or extra file-relocation tests are needed.

## Recommendation on handling

Do not implement the whole original plan in one change. Keep the report-only scope here; manager can incorporate this recommendation into the plan later. Expected counts below are estimates from the current 1,190-line source, not measured patches; adapters/comments can move them.

| Ordered sub-task | Scope / decision gate | Expected PadView.cs lines after | User hand-check in dev copy |
|---|---|---:|---|
| A42a-math-notes | Extract MathNotes plus ListEditing prefix/number parsing verbatim; rewire analysis/tests; add focused math/parser rules/checks | ~1,125-1,135 | Inline/bullet/label results and token colors; math variables and mixed-currency display; ordinary Enter/Tab unchanged |
| A42b-note-analysis-file | Move existing NoteAnalysis unchanged, remove TimerKey shim/test calls; move ChangedLineRange with no scheduler change; layer guards now prohibit NoteAnalysis -> PadView | ~855-875 | Switch notes/undo isolation; mode/fence/fold/results after edits; 5,000+ line typing, pause and line insertion/deletion; no live settings refresh fix |
| A42c-checkbox-indent (optional) | Pure checkbox/indent edits + thin DocumentCommands; move Box regex owner/visual alias; single Undo checks | ~825-850 | Click and Ctrl+Enter box toggle including uppercase/no-space form; Tab/Shift+Tab, indentation, caret and single Undo |
| A42d-list-commands (optional, only if headless command coverage justifies complexity) | Coupled ContinueList/Renumber/IsListLine split, block-local plans and live offsets, same callbacks/update group | ~745-795 | Enter mid/end line; bullet/number/checklist; checked->unchecked; empty nested/root; 999,999,999 fallback; Tab nested renumber including 9->10; Ctrl+Z one step; caret at viewport bottom; long-note response |
| A42e-chip-fold (defer) | No present work. If later justified, separate draw descriptor and fold target changes | ~730-780 if eventually both done | All result/timer chip states/copy/start/pause/reset and colors; nested/overlapping fold keyboard targeting |

My recommended stopping point is **A42b-note-analysis-file**, roughly **860-875 lines** rather than chasing 750. It removes the reversed analysis -> control dependency, gives math a clear testable home, and relocates ~255 analysis lines without changing editing commands. A42c-checkbox-indent has concrete headless value but should be optional. A42d-list-commands is the highest editing/caret/undo risk and adds planners/adapters for behavior that currently works. ChipText touches draw latency; FoldSelection mostly adds ceremony. Skip both now.

At each accepted implementation milestone: manager builds with telemetry opted out, runs existing selftest plus only focused new capability checks, confirms layer violations fail when deliberately injected/reverted, and obtains the listed dev-copy hand checks before merge. This audit establishes source compatibility; **typing latency, caret anchors, GUI behavior and runtime guard output remain unverified**.

