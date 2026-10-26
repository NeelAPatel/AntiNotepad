# A41b-clipboard-policy: plan (against dev 9e8e04d, after A39, A40, A41a)

Status: PLAN ONLY (investigator session; no implementation here). Evidence: `.coord/reports/A41b-clipboard-policy-impact.md`
(Codex director, read-only). Manager spot-check: re-read `PadWindow.Navigation.cs:296-319` (matches the report line for line),
confirmed `PadWindow.cs:121` joined statements date from `b54b476` (not an A41a leftover), confirmed `reorg/A42-padview-commands`
exists and its plan leaves the document cache and `DocumentFor` out of scope (`A42 plan:22-25`). Lines are dev 9e8e04d; re-anchor by name.

## Recommendation on handling
- **Worth doing now?** Yes, but as a **small, optional, low-risk task**, not a size reduction. Only about 8 of the 33 lines at
  `N:287-319` are pure; the class plus tests will add about as many lines as they remove from the window. The value is a selftest
  on the dedupe/format rules (echo, repeat, substring, indent, newline join), which today can only be exercised through a live
  document. Skip it if line count is the goal.
- **Before or after A42?** **Before.** A42's current plan does not touch `DocumentFor`, the cache, or `doc.Insert` (cache extraction
  is out of its scope), so there is no conflict; waiting buys nothing. Only a later revised A42 that extracts the cache would
  touch `N:314`/`N:316`; re-audit then. This reverses my earlier plan's "after A42" advice, which assumed A42 moves `DocumentFor`.
- **Scope:** one new class, `Editor/ClipboardCapturePolicy.cs` (internal sealed, namespace `AntiNotepad.Editor`; not UI, because a
  whole-UI ban row would flag the class itself). It holds the two raw strings (`_ownClipboard`, `_lastCaptured`), own-copy
  recording, the blank/own/last filters, the "already contained" test and the append formatting. Everything else stays in the window.
- **Correction to my earlier plan:** do **not** pass an `isPrivate` flag into the policy. The privacy check must run *before* the
  clipboard text is read (`N:305-309`); a boolean after the read cannot enforce that. The privacy check stays in the window, and
  the selftest does not claim to test password-manager privacy.
- **Riskiest parts and your hand checks (dev copy, `tools/dev.ps1`), a Clipboard-mode note must exist:**
  1. Copy text in another app while the pad is hidden -> it appears as a `- ` bullet in the Clipboard note (and reopening after
     a long hidden idle still behaves as before: hidden capture refreshes the idle timer that decides fresh-note on show).
  2. Copy out of the pad (chip click and Ctrl+Shift+C) -> not echoed back as a new bullet; the same text copied twice -> one bullet;
     copying a piece of text already in the Clipboard note -> not added.
  3. Copy from a password manager (marked private) -> ignored.
  4. Snip-OCR: start a snip, navigate to another note or delete the original, finish the snip -> text lands in the original note.
  5. Multi-line copy (CRLF, indented lines) -> bullet with continuation lines indented two spaces, same as before.
  Known existing behavior to leave alone: the editor menu Copy and plain Ctrl+C bypass `CopyText` (own-copy is not recorded), so
  a selection copied from another note can be captured.

## What
New `Editor/ClipboardCapturePolicy.cs` with a phased API (proposed by the report, preserves original order and lazy `PeekText`):
```
void RecordOwnCopy(string text);                  // raw assign, before the clipboard write
TextCheck CheckText(string? text);                // Blank | OwnCopy | LastCaptured | Eligible (read-only)
bool Decide(string text, string? existingText);   // contained => false; else records raw last text, true
static string FormatAppend(string text, bool endsWithLf);
```
`PadWindow` (`UI/PadWindow.Navigation.cs`): remove `_ownClipboard`/`_lastCaptured`; create one policy eagerly in the constructor before `InstallHooks`; `CopyText` calls `RecordOwnCopy` first; `OnClipboardChanged` keeps its own body and calls the policy at the three points above.

## Why
Regressions here are silent and found late: a changed comparison echoes the pad's own copies or re-adds the last capture; testing
containment after newline normalization instead of before changes what counts as a duplicate; the `"\n" + "- "` join or indent
drifts. Example: `text == _ownClipboard` is a raw compare while containment uses `text.Trim()` (`N:310-312`), a distinction a
future cleanup could easily "normalize". Today none of this is testable without a window and a live `TextDocument`.

## How
1. Add the class and a `CLIPBOARD policy` selftest (table below), `LAYERS` rows, build.
2. Wire `PadWindow` (three call sites), keep ordering: `RecordOwnCopy` before the `Clipboard` lookup and `SetTextAsync`; OCR-first check
   unchanged (`_awaitingSnip`, strict `< 120` s); `target` pick unchanged; `Native.ClipboardIsPrivate()` before `TryGetTextAsync`;
   `CheckText` then (only if Eligible) `target.PeekText()` then `Decide`; then `DocumentFor`, `FormatAppend(text, doc.Text.EndsWith('\n'))`,
   `Insert`, `_hiddenAt` if not visible, `Edited()`. No await between `CheckText` and `Decide`. No new dispatcher post, subscription or lazy init.
