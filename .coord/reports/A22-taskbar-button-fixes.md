# A22-taskbar-button-fixes

## Work split

- Director: required reading, primary-source review, integration review, focused selftests, decisions entry, build/checks and this report.
- `a22_native` (`gpt-6-luna`, xhigh): native taskbar helper, pad lifecycle and hotkey measurement correction.
- `a22_settings` (`gpt-6-luna`, xhigh): Taskbar icon row only.
- `a22_review` (`gpt-6-luna`, xhigh): independent read-only lifecycle/native review.

No branch switch, merge or commit; no STATUS/audit edit; no GUI launch or process stop; no installed-copy or real-data access. Only `--selftest` is permitted for execution. This is the review build for the manager's live gate.

## Root cause and evidence

The manager's Windows 11 observation is the runtime evidence: the visible, focused pad had an Avalonia offscreen owner and extended style `0x200100` (no `WS_EX_APPWINDOW`), and `ITaskbarList.AddTab` produced no button. The hidden Always stand-in already worked. A20's bookkeeping marked the real pad added after the COM call returned, without evidence that Explorer retained a button.

[Avalonia 12.1.4 WindowImpl.cs](https://raw.githubusercontent.com/AvaloniaUI/Avalonia/12.1.4/src/Windows/Avalonia.Win32/WindowImpl.cs), `ShowTaskbarIcon` and `UpdateWindowProperties` (lines 794-800, 1298-1339), explains the owner/style state: disabling taskbar presence attaches the offscreen owner and clears `WS_EX_APPWINDOW`; enabling it clears that owner with an explicit hide/show for a visible HWND. Direct native owner/style patches would compete with this framework state.

[Microsoft taskbar documentation](https://learn.microsoft.com/en-us/windows/win32/shell/taskbar#managing-taskbar-buttons) describes unowned `WS_EX_APPWINDOW` eligibility and hiding before changing styles. This supports avoiding a live pad eligibility rewrite; the manager's observation establishes the actual A20 failure. The chosen approach reuses the already-working unowned stand-in as the sole button in both states, while keeping the real pad's framework taskbar setting false.

The Settings row was already a separate sibling in `Card`, outside `Expander`'s visibility-controlled children. Its missing icon made it visually read as a nested option. It now uses the existing Fluent desktop/System glyph, normal row padding and divider, alongside Fresh note after. Expanding/collapsing Open at must leave it visible.

The old hotkey timestamp was stored even for a hotkey hide and consumed only on a later show. A stand-in click could therefore report time elapsed since that earlier hotkey instead of its own trigger.

## Design as built

`TaskbarProxy` is now the only pad button. The real pad stays `ShowInTaskbar=false`; `TaskbarItem` and its COM registration are removed. The stand-in is created at hook installation or a non-Never Settings change, outside the hotkey show path, and reused until Quit. Policy: While visible shows it only with the pad; Always keeps the same stand-in shown through both states; Never hides it. Reconciliation does not rewrite the pad owner/styles, hide/re-show the pad, or move its bounds. Tray and Settings' independent taskbar presence are unchanged.

The stand-in receives `AppInfo.DisplayName` and the pad's `AppInfo.IconUri` icon: dev identity remains `AntiNotepad (dev)`/blue; ordinary builds remain `AntiNotepad`/amber. It stays minimized, offscreen and transparent. [ShowWindow](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow) documents that `SW_SHOWMINNOACTIVE` displays a minimized window without activating it. Button reconciliation performs only guarded native show/hide calls; repeated Always-mode pad toggles leave the stand-in alone.

The proxy intercepts [SC_RESTORE](https://learn.microsoft.com/en-us/windows/win32/menurc/wm-syscommand), snapshots hide/show intent before queuing it, and handles activation as a fallback. A pad-side native hook captures the transfer to the proxy before Avalonia raises Deactivated; [WM_ACTIVATE](https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-activate) supplies the counterpart HWND. Direct [GetForegroundWindow](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getforegroundwindow) and the snapshot distinguish the focused pad from an inactive pad. The queued action hides a visible focused pad, otherwise shows/focuses it. Duplicate restore/activation notifications coalesce while that action is pending; a presence generation discards a queued action if the proxy was hidden/re-shown meanwhile. Hide-on-blur uses a cancellable dispatcher callback with fresh visibility/focus/settings checks, preventing an earlier blur from undoing the taskbar action. Explorer's TaskbarCreated notification refreshes a shown proxy; Quit removes both hooks and closes it.

Hotkey timestamps are now arguments owned by the individual hotkey show invocation and its deferred log callback. A hotkey hide stores no timestamp. Public/tray/taskbar/startup/Settings shows pass zero and emit no `hotkey-to-focus` measurement. Heavy setup/COM work is absent from the hotkey path. Native click semantics, lack of flicker and measured responsiveness still require the live gate below.

## Files

- `product/AntiNotepad/Platform/TaskbarPresence.cs`
- `product/AntiNotepad/UI/PadWindow.cs`
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs` (Taskbar icon glyph only)
- `product/AntiNotepad/SelfTest.cs` (existing TaskbarChecks only)
- `docs/decisions.md` (taskbar line)
- This report.

## Headless verification

- Final `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a22-check`: **PASS**, 0 warnings, 0 errors. No NuGetAudit override needed.
- Final `build/a22-check/AntiNotepad.exe --selftest`: **SELFTEST PASS**, exit 0.
- `git diff --check`: **PASS**. TaskbarPresence.cs restored to the repository's CRLF convention after verification; no source logic changed afterward.
- Extended the existing TaskbarChecks only: retained Never/missing-property defaults, JSON round trips and all six visibility states; added missing/unknown mode fallback and all four click state combinations, including a hidden pad with stale foreground state. These checks construct no windows and establish policy only, not native shell behavior. Existing identity checks load both bundled icons; the required Release build uses ordinary production identity.
- Independent static review found and resolved the lost non-hotkey deferred editor-focus retry. Director also added the queued blur's Settings-window recheck and restored the already-shown proxy guard, avoiding redundant native calls on Always-mode show/hide. No remaining blocking static finding was identified; rapid click coalescing remains explicitly in the live gate.
- Cleanup was attempted with resolved absolute target validation restricted to this worktree's `build/a22-check`. Automatic approval review rejected the recursive removal command before execution, with reason **blocked by policy**. `build/a22-check` remains for manager cleanup.

## Manager live checklist

All items below remain unverified by headless tests. Use only the isolated dev copy and throwaway data per `docs/dev-loop.md`; installed copy and real notes remain untouched.

1. Open Settings > System. Taskbar icon has an icon, normal top-level alignment and a divider. Collapse and expand Open at: only the five position tiles toggle; Taskbar icon remains visible. Confirm all three choices fit and Settings retains its separate button.
2. Never: hotkey/tray/startup/Settings-close show leaves zero pad buttons; Escape, close and hide-on-blur leave zero. Tray continues to work.
3. While visible: each show produces exactly one pad button; every hide removes it. Cover hotkey, tray, taskbar, timer quiet show, Escape, close and blur. Show from hidden using the tray because this mode has no hidden button.
4. Always: exactly one pad button while hidden AND visible. Record the stand-in HWND: repeated show/hide must keep that same button/HWND, without a removal/re-add handoff. Click while pad focused => hide; click while hidden => show/focus editor; click with another app or Settings focused => focus pad. Repeat rapidly, with grouped buttons and taskbar previews.
5. With pad visible and Settings focused, switch all six directed pairs of modes. Check immediate expected button count, unchanged foreground HWND, no flash, hide/re-show, bounds jump or z-order jump. Repeat hidden states with Settings still open. The pad must never acquire a second native button.
6. Repeat click tests with hide-on-blur enabled/disabled and pinned enabled/disabled (including dim when pinned). Watch specifically for a click-to-hide being undone by a later show, or click-to-show being undone by blur. Check split view and all Open at placements, especially Drop down, preserve expected bounds.
7. Save each mode; restart the dev copy normally and with `--hidden`. Invisible prewarm must not flash a While visible button or steal focus. Always must have one hidden button. Quiet timer show must retain the other application's foreground focus.
8. Inspect taskbar title/icon: dev is `AntiNotepad (dev)` with blue icon; Settings has its own button. Confirm production identity from a separately staged ordinary build if desired: `AntiNotepad`, amber icon. Do not upgrade or launch the installed copy for this task.
9. Log regression: hotkey hide, wait ~10 seconds, taskbar show (Always), then tray/Settings show. None of these non-hotkey shows may emit stale `hotkey-to-focus`. A subsequent hotkey show must report only its own press, including fast repeated presses; compare responsiveness with baseline.
10. Restart Explorer during a dev session; verify the shell reconstructs exactly the expected sole stand-in button. Close Settings and quit the dev copy normally: no orphan button/helper remains. Native shell recovery, icons, focus, animation and physical performance require this live gate.
