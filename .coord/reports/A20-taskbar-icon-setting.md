# A20-taskbar-icon-setting

## Work split
- Director: inspected startup, tray, Settings, and lifecycle paths; added the single logic selftest; integrated and checked the implementation; ran the release build and selftest; wrote this report.
- `setting_row` (`gpt-6-luna`, xhigh): only the new Store property, the Settings row/callback, and the decisions entry.
- `window_taskbar` (`gpt-6-luna`, xhigh): PadWindow lifecycle and the native taskbar helper.
- `review_native_options` (`gpt-6-luna`, xhigh): independent read-only review of native hidden-window behavior.

No branch switches, merges, commits, application launches other than `--selftest`, process stops, installed-data access, STATUS edits, or audit edits were performed.

## Root cause and source review
The previous `PadWindow` constructor unconditionally set `ShowInTaskbar = false`. This applies equally to dev and installed copies. There were no separate taskbar-style overrides in `Platform/`. The tray is independent: `Tray` creates its own `TrayIcon` with `IsVisible = true`, using the same bundled icon as the pad and executable. It is disposed only during Quit.

Before this change, the dev marker affected the pad's title suffix and toolbar badge, plus existing dev tooling; neither suppressed the taskbar. Custom data paths and dev hotkeys select isolation, not a different taskbar policy. The pad already had a `WindowIcon` from `AppInfo.IconUri`. Its title is now exactly `AppInfo.Name` in both copies, matching the proxy title; the existing dev toolbar badge remains. Settings already uses its own unowned `window.Show()` and leaves Avalonia's `ShowInTaskbar` default enabled; its existing separate taskbar button is retained.

