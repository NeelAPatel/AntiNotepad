# A10-ocr-repeat-and-target

## Cause

`/ocr` treated any image on the clipboard as new input. Windows keeps the last screen snip there, so the next command OCR'd that same image again. Separately, OCR waited for recognition before looking up the pad's current document and caret; switching notes during that wait redirected the result.

## Fixes

- Added `GetClipboardSequenceNumber` in `Native` and remember the sequence used by each OCR read. The copied-image shortcut now runs only when the clipboard has an image whose sequence differs from the last OCR read. An unchanged image starts a new screen snip. The clipboard is left untouched, and the clipboard listener still handles snip completion.
- Capture the target note, document and caret when `/ocr` runs, or when its screen snip starts. On completion, insert at the saved offset, clamped to the document length, if that document is still bound in a pane. If the note is no longer shown, append through the note model and show `Text added to <note title>`. If the note was deleted, show `Target note was deleted · text not added` and discard the result. The hidden-note path also keeps any cached document synchronized.
- Updated the `/ocr` completion summary to say “newly copied image.”

## Files changed

- `product/AntiNotepad/Platform/Native.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs` (OCR section only)
- `product/AntiNotepad/Editor/Completion.cs`
- `.coord/reports/A10-ocr-repeat-and-target.md` (this report)

`product/AntiNotepad/Platform/Ocr.cs` did not need changes; its existing read routine was retained.

## Build and selftest

- Release build passed with 0 warnings and 0 errors using `AVALONIA_TELEMETRY_OPTOUT=1` and `NuGetAudit=false` in the environment. The initial build without `NuGetAudit=false` compiled successfully but emitted two `NU1900` warnings because the sandbox could not reach `https://api.nuget.org/v3/index.json` for vulnerability metadata.
- `build/a10-check/AntiNotepad.exe --selftest`: `SELFTEST PASS`.
- The `build/a10-check` cleanup was attempted, but the shell policy rejected recursive removal; the output directory remains.
- No app or snipping tool was launched. Live UI behavior remains for the manager's dev-copy check.

## Manual dev-copy confirmation

Run these from the repository root:

```powershell
pwsh -NoProfile -File tools/dev.ps1
pwsh -NoProfile -File tools/press-hotkey.ps1 -Key D
```

1. In a note, invoke `/ocr` from the completion list, snip text with a distinctive phrase, and wait for the recognized text. Invoke `/ocr` again without copying anything else. Confirm a new screen snip starts. Snip a different phrase and confirm that second phrase is inserted once, with no duplicate of the first result.
2. Turn on Split view. Put a distinctive marker in the left note and place the caret between two known words. With a newly copied image containing a unique phrase on the clipboard, invoke `/ocr` from the left note, then activate the right pane while recognition is pending. Confirm the phrase is inserted at the saved position in the left note and nothing is inserted into the right note.
3. Turn Split view off. In a titled note, invoke `/ocr` with another newly copied image and navigate to a different note before recognition finishes. Confirm the `Text added to <note title>` toast, return to the original note, and verify the phrase was appended at the end.
4. Start another OCR for a note, then delete that target before recognition finishes. Confirm the deleted-note toast and that no recognized text was added to the note currently shown.
