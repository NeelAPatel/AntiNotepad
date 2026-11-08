# L3-lv4-agent-roster: loop roles as fixed agent files

Item card (`docs/loop.md`). Codex reads the whole card and follows "For Codex". Codex never edits this file.

Tier: Low · config files and two doc lines, no app code (rules: `docs/loop.md`)
Branch: `chore/L3-lv4-agent-roster` in `.claude/worktrees/chore/L3-lv4-agent-roster` (created at Approve)

## Request
Level 4 item 3 of 7 (brief: `.coord/research/lv4-shared-brief.md`, 2026-11-08): add project agent files under
`.claude/agents/` so each loop role is a fixed config: builder, reviewer, auditor beside `design-reviewer`. Each gets
`model`, `effort`, `tools`/`disallowedTools`, `maxTurns` and a short body, matching the role table. Point `docs/agents.md`
at the files. Prove one loads with the intended model/tools.

## Findings
- **Kind and why:** process; role model/tools live only as prose in `docs/loop.md` (role table, ~line 256).
- **Now:** one agent file (`.claude/agents/design-reviewer.md`: name, description, tools, model, body).
- **Where:** `.claude/agents/*.md`, `docs/agents.md`, `docs/loop.md` (one line under the role table).
- **Risk:** a wrong field name is ignored silently; effort may be overridden by a session or env setting.
- **Source:** live docs https://code.claude.com/docs/en/sub-agents.md (read 2026-11-08 via a helper).
- **Verified (docs):** fields `name`, `description` (required), `tools`, `disallowedTools`, `model` (sonnet|opus|haiku|fable|inherit|full ID),
  `effort` (low|medium|high|xhigh|max), `maxTurns`; camelCase; unknown fields silently ignored.
- **Assumed / to prove:** effective tools and model of a spawned agent (docs: `/tasks` shows model/effort; no tool listing
  command found); `maxTurns` default unstated.

## Plan
- **After:** `builder.md` (sonnet, medium, edit tools, runs Codex; maxTurns 60), `reviewer.md` (sonnet, medium, no
  Edit/Write/NotebookEdit; maxTurns 40), `auditor.md` (haiku, read-only: Read, Grep, Glob, Bash for read-only commands only;
  maxTurns 30). Bodies 5-10 lines, pointing at `docs/loop.md`/`docs/agents.md` for the job. One line each in
  `docs/agents.md` (under "Pick the role by the job") and `docs/loop.md` (under the sub-session role table).
- **Expected time:** about 30 minutes.
- **Must not change:** `design-reviewer.md`, hooks, settings, app code, gate scripts.
- **Scope questions:** None.
- **Test plan, code:** None (config only).
- **Test plan, UI:** None (not visible).
- **Gates:** frontmatter parse check (each file has the fields, valid values); `git diff --stat` shows only Files below;
  proof run: spawn `auditor` on a read-only task, confirm model and that it has no edit tools.
- **Needs from you:** hand test = read the three files and the two doc lines.

## Files
- `.claude/agents/builder.md`
- `.claude/agents/reviewer.md`
- `.claude/agents/auditor.md`
- `docs/agents.md`
- `docs/loop.md`

## Contracts
None.

## Approve
User, 2026-11-08: "plan is approved, start the worktree, branch and build this out. Once built, debrief me and wait for
additional corrections or green light to prep for merging." Scope: both doc lines (user: "I pick both as well"); I tell
Loop Hub 2 about the possible `docs/loop.md` clash with L6.

## For Codex
Role: quick (Luna medium), because three small config files and two doc lines. Product `product/AntiNotepad` is not touched.
The branch is already checked out: never switch, merge or commit (you cannot write to `.git`). Read `AGENTS.md`, then the
files above and `.claude/agents/design-reviewer.md` as the format example.

Constraints:
- Build exactly the Plan's "After" for the Files above. Frontmatter fields exactly: name, description, tools or
  disallowedTools, model, effort, maxTurns. Plain words, US spelling, bodies 5-10 lines.
- Builder body: runs Codex per `docs/agents.md` in a visible sub-session, commits on its branch, never merges or edits STATUS.
- Reviewer body: runs the Check phase (`/review-task`), read-only. Auditor body: read-only Findings scout, reports, never edits.
- Doc lines: `docs/agents.md` "Claude session roles (builder, reviewer, auditor) are config files in `.claude/agents/`; this
  table covers Codex roles only." and `docs/loop.md` "Each role's model, effort and tools are fixed in
  `.claude/agents/<role>.md` (builder, reviewer, auditor); this table says when to use them."
- Never launch or kill AntiNotepad. Never edit `.coord/STATUS.md` or this card. No build needed.

Done means (offline checks only):
- The three files exist with valid frontmatter and the values in the Plan.
- Only the Files above changed.

Report: `.coord/reports/L3-lv4-agent-roster.md`, short, with exactly these sections: 1 Summary, 2 Change list,
3 Behavior changes, 4 Out-of-scope files, 5 Plan deviations, 6 Contracts, 7 Lens review (say "self-checked"), 8 Checks run,
9 Hand checks for the user.

## Check
- Files: 3 agent files + 2 doc lines, only the card's Files changed; within plan.
- Verified: frontmatter fields/values (Codex offline check + my read); proof run `claude -p` from the worktree spawned
  `auditor`: it loaded, reported Haiku 5.5 and tools Read/Grep/Glob/Bash (+ `advisor`), no Edit/Write.
- Round 2 (user asked for cheap-model tests; driver was `claude -p --model haiku`, no Codex needed): `builder` loaded, Sonnet 5.5
  (differs from the Haiku driver, so `model` is applied, not inherited), tools Read/Grep/Glob/Bash/Edit/Write + `advisor`.
  `reviewer` loaded, Sonnet 5.5, no Edit/Write/NotebookEdit; no probe file was created. `maxTurns` works: a temp agent
  (`--agents` JSON, maxTurns 2) was cut off with a partial note after 2 turns.
- Still assumed: all of it is agent self-report except the turn cutoff and the missing file; `effort` has no readout;
  `advisor` is a harness tool (user, 2026-11-08), not config; `claude --agent` does not list project subagents.
- Note: this app session's Agent tool did not see the new files until a fresh `claude` process (agent list loads at start).

## Hand test
User, 2026-11-08: "ready to merge" (after reading the files and the cheap-model test results).
