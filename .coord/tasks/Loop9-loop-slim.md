# Loop9-loop-slim: cut the loop's token use and friction without removing Level 4

Item card (`docs/loop.md`). Director (Loop Hub 3) writes Request to Approve; the build chat owns Build, Check, Hand test.

Tier: Medium · docs-tools (loop tooling and hooks; no product code)
Branch: `chore/Loop9-loop-slim` in `.claude/worktrees/chore/Loop9-loop-slim` (created at Approve). **Umbrella item:** steps a-g are commits on this one branch and land in one merge (user override, 2026-11-12; precedent `A4-design-tokens`).

## Request
User, 2026-11-12: usage is far higher after Level 4; adding a mod throws the loop into a frenzy (`.claude/settings.json`); sub-chats should be one level per item; too many permission prompts on STATUS and other files. "Find out what are the causes ... streamline ... while primary goal being to cut usage% and make our $20 plans last longer." Then: shrink other hot `.md` files too; pause plugins/connectors; "put it on the roadmap with the correct prefix"; run overnight on a branch, merge tomorrow.

## Findings
- **Evidence (Loop Hub 3 analysis, 2026-11-12, chat logs since 11-05):** long chats re-read big contexts every turn; STATUS is 82 KB (31 KB finished history), read by every director at start and after each compaction; the compaction hook prints 46 KB of all directors' notes into every compacting chat; project `ask` rules prompt on every STATUS and lessons edit; the app rewrote tracked `.claude/settings.json` when a mod was enabled at project scope, so `approve.ps1` saw a dirty checkout.
- **Not the cause:** Level 4 itself (per-turn hook about 280 characters). Keep agents, user-only skills, status line, log-only guards.
- **Out of scope here (user's own settings, listed in the report):** compact window, advisor model, directors on Sonnet. Plugins and connectors guide: paused by the user.

## Plan
Run in this order (steps that need no permission prompt first, so a stall costs little):
- **a-settings-scope:** `tools/approve.ps1` ignores `.claude/settings.json` drift; one rule line in `docs/dev-loop.md`: mods go in user scope or `settings.local.json`, never project scope.
- **b-ask-rules-trim:** `.claude/settings.json` keeps `ask` only on `docs/loop.md` and `AGENTS.md` (and `start_session`); STATUS and lessons edits are allowed. Stop `tools/catch-up.ps1` and the merge-task text from suggesting a force-push: merge from the local branch.
- **c-notes-own-only:** `tools/director-notes.ps1` prints only the chat's own notes file, and only in director chats; old Master 2 and 3 notes move to `.coord/directors/archive/` (untracked).
- **d-status-diet:** finished rows move to `.coord/status-archive.md`; STATUS target **8 KB** (rules, live rows, Plan table). Merge keeps only the last 5 done items in "Now". Plan section format unchanged; check `tools/roadmap-data.ps1` output and `tools/loop-card.ps1` before and after.
- **e-budgets-cards:** `.coord/templates/item.md` to about 3 KB (target 4 KB per card; logs go in reports); `.coord/lessons.md` to **3 KB** (last 10 Scores rows kept, rest in `.coord/lessons-archive.md`); Level 4 research briefs move to `.coord/research/archive/`; skills `next-task`, `review-task`, `merge-task` trimmed to about 5 KB each.
- **f-budgets-loop-docs:** `docs/loop.md` to **8 KB** (rationale and user quotes to `docs/decisions.md`; one "Loop version" line; no text duplicated in `loop-sessions.md`); `docs/decisions.md` gets a 15-line "locked choices" list on top; add the one-level rule line ("a sub-chat never starts a chat").
- **g-budget-guard:** `tools/loop-lint.ps1` WARNs (never fails) when a hot file is over budget: STATUS 8 KB, `docs/loop.md` 8 KB, `.coord/lessons.md` 3 KB, item cards 4 KB.
- **Expected time:** about 90 minutes of work; overnight wall time 2-3 hours with gates. Over an hour, so the cut line is: steps a-d carry about 80% of the saving.
- **Must not change:** product code, the installed copy, the Plan table format, Level 4 hooks and agents, the plugin and connector setup.
- **Test plan:** gate offline on every commit; `pwsh tools/roadmap-data.ps1` parse unchanged (same Plan rows before and after); `tools/loop-card.ps1` lists the same in-flight items; before and after sizes in the report.
- **Gates:** gate (Scope, Format, build, selftest); no Codex review (no product code); no hand test beyond the user reading the report.
- **Overnight rules for the build chat:** a permission prompt is never worked around (no Bash writes to a protected file): commit what is done, mark the step `blocked` in the report, go to the next step. Three failed tries on a step: stop that step, log it, continue. No force-push; push the branch once, plainly, at the end. Never touch STATUS outside step d (the director owns it).
- **Merge notes (director):** step d edits STATUS, so before merging I set my pending STATUS edits aside as a patch (no stash), merge, then re-apply them and the done rows on the slimmed file.
- **Needs from you:** read `.coord/reports/Loop9-loop-slim.md` tomorrow; say merge. Then, in your own settings, apply the three values the report lists.

## Files
- `.coord/STATUS.md`
- `.coord/status-archive.md`
- `.coord/lessons.md`
- `.coord/lessons-archive.md`
- `.coord/templates/item.md`
- `.coord/tasks/Loop9-loop-slim.md`
- `.coord/reports/Loop9-loop-slim.md`
- `.claude/settings.json`
- `.claude/skills/next-task/SKILL.md`
- `.claude/skills/review-task/SKILL.md`
- `.claude/skills/merge-task/SKILL.md`
- `.claude/skills/merge-task/branches.md`
- `tools/approve.ps1`
- `tools/catch-up.ps1`
- `tools/director-notes.ps1`
- `tools/loop-lint.ps1`
- `docs/loop.md`
- `docs/loop-sessions.md`
- `docs/decisions.md`
- `docs/dev-loop.md`

## Contracts
None (no product code).

## Approve
User, 2026-11-12: "Approve, start it" (on the plan above: umbrella branch, steps a-g, overnight, merge tomorrow after reading the report; plugins and connectors guide paused).

## Build
Overnight build chat, 2026-11-12: steps a-g committed one per step (b58689e, 3fc7eda, 6f87ff4, 8aafbe3, e06c417, 14e5e76, d1f28e2); no Codex needed (docs and scripts). Blocked: `docs/loop.md` edit (ask rule), so the slim version is a proposal file. Report: `.coord/reports/Loop9-loop-slim.md`.

## Check
Offline gate PASS on every commit. STATUS Plan md5 identical; roadmap-data parses 121 rows before and after; STATUS 8 KB target missed (Plan table is 42 KB).

## Hand test
None beyond the user reading `.coord/reports/Loop9-loop-slim.md`.
