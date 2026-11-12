# Loop10-status-split: STATUS holds only active work; the rest moves to tiered files

Item card (`docs/loop.md`). Director (Loop Hub 3) writes Request to Approve; the build chat owns Build, Check, Hand test.

Tier: Medium · docs-tools (loop tooling and docs; no product code)
Branch: `chore/Loop10-status-split` in `.claude/worktrees/chore/Loop10-status-split` (created at Approve). Starts after `Loop9-loop-slim` is merged to dev.

## Request
User, 2026-11-12: STATUS must be a high read/write stop, so keep it small.
- Short bullets, no full sentences (ADHD-friendly).
- STATUS = what is being worked or discussed now. Backlog, pending and history live in other files.
- The roadmap = STATUS + those files. Reuse existing files and data; no new copies of the same text.
- User approved the tier map on 2026-11-12 ("this split and plan makes sense now").

## Findings
- **Size:** after `Loop9d` STATUS is 54 KB; the Pending work table is 41 KB (133 rows, 14 over 600 characters). Other sections are small.
- **Duplicates found (Loop Hub 3, 2026-11-12):**
  - Row Scope text repeats the item card (142 cards in `.coord/tasks/`).
  - "Now" repeats card and report text.
  - Sessions and roles text is in `docs/loop.md`, `docs/loop-sessions.md`, `docs/playbook.md`, `docs/agents.md`, `AGENTS.md`, wiki Who-Does-What.
  - File inventory is in `docs/loop-sessions.md`, `docs/loop.md` "Where the detail lives", wiki Where-Files-Live.
  - Glossary is in `docs/glossary.md`, wiki Glossary and `docs/playbook.md`.
  - Loop version is stated in `docs/loop.md` twice, `docs/decisions.md` and STATUS.
- **Roadmap today:** `tools/roadmap-data.ps1` reads only STATUS "### Pending work", a 5-column table (Item, Scope, Tier, Effort, State). Other readers of STATUS: `approve`, `catch-up`, `merge-preflight`, `loop-card`, `session-where`, `status-line`, four skills, `builder.md`.
- **Reuse:** `.coord/status-archive.md` (exists from Loop9d) holds finished rows. The item card is the single home for scope.

## Plan
- **a-roadmap-reads-tiers:** `tools/roadmap-data.ps1` reads STATUS (active rows), the pending list and `status-archive.md`; the page still shows every row (121 before = 121 after). Do this first and test it before moving any rows.
- **b-pending-list:** pending, planned, someday and deferred rows move out of STATUS into one file (`.coord/pending.md`, same 5-column table, one line per item). A row without a card keeps its scope in that line.
- **c-status-shape:** STATUS = working rules, where things are, **active** rows (approved, building, checking, hand test, discussed now), open questions for the user. One line per row: ID-slug, goal in about 8 words, tier, state, link to the card. Target 7 KB; a bullet style, no full sentences. "Now" = one line per active item.
- **d-move-rules:** Approve moves a row pending → STATUS. Merge moves it STATUS → archive. Update `approve.ps1`, `merge-preflight.ps1`, `/next-task`, `/merge-task` and `docs/loop.md` (one rule line each). No new gate.
- **e-card-state:** the card header gets a `State:` line the roadmap can read for an item with a card (one source of truth).
- **f-dedupe-docs:** one home each. Sessions and roles in `docs/loop-sessions.md`; the other docs link to it. One file inventory. Glossary only in `docs/glossary.md`. One "Loop version" line in `docs/loop.md`. Wiki is not touched (not loaded by chats).
- **g-readers-checked:** every script and skill that reads STATUS still works (listed in Findings); `tools/loop-lint.ps1` budget WARN for the new `pending.md` is optional.
- **Expected time:** about 2-3 hours of build with gates. Over an hour, so the cut line is **a-d** (roadmap, pending file, STATUS shape, move rules): they carry about 90% of the saving. Steps e-g can be a follow-up.
- **Must not change:** product code, the installed copy, the Plan row format the roadmap parses, Level 4 hooks and agents.
- **Test plan:** gate offline on every commit; `pwsh tools/roadmap-data.ps1` row count and per-state counts equal before and after; `tools/loop-card.ps1`, `approve.ps1 -WhatIf` style dry runs and `merge-preflight` run on a sample item; before and after sizes in the report.
- **Lab-first:** the user previews the new STATUS (the real file, with the live rows) before the rest is built; step c waits for that nod.
- **Overnight rules (if run unattended):** a permission prompt is never worked around; three failed tries per step then stop; no force-push.

## Files
- `.coord/STATUS.md`
- `.coord/pending.md`
- `.coord/status-archive.md`
- `.coord/templates/item.md`
- `.coord/tasks/Loop10-status-split.md`
- `.coord/reports/Loop10-status-split.md`
- `.claude/skills/next-task/SKILL.md`
- `.claude/skills/merge-task/SKILL.md`
- `.claude/agents/builder.md`
- `tools/roadmap-data.ps1`
- `tools/roadmap-serve.ps1`
- `tools/approve.ps1`
- `tools/merge-preflight.ps1`
- `tools/loop-card.ps1`
- `tools/session-where.ps1`
- `tools/status-line.ps1`
- `tools/catch-up.ps1`
- `docs/loop.md`
- `docs/loop-sessions.md`
- `docs/playbook.md`
- `docs/agents.md`
- `docs/glossary.md`

## Contracts
None (no product code).

## Approve
User, 2026-11-12: "Approve Loop10" (on the plan above, after Loop9 merged; tier map approved earlier). Lab-first: build a-b, then stop with a preview of the new STATUS for the user before step c.
