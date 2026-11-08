# L4-lv4-skill-diet report

## Summary
Four loop skills are now typed-only (`disable-model-invocation: true`) with one-sentence descriptions. next-task,
review-task and merge-task get a `## Live facts` block of `!` git commands (shell `powershell`, `allowed-tools:
PowerShell(git *)`). review-task notes an optional forked bounds check. Codex wrote the first pass; the manager corrected
the `!` block after tests showed the first form was blocked by permissions.

## Sizes (bytes of `description:` lines; what the model's skill list carries)
| | Before | After |
|---|---|---|
| next-task | 392 | 0 (hidden) |
| review-task | 315 | 0 (hidden) |
| merge-task | 418 | 0 (hidden) |
| scan-backlog | 120 | 0 (hidden) |
| roadmap | 150 | 150 |
| **Total** | **1,395** | **150** (about 90% less; kept descriptions would now total 405) |
SKILL.md bodies grew 22,216 to 22,624 bytes (+408); bodies load only when a skill runs.

## Verified vs assumed
| Claim | State |
|---|---|
| Hidden skills do not appear in a fresh model's skill list (headless haiku test lists only `roadmap`) | verified |
| `!` runs, `$ARGUMENTS` is substituted, `shell: powershell` works (temp skill via `claude -p`, deleted) | verified |
| `!` command needs a permission: without `allowed-tools: PowerShell(git *)` it was refused, and compound `; exit 0` or `Select-String` would not match that rule | verified; Codex's first form fixed |
| `context: fork` + `agent: design-reviewer` runs and the agent sees CLAUDE.md | verified (temp skill, deleted) |
| Typed `/name` runs a `disable-model-invocation` skill | verified for the `!` test skill only (that one lacked the flag): NOT tested on the real four |
| "merge X" in plain words no longer starts merge-task | implied by the list test; not tried directly (a real merge risk) |
| Full end-to-end run of each changed skill | NOT done; the `!` path is tested, the first real runs are this item's own Check and Loop Hub's `/merge-task` |
| `git grep` in review-task aborts the skill if the card is missing | by design (assumed from docs: failed command aborts) |

## Change list / Behavior changes / Deviations
- Four `SKILL.md` files only. Behavior: models cannot auto-start those four skills; type `/name`. Missing card stops review-task.
- Deviation: whole-skill `context: fork` for review-task not used (cannot ask the user); only a note for a forked pass.
- Stale prose for L6: loop.md and AGENTS.md may say "say merge X"; not edited here.

## Checks run
`gate.ps1 -Offline`: PASS (the online gate fails only on NU1900, no network to NuGet in this shell).
