# A27-settings-search-rework

You are the implementation worker (gpt-6-luna xhigh). Do the work yourself.

Repo: this folder, the git worktree on branch `feat/A27-settings-search-build-matches`, which holds your uncommitted
A27-settings-search-build-matches change (brief: `.coord/tasks/A27-settings-search-build-matches.md`, your report:
`.coord/reports/A27-settings-search-build-matches.md`). Never switch branch, merge or commit.

## Review result
Accepted, keep as is: the lazy `Row`/`SubRow`/`Expander` factories, `_searchQuery` gating, `SearchMatches`, `BuildSearchResults`,
the deferred recovered-folder count and Start-with-Windows read. The page methods staying the single definition is exactly right.

Rejected: the selftest plumbing inside the product window. Remove all of it:
- `SearchBuildMatchesSelfTest`, `BuildNormalSearchRows`, `SearchResultRows`, `SearchResultBorders`, `BuildAllSearchResults`
  (a second copy of the old search path), `SelfTestCursorFactory`, `SelfTestCursor`, and the now-unused usings
  (`Avalonia.Platform`, `System.Reflection`, `System.Runtime.CompilerServices`).
- `_selfTestSettings` and its uses: `S` goes back to `_pad.Settings`; the two `HotkeyEditor` status lambdas go back to
  `() => _pad.HotkeyStatus` / `() => _pad.NewNoteHotkeyStatus`.
- `_controlFactoryObserved` (and its call in `CreateControl`; inline `CreateControl` if it becomes trivial).
- `_scroll` and `_rail` go back to `readonly`.
- The `SelfTest.cs` method and its call line.
Reason: tests here are small capability checks (`AGENTS.md`), and reflection into Avalonia internals (`AvaloniaLocator`),
`DispatchProxy` cursors and an uninitialized `Window` are fragile across Avalonia updates and leak test state into product code.

Replace it with at most ONE small check at the end of `SelfTest.cs` (one call line in `Run`) that needs no Avalonia platform,
no reflection and no window: e.g. that `SearchMatches` ignores the `sub ` prefix and case and that a null query matches
everything (make it `internal static` if needed). If nothing meaningful fits without a window, add no check and say so; the
user tests Settings search live.

## Done means
- Same build/selftest gate as the original brief (`build/a27-check`, 0 warnings, SELFTEST PASS; delete the folder if you can).
- `git diff dev -- product` shows only the search change plus the small check.
- Append a "Rework" section to `.coord/reports/A27-settings-search-build-matches.md`: what was removed, the check kept (or
  why none), and drop the selftest-based timing numbers or mark them as from the removed harness.
- Never launch or kill AntiNotepad; never touch `%LOCALAPPDATA%\AntiNotepad`, `Programs\AntiNotepad` or real Documents.
