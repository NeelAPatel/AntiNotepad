# Loop10-status-split: report (steps a-g)

- **Done:** step a (`Loop10a-roadmap-reads-tiers`), step b (`Loop10b-pending-list`). Gate PASS (offline) after each commit.
- **Stopped before step c** (lab-first). Preview: `.coord/reports/Loop10-status-preview.md`.

## What changed
- `tools/roadmap-data.ps1`: reads STATUS "### Pending work", `.coord/pending.md` (whole file) and `status-archive.md` "## Finished rows". `-Check` sums all three. Each row gets a `source` field.
- `.coord/pending.md` (new): 41 rows (proposed, needs you, planned, paused, deferred) + their group header rows.
- `.coord/status-archive.md`: new "## Finished rows" table, 88 done/dropped rows, old order.
- `.coord/STATUS.md`: Pending work table keeps only Loop10-status-split (state set to building). Section prose untouched.

## Before / after (roadmap data run on this worktree; HEAD~1 export vs now)
| Measure | Before | After |
|---|---|---|
| Plan rows | 130 | 130 |
| done / dropped | 85 / 3 | 85 / 3 |
| needs you / proposed | 25 / 10 | 25 / 10 |
| paused / deferred | 2 / 2 | 2 / 2 |
| planned | 3 | 2 (+1 building = Loop10 row, intended state edit) |
| STATUS.md bytes | 56222 | 13485 |
| pending.md / archive bytes | - / 28788 | 10781 / 61955 |

- The card says "121 rows"; the live count was already 130 (counts are same before and after).
- Rows now appear in tier order (STATUS, pending, archive), so the roadmap's position numbers shift; set of rows is identical.
- Gotcha: `roadmap-data.ps1` resolves the **main checkout** by default; test a worktree with `-Root <worktree>`.

## Blocked
- Nothing. Open for the preview: where "Now" prose and the 4 KB "Backlog (someday)" go (proposed: pending.md).

## Turn 2 (user approved the preview 2026-11-12)
- **c:** STATUS rewritten: 2.8 KB (was 56.2 KB). Kept a `## Plan` / `### Pending work` table with the active row (the roadmap parses it) and bullets under `## Now` (the roadmap reads bullets, so not a table). Old Now entries went to the archive; Backlog (someday) to the end of `pending.md`.
- **d:** `approve.ps1` and `merge-preflight.ps1` print the row-move step; `/next-task`, `/merge-task` and `docs/loop.md` carry one rule line each. No new gate. Both scripts parse clean.
- **e:** item template has a `State:` line; `roadmap-data.ps1` adds `cards[].state` (not yet drawn by the page).
- **f:** mostly already done by Loop9: loop version lives once in `docs/loop.md` (STATUS now links it, glossary only defines it); file inventory is in `docs/loop-sessions.md` and `loop.md` only points to it. I did not rewrite the playbook or agents docs; sessions text is still partly repeated in `docs/playbook.md` (follow-up if wanted).
- **g:** `loop-card`, `session-where`, `status-line`, `roadmap-data -Check` run clean. `merge-preflight` refuses from a worktree by design (not run). loop-lint PASS; WARNs: `loop.md` 8.6 KB, `lessons.md`, this card 5.3 KB. loop-card reads the main checkout's STATUS, so it only shows new layout after merge.
- Roadmap rows stay 130. Not done: Roadmap page does not yet show a card State; no `pending.md` budget rule.
