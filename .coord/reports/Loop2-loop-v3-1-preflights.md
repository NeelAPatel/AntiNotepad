# Loop2-loop-v3-1-preflights: step 3.1b

## Summary
Added scripted Approve and Merge preflights and connected the skills to them. Gate reports identify the tested HEAD,
and the loop-card hook warns when more than three items are past Approve.

## Change list
| File:lines | What changed | Plan line |
|---|---|---|
| `tools/approve.ps1:1-95` | Main checkout, card, cap and overlap checks; dry run; branch/worktree creation and card commit | After 1 |
| `tools/merge-preflight.ps1:1-78` | Branch/card/checkout checks, dev ancestry, newest gate and exact HEAD, main merge checks | After 2 |
| `tools/gate.ps1:1,40-46,248` | File header; capture HEAD and tracked dirty state at start; emit Head immediately after GATE | After 3 |
| `tools/loop-card.ps1:14,23,32` | Count past-Approve rows with the existing table parser and warn above three | After 4 |
| `.claude/skills/next-task/SKILL.md:53-56` | Step 4 invokes approve; FAIL stops; NoWorktree requires user's instruction | After 5 |
| `.claude/skills/merge-task/SKILL.md:14-17` | Steps 1-2 invoke merge-preflight and retain outside-Files diff check | After 5 |

## Behavior changes
- Approve refuses unsafe checkout/card/cap/overlap states before creating an item. Default creates a worktree; NoWorktree
  creates the branch in the main checkout and preserves unstaged STATUS. Only the untracked card and matching audit
  tasks/reports are moved and committed; STATUS is never edited or staged. DryRun executes checks and prints commands.
- Merge preflight reads the committed branch card and the newest gate report in that branch's checkout. It rejects
  dirty branch checkouts, missing phases, open questions, behind-dev branches, failed/stale/dirty gate HEADs, and a staged
  main index or merge marker. It prints the required switch-to-dev instruction for NoWorktree items.
- Gate Summary now records the full initial HEAD, adding `-dirty` for tracked changes outside STATUS. Untracked files
  do not trigger that suffix. Existing gate checks and outcomes are unchanged.
- Hook output adds an ASCII warning only above three past-Approve items. Skills now stop on preflight failures.

## Out-of-scope files
None. Disposable probe harness, bundle, results and clone are under `build/`; the requested report is here. Existing STATUS,
history-preview and brief changes were left untouched.

## Plan deviations
None in implementation. Direct local cloning failed in Git for Windows' shell (`CreateFileMapping`, Win32 error 5);
the disposable clone was instead created from a local `git bundle` of dev. No network access or source Git writes.
Cleanup remains outstanding: automatic approval review rejected the recursive deletion and a guarded file-by-file
PowerShell deletion with `blocked by policy`. No cleanup was performed by those commands. The manager must remove
`build/Loop2-loop-v3-1-probe` and the adjacent `Loop2-loop-v3-1-probe.ps1`, `.bundle` and `-results.txt` artifacts.

## Contracts
None (docs/tools only; no product changes).

## Lens review
Fresh `lens_reviewer` subagent: no proven findings, behavior list matches, scope in plan, no unproven risks identified;
`VERDICT: accept`. Read-only review of both new scripts, changed hunks, overlap dependency, card, brief, report and gate;
no additional builds or mutations. Reviewer also noted the outstanding disposable-clone cleanup.

## Checks run
- Disposable clone: `build/Loop2-loop-v3-1-probe`, small STATUS and synthetic cards, local probe commits only. Gate reports
  below were synthetic with the appropriate branch SHA; these checks validate the preflight, not a real product gate.
- `PROBES: PASS (21 cases)`:

| Case | Observed result | Exit |
|---|---|---|
| Approve, three other past-Approve rows | FAIL (cap) | 1 |
| Approve, empty Approve | FAIL (Approve section missing or unfilled) | 1 |
| Approve, missing Approve | FAIL (Approve section missing or unfilled) | 1 |
| Approve, placeholder Approve | FAIL (Approve section missing or unfilled) | 1 |
| Approve, dirty tracked file | FAIL (dirty checkout) | 1 |
| Approve, non-hot Files overlap | FAIL (overlap) | 1 |
| Approve DryRun | PASS (dry run); card remains, no worktree created | 0 |
| Approve worktree | PASS (card committed); card and audit task/report moved and tracked; subject checked | 0 |
| Merge, caught-up branch and matching gate | PASS (ready) | 0 |
| Merge, stale Head | FAIL (stale Head: re-run the gate) | 1 |
| Merge, newer failing round gate | FAIL (newest gate did not pass) | 1 |
| Merge, dirty item checkout | FAIL (dirty item checkout) | 1 |
| Merge, open ask the user line | FAIL (open ask the user line) | 1 |
| Merge, same line answered | PASS (ready) | 0 |
| Merge, staged main squash content | FAIL (staged changes in main checkout) | 1 |
| Merge, SQUASH_MSG | FAIL (merge in progress: SQUASH_MSG) | 1 |
| Merge, MERGE_HEAD | FAIL (merge in progress: MERGE_HEAD) | 1 |
| Merge, dev advanced | FAIL (behind dev: run tools/catch-up.ps1 -Task T900-probe) | 1 |
| Approve NoWorktree, unstaged STATUS | PASS (card committed); STATUS content preserved | 0 |
| Merge NoWorktree, matching gate | PASS (ready); prints switch-to-dev instruction | 0 |
| loop-card, four past-Approve rows | WARNING: 4 items past Approve (cap 3). | 0 |

- Main checkout `pwsh -NoProfile -File tools/loop-card.ps1`: exit 0; normal reminder and
  `In flight per STATUS: Loop2-loop-v3-1 (approved, Low).`
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline`: exit 0, `GATE: PASS`;
  build 0 warnings / 0 errors, `SELFTEST PASS`, CONTRACTS 13 rows (12 pass, 1 manual), seam/format/scope/size/cleanup PASS.
  Only expected WARN: offline NuGet audit disabled. Summary immediately follows GATE with
  `Head: ac450d2ca41f41c2095a0b7b624c8635658dc9d8-dirty` (tracked tooling edits).
- `git diff --check`: exit 0 (existing STATUS line-ending warning only).

## Hand checks for the user
None (not visible).
