# A32-native-show-hide-animation

You are the director (gpt-6.1-sol medium). You plan, split and check the work; delegate the implementation to gpt-6-luna xhigh
subagents (how many, and how to split, is your call). If subagent spawning is not available to you in this run, say so at the
top of your report and do the work yourself.

Repo: this folder, a git worktree on branch `feat/A32-native-show-hide-animation` (off `dev`). Never switch branch, merge or
commit (.git is read-only for you and your subagents); the manager (a Claude session) reviews the diff, sends fixes back, and
integrates.
App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10). Read first: `AGENTS.md`, `docs/dev-loop.md`,
`docs/decisions.md` (taskbar stand-in), and `.coord/reports/` for A20, A22, A25, A26 and A31-no-windows-show-animation.
Match the surrounding style, naming and comment density. First-party only; Win32/DWM through the existing P/Invoke style in
`Platform/Native.cs`.

## What the user wants (this reverses A31, which misread the request)
- **Show** (hotkey, tray, taskbar button, `--show`, new-note hotkey): Windows' own native window **open** animation plays, the
  same one an ordinary app window gets when it opens.
- **Hide** (hotkey again, tray, taskbar click on a focused pad, Esc, Alt+F4 hide, hide on blur): Windows' own native
  **close/minimize** animation plays.
- Today (after A31) show plays the app's own content fade/rise (`PadWindow.PlayShow`), which the user calls "some weird
  non-windows animation", and hide plays nothing. Before A31 there was most likely no native animation either, so this is new
  work, not just a revert.

## Direction
1. Revert A31: remove `Native.DisableDwmTransitions` and its two call sites (`UI/PadWindow.cs` constructor,
   `Platform/TaskbarPresence.cs`).
2. Find out why Windows doesn't animate the pad's `ShowWindow` show/hide: DWM plays open/close transitions only for top-level
   windows with certain styles (e.g. a caption/system-menu frame style; a pure `WS_POPUP`, tool window, layered or cloaked window
   may get none). Avalonia's frameless, transparent, `ExtendClientAreaToDecorations` window and `ShowInTaskbar = false` (and
   any owner set by the taskbar stand-in) all matter. Determine which style or setting suppresses it, and change the smallest
   thing that makes Windows animate while the pad still looks exactly as today: no title bar, no visible frame or border
   change, same corners, acrylic/mica/solid unchanged, still off the taskbar unless the Taskbar icon setting says otherwise.
3. Pick the native hide animation: prefer the close-style transition DWM plays when a window is hidden. If only the minimize
   animation is achievable, say so and explain where it animates to (the pad is normally not on the taskbar). Don't fake either
   one with an app-drawn animation.
4. Remove `PlayShow`'s content fade/rise on show; the native open animation replaces it. Keep the Open at "Drop down" slide
   (it's a separate, explicit option) and say in the report how it combines with the native animation. Windows' own "Animation
   effects" setting (and Reduce motion, which should keep turning the app's own motion off) must be respected.
5. Prewarm (hidden first show at Opacity 0), `ShowQuietly` (finished timers) and the session-end path must not flash or animate
   visibly.

## Must keep working (verified by the user)
Show/hide speed and the `hotkey-to-focus` log, focus on show, Open at (remember/center/mouse/tray/drop-down), Keep on top and
Dim when pinned, hide on blur, the Settings-focus exception, taskbar modes While visible/Always/Never and click-to-hide on a
focused pad (A22-A25), A26 shutdown, acrylic/mica/solid backgrounds, Windows 11 corner/border handling, the float-below pager.

## Scope
`Platform/Native.cs`, `UI/PadWindow.cs` (show/hide path and constructor only), `Platform/TaskbarPresence.cs` (only if the
stand-in or owner relationship affects the animation), `SelfTest.cs` (ONE new method at the END plus one call line in `Run`,
only if something is checkable headless, e.g. the window styles applied; otherwise none, say so).

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a32-check`: 0 errors, 0 warnings;
  `build/a32-check/AntiNotepad.exe --selftest` prints SELFTEST PASS. Delete `build/a32-check` if you can.
- Never launch AntiNotepad any other way, never kill it, never read or write `%LOCALAPPDATA%\AntiNotepad`,
  `%LOCALAPPDATA%\Programs\AntiNotepad` or the real Documents folder. No desktop/computer-use testing; the user checks it by
  hand in the dev copy.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A32-native-show-hide-animation.md`: split, files changed, why Windows didn't animate the pad and
  what change makes it, which native show and hide animations now play, what the user must check by hand in the dev copy
  (every show/hide route, Drop down, Reduce motion, Windows "Animation effects" off, all three backgrounds and taskbar modes;
  marked "needs a live check"), anything not done.
