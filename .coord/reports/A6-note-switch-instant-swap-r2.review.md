# A6-note-switch-instant-swap-r2 lens review

## Findings (proven)
None.

## Behavior changes check
Report's list vs the diff: matches, reading the original implementation report together with the round-2 report.

- `NoteSwipe.Fire` stages the displayed pull, navigates once synchronously, and discards unused staging in `finally`. `Show` and `Cancel` cancel competing motion and restore state. The two owners are PadWindow's active pane and MotionPreview's sample pane; both still hold.
- `SwitchTo` callers are `PadWindow.BindPanes` (left and right), MotionPreview's navigation callback, and the new selftest. Binding remains immediate; animation callbacks never navigate or change the selected note. All callers retain their selection semantics.
- `BindPanes` callers checked: constructor, Navigate, NewNote, OpenFreshNote, ShowStack, OpenSlot, PromoteToSlot, ToggleSplit, CycleSplitSlot, DeleteCurrent, RestoreFromTrash, SearchChangedData, and JumpTo. The new optional right-arrival argument is used only by CycleSplitSlot, replacing its previous explicit right-pane Arrive call.
- Direct `Bind` callers are SwitchTo, MotionPreview.UseSamples, and selftests; its new same-note cancellation behavior is documented in the original report. Arrive is called only inside SwitchTo. Release and detach now stop transition work; both owners retain their cleanup paths.
- Snapshot departure is 160 ms; normal arrival remains 200 ms, 34 px, and 0.25 initial opacity. Missing top level, zero bounds, reduced motion, and the caught rendering exceptions use normal arrival. Images are removed and bitmaps disposed on completion, cancellation, or binding failure.

The round-2 implementer report records an earlier Scope failure; the supplied round-2 gate report records a later PASS. Its head label is `6e38864-dirty`, so the label alone cannot establish an exact match to committed HEAD `8ba519f`. Independent build/selftest results below cover the current product tree.

## Scope and plan check
In plan. Reviewed every hunk of `git diff dev...HEAD -- product`; `git diff -- product` is empty. All seven changed files are on the original approved list. The round-2 brief explicitly selects variant 2 for release and supersedes the original lab-only plan. No selector, sequential reveal, or continuous-start parameters remain. AppInfo is unchanged.

Headers, named arrival geometry, navigation integration, cleanup, selftests, and the CONTRACTS row serve the approved transition and settled-pane promises. Swipe settings, trigger distances, wheel pay-out, selection rules, and NoteArrive duration are preserved. PadView's added code still owns one pane's transition; no unrelated split or layering dependency was introduced.

The report explicitly lists the two pre-existing Scroll Lab comments outside the approved files as a deviation from the literal product-wide wording condition. They are unrelated to note-switch variant selection and do not warrant expanding this fix. No unlisted plan deviation found.

## Unproven risks
- Captured pixels, capture cost, first-reparent focus behavior, DPI, and split-pane containment require the specified dev-copy hand checks; headless success does not prove them.
- New checks cover fallback completion and seeded interruption, not an actual rendered departure completing across display frames.

## Checks run
- Read the item card, round-2 brief, both implementation reports, gate report, and lens-review skill; checked all nine checklist items against changed hunks and surrounding callers.
- Built an isolated copy of the current product sources under `build/A6-note-switch-instant-swap-review` to avoid writing build artifacts under product: Build succeeded, 0 warnings, 0 errors, NuGet audit disabled, Avalonia telemetry opted out.
- Ran that DLL with `--selftest` and disposable ANTINOTEPAD_DATA: `SELFTEST PASS`; `CONTRACTS: 14 rows, 13 pass, 1 manual`; all emitted LAYERS checks passed. Design hash: `1032fadd9067c61f4ce6299ad46b414464a0fb383a95827eaa79960cdb5c716c`, 16160 lines.
- `git diff --check`: pass. Product working-tree diff: empty. Note-switch selector search: absent.
- Supplied offline gate: PASS for Build, Selftest, Seam, Tokens, Format, Scope, and Cleanup; Size warning for PadView, LoopLint skipped. The full gate was not rerun during this review.
- No app UI launched, installed instance controlled, product files edited, or git writes performed.
- Review-folder cleanup was attempted with resolved-target and reparse-point guards. Automatic execution policy rejected deletion as blocked by policy; `build/A6-note-switch-instant-swap-review` remains for manager cleanup.

VERDICT: accept
