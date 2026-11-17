# A23-paste-keep-structure-r4 lens review

Reviewed `git diff dev...HEAD -- product` at HEAD `ff0b31b4c70977320e3e185e74e44559ff765abd`, the item card, r2-r4 briefs, implementation reports and r4 gate report. Product files were read only. The supplied passing hand test is accepted as UI evidence; no live application or clipboard was used here.

## Findings (proven)

1. [major] `product/AntiNotepad/Editor/PasteHtml.cs:141` - Deep clipboard HTML can terminate the process. `Parse` accepts arbitrary nesting, then `Convert` enumerates recursive `Descendants` at line 85; `Inline`, `Walk` and list conversion also recurse without a depth bound. A Release probe linking the unchanged product engine used `"<span>"` repeated 20,000 times, then `<b>A</b>`, then 20,000 closing spans, with plain text `A` (260,008 HTML characters). Calling `PasteEngine.Analyze` printed `Stack overflow.` and exited **-1073741571 (0xC00000FD)**, with repeated descendant iterator frames. The catch in `PasteEngine.cs:25` cannot recover this failure. Smallest fix: reject excessive nesting during parsing before any recursive conversion or enumeration; add a bounded-depth fallback selftest.

2. [major] `product/AntiNotepad/Editor/PasteHtml.cs:40` - Each tag copies the entire unconsumed HTML through `html[pos..]`, making parsing allocation quadratic and freezing paste on moderately large clipboards. The synchronous call from `PadView.cs:951` runs in the UI continuation. Release probe input was `<table>` plus `<tr><td>A</td></tr>` repeated N times plus `</table>`, with N plain `A` lines. Two runs produced:

   | Rows | HTML characters | Elapsed milliseconds | Cumulative allocated MiB |
   |---|---:|---:|---:|
   | 2,000 | 38,015 | 47-51 | 302.3 |
   | 4,000 | 76,015 | 322-327 | 1,184.7 |
   | 8,000 | 152,015 | 1,108-1,350 | 4,688.8-4,689.1 |
   | 16,000 | 304,015 | 4,574-4,922 | 18,655.6-18,656.1 |

   Every case converted successfully; allocations are total allocations, not peak retained memory. Smallest fix: scan the original string/span at an offset without copying the suffix, with an input/output or table-size budget that falls back before excessive work. Moving the existing quadratic parser to a worker alone does not fix the allocation problem.

3. [major] `product/AntiNotepad/Editor/PadView.cs:956` - Overlapping converted paste commands silently discard an invocation. Exact path: Ctrl+V starts a fire-and-forget `PasteAsync` (`:835`); two invocations before the clipboard awaits finish capture the same document, selection and caret (`:935-937`). The first inserts at `:964`. AvaloniaEdit's `EmptySelection.ReplaceSelectionWithText` inserts into the document (`third_party/AvaloniaEdit/Editing/EmptySelection.cs:71`), and document-change processing advances the caret (`TextArea.cs:382`, `Caret.cs:180-198`). The second invocation then fails the caret comparison at `:956` and returns, so two paste commands produce only one insertion. This applies even with an unchanged clipboard and no user edit between commands. Smallest fix: serialize paste requests and capture the insertion state when each queued request begins, while retaining cancellation for actual user navigation/edits. Add a delayed-clipboard check for two requests. Proof is the exact code path, not a live timing reproduction.

4. [minor] `product/AntiNotepad/Editor/PasteTable.cs:33` - Width counts code points except ZWJ continuations, so common multi-code-point emoji consume too much or too little padding. A probe invoking the unchanged private width function returned `Width("🇺🇸") = 4`, `Width("👍🏽") = 4`, and `Width("❤️") = 1`; each represents one emoji and should occupy 2 columns under r3's rule. Control cases `🟢` and `👩‍💻` returned 2. This misaligns table rows containing flags, skin-tone modifiers or emoji presentation selectors. Smallest fix: measure emoji grapheme sequences as a unit and test these three cases alongside the existing ZWJ case.

## Behavior changes check

Reports describe the intended list/block/inline/link/table conversions, image/file no-op stubs, guards and existing-toast outcomes: matches the main diff. They do not identify the failure behavior in findings 1-4.

Caller check:
- `Upgrade`: only `SelfTest/Paste.cs`; still returns converted text or null.
- `Analyze`: `PadView.PasteAsync` and `SelfTest/Paste.cs`; expected result shape holds on ordinary fixtures, but the bad-HTML safety contract fails in finding 1.
- `IsPreformatted`: `HtmlReader` and `PadView.PasteAsync`; both use the same guard for conversion and notice suppression.
- Parser/converter/table helpers are confined to the engine and one another; no Model dependency, Editor-to-UI dependency or native calls were introduced.

Converted selection replacement uses AvaloniaEdit's existing update grouping (`SimpleSelection.cs:60`); empty-selection insertion is one document edit. No separate undo-grouping defect was proven. The fallback still delegates to AvaloniaEdit's existing paste command.

## Scope and plan check

All six changed product files serve the card or r2-r4 expansions: engine and hook, split parser/table converter, paste checks and CONTRACTS row. No unrelated product hunks, UI styling/default changes, or unlisted file deviations found. Standalone inline/heading conversion is covered by r3's formatting expansion. Toast strings and use of the existing toast match r4; no PadWindow or Settings changes.

The r4 implementation report's scope failure is historical: the current card includes the lab file, and the supplied r4 gate reports Scope PASS. That gate identifies HEAD `6c75cc1f2553bd4b352141468935f50c847d70e1`, not the reviewed HEAD; its full build/selftest results were read as recorded evidence, not rerun or represented as current-HEAD verification.

## Unproven risks

- The hook checks document identity, selection and caret, not document version; edits elsewhere or navigation away and back can leave the guard satisfied.
- Normal fallback calls `Editor.Paste()` after validation; its existing async command performs another clipboard read and has no equivalent post-await document/selection check (`third_party/AvaloniaEdit/Editing/EditingCommandHandler.cs:506-543`). Live note-switch behavior was not exercised.
- Source detection recognizes indentation, list prefixes and HTML preformatted hints; unindented code/markdown offered as ordinary formatted HTML can still acquire marks. Engine probes confirmed text rewriting, but did not establish a real source application that supplies that combination.

## Checks run

- Read all changed product files around the diff, callers, local AvaloniaEdit insertion/selection/caret/undo and fallback paste paths; checked all nine lens checklist areas. No new persistent fields, subscriptions or cleanup obligations were introduced by the product diff.
- Built a temporary Release console probe under `build/A23-review`, linking the three unchanged product conversion files and the existing `SelfTest/Paste.cs`. Build succeeded; isolated existing paste checks printed **`PasteChecks: PASS`**, exit 0. This is not the complete application selftest.
- Ran deep-nesting, table allocation/timing, source-guard and width probes; results above. No product files were modified and no full product build was run.
- `git diff --check dev...HEAD -- product` reports trailing blank lines at EOF in the three new files; these are not behavioral findings.
- Removed the temporary `build/A23-review` folder after probing. Final product diff remains unchanged.

VERDICT: rework
