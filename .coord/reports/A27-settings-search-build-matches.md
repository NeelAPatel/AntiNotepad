# A27-settings-search-build-matches

## Design

The seven page methods remain the only definitions of Settings rows. `BuildSection` now accepts an optional search query, and the existing `Row`, `SubRow`, and `Expander` helpers use the same title/description tags to decide whether to invoke row control factories. Icons, right-side controls, palette chips, tiles, pictures, and nested controls are created only for matching rows. The result builder keeps the existing section and row order and reuses the live controls.

The 150 ms typing debounce remains in place. Conditional rows still come from the current settings, including Tint when Material is not Solid. Matching nested rows keep their existing 52 px indent and are made visible in results as before.

The recovered-folder description is cached per page build. Its file count is deferred until the row is shown or a query could match the count/status line. Queries that match neither the folder path nor the static description can skip directory enumeration; digit, newline, and status-word queries resolve the count when needed. The Start with Windows registry read is deferred until that row's control is built. Remaining work for nonmatching rows is limited to cheap titles/descriptions, current-setting conditions and labels, small language-count calculation, and section/group/card shells.

## Files changed

- `product/AntiNotepad/UI/SettingsWindow.cs` — query-aware page builds, lazy row factories, and deferred recovered-folder count.
- `product/AntiNotepad/UI/SettingsParts.cs` — match gating in the shared row helpers.
- `product/AntiNotepad/SelfTest.cs` — one pure matching check and its call from `Run`.

## Timings

No timings were collected in this rework. The previous numbers came from the removed selftest harness and are omitted.

## Verification

- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a27-check` — 0 warnings, 0 errors.
- `build/a27-check/AntiNotepad.exe --selftest` — `SELFTEST PASS`.
- The A27 selftest check verifies case-insensitive matching, ignores the `sub ` prefix, and confirms a null query matches everything.
- `git diff --check` — passed.

## Needs a live check

Check Settings search appearance and interaction in the dev copy: search `palette`, `taskbar`, and `tint`; switch Material between Acrylic and Solid and confirm Tint follows the current setting; search for a nested setting and confirm indentation; then change a matched control and confirm it still applies and the results refresh as before.

No live UI check was performed. The app was not launched.

## Rework

Removed the Settings-window test harness and all of its test-only plumbing: `SearchBuildMatchesSelfTest`, `BuildNormalSearchRows`, `SearchResultRows`, `SearchResultBorders`, `BuildAllSearchResults`, `SelfTestCursorFactory`, `SelfTestCursor`, `_selfTestSettings`, and `_controlFactoryObserved`. Removed the reflection/proxy-only usings; restored `S` and both hotkey status lambdas to read directly from `_pad`; restored `_scroll` and `_rail` as readonly; and removed the harness call from `SelfTest.cs`. `CreateControl` only forwarded a factory invocation, so those calls are now inline.

Kept one small check at the end of `SelfTest.cs`. It exercises `SearchMatches` without creating an Avalonia platform or window: case is ignored, the `sub ` prefix is excluded from matching, and a null query matches everything. No row-builder equivalence, factory-count, empty-result UI, or timing check remains in selftest; Settings search appearance and interaction need a live check.

The requested Release build and `--selftest` gate passed after this rework. Cleanup of `build/a27-check` was attempted but the shell policy rejected the recursive removal; the folder remains in this worktree.
