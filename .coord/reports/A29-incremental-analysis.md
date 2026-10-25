# A29-incremental-analysis

Completed 2026-10-25 on `feat/A29-incremental-analysis`. No branch switch, commit, merge, normal application launch, process termination, desktop testing or installed-data access. STATUS and the audit were left unchanged as requested.

## Split and files

The director planned, reviewed the implementation, ran the verification and temporary measurement harness, and wrote this report. Two gpt-6-luna / xhigh workers shared the worktree with disjoint ownership:

| Worker | Files under product/AntiNotepad |
|---|---|
| a29_structure | Editor/Structure.cs: NoteStructure only; A28 fence cache preserved |
| a29_analysis | Editor/PadView.cs: analysis path; SelfTest.cs: one new method at the end and one Run call |

These are the only product files changed. No Calc changes or new packages were needed. This report is the only additional deliverable.

## Design as built

`NoteAnalysis` is a production analysis API in PadView.cs. It caches text by DocumentLine identity in an instance-owned ConditionalWeakTable. Each change invalidates its current affected lines plus one neighboring line on each side, including line split/join boundaries. Analysis snapshots reuse unchanged strings rather than copying the document text or every line. Mode detection reads the first cached line. Bind/release clears the cache; binding also stops a pending analysis timer. Unavailable notes still bind an empty read-only document through the existing path, without writing an error body into note text.

`NoteStructure` caches a signature per line: blank status, raw indentation, heading level, fence delimiter status and normalized fence language. For unchanged line counts, Update compares signatures only in the widened changed range. If all match, it reuses fences, fence membership, inherited indentation and fold line ranges exactly. This covers ordinary typing, fence-body text edits and list text edits that preserve structure. Any signature change, invalid range or line-count change rebuilds the structure for the whole note. Thus heading changes, fence opening/closing/language changes, blank-gap changes and indentation changes safely widen to the entire document; the implementation does not guess at distant structural effects.

Blank-line indentation now uses two linear passes. Heading boundaries use a reverse pass over six heading levels and precomputed blank-pair boundaries. Indented children use a monotonic stack and previous-content indices. Fence opening/interior membership is indexed, replacing repeated fence searches. Both plain/code fold line-range sets are cached. Folds still materializes current document offsets and sorts foldings, preserving their ordering and AvaloniaEdit integration. The original two-blank-line and closing/unclosed-fence semantics are preserved.

Inline answers reuse unchanged LineEval objects and evaluate the changed range. Line-count edits shift surviving result keys; mode or inline-math-setting changes rebuild relevant results. Timer line parses are cached, but duplicate-duration ordinals and active keys are reconstructed in document order on each timer-note analysis before the existing Ensure/Reconcile calls. Timer key reconciliation remains global. Math notes deliberately keep the original complete two-pass evaluation: variables, previous results, totals and prose-label retries can affect distant lines. Mixed-currency results use the existing calculator path unchanged.

The 1,500-line synchronous threshold and 140 ms debounce above it remain. Pending same-count edits union their widened ranges. A line-count change after another pending edit triggers full analysis; crossing back into synchronous analysis with pending edits also triggers a full pass. These rules favor correctness over trying to remap ambiguous pending ranges.

`LastCopiedLineCount` exposes actual document-line text copies. `LastAnalysedLineCount` exposes structural signature computations plus inline/math/timer line work. These are production API observations, with no reflection or test-only hooks. They do not count every collection visit: reference-array snapshots still traverse all lines, fold offsets still materialize, and timer ordinal reconciliation and math remain global. This is bounded semantic re-analysis for common edits, not constant-time end-to-end editing.

The A28 `_fences.UpdateStructure` pruning call remains after analysis, and fence-cache bounds/version behavior is unchanged. Numbered-list editing, rendering, commands, limits, store/settings/window/platform code were not changed.

## Before/after measurements

Temporary .NET 10 Release console harness, using Avalonia 12.1.4 and AvaloniaEdit 12.0.0. Before used the original saved NoteStructure code plus the original Analyse operations for full text/line copies, mode detection, folds and inline answers. After used the actual new NoteStructure and NoteAnalysis source with the existing calculator/helpers. Both used TextDocument replacements and materialized fold offsets, with inline math enabled. Fixtures contained headings, lists, blank gaps, fenced code, inline calculations and ordinary text. Each synthetic key replaced one character in an ordinary middle line. After used the actual one-neighbor-per-side widening rule. Each case warmed up for 100 edits, then measured seven batches of 200 edits; table values are the median batch mean.

| Note size | Before, ms/edit | After, ms/edit | Reduction |
|---|---:|---:|---:|
| 1,500 lines | 0.5084 | 0.0786 | 84.5% |
| 5,000 lines | 1.3525 | 0.2731 | 79.8% |

Observed batch ranges: 1,500 before 0.3443–0.7632, after 0.0582–0.0937 ms/edit; 5,000 before 1.2098–1.6362, after 0.2578–0.2856 ms/edit. Each after edit copied three line texts and processed six line operations (three signatures plus three inline eligibility checks).

These are analysis-path timings per synthetic keystroke, including document replacement; they exclude UI rendering, FoldingManager updates, event subscribers and input-to-display latency. The harness deliberately ran analysis after each 5,000-line edit for a comparable workload; the real application still defers such analysis until the typing pause. Timer/math-mode performance and live GUI performance were not measured. Initial harness restore required pinning the cached Avalonia version and restoring from the local package cache; no package/config changes were made to the product.

## Verification

- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a29-check`: **0 warnings, 0 errors**.
- `build/a29-check/AntiNotepad.exe --selftest`: exit 0, **SELFTEST PASS**.
- New single selftest method checks incremental/full equivalence for fence-body edits, fence opening/language/closing edits, heading insertion/deletion, list indentation, a variable used later in math, inline results, duplicate timer ordinals, structure arrays and fold offsets.
- A two-edit 5,000-line typing burst checks both copy and analysis bounds and equivalence with fresh full analysis.
- Temporary harness: 10,000 seeded randomized notes matched the original structure implementation for fences, fence membership, indentation and fold offsets in both plain and code modes.
- Temporary harness: 9,000 seeded randomized replacements, insertions and deletions across plain/math/timer/code/list notes matched fresh full analysis, including cached line text, mode, folds, math tokens/results/errors, inline answers and timer keys.
- `git diff --check`: passed.

## Needs a live check

All items below **need a live check** by the user in the dev copy; no normal application launch was allowed here:

- Type in 1,500- and 5,000-line notes, including a sustained burst, line insertion/deletion, undo/redo and switching notes during the debounce window; judge responsiveness and immediate/correct eventual decoration updates.
- Headings, nested folds and the two-blank-line fold end; fenced code colors while typing, opening/closing/moving fences and changing language, including long notes.
- Numbered-list renumbering, list indentation and indent guides, clean markdown, checkboxes, dividers and slash commands.
- Math variables used later, totals and prose labels; inline `=` answers with the single answer column, including mixed-currency error chips.
- Duplicate-duration timers, edited labels/durations, running/paused/done chips and timer limits; calculator/list limits.
- Unavailable-body notes remain read-only and cannot save the visible error message as note text.

Cleanup: `build/a29-check` and `build/a29-harness` remain. Recursive removal was requested with absolute-target containment checks, but automatic tool policy rejected the cleanup command as blocked by policy. No alternate deletion route was attempted. The manager can remove these disposable folders locally; the harness is not part of product code or the tracked deliverable.

No implementation was deferred. Math evaluation, structural fallback cases and timer key reconciliation intentionally remain global. Live behavior and end-to-end keystroke latency remain unverified.
