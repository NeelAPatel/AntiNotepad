# A31-no-windows-show-animation

You are the director (gpt-6.1-sol medium). You plan, split and check the work; delegate the implementation to gpt-6-luna xhigh
subagents (how many, and how to split, is your call). If subagent spawning is not available to you in this run, say so at the
top of your report and do the work yourself.

Repo: this folder, a git worktree on branch `feat/A31-no-windows-show-animation` (off `dev`). Never switch branch, merge or
commit (.git is read-only for you and your subagents); the manager (a Claude session) reviews the diff, sends fixes back, and
integrates. Two other Codex runs work in parallel in other worktrees (A29-incremental-analysis: editor analysis;
A30-background-save-and-log: save and logging, including save calls in `UI/PadWindow.cs`); stay out of their areas and keep
your `UI/PadWindow.cs` edits small and local to show/hide.
App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10). Read first: `AGENTS.md`, `docs/dev-loop.md`,
`docs/decisions.md` (taskbar stand-in), and the A20, A22, A25 reports in `.coord/reports/`. Match the surrounding style,
naming and comment density. First-party only; Win32/DWM via the existing P/Invoke style in `Platform/Native.cs`.

## Request (from the user)
When the pad is shown or hidden (hotkey, tray, taskbar button, `--show`/new-note hotkey, hide on blur, Esc/Alt+F4 hide),
Windows' own open/close window animation must NOT play. Speed of show/hide is the product's priority. The app's own short
motion stays as it is: `PlayShow` content fade/rise (140 ms), the drop-down slide for Open at "Drop down", and Reduce motion
turning those off. Only the OS animation goes.

Direction: find where Windows animates the pad today (DWM show/hide transitions on `Show()`/`Hide()`, minimize/restore
animation from the `WindowState.Normal` set or the taskbar stand-in path, any fade from window styles) and turn it off at
the source, e.g. `DwmSetWindowAttribute(DWMWA_TRANSITIONS_FORCEDISABLED, TRUE)` on the pad window once its handle exists,
and the same for the taskbar stand-in and any other window that is shown/hidden with the pad if they animate. Settings and
other secondary windows are out of scope unless they share the pad's show/hide. Explain in the report which animation(s) you
found and why your change removes each one.

## Must keep working (already set up, verified by the user)
Prewarm hidden start and first-press speed, hotkey-to-focus timing log, focus on show, Open at (remember/center/mouse/tray/
drop-down), Keep on top and Dim when pinned fade, hide on blur, taskbar modes While visible/Always/Never and click-to-hide
on a focused pad (A22-A25), A26 shutdown close, acrylic/mica/solid backgrounds and Windows 11 corner/border handling.

## Scope
Files: `Platform/Native.cs` (new attribute call), `UI/PadWindow.cs` (show/hide path, minimal), the taskbar stand-in file if
needed, `SelfTest.cs` (ONE new method at the END plus one call line in `Run`, only if something is checkable headless;
otherwise none, say so).

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a31-check`: 0 errors, 0 warnings;
  `build/a31-check/AntiNotepad.exe --selftest` prints SELFTEST PASS. Delete `build/a31-check` if you can.
- Never launch AntiNotepad any other way, never kill it, never read or write `%LOCALAPPDATA%\AntiNotepad`,
  `%LOCALAPPDATA%\Programs\AntiNotepad` or the real Documents folder. No desktop/computer-use testing; the user checks it by
  hand in the dev copy.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A31-no-windows-show-animation.md`: split, files changed, which OS animations exist and how each is
  disabled, what the user must check by hand in the dev copy (show/hide by every route listed above, with and without Reduce
  motion; marked "needs a live check"), anything not done.
