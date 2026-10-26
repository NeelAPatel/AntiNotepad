# A40-settings-change-event

Status: r1 implementation complete on `reorg/A40-settings-change-event-impl`, 2026-10-26. No commit. r2 remains a separate gated run.

## Changed files

- `product/AntiNotepad/Model/SettingsService.cs` — new settings owner, change flags, property map, scalar/list diff, queued event dispatch.
- `product/AntiNotepad/Model/Store.cs` — owns the service; keeps `Data.Settings` aligned with `Current`; load and normalize through the owner.
- `product/AntiNotepad/Program.cs` — exposes the composed service, passes it to the pad, and attaches the one ordered change handler.
- `product/AntiNotepad/UI/ISettingsHost.cs` — small host interface for palette, icon, hotkeys, toast, save, reset, help, show and focus commands.
- `product/AntiNotepad/UI/PadWindow.cs`, `PadWindow.Keys.cs` — service-backed settings, host implementation, ordered change reaction, reset/undo replacement.
- `product/AntiNotepad/UI/SettingsWindow.cs` — takes the service and host; Set/Store mutate through `Update`.
- `product/AntiNotepad/SelfTest.cs` — exact-type layer rules and `SETTINGS Changed` behavior check.
- `.coord/STATUS.md` — r1 progress and gate state.
- `.coord/reports/A40-settings-change-event.md` — implementation and verification report.

`Settings` remains the same JSON POCO/schema. The background writer still serializes detached `CopyForSave()` snapshots; it does not raise `Changed`.

## Property-to-flags table

Every changed persisted property includes `Persistence`. `Appearance` marks settings that run the existing appearance chain. Geometry, Split and Search deliberately omit `Appearance`; unknown writable Settings properties fall back to `Appearance | Persistence`.

| Flags | Properties |
|---|---|
| `Appearance | Palette | Persistence` | Theme, LightPalette, DarkPalette, Accent, Tone, SplitPalette |
| `Appearance | Persistence` | Material, Tint, UiScale |
| `Appearance | EditorLook | Persistence` | Font, FontSize, TextWidth, LineSpacing, Paper, PaperStrength, Bubbles |
| `Appearance | Analysis | Persistence` | InlineMath, ShortenLinks, CleanMarkdown, IndentGuides, Folding, DisabledLanguages |
| `Appearance | Caret | Persistence` | CaretStyle, CaretBlink, CaretGlide |
| `Appearance | Motion | Persistence` | ReduceMotion, Scrolling, LinesPerNotch, ScrollDistance, ScrollBoost, FreeFriction, SwipeLength, SwipeFiresAt, Swipe |
| `Appearance | Chrome | Persistence` | Chrome, FadeAfterMs, Bars, EdgeFade, MenuActionRow, MenuColorIcons |
| `Appearance | Hotkeys | Persistence` | Hotkey, NewNoteHotkey |
| `Appearance | WindowPolicy | Persistence` | FreshNoteAfterMinutes, HideOnBlur, DimWhenPinned, OpenAt, Pinned |
| `Appearance | Taskbar | Persistence` | Taskbar |
| `Geometry | Persistence` | Width, Height, X, Y |
| `Split | Persistence` | Split, SplitSlot |
| `Search | Persistence` | SearchGrid |
| `Persistence` | Welcomed |

`DisabledLanguages` is compared as a copied list. Other mapped values use scalar equality. The map is the single classification table; reflection adds any future writable property missing from it with the safe fallback flags.

## Ordering, batching and ownership

The composition root attaches one `Changed` handler, `PadWindow.HandleSettingsChanged`. For appearance-affecting changes it preserves the order: `Topmost`, taskbar proxy, `ApplyTaskbarSetting`, `ApplyAppearance`, then `Edited()`. `ApplyAppearance` retains the existing order, including `Anim.Reduced` and the disabled-language hash before pane `ApplyLook`. Geometry, split and search batches routed through the service call `ScheduleSave()` only; they skip the appearance chain and `Edited()`'s chrome refresh. `Edited()` retains the 400 ms trailing save timer for the full appearance path.

