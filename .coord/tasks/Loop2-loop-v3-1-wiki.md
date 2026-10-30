# Loop2-loop-v3-1-wiki: the wiki explains loop v3.1, with evidence, and teaches how to start using it

Last step of `Loop2-loop-v3-1` (card `.coord/tasks/Loop2-loop-v3-1.md`, Plan row "close": "once the user is satisfied,
wiki pages updated (published on the user's go)"). Loop v3.1 is merged on dev (12e9e1c). This step rewrites
`docs/wiki/` only. Publishing to the GitHub wiki is not part of it.

Tier: Low · docs-tools
Branch: `docs/Loop2-loop-v3-1-wiki` in `.claude/worktrees/docs/Loop2-loop-v3-1-wiki` (made by the manager off dev)
Role: director (gpt-6.1-sol medium); delegate page drafts to workers as you see fit

## Request
User, 2026-10-30 (verbatim), after seeing the flow diagram:
"The diagram alone should convince me that the dev loop is reasonable for both parallel worktrees and branch based work.
The documentation in the final wiki should convince me that each stage in each layer is well thought out and robust,
with exact evidence and file references. If it doesn't exist in codebase, it will not exist in wiki. The documentation
also serves as an instruction guide on how to start using this dev loop instead of traditional chat-and-respond
development. Technical detail is good, but simplifying into normal layman terms, as if you're presenting to junior devs
and stake holders is key."
And on the boxes: "I expect relevant blocks to show me if it's a main chat, sub chat in Claude, which model is being used
in that thread, whether if it's a script file, or an agent file that's being used with handed over context/chat query,
etc. Ex: codex is being called, but what model? What payload? What header (you're a reviewer etc etc)?"

## Inputs
- The rules, which are the only source of truth: `AGENTS.md`, `docs/loop.md`, `docs/agents.md`, `docs/glossary.md`,
  `docs/dev-loop.md`, `docs/coding.md`, `.claude/skills/*/SKILL.md`, `.claude/agents/design-reviewer.md`,
  `.claude/settings.json`, `.codex/agents/lens-reviewer.toml`, `.agents/skills/lens-review/SKILL.md`,
  `.coord/templates/*.md`, `.coord/tools/codex-run.sh`, `tools/*.ps1`, `.coord/lessons.md`.
- Real evidence of a run: `.coord/tasks/Loop2-loop-v3-1*.md` and `.coord/reports/Loop2-loop-v3-1*.md` (steps, reviews,
  the 3.1i rehearsal report), and `git log --oneline 8d18f72..12e9e1c`.
- Diagrams (already in place, do not edit): `docs/wiki/images/loop-v3-1-flow.svg` (swimlanes: phases across; you, main
  chat, Claude sub-chats, Codex runs, scripts down), `docs/wiki/images/loop-v3-1-parallel-worktrees.svg`,
  `docs/wiki/images/loop-v3-1-one-branch.svg`.
- The approved interactive page they come from, for wording and the per-box details (kind, model, started by, given,
  brief opens with, does, leaves, evidence): `build/loop-v3-1-flow-page.html` (its `const N = [...]` list). Use it as a
  starting point, but check every fact against the rule files above; where they differ, the rule files win and you
  list the difference in the report.

## After
Pages in `docs/wiki/` (GitHub wiki Markdown; links between pages as `[Title](Page-Name)`, images as
`![alt](images/<file>.svg)`). Each page opens with **In short** (two to four plain sentences a stakeholder understands),
then the detail. Every claim about how the loop works names its file in backticks, with the skill step, section or
`file:line` where that helps; each page ends with a **Sources** list.
1. `Home.md`: what the loop is and why it beats one long chat (cheapest capable worker per job, scripts check each
   hand-off, you decide at two points: Approve and the hand test); a "Start here" list; where the rules live
   (`docs/loop.md` wins over the wiki); loop v3.1, dated.
2. **New `Getting-Started.md`**: the instruction guide. What you need (Claude desktop app with this repo, Codex CLI, git,
   PowerShell 7, the .NET SDK; name only what the repo's files show it uses); how to open a master chat (the handover
   prompt quoted from `docs/loop.md`, "Sessions"); your first item step by step (say what you want, answer the scope
   questions, approve, wait, hand test in the dev copy, "merge it"); the commands you type (`/next-task`, `/scan-backlog`,
   `/review-task`, `/merge-task`, `/roadmap`) and what each does; what you will see (status card, question cards,
   next-step box, sub-chats in the sidebar); what you never need to do (run Codex by hand, commit, resolve gate output);
   "chat-and-respond vs the loop" in a short side-by-side table.
3. `Dev-Loop-Overview.md`: the flow image, then one section per phase: in plain words, then "Under the hood" (who runs it,
   which model, the command, what it is given, what it leaves), then "Why it is safe" (the check that stops a mistake at
   this phase, with its file).
4. `Who-Does-What.md`: one entry per worker in the diagram (you; the master; scout, auditor, builder/reviewer/tester
   sub-chats; Codex director, workers, `lens_reviewer`, fresh reviewer, backup; `design-reviewer`; each script and the
   hook). For each: kind (main chat, Claude sub-chat, Claude subagent, headless Codex run, Codex subagent, script, hook),
   model and effort, started by (exact command or tool), given (the payload file and how it is passed, e.g. stdin to
   `codex exec`), the brief's opening line quoted exactly, what it may and may not do (sandbox, read-only, never commits),
   what it leaves. Include the Codex failover chain and run counter from `.coord/tools/codex-run.sh`.
5. **New `Parallel-and-Branches.md`**: both git images. Parallel worktrees (cap 3, the overlap check and hot files,
   catch-up with RETEST, conflict and semantic-clash routes, the hand-test queue, the merge preflight refusing a branch
   that is behind dev) and one branch with no worktree (when allowed, what it costs, how Loop2-loop-v3-1 ran). A table:
   which risk each script stops, with the rehearsal check that showed it working.
6. **New `Backlog-Scans.md`**: `/scan-backlog` and `tools/scan-delta.ps1`, when to use them, what a research file
   holds, when a rescan is forced.
7. `Tiers-and-Gates.md`, `Triage-and-Lessons.md`, `Where-Files-Live.md`, `Glossary.md`, `Loop-History.md`: brought to
   v3.1 (strict scope, loop-lint, `Head:`, preflights, catch-up, `conflict` cause, read-only audits, loop freeze, the
   loop discussion chat, version rule). Loop-History gets v3 and v3.1 rows from git subjects.
8. **New `Worked-Example-Loop2.md`**: Loop2-loop-v3-1 as a real worked example: request, findings, plan with steps, each
   step's gate and review outcome (the 7 proven Codex findings and their fixes, from the review reports), the 3.1i
   rehearsal table, the close. Real hashes from git.
9. `Worked-Example-A55.md`: keep only if the files it cites still exist on dev; otherwise cut every claim whose source
   is gone and say at the top that A55 ran on loop v2 in era one (history: tag `archive/full-history-2026-10-29`).
10. `Loop-v2.2-Plan.md`: delete; Loop-History says v2.2 was absorbed into v3.1 (`docs/v2.2/README.md`).
11. `_Sidebar.md`: the new page order, Getting Started second.
12. **Known limits** section in `Home.md` (or its own page linked from Home), stated plainly, each with its source:
    the master's model is not fixed by the docs; a live `/scan-backlog` has not run yet; the rehearsal showed the nested
    worktree sees `.codex/agents` but did not prove `lens_reviewer` spawns there; CI is manual; one dev copy at a time.

## Rules for this step
- **No claim without a file.** If the repo does not show it, it is not in the wiki. No plans, intentions or "will".
- Every backticked repo path must exist on this branch. Check before reporting with:
  `pwsh -NoProfile -Command "Get-ChildItem docs/wiki -Filter *.md | Select-String -Pattern '`([^`]+)`' -AllMatches | ForEach-Object { $_.Matches } | ForEach-Object { $_.Groups[1].Value } | Where-Object { $_ -match '^(\.?[\w.-]+/)+[\w.-]+\.\w+$' } | Sort-Object -Unique | Where-Object { -not (Test-Path $_) }"`
  It must print nothing.
- Plain words first, US spelling, short sentences; explain a term the first time or link `Glossary`. Follow
  `docs/coding.md` for Markdown where it applies. Mermaid only where an image does not exist already.
- Quotes from files are exact; quotes from the user carry the date.

## Files
- `docs/wiki/Home.md`
- `docs/wiki/Getting-Started.md`
- `docs/wiki/Dev-Loop-Overview.md`
- `docs/wiki/Who-Does-What.md`
- `docs/wiki/Parallel-and-Branches.md`
- `docs/wiki/Backlog-Scans.md`
- `docs/wiki/Tiers-and-Gates.md`
- `docs/wiki/Triage-and-Lessons.md`
- `docs/wiki/Where-Files-Live.md`
- `docs/wiki/Glossary.md`
- `docs/wiki/Loop-History.md`
- `docs/wiki/Worked-Example-Loop2.md`
- `docs/wiki/Worked-Example-A55.md`
- `docs/wiki/Loop-v2.2-Plan.md`
- `docs/wiki/_Sidebar.md`
- `docs/wiki/images/loop-v3-1-flow.svg`
- `docs/wiki/images/loop-v3-1-parallel-worktrees.svg`
- `docs/wiki/images/loop-v3-1-one-branch.svg`

## Done means
- All pages above written; the path check prints nothing; `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1-wiki`
  passes (scope against Files).
- Report `.coord/reports/Loop2-loop-v3-1-wiki.md`: pages written, each difference found between the diagram page and the
  rule files, every claim you dropped for lack of a source, and the Known limits list.

## Constraints
Work only in this worktree with absolute paths. Never commit, switch branches, merge or push; never edit anything outside
`docs/wiki/` except the report; never touch `product/`, STATUS or the card. Never launch or kill AntiNotepad.
