# A32-native-show-hide-animation

## Result and work split

Implemented native transition routing for manager review on `feat/A32-native-show-hide-animation`. Native animation appearance and shell behavior **need a live check**; no desktop observation was permitted. The current user message supersedes the older checked-in brief: taskbar/system minimize and restore are distinct from hotkey/tray open and close.

- Director (`gpt-6.1-sol`, medium): required reading, first-party source review, architecture and diff review, integration checks, decisions update, and this report.
- `a32_native_research` (`gpt-6-luna`, xhigh): native style/suppression helpers, system animation preference, nonactivating native commands, the single new selftest method, and a second static lifecycle review.
- `a32_lifecycle` (`gpt-6-luna`, xhigh): pad lifecycle, real-pad taskbar presence and closed-state proxy, tray toggle, and related partials.

## Why transitions were absent and what changed

A31 definitely suppressed DWM transitions on both HWNDs. Its `DisableDwmTransitions` helper and both original calls are removed. Suppression is now temporary, only for invisible preparation, quiet shows, and exit.

There is a second source-based eligibility issue: Avalonia 12.1.4 builds `BorderOnly` windows without `WS_CAPTION` or `WS_SYSMENU`; setting `ShowInTaskbar=false` also assigns an offscreen owner. The previous taskbar button belonged to a different minimized HWND, so it could not supply the real pad's normal minimize/restore relationship. See [Avalonia WindowImpl](https://raw.githubusercontent.com/AvaloniaUI/Avalonia/12.1.4/src/Windows/Avalonia.Win32/WindowImpl.cs) and [Microsoft taskbar eligibility](https://learn.microsoft.com/en-us/windows/win32/shell/taskbar).

