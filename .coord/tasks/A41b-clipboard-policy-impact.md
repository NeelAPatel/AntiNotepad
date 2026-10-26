# A41b-clipboard-policy-impact: READ-ONLY audit of the clipboard capture path on the latest dev

Role: director (gpt-6.1-sol medium; luna xhigh subagents allowed). Branch `reorg/A41-padwindow-split` (this worktree, merged with
dev 9e8e04d). **READ-ONLY: edit no file except your report `.coord/reports/A41b-clipboard-policy-impact.md`.** No builds into the
tree; never launch or kill AntiNotepad. Product: `product/AntiNotepad`.

## Context
Read first: `AGENTS.md`, `docs/decisions.md`, `.coord/tasks/A41-padwindow-split-plan.md` (A41b section and Recommendation on
handling), `.coord/reports/A41-padwindow-split-impact-r2.md` (clipboard rows). Since those, dev gained A41a-hotkeys (2b85bfb:
`Platform/HotkeyRegistrar`, `HOTKEYS swap` selftest, 3 new LAYERS rules). Candidate task `A41b-clipboard-policy`: move the pure
rules of clipboard capture (own-copy, last-captured, already-contained, trim/indent normalization) out of PadWindow into a pure
class with a table selftest; everything touching the window, Avalonia clipboard, native, OCR, documents stays.
A42-padview-commands (queued) moves PadView text commands and touches `PadView.DocumentFor`.

## Do
1. **Re-map the capture path** on current code: `PadWindow.CopyText`, `OnClipboardChanged`, `_ownClipboard`, `_lastCaptured`,
   `Native.ClipboardIsPrivate`, `Native.ListenClipboard` hook (installed in `InstallHooks`), OCR-first check (`_awaitingSnip`,
   `Ocr.ClipboardHasImage`), target note pick, `PadView.DocumentFor` insert, `_hiddenAt` update, `Edited()`. Give file:line,
   state, ordering, and every caller of `CopyText` (menu copy, chip copy, key copy, `PadView.Copy` event). Note any path that
   copies without going through `CopyText` (the standard editor context-menu Copy).
2. **What A39/A40/A41a changed around it** (diff since b54b476): imports, `ISettingsHost`, `HotkeyRegistrar` neighbors. Also look
   at the odd formatting in `PadWindow.cs` around line 121 (`RegisterNewNoteHotkey(...);        _clipboardHook = ...` on one line)
   and say whether it is a leftover from A41a and whether it is harmless.
3. **Pure vs window**: precisely what can move into a pure policy class (state, method signatures with inputs/outputs, no Avalonia,
   no Native, no PadView, no Dispatcher) and what must stay (OCR-first, private check before text read, target selection, awaits,
   `DocumentFor`, `_hiddenAt`, `Edited`). Propose the exact class name/location (Editor vs Model; check what the LAYERS rules and the
   `HotkeyRegistrar` precedent suggest), its API, and how `OnClipboardChanged`/`CopyText` call it.
4. **Ordering constraints** to preserve, with the reason and line: `_ownClipboard` set before `SetTextAsync`; `_lastCaptured` set
   before `DocumentFor`/insert; image-first before text; private before read; the 120 s snip window; the trim/indent format; the
   "already contained" check uses `PeekText()` of the target. Mention the existing await interleaving hazard (no serialization).
5. **A42 interplay**: read the `claude/a42-padview-commands-*` branch / any A42 brief if present (`git branch -a`, `git show`) and
   state which clipboard lines A42 would touch (`DocumentFor`, `Insert`), whether A41b before or after A42 is cleaner, and the
   conflict surface.
6. **Selftest table and LAYERS rows**: concrete cases (blank, whitespace, own text, repeat of last, already contained, CRLF and
   multiline indent, trailing-newline join, null existing text, order of checks) and how to express them against the pure API;
   which LAYERS rows fit the existing `LayerRule` schema (see `SelfTest.cs`, the A41a rows) and what a deliberate-violation proof
   would look like.
7. **Value check (blunt)**: how many lines are actually pure, what regression risk a test of them removes, show/hide hot-path
   contact (hidden capture updates `_hiddenAt`, which feeds fresh-note logic in `ShowResident`), and whether it is worth doing now
   or after A42.
8. **Uncertain** list.

## Report
`.coord/reports/A41b-clipboard-policy-impact.md`, sections 1-8, file:line for every claim, concise (under 200 lines).

## Done means
Only the report file is added by you.
