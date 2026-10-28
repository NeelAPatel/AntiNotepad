---
name: next-task
description: Master-session start of the AntiNotepad loop - pick the next STATUS plan item (or a named one), write its brief from the template with a tier, launch the read-only Codex audit for Medium/High, then ask the user to decide. Use when the user says "/next-task", "next", "next item" or "/next-task <ID-slug>".
argument-hint: "[ID-slug]"
---

# /next-task [ID-slug]

Steps 1-4 of `docs/loop.md`, run in the master session. After the user says go, start the implementation sub-session (step 5).

## 1. Pick
Read `.coord/STATUS.md` "Plan (user order, ...)". Take the named item, or the first one not done in plan order. Stop and ask
if the next row is a release marker (dev -> main needs the user's go) or needs the user (lab-first design, "Open").
Set its STATUS state to `briefing`.

## 2. Brief
Write `.coord/tasks/<ID-slug>.md` from `.coord/templates/brief.md`. Fill every section; the `## Files` list must be exact
paths (the gate reads it). Pick the tier with the table in `docs/loop.md` (unsure: the higher one). Under "Contracts" name the
existing `CONTRACTS` rows this touches (`product/AntiNotepad/SelfTest/Contracts.cs`) and any new promise the fix creates.
Find files with a quick `rg`, not by reading whole files. STATUS: `briefed (<tier>)`.

## 3. Audit (Medium and High only)
Write `.coord/tasks/<ID-slug>-audit.md`: read-only (no edits outside `.coord/reports/`), confirm or correct the Files list
and tier, list callers and shared state the change can reach, riskiest parts and the hand checks, and a "Recommendation on
handling" (scope, split or not). Report to `.coord/reports/<ID-slug>-audit.md`. Launch in the background:
`bash .coord/tools/codex-run.sh director <ID-slug>-audit .coord/tasks/<ID-slug>-audit.md`
Tell the user in one line: working folder, brief file, log, report. Wait for the exit notification; do not poll.
Fold accepted corrections into the brief. STATUS: `audited`.

## 4. Ask
Give the user a short summary (goal, tier, files, risks, hand checks) and one question: go, change scope, or drop.
Record the answer in STATUS (`approved` / `changed` / `dropped`).

## 5. On "go"
Create the branch `feat/<ID-slug>` off dev (a worktree if another task is in flight). Start the implementation sub-session
(Sonnet manager) with a brief that says: launch `bash .coord/tools/codex-run.sh director <ID-slug> .coord/tasks/<ID-slug>.md`
in that branch's folder, then run `/review-task <ID-slug>` when it exits, relaunch rework itself, give the user the hand
checks, and report back here. STATUS: `implementing`.
