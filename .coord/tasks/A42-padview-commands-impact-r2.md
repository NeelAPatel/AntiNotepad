# A42-padview-commands-impact-r2: re-audit of PadView after A39-A41b (read-only)

Role: director (gpt-6.1-sol medium). **READ-ONLY.** Do not edit, create or delete any file except your report
`.coord/reports/A42-padview-commands-impact-r2.md`. Do not launch or kill AntiNotepad (not even `--quit`). You cannot commit.
You may build into `build/A42-r2-check` and run `--selftest` there (set `AVALONIA_TELEMETRY_OPTOUT=1`), then delete that folder.
Branch `reorg/A42-padview-commands` (dev tip 34e5dee merged in). Product: `product/AntiNotepad`. Read `AGENTS.md` first.

## Context
First pass: `.coord/reports/A42-padview-commands-impact.md` and plan `.coord/tasks/A42-padview-commands-plan.md` (read both).
Since then dev gained, all merged: A39-motion-out-of-ui (Anim -> `Motion/`, ChromeTokens -> `Theme/`, context menu shown by a host
`ShowMenu` delegate via `RequestMenu`, `Editor/MenuEntry.cs`, table-driven IL `LAYERS` rules in `SelfTest.cs` with `LayerRule`
incl. `ExactFrom`/`ExactTo`), A40-settings-change-event (`Model/SettingsService`, one `Changed` event, setter guard; PadView and
`SmoothCaret` now get look updates via PadWindow's ordered handler), A41a-hotkeys (`Platform/HotkeyRegistrar`),
A41b-clipboard-policy (`Editor/ClipboardCapturePolicy`; the window still calls `PadView.DocumentFor` for clipboard inserts).
Goal of A42-padview-commands is unchanged: move pure text logic and analysis out of `Editor/PadView.cs` into UI-free testable types,
behavior-neutral, with no typing-latency or A29 incremental-analysis regression.

## Task
1. Run `git diff b54b476 HEAD -- product/AntiNotepad` limited to what touches `Editor/`, `SelfTest.cs`, `UI/PadWindow*.cs` calls into
   `PadView`, and list what A39-A41b already changed in or around PadView/Editor (members renamed/added/removed, line shifts).
2. Re-classify every PadView member on the current code (control/glue | pure text logic | document command | analysis) with callers,
   state, hot-path yes/no and destination. A table of only the rows that CHANGED versus the first report plus a statement that all
   other rows stand is acceptable, but spot-verify at least 15 unchanged rows by reading the code, and say which.
3. Say for each of the plan's moves (MathNotes, ListEditing, DocumentCommands, NoteAnalysis.cs, ChipText, FoldSelection), risks
   (undo grouping `RunUpdate`, caret after commands, A29 incremental invalidation, static `Documents`/`Evict`/`DocumentFor`,
   debounce, regex grammars) and guards whether they still hold, or what changed. Check any interaction with `SettingsService`
   (e.g. `Settings.InlineMath` into `NoteAnalysis.Update`, `Settings.*` reads in `ApplyLook`/margins) and with `ClipboardCapturePolicy`/
   `PadView.DocumentFor`.
4. New `LAYERS` rows for the A42 types, written in the exact `LayerRule` form now used (`new("MathNotes", "Control", ExactFrom: typeof(...),
   ExactTo: typeof(...))`, etc.). Check each row against the current code or against the planned extraction and say which hold
   today. Include existing-pattern forbidden targets: `UI`, `Platform`, `Motion`, `PadWindow`, `PadView`, `Program`, `Native`, `Window`,
   `Control`, `Dispatcher`, `TextDocument` (the last only where the type must not need a document), plus `Timers`/`Settings` where relevant.
   Note the existing `NoteAnalysis` (needs `TextDocument` and `Timers`) cannot take every row.
5. Verify the new selftest sketches from the first report still match the current code (read-only; especially `SelfTest.cs` lines
   that call `PadView.BlankLabel`, `StripListMarker`, `TryListNumber`, `TimerKey`, now at about :63, :64, :908, :937-938).
6. Honest scope assessment for the manager: which parts have clear value and low risk (e.g. `NoteAnalysis` file move, `MathNotes`),
   which are risky for typing latency or editing behavior (`ContinueList`/`Renumber`/`Indent` split, `ChipText`), what you would skip,
   and a suggested split into sub-tasks with order, expected PadView.cs line count after each, and what a user must hand-check for each.
7. Optional: build and run `--selftest` on the merged tree in `build/A42-r2-check` and report the baseline LAYERS/other lines
   (copy the exact tail), then delete the folder.

## Done means
Report at `.coord/reports/A42-padview-commands-impact-r2.md`: file:line references, short tables, flagged uncertainties; no code changes.
