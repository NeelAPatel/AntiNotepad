# Loop2-loop-v3-1-scan-backlog

## Summary
Added batch backlog scan instructions and a SHA-stamped scout template. Findings now starts from scan deltas;
Approve carries the matching untracked research file into the item branch with its card.

## Change list
| file:line-range | what changed | Plan line |
|---|---|---|
| `.coord/templates/scan.md:1-33` | Haiku scout brief and evidence report format | After 1 |
| `.claude/skills/scan-backlog/SKILL.md:1-32` | selection, detached snapshot, linked scouts, table, ordering and cleanup | After 2 |
| `tools/scan-delta.ps1:1-79` | validated scan input, first-parent history, Where/near paths, cards, delta and rescan | After 3 |
| `.claude/skills/next-task/SKILL.md:29-34` | scan-first Findings and unchanged tier audit requirements | After 4 |
| `tools/approve.ps1:72-79` | include matching untracked research among planning files | After 5 |
| `docs/loop.md:40-45,271,277,286` | Backlog scans paragraph and three Files rows | After 6 |

## Behavior changes
- New read-only `scan-delta.ps1 -Slug <slug> [-Base dev]`: prints commit subjects, distinct WHERE/NEAR paths and
  existing task cards, then RESCAN and a final DELTA line. Counts paths touched across commits, including reverted
  changes; renames are delete/add. More than half of Where files touched or any Where deletion requires a rescan.
  Missing research, malformed/unknown SHA, missing Where section or Git failure exits 1; successful comparisons exit 0.
- Approve moves and commits the research file whose slug is Task without the leading ID. Existing research must be
  untracked, consistent with the planning-file lifecycle; tracked or ignored research refuses before branch creation.
  The existing no-research route and DryRun behavior remain supported.
- The new master skill uses Haiku scouts without Codex; proposed ordering changes STATUS only on the user's word.
  Findings consumes scan, delta, cards and relevant files; rescan and Medium/High audit requirements remain explicit.

## Out-of-scope files
None. This report and the generated `.coord/reports/Loop2-loop-v3-1.gate.md` are reporting artifacts.
Temporary probe scripts, bundle and clone under build are removed after checks. Pre-existing STATUS edits,
history preview and step brief were left untouched.

## Plan deviations
The local clone route hit Git-for-Windows `sh.exe` CreateFileMapping error 5. Created a local bundle and cloned it
into the required disposable path instead; no network or source checkout Git mutations. No functional deviations.

## Contracts
None (docs/tools only; no product files touched).

## Lens review
Fresh `lens_reviewer`: no proven findings, behavior list matches, all six files in plan, VERDICT: accept.
Reviewer also accepted the final merge-diff option correction with no remaining findings. Live scout orchestration
remains unverified.

## Checks run
- Disposable clone: `build/Loop2-loop-v3-1-probe`, deleted after all probes. Commits, branch creation and research
  moves occurred only inside that clone and its nested worktrees.
- PASS: no commits gives `RESCAN: no`, final `DELTA: none`.
- PASS: nearby-only gives `NEAR fixture/near.txt`, final `DELTA: near (1)`.
- PASS: one Where file plus nearby and unrelated changes prints WHERE/NEAR, excludes the unrelated path, includes
  `.coord/tasks/A1-probe-change.md`, gives `RESCAN: no`, final `DELTA: where (1 of 2 Where files changed)`.
- PASS: both Where files changed gives `RESCAN: yes`, `DELTA: where (2 of 2 Where files changed)`.
- PASS: one Where deletion gives `RESCAN: yes`, `DELTA: where (1 of 2 Where files changed)`.
- PASS: absent stamp, unknown full SHA, unknown Base and missing research each exit 1.
- PASS: a non-fast-forward merge includes integrated Where changes while omitting the side branch's commit subject.
  The initial merge probe caught suppressed merge diffs; using `--diff-merges=first-parent` fixed it, and the complete
  clone probe set passed again.
- PASS: Approve with research moves it out of the clone's main checkout and commits it with the card in its item
  worktree. Approve without research also commits the card and passes.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline`: exit 0, GATE: PASS.
  Build: 0 warnings, 0 errors; SELFTEST PASS; CONTRACTS: 13 rows, 12 pass, 1 manual. Seam, format, scope, size and cleanup
  PASS. Expected Offline warning: NuGet audit disabled for this build only.
- Live `/scan-backlog` Haiku orchestration was not exercised in this Codex session; the skill is an instruction artifact.

## Hand checks for the user
None (not visible).
