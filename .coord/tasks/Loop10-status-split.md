# Loop10-status-split: STATUS holds only active work; the rest moves to tiered files

Item card (`docs/loop.md`). Director (Loop Hub 3) writes Request to Approve; the build chat owns Build, Check, Hand test.

Tier: Medium · docs-tools (loop tooling and docs; no product code)
State: checking
Branch: `chore/Loop10-status-split` in `.claude/worktrees/chore/Loop10-status-split` (created at Approve). Starts after `Loop9-loop-slim` is merged to dev.

## Request
User, 2026-11-12: STATUS must be a high read/write stop, so keep it small.
- Short bullets, no full sentences (ADHD-friendly).
- STATUS = what is being worked or discussed now. Backlog, pending and history live in other files.
- The roadmap = STATUS + those files. Reuse existing files and data; no new copies of the same text.
- User approved the tier map on 2026-11-12 ("this split and plan makes sense now").

## Findings
- **Size:** after `Loop9d` STATUS is 54 KB; the Pending work table is 41 KB (133 rows, 14 over 600 characters). Other sections are small.
- **Duplicates (Loop Hub 3, 2026-11-12):** row Scope repeats the card; "Now" repeats card and report; sessions/roles, file inventory, glossary and loop version each appear in several docs.
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
- **Must not change:** product code, the installed copy, the Plan row format the roadmap parses, Level 4 hooks and agents.
- **Test plan:** gate offline on every commit; `pwsh tools/roadmap-data.ps1` row count and per-state counts equal before and after; `tools/loop-card.ps1`, `approve.ps1 -WhatIf` style dry runs and `merge-preflight` run on a sample item; before and after sizes in the report.
- **Lab-first:** the user previews the new STATUS (the real file, with the live rows) before the rest is built; step c waits for that nod.

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

## Check
Bounds check (build chat, 2026-11-12): **within plan**. Request met: STATUS 56 KB to 2.8 KB, active rows only; pending, archive and cards hold the rest; roadmap reads all tiers (130 rows before and after, same per-state counts); no product code.
- Plan a-g all done. Changed files match `## Files` plus reports (13 files, none outside).
- Gate PASS (offline) after each commit a-g. loop-lint PASS; WARNs: loop.md, lessons.md budgets.
- Deviations: Now stays bullets (roadmap reads bullets); f is small (Loop9 had deduped most).
- Noticed: playbook.md repeats sessions text (follow-up); loop-card reads the main checkout STATUS; roadmap page does not yet draw card State.
- Misses: None.

## Hand test
None: no UI, docs and tooling only.
