# A42b-note-analysis-file-impact: re-audit of the NoteAnalysis file move on latest dev (read-only)

Role: director (gpt-6.1-sol medium). **READ-ONLY.** Do not edit, create or delete any file except your report
`.coord/reports/A42b-note-analysis-file-impact.md`. Do not launch or kill AntiNotepad (not even `--quit`). You cannot commit.
You may build into `build/A42b-check` and run `--selftest` there (`AVALONIA_TELEMETRY_OPTOUT=1`), then delete that folder; if you do,
quote the exact LAYERS/MATH NOTES/LIST EDITING tail. Branch `reorg/A42-padview-commands` (dev tip 037f4c4 merged). Product:
`product/AntiNotepad`. Read `AGENTS.md` first, then `.coord/tasks/A42-padview-commands-plan.md` (esp. "A42b" and the r2 section)
and `.coord/reports/A42-padview-commands-impact-r2.md`.

## Context
A42a-math-notes is merged: `Editor/MathNotes.cs` (54 lines) and `Editor/ListEditing.cs` (19 lines) now exist; `PadView.cs` is
1,132 lines with `NoteAnalysis` starting near line 878 and calling `MathNotes.*` (about :1008-1019). SelfTest has 55 layer-rule lines.
A42b-note-analysis-file scope (from the plan): move `NoteAnalysis` (with `CachedLine`, `TimerLine`, `TimerKey`) UNCHANGED to
`Editor/NoteAnalysis.cs`; move `ChangedLineRange` with it; remove the `PadView.TimerKey` shim (PadView.cs:540) and point
`SelfTest.cs` (about :1018-1019) at `NoteAnalysis.TimerKey`; add an `NoteAnalysis -> PadView` layer rule; the 140 ms debounce,
>1500-line pending logic and `Analyse` stay in `PadView`.

## Task
1. Verify the A42b scope on the current code: exact line range of `NoteAnalysis` and nested types in `PadView.cs`; every `using`
   it needs (so the new file compiles with the right usings and PadView keeps the ones it still needs); any other type or helper in
   the file that `NoteAnalysis` depends on (e.g. static members of PadView, `ListEditing`, `MathNotes`, `Modes`, `NoteStructure`, `Timers`).
2. List every remaining `NoteAnalysis -> PadView`, `-> UI`, `-> Avalonia`, `-> AvaloniaEdit` reference (read the class body, and use
   the IL layer scanner's rules to say what would trip). Confirm `NoteAnalysis -> PadView` is 0 today after A42a.
3. All callers of `NoteAnalysis` and of `PadView.TimerKey` / `ChangedLineRange` across the product (PadView, SelfTest, anything else);
   what must be renamed or re-pointed; anything `internal`/`private` visibility that changes when the class leaves the file
   (e.g. private nested records, `ChangedLineRange` accessibility; keep `internal static`).
4. The per-keystroke / A29 incremental path: trace `OnDocumentChanged` -> `ChangedLineRange` -> `Invalidate` -> (debounce) -> `Analyse`
   -> `NoteAnalysis.Update` (ReadLines, ShiftInlineLines, UpdateTimers, MathNotes, Structure) and state which parts the move
   touches (should be none beyond file location) and what could silently change (static initializers, JIT/inlining, nested type
   names used by selftests via reflection, `ConditionalWeakTable` identity).
5. Existing A29 selftests that exercise it (SelfTest.cs about :1480-1700 region, current numbers) and any reflection-based tests
   or LAYERS rules that mention `NoteAnalysis` or `PadView` nested types; will they still pass unchanged? Is the IL scanner
   sensitive to moved nested types (e.g. `Editor.NoteAnalysis+CachedLine`)?
6. Proposed LAYERS rows for A42b in the exact existing `LayerRule` table form (see the A42a rows for `MathNotes`/`ListEditing` as
   the style), saying which hold today. Include `NoteAnalysis -> PadView` (ExactFrom/ExactTo), UI, Platform, Motion, Theme, PadWindow,
   Program, Native, Window, Control, Dispatcher, Settings, SettingsService, TextEditor, TextArea, TextView; note which cannot be
   asserted (Timers, TextDocument).
7. Expected `PadView.cs` and `NoteAnalysis.cs` line counts after the move (measure the line range, do not guess), and a step list
   with what the manager gates (build 0 warnings, selftest PASS, no behavior change, a deliberate-violation proof for the new
   rule).
8. Honest assessment: is A42b still worth doing as scoped? Anything to drop or add? What are the riskiest parts for typing latency
   and undo/analysis behavior (expected: essentially none for a verbatim move; say so only if the code supports it)? What should
   the later A42c-checkbox-indent / A42d-list-commands gate weigh once `NoteAnalysis` is out (remaining `PadView.cs` content,
   size, risk, value)?

## Done means
Report at `.coord/reports/A42b-note-analysis-file-impact.md` with file:line references, short tables, flagged uncertainties; no code changes.
