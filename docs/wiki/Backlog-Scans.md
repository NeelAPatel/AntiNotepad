**In short**

A backlog scan helps you compare ideas before choosing what to build.
Small read-only scouts study the same snapshot, and the master asks you to choose the order.
When an idea becomes an item, a script checks what changed since its scan so old research does not silently become
current evidence (`.claude/skills/scan-backlog/SKILL.md`; `docs/loop.md`, "Backlog scans").

# Backlog scans

## When to use a scan

Type `/scan-backlog [slug ...]` in the master chat to research named ideas, or omit the slugs to select eligible
`idea:` rows marked `needs you` or `planned`. A batch above about ten ideas needs your selection first. Scanning assigns
no IDs and makes no plan or fix (`.claude/skills/scan-backlog/SKILL.md`, steps 1–2).

The master makes one detached worktree snapshot of dev, stamps its full commit SHA, then starts one linked Haiku
`scout` sub-chat per idea. All scouts read that same absolute snapshot path; `use_worktree: false` avoids another
implicit worktree. Each receives a filled `.coord/templates/scan.md`, its Plan row, screenshot references, SHA, and
absolute report path (`.claude/skills/scan-backlog/SKILL.md`, steps 2–3).

## What the research file holds

The only allowed scout write is the idea's research report in the main checkout. The report format in
`.coord/templates/scan.md` contains:

| Field | Meaning |
|---|---|
| `Scanned at: <full sha>` | Exact snapshot, so later comparison has a base |
| Kind | Bug, feature, polish, or process, with why |
| Now | Current behavior with file:line evidence |
| Where | Exact backticked file paths, with symbols/callers/shared state |
| Size and tier | A guess with a tier reason from the rules |
| Risk | Uncertainty and other behavior the idea could affect |
| Links | Overlaps and dependencies with other backlog ideas |
| Questions | Missing screenshots or user decisions; no plan or fix |

The master verifies reports and SHAs, shows a comparison table, and proposes an order. STATUS changes only on your word.
Reports remain untracked until their item is approved. Once every scout stops reading, the master removes the snapshot
without force; it reports failures and preserves completed research (`.claude/skills/scan-backlog/SKILL.md`, steps 4–5).

## At task time: compare before trusting

If research exists for the item slug, `/next-task` step 2 runs:

```powershell
pwsh -NoProfile -File tools/scan-delta.ps1 -Slug <slug>
```

`tools/scan-delta.ps1` reads the scan's full SHA and exact Where files, then compares first-parent history against local
dev by default. Its output includes:

- `COMMITS`: subjects since the scan.
- `WHERE`: changed files named in the scan.
- `NEAR`: changed files in the same folders as Where files.
- `CARDS`: existing item cards identified from commit subjects.
- `DELTA` and `RESCAN`: a summary and whether a fresh scout/audit is required.

The script counts touched paths even when a later commit undoes the change. Renames count as delete plus add.
**More than half** the Where files changed, or **any** Where file deleted, prints `RESCAN: yes`.
Missing input, a malformed SHA, or Git failure returns a failure instead of a partial comparison
(`tools/scan-delta.ps1`, validation, history walk, and final summary).

Findings reads the scan, the named cards, and the Where/Near files; for deleted files it reads the diff. It records a
"Since the scan" line on the card. `RESCAN: yes` requires fresh research. A Medium/High item still gets its normal
Codex audit, with scan and delta included in the brief (`.claude/skills/next-task/SKILL.md`, step 2).

## Evidence and limit

Rehearsal check 6 changed one of two Where files. It printed `WHERE`, a same-folder `NEAR`, a related card,
`DELTA: where (1 of 2 Where files changed)`, and `RESCAN: no`. That exercised the script, not a live scout batch:
no live `/scan-backlog` with Haiku ran in the rehearsal
(`.coord/reports/Loop2-loop-v3-1-rehearsal.md`, check 6 and Summary).

## Sources

- `.claude/skills/scan-backlog/SKILL.md`: steps 1–5.
- `.coord/templates/scan.md`: scout payload and research format.
- `tools/scan-delta.ps1`: validation, first-parent history, Where/Near classification, rescan threshold.
- `.claude/skills/next-task/SKILL.md`: step 2, scan-first Findings.
- `docs/loop.md`: Findings, Backlog scans.
- `.coord/reports/Loop2-loop-v3-1-rehearsal.md`: check 6 and live-scan limit.
