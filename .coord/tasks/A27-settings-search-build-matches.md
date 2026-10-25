# A27-settings-search-build-matches

You are the implementation worker (gpt-6-luna xhigh). Do the work yourself.

Repo: this folder, a git worktree on branch `feat/A27-settings-search-build-matches` (off `dev`). Never switch branch, merge or
commit (.git is read-only for you); the manager (a Claude session) reviews the diff and commits.
App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10). Read first: `AGENTS.md`, `docs/dev-loop.md`,
`docs/decisions.md`, and finding A1-12 in `docs/audit/A1-codebase-audit.md`. Match the surrounding style, naming and comment
density. First-party .NET/Avalonia only, no new packages.

## Finding
**A1-12-settings-search-materializes-all.** `UI/SettingsWindow.cs` `ShowResults` (~:260) calls `BuildSection` for all seven
sections on every search (after a 150 ms pause, ~:172), building every page's controls (palette chips, tiles, pictures,
dropdowns, the recovered-folder file count…), then keeps only rows whose `Tag` (title + description, `sub ` + title for
sub-rows, see `UI/SettingsParts.cs` `Row`/`SubRow`/`Expander`) contains the query.

## Goal
Typing in Settings search builds only the rows that match, not all seven pages. The visible result is identical to today:
same rows, same order, same section groups, same "No setting matches" text, rows still live (changing a control works and
re-renders as today), nested sub-rows shown and indented as today, conditional rows (e.g. Tint hidden for Solid) follow the
current settings.

## Hard constraint: one source of truth
An earlier attempt (A18) added a second catalog of row metadata beside the page builders; the manager rejected it because every
settings change would have to be made twice. Do NOT duplicate row titles/descriptions or a list of rows anywhere. The page
methods (`ThemePage`, `UiPage`, `EditorPage`, …) stay the single definition of each row.

Your call how, for example:
- make the expensive part of a row lazy at its definition (the control passed as a factory, built only when the row is
  kept), with the page builders running in a cheap "search" mode where `Row`/`SubRow`/`Expander` test the match before
  building; or
- another design that meets the goal and the constraint. Pick the smallest change that does; say why in the report.
Keep the 150 ms debounce unless your design makes it pointless; say which.
Whatever is still evaluated for non-matching rows (e.g. dynamic descriptions like the recovered-folder count) must be cheap or
deferred; list what remains in the report.

## Scope
Files: `UI/SettingsWindow.cs`, `UI/SettingsParts.cs`, `SelfTest.cs` (ONE new method at the END plus one call line in `Run`).
No other files. No behavior change outside search.

## Selftest
- a search for a known word (e.g. "palette", "tint", "taskbar") returns the same row titles, in the same order and groups, as
  the old build-everything path would (compute the expected list from the normal page builders in the test, not a hard-coded
  copy that can drift);
- a search builds no control for non-matching rows (count factory calls, or an equivalent observable);
- an empty result shows the "No setting matches" text.

## Measure
Time one search keystroke (UI-thread work for `ShowResults`) before and after, for a common word and a no-match word. Put the
numbers in the report.

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a27-check`: 0 errors, 0 warnings.
  `build/a27-check/AntiNotepad.exe --selftest` prints SELFTEST PASS. Delete `build/a27-check` if you can.
- Never launch AntiNotepad any other way, never kill it, and never read or write `%LOCALAPPDATA%\AntiNotepad`,
  `%LOCALAPPDATA%\Programs\AntiNotepad` or the real Documents folder.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A27-settings-search-build-matches.md`: design as built and why, files changed, before/after
  timings, selftest list, what a person must check live (Settings search look and interaction, marked "needs a live check"),
  anything not done.
