# A41b-clipboard-policy

## Result

Moved raw clipboard dedupe state, filtering, containment and append formatting into `Editor.ClipboardCapturePolicy`. The window
keeps OCR, target selection, privacy-before-read, lazy `PeekText`, and document effects in their prior order. `CopyText` records
before writing to the clipboard; construction is eager.

## Files changed

- `product/AntiNotepad/Editor/ClipboardCapturePolicy.cs` — new `ClipboardCapturePolicy` type and nested `TextCheck` enum.
- `product/AntiNotepad/UI/PadWindow.Navigation.cs` — clipboard region wiring.
- `product/AntiNotepad/UI/PadWindow.cs` — eager policy creation in the constructor.
- `product/AntiNotepad/SelfTest.cs` — 12-case `CLIPBOARD policy` check and 12 `ClipboardCapturePolicy` layer rules.
- `.coord/STATUS.md` — implementation gate status.

No `_ownClipboard` or `_lastCaptured` references remain under `product/AntiNotepad/UI`. `Dispatcher.UIThread.Post` count in
`product/AntiNotepad` was 25 before and 25 after.

## Verification

`AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A41b-codex-check` succeeded with 0 warnings and
0 errors. The built `AntiNotepad.exe --selftest` exited 0 with `SELFTEST PASS`.

Relevant selftest output:

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
LAYERS ClipboardCapturePolicy->UI 0 refs
LAYERS ClipboardCapturePolicy->Platform 0 refs
LAYERS ClipboardCapturePolicy->Motion 0 refs
LAYERS ClipboardCapturePolicy->PadWindow 0 refs
LAYERS ClipboardCapturePolicy->PadView 0 refs
LAYERS ClipboardCapturePolicy->Program 0 refs
LAYERS ClipboardCapturePolicy->Native 0 refs
LAYERS ClipboardCapturePolicy->Window 0 refs
LAYERS ClipboardCapturePolicy->Control 0 refs
LAYERS ClipboardCapturePolicy->Dispatcher 0 refs
LAYERS ClipboardCapturePolicy->TextDocument 0 refs
LAYERS SettingsWindow->PadWindow 0 refs
LAYERS SettingsService->UI 0 refs
LAYERS SettingsService->Editor 0 refs
LAYERS Settings.set_* outside owner 0
SETTINGS Changed
HOTKEYS swap same-label no-op; release-before-create; refusal rollback and failed-restore status; blank new-note disable; idempotent disposal; parse toast; status events
CLIPBOARD policy 12 cases
SELFTEST PASS
```

The clipboard check covers string rules only; it does not test password-manager privacy. No guard-failure injection or live UI
hand check was run.

## Deviation

The requested build-output cleanup could not be completed. The task output path resolved inside the repository's `build/` folder,
but the automatic command review rejected `Remove-Item -Recurse -Force` as blocked by policy. The output remains at
`build/A41b-codex-check` for manager cleanup/review.
