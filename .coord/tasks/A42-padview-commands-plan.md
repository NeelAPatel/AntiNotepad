# A42-padview-commands: plan

Status: PLAN ONLY (investigator session; no product code changed). Evidence: `.coord/reports/A42-padview-commands-impact.md`
(Codex director, source-derived, nothing executed), spot-checked against `Editor/PadView.cs` and `SelfTest.cs`.
Implementation starts later from a separate flow, after A39, A40, A41 have landed (order A39, A40, A41, A42, A43, A44).
Line numbers below are from `dev` at b54b476. Re-audited on dev 34e5dee (A39-A41b merged) in `.coord/reports/A42-padview-commands-impact-r2.md`:
every PadView line >= 79 is +1 (PadView now 1,190 lines, `NoteAnalysis` at :936); no moved member changed. See **Recommendation on handling** at the end, which supersedes the single-task scope.

## What
Move text logic out of `Editor/PadView.cs` (1,189 lines: control 1-932, `NoteAnalysis` 934-1189) into UI-free types in `Editor/`:

| New file / type | Takes from PadView | Notes |
|---|---|---|
| `NoteAnalysis.cs` | whole `NoteAnalysis` class verbatim (with `CachedLine`, `TimerLine`, `TimerKey`), plus `ChangedLineRange` (:493) | same namespace and type name, so the A29 SelfTest checks keep compiling |
| `MathNotes.cs` | `EvaluateMath` (:538), `BlankLabel` (:568), `StripListMarker` (:575) | only caller is `NoteAnalysis.Update` (:1065-1076) and SelfTest:44-45 |
| `ListEditing.cs` | `ListPrefix` regex (:32), both `TryListNumber` (:763/765), the pure decisions of `IsListLine`, `ContinueList`, `Indent`, `Renumber`, `ToggleCheckbox` | returns edits (offset, length, replacement, handled flags); no `TextDocument`, no Avalonia |
| `DocumentCommands.cs` | the document-applying parts of those five commands | takes `TextDocument` plus scalars (line, caret, mode); caller owns the update group |
| `ChipText.cs` (optional, last) | text/kind/value choice in `ChipFor` (:590) | value record; `PadView.ChipFor` stays the palette/timer adapter with the same tuple signature |
| `FoldSelection.cs` (optional, last) | target choice in `FoldAtCaret` (:909) | `TargetIndex` over (start, end, folded) tuples |

Stays in `PadView`: all control/glue (ctor, `Bind`, `Release`, `ApplyLook`, margins, edge fade, `OnKeyDown` dispatch, timer actions
`ToggleTimerAtCaret`/`ResetDoneTimer`/`StartTimerOnLine`, `ShowMenu`, the 140 ms debounce and >1500-line pending logic in
`OnDocumentChanged`/`Analyse`, the static `Documents`/`Evict`/`Forget` cache). Public facades (`MathLine`, `InlineLine`, `InlineResult`,
`InFence`, `ChipFor`, `ToggleCheckbox`, `FoldAtCaret`) keep their signatures, so renderers, colorizers, `Elements`, `Structure`,
`Completion`, `PadWindow*` need no change. `TextCommands.cs` (whole-note transforms, different grammar) is left alone.
Document-cache extraction is out of scope.

## Why
The regression risk removed: logic that today can only be exercised through a live window, and logic reachable through `PadView` from many places.
- `NoteAnalysis` calls `PadView.BlankLabel/StripListMarker/EvaluateMath` (PadView.cs:1065-1076): the analysis type depends on the control type that owns it. A change to a PadView member silently changes analysis.
- The list/checkbox commands (`ContinueList` :819, `Renumber` :791, `Indent` :773) can only be tested by typing in the dev copy, so a tweak to numbering (the 999,999,999 limit, nested counter reset) is found late. After the split they get headless checks.
- `SelfTest` reaches into `PadView` statics (SelfTest.cs:44, 45, 234, 263, 264); a window-free home makes that dependency honest.
- The regex grammars `ListPrefix`, `TextCommands.Marker` and `CheckboxGenerator.Box` differ on purpose (one `\s` vs `\s+`, ASCII digits vs `\d`); one owner each makes an accidental unification visible.

