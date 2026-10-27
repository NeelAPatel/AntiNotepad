# A43a-runtime-log-and-launch: RuntimeLog + launch values injected instead of read from `Program`

Role: worker (gpt-6-luna xhigh). **Implementation.** Product `product/AntiNotepad`. Read `AGENTS.md`, then the plan
`.coord/tasks/A43-app-context-plan.md` ("Recommendation on handling" supersedes round 1) and
`.coord/reports/A43-app-context-impact-r2.md`. Branch `reorg/A43a-runtime-log-and-launch` (already checked out; do not switch
branches, do not commit: you cannot write to `.git`). Behavior-neutral refactor. Line numbers are from the current tree; re-read
before editing. **Scope is A43a ONLY.** Do not touch `Store`, notes location, `DataFolder`, `NotesLocationError`, `AppHome`
(the static), `MoveNotes`, `SettingsService` reads, or `Program.Store` reads (all A43b). Do not add a DI container, `AppRuntime`,
`AppInfo.IsDev`, or touch other statics (`Anim.Reduced`, `SessionEnd`, ...), install scripts, window frame styles, or the
hotkey/clipboard/math code.

## What changes
1. **New `RuntimeLog.cs`** (namespace `AntiNotepad`, `internal sealed class RuntimeLog : IDisposable`). `BackgroundLog` moves
   into this file **verbatim** (same class, `internal`). `RuntimeLog` owns the current `Program.Log` logic, same semantics:
   - `SetHome(string home)`: stores the home folder (`Program` calls it wherever it assigns `AppHome`, i.e. the provisional and
     then the final home). No logger exists until the first `Write` with a non-empty home; once the `BackgroundLog` is created it
     is **never retargeted** (a later `SetHome` only affects a logger that does not exist yet). Thread-safe with one gate, as today.
   - `Write(string line)`: no-op while home is empty/whitespace; lazy-creates `BackgroundLog(Path.Combine(home, AppInfo.LogFileName))`;
     swallows all exceptions (exactly as `Program.Log` does).
   - `Flush()` (= `FlushLogs`) and `Dispose()` (= `DisposeLogs`): same behavior, exceptions swallowed.
   - `LogCrash(object? error)`: `Write("crash " + error)` then `Flush()`; both crash handlers in `Program` call it.
2. **`Program.cs`**:
   - `Main` creates the `RuntimeLog` first (before `ProcessExit` is hooked): `ProcessExit += (_, _) => log.Flush()`;
     `try { return RunApplication(args, log); } finally { log.Dispose(); }`. `--selftest` still returns before any of this.
   - `RunApplication` takes the log; both unhandled-exception handlers capture it and call `LogCrash`. All current `Log(...)`
     calls and `Startup.MigrateLegacy(Log)` use `log.Write`. `log.SetHome(...)` is called where `AppHome` is assigned.
   - Remove `Program.Log`, `FlushLogs`, `DisposeLogs`, `_logGate`, `_logger`, and the static `IsDev`, `StartHidden`,
     `StartupNotice`, `SoftwareRendering` properties. They become locals / launch values (below). `Program` keeps its
     existing static `AppHome`, `DataFolder`, `NotesLocationError`, `Store`, `SettingsService`, `MoveNotes`, `_window`, `_pendingRequests`.
   - `MoveNotes` keeps working: it logs its warning through a log reference held by `Program` (a private static set once in
     `RunApplication` for this purpose, **not** public, not read outside `Program.cs`). This goes away in A43b.
   - `RequestShow`/`RequestQuit`/`Attach` keep their logic and order.
   - Instance id, `usesCustomData`, `--quit` routing (decided before `--hidden`/lock), `ANTINOTEPAD_DATA`/`DEV` parsing,
     `SingleInstanceService` wiring, migration, `Welcome`/`Showcase` calls: unchanged in behavior and order.
     `Showcase.EnsureInDev(Store.Data, isDev)` uses the local.
3. **`App`** (same file): the injected launch values. First verify the Avalonia 12 factory overload exists:
   `AppBuilder.Configure<App>(Func<App>)`. If it does, use it with an `internal` constructor `App(AppLaunch launch)`
   (`internal sealed record AppLaunch(RuntimeLog Log, bool IsDev, bool StartHidden, string? StartupNotice, bool SoftwareRendering, Action<PadWindow> Attach)`,
   or an equivalent small type), and remove the parameterless usage. If the overload does not exist or Avalonia insists on a
   parameterless ctor, use the plan's fallback: **one set-once hand-off confined to `Program.cs`** that `App` reads, and say so in the
   report. Either way `App` reads no `Program.IsDev|Log|FlushLogs|StartHidden|StartupNotice|SoftwareRendering|Attach`. It may still read
   `Program.Store` / `Program.SettingsService` (A43b). Order inside `OnFrameworkInitializationCompleted` is unchanged:
   build window, subscribe `Changed`, `ShutdownRequested`, `QuitRequested`, tray, `HotkeyStatusChanged`, `InstallHooks`,
   `Attach(window)`, then show-or-prewarm, then the startup toast.
4. **`PadWindow`** (+ `.Keys.cs`, `.Layout.cs`): constructor gains `RuntimeLog log, bool isDev, bool softwareRendering`
   (stored readonly). Every `Program.Log` -> `_log.Write`, `Program.FlushLogs` -> `_log.Flush` (keep save -> log -> flush order at
   session end), every `Program.IsDev` -> `_isDev`. The `new SettingsWindow(_settingsService, this)` call passes `softwareRendering`
   so `SettingsWindow.MaterialHint` stops reading `Program.SoftwareRendering` (add a readonly ctor parameter; do not touch its
   other `Program.` reads). Construct a `TaskbarTrace` (below) in the ctor and pass it to the proxy. Keep `Program.Store` in the `Timers`
   lambda (A43b).
