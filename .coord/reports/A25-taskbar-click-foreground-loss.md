# A25-taskbar-click-foreground-loss

## Rule and change

- A visible pad counts as foreground before a taskbar click when its most recent recorded foreground loss occurred within 500 ms (inclusive) before the first stand-in message and the counterpart is null (`None`), `Shell_TrayWnd` / `Shell_SecondaryTrayWnd` (`Taskbar`), or the stand-in (`Proxy`). The earliest click snapshot preserves that evidence through the decision, even if later messages arrive after the grace period.
- A null `WM_ACTIVATE/WA_INACTIVE` counterpart cannot identify the other-thread window. The supplied Windows 11 trace shows that this is how the taskbar transfer arrives, so a recent null loss followed by a stand-in click is accepted as evidence. This is a bounded timing inference, not proof that every null loss is Explorer. A non-null unrelated counterpart remains `Other` and does not qualify; a non-null HWND whose class lookup fails also remains `Other`.
- Old losses do not qualify. Hidden clicks still show, and visible inactive clicks without qualifying foreground evidence still focus. Presence modes, dispatch coalescing, post-hide fallback suppression, and dev-only tracing are unchanged. No PadWindow changes were needed.

## Why `lossAt=none` happened

`StartClickSnapshot` copied the timestamp/kind into `ClickSnapshot`, then immediately reset `_lastForegroundLoss` and its counterpart. `QueueTaskbarClick` logged and checked those cleared fields, producing `lossAt=none`. The timestamp was still in `earlySnapshotLossAt`, but the snapshot's foreground result was false because null counterparts were classified as `Other`.

Snapshot creation now leaves the loss intact. `QueueTaskbarClick` consumes it only after deciding and logging, so the decision can report the same loss as the early snapshot. The immutable snapshot's `PadWasForeground` remains the first-message evidence used by the decision.

## Verification

- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a25-check`: PASS, 0 warnings, 0 errors. No NuGet audit override needed.
- `build/a25-check/AntiNotepad.exe --selftest`: exit 0, `SELFTEST PASS`.
- Extended `TaskbarChecks`: null counterpart loss 17 ms before the first stand-in message hides a visible pad; the same evidence still shows a hidden pad; null loss at the inclusive 500 ms boundary qualifies; expired null loss focuses a visible inactive pad or shows a hidden pad. Existing taskbar/proxy/other attribution, future timestamp rejection, presence modes, and post-hide fallback checks pass.
- `git diff --check`: PASS.
- Only `--selftest` was launched. No app was killed; installed app/data, STATUS, and audit document were untouched. No git branch change, merge, or commit. Cleanup of task build output was attempted per `docs/dev-loop.md`, but automatic approval review rejected the removal command as "blocked by policy"; `build/a25-check` remains.
- Native message order and live shell behavior have not been exercised here; the manager should validate in the dev copy.

## Expected focused-pad click trace

Illustrative excerpts for the supplied message order; omitted fields are marked `...`, timestamps/handles vary. Inspect the dev copy's `taskbar:` trace:

```text
taskbar: t=...ms pad WM_ACTIVATE state=WA_INACTIVE ... counterpart=0x0/(none) visible=True ...
taskbar: t=...ms pad foreground-loss kind=None at=<loss-time>ms counterpart=0x0/(none) included=True
taskbar: t=...ms proxy hwnd=... WM_WINDOWPOSCHANGING ... internal=False
taskbar: t=...ms click snapshot source=WM_WINDOWPOSCHANGING padVisible=True padActive=False ... lossKind=None lossAt=<loss-time>ms lossAge=17.0ms padWasForeground=True
taskbar: t=...ms proxy hwnd=... WM_ACTIVATE state=WA_ACTIVE ...
taskbar: t=...ms decision source=WM_ACTIVATE result=hide ... lossKind=None lossAt=<loss-time>ms lossAge=...ms recentTaskbarLoss=True ... earlySnapshotLossKind=None earlySnapshotLossAt=<loss-time>ms ... earlySnapshotPadWasForeground=True ...
taskbar: t=...ms decision duplicate-suppressed source=Avalonia.Activated reason=activation-pending
taskbar: t=...ms decision duplicate-suppressed source=SC_RESTORE reason=activation-pending
taskbar: t=...ms decision dispatch source=WM_ACTIVATE result=hide padVisible=True
taskbar: t=...ms pad taskbar callback source=WM_ACTIVATE action=hide visible=True
taskbar: t=...ms pad action=hide-begin cause=taskbar-click:WM_ACTIVATE visible=True
taskbar: t=...ms pad action=hide-end cause=taskbar-click:WM_ACTIVATE visible=False
```

`recentTaskbarLoss` retains its existing trace field name and now includes qualifying null-counterpart losses. Later activation-only fallback callbacks should remain suppressed, with no taskbar-caused `show-begin` in that cycle. If another message starts the click first, its `source=` replaces `WM_WINDOWPOSCHANGING`; restore-first clicks can dispatch with `source=SC_RESTORE`.