Nested `Update` calls inside one mutation share the outer snapshot and emit once. If a `Changed` subscriber requests an update, the service queues its mutation until all subscribers finish the current event, combines queued mutations into one follow-up batch, and dispatches it without recursion. No-op updates emit nothing. `Replace` changes `Current` identity and emits one all-flags batch. The service asserts that calls stay on its creating thread.

Store load replaces through the service, and normalization uses an owner-internal `Update` before UI subscribers are attached. `Data.Settings` is synchronized by the service replacement callback. Reset and Undo use `Replace`; they preserve Width, Height, X, Y, Split, SplitSlot and `Welcomed = true`, re-register both hotkeys, refresh both pane references and refresh Settings/preview references. Hotkey edits still register before committing. `SettingsWindow` and its parts receive `SettingsService` and `ISettingsHost`; neither settings source file refers to `PadWindow`.

## Guard proofs

- Temporarily added a compile-safe `SettingsWindow` method accepting `PadWindow`. The selftest printed `LAYERS SettingsWindow->PadWindow 1 refs` and failed with: `layer violation SettingsWindow->PadWindow: AntiNotepad.UI.SettingsWindow -> AntiNotepad.UI.PadWindow`. Removed the method.
- Temporarily added a `SettingsService` method accepting `AntiNotepad.UI.ISettingsHost` and `AntiNotepad.Editor.PadView`. The selftest printed `LAYERS SettingsService->UI 1 refs` and `LAYERS SettingsService->Editor 1 refs`, then failed with: `layer violation SettingsService->UI: AntiNotepad.Model.SettingsService -> AntiNotepad.UI.ISettingsHost` and `layer violation SettingsService->Editor: AntiNotepad.Model.SettingsService -> AntiNotepad.Editor.PadView`. The existing namespace rules also reported those same temporary edges. Removed the method.
- Final clean selftest printed all eight original A39 `LAYERS` lines at `0 refs`, plus `LAYERS SettingsWindow->PadWindow 0 refs`, `LAYERS SettingsService->UI 0 refs`, `LAYERS SettingsService->Editor 0 refs`, `SETTINGS Changed`, and `SELFTEST PASS`.

## Still open and r2 inventory

Folding still installs/uninstalls only on note Bind; InlineMath analysis still updates only on Analyse. This run did not change either behavior.

The remaining runtime writers to migrate in r2 are:

- `Model/Welcome.cs` — `Welcomed` seed write.
- `UI/PadWindow.Keys.cs` — zoom writes `FontSize`.
- `UI/PadWindow.cs` — `RememberBounds` writes X, Y, Width and Height.
- `UI/PadWindow.Navigation.cs` — SplitSlot conflict repair/selection/cycle, Split toggle and Pinned toggle.
- `UI/SearchOverlay.cs` — SearchGrid toggle (also supplies its currently missing save scheduling).
- `UI/SettingsWindow.cs` — DisabledLanguages Remove/Add list edit; move this to an owner method such as `SetLanguageEnabled`.

The SettingsWindow property assignments already run inside the service-backed Set/Store `Update` helpers. `SelfTest.cs` has test-only setup writes for Scrolling, Swipe, Taskbar and DisabledLanguages, including the background-save fixture's detached-list check; keep those as test fixtures. Store normalization is already inside the service owner path.

## Verification

- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A40-check` — succeeded, 0 warnings, 0 errors.
- `build/A40-check/AntiNotepad.exe --selftest` — `SELFTEST PASS`, including existing checks, A39 layer lines, the three new layer lines and `SETTINGS Changed`.
- `rg -n "SettingsChanged\(\)" product/AntiNotepad` finds only `SmoothCaret.SettingsChanged` and its `PadView` call.
- `rg -n "PadWindow" product/AntiNotepad/UI/SettingsWindow.cs product/AntiNotepad/UI/SettingsParts.cs` — no matches.
- No GUI or installed copy was launched or stopped. I resolved `build/A40-check` to its exact workspace path, but the command policy rejected recursive removal (`blocked by policy`); the check output remains in that folder.