5. **`Platform/TaskbarPresence.cs`**: new instance `internal sealed class TaskbarTrace(bool isDev, Action<string> log)` replacing the
   static `Trace`, `TraceStartedAt`, `SinceTraceStart`, `TracePadAction`, `PadActionContext`, `_padActionAt`, `_padActionCause`
   (state per instance: the stopwatch origin is taken at construction, the pad-action timestamp/cause persist across calls so click-cause
   correlation is kept). `IsDev` exposed readonly. Output text format unchanged (`taskbar: t=...ms ...`), gated by `isDev` as today.
   The pure static decision helpers (`ShouldShow`, `ShouldHideOnClick`, `ShouldSuppressPostHideActivation`, `WasPadRecentlyForeground`)
   and the constants stay in static `TaskbarPresence`. `TaskbarProxy` ctor gains `bool isDev, TaskbarTrace trace` (readonly fields);
   all 28 `Program.IsDev` reads become `_isDev` and all `TaskbarPresence.Trace(...)` become `_trace.Write(...)`; the statics
   `LossTimestampText` etc. that used the trace origin become instance members. No change to message order, thread use, or
   which events call what. `TaskbarPresence.cs` must no longer mention `Program`.
6. **`SelfTest.cs`**: (a) new `RUNTIME LOG` check (name it `RUNTIME LOG` in the output list like the others; keep the old
   `BackgroundLog` Flush check as is): empty home `Write` is a no-op and creates no file; `SetHome(tmp)` then `Write`+`Flush`
   lands in `tmp/<AppInfo.LogFileName>`; **never retargeted**: after the first write, `SetHome(other)` and another `Write` still go to
   the first file and nothing appears in `other`; **synthetic early-crash path**: a fresh `RuntimeLog` with only `SetHome` set, then
   `LogCrash(new InvalidOperationException("boom"))` with **no explicit `Flush`**, leaves a `crash ...boom` line in the file; `Dispose`
   drains queued lines and a `Write` after `Dispose` does not throw; `Flush` on a log that never created a logger does not throw.
   (b) **`TASKBAR TRACE`** check: with `isDev=false` the sink gets nothing; with `isDev=true` a `Write("x")` reaches the sink as
   `taskbar: t=...ms x`, `PadActionContext` is `cause=unknown age=na` before any `TracePadAction` and `cause=<c> age=...` after,
   `TracePadAction` writes the `pad action=<phase> cause=<c> visible=<v>` line. (c) LAYERS rows (same schema as the existing ones),
   added after the `ClipboardCapturePolicy` rows:
```csharp
new("Platform", "Program", ExactTo: typeof(Program)),
new("Model", "Program", ExactTo: typeof(Program)),
new("RuntimeLog", "UI", ExactFrom: typeof(RuntimeLog)),
new("RuntimeLog", "Editor", ExactFrom: typeof(RuntimeLog)),
new("RuntimeLog", "Platform", ExactFrom: typeof(RuntimeLog)),
new("RuntimeLog", "Program", ExactFrom: typeof(RuntimeLog), ExactTo: typeof(Program)),
new("RuntimeLog", "Avalonia", ExactFrom: typeof(RuntimeLog), ExactTo: typeof(Avalonia.Application)),
```
   Use the real spellings the existing rows use (e.g. whatever lets "Platform" match `typeof(Program)` exactly; adjust only if one
   does not compile; if a scanner extension is truly needed, keep it minimal and say so). The existing 73 rows stay untouched.
   Do not prove the guards fail yourself; the manager does.

## Rules
- Behavior-neutral: same log file/location/text, same instance ids, `--quit` routing, `--selftest` path, `ANTINOTEPAD_DATA`/`DEV` handling,
  window frame styles, and activation/taskbar callback order. US spelling. Do not launch or kill AntiNotepad (never run
  `AntiNotepad.exe --quit`). Do not commit. No behavior fixes, no import cleanup beyond what the change needs.
- Build: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A43a-codex-check` (0 warnings, 0 errors),
  then `build/A43a-codex-check/AntiNotepad.exe --selftest` must print `SELFTEST PASS` with all existing lines (LAYERS now 73 + the new
  rows, SETTINGS, HOTKEYS, CLIPBOARD, MATH NOTES, LIST EDITING, A29 checks, the BackgroundLog check) plus `RUNTIME LOG` and `TASKBAR TRACE`.
  Delete `build/A43a-codex-check` afterwards.

## Done means
- Build 0 warnings 0 errors; `SELFTEST PASS`.
- Baseline outside `Program.cs`: 75 qualified `Program.` tokens in 8 files (`rg -o 'Program\.\w+' product/AntiNotepad --glob '*.cs'`
  counts `Program.cs` itself too: 75 total, including its own 6 inside `App`). After: `IsDev`, `Log`, `FlushLogs`, `StartHidden`,
  `StartupNotice`, `SoftwareRendering`, `Attach` = **0** anywhere outside `Program.cs`, and `TaskbarPresence.cs` has 0 `Program.`;
  what remains is only `DataFolder`, `NotesLocationError`, `AppHome`, `Store`, `SettingsService`, `MoveNotes` (A43b). Report the counts per
  token before and after.
- Report at `.coord/reports/A43a-runtime-log-and-launch.md`: files changed, the factory-overload finding (exists or fallback used),
  the token counts, selftest summary line for `RUNTIME LOG`/`TASKBAR TRACE`/LAYERS, any deviation and why (including the trace
  time-origin move to construction).
