# A18-search-speed

You are the director (gpt-6.1-sol medium) for audit batch 2. You plan, split and check the work; delegate the implementation to
gpt-6-luna xhigh subagents as you see fit (how many, and how to split, is your call). If subagent spawning is not available to
you in this run, say so at the top of your report and do the work yourself.

Repo: this folder, a git worktree on branch `feat/A18-search-speed`. Never switch branch, merge or commit (.git is read-only for
you and your subagents); the manager (a Claude session) reviews and commits. Your subagents share this one worktree, so give each
one a disjoint set of files.
App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10). Read first: `AGENTS.md`, `docs/dev-loop.md`,
`docs/decisions.md`, and in `docs/audit/A1-codebase-audit.md` findings A1-11 and A1-12 plus A5-1 (unavailable note bodies).
Match the surrounding style, naming and comment density. First-party .NET/Avalonia only, no new packages.

## Findings
1. **A1-11-synchronous-note-search** (`UI/SearchOverlay.cs` ~:79, `Model/Store.cs` `PeekText` path). Each search keystroke calls
   `PeekText()` for every note, once per term, which reads unloaded note bodies from disk on the UI thread, then rebuilds all the
   results. With hundreds of notes typing in Ctrl+F stalls.
   Direction (refine it): debounce typing (short, ~100-150 ms; first keystroke on a small store can stay instant), read each note's
   text at most once per query (per-note cache keyed by note and its modified stamp, invalidated on edit/save), match all terms
   against that one string, run the matching off the UI thread with cancellation of stale queries, and update the results list
   without rebuilding unchanged rows if that's simple.
2. **A1-12-settings-search-materializes-all** (`UI/SettingsWindow.cs` ~:256). Settings search builds all six sections' controls,
   then filters them. Direction: filter on the rows' text (titles, descriptions, keywords) first and build only the matching rows;
   keep the visible result identical to today.

## Constraints
- Never read live, mutable note state off the UI thread. Snapshot what the background search needs (ids, titles, text already
  loaded in memory) on the UI thread; disk reads of unloaded bodies may happen in the background but must not write into the
  Note/Store objects from there (marshal results back, or keep them in a search-only cache).
- Notes whose body is missing or unreadable (A5-1, `BodyReadStatus`/`BodyUnavailable`) are never treated as empty text, never
  cached as "", and never marked dirty or saved by search. They can still match on their title.
- Results order, highlighting, keyboard navigation, the grid/list toggle and the empty state behave as today.
- No change to the store format, the save path or other windows. Files in scope: `UI/SearchOverlay.cs`, `UI/SettingsWindow.cs`,
  `UI/SettingsParts.cs` (only if a helper is needed), `Model/Store.cs` (search/peek path only; another worker edits the `Settings`
  defaults in this file, so don't touch that class), `SelfTest.cs` (ONE new method at the END with one call line in `Run`).
- Other workers edit `UI/NoteSwipe.cs`, `UI/MotionPreview.cs`, `Program.cs`, `Platform/SingleInstance.cs`, `Model/Welcome.cs`,
  `tools/*`. Don't touch them.

## Measure
Add a small timing check (selftest or a debug-only helper, your call) that builds a temp store with 200 notes of mixed size, half
of them unloaded, and measures one query before and after (wall time per keystroke on the UI-thread part). Put the numbers in the
report. Use temp folders only.

## Selftest
- search finds a term in an unloaded note and in a loaded one, with each body read at most once per query;
- a stale query is cancelled and its results never replace a newer query's;
- an unavailable body is not treated as empty and is not saved;
- Settings search for a known word returns the same rows as before and does not build non-matching sections.

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a18-check`: 0 errors, 0 warnings.
  `build/a18-check/AntiNotepad.exe --selftest` prints SELFTEST PASS. Delete `build/a18-check` if you can.
- Never launch AntiNotepad any other way, never kill it, and never read or write `%LOCALAPPDATA%\AntiNotepad`,
  `%LOCALAPPDATA%\Programs\AntiNotepad` or the real Documents folder.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A18-search-speed.md`: how you split the work (which subagents, which files), files changed, the
  design as built, before/after timings, selftest list, what a person must check live (the manager cannot do live UI tests this
  round; mark those "needs a live check"), anything not done.
