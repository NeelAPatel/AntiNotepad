# A9-pinned-accent-ring-r2 lens review

## Findings (proven)
None.

## Behavior changes check
Report's list vs the diff: matches. The original report describes the ring, stacking, cancellable fade, initial state, Reduce motion and appearance-batch handling; the r2 report describes the test setup, contract ordering and design hash update. No missing behavior change found.

- `ApplyAppearance` callers remain valid: the constructor initializes the ring immediately; `HandleSettingsChanged` updates it once after palette and Reduce motion synchronization; the platform color-change callback refreshes its accent. The selftest also calls this method directly with reduced motion enabled.
- `HandleSettingsChanged` retains its existing settings-service subscription in `Program.cs`. Pinned and ReduceMotion changes both include Appearance in `SettingsService.PropertyMap`, so the normal pin path reaches `ApplyAppearance`; a WindowPolicy-only change uses the explicit ring update. Existing taskbar and persistence reactions are preserved.
- New `ApplyPinRing` callers are only those two methods. Each update cancels the preceding handle, reads current opacity and targets the current pinned state. `Anim.Run` settles immediately under Reduce motion; initialization skips animation. `PinRingOpacity` is used only by the ring update and its capability check.
- No existing method signature or return contract changed. The contract suite retains its prior checks and adds the ring check last; the design baseline changes only its expected hash.

## Scope and plan check
In plan. Reviewed all eight changed product files and their surrounding code against the card's After and Must not change clauses, plus the r2 brief.

- Tokens, the overlay Border and appearance/settings integration implement the approved 4 px, Shape.Card, palette accent, Paint.FocusBorder and 250 ms behavior. The Border is a sibling overlay with no child, padding or margin, and is excluded from hit testing. Explicit ZIndex preserves its stacking when the pager is reparented.
- No edits to show/hide, window bounds, pane layout, shadow, pin icon, Dim when pinned, toast or native window policy were found. Header comments serve the touched files; no unrelated product hunks or unlisted deviations found.
- The new selftest and CONTRACTS row fulfill the capability-check promise. Fluent setup, loaded/seeded temporary store, last-contract placement and DesignHash update are authorized by r2 and explained in its report. No test hooks were added to product classes beyond the explicitly planned opacity helper.

## Unproven risks
- The selftest still relies on the ring check running after the store contracts because it initializes shared Avalonia state; r2 documents this ordering constraint, but future contract additions must preserve it.
- Displayed fade, rounded-edge alignment, custom accents, split view, rapid toggles, drag/resize, icon clicks and show/hide timing remain unverified until the planned user hand test.

## Checks run
- Read the item card, r2 and review briefs, both implementation reports, r2 gate report and lens-review skill; traced settings dispatch, appearance callers, animation cancellation, layout stacking and selftest execution order.
- Reviewed `git diff dev...HEAD -- product`: eight product files. `git diff -- product`: empty; no uncommitted product changes.
- `git diff --check dev...HEAD -- product` and `git diff --check -- product`: PASS, no output.
- Gate evidence checked against current HEAD `5add004681d7541c7c81297709ecfc6f5215b6e1`: matches. Supplied r2 gate reports Build PASS (0 warnings/errors), Selftest PASS (`CONTRACTS: 16 rows, 15 pass, 1 manual`, `SELFTEST PASS`), Seam/Tokens/Format/Scope/Cleanup PASS; Size WARN for Layout.cs 606 -> 636 lines. Build and selftest were not independently rerun in this review.
- No app launched, product files edited, probes/build artifacts created, branch changes or git writes performed.

VERDICT: accept
