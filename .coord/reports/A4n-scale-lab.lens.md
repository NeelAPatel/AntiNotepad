# A4n-scale-lab lens review

## Findings (proven)
1. [major] `.coord/tasks/A4n-scale-lab.md:30` - The Files section is prose rather than explicit backticked bullets. The final gate reports Scope FAIL. The owning session must enumerate the actual files, including the justified snapshot refresh; the implementation report supplies the exact list.
2. [minor] `product/AntiNotepad/UI/PadWindow.Layout.cs:498` - The two required compact save-action padding assignments raise the spacing-literal count from 24 to 26. The final gate reports Tokens FAIL. Record the mapping reason on the item card and update the approved baseline, then rerun the gate.

## Behavior changes check
Matches the report: approved visual values only. No input routing, persistence, shared-state, lifecycle or cleanup change found. Dropdown height 48 accommodates the specified insets; PreviewBar resolves .09/.06 correctly.

## Scope and plan check
Product diff accepted. No further mapped-value omission or Recommended-value disagreement with tokens.json proven. All 13 exceptions remain intact; unmapped Tile gap remains 5. The only additional product file is SelfTest/Design.cs: its diff changes only DesignHash to `1032fadd9067c61f4ce6299ad46b414464a0fb383a95827eaa79960cdb5c716c`. Deterministic capture equality and hash assertions remain unchanged. This is a necessary verification update for the intentional design migration and is disclosed in the implementation report.

## Unproven risks
- Visible layout and show/hide timing require the planned user hand test.

## Checks run
- Read item card, implementation report, final recorded gate, design map, JSON roles and source diff against feat/A4-design-tokens.
- Verified the final snapshot change is hash-only; inspected the corrected Tile gap and PreviewBar branch.
- git diff --check: PASS.
- Final recorded gate: Build PASS (0 warnings/errors), Selftest PASS, Format PASS, Seam PASS, Size PASS; Scope FAIL and Tokens FAIL.
- No additional broad tests, GUI launches or process-control actions.

VERDICT: rework - product diff accepted; Scope and Tokens bookkeeping remain before hand-test acceptance.
