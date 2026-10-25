# A24-taskbar-click-diagnose

## Sequences considered

- **`SC_RESTORE` arrives first.** Snapshot visibility, Avalonia active state, `GetForegroundWindow`, its class, and any recent foreground loss before consuming the restore. If the visible pad was foreground, decide `hide`; if hidden, decide `show`; if visible but inactive, decide `focus`.
- **`WM_WINDOWPOSCHANGING`, `WM_SHOWWINDOW(show)`, or `WM_NCACTIVATE(active)` arrives first.** Keep that earliest snapshot and reuse it when `SC_RESTORE` or `WM_ACTIVATE` supplies the action signal. `WM_WINDOWPOSCHANGING` / `WM_SHOWWINDOW` caused by our own native show/hide calls are logged but excluded as click starts. `WM_NCACTIVATE` captures state without deciding by itself.
- **Explorer's taskbar activates before the proxy.** The pad hook records the `WM_ACTIVATE/WA_INACTIVE` counterpart and monotonic time. A recent loss to `Shell_TrayWnd`, `Shell_SecondaryTrayWnd`, or the proxy remains foreground evidence for 500 ms; a loss to another class does not.
- **Another app was already foreground.** The early foreground HWND/class and any loss to an unrelated window keep this as a `focus` decision. The activation path still calls `ShowResident` to focus the pad.
- **The pad is hidden in Always mode.** Its click snapshot is hidden, so the decision is `show`, regardless of stale foreground state.
- **Restore and activation callbacks repeat.** Pending callbacks coalesce. After a hide decision, activation-only duplicates are suppressed for up to one second; a new window-position/show start or explicit `SC_RESTORE` begins a fresh click snapshot. This prevents a fallback callback from turning the just-hidden pad back on.

## Changes

- Added dev-only `taskbar:` trace lines with monotonic milliseconds for the stand-in's `WM_SYSCOMMAND`, `WM_ACTIVATE`, `WM_ACTIVATEAPP`, `WM_NCACTIVATE`, `WM_SHOWWINDOW`, and `WM_WINDOWPOSCHANGING` flags, plus the pad's activation, visibility, and non-client activation messages.
- Decision lines include current and early-snapshot visibility, foreground HWND/class, active state, foreground-loss timestamp/kind/age, decision (`hide` / `show` / `focus`), dispatch source, and snapshot age.
- Pad show/hide and activation traces carry their cause, including `taskbar-click:SC_RESTORE`, `taskbar-click:WM_ACTIVATE`, `taskbar-click:Avalonia.Activated`, `hide-on-blur`, prewarm, and quiet timer show. The existing `Program.Log` writes to the dev data folder's `antinotepad.log`.
- Kept button presence policy and the existing hidden/inactive click rules. Added a pure one-second post-hide fallback suppression rule and cases to `TaskbarChecks`.

## Expected log lines by click type

Each example is one `Program.Log` line; the file adds its normal wall-clock prefix before `taskbar:`.

### Focused visible pad, click to hide

```text
taskbar: t=...ms proxy hwnd=... WM_SYSCOMMAND wParam=0xF120 command=0xF120 name=SC_RESTORE ...
taskbar: t=...ms click snapshot source=... padVisible=True ... padWasForeground=True
taskbar: t=...ms decision source=SC_RESTORE result=hide ... earlySnapshotPadWasForeground=True ...
taskbar: t=...ms pad action=hide-begin cause=taskbar-click:SC_RESTORE visible=True
taskbar: t=...ms pad WM_SHOWWINDOW show=False ... cause=taskbar-click:SC_RESTORE ...
```

If the OS sends activation fallback messages after the hide, expect `decision duplicate-suppressed` or `fallback suppressed ... reason=post-hide duplicate`, and no later `pad action=show-begin` from that cycle.

### Hidden pad, click to show (Always)

```text
taskbar: t=...ms click snapshot source=... padVisible=False ... padWasForeground=False
taskbar: t=...ms decision source=SC_RESTORE result=show padVisible=False ...
taskbar: t=...ms pad action=show-begin cause=taskbar-click:SC_RESTORE visible=False
taskbar: t=...ms pad WM_SHOWWINDOW show=True ... cause=taskbar-click:SC_RESTORE ...
taskbar: t=...ms pad WM_ACTIVATE state=WA_ACTIVE ... cause=taskbar-click:SC_RESTORE ...
```

### Visible but inactive pad, click to focus

```text
taskbar: t=...ms click snapshot source=... padVisible=True ... fg=0x.../OtherClass ... padWasForeground=False
taskbar: t=...ms decision source=SC_RESTORE result=focus ... recentTaskbarLoss=False ...
taskbar: t=...ms pad action=show-begin cause=taskbar-click:SC_RESTORE visible=True
taskbar: t=...ms pad WM_ACTIVATE state=WA_ACTIVE ... cause=taskbar-click:SC_RESTORE ...
```

### Taskbar takes activation first

```text
taskbar: t=...ms pad WM_ACTIVATE state=WA_INACTIVE ... counterpart=0x.../Shell_TrayWnd ...
taskbar: t=...ms pad foreground-loss kind=Taskbar ... included=True
taskbar: t=...ms click snapshot source=... lossKind=Taskbar lossAge=...ms padWasForeground=True
taskbar: t=...ms decision source=SC_RESTORE result=hide ... recentTaskbarLoss=True ...
```

The exact first proxy message can differ by Windows build; the `source=` field and monotonic order show which sequence occurred on the manager's live system.

## Verification

- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a24-check -p:NuGetAudit=false`: **PASS**, 0 warnings, 0 errors. The initial build reported NU1900 because `api.nuget.org` was unreachable, so the permitted audit override was used.
- `build/a24-check/AntiNotepad.exe --selftest`: **SELFTEST PASS**. This includes existing Never / While visible / Always presence and click-policy checks, foreground-loss attribution, and the new post-hide activation-fallback cases.
- `git diff --check`: **PASS**.
- No GUI app was launched. Native message order and live shell behavior remain for the manager to verify from the dev copy's `build/testdata/antinotepad.log` after a click.