The exact package is Avalonia **12.1.4**. Its [WindowImpl source](https://raw.githubusercontent.com/AvaloniaUI/Avalonia/12.1.4/src/Windows/Avalonia.Win32/WindowImpl.cs) shows `ShowTaskbarIcon` forwarding to `UpdateWindowProperties`. When enabled after a window has acquired Avalonia's offscreen owner, it hides the native window, clears the owner, then shows the same HWND again. This does not recreate the HWND, but does violate the desired no-hide/re-show live setting path. The false path uses an offscreen owner and clears `WS_EX_APPWINDOW`; it does not set `WS_EX_TOOLWINDOW`.

Microsoft documents [taskbar eligibility](https://learn.microsoft.com/en-us/windows/win32/shell/taskbar#managing-taskbar-buttons) and that [ITaskbarList.AddTab](https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-itaskbarlist-addtab) accepts any type of window. Source review establishes the API behavior and framework risk; it does not establish Windows 11's live shell behavior for the implementation below.

## Design as built
Default: **Never**, preserving the existing tray-only behavior for new settings and old JSON without the property. The Settings segmented control offers **While visible**, **Always**, and **Never**, with a description explaining that the tray remains available. Changes use the existing Set/persistence path and apply the taskbar policy immediately.

The pad keeps `ShowInTaskbar = false`; `ITaskbarList.AddTab/DeleteTab` controls its registration without invoking Avalonia's live hide/re-show setter or rewriting its styles/owner. While visible uses the real pad's HWND and bundled icon/title. Always uses that same button while shown and a stable minimized, nonactivating taskbar proxy while hidden. The proxy is created during startup with saved Always, or when selecting Always in Settings, and then reused until Quit. COM initialization also happens at startup/settings change, outside the hotkey path. Restoring the proxy routes to the existing pad show/toggle path; minimizing the focused pad from the taskbar routes to the existing resident-hide path. Never removes pad/proxy taskbar presence. The tray is unchanged in every mode.

The real button is removed before hiding the pad or showing the proxy, and the proxy is hidden before showing/registering the pad, avoiding concurrent pad/proxy buttons. `ShowResident`, `ShowQuietly`, `HideResident`, and prewarm completion reconcile the selection. Prewarm does not register the invisible pad: its final hide installs the hidden state, including the proxy for Always. Run-at-login's `--hidden` uses that same prewarm path. Existing split sizing, Open at positioning, pinned/topmost logic, Settings-focus exception, and hide-on-blur logic all continue to use the same pad and existing show/hide methods. Settings changes, reset, and Undo reconcile through `SettingsChanged`; existing delayed settings saving persists the property. Quit removes the manual button, releases COM, removes hooks, and closes the helper.

The proxy was chosen after an independent review found no documented click-to-show guarantee for `AddTab` on an `SW_HIDE` HWND. It uses minimized-window restoration rather than relying on hidden-window activation. [ShowWindow](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow) documents `SW_SHOWMINNOACTIVE` as showing a minimized window without activation.

The proxy uses an Avalonia Window for native lifetime, icon, title, and message hooks, with direct native show/hide calls and its own shown flag. It never calls Avalonia's logical `Show()` or renders note content. This intentionally avoids Avalonia's minimized Show path, which uses `SW_MINIMIZE`. Avalonia [Window's constructor](https://raw.githubusercontent.com/AvaloniaUI/Avalonia/12.1.4/src/Avalonia.Controls/Window.cs) immediately creates its platform implementation, and Win32's implementation creates the HWND in its constructor; a preliminary Show is unnecessary. The helper remains minimized, offscreen, transparent, and nonactivating, and intercepts restore before normal presentation.

The visible pad is activated and focused before its taskbar registration call. Explorer's `TaskbarCreated` notification resets the COM connection and re-adds the wanted real-pad button; the proxy uses ordinary native taskbar eligibility. Failed COM initialization releases the object as required by [HrInit documentation](https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-itaskbarlist-hrinit), and shell failures are logged without repeated identical messages. Failures can prevent a requested button change; they do not interrupt the pad's show/hide flow.

## Files changed
- `product/AntiNotepad/Model/Store.cs`: only `Settings.Taskbar` near window settings.
- `product/AntiNotepad/UI/SettingsWindow.cs`: only the Taskbar icon row and Set callback near Open at; search code unchanged.
- `product/AntiNotepad/UI/PadWindow.cs`: lifecycle integration.
- `product/AntiNotepad/UI/PadWindow.Keys.cs`: taskbar setup/reconciliation for settings changes, reset, and Undo.
- `product/AntiNotepad/Platform/TaskbarPresence.cs`: taskbar mapping and native integration.
- `product/AntiNotepad/SelfTest.cs`: one new method at the end, one call in Run.
- `docs/decisions.md`: taskbar default/choice entry.
- This report.

## Headless checks
- Final `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a20-check`: **PASS**, 0 warnings, 0 errors. NuGet audit did not need disabling.
- Final `build/a20-check/AntiNotepad.exe --selftest`: **SELFTEST PASS**, exit 0.
- `git diff --check`: **PASS**.
- Cleanup was attempted with a resolved-path equality check before `Remove-Item -LiteralPath ... -Recurse -Force`. Automatic approval review rejected that command as **blocked by policy** before execution; `build/a20-check` remains.

The new selftest checks new settings default to Never, missing-property JSON defaults to Never, all three values survive a StoreData JSON round trip, and the explicit six-case table for Never/While visible/Always with the pad hidden/shown. It constructs no windows. Existing selftests continue to run.

## Needs a live check
Every item below **needs a live check**. No GUI or shell behavior has been exercised this round.

1. In an isolated dev copy launched through `tools/dev.ps1`, open Settings > System and verify Taskbar icon appears once, its three Seg options fit, and its description is readable. Verify Settings has exactly its pre-existing separate button.
2. Never: show/hide repeatedly using the dev hotkey, tray, Escape, close, and hide-on-blur. Confirm no pad button and the tray remains available.
3. While visible: each show adds exactly one pad button; every hide removes it. Repeat via hotkey, tray, timer's quiet show, close, Escape, and hide-on-blur. Watch for button flicker during `--hidden` prewarm.
4. Always: while hidden, verify exactly one pad button remains. Click it to show and focus the editor. Click the focused visible pad's button to hide it. With another app focused, click the pad button to focus the pad without hiding it. Repeat several times rapidly and through taskbar previews and grouped buttons.
5. Change between all three modes while the pad is visible and Settings has focus. Verify immediate changes without any pad flash, focus theft, z-order change, position/size jump, or duplicate pad button. Hide with Settings still open and recheck the hidden states.
6. Persist each mode, exit normally, restart the isolated dev copy normally and with `--hidden` using throwaway data. Never and While visible should have no pad button while hidden; Always should have one from startup. No invisible prewarm activation or taskbar flash.
7. Repeat visible/hidden and taskbar click checks with split view on/off, pinned on/off, hide-on-blur on/off, dim-when-pinned on/off, and each Open at position (including dropdown). Bounds must remain correct and foreground applications must retain focus during quiet show/prewarm.
8. Compare repeated hotkey show/hide responsiveness with the baseline; no layout/render/allocation regression is claimed from headless checks. Restart Explorer and verify the selected button state recovers without duplication.
9. When separately authorized to upgrade, repeat the same checks on the installed copy to confirm parity. This task does not install, launch, stop, or inspect the installed copy or real notes.

## Limitations / not done
Native shell click behavior, icons/titles as displayed by Explorer, focus, z-order, animation, physical timings, and dev/installed GUI parity need the live checks above. No installed upgrade or live UI tests were attempted, as instructed.

Implementation and headless gates are complete; no known source-level defect remains from the two review passes. This is not a claim of flawless live Windows behavior: the manager explicitly cannot run the live checks this round. No packages were added, other Settings defaults/welcome notes were untouched, and no Settings search code was changed.
