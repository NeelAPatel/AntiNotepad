# Loop10-status-split: report (steps a-b, turn 1)

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
