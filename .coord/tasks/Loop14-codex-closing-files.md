# Loop14-codex-closing-files: one defined cleanup for Codex closing-message files

Item card (`docs/loop.md`). Budget: 4 KB.

Tier: Low · docs-tools
State: approved
Branch: `chore/Loop14-codex-closing-files` in `.claude/worktrees/chore/Loop14-codex-closing-files` (created at Approve)

## Request
Master 6 on the user's word, 2026-11-17: "set up a Loop item so item cleanup stops tripping on Codex closing-message files." Confirmed with the number by the user in Loop Hub 4, 2026-11-17: "Both need to be done".
- **Goal:** merge cleanup never stalls on Codex closing-message files, with one written process and no per-case asking.
- **Done check:** a dry run of `/merge-task` cleanup on a worktree holding untracked `.coord/reports/<ID-slug>.<role>.codex.md` removes the worktree without asking; the process is one rule in `docs/agents.md` and `merge-task`; `tools/gate.ps1 -Offline` passes.
- **Proof:** the dry-run output, the rule text, gate output.
- **What we know:** Codex's last message is saved untracked as `.coord/reports/<ID-slug>.<role>.codex.md` (T17 left `.quick` and `.worker`); they block `git worktree remove`; the item chat asks before each delete. Tracked `.codex.md` reports also exist in `.coord/reports/` (e.g. A24's), so the rule must tell tracked from untracked. Options: (a) item chat folds anything useful into the real report then deletes; (b) `.coord/tools/codex-run.sh` writes them to an ignored folder such as `.coord/logs/`; plus `merge-task` and `tools/merge-preflight.ps1` treat them as known leftovers at cleanup. User has said yes to deleting the T17 pair (done by Master 6). Overlaps the pstack repeat-mistake "Codex cannot delete check folders" (`.coord/research/pstack-trial-log.md` step 4): fold in if cheap.
- **Constraints:** Low tier, loop tools and docs; prefer a text rule or a wrapper path change over a new gate; the rule itself is the user's to confirm in the plan; Codex may build, offline only. Loop13 edits `merge-task` and `codex-run.sh` too; whichever merges second rebases.

## Findings
Own scout, 2026-11-17.
- `.coord/tools/codex-run.sh` passes `--output-last-message "$ROOT/.coord/reports/<task>.<role>.codex.md"`, so the file lands in the worktree's tracked folder. The log already goes to `$MAIN/.coord/logs/` (ignored, outlives worktrees).
- All 115 `.codex.md` files in the repo are tracked: the item chat committed them with the branch, so they never block. Untracked ones appear only when Codex's last run finishes after the last commit (T17 `.quick`, `.worker`).
- `git worktree remove` refuses untracked files; `merge-task` step 7 then says "stop, list for the user", which is the per-case asking. `tools/merge-preflight.ps1` uses `--untracked-files=no`, so it never sees them and needs no change.
- Each file is only Codex's final chat message (A24: 594 B); the real report is the separate `.coord/reports/<ID-slug>.md` the brief asks for.
- "Codex cannot delete check folders" (pstack-trial-log step 4): the log row gives no detail and I found no matching line; not folded in (not cheap).

## Plan
Recommend (b), wrapper path change, over (a): the cause is the file's location, so move it out of the worktree and nothing is left to clean or to remember. (a) would need every item chat to fold and delete by hand each time.
- **Rule (for `docs/agents.md` Output line):** "Codex's last message is written to `.coord/logs/<ID-slug>.<role>.last.md` (ignored, main checkout). It is not part of the item's commits. The Codex report is `.coord/reports/<ID-slug>.md`." Existing tracked `.codex.md` files stay as history.
- **Rule (for `merge-task` step 7 and `branches.md` Clean-up):** "Untracked `.coord/reports/*.codex.md` in the worktree is a known leftover (older wrapper copy): delete it, then `git worktree remove`. Any other leftover: stop and list it." Tracked files are never touched.
- Files: `.coord/tools/codex-run.sh` (output path and DONE line), `docs/agents.md`, `.claude/skills/merge-task/SKILL.md`, `.claude/skills/merge-task/branches.md`.
- Expected time: about 10 minutes of Codex, small.
- Dry-run test: `CODEX_DRY=1 codex-run.sh quick X-y f` prints `report=` under `.coord/logs`; then a scratch worktree with an untracked `A-b.quick.codex.md` is removed by the rule's steps without asking, and one with another untracked file still stops. `tools/gate.ps1 -Offline` passes.
- Rebase: Loop13 also edits `merge-task` and `codex-run.sh`; second to merge rebases.

## Files
- `.coord/tools/codex-run.sh`
- `docs/agents.md`
- `.claude/skills/merge-task/SKILL.md`
- `.claude/skills/merge-task/branches.md`

## Contracts
None (docs and tooling).

## Approve
User, 2026-11-17: "Approve"

## For Codex
Role: quick (`docs/agents.md`), because small text and path edits. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then the Files.
- Plan's rule: `codex-run.sh` writes Codex's last message to `$LOGS/<task>.<role>.last.md` (not `.coord/reports/*.codex.md`); fix the PLAN and DONE lines. `docs/agents.md` Output line states it. `merge-task` step 7 and `branches.md` Clean-up: untracked `.coord/reports/*.codex.md` in the worktree is a known leftover: delete it, then `git worktree remove`; any other leftover stops and is listed. Tracked files never touched.
- Offline checks only: `CODEX_DRY=1 bash .coord/tools/codex-run.sh quick X-y <file>` prints `report=` under `.coord/logs`; `tools/gate.ps1 -Task Loop14-codex-closing-files -Offline`. Never edit STATUS or this card.
- Done means: those edits only; dry-run output pasted in the report; gate passes.

Report `.coord/reports/Loop14-codex-closing-files.md`, short: 1 Summary; 2 Change list; 3 Out-of-scope files; 4 Checks run.
