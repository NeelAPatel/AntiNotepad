# Loop2-loop-v3-1-scan-backlog-review lens review

## Findings (proven)
None.

## Behavior changes check
Report's list vs the diff: matches.

- `tools/scan-delta.ps1:31-34,44-60,70-78`: distinct touched paths produce WHERE or same-folder NEAR marks; unrelated paths are omitted. Exactly half does not trigger RESCAN; more than half or any Where deletion does. Renames are delete/add, and changes later reverted still count. Successful output ends with DELTA and exits 0; failures exit 1 before a success DELTA.
- `tools/scan-delta.ps1:12-15,24-28,37,44-46,63-68`: SHA/Base validation and every Git read use the checked helper. History subjects follow the first parent; merge file changes are compared against the first parent. Existing cards are selected from anchored ID-slug subjects and deduplicated. The new next-task Findings block is the caller and explicitly stops on failure before using the scan.
- `tools/approve.ps1:28,46,72-79`: removing the first ID separator derives the research slug, including lettered step IDs and multiword slugs. Existing research must appear in Git's untracked, nonignored list. Absent research contributes no file and preserves the existing route.
- `tools/approve.ps1:82-99`: research joins the existing planning-file collection. Worktree mode creates its destination directory, moves it, stages it and commits it with the card; NoWorktree stages it in place. All mutations remain inside `if (-not $DryRun)`. STATUS is neither selected nor written; its only use is the existing read-only preflight. The existing next-task Approve caller needs no argument change.
- The scan-backlog skill and scout template agree on the detached dev snapshot, absolute paths, one main-checkout report per slug, full scan SHA, evidence sections, untracked lifecycle, and cleanup after all readers finish. The next-task block and Backlog scans paragraph agree with the script's rescan threshold and retain Medium/High audits.

## Scope and plan check
In plan. The reviewed implementation contains exactly the brief's six files: the three new scan artifacts, next-task skill, approve script and loop documentation. `docs/loop.md` changes only the Backlog scans paragraph and the three specified Files rows. No unlisted implementation deviations found. Existing coordination artifacts and STATUS changes are outside this implementation diff and were left untouched.

The checklist found no new persistent state, deferred work, event/resource lifecycle, product layering change, or product contract requirement. Snapshot cleanup and failed-scout handling are explicit in the skill. Script promises have focused probe evidence in the implementer report; no product tests are needed for these changes.

## Unproven risks
- Live linked Haiku scout orchestration and snapshot cleanup were not exercised in this review or the implementer run.

## Checks run
- Read the item card, step brief, implementer report, supplied gate report, review skill, coding guidance, scoped diff, full changed scripts and skills, and relevant callers. Reviewed failure paths and all checklist items.
- In-memory execution of scan-delta's parsing/classification/output logic with substituted scan text and Git responses: PASS for none, near-only, one of two Where files, both Where files, deletion, card selection, unrelated-path omission, and a simulated Git failure producing no DELTA. Filesystem existence checks were stubbed; exit statements were adapted to return/throw for the harness. No probe files were created.
- Real read-only Git merge check at `dd86b4245ebc6b7c65dfa6214f2dff36ead569b5`: PASS; the script's diff-tree options returned the same 22 changed paths as an explicit diff against the merge's first parent. First-parent range logging returned only the merge subject.
- Executed the actual Invoke-Git helper with an invalid ref: PASS, Git failure throws. Ran the unchanged scan-delta script with a missing research slug: PASS, `SCAN-DELTA: FAIL`, exit 1, no DELTA.
- PowerShell parser checks: PASS for scan-delta.ps1 and approve.ps1.
- Approve's research move, no-research route, DryRun and STATUS exclusion were checked by exact source paths above. Mutation probes were not rerun because this review permits writes only to this report. The supplied implementer report records disposable-clone move/no-research probes; the supplied gate records GATE: PASS. No build or gate was rerun.
- No branch switch, merge, commit, product action, or file edit outside this report.

VERDICT: accept