## How
Smallest and lowest-risk first. Each step builds, passes `--selftest`, and is one commit (code and status separate).
1. `MathNotes` plus the `ListEditing` prefix/number parser. Move `EvaluateMath`/`BlankLabel`/`StripListMarker` and `ListPrefix`/`TryListNumber` verbatim (same patterns and options; `BlankLabel` keeps its non-compiled `Regex.Match`). Update `NoteAnalysis` and SelfTest:44, 45, 234. Delete the `PadView` statics.
2. `NoteAnalysis.cs`: cut and paste the class unchanged; add `ChangedLineRange`; `PadView.OnDocumentChanged` calls it. Delete the `PadView.TimerKey` shim; SelfTest:263-264 call `NoteAnalysis.TimerKey`. The scheduler does not move.
3. `ToggleCheckbox`, then `Indent`, as pure edit plus `DocumentCommands` adapter. The `CheckboxGenerator.Box` regex moves to `ListEditing` and `Elements.cs:46` uses it (group 2 preserved).
4. `ContinueList`, `Renumber`, `IsListLine` (coupled). The pure planner receives only the contiguous list block, not the whole note. Keep one `RunUpdate` group around continue+renumber and indent+renumber (PadView.cs:689, 696); `Renumber` uses `CaretLine` after mutation; `Insert` keeps `Document.Insert(CaretOffset, ...)` plus `BringCaretToView` in PadView (caret anchors belong to AvaloniaEdit); number edits use live offsets.
5. Optional: `ChipText`, `FoldSelection`, only if still worthwhile (palette/timer sampling and `IsFolded` mutation stay in PadView).
6. Add the guards below and remove obsolete forwarding members.

Unchanged: behavior, regexes, `internal static` visibility for SelfTest, `Documents` and undo limit 400, `_binding` ordering, the 140 ms / >1500-line debounce, A29 invalidation (`Invalidate` before scheduling, DocumentLine-keyed `ConditionalWeakTable`, zero-based changed range, one-based inline/timer keys). No whole-document copies on Enter/Tab.

## Success looks like
- `Editor/PadView.cs` about 750 lines (730-790 acceptable), down from 1,189; `NoteAnalysis.cs` about 256 lines. Total LOC may grow slightly (adapters).
- `grep -n "BlankLabel\|StripListMarker\|EvaluateMath\|TryListNumber\|ListPrefix" Editor/PadView.cs` finds no definitions; `grep -rn "PadView\.\(BlankLabel\|StripListMarker\|TryListNumber\|TimerKey\)" product` finds nothing; `grep -n "class NoteAnalysis" Editor/PadView.cs` finds nothing.
- Build 0 warnings, 0 errors; `--selftest` prints SELFTEST PASS with all existing A29 lines unchanged (5,000-line fixture: `LastCopiedLineCount <= 8`, `LastAnalysedLineCount < 32`).
- New headless selftests (inputs/expected come from the report, derived from current code; re-verify each against the code when implementing):
  - `BlankLabel`/`StripListMarker` offsets ("spent 12.50 + 8.25 =" blanked to the number; "2. 19 + 3 =" numbered marker kept; "  - [x] 3 + 4 =" offsets).
  - `TryListNumber`: 9 digits accepted, 10 rejected.
  - Checkbox toggle: `[X]` to `[ ]`; `  -   task` to `  [ ] task`; `[ ]task` to `[x]task`.
  - Outdent removes ASCII spaces only.
  - Continue: checked box continues unchecked; empty nested item outdents; empty root item exits; List-mode fallback `[ ] `; 999,999,999 declines; Code mode declines.
  - Renumber: keeps starting number; nested fresh; bullet resets same-level counter; digit-width change 9 to 10.
  - `EvaluateMath`: label retry keeps variables and the Comment token span; bullet math stays positive.
  - Real `TextDocument`: indent+renumber in one `RunUpdate` undoes with a single `Undo`; Continue+Renumber at end-of-line and mid-line.
  - Fold target selection and chip descriptors if step 5 is done.
- New `LAYERS` rules in A39's table (reuse its mechanism, no parallel framework): `MathNotes`, `ListEditing`, `ChipText`, `FoldSelection` must not reference `Avalonia.*`, `AvaloniaEdit.*`, `PadView`, `UI`, `Theme`, `Timers`, `Settings`; `DocumentCommands` must not reference `Avalonia.Controls`, `AvaloniaEdit.Rendering`/`Editing`, `PadView`, `UI`, `Theme`; `NoteAnalysis` must not reference `PadView`, `Avalonia.Controls`, `AvaloniaEdit.Rendering`, `UI` (it keeps `AvaloniaEdit.Document`, `Model`, `Calc`, and `Timers`/Dispatcher). Not asserted: `NoteStructure` purity (it uses `NoteColorizer.HeadingLevel` and `NewFolding`).
- Guard proof: temporarily add a `PadView` reference to `MathNotes` and an `Avalonia.Controls` use to `ListEditing`; the selftest must fail and name the offender; revert.
- Live checks by the user in the dev copy (`tools/dev.ps1`):
  - Enter continues bullets, numbers (`1.`, `2)`) and checkboxes (checked becomes unchecked); Enter mid-line splits.
  - Empty item exits; empty nested item outdents.
  - Tab / Shift+Tab indent and renumber nested lists.
  - Ctrl+Enter toggles a checkbox or starts a timer.
  - One Ctrl+Z undoes an Enter-continue or a Tab-renumber in a single step.
  - Caret lands after the new marker and stays in view at the bottom of a long note.
  - Ctrl+Shift+[ and ] fold/unfold.
  - Math labels ("lunch 12 + 3 =") and bullets ("- 19.99 * 3 =") still show results.
  - Typing in a 5,000+ line note feels the same; each note keeps its own undo history when switching.

