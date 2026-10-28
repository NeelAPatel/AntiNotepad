# A43b-notes-session

Implementation on `reorg/A43b-notes-session`; no commit made.

## Stage 1: explicit services

### Changes

- `product/AntiNotepad/Program.cs`: `RunApplication` now keeps the loaded `Store` and its `SettingsService` in locals and passes both through the existing `Configure<App>(() => ...)` factory. `App` retains those same instances and forwards them to `PadWindow`; the `SettingsService.Changed` subscription remains one subscription.
- `product/AntiNotepad/UI/PadWindow.cs`: the timer predicate is initialized in the constructor after `_store` assignment and reads the injected store's current `Data.Stack` and `Data.Slots` on every call.
- `product/AntiNotepad/UI/SearchOverlay.cs` and `UI/PadWindow.Keys.cs`: search receives the injected `Store`; it still creates a fresh search snapshot and context on each query.

### Program reference counts

Counts are `Program.<name>` occurrences across `product/AntiNotepad/**/*.cs`.

| Token | Before | After stage 1 |
|---|---:|---:|
| `DataFolder` | 6 | 6 |
| `Store` | 5 | 1 |
| `NotesLocationError` | 4 | 4 |
| `AppHome` | 3 | 3 |
| `SettingsService` | 2 | 0 |
| `MoveNotes` | 1 | 1 |
| **Total** | **21** | **15** |

### Gate

- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A43b-codex-check`: succeeded with 0 warnings and 0 errors.
- `build/A43b-codex-check/AntiNotepad.exe --selftest`: exit 0, `SELFTEST PASS`; all 80 existing `LAYERS` output lines remained present and unchanged. Existing `SETTINGS Changed`, `HOTKEYS swap`, `CLIPBOARD policy`, `RUNTIME LOG`, `TASKBAR TRACE`, `MATH NOTES`, and `LIST EDITING` checks passed.

No stage-1 scope deviations.

## Stage 2: NotesSession and live location

### Changes

- `product/AntiNotepad/Model/INotesLocation.cs` adds the read-only `DataFolder` and `NotesLocationError` view.
- `product/AntiNotepad/NotesSession.cs` owns the same loaded `Store`, final `AppHome`, current notes folder/error, and injected warning sink. `MoveNotes` follows the original guard, save, move, folder switch, state update, warning log, and return order.
- `Program.RunApplication` now keeps home, folder, error, store, and settings service as locals; it constructs one session after load/seeding/renderer selection beside `AppLaunch`. The service/location statics, `_runtimeLog`, and `Program.MoveNotes` are removed. Bootstrap identity, migration, early exits, instance routing, and log-home ordering remain in place.
- `App`, `PadWindow`, and `SettingsWindow` receive the same session. `PadView` and the Settings motion preview use `INotesLocation`. Search and timers retain the stage-1 injected Store behavior.
- `product/AntiNotepad/SelfTest.cs` adds the Program universal layer row, session/interface layer rows, method-scoped Program `ldtoken` allowance, typed static exposure and DATA/DEV environment-read guards, and the `NOTES SESSION move` capability check.
- `docs/dev-loop.md` changes only the two requested notes-folder sentences.

### Program reference counts

| Token | Before | After stage 1 | After stage 2 |
|---|---:|---:|---:|
| `DataFolder` | 6 | 6 | 0 |
| `Store` | 5 | 1 | 0 |
| `NotesLocationError` | 4 | 4 | 0 |
| `AppHome` | 3 | 3 | 0 |
| `SettingsService` | 2 | 0 | 0 |
| `MoveNotes` | 1 | 1 | 0 |
| **Total** | **21** | **15** | **0** |

The final `rg --no-heading -o 'Program\.\w+' product/AntiNotepad --glob '*.cs'` check returned 0 matches.

### Gate and guard summaries

- Stage-2 Debug build: succeeded with 0 warnings and 0 errors.
- `build/A43b-codex-check/AntiNotepad.exe --selftest`: exit 0 and `SELFTEST PASS`.
- Selftest printed 89 `LAYERS` lines: all 80 existing lines stayed unchanged and the nine new rows reported 0 references. Existing `SETTINGS Changed`, `HOTKEYS swap`, `CLIPBOARD policy`, `RUNTIME LOG`, `TASKBAR TRACE`, `MATH NOTES`, `LIST EDITING`, identity, and later capability checks passed.
- `NOTES SESSION move passed`; it covers the empty sibling move, retained Store/Data/SettingsService identities, pointer follow, a later save into the target, occupied/nested refusal, and unavailable-store refusal without folder creation. Success-with-unavailable remains a source invariant; no test seam was added.
- `STATIC service exposure 0 refs`; the reflection check includes static fields, properties, and backing fields and asserts Program itself has no typed service statics. Its source comment documents that delegate, dictionary, and object-typed locators are outside this check's bound.
- `ENV DATA/DEV confinement 0 refs`; the IL check inspects `ldstr` values in methods that call `Environment.GetEnvironmentVariable` and confirms the DATA/DEV reads remain under Program.

### Readers moved to the live view

- `PadView.Bind` now reads the session's location error and folder through `INotesLocation`; the existing per-bind error local and same-note early return remain unchanged.
- Settings storage details read the current folder/error when their factory builds controls. The recovered-folder path remains a page-build snapshot as before; folder selection compares against the live folder after the picker returns; Open reads the current folder at click time.
- Settings move preflight still saves through the host, checks `LastError`, snapshots the old folder/home/count/backup root before moving, and refreshes the post-move backup text. It now reads those values and calls `MoveNotes` through the session.
- The app-data description and recovered-folder details keep their prior page-build snapshots. App-home Open and notes-folder Open callbacks read the session when clicked. The preview passes the live interface to its private PadView.
- App, timer, and search consumers use the injected Store; search context creation remains per query.

### Deviations and limits

- The typed static exposure check passes. The literal broad grep `rg 'static .*(Store|SettingsService|NotesSession|RuntimeLog)' product/AntiNotepad` still prints existing static method signatures outside Program: `Welcome.SeedIfNew` (`StoreData`/`SettingsService` parameters), `Store.IsComplete` and `Store.CloneData` (`StoreData` parameters), `Showcase.EnsureInDev` (`StoreData` parameter), and `SelfTest.RuntimeLogChecks` (method name). It also prints `Program.RunApplication` with its permitted `RuntimeLog` parameter. These are not static fields/properties exposing service instances; I left the unrelated methods alone under the plan's drop/leave-alone scope.
- The task check output is still present at `build/A43b-codex-check`. I verified its resolved path is inside this workspace, but the tool policy rejected the requested recursive removal. No app GUI or process was launched.

No implementation scope changes beyond these reported check limitations.
