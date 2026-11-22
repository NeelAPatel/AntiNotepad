# A35d-spacing-tokens build: step 2

## 1 Summary
Scanner and baseline updated for spacing zeros, pen strokes and drawn radii; icon-code support and fixtures retained.

## 2 Change list
- `tools/check-design-tokens.ps1:10` | Skip zero spacing literals | step 2 scanner
- `tools/check-design-tokens.ps1:264` | Inspect nested call arguments for pen thickness and drawing radii | step 2 scanner
- `tools/check-design-tokens.ps1:373` | Hit and no-hit fixtures for each rule | step 2 fixtures
- `tools/design-tokens-baseline.tsv:1` | Recount all categories | step 2 baseline

## 3 Behavior changes
The token ratchet ignores zero spacing and counts literal pen thickness and drawing radius arguments. Product behavior is unchanged.

| Category | Old | New |
|---|---:|---:|
| alpha | 0 | 0 |
| color | 5 | 5 |
| font-family | 0 | 0 |
| font-size | 4 | 4 |
| font-weight | 0 | 0 |
| icon-code | 0 | 0 |
| opacity | 2 | 1 |
| radius | 6 | 21 |
| shadow | 0 | 0 |
| spacing | 193 | 27 |
| stroke | 0 | 6 |
| timer | 14 | 12 |
| tween | 0 | 0 |

The recount also incorporates changes since the previous baseline, including step 1 and the rebase.

## 4 Out-of-scope files
None edited. The pre-existing untracked `.coord/tasks/A35d-spacing-tokens.build-2.md` was left untouched. The gate generated its usual ignored report.

## 5 Deviations
None.

## 6 Checks run
- Post-rebase, before edits: Debug build, 0 warnings and 0 errors; `DESIGN dump f08e0c448f3f7a400dd73fe7710bd51e05f6545782c40b3caa6ed5c24466aa79 18048`; `SELFTEST PASS`.
- Scanner fixtures: `TOKENS SELFCHECK PASS`.
- Recount: `TOKENS baseline updated`.
- Offline gate with base `3b6ecb5`: `TOKENS PASS`, `GATE: PASS`; build 0 warnings, selftest pass. LoopLint reported existing budget warnings for loop.md, lessons.md and the item card.

The pre-edit check output remains at `build/A35d-spacing-tokens-check`: the sandbox rejected the guarded cleanup command. The gate cleaned its own task output.
