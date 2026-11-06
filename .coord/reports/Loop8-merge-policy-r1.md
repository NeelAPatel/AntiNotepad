# Loop8-merge-policy-r1 report

## Summary
Implemented the script changes for rebase and a merge commit per item. Behavioral verification remains incomplete because the scratch clone failed in the sandbox.

## Change list
- `tools/merge-preflight.ps1`: detached per-commit build and selftest, merge-commit rejection, STATUS allowance and merge wording.
- `tools/catch-up.ps1`: rebase, rebase-state refusal and optional item-branch push with lease.
- `tools/approve.ps1`: worktree-default comments only.

## Behavior changes
Product-changing commits are checked oldest first; other commits are skipped. Catch-up leaves conflicts in an open rebase and pushes only when requested. `-Offline` disables NuGet audit for preflight builds; `-SkipBuilds` bypasses them for scratch tests.

## Out-of-scope files
No product, card, STATUS, documentation or skill files changed. This report is the requested reporting artifact.

## Plan deviations
The brief replaces the card's proposed rebase-exec check with a detached scratch worktree. Scratch clone creation failed with Git's `CreateFileMapping` Win32 error 5, so the required catch-up scenarios could not be run; no scratch clone remained.

## Contracts
No contracts listed on the card.

## Lens review
No independent lens review performed in this run; a separate read-only review remains for the item session.

## Checks run
- PowerShell parsing passed for all three scripts.
- `git diff --check` passed, with Git line-ending notices.
- Scratch clone attempt failed before the UP TO DATE, CLEAN and CONFLICT checks could run. Per-commit build, selftest, stale-head and failure paths were not exercised.

## Hand checks for the user
In an environment that permits local Git clones and worktrees, run the three scratch catch-up scenarios and verify the preflight success, skip and failing-commit cases before merge.
