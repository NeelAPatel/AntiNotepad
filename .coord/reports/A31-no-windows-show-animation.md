# A31-no-windows-show-animation

## Work split

- Director (`gpt-6.1-sol`, medium): required reading, native/framework source review, diff review, Release build, selftest, cleanup attempt, and this report.
- `a31_native` (`gpt-6-luna`, xhigh): implemented the native helper and its two constructor call sites; reviewed the floating pager companion.

## Files changed

- `product/AntiNotepad/Platform/Native.cs`: `DWMWA_TRANSITIONS_FORCEDISABLED` (3) and `DisableDwmTransitions(TopLevel)`, using the existing four-byte DWM P/Invoke helper with TRUE (1). Missing handles/unavailable DWM safely leave window operation intact, following existing cosmetic native-helper behavior. HRESULTs are ignored as in the existing DWM helper; this is not runtime confirmation of success.
- `product/AntiNotepad/UI/PadWindow.cs`: one constructor call after `ShowInTaskbar = false`, before the first show or state restoration. No save/log edits.
- `product/AntiNotepad/Platform/TaskbarPresence.cs`: one call after constructing the stand-in, before native showing or any activation handler can run.
- This report. `SelfTest.cs` is unchanged: compositor animation needs a real HWND/compositor and visual observation; a constant or source-shape assertion would not verify it headlessly.

## Animation sources and suppression

Source inspection identifies the native transition entry points; no animation was observed live in this run. [Microsoft documents the attribute](https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwmwindowattribute) as disabling DWM transitions when its BOOL value is TRUE. The setting belongs to each HWND, independently of Reduce motion or system-wide preferences.

1. **Pad open/close transitions:** resident show/hide, hidden prewarm, and quiet timer show ultimately call native `ShowWindow`. The pad's lifetime-long flag disables the compositor transition at those calls, including the first show. Hotkey, tray, taskbar, `--show`, new-note hotkey, blur, Escape, and canceled Alt+F4 close converge on this same pad.
2. **Restore/minimize transitions:** the pad retains its existing `WindowState.Normal` assignment; the stand-in retains `SW_SHOWMINNOACTIVE`, `SW_HIDE`, and restore interception. Both HWNDs are configured before these paths can run, covering DWM transitions without changing taskbar click attribution, coalescing, or focus handling. No system animation setting or window owner/style rewrite is introduced.
3. **Opacity/style effects:** no `AnimateWindow` call or separate OS fade request was found in product source. Pad content fade/rise (140 ms), drop-down position slide (150 ms), pinned dim fade (160 ms), and prewarm opacity are application-controlled and unchanged. Acrylic/mica/solid configuration and corner/border attributes are unchanged.
4. **Floating pager companion:** its native host is Avalonia's undecorated, non-minimizable `WS_POPUP`/`WS_EX_TOOLWINDOW`, shown without activation. No ordinary main-window minimize/restore or explicit OS fade path was found there. Its existing content-opacity fade stays; no popup or Layout change was made. Confirm this source assessment live in Float below mode. Settings and other independent secondary windows are out of scope and untouched.

Framework evidence: [Avalonia 12.1.4 WindowImpl](https://raw.githubusercontent.com/AvaloniaUI/Avalonia/12.1.4/src/Windows/Avalonia.Win32/WindowImpl.cs) creates the HWND during construction and maps normal-state showing to native normal/restore commands; [PopupImpl](https://raw.githubusercontent.com/AvaloniaUI/Avalonia/12.1.4/src/Windows/Avalonia.Win32/PopupImpl.cs) supplies the popup styles and nonactivating show path. Constructor placement therefore precedes prewarm and visible startup. The flag adds no calls to hotkey toggles; motion, focus retries/timing log, taskbar policy, and shutdown logic retain their existing code.

## Headless verification

- Requested `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a31-check`: succeeded, **0 errors, 2 NU1900 warnings**, because `https://api.nuget.org/v3/index.json` was unreachable for vulnerability data. The exact zero-warning gate is therefore not satisfied without the following environment workaround.
- Repeat with the same telemetry setting and **`-p:NuGetAudit=false`**: succeeded, **0 warnings, 0 errors**. No project/package configuration was changed. Vulnerability auditing was not completed.
- `build/a31-check/AntiNotepad.exe --selftest`: **SELFTEST PASS**, exit 0.
- `git diff --check`: PASS.
- Cleanup was attempted with resolved absolute target validation against this worktree's `build/a31-check`. Automatic approval review rejected recursive removal before execution with reason **blocked by policy**. `build/a31-check` remains for manager cleanup.

## User dev-copy checklist — needs a live check

Every item below **needs a live check**, performed by the user in the isolated dev copy per `docs/dev-loop.md`. Repeat with Reduce motion **off and on**. Windows' own zoom/fade/minimize/restore transition should be absent in both cases; the existing content fade/rise and drop-down slide should remain when Reduce motion is off and be skipped when on.

1. Hidden prewarm startup: no flash or focus theft; first hotkey press shows promptly and focuses the editor. Compare later presses and the `hotkey-to-focus` log with the baseline.
2. Show/hide repeatedly by the main hotkey; show via tray and hide via tray/menu. Test `--show` against the running dev instance with its isolated data identity, and the new-note hotkey. Check ordinary visible startup too.
3. Taskbar **Always**: hidden click shows/focuses; focused click hides; inactive visible click focuses. Repeat rapidly and with taskbar previews/grouping. **While visible**: one button while shown, none hidden; focused click hides. **Never**: no pad button. No proxy flash, duplicate button, unwanted restore, or activation race.
4. Hide on blur, Escape (without an overlay), and Alt+F4: immediate resident hide. Reopen through every available route; preserve note content and focus.
5. Repeat each show/hide route with Open at **remember, center, mouse, tray, and Drop down**, including mixed-DPI/multiple monitors if available. Only the app's drop-down slide should move the window with Reduce motion off.
6. Keep on top and Dim when pinned: activation/deactivation opacity still changes as before. Check hide-on-blur on/off and Settings-focus exception. Repeat with acrylic, mica, and solid backgrounds; Windows 11 corners/border stay correct.
7. Float below pager visible during hide/reopen: no separate OS window animation or orphan popup; its own existing fade remains. Also check docked/overlay bars, split view, and nonactivating finished-timer show.
8. A26 regression: normal Alt+F4 hides; user-initiated dev shutdown/session-end check still saves and exits without veto. No shutdown was initiated here.

## Boundaries / not done

No GUI/computer-use testing, ordinary app launch, process stop, installed upgrade, real-data access, branch switch, merge, commit, STATUS edit, or audit edit. Only the check binary's `--selftest` was launched. Native DWM success, animation appearance, focus, physical timing, shell behavior, and regression checklist remain unverified until the user checks the dev copy. The exact requested build's NuGet warning and policy-blocked cleanup are the remaining headless-gate qualifications.