The pad now retains ordinary caption/system-menu/minimize/maximize style bits through Avalonia's public `AddWindowStylesCallback`, rather than a one-time style patch that a framework update would overwrite. `WindowDecorations.BorderOnly` and extended client area remain. Avalonia's existing [extended-frame WndProc](https://raw.githubusercontent.com/AvaloniaUI/Avalonia/12.1.4/src/Windows/Avalonia.Win32/WindowImpl.AppWndProc.cs) already removes the native caption area and retains resize-border handling; no custom nonclient drawing or full-client override is introduced. Material, corner and border configuration is retained. Exact visual parity, including insets and mixed DPI, **needs a live check**.

The pad is permanently framework-taskbar-enabled and unowned; `ITaskbarList.AddTab/DeleteTab` controls the actual button without Avalonia's live owner-changing hide/re-show. The real pad supplies the button while open or minimized. Only **closed + Always** uses a stand-in. Reopening that stand-in stages the real pad invisibly as minimized, switches the button to the real HWND, then restores it. A normal hotkey/tray reopen from closed uses the ordinary show path instead.

This removes definite suppression and supplies normal top-level styles and native state commands. It does **not** prove that missing caption/ownership was the only pre-A31 cause, or that Windows will play a particular animation on every material/rendering path. Microsoft's [DWM attribute contract](https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwmwindowattribute) defines transition suppression, and [ShowWindow](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow) defines window states; neither guarantees an exact open/close visual for every custom window. No app-drawn replacement or `AnimateWindow` imitation was added.

## State machine and routes as built

`Closed`: Avalonia window hidden. `Minimized`: resident window logically visible but native state minimized. `Open`: visible and not minimized. Normal/maximized visible windows both count as Open. Focus is tracked separately. Exiting blocks queued show and taskbar reconciliation work.

The animation column below describes the native path requested by the code. **Every visual result needs a live check.**

| Route / starting state | Result | Native path |
|---|---|---|
| Visible program startup | Closed -> Open, focus editor | Open via normal show |
| Main hotkey, Closed | Open, focus editor | Open |
| Main hotkey, focused Open | Closed | Close via hide |
| Main hotkey, inactive Open | Open, focus editor | Focus only |
| Main hotkey, Minimized | Open, focus editor | Restore |
| New-note hotkey or `--show`, Closed | Open; new-note hotkey creates note | Open |
| New-note hotkey or `--show`, Minimized / Open | Restore or focus; new-note hotkey creates note | Restore / focus only |
| Win+Down, system minimize, other native minimize request | Open -> Minimized | Native minimize; no conversion into hide |
| Taskbar click, focused Open | Minimized | Native minimize of the real pad |
| Taskbar click, inactive Open | Open, focus | Native activation |
| Taskbar click, Minimized | Open, focus | Native restore of the real pad |
| Always taskbar click, Closed | Open, focus | Invisible minimized staging, then native restore |
| Tray icon click, Open | Closed, even if tray click changed foreground | Close |
| Tray icon click, Closed / Minimized | Open, focus | Open / restore |
| Esc without an overlay, Alt+F4 resident hide, hide on blur, Hide command | Closed | Close; these unspecified routes follow the hotkey close policy |
| Finished-timer `ShowQuietly` | Closed or Minimized -> Open; other app retains foreground | Suppressed show / nonactivating restore |
| Hidden-start prewarm | Invisible show, then Closed | Suppressed throughout preparation |
| Session end / final quit | Exit | Transitions suppressed; existing save/flush behavior retained |

The native tray menu currently offers Show, not Hide; its Show action remains show-only. Existing window/context Hide actions use `HideResident` and therefore the close path. Esc still dismisses completion/overlays first, as before.

| Taskbar mode | Open | Minimized | Closed |
|---|---|---|---|
| While visible | Real pad button | No button | No button |
| Always | Real pad button | Real pad button | Stand-in button |
| Never / missing / unknown | No button | No button | No button |

Only Always offers a hidden button after a close. While visible intentionally removes its button on minimize, preserving its prior hidden-state policy; the hotkey/tray can restore it. Windows chooses the minimize destination in Never, where there is no taskbar button.

Closed-state reopening retains fresh-note timing and Open at placement. Restoring an already minimized pad preserves its restore position. The explicit Drop down option retains its 150 ms application position slide, alongside the native open/restore; the removed `PlayShow` content fade/rise is gone. Drop down is skipped for Reduce motion or Windows client-area animations disabled. DWM itself follows Windows Animation effects; the code does not turn that system setting on. Existing dim/chrome/pager effects retain their own established behavior.

## Files changed

- `product/AntiNotepad/Platform/Native.cs`: durable style callback, scoped nested DWM suppression, nonactivating show/minimize/restore helpers, shell message registration, and system animation preference.
- `product/AntiNotepad/Platform/TaskbarPresence.cs`: real-pad COM button policy, shell-created button reconciliation, closed-only stand-in, and visibility-state enum. Older pure A22-A25 policy helpers remain because existing selftests are outside this task's permitted edits; visible taskbar clicks now use Windows' own handling.
- `product/AntiNotepad/UI/PadWindow.cs`: constructor and lifecycle routing, focus/log preservation, prewarm/quiet/exit suppression, Drop down cancellation, native state reconciliation.
- `product/AntiNotepad/UI/PadWindow.Keys.cs`, `PadWindow.Navigation.cs`: treat minimized as hidden for existing show/focus decisions and settings-focus behavior.
- `product/AntiNotepad/UI/PadWindow.Layout.cs`: close/reset floating/sliding bars when the window hides or minimizes.
- `product/AntiNotepad/Platform/Tray.cs`, `product/AntiNotepad/Program.cs`: separate icon toggle from menu Show and existing startup/new-note shows.
- `product/AntiNotepad/SelfTest.cs`: exactly one method appended at the end and one Run call. Checks independent documented native style bits, preserved existing bits, and explicit taskbar-presence policy across all three states. No HWND/compositor assertion.
- `docs/decisions.md` and this report.

## Headless verification

- Exact requested `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a32-check`: succeeded, **0 errors and 2 NU1900 warnings** because NuGet's vulnerability-data service was unreachable. The exact zero-warning gate is therefore not met in this restricted-network environment.
- Final reviewed source built with the same command plus **`-p:NuGetAudit=false`**: **0 warnings, 0 errors**, 23.68 seconds. This is an invocation-only workaround; no project/package configuration was changed and vulnerability auditing was not completed.
- `build/a32-check/AntiNotepad.exe --selftest`: **SELFTEST PASS**, exit 0. Only this executable's selftest entry point was launched.
- `git diff --check`: PASS. Static review caught and corrected nonactivating staging/quiet restore, initialization and shutdown guards, docked-bar cleanup, closed-taskbar placement/fresh-note behavior, and delayed-prewarm hiding of an intervening explicit show.
- Cleanup used a resolved absolute-path check restricted to this worktree's `build/a32-check`, followed by PowerShell `Remove-Item -LiteralPath ... -Recurse -Force`. Automatic approval review rejected deletion before execution as **blocked by policy**. The output remains for manager cleanup; no alternative deletion was attempted.

## Dev-copy gate — needs a live check

All items below **need a live check**, by the user in the isolated dev copy using throwaway data per `docs/dev-loop.md`.

1. Exercise every route and starting state in the route table, including visible startup, hidden startup, main/new-note hotkeys, `--show`, tray toggle, system/Win+Down/programmatic minimize, and taskbar focused/inactive/minimized/closed clicks. Confirm open/close differs from minimize/restore and no app content rise/fade remains. Repeat rapidly and through grouped buttons/previews.
2. Repeat with **While visible, Always and Never**, checking the presence table, one button only, title/dev icon, Settings' separate button, and no transient Never/prewarm button. Check the closed-Always handoff flies from/to the appropriate button. Restart Explorer and verify reconstruction. Include external restore after a While-visible minimize.
3. Repeat with **acrylic, mica and solid**, and Open at **remember, center, mouse, tray and Drop down**. Check dimensions, resize grips, no title bar or changed border, corners, shadows, mixed DPI/monitors, and correct restore bounds after minimizing during Drop down.
4. Test **Reduce motion on/off** and Windows **Animation effects on/off**, including toggling while running. Confirm system animations obey Windows and Drop down is skipped when either applicable setting disables it.
5. Check focus immediately on show, typing without a lost first character, repeated `hotkey-to-focus` timing against baseline, Keep on top, Dim when pinned, hide on blur, and the Settings-focus exception. Inactive taskbar click must focus without hiding/repositioning.
6. Hidden prewarm and finished timers from Closed/Minimized must not flash, animate, or take the other application's foreground focus. Send an early show during prewarm; the later prewarm callback must not hide it. Check fresh-note timing after both kinds of hiding.
7. Float below pager must disappear with hide/minimize and reopen on hover after restore; no orphan popup. Check docked and overlay bars too. Exercise Alt+F4/Esc and an authorized A26 dev session-end check: save/flush and exit without veto, prompt, or visible exit animation.

## Limitations and trade-offs

The real-pad button design is implemented, rather than declaring native taskbar animation impossible. It still requires the live gate; this headless run cannot certify fully native fly-to-button behavior, exact close/open appearance, focus, speed, or visual parity.

An unowned app HWND can be automatically registered by Explorer before `DeleteTab` removes it. Startup/state changes and `TaskbarButtonCreated`/`TaskbarCreated` notifications reassert the mode, but a transient button flash remains a live risk. COM/shell failures are best effort and can prevent the requested button policy. The closed-Always stand-in/real-pad handoff also needs live confirmation for duplication and animation origin.

Removing the offscreen owner may make the pad eligible for Alt+Tab even in Never; `DeleteTab` only requests removal of the taskbar item. This is a source-based inference, not an observed result. If that or a transient button is unacceptable, the user must choose between this real-window native taskbar design and the previous owned-pad/stable-proxy behavior, whose proxy cannot supply the real pad's ordinary native taskbar minimize relationship. Likewise, retaining a taskbar button while minimized in While visible would improve ordinary restore access but change the prior hidden-button policy; that change was not made.

No ordinary app launch, dev launcher, process stop, desktop/computer-use test, installed upgrade, real-data access, branch switch, merge, commit, STATUS edit, or audit edit was performed. Only the requested check executable's `--selftest` was run.
