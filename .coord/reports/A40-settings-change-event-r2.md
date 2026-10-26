# A40-settings-change-event-r2

Status: implementation complete on `reorg/A40-settings-change-event-impl`; no commit.

## Runtime writers migrated

- `UI/PadWindow.Keys.cs` — `Settings.FontSize = ...` in `Zoom` now uses `_settingsService.Update`. A changed value is handled once by the existing `Appearance | EditorLook | Persistence` event path; the local duplicate `ApplyLook` and `Edited` calls are gone. At the 10/28 clamp, `Update` is a no-op, so the old short refresh/save path still runs and the toast remains.
- `UI/PadWindow.cs` — `RememberBounds`' X, Y, Width and Height writes are one `_settingsService.Update` batch. `Geometry | Persistence` schedules the save and skips the appearance chain; the direct timer scheduling was removed.
- `UI/PadWindow.Navigation.cs` — `BindPanes` conflict repair and `CycleSplitSlot` use `Update`; `ToggleSplit` batches Split plus its selected SplitSlot into one update; `TogglePin` updates Pinned through the service. Split changes use `Split | Persistence`; Pinned uses `Appearance | WindowPolicy | Persistence` and the ordered handler applies topmost, taskbar and appearance effects.
- `UI/SearchOverlay.cs` — the SearchGrid toggle uses the service and keeps reopening the search overlay. `Search | Persistence` schedules the previously missing save without running appearance work.
- `UI/SettingsWindow.cs` — the DisabledLanguages Add/Remove callback now calls the service owner method and then keeps the existing row refresh behavior.
- `Model/SettingsService.cs` — added `SetLanguageEnabled(string, bool)`, implemented with `Update`; list snapshots still detect DisabledLanguages changes as `Appearance | Analysis | Persistence`.
- `Model/Welcome.cs`, `Program.cs` — the Welcomed seed write now uses `SettingsService.Update`; Program passes the live service. Welcome selftest fixtures also create a service over their fixture settings.

The existing property map is unchanged: X/Y/Width/Height are Geometry-only plus Persistence; Split/SplitSlot are Split-only plus Persistence; SearchGrid is Search-only plus Persistence; Pinned is Appearance|WindowPolicy|Persistence; FontSize is Appearance|EditorLook|Persistence; Welcomed is Persistence. Each batch emits one change. No Folding or InlineMath propagation was changed; both remain open as recorded in r1.

## Setter guard

`SelfTest.CheckSettingsSetters` now scans IL in every app-assembly type and inspects `call` and `callvirt` targets for `Settings.set_*`. It prints exactly `LAYERS Settings.set_* outside owner 0` on a clean run and names both caller and setter for violations.

The explicit allow-list in `SelfTest.SettingsSetterAllowances` is:

- `Settings.CopyForSave` — assigns the copied DisabledLanguages list on a detached save snapshot.
- `StoreData` constructor — JSON/default construction only.
- `Store.Normalize` — its permitted normalization writes run inside `SettingsService.Update` before UI subscribers attach.
- `Welcome.SeedIfNew` — the permitted write is inside the live service update.
- `PadWindow.Zoom`, `RememberBounds`, `BindPanes`, `ToggleSplit`, `CycleSplitSlot` and `TogglePin` — each contains only an `Update` mutation lambda. `PadWindow.ResetSettings` is separately allowed for its explicit `new Settings { ... }` replacement initializer; the replacement itself still goes through `Replace`.
- `SettingsWindow.ThemePage`, `AccentDots`, `UiPage`, `EditorPage`, `FeelPage`, `TuningRows` and `SystemPage` — their setting callbacks go through the existing Set/Store helpers into `Update`.
- `SearchOverlay.ToggleGrid` — its callback is an `Update` mutation lambda.
- `SettingsService` and nested types — owner implementation; `SelfTest` and nested types — fixtures.

No production Welcome exemption is needed: both Program and fixtures supply a service. Direct test fixture assignments remain allowed by the SelfTest rule. Store normalization remains service-owned.

Guard proof: temporarily added `TemporarySettingsSetterGuardProbe` to `SearchOverlay`, containing `_window.Settings.FontSize = 1`. The selftest printed `SELFTEST FAIL` and:

> settings setter violation: AntiNotepad.UI.SearchOverlay.TemporarySettingsSetterGuardProbe -> AntiNotepad.Model.Settings.set_FontSize

It also printed `LAYERS Settings.set_* outside owner 1`. The probe was removed; the final clean run reports 0.

## Zoom finding

The old zoom path applied look to both panes and called `Edited`. The shared `Appearance` path also applies topmost/taskbar policy, resolves and applies appearance, refreshes chrome/bars/blur and any open overlay/settings look, then calls `Edited`; static path inspection shows it does more work than the old zoom path. It is the same processing path used for a font-size change from Settings, and Zoom now incurs that path once with no extra local appearance work. The clamped no-op case keeps the prior short refresh/save behavior because the service intentionally emits no event for a no-op. No timing benchmark or live GUI check was run.

## Verification

- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A40-check` — passed, 0 warnings and 0 errors.
- `build/A40-check/AntiNotepad.exe --selftest` — `SELFTEST PASS`; all eight A39 layer lines, all three r1 A40 layer lines, `LAYERS Settings.set_* outside owner 0`, `SETTINGS Changed`, and the prior checks passed.
- `SETTINGS Changed` additionally verifies a Split-only update emits `Split | Persistence` without reaching the appearance subscriber, and `SetLanguageEnabled` emits one DisabledLanguages change with its mapped flags.
- Source search found no remaining direct runtime writer outside the allowed set. No app or installed copy was launched or stopped. `.coord/STATUS.md` was not edited by this run.