## Impact radius
Direct: `Editor/PadView.cs`, the new files, `Editor/Elements.cs` (Box regex, line 46), `SelfTest.cs` (lines 44, 45, 234, 263, 264, plus new checks and LAYERS rows). No change expected in `Renderers`, `Colorizers`, `Structure`, `Completion`, `Motion`, `ChevronGutter`, `PadWindow*`, `SearchOverlay`, `MotionPreview`, because the PadView facades keep their signatures. Hot paths touched: Enter/Tab (list commands), every edit (analysis, moved not changed), redraw (`ChipFor`, only if step 5).

## Risks
| Risk | Spot | Mitigation |
|---|---|---|
| Undo groups split or merged | PadView.cs:689, 696, 841 (only `RunUpdate` exists; no BeginUpdate/UndoGroup) | keep the caller-owned `RunUpdate`; selftest single Undo; user checks Ctrl+Z |
| Caret placement/scroll after Enter | :830, 855, 859-862 | keep `Insert` + `BringCaretToView` in PadView; headless tests cannot prove it, so the live check is mandatory |
| Stale analysis cache | `Invalidate` before scheduling (:459-460), DocumentLine identity, 0/1-based conventions | move `NoteAnalysis` verbatim; run the A29 checks between steps |
| Debounce semantics changed | `>1500`, 140 ms, pending union, full fallback (:455-507) | leave in PadView untouched |
| Stale offsets when digit width changes in Renumber | :791-815 | live offsets in traversal order (or descending); selftest 9 to 10 |
| Regex grammars unified by accident | `ListPrefix` vs `TextCommands.Marker` vs `Box` | three grammars, one owner each; selftest cases |
| Static init order / first-use cost | compiled regex statics | keep `RegexOptions.Compiled` statics, never construct per call |
| Static `Documents`/`Evict` touched | :201-247 | out of scope; do not move |
| "Window-free" is not "dependency-free" | `NoteAnalysis` takes `TextDocument` and `Timers` (DispatcherTimer) | say so in the LAYERS rules; no purity claim for `NoteAnalysis` |
| Latency regression | `ReadLines` is still O(n) | no new whole-document copies; compare feel on a 5,000-line note |

Pre-existing, not to be fixed here: `InFence`/`MathLine` negative-index precondition; redundant returns in `ContinueList` :843-844.

## Depends on / conflicts with
- **A39-motion-out-of-ui** (lands first): edits the top of PadView.cs (usings, `Anim` at :441-450, menu in `ShowMenu`/`AppMenu` :734-755, `ChromeTokens` in edge fade) and adds the `LAYERS` table to SelfTest. A42 adds rows to that table; conflicts are textual (usings, SelfTest). Re-read A39's real schema before step 6.
- **A40-settings-change-event**: touches `PadView.ApplyLook` / `_caret.SettingsChanged()` (:175-178) and Settings reads. No overlap with moved members, but `Analyse` passes `Settings.InlineMath` into `NoteAnalysis.Update`: keep that scalar-argument signature.
- **A41-padwindow-split**: PadWindow only; it calls `PadView` public members, which A42 keeps. Low conflict.
- **A43-app-context**: replaces `Program.NotesLocationError/DataFolder` in `Bind` (PadView.cs:275-280); control/glue, outside moved code. Low conflict (same file, different lines).
- **A44-platform-seam**: no `Native.*` in PadView. None.

