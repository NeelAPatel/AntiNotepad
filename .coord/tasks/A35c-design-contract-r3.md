# A35c-design-contract-r3: the two selftests runs 1 and 2 skipped

Rework round of `A35c-design-contract` (`docs/loop.md`). Read the item card and `-r2.md` first.

Tier: Medium, same as the card
Role: worker, terra twice reported "partial" on exactly these two selftests (hard: shared tree-walk code)

## Why this round
Runs 1 and 2 left the rendered-tree check and the extra contrast pairs unwritten. Do only these two, nothing else.
- **Rendered-tree check.** `SelfTest/Design.cs` already has `DesignDump()` and `DesignProperties` that walk the live screens. Reuse that walk (do not add a second one). Add `DesignTokenChecks`: collect every FontSize, CornerRadius, Thickness (Margin, Padding, BorderThickness) and BoxShadow value from the walked pad, settings, search and help trees. Each value must be in the token value set (built by reflection from `Theme/Space`, `Shape`, `Typography`, `ChromeTokens`, `Paint`, plus 0 and sums/halves the dump already produces) or on an allow list in one place in `SelfTest/Design.cs` with a reason. Wire into `Run.cs` as its own `Check(...)` line. Print `DESIGN-TOKENS n values, m allowed`.
- **Prove it.** Temporarily change one real `Space.*` consumer to an off-token literal, run `--selftest`, paste the failing line into the report, revert.
- **Contrast pairs.** Extend `SelfTest/Contrast.cs` with: empty-note hint on Surface and Canvas, markdown marker text, placeholder text. Compute with the same roles the app uses (find them in `UI/` and `Editor/`). The Clear-tint hint (1.24:1, user kept the look 2026-11-20) is a named allow-list entry, not a failure; all other pairs must pass or be reported.

## Change to the plan
None.

## Files
- `product/AntiNotepad/SelfTest/Design.cs`
- `product/AntiNotepad/SelfTest/Contrast.cs`
- `product/AntiNotepad/SelfTest/Run.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `docs/design-contract.md`

## Done means
- Offline only. Build 0 warnings, `SELFTEST PASS` showing the new lines, `pwsh tools/check-design-contract.ps1` exit 0, `tools/gate.ps1 -Task A35c-design-contract-r3 -Offline -Base docs/A35-ux-pass` passes (no parallel builds).
- If an existing screen value fails and no token fits, add it to the allow list with a reason rather than changing the UI; report each.

Report: `.coord/reports/A35c-design-contract-r3.md`, short: Summary, the failing-line proof, allow-list rows added, checks run.