3. Do not touch: `InstallHooks` order or the joined line at `PadWindow.cs:121` (harmless, predates A41a), OCR code (`N:334-403`), `Native`, `Ocr`, `PadView`, hotkey/registrar code, show/hide bodies.

Selftest table (fresh state per row unless sequential; escaped newlines):

| Case | Expectation |
|---|---|
| null / "" / whitespace | CheckText = Blank |
| own raw text | after RecordOwnCopy("x"): CheckText("x") = OwnCopy; " x " and "X" = Eligible |
| repeat of last | Decide("x","clipboard") true; CheckText("x") = LastCaptured; " x " = Eligible |
| already contained | Decide(" cat ","clipboard\nconcatenate") false (substring, no word boundary); Decide("Cat", same) true |
| rejection keeps last | after accepted "old", a rejected "cat" leaves CheckText("old") = LastCaptured |
| dedupe before formatting | Decide("a\r\nb","a\nb") true (no CRLF normalization in containment); Decide(" a ","a") false (trimmed) |
| CRLF + indent | FormatAppend(" \r\nalpha\r\n  beta\r\n ", false) = "\n- alpha\n    beta" |
| LF / bare CR | FormatAppend("a\nb", true) = "- a\n  b"; FormatAppend("a\rb", true) = "- a\rb" |
| newline join | FormatAppend(" x ", true) = "- x"; (" x ", false) = "\n- x" |
| null existing | Decide("x", null) true (defensive only; real `PeekText` is non-null) |
| precedence | RecordOwnCopy(" ") then CheckText(" ") = Blank; own beats last |
| record before effect | after Decide true with no simulated insert, CheckText still = LastCaptured |

LAYERS rows (existing `LayerRule` schema, exact source `typeof(Editor.ClipboardCapturePolicy)`, mirroring the A41a rows): forbidden namespaces `UI`, `Platform`, `Motion`; forbidden exact types `PadView`, `Program`, `Avalonia.Controls.Window`, `Avalonia.Controls.Control`, `Avalonia.Threading.Dispatcher`, `AvaloniaEdit.Document.TextDocument`; optionally namespace `Model` (string-only API). External namespace strings cannot be To rows; exact types only. Guard proof (in the implementing run): temporarily add one violating reference per row (a `typeof(...)` return avoids optimizer removal), see `LAYERS ClipboardCapturePolicy->...` fail and name it, remove, rebuild, PASS.

## Success looks like
- Build 0 warnings, 0 errors; `--selftest` PASS with the new `CLIPBOARD policy` line and new `LAYERS` rows; guard-failure proof reported.
- `git diff` shows `OnClipboardChanged` keeps the same statement order (OCR, target, private, read, check, PeekText, decide, DocumentFor, insert, `_hiddenAt`, `Edited`); `ShowResident`, `HideResident`, `Prewarm`, `FocusActive`, `InstallHooks`, OCR code unchanged.
- `rg "_ownClipboard|_lastCaptured" product/AntiNotepad/UI` finds nothing; `Dispatcher.UIThread.Post` count unchanged; `PadWindow.Navigation.cs` shrinks by a few lines only (no line target).
- Hand checks 1-5 above pass; user approves in the implementing session.

## Impact radius
`UI/PadWindow.Navigation.cs` (clipboard region only), new `Editor/ClipboardCapturePolicy.cs`, `SelfTest.cs`. Behavior radius: clipboard-mode capture and its dedupe/format only. Not touched: OCR, Native/Ocr, PadView, show/hide, hotkeys, settings.

## Risks
- Indirect hot-path contact: accepted hidden capture updates `_hiddenAt`, which `ShowResident` uses for the fresh-note decision (`W:142-143`); the update must stay after the insert and only when hidden.
- Order mistakes: `_lastCaptured` must be set before `DocumentFor`/insert; own copy recorded before the write; the 120 s window strict; containment tested on the trimmed raw text before normalization.
- Existing hazard kept as is: independent discarded tasks per clipboard event, awaits can interleave, no serialization, target and privacy can be stale after the read. Not fixed here.
- Tests prove string rules only, not privacy, native notification, target choice, OCR destination or persistence.
- `PeekText` must stay lazy (suppressed events must not load the note body).

## Depends on / conflicts with
- **A39/A40/A41a (landed):** no clipboard-region changes; A41a only moved hotkeys; adds `LAYERS` rows in the same style as its three.
- **A42-padview-commands:** current plan keeps `DocumentFor`/cache/`Insert`-for-lists in `PadView`; do A41b first. If A42 is revised to extract the cache, re-audit `N:314`/`N:316`.
- **A43-app-context:** policy takes no `Program.*`. **A44-platform-seam:** `Native.ClipboardIsPrivate()` and `Native.ListenClipboard` stay in the window for A44 to wrap; no `Native.*` in the policy.
