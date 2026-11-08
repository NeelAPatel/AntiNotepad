# L7-lv4-playbook: one-page habit guide for the Level 4 dev loop

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build.

Tier: Low · docs-tools (rules: `docs/loop.md`)
Branch: `chore/L7-lv4-playbook` in `.claude/worktrees/chore/L7-lv4-playbook` (created at Approve)

## Request
Level 4 item 7 of 7 (2026-11-08, via `.coord/research/lv4-shared-brief.md`): write `docs/playbook.md`, a one-page habit guide
with a numbered walk-through for an A, a T and an L item, what the hooks/agents/skills/status line do automatically versus
what the user still does, every slash command (who runs it, when), what to do when a hook blocks something, and a short
"results" section from measured L1-L6 numbers. Link it from `AGENTS.md`'s read-when list. Validate by dry-running it.

## Findings
- **Kind and why:** process; the user wants to build routines on L1-L6, and today the steps live across `loop.md` (35 KB).
- **Now:** no playbook. Only 3 slash commands exist (`/next-task`, `/review-task`, `/merge-task`) plus `/roadmap`,
  `/scan-backlog`. L1-L6 have no merged work yet (checked 2026-11-08: dev tip is T11-roadmap-hide-rows; no L1-L6 cards).
- **Where:** new `docs/playbook.md`; one line in `AGENTS.md` "Read when". Possibly one line in `docs/glossary.md`.
- **Risk:** describing things that L1-L6 end up building differently. Mitigation: final text written only after L1-L6 merge,
  every claim checked against the merged files (hook names, agent names, skill flags, status-line fields).
- **Source:** master-less scout (this session), read `loop.md` phases, Sessions and Triggers, and the native-features note.

## Plan
- **After:** `docs/playbook.md`, one page (about 4 KB): (1) the 3 walk-throughs (A, T, L) as numbered steps; (2) "what runs for
  you vs what you do" table; (3) slash-command table (command, who types it, in which chat, when); (4) "a hook blocked
  something" table (message seen, why, what to do, how to switch off); (5) results table. Plain words, each term glossed.
- **Expected time:** drafting 20 min now; final pass 30-40 min after L1-L6 merge. Under an hour of my work, but the merge
  wait is outside my control. Smaller cut if L5/L6 lag: ship without the results section and add it later.
- **Must not change:** app code, gate scripts, `.claude/` config, loop.md content (L6 owns its split).
- **Scope questions:** none beyond the one asked in the first reply (when to write).
- **Test plan, code:** None (docs only).
- **Test plan, UI:** None (not visible). Hand test = the user reads the guide and says whether it reads clearly.
- **Gates:** `gate.ps1 -Task L7-lv4-playbook -Offline` runs format, scope, loop-lint (catches dead file references and
  command names), size. Dry run: walk each numbered step against the merged repo (command exists, hook message matches
  text in the hook file, status-line fields match). Results table numbers copied from L1-L6 reports with the report named.
- **Needs from you:** approve the plan; later, read the guide and say pass or fail; optionally pick a real next item to walk
  through it.

## Files
Only these files may change.
- `docs/playbook.md`
- `AGENTS.md` (one link line, after L6 merged)
- `docs/glossary.md` (only if a new term needs a line)

## Contracts
None.

## Approve
"plan is approved, start the worktree, branch and build this out." (user, 2026-11-08). Draft now; finish and verify after L1-L6 merge.

## For Codex
Role: quick (Luna medium), because it is a short docs write from verified facts. Brief to be written at Approve, after
L1-L6 merge, listing exact hook/agent/skill names read from the merged tree. No port, server or browser checks.

## Hand test
"Reads clearly" (user, 2026-11-08). No live walk-through of a real item yet; guard still log-only.

## Check
Gate: `tools/gate.ps1 -Task L7-lv4-playbook -Offline` PASS (build 0 warnings, selftest, seam, tokens, format, scope, loop-lint, size).
Bounds: request = one-page guide + AGENTS link; plan = same; done = `docs/playbook.md` and one line in `AGENTS.md`. No other files.
| Claim | State |
|---|---|
| Commands, hook names, ask rules, agent limits, skill flags in the guide match the merged files (read on dev 21b72b9) | verified by reading each file |
| Guard messages and `log`/`block` switch (`tools/guard.mode`), log at `build/guard.log` | verified in `tools/guard-bash.ps1` |
| Results numbers (skills 1,395 to 150 B; start reading 39,078 to 18,225 B; hook text 487 to 279; status line 358 ms) | taken from L4, L5, L6 reports; start-reading sum re-measured with `wc -c` |
| Guard stopping a real command in a live session; auditor's real model and tools | not tested (stated as open in the guide) |
| Walk of a real item through the guide | not done |
Deviation: no Codex run (short docs file written directly from verified files).
