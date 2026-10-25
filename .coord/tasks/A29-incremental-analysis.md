# A29-incremental-analysis

You are the director (gpt-6.1-sol medium). You plan, split and check the work; delegate the implementation to gpt-6-luna xhigh
subagents (how many, and how to split, is your call). If subagent spawning is not available to you in this run, say so at the
top of your report and do the work yourself.

Repo: this folder, a git worktree on branch `feat/A29-incremental-analysis` (off `dev`). Never switch branch, merge or commit
(.git is read-only for you and your subagents); the manager (a Claude session) reviews the diff, sends fixes back, and
integrates. Subagents share this one worktree, so give each one a disjoint set of files. Two other Codex runs work in parallel
in other worktrees (A30-background-save-and-log: store save and logging; A31-no-windows-show-animation: pad show/hide); stay
out of their areas.
App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10). Read first: `AGENTS.md`, `docs/dev-loop.md`,
`docs/decisions.md`, finding A1-8 in `docs/audit/A1-codebase-audit.md`, and `.coord/reports/A28-editor-render-memory.md`
(the fence cache now prunes from `PadView.Analyse` via `_fences.UpdateStructure`). Match the surrounding style, naming and
comment density. First-party .NET/Avalonia only, no new packages.

## Finding: A1-8-whole-document-analysis (effort L)
`Editor/PadView.cs` `Analyse()` (~:375) copies every line, rebuilds `NoteStructure` (headings, fences, lists, folds), timer
keys, math and inline answers for the whole note after every edit; up to 1500 lines it runs synchronously in
`OnDocumentChanged`, above that after a 140 ms pause. Some blank-line and indent scans in `Editor/Structure.cs` are quadratic.

Direction (refine it): make analysis incremental where it is cheap to be exactly right: reuse unchanged line text and
per-line results, re-analyse only the affected region (with a safe widening rule for fences, folds, list runs and math
variables that depend on earlier lines), and fix the quadratic scans. Fall back to the full rebuild whenever the change is
structural in a way the incremental path can't prove correct. Correctness beats speed: the result must be identical to a full
rebuild.

## Must keep working (already set up, verified by the user)
Headings/folds and the two-blank-line fold end (C6), fenced code colors and the A28 fence cache pruning, numbered list
renumbering (C5), indent guides, math notes and inline `=` answers with one answer column (V5), timers and their chips,
mixed-currency error chip (A13), clean markdown, checkboxes, dividers, slash commands, list/timer/calculator limits (A9),
and unavailable-body notes (A5-1) never being analysed into saved text.

## Scope
Files: `Editor/PadView.cs` (analysis path only), `Editor/Structure.cs`, `Calc/*` only if math needs an incremental entry
point, `SelfTest.cs` (ONE new method at the END plus one call line in `Run`). No store, settings, window or platform changes.

## Selftest (small capability checks; no reflection into Avalonia internals, no test-only hooks in product classes)
- for a set of scripted edits (type in a fence, open/close a fence, insert/delete heading, edit a list run, change a math
  variable used later), incremental analysis equals a full rebuild;
- a typing burst on a large note re-analyses only a bounded region (observable through the analysis API, not a hook).

## Measure
Time per keystroke in a 1500-line and a 5000-line note, before and after, in the report (temporary harness, not left in
product code).

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a29-check`: 0 errors, 0 warnings;
  `build/a29-check/AntiNotepad.exe --selftest` prints SELFTEST PASS. Delete `build/a29-check` if you can.
- Never launch AntiNotepad any other way, never kill it, never read or write `%LOCALAPPDATA%\AntiNotepad`,
  `%LOCALAPPDATA%\Programs\AntiNotepad` or the real Documents folder. No desktop/computer-use testing.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A29-incremental-analysis.md`: split (subagents, files), files changed, design as built (the
  widening and fallback rules), before/after timings, selftest list, what the user must check by hand in the dev copy
  (marked "needs a live check"), anything not done.
