# Loop2-loop-v3-1-fix-loop-scripts-r2

## Summary
Fixed the false PASS when Git cannot enumerate worktrees; catch-up now refuses when required Git inspection fails.

## Change list
- `tools/files-overlap.ps1`: check root lookup and captured worktree-list exit codes immediately; explicit success exit 0.
- `tools/catch-up.ps1`: check root/branch lookup, base resolution, ancestry comparison and both diff calls. Existing status/fetch checks retained.

## Behavior changes
Git inspection errors produce FILES-OVERLAP: FAIL or REFUSED, exit 1. Missing origin base still falls back to the local base; ancestry exit 1 still means catch-up is needed. Successful and conflict paths retain their output/exit contracts.

## Out-of-scope files
Only the two authorized scripts edited. This report and the requested gate's regenerated `.coord/reports/Loop2-loop-v3-1.gate.md` are coordination outputs. Existing STATUS edits, old-path deletions and other artifacts left untouched.

## Plan deviations
None.

## Contracts
No product contracts changed; failure/exit behavior verified by throwaway probes.

## Lens review
Fresh lens reviewer found no proven issues in the r2 changes; accept. Immediate exit-code checks, local-base fallback and ordinary not-ancestor handling reviewed; reviewer made no file changes.

## Checks run
- Child PowerShell processes ran the actual scripts with a stubbed Git function; explicit exit propagation verified each result.
- Injected worktree-list failure: FILES-OVERLAP: FAIL (cannot list worktrees), exit 1. Root failure: FAIL, exit 1.
- Normal checkout overlap: PASS, exit 0. Stubbed other checkout: 17 plain overlaps FAIL plus docs/loop.md hot WARN, exit 1.
- Catch-up root, branch, status, remote-base, ancestry, incoming-diff and conflict-diff failures: REFUSED, exit 1.
- Stubbed UP TO DATE and local-base fallback: exit 0; CLEAN lists incoming.txt, exit 0; CONFLICT marks docs/loop.md [hot], exit 2. These are control-flow probes, not real merges.
- Throwaway `build/Loop2-loop-v3-1-r2-probe.ps1` deleted; absence verified. No repository mutations or network calls from probes.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline`: GATE: PASS, exit 0; build 0 warnings/errors, SELFTEST PASS, seam/scope/format/size/cleanup PASS. Expected offline NuGet audit WARN.
- `git diff --check`: exit 0; existing STATUS line-ending warning only.

## Hand checks
None (docs/tools; no UI). No installed app access, commit, push, branch switch or merge in this checkout.
