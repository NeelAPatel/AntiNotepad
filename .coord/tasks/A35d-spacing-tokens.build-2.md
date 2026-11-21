# A35d-spacing-tokens build, step 2

Role: terra. This step changes one script and its fixtures, then recounts the baseline.

Read `.coord/tasks/A35d-spacing-tokens.build.md` in full. Do **step 2 only**, then write its report.

Context since step 1:
- The branch was rebased onto `docs/A35-ux-pass`, which now includes A35a-data-safety and A35b-icon-registry.
- A35b already extended `tools/check-design-tokens.ps1` with an `icon-code` category. Keep it, and keep its fixtures.
- Before you edit anything, run the build and `--selftest` once. Record the `DESIGN dump` line and `SELFTEST PASS` in
  the report as the post-rebase baseline. If the selftest fails, stop and report; do not fix product code in this step.
- Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A35d-spacing-tokens -Base 3b6ecb5 -Offline`. The base is the
  approve commit, so the lab files committed before it are out of the Scope check.
