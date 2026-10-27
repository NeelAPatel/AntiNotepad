# A43a-runtime-log-and-launch

Implemented on `reorg/A43a-runtime-log-and-launch` as a behavior-neutral runtime-context refactor. No commit was made.

## Changes

- Added `product/AntiNotepad/RuntimeLog.cs`. It owns the verbatim `BackgroundLog`, accepts provisional/final app homes, lazily creates one logger on the first non-empty-home write, never retargets it, and provides swallowed best-effort `Flush`, `Dispose`, and `LogCrash` operations.
- Updated `Program.cs`: `Main` creates the log before registering `ProcessExit`; `RunApplication` owns the log callbacks and local launch values. The existing `AppHome` assignments each call `SetHome`. `MoveNotes` writes through a private `RuntimeLog` reference set once during startup. `App` receives an `AppLaunch` and no longer reads the removed `Program` launch properties or `Attach`.
- Confirmed Avalonia 12.1.4 provides `AppBuilder.Configure<TApp>(Func<TApp>)` in its installed XML API docs (`Avalonia.Controls.xml`, `M:Avalonia.AppBuilder.Configure``1(System.Func{``0})`). Used that factory overload with `App(AppLaunch)`; no static hand-off was needed.
- Updated `PadWindow` and its partials to hold the injected log, dev flag, renderer flag, and one `TaskbarTrace`. `SettingsWindow` receives the renderer flag for `MaterialHint`; its other `Program` reads remain for A43b.
- Replaced static taskbar trace state with per-instance `TaskbarTrace`; `TaskbarProxy` now receives its dev flag and trace. Pure static taskbar decisions are unchanged, and `TaskbarPresence.cs` has no `Program` references.
- Added `RUNTIME LOG` and `TASKBAR TRACE` capability checks and the seven requested LAYERS rows. Existing rows and the `BackgroundLog` Flush check were left in place.
- Updated `.coord/STATUS.md` for the implementation milestone.

## Program token counts

Counts are `Program.<name>` occurrences across product C# files. The baseline is from the A43 impact report; the outside-`Program.cs` column is the current tree.

| Token | Before | After, all files | After, outside `Program.cs` |
|---|---:|---:|---:|
| `IsDev` | 40 | 0 | 0 |
| `Log` | 8 | 0 | 0 |
| `DataFolder` | 6 | 6 | 6 |
| `Store` | 5 | 5 | 4 |
| `NotesLocationError` | 4 | 4 | 4 |
| `AppHome` | 3 | 3 | 3 |
| `FlushLogs` | 2 | 0 | 0 |
| `SettingsService` | 2 | 2 | 0 |
| `Attach` | 1 | 0 | 0 |
| `StartHidden` | 1 | 0 | 0 |
| `StartupNotice` | 1 | 0 | 0 |
| `SoftwareRendering` | 1 | 0 | 0 |
| `MoveNotes` | 1 | 1 | 1 |
| **Total** | **75** | **21** | **18** |

The remaining references are the A43b set: `DataFolder`, `NotesLocationError`, `AppHome`, `Store`, `SettingsService`, and `MoveNotes`. `Program.cs` contains the remaining `Store` and `SettingsService` references used by `App`.

## Verification

- Build: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A43a-codex-check` — succeeded with 0 warnings and 0 errors.
- Selftest: `build/A43a-codex-check/AntiNotepad.exe --selftest` — `SELFTEST PASS`; `RUNTIME LOG 5 checks`; `TASKBAR TRACE 5 checks`; 80 LAYERS lines total (the seven added rows each report 0 refs, as do the existing rows and settings-setter guard). Existing `SETTINGS Changed`, `HOTKEYS swap`, `CLIPBOARD policy`, `MATH NOTES`, `LIST EDITING`, identity, and taskbar checks passed. The existing background save/log check ran as part of the pass.
- The task check output was removed after verification. No GUI or installed copy was launched; the user-controlled dev hand check remains pending.

## Deviation / timing note

There were no scope deviations. `TaskbarTrace` takes its stopwatch origin at construction, as required for per-instance state. This is slightly later than the previous static type-initialization origin; trace text format and callback/message order are unchanged.
