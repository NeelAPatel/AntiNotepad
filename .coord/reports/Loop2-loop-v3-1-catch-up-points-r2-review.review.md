# Loop2-loop-v3-1-catch-up-points-r2 lens review

## Findings (proven)
None.

## Behavior changes check
Report's list vs the diff: matches. Incoming and conflicting paths use `-z` and split on NUL with empty entries removed. Both Git exit-code checks remain immediately before parsing (`tools/catch-up.ps1:72,83`). Empty output produces zero paths; a single path stays one path with or without its trailing NUL; multiple literal paths retain their boundaries, Unicode and spaces.

The existing card parser and Write-Retest are unchanged by this round. Listed literal paths trigger RETEST yes, unlisted paths and empty incoming output produce no. CLEAN lists literal incoming paths with the correct count; CONFLICT preserves its count, indentation and hot-file marking. UP TO DATE/CLEAN/CONFLICT retain exits 0/0/2; refusals retain exit 1. Conflict and refusal paths emit no RETEST. No encoding setting, persistent state, timing, cleanup ownership or product dependency changed.

Consumers checked: review-task steps 2 and 5, merge-task step 1 and docs/loop.md's catch-up paragraph. Their existing output and exit-code contracts still hold; there are no signature changes or new callers.

## Scope and plan check
In plan. The round's two path-collection changes address the prior proven finding. The complete working diff against HEAD also includes the earlier step's card parser, local-base selection and RETEST additions; those match the step brief and prior review. Beyond those earlier changes, the script's only differences are adding `-z` to the two diff commands and adding their NUL-split assignments. No unrelated script changes or unlisted implementation deviations found. The report discloses the bundle-origin probe and cleanup deviation.

## Unproven risks
- None retained.

## Checks run
- Read the lens-review skill, STATUS, item card, both briefs, prior finding and implementer report; inspected the complete tools diff, surrounding script, card-parser peer and output consumers. Branch verified as chore/Loop2-loop-v3-1.
- PowerShell AST parse: PASS, zero errors. `git diff --check -- tools`: PASS.
- In-memory child-PowerShell probes exercised the current script with Git mocked and only Get-CardFiles replaced by a fixed literal list: empty incoming, single path without trailing NUL, single path with trailing NUL, Unicode, spaces, multiple paths and unlisted path all PASS. CLEAN counts and each literal incoming listing were asserted, along with RETEST yes/no and exit 0.
- UP TO DATE: PASS, RETEST no, exit 0. Three-path CONFLICT including Unicode, spaces and a hot file: PASS, exact header/count and literal listings, no RETEST, exit 2.
- Incoming-diff failure, conflict-list failure, failed merge without conflicts and dirty-checkout refusal: PASS, exit 1, REFUSED, no RETEST or success header. Failed Git calls supplied nonempty path output in the two diff-failure cases, confirming it cannot lead to a successful result.
- Native child-PowerShell stdout containing Unicode, spaces and trailing NUL delimiters, captured and parsed using the same join/split expression: PASS. No encoding override needed for this host probe. This producer was PowerShell, not Git.
- Supplied gate report reviewed: GATE PASS, build zero warnings/errors, SELFTEST PASS, CONTRACTS 13 rows (12 pass, 1 manual). Gate and real-Git fetch/merge probes were not independently repeated in this read-only review. The implementer's reported probe harness is no longer present at build/Loop2-loop-v3-1-probe/probe.ps1, so its real-Git results are report evidence only.
- Only this report was written; no probe files, tool/product edits, STATUS edits, branch switches, commits, fetches, merges or app actions performed.

VERDICT: accept
