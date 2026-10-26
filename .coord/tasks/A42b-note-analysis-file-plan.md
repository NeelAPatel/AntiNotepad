# A42b-note-analysis-file: plan

Status: PLAN ONLY (investigator session; no product code changed). Evidence: `.coord/reports/A42b-note-analysis-file-impact.md`
(Codex director, source-derived, nothing built or run), spot-checked against `Editor/PadView.cs` on dev 037f4c4 (A42a merged):
`NoteAnalysis` is a top-level class at :878-1132 (summary :877), `ChangedLineRange` at :493-500, the `TimerKey` shim at :540,
`using System.Runtime.CompilerServices` at :3 (only used by `NoteAnalysis`), and the class body has no `PadView` reference.

## What
- Move `internal sealed class NoteAnalysis` (with private nested records `CachedLine`, `TimerLine`, and `internal static TimerKey`) **unchanged** from `Editor/PadView.cs` to a new `Editor/NoteAnalysis.cs`, namespace `AntiNotepad.Editor`, same type name.
- Move `ChangedLineRange` (PadView.cs:493-500) into `NoteAnalysis` as `internal static` (only change: `private` -> `internal`); `PadView.OnDocumentChanged` (:459) calls `NoteAnalysis.ChangedLineRange(doc, e)`.
- Delete the `PadView.TimerKey` shim (:540); `SelfTest.cs` (about :1018-1019) calls `Editor.NoteAnalysis.TimerKey`.
- Remove the now-unused `using System.Runtime.CompilerServices;` from `PadView.cs`. New file imports: `System.Runtime.CompilerServices`, `AvaloniaEdit.Document`, `AntiNotepad.Calc`, `AntiNotepad.Model` (implicit usings cover System/Linq/Collections). Leave every other PadView import alone.
- Add 16 `NoteAnalysis` layer rules (below).
- Stays in `PadView`: the 140 ms `_analyse` timer, >1500-line pending/union/full-fallback logic, `OnDocumentChanged`, `Analyse`, `ClearPendingAnalysis`, `_analysis` field and all facades (`MathLine`, `InlineLine`, `InlineResult`, `ChipFor`, `Structure`, `Mode`...), `Documents`/`Evict`.

## Why
- After A42a the analysis no longer calls control code, but nothing stops it coming back: the class still lives in the 1.1k-line control file, one edit away from reaching `PadView` statics. A file of its own plus an `ExactFrom`/`ExactTo` rule makes a new `NoteAnalysis -> PadView` (or UI/Avalonia control) reference fail the selftest instead of being found later.
- It is the part of the file with the most careful invariants (A29: DocumentLine-keyed cache, 0/1-based indexes, timer ordinals). Isolated, an edit to the control cannot silently touch it and a reviewer sees it as one unit.
- Honest limit: no typing-speed gain is expected or claimed; this is ownership and guard value only.

## How
1. Cut the summary + class (PadView.cs:877-1132) verbatim into `Editor/NoteAnalysis.cs` with the four imports. Insert `ChangedLineRange` first in the class (private -> internal, body unchanged).
2. In `PadView.cs`: delete the moved text, the old `ChangedLineRange`, the `TimerKey` shim and the CWT using; qualify the call at :459.
3. `SelfTest.cs`: re-point the two `TimerKey` calls; add the 16 rows after the A42a rows:
```csharp
new("NoteAnalysis", "PadView", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Editor.PadView)),
new("NoteAnalysis", "UI", ExactFrom: typeof(Editor.NoteAnalysis)),
new("NoteAnalysis", "Platform", ExactFrom: typeof(Editor.NoteAnalysis)),
new("NoteAnalysis", "Motion", ExactFrom: typeof(Editor.NoteAnalysis)),
new("NoteAnalysis", "Theme", ExactFrom: typeof(Editor.NoteAnalysis)),
new("NoteAnalysis", "PadWindow", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(PadWindow)),
new("NoteAnalysis", "Program", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Program)),
new("NoteAnalysis", "Native", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Native)),
new("NoteAnalysis", "Window", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Avalonia.Controls.Window)),
new("NoteAnalysis", "Control", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Avalonia.Controls.Control)),
new("NoteAnalysis", "Dispatcher", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Avalonia.Threading.Dispatcher)),
new("NoteAnalysis", "Settings", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Settings)),
new("NoteAnalysis", "SettingsService", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(SettingsService)),
new("NoteAnalysis", "TextEditor", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(AvaloniaEdit.TextEditor)),
new("NoteAnalysis", "TextArea", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(AvaloniaEdit.Editing.TextArea)),
new("NoteAnalysis", "TextView", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(AvaloniaEdit.Rendering.TextView)),
```
   No rows for `Timers` or `TextDocument` (both are required). No namespace-string bans such as `"Avalonia.*"` (the matcher only understands `AntiNotepad.<segment>`; they would silently match nothing).