## Re-audit on dev 34e5dee (A39-A41b merged)
- What the earlier tasks changed in PadView: `Motion`/`Theme` usings replace `UI`; `ShowMenu` property (:79) and `RequestMenu` (:734, was `ShowMenu`) with `MenuGlyphs` from new `Editor/MenuEntry.cs`; nothing in math, list, checkbox, fold, `Documents`, debounce or `NoteAnalysis`. A40 changed no PadView body (`ApplyLook` already calls `_caret.SettingsChanged()`); `Analyse` still passes the scalar `Settings.InlineMath` to `NoteAnalysis.Update`, keep it that way. A41b: the window still calls `PadView.DocumentFor` for clipboard inserts; the document cache stays untouched.
- All risks and guards above still hold (undo `RunUpdate` at :690/:697, caret read after mutation, A29 invalidation, 140 ms / >1500 debounce, `Documents`/`Evict`, three regex grammars).
- Selftest call sites to rename are now SelfTest.cs:63, 64 (`BlankLabel`, `StripListMarker`), :908 (`TryListNumber`), :937-938 (`TimerKey`).
- LAYERS rows must use the live `LayerRule` form: `new("MathNotes", "UI", ExactFrom: typeof(Editor.MathNotes))` for `AntiNotepad.*` namespaces, `ExactFrom`+`ExactTo` for single types (`PadView`, `PadWindow`, `Program`, `Native`, `Avalonia.Controls.Window`/`Control`, `Dispatcher`, `TextDocument`, `Timers`, `Settings`, `SettingsService`). The matcher only understands `AntiNotepad.<segment>` namespaces, so `new("MathNotes", "Avalonia.Controls", ...)` would silently match nothing. It also cannot ban whole external namespaces (`Avalonia.*`, `AvaloniaEdit.*`); use exact-type bans plus review. Full literal row blocks are in the r2 report (MathNotes, ListEditing, DocumentCommands, NoteAnalysis, ChipText, FoldSelection). `NoteAnalysis -> PadView` fails today and holds after A42a. `NoteAnalysis` needs `TextDocument` and `Timers`, so no rows for those; `DocumentCommands` gets no `TextDocument` ban; add `ListEditing -> CheckboxGenerator`.

## Recommendation on handling
**Still worth doing, but not as one task at original scope.** Do not chase the 750-line target. Split in order; stop after A42b unless the gate below says continue.

| Order | Task | Scope | Risk | PadView.cs after (est.) |
|---|---|---|---|---|
| 1 | `A42a-math-notes` | `MathNotes` (`EvaluateMath`, `BlankLabel`, `StripListMarker`) + `ListEditing` prefix/`TryListNumber` verbatim; rewire `NoteAnalysis` and SelfTest:63, 64, 908; first LAYERS rows | low | ~1,125-1,135 |
| 2 | `A42b-note-analysis-file` | move `NoteAnalysis` unchanged to its own file; `ChangedLineRange` moves with it; remove `TimerKey` shim (SelfTest:937-938 -> `NoteAnalysis.TimerKey`); `NoteAnalysis -> PadView` rule; scheduler stays | low | ~855-875 |
| gate | user decision | A42a and A42b remove the reversed analysis -> control dependency and give math a testable home. Recommended stopping point (~860-875 lines). | | |
| 3 | `A42c-checkbox-indent` (optional) | pure checkbox/indent edits + thin `DocumentCommands`; `Box` regex owner moves to `ListEditing`; single-Undo checks | medium | ~825-850 |
| 4 | `A42d-list-commands` (optional, only if headless coverage is worth it) | coupled `ContinueList`/`Renumber`/`IsListLine` split | highest | ~745-795 |
| skip | `ChipText`, `FoldSelection` | `ChipFor` is on the redraw/timer-tick path and a descriptor layer adds allocations and changes timer sampling; the fold target is ~10 lines. No value now. | | |

Riskiest for typing latency and editing behavior, and the user's hand checks (dev copy, `tools/dev.ps1`):
- **A42a** (low): math/inline results and token colors. Check "lunch 12 + 3 =" and "- 19.99 * 3 =" still show results, mixed-currency badge, math variables; ordinary Enter/Tab unchanged.
- **A42b** (low; moves ~255 lines of the per-keystroke analysis): switching notes keeps undo isolated; mode, fences, folds and results update after edits; typing, pausing, inserting and deleting lines in a 5,000+ line note feels the same. Known pre-existing: live InlineMath/Folding setting refresh limits are not to be "fixed" here.
- **A42c** (medium): click and Ctrl+Enter toggle a checkbox (`[X]`, `[ ]task`, bullet to checkbox); Tab / Shift+Tab indent; caret stays; one Ctrl+Z.
- **A42d** (highest: caret anchors, undo grouping, offsets after digit-width change): Enter at mid and end of line on bullets, numbers and checkboxes; checked becomes unchecked; empty nested item outdents, empty root exits; 999,999,999 falls through; Tab renumber nested incl. 9 to 10; one Ctrl+Z per Enter/Tab; caret stays visible at the bottom of a long note; typing speed in a long list.
At every milestone the manager builds with 0 warnings, runs `--selftest` (new focused checks + existing A29 bounds), proves each new LAYERS row fails on a deliberate real IL reference (not an unused using) and reverts it, then the user does the hand checks. Nothing in this audit was built or run; latency, caret anchors and GUI behavior remain unverified.
