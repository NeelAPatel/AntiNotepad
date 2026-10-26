# A42a-math-notes: move the math-line helpers and the list-prefix parser out of PadView

Role: worker (gpt-6-luna xhigh). **Implementation.** Product `product/AntiNotepad`. Read `AGENTS.md`, then the plan
`.coord/tasks/A42-padview-commands-plan.md` (the "Recommendation on handling" section supersedes the original scope) and
`.coord/reports/A42-padview-commands-impact-r2.md` (esp. "Concrete LAYERS rows"). Branch `reorg/A42a-math-notes` (already checked
out; do not switch branches, do not commit: you cannot write to `.git`). Behavior-neutral refactor, verbatim moves.

## Files in scope
- New `Editor/MathNotes.cs`: `internal static class MathNotes`, namespace `AntiNotepad.Editor`. Takes `EvaluateMath`,
  `BlankLabel`, `StripListMarker` from `Editor/PadView.cs` (~:535-580), bodies and doc comments verbatim.
- New `Editor/ListEditing.cs`: `internal static class ListEditing`, namespace `AntiNotepad.Editor`. Takes the `ListPrefix` regex
  (PadView.cs:32) and both `TryListNumber` overloads (:764-772). Same pattern, same `RegexOptions.Compiled`, still a static
  readonly field (never constructed per call). `ListPrefix` and `TryListNumber(Match, ...)` become `internal`.
- `Editor/PadView.cs`: delete the moved members; remaining uses (`IsListLine`, `Renumber`, `ContinueList`, `Indent`-adjacent code)
  call `ListEditing.ListPrefix` / `ListEditing.TryListNumber`. Drop usings that become unused ONLY if the build says so
  (0 warnings required; do not otherwise touch usings).
- `Editor/PadView.cs` `NoteAnalysis` class (bottom of file, ~:1066-1077): `PadView.StripListMarker`/`PadView.BlankLabel`/
  `PadView.EvaluateMath` become `MathNotes.*`. Nothing else in `NoteAnalysis` changes; it does NOT move (that is A42b).
- `SelfTest.cs`: rename call sites (:63, :64 `BlankLabel`/`StripListMarker`, :908 `TryListNumber`) to the new classes; add new
  checks and `LayerRules` rows below.
- Nothing else. Out of scope: moving `NoteAnalysis`, `TimerKey`, `ChangedLineRange`, checkbox/indent/continue/renumber logic,
  `ChipFor`, folding, `CheckboxGenerator.Box`, `TextCommands`.

## Goal
`MathNotes` and `ListEditing` are UI-free homes for text logic that today hangs off the control, so `NoteAnalysis` no longer
references `PadView` for math. Exactly the same results for every input. Do not "fix" anything (not the InlineMath/Folding
live-refresh limits, not negative-index preconditions, not redundant returns).

## Hot-path rules
Per-keystroke analysis calls these. No new allocations in loops, no new Regex construction per call (`BlankLabel` keeps its
literal `Regex.Match` call exactly as is), no LINQ added, no extra copies. Visibility stays `internal static`.

## Guards in SelfTest.cs
1. New focused checks (small, in the style of the existing `Check(...)` lines; failures go to the same failure list so
   `SELFTEST PASS` depends on them). Re-verify each expected value against the real code before asserting:
   - `BlankLabel("spent 12.50 + 8.25 =")` blanks "spent " to six spaces, rest unchanged.
   - `StripListMarker`: `"- 19.99 * 3 ="` bullet replaced by equal-length spaces; `"2. 19 + 3 ="` returned unchanged
     (numbered marker kept); `"  - [x] 3 + 4 ="` first eight characters blanked.
   - `EvaluateMath(["math", "lunch 12 + 3 =", "x = 4", "x * 2 ="])`-style input: the label line yields a result, variables survive
     the retry, and the label line's first token is a Comment token starting at 0 with the label length; `"- 2 + 3 ="` as a
     bullet line evaluates to a positive 5. Pick fixtures that exist cleanly (avoid `Calculator.Clock`-dependent input).
   - `ListEditing.TryListNumber`: 9 digits accepted, 10 digits rejected (the existing 12-digit check stays).
2. New `LayerRules` rows, same schema and style as the `ClipboardCapturePolicy` rows (existing rows unchanged). For BOTH
   `typeof(Editor.MathNotes)` and `typeof(Editor.ListEditing)`: namespace rows `UI`, `Platform`, `Motion`, `Theme` (ExactFrom
   only); exact-type rows `PadWindow`, `PadView`, `Program`, `Native`, `Avalonia.Controls.Window`, `Avalonia.Controls.Control`,
   `Avalonia.Threading.Dispatcher`, `AvaloniaEdit.Document.TextDocument`, `Timers`, `Settings`, `SettingsService`. Plus
   `("ListEditing", "CheckboxGenerator", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Editor.CheckboxGenerator))`.
   Use the real type names/namespaces that exist (check how `Timers`, `Settings`, `SettingsService` are spelled in the existing
   rows); the namespace matcher only understands `AntiNotepad.<segment>`, so do not add `Avalonia.*` namespace rows.
   Do NOT add a `NoteAnalysis -> PadView` row: that belongs to A42b. Do not prove the guards fail yourself; the manager does.

## Rules
- Verbatim moves: copy bodies character for character; only qualifiers change. No renames of public/internal surface other than
  the move. US spelling, match surrounding comment density, no new library, never cache `Settings`.
- Do not launch or kill AntiNotepad. Do not commit.
- Build: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A42a-codex-check` (0 warnings), then
  `build/A42a-codex-check/AntiNotepad.exe --selftest` must print `SELFTEST PASS` with all existing `LAYERS`/`SETTINGS`/`HOTKEYS`/
  `CLIPBOARD` lines, the A29 analysis checks, plus the new ones. Delete `build/A42a-codex-check` afterwards.

## Done means
- Build 0 warnings 0 errors; `SELFTEST PASS` with the new checks and `LAYERS` rows.
- `grep -n "BlankLabel\|StripListMarker\|EvaluateMath\|TryListNumber\|ListPrefix" Editor/PadView.cs` finds only
  `MathNotes.`/`ListEditing.` call sites (no definitions); `grep -rn "PadView\.\(BlankLabel\|StripListMarker\|TryListNumber\|EvaluateMath\)" product` finds nothing.
- Short report at `.coord/reports/A42a-math-notes.md`: files changed, PadView line count before (1,190) and after, selftest
  output lines (new checks and LAYERS summary), any deviation from this brief and why.
