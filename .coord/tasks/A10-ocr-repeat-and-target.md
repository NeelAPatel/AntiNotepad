# A10-ocr-repeat-and-target

You are a worker (gpt-6-luna xhigh). Repo: this folder, a git worktree on branch `feat/A10-ocr-repeat-and-target`. Never switch
branch, merge or commit (.git is read-only for you). App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10).
Read first: `AGENTS.md`, `docs/audit/A1-codebase-audit.md` (A5-3, decision 4). Match the surrounding style. First-party only.

## 1. Bug: `/ocr` twice in a row reuses the old snip (reported by the user)
Steps:
1. Run `/ocr`: the screen snip starts, and the recognized text is pasted in.
2. Run `/ocr` again.

Expected: a new snip. Actual: the text from the previous snip is pasted again, so `/ocr` can't be used twice without copying
something else in between.

Cause: `RunOcr` (`UI/PadWindow.Navigation.cs` ~:333) first checks `Ocr.ClipboardHasImage()` and reads that image. After the first
snip, the clipboard still holds the screenshot.

Fix: keep the "copied image" convenience only for an image that is **new since the last OCR**. Track the Win32 clipboard sequence
number (`GetClipboardSequenceNumber` via P/Invoke in `Platform/Native.cs`, next to the existing clipboard code). Record it when an
OCR reads the clipboard. `/ocr` reads the clipboard directly only if the clipboard has an image and the sequence number changed
since that last OCR; otherwise it always starts a new snip. The snip-complete path (`OnClipboardChanged`) is unchanged. Don't
clear or modify the user's clipboard.

## 2. Text lands in the note you started from (A5-3, decision 4: approved)
`OcrIntoPane` awaits recognition, then inserts at `pad.Editor.Document` and the caret. If you switched notes in that pane
meanwhile, the text goes to the wrong note.

Fix: capture the **target note**, its document and the caret offset when `/ocr` runs (for the snip path, when the snip starts),
and insert into that note when recognition finishes:
- If that note is still bound in a pane, insert into that document at the saved offset, clamped to its length.
- If it's no longer shown, append to the note's text through the model so it saves, and toast "Text added to <note title>".
- If the note was deleted, toast that and insert nothing.

## Files in scope
`UI/PadWindow.Navigation.cs` (the OCR section only), `Platform/Ocr.cs`, `Platform/Native.cs` (the clipboard P/Invoke only),
`Editor/Completion.cs` (only the `/ocr` summary, if it should change). Other workers are editing Store, PadView, Settings,
PadWindow's quit and timers in parallel: stay out of them, and keep your Navigation edits inside the OCR section.

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a10-check`: 0 errors, 0 warnings. `--selftest`
  PASS. Delete `build/a10-check` if you can. Never launch or kill AntiNotepad, and never run the snipping tool.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A10-ocr-repeat-and-target.md`: the cause in your own words, the fix for each part, files changed, the
  build and selftest, and the exact manual steps the manager should run in the dev copy to confirm both fixes.
