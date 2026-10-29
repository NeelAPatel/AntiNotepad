---
name: next-task
description: Master-session start of the AntiNotepad loop - pick the next STATUS plan item (or a named one), write its brief from the template with a tier, launch the read-only Codex audit for Medium/High, then ask the user to decide. Use when the user says "/next-task", "next", "next item" or "/next-task <ID-slug>".
argument-hint: "[ID-slug]"
---

# /next-task [ID-slug]

Steps 1-4 of `docs/loop.md`, run in the master session. After the user says go, start the implementation sub-session (step 5).

## 1. Pick
Read `.coord/STATUS.md` "Plan (user order, ...)". Take the named item, or the first one not done in plan order. Stop and ask
if the next row is a release marker (dev -> main needs the user's go), a `Decision:` row, `proposed`/`needs you`, or needs
the user (lab-first design, "Open"). An unnumbered `idea:`/`step:` row needs the user to confirm its scope first; on that
confirmation assign the next free number (`docs/loop.md`, "Numbering") and rename the rows. The row's Scope cell is the brief's scope: anything more is a new plan row with its own
ID (`docs/loop.md`). Set the row's State cell to `briefing`, and keep that cell current at every later step.

## 2. Branch, worktree, brief
From the master checkout (on dev): `git worktree add .claude/worktrees/<ID-slug> -b feat/<ID-slug> dev`. Everything for
this item from here on happens in that worktree ("Branches, worktrees and commits" in `docs/loop.md`); STATUS stays on dev,
edited but uncommitted. Write `.coord/tasks/<ID-slug>.md` in the worktree from `.coord/templates/brief.md`. Fill every section; the `## Files` list must be exact
paths (the gate reads it). Pick the tier with the table in `docs/loop.md` (unsure: the higher one). Under "Contracts" name the
existing `CONTRACTS` rows this touches (`product/AntiNotepad/SelfTest/Contracts.cs`) and any new promise the fix creates.
Find files with a quick `rg`, not by reading whole files. Commit the brief on the branch (`<ID-slug>: brief`). STATUS:
`briefed (<tier>)`.

## 3. Audit (Medium and High only)
Write `.coord/tasks/<ID-slug>-audit.md`: read-only (no edits outside `.coord/reports/`), confirm or correct the Files list
and tier, list callers and shared state the change can reach, riskiest parts and the hand checks, and a "Recommendation on
handling" (scope, split or not). Report to `.coord/reports/<ID-slug>-audit.md`. Start a linked **Haiku** sub-session
(start_session, user_asked, use_worktree false, wake_when_done) whose brief is: in `.claude/worktrees/<ID-slug>` (absolute
paths only), run `bash .coord/tools/codex-run.sh director <ID-slug>-audit .coord/tasks/<ID-slug>-audit.md` in the
background, wait for the exit notification (no polling), check the report exists, commit the audit brief and report on
`feat/<ID-slug>`, and end its turn with the report path and the run's last lines.
Tell the user in one line: working folder, brief file, log, report. Wait for the session's note; do not poll.
Fold accepted corrections into the brief. STATUS: `audited`.

## 4. Ask
Give the user a short summary (goal, tier, files, risks, hand checks) and one question: go, change scope, or drop.
Record the answer in STATUS (`approved` / `changed` / `dropped`).

## 5. On "go"
The branch and worktree already exist (step 2). Start the implementation sub-session (linked, **Sonnet medium**,
use_worktree false) with a brief that says: work only in `.claude/worktrees/<ID-slug>` with absolute paths; launch
`bash .coord/tools/codex-run.sh director <ID-slug> .coord/tasks/<ID-slug>.md` there, then run `/review-task <ID-slug>` when
it exits, relaunch rework itself, commit each round on `feat/<ID-slug>` and push the branch, never touch dev or STATUS,
give the user the hand checks (`tools/dev.ps1` from the worktree), and report back here. STATUS: `implementing`.
