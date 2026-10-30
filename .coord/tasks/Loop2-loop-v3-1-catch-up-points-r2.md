# Loop2-loop-v3-1-catch-up-points-r2: RETEST misses git-quoted non-ASCII incoming paths

Rework round of `Loop2-loop-v3-1-catch-up-points` (`docs/loop.md`). Read the item card `.coord/tasks/Loop2-loop-v3-1.md` and
the step brief `.coord/tasks/Loop2-loop-v3-1-catch-up-points.md` first; they still apply unless this file says otherwise.

Tier: Low · docs-tools
Role: director

## Why this round
Proven review finding (`.coord/reports/Loop2-loop-v3-1-catch-up-points-review.review.md`, finding 1): in
`tools/catch-up.ps1` incoming files come from `git diff --name-only` (no `-z`), which git quotes for non-ASCII paths
(`"docs/caf\303\251.md"`), so a listed `docs/café.md` yields `RETEST: no`: a false "no hand test needed".

## Change to the plan
None. Fix: collect incoming paths with `git diff --name-only -z` and split on NUL (keep the exit-code check; the CLEAN
file listing and the CONFLICT file list that use the same pattern should also use `-z`/literal paths so output is not
quoted either, where it is cheap; for the conflicting list `git diff --name-only --diff-filter=U -z`). Compare literal
paths, normalized as before. Keep exit codes and output format otherwise unchanged. PowerShell native output encoding for
non-ASCII must round-trip: set `[Console]::OutputEncoding`/`$OutputEncoding` only if the probe shows it is needed, locally
in the script.

## Files
Edit only `tools/catch-up.ps1`.

## Done means
- Probe in a throwaway clone under `build/Loop2-loop-v3-1-probe` (delete after, including bundle/script leftovers; if
  blocked, say so): incoming `docs/café.md` listed in the card Files => CLEAN with `RETEST: yes (docs/café.md)`; plain
  ASCII listed => yes; unlisted => no; UP TO DATE => no; CONFLICT exit 2; a refusal exit 1.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline` passes.

Constraints: no commit, push, branch switch or merge in this checkout; never edit STATUS, the card, product.
Report: `.coord/reports/Loop2-loop-v3-1-catch-up-points-r2.md` (short, step report format).
