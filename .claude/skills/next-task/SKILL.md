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

Phase 1 (Request) runs in the director chat, which then starts the item chat (section 1a). Phases 2-5 (Findings, Plan, Approve, Build) run in that **item chat**, which the user answers; the director steps back.
Every reply opens with the status card (`docs/loop.md`, "Status card"). Keep the plan row's State cell current. At Approve the row moves from `.coord/pending.md` to STATUS Pending work. Widgets: `.claude/skills/next-task/phases.md`.

The director sets the item chat title to `[Dropped]` or `[Paused]` when it drops or pauses the item; see `docs/loop.md` for the item title rule.

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

## 1a. Start the item chat (director, once)
Write the Request and the pending/STATUS row, then start one linked **Sonnet medium** chat `<ID> · <slug>`
(start_session, user_asked, `use_worktree: false`, cwd = the main checkout `F:\LocalVSProj\Visual Studio Apps\AntiQuickPad`, already trusted, so no "Trust this workspace?" popup; the chat works in its worktree by absolute path and Codex runs from the worktree's own `.coord/tools/codex-run.sh`) and step back. Prompt line 1 is the bare `/pstack:poteto-mode` and line 2 is "Your first action is a Skill tool call with skill `pstack:poteto-mode`. If it is refused, reply with one line and a code block holding `/pstack:poteto-mode <ID-slug>: work the brief above, Findings then Plan, stop at Approve`, then stop." (`tools/poteto-gate.ps1` refuses a start without them). Then: you are the item chat for `<ID-slug>`; read the
card, `AGENTS.md`, `docs/loop.md`; run sections 2-5 below and `/review-task`; the user answers you, not the director; send
the director one line (tier, reason) after Findings; edit only your own row in the main checkout's `.coord/STATUS.md` by
absolute path; never start a chat; never merge. The director asks the user only about merges, releases and cross-item matters.

## 2. Findings (item chat)
Answer the card's Findings lines with evidence. Never skip this, whatever the tier.
- **Scan first:** if `.coord/research/<slug>.md` exists (slug = the item name without its leading `<ID>-`), run
  `pwsh -NoProfile -File tools/scan-delta.ps1 -Slug <slug>`. On failure, resolve the missing input or Git error before
  using the scan. Read the scan, the delta's cards and its `WHERE`/`NEAR` files (deleted files: read the relevant diff).
  Write Findings from them, including one line: "Since the scan (<short sha>): <unchanged | changed: ... | new code to use: ...>".
  `RESCAN: yes` requires a fresh scout or audit as below. A Medium/High audit still runs and its brief includes the scan
  and delta as inputs. With no scan, follow the existing scout/audit path below.
- **Low:** your own scout: a few `rg` and targeted reads, at most about ten tool calls. If that is not enough, or the item
  is a bug whose cause you cannot prove, raise it to Medium and use the audit.
- **Medium, High:** fill `.coord/templates/findings.md` into `.coord/tasks/<ID-slug>-audit.md`, then in the main checkout
  (absolute paths, product files read-only) run
  `CODEX_SANDBOX=read-only bash .coord/tools/codex-run.sh director <ID-slug>-audit .coord/tasks/<ID-slug>-audit.md` as a
  background run in this chat (no audit chat). On exit copy the report named on the final `DONE via <role>. Report: <path>`
  line to `.coord/reports/<ID-slug>-audit.md`. No `DONE` line: stop and report the wrapper's last lines; never reuse an
  earlier run's output. Tell the user in one line: folder, brief, log, report. Do not poll.
- Stop and ask the user, with the card, when: dev already does what was asked; a bug did not reproduce (`READY: no`); the
  findings show the request means something else.
- Tier: set it here and send the director one line (tier, reason). If findings raise it, tell the user at once: what changes, why, the extra cost.
- STATUS: `findings`.

## 3. Plan (item chat)
Fill the card's `## Plan`, `## Files` (exact paths; the gate reads them), `## Contracts`, `Tier:` and "For Codex" (role
picked by the job with a one-line reason, `docs/agents.md`; Done means with offline checks only). Write "Expected time"
from similar runs in `.coord/logs/usage.tsv`; over an hour, say so and offer a smaller cut. Side tasks (`T`, tooling
that does not ship) take the fast lane in `docs/loop.md` "Rules" instead of this skill. Pick gates from the menu in .claude/skills/next-task/phases.md: the tier's defaults plus what this item needs, each with what it
catches here. Put every open scope question in "Scope questions" now. Over about 400 lines of product code: split into
lettered steps (.claude/skills/next-task/phases.md, 3. Plan). A touched file over 600 lines: answer the "one job?" check from
`docs/coding.md`. STATUS: `plan ready (<tier>)`.

## 4. Approve (item chat)
Show the user, under the status card: Now -> After, the test plan (code and UI), the gates and what they catch, Files,
and what you need from them. Ask one question: approve, change, or drop. Record their answer verbatim under
`## Approve`. Change: back to Plan. Drop: STATUS `dropped`, delete the untracked card on their word.
On approval, from the main checkout (this chat's start folder), run
`pwsh -NoProfile -File tools/approve.ps1 -Task <ID-slug> -Prefix <prefix>` (add `-NoWorktree` only when the user said so).
FAIL: stop and tell the user the reason. PASS: the script created the branch and committed the card and audit files.
STATUS: `approved` (your own row only).

## 5. Build (same chat)
Continue in this chat; no new chat. Show the user the status card, role `builder` on line one.
- Work only in `.claude/worktrees/<prefix>/<ID-slug>` with absolute paths.
- Launch `bash .coord/tools/codex-run.sh director <ID-slug> .coord/tasks/<ID-slug>.md` there as a tracked background call you own (`docs/agents.md` "Launch": end the turn, resume on the exit notice, never ask the user to check), run `/review-task <ID-slug>`
  when it exits, relaunch rework yourself, commit each round on the item branch.
- Ask the user, with the card, whatever the bounds check marks "ask the user". Never touch dev or other STATUS rows;
  never mint IDs; never start a chat.
- After Check accepts: run `tools/dev.ps1` from the worktree, verify your slot's dev badge, EXE, DATA and log paths, give the user
  the Plan's UI checks, record their result under `## Hand test`, tell the director it is ready to merge.
STATUS: `building`.