4. Gate (manager): diff review ignoring file location (class text identical, helper byte-equal), no old shim callers, `grep -n "class NoteAnalysis" Editor/PadView.cs` empty; build 0 warnings 0 errors with `AVALONIA_TELEMETRY_OPTOUT=1`; `--selftest` SELFTEST PASS, A29 checks and MATH NOTES / LIST EDITING intact, new rows 0 refs (56 table rules + 16 = 72; 73 LAYERS lines including the settings-setter summary, if nothing else lands); guard proof: temporarily add `private PadView? _layerProbe = null;` to `NoteAnalysis`, selftest must fail naming `NoteAnalysis->PadView`, revert, rebuild, rerun.
Unchanged: every algorithm, constructor/Reset order, `ConditionalWeakTable` identity, `internal` visibility, namespace and nested type names (`Editor.NoteAnalysis+CachedLine`/`+TimerLine`).

## Success looks like
- `PadView.cs` 1,132 -> about 864 lines (measured projection; 865 if the CWT using is kept); `NoteAnalysis.cs` about 272 lines (total +4).
- `grep -n "class NoteAnalysis" product/AntiNotepad/Editor/PadView.cs` and `grep -rn "PadView.TimerKey" product` find nothing; `grep -n "PadView" product/AntiNotepad/Editor/NoteAnalysis.cs` finds nothing.
- Build 0 warnings; SELFTEST PASS; A29 bounds unchanged (`LastCopiedLineCount <= 8 && LastAnalysedLineCount < 32` on the 5,000-line fixture); 16 new rows print 0 refs; guard proof recorded.
- User hand checks in the dev copy (`tools/dev.ps1`, never the installed copy):
  - Switching between notes keeps each note's undo isolated.
  - Mode, fences, headings, lists and inline/math results update after edits; first-line mode keyword changes switch mode.
  - Timer durations and duplicate timers still work and keep state.
  - Typing, pausing, inserting and deleting lines in a 5,000+ line note, and just under/over 1,500 lines (including several line-count changes before the pause), feels the same.
  - Enter/Tab list behavior and one-step Ctrl+Z unchanged.

## Impact radius
`Editor/PadView.cs`, new `Editor/NoteAnalysis.cs`, `SelfTest.cs` (2 call sites + 16 rows). Only `PadView` and `SelfTest` reference `NoteAnalysis`; no other caller of `ChangedLineRange` or `PadView.TimerKey`. Per-keystroke path: `OnDocumentChanged` -> `ChangedLineRange` -> `Invalidate` -> (debounce) -> `Analyse` -> `Update`; only the owner/qualification of `ChangedLineRange` changes.

## Risks
| Risk | Mitigation |
|---|---|
| Imperfect cut/paste changes the class | review diff apart from location; A29 selftests exercise `NoteAnalysis` directly |
| `ChangedLineRange` arithmetic or order altered (`LineNumber - 2`, clamps, invalidate before scheduling) | move byte-for-byte; keep the call position in `OnDocumentChanged` |
| Forgetting `private` -> `internal` (compile error) or making it `public` | build catches the first; keep `internal static` |
| Unchanged-but-moved state (CWT, Reset order, `_document` identity) | no edits inside the class |
| Scope creep into scheduler, commands, cache | out of scope; no behavior fixes (e.g. live InlineMath/Folding refresh limits stay as is) |
| Guard weakness | rows are direct-reference only (not transitive: `Timers` uses `DispatcherTimer`, `NoteStructure` uses colorizer/`NewFolding`); prove with a real field-type probe, not an unused using |
| Latency | no algorithmic change; JIT/layout variation cannot be excluded from source alone, hence the 5,000-line hand check |

## Depends on / conflicts with
- Depends on A42a (merged, 7f845be). Merge latest `dev` and re-read `PadView.cs` before starting; line numbers are from dev 037f4c4.
- A43-app-context: touches `Program.*` reads in `PadView.Bind`, not in the moved code; low conflict. A44-platform-seam: none.
- Any task editing `SelfTest.cs` layer table or the `TimerKey` test lines will textually conflict; resolve by keeping both.

## Recommendation on handling
Do A42b as scoped, one small task, one commit for code and a separate status commit. It is a verbatim move with one accessibility change; drop nothing, add nothing (no behavior fixes, no scheduler or cache extraction, no import cleanup beyond the one now-unused using). Riskiest points are only the cut/paste fidelity and the range-helper arithmetic; the typing-latency and undo risk is essentially nil at the algorithm level but not measured, so the 5,000-line and ±1,500-line hand checks stay in the gate.

**What the A42c / A42d gate should weigh after A42b** (PadView will be about 864 lines, still the control plus glue):
- A42c-checkbox-indent: gain is headless tests for checkbox variants (`[X]`, `[ ]task`, bullet to checkbox) and two-space indent/outdent, against new planner/adapter code and moving the `Box` regex owner. Worth it only if you want those checks; decide on capability, not on line count.
- A42d-list-commands: highest risk (live caret and offsets, post-edit renumber, one-step undo through `RunUpdate`, 9 to 10 digit-width edits). Do it only if you want Enter/Tab list behavior under headless tests and accept a thorough hand-check round; otherwise the current code works and A42b already delivers most of the structural value.
- ChipText / FoldSelection stay skipped (redraw-path allocations, ten-line selector).
- Reasonable to stop after A42b.
