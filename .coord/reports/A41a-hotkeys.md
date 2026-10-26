# A41a-hotkeys report

## Changes

- `product/AntiNotepad/Platform/HotkeyRegistrar.cs`: added `IHotkeyRegistration`, the `GlobalHotkeyRegistration` adapter, and `HotkeyRegistrar` for both hotkey slots, registration status, rollback, status events, and idempotent disposal.
- `product/AntiNotepad/UI/PadWindow.cs`: constructs the registrar before `InstallHooks`, supplies the two existing dispatcher-post actions and native factories, forwards status/events and registration calls, and disposes the registrar in `CompleteQuit`.
- `product/AntiNotepad/SelfTest.cs`: added fake-seam rollback/disposal/status checks and three exact-type `LAYERS` guards.

New types: `IHotkeyRegistration`, `GlobalHotkeyRegistration`, `HotkeyRegistrar`; selftest fakes: `FakeHotkeyOutcome`, `FakeHotkeyRegistration`, `FakeHotkeyFactory`.

## Verification

- Dispatcher posts across `PadWindow*.cs` plus `HotkeyRegistrar.cs`: **11 before, 11 after**.
- Frame-style grep (`ShowInTaskbar|WindowDecorations|ExtendClientArea|SetWindowLong|DwmSetWindowAttribute`): unchanged, 5 matches before and after.
- Protected lifecycle members and `HandleSettingsChanged`: no changes in the diff.
- Build: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A41a-codex-check` — 0 warnings, 0 errors.
- `build/A41a-codex-check/AntiNotepad.exe --selftest` — exit 0:

```text
LAYERS Editor->UI 0 refs
LAYERS Model->UI 0 refs
LAYERS Model->Editor 0 refs
LAYERS Calc->*any other AntiNotepad namespace* 0 refs
LAYERS Theme->UI 0 refs
LAYERS Theme->Editor 0 refs
LAYERS Platform->UI 0 refs
LAYERS Platform->Editor 0 refs
LAYERS HotkeyRegistrar->PadWindow 0 refs
LAYERS HotkeyRegistrar->PadView 0 refs
LAYERS HotkeyRegistrar->Program 0 refs
LAYERS SettingsWindow->PadWindow 0 refs
LAYERS SettingsService->UI 0 refs
LAYERS SettingsService->Editor 0 refs
LAYERS Settings.set_* outside owner 0
SETTINGS Changed
HOTKEYS swap same-label no-op; release-before-create; refusal rollback and failed-restore status; blank new-note disable; idempotent disposal; parse toast; status events
IDENTITY AntiNotepad, avares://AntiNotepad/antinotepad.ico; both icons load; stable instance/data/resource identities
A18-search-speed timing: 200 notes (100 unloaded), old UI filter 30.757 ms; snapshot/context 0.588 ms, 0 disk reads; cold background query 12.405 ms, 100 disk reads.
SELFTEST PASS
```

## Deviation

The brief's successful-rollback case says the refusal error should appear in `HotkeyStatus`. The existing verbatim status expression reports the restored old gesture when restoration succeeds and shows `"{label} unavailable: {error}"` only when the slot ends empty. I preserved that behavior; the selftest verifies the refusal status text when both the replacement and restoration fail.
