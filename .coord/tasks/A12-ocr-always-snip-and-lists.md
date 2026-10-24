# A12-ocr-always-snip-and-lists

You are a worker (gpt-6-luna xhigh). Repo: this folder, a git worktree on branch `feat/A12-ocr-always-snip-and-lists`. Never switch
branch, merge or commit (.git is read-only for you). App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10). Read `AGENTS.md`.
Context: A10-ocr-repeat-and-target changed `/ocr` (`UI/PadWindow.Navigation.cs`, OCR section). The user tested it and reported
two problems. Match the surrounding style. First-party only.

## 1. `/ocr` must always start a screen snip
User report: "ocr still automatically pastes if the last item on the clipboard was an image. It should always do a screenshot first."
- `RunOcr` must **always** start a new snip. Remove the path that reads an image already on the clipboard, and the clipboard
  sequence-number tracking added only for it (`_lastOcrClipboardSequence`; and `Native.ClipboardSequenceNumber` with its
  P/Invoke, if nothing else uses them).
- Keep the snip-complete path (`OnClipboardChanged` with `_awaitingSnip`), and A10's capture of the target note, which inserts
  into the note you started from.
- Check that the snip-complete path can't fire on an image that was on the clipboard **before** the snip began. Today it reacts
  to the next clipboard change while awaiting, which is right; confirm nothing triggers it immediately.
- Update the `/ocr` summary in `Editor/Completion.cs` to "Read text from a screen snip".

## 2. `/ocr` doesn't work in list notes
User report: "does not work on list type notes". A list note has `list` on line 1, and its lines get `[ ] ` boxes
(`Editor/PadView.cs` `ContinueList`, list-mode handling). Slash commands open from `Completion.OnTextEntered` when `/` is typed at
the start of a line or after a space.
- **Find the root cause first** and state it in the report. Candidates to check:
  - the `/` after `[ ] ` not opening the completion;
  - Enter being taken by `ContinueList` before the completion window accepts the item;
  - the command running but the snip or insert failing in list mode.
- Fix it at the cause. Other slash commands (`/sort`, `/count` and so on) must work in list notes too, and a bulleted or numbered
  list inside a plain note must behave the same way.
- Keep list behavior otherwise unchanged: Enter continues with `[ ] `, and Enter on an empty item ends the list.

## Files in scope
`UI/PadWindow.Navigation.cs` (OCR section only), `Platform/Native.cs` (only to remove the unused P/Invoke), `Editor/Completion.cs`,
`Editor/PadView.cs` (only the key, list or completion handling the root cause needs). Other workers are editing PadView's hint text,
Settings, timers and the calculator in parallel: keep PadView edits minimal.

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a12-check`: 0 errors, 0 warnings. `--selftest`
  PASS. Never launch or kill AntiNotepad, and never start the snipping tool.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A12-ocr-always-snip-and-lists.md`: the root cause of item 2 with file:line, the fixes, the files
  changed, and exact manual test steps for the manager.
