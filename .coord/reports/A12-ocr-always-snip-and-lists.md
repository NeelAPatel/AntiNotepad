# A12-ocr-always-snip-and-lists

## Root cause: slash commands in lists

`Completion.OnTextEntered` accepts `/` after whitespace, so typing `/` after a list prefix such as `[ ] ` does open the completion window (`product/AntiNotepad/Editor/Completion.cs:60-63`). The failure was Enter handling: `PadView` registers `OnKeyDown` with tunnel routing (`product/AntiNotepad/Editor/PadView.cs:128`). Its Enter branch called `ContinueList` and marked the event handled without checking whether completion was open (`product/AntiNotepad/Editor/PadView.cs:535-539`). For `/count` on a non-empty checkbox, bullet, or numbered item, `ContinueList` inserted the next item and swallowed Enter before the completion window could accept the command.

The Enter branch now yields while `Completion.IsOpen`. Other slash commands use the same completion path, so this fixes them in list-mode notes and in bulleted or numbered lists inside plain notes. Normal Enter continuation and empty-item list ending still go through `ContinueList` when completion is closed.

## Fixes

- `/ocr` now captures its target note, sets `_awaitingSnip`, and always starts `Native.StartScreenSnip()`. It no longer checks or OCRs an image already on the clipboard. The clipboard sequence-number state and its native P/Invoke were removed.
- The snip-complete handler remains driven by the clipboard listener's next clipboard-update event. `RunOcr` does not call `OnClipboardChanged` or inspect the existing image, so an unchanged image already on the clipboard does not complete a new snip immediately. The captured note, document, and caret still determine where recognized text is inserted.
- The `/ocr` completion summary is now `Read text from a screen snip`.
- List continuation yields Enter to an open completion window.

## Files changed

- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/Platform/Native.cs`
- `product/AntiNotepad/Editor/Completion.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `.coord/reports/A12-ocr-always-snip-and-lists.md`

## Verification

- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a12-check`: 0 warnings, 0 errors, with `NuGetAudit=false` in the process environment. Without that setting, restore reported two `NU1900` warnings because this environment could not reach `https://api.nuget.org/v3/index.json` for vulnerability data.
- `build/a12-check/AntiNotepad.exe --selftest`: `SELFTEST PASS`.
- No app window or snipping tool was launched by the worker.
- The generated `build/a12-check` directory remains in the worktree; automatic workspace policy rejected its recursive cleanup command.

## Manual checks for the manager

1. In the dev copy, open or create a note whose first line is `list`. On a checkbox line, place the caret after `[ ] `, type `/count`, and press Enter to accept it. Confirm the count toast appears and Enter did not add another checkbox. Repeat with another slash command if desired.
2. In a plain note, test `/count` after a bullet prefix (`- `) and after a numbered prefix (`1. `). Confirm Enter accepts the command in both cases.
3. In the list-mode note, type text after `[ ] ` and press Enter; confirm the next line starts with `[ ] `. Press Enter on that empty item; confirm it ends the list. In the plain note, confirm Enter continues a non-empty bullet and numbered item, and ends each empty item as before.
4. Copy an image to the clipboard before invoking `/ocr` in a note. Confirm `/ocr` starts a fresh screen snip instead of immediately reading that image. Select a region and confirm OCR text is inserted at the caret in the note where `/ocr` was started.
