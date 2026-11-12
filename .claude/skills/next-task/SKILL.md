---
name: next-task
description: Start the AntiNotepad loop by preparing, planning, and approving an item.
disable-model-invocation: true
shell: powershell
allowed-tools: PowerShell(git *)
argument-hint: "[ID-slug]"
---

# /next-task [ID-slug]

Phase detail: `.claude/skills/next-task/phases.md`.

## Live facts

!`git branch --show-current`
!`git rev-list --count dev..HEAD`
!`git status --short`

Branch, commits ahead of dev and changed files are above; steps below need not re-run them.

Phases 1-4 of `docs/loop.md` (Request, Findings, Plan, Approve), run in the master session, then the start of Build.
Every reply opens with the status card (`docs/loop.md`, "Status card"). Keep STATUS's State cell current. Widgets: `.claude/skills/next-task/phases.md`.

## 1. Request
- The item is the user's request, the named item, or the first plan row not done (`.coord/STATUS.md`, .claude/skills/next-task/phases.md, 3. Plan). Stop and
  ask if the next row is a release marker, a `Decision:` row, `proposed`/`needs you`, or lab-first design. An unnumbered
  `idea:`/`step:` row needs the user to confirm its scope first; on that confirmation assign the next free number
  (`docs/loop.md`, "IDs and numbering").
- No branch yet: it is created at Approve. Until then the card and any audit files live untracked in the master
  checkout (on dev); STATUS stays on dev, edited but uncommitted.
- Pick the prefix (`A`, `Loop`, `D`, `T`; the area it affects most) and the branch prefix it will get (`feat/`, `fix/`,
  `refactor/`, `docs/`, `chore/`), and write both on the card's `Tier:`/`Branch:` lines.
- Create `.coord/tasks/<ID-slug>.md` from `.coord/templates/item.md`. Fill `## Request` with the user's
  words verbatim (and screenshot paths). A plan row's Scope cell counts as the request only if the user wrote or confirmed it.
- STATUS: `request`.

## 2. Findings
Answer the card's Findings lines with evidence. Never skip this, whatever the tier.
- **Scan first:** if `.coord/research/<slug>.md` exists (slug = the item name without its leading `<ID>-`), run
  `pwsh -NoProfile -File tools/scan-delta.ps1 -Slug <slug>`. On failure, resolve the missing input or Git error before
  using the scan. Read the scan, the delta's cards and its `WHERE`/`NEAR` files (deleted files: read the relevant diff).
  Write Findings from them, including one line: "Since the scan (<short sha>): <unchanged | changed: ... | new code to use: ...>".
  `RESCAN: yes` requires a fresh scout or audit as below. A Medium/High audit still runs and its brief includes the scan
  and delta as inputs. With no scan, follow the existing scout/audit path below.
- **Low:** your own scout: a few `rg` and targeted reads, at most about ten tool calls. If that is not enough, or the item
  is a bug whose cause you cannot prove, raise it to Medium and use the audit.
- **Medium, High:** fill `.coord/templates/findings.md` into `.coord/tasks/<ID-slug>-audit.md` and start a linked **Haiku**
  sub-session `<ID> · audit · <slug>`, role `auditor` (start_session, user_asked, use_worktree false, wake_when_done). Its
  brief: in the master checkout (absolute paths, product files read-only) run
  `CODEX_SANDBOX=read-only bash .coord/tools/codex-run.sh director <ID-slug>-audit .coord/tasks/<ID-slug>-audit.md` in the
  background, wait for the exit notification, copy the report named on the final `DONE via <role>. Report: <path>` line to
  `.coord/reports/<ID-slug>-audit.md`, and end with the path and its `READY:` line. No `DONE` line (stopped or failed):
  stop and report the wrapper's last lines; never reuse an earlier run's output. Tell the user in one line: folder, brief,
  log, report. Wait for the note; do not poll.
- Stop and ask the user, with the card, when: dev already does what was asked; a bug did not reproduce (`READY: no`); the
  findings show the request means something else.
- STATUS: `findings`.

## 3. Plan
Fill the card's `## Plan`, `## Files` (exact paths; the gate reads them), `## Contracts`, `Tier:` and "For Codex" (role
picked by the job with a one-line reason, `docs/agents.md`; Done means with offline checks only). Write "Expected time"
from similar runs in `.coord/logs/usage.tsv`; over an hour, say so and offer a smaller cut. Side tasks (`T`, tooling
that does not ship) take the fast lane in `docs/loop.md` "Rules" instead of this skill. Pick gates from the menu in .claude/skills/next-task/phases.md: the tier's defaults plus what this item needs, each with what it
catches here. Put every open scope question in "Scope questions" now. Over about 400 lines of product code: split into
lettered steps (.claude/skills/next-task/phases.md, 3. Plan). A touched file over 600 lines: answer the "one job?" check from
`docs/coding.md`. STATUS: `plan ready (<tier>)`.

## 4. Approve
Show the user, under the status card: Now -> After, the test plan (code and UI), the gates and what they catch, Files,
and what you need from them. Ask one question: approve, change, or drop. Record their answer verbatim under
`## Approve`. Change: back to Plan. Drop: STATUS `dropped`, delete the untracked card on their word.
On approval, from the master checkout, run
`pwsh -NoProfile -File tools/approve.ps1 -Task <ID-slug> -Prefix <prefix>` (add `-NoWorktree` only when the user said so).
FAIL: stop and tell the user the reason. PASS: the script created the branch and committed the card and audit files.
STATUS: `approved`.

## 5. Start Build
Start the Build sub-session `<ID> · build · <slug>` (linked, **Sonnet medium**, use_worktree false; names and roles:
`docs/loop.md`) with a brief that says:
- your role is `builder`; show it on the status card's first line; rename yourself `<ID> · review · <slug>` when Check
  starts and `<ID> · test · <slug>` at the hand test;
- work only in `.claude/worktrees/<prefix>/<ID-slug>` with absolute paths; read the card first;
- launch `bash .coord/tools/codex-run.sh director <ID-slug> .coord/tasks/<ID-slug>.md` there, run `/review-task <ID-slug>`
  when it exits, relaunch rework itself, commit each round on the item branch; you start no other chat;
- open every reply with the status card; ask the user, with the card, whatever the bounds check marks "ask the user";
- never touch dev or STATUS; never mint IDs;
- after Check accepts: run `tools/dev.ps1` from the worktree, verify the dev badge, EXE, DATA and log paths, give the user
  the Plan's UI checks, record their result under `## Hand test`, report back.
STATUS: `building`.
