# A41b-clipboard-policy: move the clipboard dedupe/format rules into one testable class

Role: worker (gpt-6-luna xhigh). **Implementation.** Product `product/AntiNotepad`. Read `AGENTS.md`, then the plan
`.coord/tasks/A41b-clipboard-policy-plan.md` (all of it) and `.coord/reports/A41b-clipboard-policy-impact.md`. Branch
`reorg/A41b-clipboard-policy` (already checked out; do not switch branches, do not commit: you cannot write to `.git`).
Behavior-neutral refactor. OCR, save scheduler, navigation, A42-A44 are OUT of scope.

## Files in scope
- New `Editor/ClipboardCapturePolicy.cs`: `internal sealed class`, namespace `AntiNotepad.Editor`.
- `UI/PadWindow.Navigation.cs`: the clipboard-capture region only (fields, `CopyText`, `OnClipboardChanged`), plus a one-line
  policy creation in the `PadWindow` constructor (`UI/PadWindow.cs`) BEFORE `InstallHooks`. Eager, no lazy init.
- `SelfTest.cs`: new `CLIPBOARD policy` check and new `LayerRules` rows (mirror the `HotkeyRegistrar` rows).
- Nothing else.

## Goal
The policy class owns `_ownClipboard` and `_lastCaptured` (raw strings), own-copy recording, the blank/own/last filters, the
"already contained" test and the append formatting. Everything else stays in the window in exactly the same order.

## Proposed API (names may differ, shape must not)
```
void RecordOwnCopy(string text);                  // raw assign
TextCheck CheckText(string? text);                // Blank | OwnCopy | LastCaptured | Eligible; read-only
bool Decide(string text, string? existingText);   // contained => false (last unchanged); else records raw last text, true
static string FormatAppend(string text, bool endsWithLf);
```
- `CheckText`: `string.IsNullOrWhiteSpace` => Blank; raw `==` own => OwnCopy; raw `==` last => LastCaptured. Precedence
  Blank, then own, then last. All comparisons raw (no trim).
- `Decide`: `existingText?.Contains(text.Trim()) == true` => return false without touching last (a null existing counts as
  not contained); otherwise `_lastCaptured = text` (raw) and true.
- `FormatAppend`: `(endsWithLf ? "" : "\n") + "- " + text.Trim().Replace("\r\n", "\n").Replace("\n", "\n  ")`. Keep the
  exact expression semantics (ordinal `Contains`, the same Replace sequence).

## Window wiring: order must be kept exactly
`OnClipboardChanged` keeps its own body and statement order:
1. OCR-first block unchanged (`_awaitingSnip`, strict `< 120` s, `Ocr.ClipboardHasImage()`, `OcrIntoPane(_snipPane ?? _active)`).
2. `target` pick unchanged; `Clipboard is not { } clipboard` check unchanged.
3. `Native.ClipboardIsPrivate()` check BEFORE the clipboard text is read. No `isPrivate` flag goes into the policy.
4. `TryGetTextAsync` in the same try/catch.
5. `CheckText(text)`: anything but Eligible => return.
6. `target.PeekText()` evaluated only after step 5 (stays lazy), then `Decide(text, PeekText())`; false => return.
   No `await` between `CheckText` and `Decide`.
7. `PadView.DocumentFor(target)` AFTER `Decide` (so last is set before the insert), `doc.Insert(doc.TextLength, FormatAppend(text, doc.Text.EndsWith('\n')))`,
   `if (!IsVisible) _hiddenAt = DateTime.Now;`, `Edited();`.
`CopyText`: `RecordOwnCopy(text)` first, then the `SetTextAsync` as today (own copy recorded before the write).
Leave alone: the editor menu Copy and plain Ctrl+C bypass `CopyText` (known behavior, do not fix). No new dispatcher post,
subscription, timer, task or lock; `Dispatcher.UIThread.Post` count unchanged. `rg "_ownClipboard|_lastCaptured" product/AntiNotepad/UI` must find nothing.

## Guards in SelfTest.cs
1. `CLIPBOARD policy` table, one printed line in the style of the `HOTKEYS swap` / `SETTINGS` lines, failures to the same
   failure list so `SELFTEST PASS` depends on it. Cases (fresh policy per row unless sequential):
   - null / "" / whitespace => Blank.
   - after `RecordOwnCopy("x")`: `CheckText("x")` = OwnCopy; `" x "` and `"X"` = Eligible.
   - `Decide("x","clipboard")` true; then `CheckText("x")` = LastCaptured; `" x "` = Eligible.
   - `Decide(" cat ","clipboard\nconcatenate")` false (substring, no word boundary); `Decide("Cat", same)` true.
   - after accepted "old", a rejected "cat" leaves `CheckText("old")` = LastCaptured.
   - `Decide("a\r\nb","a\nb")` true (no CRLF normalization in containment); `Decide(" a ","a")` false (trimmed).
   - `FormatAppend(" \r\nalpha\r\n  beta\r\n ", false)` = `"\n- alpha\n    beta"`.
   - `FormatAppend("a\nb", true)` = `"- a\n  b"`; `FormatAppend("a\rb", true)` = `"- a\rb"`.
   - `FormatAppend(" x ", true)` = `"- x"`; `(" x ", false)` = `"\n- x"`.
   - `Decide("x", null)` true.
   - `RecordOwnCopy(" ")` then `CheckText(" ")` = Blank; own beats last (same text both recorded => OwnCopy).
   - after `Decide` true with no simulated insert, `CheckText` still = LastCaptured.
   Do not claim the test covers password-manager privacy; it is string rules only.
2. New `LayerRules` rows, exact source `typeof(Editor.ClipboardCapturePolicy)` (existing schema; existing rows unchanged).
   Forbidden namespaces `UI`, `Platform`, `Motion`; forbidden exact types `PadWindow`, `PadView`, `Program`, `Native` (the
   `Platform` namespace row may already cover it: if `Native` is outside `Platform`, add it by exact type),
   `Avalonia.Controls.Window`, `Avalonia.Controls.Control`, `Avalonia.Threading.Dispatcher`,
   `AvaloniaEdit.Document.TextDocument`. Row names follow `new("ClipboardCapturePolicy", "<target>", ...)`.
   If the schema cannot express a namespace row with `ExactFrom`, follow how the `SettingsService` rows do it.
Do not prove the guards fail yourself; the manager does that.

## Rules
- Do not touch: `InstallHooks` order, the joined statement line near `PadWindow.cs:121`, OCR code (`OcrIntoPane` and friends),
  `Native`, `Ocr`, `PadView`, hotkey/registrar code, `ShowResident`/`HideResident`/`Prewarm`/`FocusActive`.
- Never cache `Settings`. US spelling, match surrounding comment density, no new library.
- Do not launch or kill AntiNotepad. Do not commit.
- Build: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A41b-codex-check` (0 warnings), then
  `build/A41b-codex-check/AntiNotepad.exe --selftest` must print `SELFTEST PASS` with all existing `LAYERS`/`SETTINGS`/`HOTKEYS`
  lines plus the new ones. Delete `build/A41b-codex-check` afterwards.

## Done means
- Build 0 warnings 0 errors; `SELFTEST PASS` including `CLIPBOARD policy` and the new `LAYERS` rows.
- `git diff` shows `OnClipboardChanged` with the statement order above; untouchable members unchanged; Post count unchanged.
- Short report at `.coord/reports/A41b-clipboard-policy.md`: files changed, new type names, Post count before/after, selftest
  output lines, any deviation from this brief and why.
