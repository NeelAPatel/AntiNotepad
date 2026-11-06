**In short**

The item card explains one change; STATUS explains the whole project. Rules, briefs, reports, and scripts each have a separate home. Masters own their area's project record while item sessions work in isolated checkouts (`docs/loop.md`, “Files,” “Sessions,” and “Branches, worktrees and commits”).

# Where Files Live

## Rules and instructions

Paths here are relative to the repository root. This wiki explains the rules; `docs/loop.md` is authoritative when they differ (`docs/loop.md`, “Files”).

| File | Purpose |
|---|---|
| `AGENTS.md` | Project rules and pointers, loaded through `CLAUDE.md` for Claude and by Codex |
| `docs/loop.md` | Phases, tiers, ownership, sessions, branches, and triggers |
| `docs/agents.md` | Codex models, launch wrapper, sandbox, and reports |
| `docs/dev-loop.md` | Dev copy, data isolation, build and install commands |
| `docs/coding.md` | Comments, formatting, file jobs, and folders, applied on touch |
| `docs/decisions.md` | Locked design choices |
| `docs/glossary.md` | Shared terms |
| `.claude/skills/next-task/SKILL.md` | Request through Approve and Build dispatch |
| `.claude/skills/scan-backlog/SKILL.md` | Snapshot research and proposed backlog order |
| `.claude/skills/review-task/SKILL.md` | Gates, bounds check, triage, and hand-test handover |
| `.claude/skills/merge-task/SKILL.md` | Merge checks, merge commit, lessons, push, and cleanup |
| `.claude/skills/roadmap/SKILL.md` | Open the local roadmap |

The purpose and ownership of these files are defined in `docs/loop.md`, “Files,” and in each skill's numbered steps. See [Getting Started](Getting-Started) for the commands a user types.

## Working records

| Location or example | Purpose and lifetime | Source |
|---|---|---|
| `.coord/STATUS.md` | Master-owned state and plan on dev; uncommitted edits ride the next item commit, or a `chore: status` commit at the end of the day if nothing lands | `docs/loop.md`, Branches |
| `tools/roadmap/index.html`, `tools/roadmap/eras.json` | Live overview page and its era list, served at http://localhost:47811 by `tools/roadmap-serve.ps1` | [Roadmap Portal](Roadmap-Portal) |
| `.coord/templates/item.md` | Blank card: Request, Findings, Plan, Files, Contracts, Approve, For Codex, Check, Hand test | Template headings |
| `.coord/templates/findings.md` | Read-only audit prompt | Template role and final-message sections |
| `.coord/templates/scan.md` | Snapshot scout prompt and research format | Template opening and headings |
| `.coord/templates/review.md` | Fresh review prompt | Inputs and Write sections |
| `.coord/templates/round.md` | Rework prompt | Template |
| `.coord/tasks/Loop2-loop-v3-1.md` | Concrete example of a growing item card | Card sections |
| `.coord/reports/Loop2-loop-v3-1-rehearsal.md` | Concrete rehearsal report | Report checks |
| `.coord/lessons.md` | Master-owned Misses and Scores, copied at Merge | Lessons opening; merge-task step 5 |

Cards use the task-name pattern under .coord/tasks; reports use it under .coord/reports. Generated gate reports end in .gate.md and are not committed. Backlog research lives under .coord/research with the idea slug and a full scanned SHA; it remains untracked until Approve (`docs/loop.md`, “Files,” “Rules,” and “Backlog scans”).

Codex logs and usage.tsv live in the main checkout's .coord/logs, shared across worktrees so removing a worktree does not reset counters. The wrapper's last-message output goes to the launching checkout's .coord/reports, named by task and actual role (`.coord/tools/codex-run.sh`, MAIN, LOGS, LEDGER and attempt).

## Checkouts and app data

Each approved item normally gets a branch and a matching folder under .claude/worktrees, with feat/, fix/, refactor/, docs/, or chore/ prefix. The master stays on dev; item branches never edit STATUS. At Merge, one `--no-ff` merge commit carries the STATUS/lessons changes on top of the item's tidy commits. Cleanup removes the worktree without force and deletes local and remote branches (`docs/loop.md`, “Branches, worktrees and commits”; `.claude/skills/merge-task/SKILL.md`, steps 3–7).

Only a user-authorized docs/tooling item can run without a worktree; that blocks other work until merge. See [Parallel and Branches](Parallel-and-Branches) (`docs/loop.md`, “No worktree”).

The dev launcher builds under build/dev and uses build/testdata for app home, with a separate dev identity and badge. One dev copy runs at a time. A moved notes-folder pointer still needs checking for disposable data before hand testing. The installed copy uses real data and is upgraded only on request (`docs/dev-loop.md`, “Installed and dev copies” and “Dev copy”; `AGENTS.md`, “Rules”).

Master notes live in gitignored `.coord/directors/<slug>.md` and are restored after compaction by `tools/director-notes.ps1` (`docs/loop.md`, "Sessions").

## Sources

- `AGENTS.md`, `CLAUDE.md`, `docs/loop.md` — authority, ownership, and layout.
- `docs/agents.md`, `docs/dev-loop.md`, `docs/coding.md`, `docs/decisions.md`, `docs/glossary.md` — specialist rules.
- `.claude/skills/next-task/SKILL.md`, `.claude/skills/scan-backlog/SKILL.md`, `.claude/skills/review-task/SKILL.md`, `.claude/skills/merge-task/SKILL.md`, `.claude/skills/roadmap/SKILL.md` — command instructions.
- `.coord/templates/item.md`, `.coord/templates/findings.md`, `.coord/templates/scan.md`, `.coord/templates/review.md`, `.coord/templates/round.md` — payloads.
- `.coord/STATUS.md`, `tools/roadmap/index.html`, `.coord/lessons.md`, `.claude/settings.json`, `.coord/tools/codex-run.sh` — shared records and hooks.
- `.coord/tasks/Loop2-loop-v3-1.md`, `.coord/reports/Loop2-loop-v3-1-rehearsal.md` — examples on this branch.
