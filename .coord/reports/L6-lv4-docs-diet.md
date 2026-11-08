# L6-lv4-docs-diet

## Summary
Split the loop guide by audience and moved product coding rules into a path-scoped Claude rule. Master startup reading fell from 39,078 B to 8,837 B.

## Change list
- Shortened docs/loop.md to the master core; added phase, session and merge references.
- Moved Findings/Plan/Gates/Widgets to next-task phases; Check/rework/triage to review-task phases; branch/catch-up detail to merge-task branches.
- Moved sessions and file inventory to docs/loop-sessions.md; moved version history to docs/decisions.md.
- Moved coding rules unchanged to .claude/rules/coding.md with paths: product/**; left the Codex pointer in docs/coding.md.
- Trimmed AGENTS.md, updated the L prefix and numbering, version references and loop-lint patterns.

## Behavior changes
None; documentation routing and reading scope only.

## Out-of-scope files
None.

## Plan deviations
None.

## Contracts
None.

## Lens review
None, docs only; checked the mapping myself.

## Old section to new file and heading
| Old section | New file and heading |
|---|---|
| Intro, Phases table, paused | docs/loop.md — intro, Phases |
| 2. Findings, Backlog scans | .claude/skills/next-task/phases.md — Findings, Backlog scans |
| 3. Plan, Gates menu, Widgets | .claude/skills/next-task/phases.md — Plan, Gates menu, Widgets |
| 6. Check (bounds check), Triage, lessons, scores | .claude/skills/review-task/phases.md — Check, Triage |
| Tiers, Status card | docs/loop.md — Tiers, Status card |
| Rules | docs/loop.md — Rules; review-task/phases.md — rework rounds, gate reports |
| IDs and numbering | docs/loop.md — IDs and numbering |
| Sessions | docs/loop-sessions.md — Sessions |
| Branches, worktrees and commits; Catching up with dev | .claude/skills/merge-task/branches.md — Branches, Catching up |
| Triggers in practice | docs/loop.md — Triggers in practice |
| Files table | docs/loop-sessions.md — File inventory |
| Loop version history | docs/decisions.md — Loop version history |

## Bytes
| File / reading set | Before | After |
|---|---:|---:|
| AGENTS.md | 3,665 | 2,333 |
| docs/loop.md | 35,413 | 6,504 |
| docs/coding.md | 5,307 | 195 |
| Master start: AGENTS.md + docs/loop.md | 39,078 | 8,837 |
| Product coding rules at their new path | — | 5,345 |

## Checks run
- pwsh -NoProfile -File tools/loop-lint.ps1 — PASS (47 files)
- pwsh -NoProfile -File tools/gate.ps1 -Task L6-lv4-docs-diet -Offline — PASS (build 0 warnings, selftest, seam, tokens, format, scope, loop-lint, size). NuGet audit was disabled for offline mode.

## Hand checks for the user
None (not visible).
