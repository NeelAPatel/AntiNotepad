# Loop2-loop-v3-1-fix-loop-scripts-r2: files-overlap false PASS when worktree enumeration fails

Rework round of `Loop2-loop-v3-1-fix-loop-scripts` (`docs/loop.md`). Read the item card `.coord/tasks/Loop2-loop-v3-1.md` and the
step brief `.coord/tasks/Loop2-loop-v3-1-fix-loop-scripts.md` first; they still apply unless this file says otherwise.

Tier: Low · docs-tools
Role: director

## Why this round
Proven review finding (`.coord/reports/Loop2-loop-v3-1-fix-loop-scripts-review.review.md`, finding 1): in
`tools/files-overlap.ps1` the native `git worktree list --porcelain` exit code is never checked. When it fails with no
output, `$others` stays empty and the script prints `FILES-OVERLAP: PASS (0 other item(s)...)`, exit 0: a false PASS.
Same risk for the earlier `git rev-parse --show-toplevel` call.

## Change to the plan
None. Fix within step 3.1a:
- Capture the `git worktree list --porcelain` output into a variable, check `$LASTEXITCODE` immediately; on nonzero print
  `FILES-OVERLAP: FAIL (cannot list worktrees)` and `exit 1`. Do the same for `rev-parse` (a failure there is also FAIL, exit 1).
- End the successful path with an explicit `exit 0`.
- Check `tools/catch-up.ps1` for the same unchecked-native-call pattern (rev-parse, branch lookup, status, diff) and fix any
  that could yield a false UP TO DATE / CLEAN; refuse with `REFUSED: ...`, exit 1.
- Prove with a throwaway probe (e.g. run the script from a non-git temp copy or a stubbed `git` function); delete it after.
Keep scripts short, in `tools/gate.ps1` style.

## Files
Same four as the step brief; edit only `tools/files-overlap.ps1` and `tools/catch-up.ps1`. Never touch `tools/v2.2` (gone), STATUS, the card, product.

## Done means
- Injected worktree-list failure prints `FILES-OVERLAP: FAIL` and exits 1; normal run in this checkout still PASS exit 0.
- Earlier probe behaviors unchanged (spot-check FAIL/WARN and catch-up CLEAN/CONFLICT exit 0/2 quickly).
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline` passes.

Constraints: no commit, push, branch switch or merge here.
Report: `.coord/reports/Loop2-loop-v3-1-fix-loop-scripts-r2.md`, in the step brief's report format (short).
