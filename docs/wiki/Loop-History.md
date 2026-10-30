**In short**

The loop grew from repeatable checks into a process with explicit ownership, approval and evidence. Version 3.1 added scripted checks and research without changing the eight phases. The proposed v2.2 was never activated; its selected safeguards were absorbed into v3.1.

# Loop History

Version meanings come from `docs/loop.md`, "Sessions". The v2.2 disposition comes from `docs/v2.2/README.md`. The table uses subjects from the rewritten dev history; the archive tag preserves the original era-one history, as recorded in `.coord/STATUS.md`, "Where things are".

## Milestones in dev history

| Date | Version or item | Git subject and evidence | Meaning |
|---|---|---|---|
| 2026-10-28 | v1, C13-dev-loop-kit | `25427b3` — C13-dev-loop-kit: build - gate script, CONTRACTS selftest | The start of v1; `docs/loop.md`, Sessions, defines v1 as C13 through C25. The subject records the gate and contracts work. |
| 2026-10-29 | v2, C26-item-card-loop | `1c2f8a2` — C26-item-card-loop: build - one item card per item | One card organizes the phases. `docs/loop.md`, Sessions, attributes v2 to this item. The surviving A55 card shows the older Go phase in practice: `.coord/tasks/A55-split-view-clip.md`. |
| 2026-10-29 | v2.1, C28-loop-cheap-checks | `45304af` — C28-loop-cheap-checks: build - format check, worktree-safe Codex | `docs/loop.md`, Sessions, attributes v2.1 to this item. Current Format and ledger rules are in `docs/coding.md`, Formatting, and `docs/agents.md`, Launch. |
| 2026-10-29 | Dormant v2.2, C30-loop-docs-v22-plan | `7e01b28` — C30-loop-docs-v22-plan: plan - v2.2 plan and wiki pages; `4aad278` — C30-loop-docs-v22-plan: build - loop v2.2 plan, wiki pages | A proposal and dormant drop, never an active loop version. `docs/v2.2/README.md` records what v3.1 adopted and what it dropped. |
| 2026-10-29 | v3, Loop1-process-v3 | `8d18f72` — Loop1-process-v3: loop v3 rules, coding standard, glossary, editorconfig | Approve, new ID prefixes and eras, branch prefixes and nested worktrees, squash merges with STATUS, roles and widgets. Source: `docs/loop.md`, Sessions. |
| 2026-10-30 | v3.1, Loop2-loop-v3-1 | `ac450d2` — 3.1a fix-loop-scripts; `0909d14` — 3.1b approve and merge preflights; `665aa6c` — 3.1c catch-up points | Generalized overlap checks, scripted entry/exit checks and catch-up. Full subjects start "Loop2-loop-v3-1:". Source: `.coord/tasks/Loop2-loop-v3-1.md`, Plan and Check. |
| 2026-10-30 | v3.1, continued | `b000381` — 3.1d conflict route and hand-test turns; `464abc8` — 3.1e scan-backlog and scan-delta; `479afd3` — 3.1f strict scope and read-only audits | Conflict triage, lightweight research, strict scope and a read-only Findings sandbox. Source: `.coord/tasks/Loop2-loop-v3-1.md`, Plan and Check. |
| 2026-10-30 | v3.1, checked and closed | `f9240eb` — 3.1g loop-lint; `68b4d9b` — 3.1h rules text says loop v3.1; `a50c538` — 3.1i rehearsal; `12e9e1c` — close, card Check, STATUS and lessons | The rehearsal records eight passing checks with explicit limits. Source: `.coord/reports/Loop2-loop-v3-1-rehearsal.md`, Summary and Surprises. |

## Why there is no active v2.2 page

`docs/v2.2/README.md` maps the absorbed parts: overlap, cap, hand-test ordering, catch-up, conflict triage and rehearsal. A separate conflict Codex round was dropped. Historical activation material is not an instruction to enable another process; the live rules are `docs/loop.md`.

## What changes a version

A merged change to phases, roles or who runs a phase bumps the major version and needs a new master at the next handover. New checks, scripts, routes or research steps bump the minor version and need no new master. After v3.1, the loop freeze blocks new Loop items until about five app items have merged; later rule changes require a repeated miss in lessons. These are the current rules in `docs/loop.md`, Sessions and Rules, not a forecast of future work.

For evidence of the latest update, read [Worked Example: Loop2](Worked-Example-Loop2). For the older item-card process, read [Worked Example: A55](Worked-Example-A55).

## Sources

- `docs/loop.md` — Sessions, Rules, IDs and numbering, Branches/worktrees/commits.
- `docs/v2.2/README.md` — absorbed status and step mapping.
- `docs/coding.md` — Formatting.
- `docs/agents.md` — Launch and shared run ledger.
- `.coord/STATUS.md` — history rewrite and archive tags, Where things are.
- `.coord/tasks/A55-split-view-clip.md` — surviving loop-v2 card.
- `.coord/tasks/Loop2-loop-v3-1.md` — v3.1 approved steps, outcomes and close.
- `.coord/reports/Loop2-loop-v3-1-rehearsal.md` — rehearsal results and limits.
- Git history: `git log dev --oneline` and `git log --oneline 8d18f72..12e9e1c` — hashes and subjects in the table.
