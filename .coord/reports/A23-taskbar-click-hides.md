# A23-taskbar-click-hides

## Cause

The A22 pad hook took its foreground snapshot only when `WM_ACTIVATE` reported the taskbar proxy as the next active window (`lParam == proxy HWND`). If Explorer's taskbar (`Shell_TrayWnd`, or a secondary taskbar) became active first, that snapshot was skipped. When the proxy later received `SC_RESTORE` or activation, the foreground query and Avalonia active state could already identify Explorer, so the click was interpreted as a request to focus the pad. This code path matches the manager's live failure.

## Fix

- Record the monotonic time of each pad `WM_ACTIVATE/WA_INACTIVE` and classify its counterpart as the primary/secondary taskbar, the proxy, or another window.
- At taskbar-click decision time, count a loss to the taskbar or proxy within the previous 500 ms as foreground. Reject unrelated, expired, and future timestamps.
- Keep A22's direct foreground/HWND checks and hidden/inactive click policy in place.
- Extend the existing `TaskbarChecks` with the foreground-loss decision cases.

## Verification

- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a23-check`: **PASS**, 0 warnings, 0 errors.
- `build/a23-check/AntiNotepad.exe --selftest`: **SELFTEST PASS**, exit 0.
- `git diff --check`: **PASS**.
- No GUI instance was launched; taskbar behavior still needs the manager's live dev-copy check.
- Cleanup of the verified `build/a23-check` output was attempted after testing, but automatic approval review blocked recursive removal as **blocked by policy**; the output remains.

## Manager live check

In the isolated dev copy, test both **While visible** and **Always**:

1. Show the pad and focus its editor. Click its taskbar button. The pad should hide.
2. In **Always**, click the same button while the pad is hidden. The pad should show and focus the editor.
3. Show the pad, focus another application, then click the taskbar button. The pad should focus rather than hide.
4. Repeat with Hide on blur enabled and disabled, confirming the click-to-hide remains hidden and the other A22 taskbar modes/button counts still behave as before.
