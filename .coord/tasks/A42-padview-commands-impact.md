# A42-padview-commands-impact: read-only impact analysis of splitting logic out of PadView

Role: director (gpt-6.1-sol medium). **READ-ONLY.** Do not edit, create or delete any file except your report
`.coord/reports/A42-padview-commands-impact.md`. Do not build into the tree, do not launch or kill AntiNotepad (not even `--quit`).
You cannot commit. You may run `grep`/`rg`, read files, and (if useful) build into `build/A42-impact-check` and delete it after.
Branch `reorg/A42-padview-commands`. Product: `product/AntiNotepad`. Read `AGENTS.md` first.

## Context
Audit A38-mvvm-audit rejected MVVM and chose behavior-neutral modularity refactors. This one (A42-padview-commands) moves pure
text logic and analysis out of `Editor/PadView.cs` (about 1.2k lines, a `Grid` control) into UI-free, testable types, leaving
`PadView` as the control plus glue. `Editor/TextCommands.cs` (86 lines, pure whole-note transforms) and `NoteAnalysis` (inside
`PadView.cs`, about line 935) already exist. Typing latency and the A29 incremental analysis (`NoteAnalysis.Invalidate/Update`,
`LastCopiedLineCount`, `LastAnalysedLineCount`, the >1500-line pending/140 ms debounce in `PadView.OnDocumentChanged`) must not
regress. Assume A39-motion-out-of-ui lands first (drops `Editor -> UI` refs, inverts the context menu, adds a table-driven
`LAYERS` selftest in `SelfTest.cs`); do not plan around the `UI` references it removes.

## Task
Analyse every member of `PadView` (fields, properties, events, methods, nested/helper types in the file, including
`NoteAnalysis` and anything else after the class) and write the report. Cover:

1. **Classification table**, one row per member: name, line, class = one of
   `control/glue` | `pure text logic` (no Avalonia/AvaloniaEdit types; string/array in, value out) |
   `document command` (needs `TextDocument`/caret but draws nothing) | `analysis`.
   Columns: callers (grep the whole product: renderers, colorizers, `Elements`, `Structure`, `Completion`, `PadWindow*`,
   `SelfTest`, others), state touched (fields, `Documents` cache, `Settings`, `Timers`), per-keystroke hot path yes/no (trace
   `OnDocumentChanged` -> `Analyse` -> renderers/chips and `OnKeyDown`), and proposed destination.
2. **What moves where**: proposed types/files (e.g. `TextCommands`, `ListEditing`/list helpers, `MathNotes` for `EvaluateMath`/
   `BlankLabel`/`StripListMarker`, `NoteAnalysis` in its own file, a document-command type taking a `TextDocument`), with the
   exact member list for each, and which callers need renaming (e.g. `PadView.EvaluateMath`, `PadView.BlankLabel`, `PadView.TimerKey`
   used from outside).
3. **Entangled parts**: members that mix logic with visuals/state (e.g. `ContinueList`, `Renumber`, `Indent`, `ToggleCheckbox`,
   `FoldAtCaret`, `ChipFor` reading `Palette`/`Timers`, `ToggleTimerAtCaret`) and how to split them (pure decision function +
   thin PadView applying it).
4. **Risks**, each with the concrete code spot: undo grouping (`BeginUpdate`/`EndUpdate`, `UndoGroup`), caret placement after
   commands, A29 incremental-analysis invalidation (`ConditionalWeakTable<DocumentLine,...>`, `_inlineLines`/`_timerKeys` shifting),
   the static `Documents` cache / `Forget` / `Evict`, `_binding` guard, regex `RegexOptions.Compiled` statics being moved (init
   order, shared `ListPrefix` vs `TextCommands.Marker`), `internal static` members used by `SelfTest`.
5. **Proposed selftests**: logic checks now possible without a window (list renumber/continue, checkbox toggle, `StripListMarker`,
   `BlankLabel`, `EvaluateMath` with labelled lines, fold range, chip text) as concrete `SelfTest` lines with inputs/expected
   output, taken from current behavior (verify by reading the code; say if you ran them). Also proposed `LAYERS` rules in the
   A39 style (from -> to) that hold after the split, e.g. new logic types must not reference `Avalonia.Controls`/`AvaloniaEdit.Rendering`
   or `PadView`; verify which hold today.
6. **Size estimate**: expected `PadView.cs` line count after the move, and the safest ordering of steps (smallest, lowest-risk first).
7. Anything surprising (dead code, duplicated logic between `PadView` and `TextCommands`/`Completion`/`Structure`).

## Done means
Report at `.coord/reports/A42-padview-commands-impact.md`, plain tables and short bullets, file:line references, no code changes.
Flag anything you were unsure of instead of guessing.
