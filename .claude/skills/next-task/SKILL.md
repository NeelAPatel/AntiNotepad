---
name: next-task
description: Master-session start of the AntiNotepad loop - take the user's request or the next STATUS plan item, open its item card, get Findings (scout or Codex audit), write the Plan with gates and tests, ask the user for go, then start the Build sub-session. Use when the user asks for a change, says "/next-task", "next", "next item" or "/next-task <ID-slug>".
argument-hint: "[ID-slug]"
---

# /next-task [ID-slug]

Phases 1-4 of `docs/loop.md` (Request, Findings, Plan, Go), run in the master session, then the start of Build. Every
reply opens with the status card (`docs/loop.md`, "Status card"). Keep STATUS's State cell current at every phase.

## 1. Request
- The item is the user's request, the named item, or the first plan row not done (`.coord/STATUS.md`, "Plan"). Stop and
  ask if the next row is a release marker, a `Decision:` row, `proposed`/`needs you`, or lab-first design. An unnumbered
  `idea:`/`step:` row needs the user to confirm its scope first; on that confirmation assign the next free number
  (`docs/loop.md`, "Numbering").
- From the master checkout (on dev): `git worktree add .claude/worktrees/<ID-slug> -b feat/<ID-slug> dev`. Everything for
  the item happens there; STATUS stays on dev, edited but uncommitted.
- Create `.coord/tasks/<ID-slug>.md` in the worktree from `.coord/templates/item.md`. Fill `## Request` with the user's
  words verbatim (and screenshot paths). A plan row's Scope cell counts as the request only if the user wrote or confirmed it.
- STATUS: `request`.

## 2. Findings
Answer the card's Findings lines with evidence. Never skip this, whatever the tier.
- **Low:** your own scout: a few `rg` and targeted reads, at most about ten tool calls. If that is not enough, or the item
  is a bug whose cause you cannot prove, raise it to Medium and use the audit.
- **Medium, High:** fill `.coord/templates/findings.md` into `.coord/tasks/<ID-slug>-audit.md`. Start a linked **Haiku**
  sub-session titled `<ID>-audit-<slug>` (start_session, user_asked, use_worktree false, wake_when_done) whose brief is: in
  `.claude/worktrees/<ID-slug>` (absolute paths only), run
  `bash .coord/tools/codex-run.sh director <ID-slug>-audit .coord/tasks/<ID-slug>-audit.md` in the background, wait for
  the exit notification (no polling), check the report exists, commit the audit brief and report on `feat/<ID-slug>`, and
  end its turn with the report path, its `READY:` line and the run's last lines. Tell the user in one line: working folder,
  brief file, log, report. Wait for the note; do not poll.
- Stop and ask the user, with the card, when: dev already does what was asked; a bug did not reproduce (`READY: no`); the
  findings show the request means something else.
- STATUS: `findings`.

## 3. Plan
Fill the card's `## Plan`, `## Files` (exact paths; the gate reads them), `## Contracts`, `Tier:` and "For Codex" (role,
Done means). Pick gates from the menu in `docs/loop.md`: the tier's defaults plus what this item needs, each with what it
catches here. Put every open scope question in "Scope questions" now. Commit the card on the branch
(`<ID-slug>: card`). STATUS: `plan ready (<tier>)`.

## 4. Go
Show the user, under the status card: Now -> After, the test plan (code and UI), the gates and what they catch, Files,
and what you need from them. Ask one question: go, change, or drop. Record their answer verbatim under `## Go`, commit it
(`<ID-slug>: go`). Change: back to Plan. Drop: STATUS `dropped`, remove the worktree and branch on their word.
STATUS: `approved`.

## 5. Start Build
Start the Build sub-session titled `<ID>-build-<slug>` (linked, **Sonnet medium**, use_worktree false; names and the
other step sessions: `docs/loop.md`, "Sessions") with a brief that says:
- rename yourself `<ID>-review-<slug>` when Check starts and `<ID>-test-<slug>` at the hand test;
- work only in `.claude/worktrees/<ID-slug>` with absolute paths; read the card first;
- launch `bash .coord/tools/codex-run.sh director <ID-slug> .coord/tasks/<ID-slug>.md` there, then run
  `/review-task <ID-slug>` when it exits, relaunch rework itself, commit each round on `feat/<ID-slug>` and push;
- open every reply with the status card; ask the user, with the card, anything the bounds check marks "ask the user";
- never touch dev or STATUS; never mint IDs;
- only after Check accepts, run `tools/dev.ps1` from that worktree, verify the dev badge, EXE, DATA and app log paths,
  then give the user the Plan's UI checks, record their result under `## Hand test`, and report back here.
STATUS: `building`.
