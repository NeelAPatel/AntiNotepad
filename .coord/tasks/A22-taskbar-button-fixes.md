# A22-taskbar-button-fixes

You are the director (gpt-6.1-sol medium). Plan, split and check the work; delegate to gpt-6-luna xhigh subagents as you see fit
(say at the top of the report if spawning isn't available). Repo: this folder, a git worktree on branch
`feat/A22-taskbar-button-fixes`. Never switch branch, merge or commit (.git is read-only); the manager reviews and commits.
App: `product/AntiNotepad` (Avalonia 12.1.4 + AvaloniaEdit 12, .NET 10). Read first: `AGENTS.md`, `docs/dev-loop.md`,
`docs/decisions.md`, `.coord/reports/A20-taskbar-icon-setting.md` (the design you are fixing), `Platform/TaskbarPresence.cs`,
`UI/PadWindow.cs`. First-party .NET/Avalonia/Win32 only.

## What the manager saw live (Windows 11, dev copy built from dev, 2026-10-24)
Setting: Settings > System > Taskbar icon (While visible / Always / Never).
1. **The real pad never gets a taskbar button.** With **While visible** or **Always**, showing the pad (hotkey, closing Settings,
   startup) leaves the taskbar with no AntiNotepad button, even with the pad visible and foreground. Window dump of the dev process
   while visible + focused: pad HWND `vis=True min=False ex=0x200100 owner=<Avalonia offscreen owner HWND>`; stand-in
   `vis=False`. So `ITaskbarList.AddTab` on the owned pad window has no visible effect (the shell skips owned windows / windows
   without WS_EX_APPWINDOW, or the tab is removed again). Net effect: While visible behaves like Never; Always only shows a button
   while the pad is hidden.
2. **The stand-in (Always, pad hidden) works:** it appears as a minimized button with the blue dev icon; clicking it shows and
   focuses the pad. Keep that behavior.
3. **Layout:** the "Taskbar icon" row renders *inside* the "Open at" expander (indented, no icon, below the five position tiles), so
   it reads as an Open at option. It must be its own top-level row in the System section's Opening card (or the Window card),
   with an icon, like its neighbors (e.g. "Fresh note after").
4. **Metric:** `hotkey-to-focus` logged `ms=10881.8 sync=10867.8` for a show triggered by the taskbar stand-in: the timer starts at
   the last hotkey press, not at the stand-in click. Start the measurement at the actual trigger for non-hotkey shows (or don't log
   it for them).

## Fix requirements
- One consistent approach that really shows exactly one button for the pad: visible (While visible, Always) and hidden (Always),
  no duplicates, no flash, no focus steal, no window jump, no hide/re-show on mode change. Options to evaluate: reuse the stand-in
  window as the one taskbar button in both states (it already works; clicking it while the pad is visible+focused hides the pad,
  otherwise shows/focuses it), or make the real pad eligible (owner/WS_EX_APPWINDOW) if that can be done without Avalonia's
  hide/re-show. Explain the choice with evidence (Avalonia 12.1.4 source, Microsoft docs).
- The button uses the right icon and title (`AppInfo.DisplayName`, dev = "AntiNotepad (dev)" + blue icon, prod = amber).
- Never mode: no button at all (as now). Tray unchanged in all modes.
- Settings window keeps its own button (existing behavior).
- Keep show/hide speed: nothing heavy on the hotkey path.

## Files in scope
`Platform/TaskbarPresence.cs`, `UI/PadWindow*.cs`, `UI/SettingsWindow.cs` (the Taskbar icon row only), `SelfTest.cs` (extend the
existing TaskbarChecks or add ONE method at the end), `docs/decisions.md` (update the taskbar line).

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a22-check` (add `-p:NuGetAudit=false` only if
  NU1900 appears because nuget.org is unreachable): 0 errors, 0 warnings; `--selftest` SELFTEST PASS. Delete the folder if you can.
- Never launch AntiNotepad except `--selftest`; never kill it; never touch `%LOCALAPPDATA%\AntiNotepad`,
  `%LOCALAPPDATA%\Programs\AntiNotepad` or Documents. Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A22-taskbar-button-fixes.md`: split, root cause with evidence, design as built, files, selftests, and a
  precise live-check list (the manager will test it live with computer use this time).
