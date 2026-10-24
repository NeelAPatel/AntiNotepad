# A20-taskbar-icon-setting

You are the director (gpt-6.1-sol medium). Plan, split and check the work; delegate implementation and investigation to
gpt-6-luna xhigh subagents as you see fit. If subagent spawning is not available in this run, say so at the top of your report
and do the work yourself.

Repo: this folder, a git worktree on branch `feat/A20-taskbar-icon-setting`. Never switch branch, merge or commit (.git is
read-only); the manager (a Claude session) reviews and commits. Subagents share this worktree: give each disjoint files.
App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10). Read first: `AGENTS.md`, `docs/dev-loop.md`,
`docs/decisions.md`. Match the surrounding style, naming and comment density. First-party .NET/Avalonia/Win32 only, no packages.

## The user's report and request
- "When Claude or something launches the dev application, there is no icon in the taskbar." Today `UI/PadWindow.cs` sets
  `ShowInTaskbar = false`, so the app only lives in the system tray. Confirm that is the whole cause (also check the tray icon,
  window styles such as WS_EX_TOOLWINDOW / WS_EX_APPWINDOW in `Platform/`, the dev badge, and anything that differs for the dev
  copy) and say what you found.
- Add a Settings choice, "Taskbar icon", with three options. The tray icon stays in every option.
  1. **While visible**: the taskbar button shows while the pad is on screen and goes away when it hides.
  2. **Always**: the taskbar button shows whenever AntiNotepad is running, including while the pad is hidden (clicking it shows
     the pad; clicking it while the pad is visible and focused hides it, like the hotkey).
  3. **Never** (today's behavior): tray only.
  Pick the default that matches today's behavior (Never) unless you find a strong reason otherwise; say which.

## Requirements ("should exist and work flawlessly")
- New `Settings` property (e.g. `Taskbar` = "never" | "visible" | "always") in `Model/Store.cs`. Another worker changes two other
  `Settings` defaults (`Scrolling`, `Swipe`) and the welcome notes; only add your property near related window settings.
- Settings row in `UI/SettingsWindow.cs`, in the section with window/tray behavior (near "Open at", hide on blur, or similar),
  using the existing `Seg` segmented control, with a one-line description. Another task (A18) edits the Settings *search* code in
  the same file: keep your edit to the one row and its Set callback.
- Changing it applies live, without a restart, and survives restart.
- Show/hide speed is the product's priority: do not add work to the hotkey show/hide path beyond flipping the taskbar state, and
  do not cause a visible flash, focus steal, or the window jumping. Check `ShowResident`, hide, `Prewarm`, `--hidden` start
  (Run at login), split view, Settings window, and the pinned/hide-on-blur paths.
- The taskbar button must use the app icon, have the app name as its title, and not duplicate (one button only). The Settings
  window should not get its own extra taskbar button unless it already does today (report).
- Changing `ShowInTaskbar` on a live Win32 window can recreate state or reorder z-order in some frameworks: verify how
  Avalonia 12 applies it (source or docs) and use the lightest correct approach (e.g. toggling WS_EX_APPWINDOW /
  WS_EX_TOOLWINDOW via `Platform/` Win32 helpers) if needed.
- Dev copy and installed copy behave the same.

## Files in scope
`UI/PadWindow*.cs`, `Platform/*` (only what's needed), `Model/Store.cs` (the new property only), `UI/SettingsWindow.cs` (the one
row), `UI/SettingsParts.cs` (only if a helper is needed), `SelfTest.cs` (ONE new method at the END with one call line in `Run`),
`docs/decisions.md` (one line about the choice).

## Selftest
Logic-level checks only (no visible windows): the setting round-trips through JSON with the right default; the mapping from
(setting, pad visible) to "show taskbar button" is right for all six combinations.

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a20-check`: 0 errors, 0 warnings
  (if NU1900 appears only because nuget.org is unreachable, add `-p:NuGetAudit=false` and say so).
  `build/a20-check/AntiNotepad.exe --selftest` prints SELFTEST PASS. Delete `build/a20-check` if you can.
- Never launch AntiNotepad any other way, never kill it, and never read or write `%LOCALAPPDATA%\AntiNotepad`,
  `%LOCALAPPDATA%\Programs\AntiNotepad` or the real Documents folder.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A20-taskbar-icon-setting.md`: how you split the work, the root cause, files changed, design as
  built (how each option is applied and when), the default, selftest list, a precise live-check list for a person (the manager
  cannot do live UI tests this round; mark those "needs a live check"), anything not done.
