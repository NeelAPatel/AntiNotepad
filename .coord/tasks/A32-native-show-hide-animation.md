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
Windows' own native animations, per route, exactly as the user specified:

| Route | Animation |
|---|---|
| Starting the program from not running (when it opens the pad visibly) | native **open** |
| Minimize (Win+Down, the window's system minimize, any minimize request) | native **minimize** |
| Clicking the pad's taskbar button (Taskbar icon While visible / Always) | native **minimize** when it hides a focused pad, native **restore** (un-minimize) when it shows it |
| Clicking the system tray icon (a show/hide toggle) | native **open** to show, native **close** to hide |
| The hotkey (and the new-note hotkey, `--show`) | native **open** to show, native **close** to hide |
| Not specified by the user: Esc, Alt+F4 hide, hide on blur, tray menu "Hide" | treat as the hotkey's native **close**; say so in the report |

So the pad has two hidden states with different animations: closed (hotkey, tray, Esc…) and minimized (minimize, taskbar
click). Define the state machine explicitly (e.g. what the hotkey does when the pad is minimized: restore with the native
restore animation is fine) and put it in the report.
Today (after A31) show plays the app's own content fade/rise (`PadWindow.PlayShow`), which the user calls "some weird
non-windows animation", and hide plays nothing. Before A31 there was most likely no native animation either, so this is new
work, not just a revert.

The taskbar row is the hard part: today the pad's taskbar button is a separate minimized stand-in window
(`Platform/TaskbarPresence.cs`, `docs/decisions.md`), and the real pad stays off the taskbar. A minimize/restore animation
that flies to and from the button needs Windows to treat the pad as that button's window. Find the smallest correct way
(e.g. the pad itself on the taskbar in While visible / Always, a different stand-in relationship, or another documented
mechanism), keep A22-A25's click behavior (focused click hides, hidden click shows, inactive click focuses) and the Never
mode's "no button". If a fully native taskbar minimize/restore isn't achievable without breaking those, implement every
other row, explain exactly why in the report, and propose the trade-off for the user to choose.

## Direction
1. Revert A31: remove `Native.DisableDwmTransitions` and its two call sites (`UI/PadWindow.cs` constructor,
   `Platform/TaskbarPresence.cs`).
2. Find out why Windows doesn't animate the pad's `ShowWindow` show/hide: DWM plays open/close transitions only for top-level
   windows with certain styles (e.g. a caption/system-menu frame style; a pure `WS_POPUP`, tool window, layered or cloaked window
   may get none). Avalonia's frameless, transparent, `ExtendClientAreaToDecorations` window and `ShowInTaskbar = false` (and
   any owner set by the taskbar stand-in) all matter. Determine which style or setting suppresses it, and change the smallest
   thing that makes Windows animate while the pad still looks exactly as today: no title bar, no visible frame or border
   change, same corners, acrylic/mica/solid unchanged, still off the taskbar unless the Taskbar icon setting says otherwise.
3. Use the animation each route in the table asks for. Don't fake any of them with an app-drawn animation.
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
`Platform/Native.cs`, `UI/PadWindow.cs` and its partials (show/hide/minimize paths and constructor only),
`Platform/TaskbarPresence.cs`, the tray and startup show path (`Program.cs`, `Platform/Tray*` as needed; nothing else in them),
`docs/decisions.md` (update the taskbar stand-in entry if its design changes), `SelfTest.cs` (ONE new method at the END plus one call line in `Run`,
only if something is checkable headless, e.g. the window styles applied; otherwise none, say so).

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a32-check`: 0 errors, 0 warnings;
  `build/a32-check/AntiNotepad.exe --selftest` prints SELFTEST PASS. Delete `build/a32-check` if you can.
- Never launch AntiNotepad any other way, never kill it, never read or write `%LOCALAPPDATA%\AntiNotepad`,
  `%LOCALAPPDATA%\Programs\AntiNotepad` or the real Documents folder. No desktop/computer-use testing; the user checks it by
  hand in the dev copy.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A32-native-show-hide-animation.md`: split, files changed, why Windows didn't animate the pad and
  what change makes it, the route-by-route table as built (with the state machine), what the user must check by hand in the
  dev copy (every row of the table, Drop down, Reduce motion, Windows "Animation effects" off, all three backgrounds and
  taskbar modes; marked "needs a live check"), anything not done, and any trade-off the user must decide.
