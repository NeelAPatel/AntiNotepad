# T3-codex-unpause-docs: remove the "Codex is paused" lines now that Codex is allowed

Item card (`docs/loop.md`), fast lane (side task, `docs/loop.md` "Rules").

Tier: Low · docs-tools (rules: `docs/loop.md`)
Branch: `chore/T3-codex-unpause-docs` in `.claude/worktrees/chore/T3-codex-unpause-docs` (created at Approve)

## Request
User to Master 2, 2026-11-01: "now that Codex is available to be used as intended originally", then "Codex allowed"
(relayed by Master 2). STATUS `idea: codex-unpause-docs`; Loop Hub 2 proposed it as T3-codex-unpause-docs; the user
approved (below).

## Findings
- **Kind and why:** process; the loop docs still tell every session Codex is paused, which contradicts the user.
- **Now:** `AGENTS.md:24-25` (Roles: "**Codex is paused** (user, 2026-10-31) ... The rules below apply when it returns."),
  `docs/loop.md:164-165` (Rules: "**Codex is paused** ... the Codex rules below wait for its return."),
  `docs/agents.md:3-4` (header: "**Paused** ... This page applies when Codex returns.").
- **Where:** those three passages only. STATUS "Working rules" also says paused; that line is Master 2's (it owns
  STATUS overall) and is not in this item.
- **Risk:** leaving a dangling reference (loop-lint catches broken file references).
- **Source:** master scout.

## Plan
- **After:** the three passages are removed; "Who builds" in `docs/loop.md` and the Roles lines in `AGENTS.md` stand as
  the rule (Codex builds, roles by job). Optionally one short history note in `docs/loop.md` Rules: "Codex pause
  2026-10-31 to 2026-11-01 (user)". Nothing else reworded.
- **Expected time:** about 10 minutes, request to merge.
- **Must not change:** any other rule text.
- **Scope questions:** None.
- **Test plan, code:** `rg -n -i "paused" AGENTS.md docs/loop.md docs/agents.md` shows no live pause rule.
- **Test plan, UI:** None (not visible).
- **Gates:** `tools/gate.ps1 -Task T3-codex-unpause-docs`: scope, loop-lint, format.
- **Needs from you:** Nothing.

## Files
- `AGENTS.md`
- `docs/loop.md`
- `docs/agents.md`

## Contracts
None.

## Approve
User, 2026-11-01: "Approve both" (T2-hot-files-stale-path and T3-codex-unpause-docs).

## For Codex
Not used: fast lane, a few doc lines; the builder sub-session edits them itself and says so in its report.

## Check
- Gate `tools/gate.ps1 -Task T3-codex-unpause-docs`: PASS (Scope 3 files allowed, LoopLint PASS 39 files, Format none, Size, Seam, selftest PASS).
- Bounds check: request (remove the pause rule) = plan (three passages) = done (6 lines deleted in AGENTS.md, docs/loop.md, docs/agents.md); no other wording changed.
- `rg -n -i "pause|when it returns|its return"` on the three files: no matches.
- Misses: None.

## Hand test
None (not visible).
