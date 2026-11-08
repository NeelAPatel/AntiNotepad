# L4-lv4-skill-diet: cheaper, more deterministic loop skills via native frontmatter

Item card (`docs/loop.md`). Part of Level 4; shared brief `.coord/research/lv4-shared-brief.md`.

Tier: Medium · process, four skill files, no app code (rules: `docs/loop.md`)
Branch: `chore/L4-lv4-skill-diet` in `.claude/worktrees/chore/L4-lv4-skill-diet` (created at Approve)

## Request
Make the loop skills cheaper and more deterministic using native frontmatter: (a) `disable-model-invocation: true` on
next-task, review-task, merge-task, scan-backlog; keep `/roadmap`; verify behavior for typed `/name` and phrases like
"merge X". (b) `` !`cmd` `` preprocessing for git state where it replaces tool calls; a non-zero exit aborts the skill.
(c) `context: fork` plus `agent:` for review-task (reviewer agent from L3; planned name if L3 has not merged).
(d) Before/after size count of what is loaded every turn. Done when each changed skill runs end to end once on a harmless
item, sizes are measured, and the card lists verified versus assumed. (Loop Hub 2 brief, 2026-11-08.)

## Findings
- **Kind and why:** process; skill descriptions are listed to the model every turn and skills make several tool calls for facts.
- **Now:** 5 skills, 22,216 bytes total; descriptions about 1,600 chars (estimate). One agent: `design-reviewer`. No `!` lines, no fork.
- **Where:** `.claude/skills/{next-task,review-task,merge-task,scan-backlog}/SKILL.md` only.
- **Risk:** a forked review cannot ask the user or see chat history; a failing `!` command aborts the skill.
- **Source:** docs https://code.claude.com/docs/en/skills (read 2026-11-08, partly summarized).
- **Verified vs assumed (docs only, tests due in Check):**
  - `disable-model-invocation` stops auto-invoke: docs, quoted. Typed `/name` still works: implied, to test.
  - Description leaves per-turn context: NOT in docs; measure.
  - `!` runs before the model reads; non-zero aborts: docs. Default shell is bash; `shell: powershell` exists: docs.
  - `context: fork` + custom `agent:` file and what returns: NOT in docs; test.

## Plan
- **After:** four skills typed-only; `shell: powershell` and a short "Live facts" block of `!` lines (branch, `dev..HEAD`
  commit count, card Tier and phase lines) in next-task, review-task, merge-task; review-task's bounds-check step runs
  forked through the reviewer agent (today `design-reviewer`; L3's name if changed), gates, Codex and user questions stay
  inline; descriptions shortened (drop the "Use when user says" phrases, keep the purpose); size table in the report.
- **Expected time:** about 45 minutes.
- **Must not change:** step order and wording of the phases; `/roadmap`; app code, `settings.json`, tools scripts, docs.
- **Scope questions:** None (user chose fork-the-check-only, 2026-11-08).
- **Test plan, code:** None (no product code).
- **Test plan, UI:** None (not visible).
- **Gates:** `tools/gate.ps1 -Task` scope/size warnings only; manual: each `!` command exits 0 on a clean repo and in a
  worktree; frontmatter parses; typed run of each changed skill on a harmless item; natural phrase does not auto-trigger.
- **Needs from you:** hand test at the end: type `/merge-task` style command is not needed; you confirm "merge X" no longer
  starts the skill.

## Files
- `.claude/skills/next-task/SKILL.md`
- `.claude/skills/review-task/SKILL.md`
- `.claude/skills/merge-task/SKILL.md`
- `.claude/skills/scan-backlog/SKILL.md`

## Contracts
None.

## Approve
User, 2026-11-08: "i'll go with your recommendation." (fork only the read-heavy bounds check) and "plan is approved, start
the worktree, branch and build this out. Once built, debrief me and wait for additional corrections or green light."

## For Codex
Role: quick, surgical frontmatter and a small block per file. Worktree `.claude/worktrees/chore/L4-lv4-skill-diet`. The branch
is already checked out: never switch, merge or commit. Read `AGENTS.md`, then the four files.

Constraints:
- Per skill, add frontmatter `disable-model-invocation: true`; add `shell: powershell` to the three that get `!` lines.
- Shorten each `description:` to one sentence of purpose (no "Use when the user says ..." list).
- next-task, review-task, merge-task: after the title add a `## Live facts` block with these lines, each safe (exit 0 always),
  PowerShell syntax, using `$ARGUMENTS` only where the arg is the ID-slug:
  `` !`git branch --show-current` ``, `` !`git rev-list --count dev..HEAD` ``, and for review/merge the card's `Tier:` and
  `Branch:` lines via `Select-String -Path .coord/tasks/$ARGUMENTS.md -Pattern '^(Tier|Branch):' ; exit 0` (guard missing card).
  Then say in one line that steps below need not re-run these.
- review-task: in step 4, say the read-only comparison (report vs plan vs `git diff --stat dev...HEAD`) is delegated to a
  forked pass: add a short "Forked bounds check" note naming agent `design-reviewer`; NOTE: skill-level `context: fork`
  applies to the whole skill, so do NOT add it to the frontmatter; instead record in the report how the fork test went
  (I, the manager, run the fork experiment). Only add the `!` block and the description/flag changes for review-task.
- No other wording changes. Keep US spelling.
- Never touch other files, never edit this card or STATUS.

Done means (offline checks only):
- Four files changed as above; frontmatter valid YAML; the gate passes offline.

Report: `.coord/reports/L4-lv4-skill-diet.md`, short, sections: 1 Summary, 2 Change list, 3 Behavior changes,
4 Out-of-scope files, 5 Plan deviations, 6 Contracts, 7 Lens review (workers: run the checklist yourself and say so),
8 Checks run, 9 Hand checks for the user.

## Check
- Gate: PASS (offline; NU1900 online is env). Bounds: four allowed files only, **within plan**; `!` form changed from Codex's (permission block) **justified**; whole-skill fork dropped **justified** (see report). Real typed runs of the four not yet done: carried to Hand test.
- Noticed: stale "say merge X" prose for L6.
- Misses: Codex used `; exit 0`/`Select-String` in `!` lines; found at manager test; cause plan (docs silent on permissions); should have caught earlier: a tiny test before briefing.

## Hand test
<Filled by the sub-session.>
