# C26-item-card-loop: one item card per item, phases Request > Findings > Plan > Go > Build > Check > Hand test > Merge

Item card (`docs/loop.md`). Done by Claude directly (process docs and one hook script, no product code).

Tier: Low
Branch: `feat/C26-item-card-loop` in `.claude/worktrees/C26-item-card-loop`

## Request
User, 2026-10-29, after reviewing how A52-A54, C25 and A45 went:
"It should've been straight forward: here's the task, this is what i want to change ... ai: let me find out where this
change would occur and what is unsatisfactory right now that justifies this task request to exist ... here's what i
found, and here's my recommendation. currently its like this, after the change it should be like this, and this is how
im going to test it ... these are the code review gates that will be run ... Okay plan looks good, go ahead ... performs
the review gates according to plan ... keeps eye out on bugs ... feedback loop of this is what i requested, this is what
i planned and approved, this is what i have done, have i gone out of bounds?"
Then: "first edit the current dev loop files to follow this plan. ... enforce that claude uses a simple visualization
card to show progress and status".

## Findings
- **Kind and why:** process. Sub-session transcripts showed most back-and-forth came before building: no "why/now"
  step for Low items (the audit ran only for Medium/High and after the brief), no current-vs-expected check
  (A53-split-divider-height: dev already did what was asked), no reproduction before building (A54-link-hover-click r1),
  scope questions found at the hand test (A54 r3).
- **Now:** `docs/loop.md` steps Pick > Brief > Audit (Medium/High) > Decide > Implement > Gate > Review > Hand test > Merge;
  brief template has Goal/Files/Contracts only; review checks files, not the approved plan; the status card lived only
  in the master's memory.
- **Agents in use:** `/review-task` loaded in every implementation sub-session (A52, A53, A54, C25, A45); the Codex
  reviewer tier ran for A52 and C25; `design-reviewer` ran for C25 (High); Codex reports record `lens_reviewer` verdicts.
- **Where:** loop doc, templates, the three loop skills, both reviewers, `docs/agents.md`, `AGENTS.md`, a new hook.
- **Risk:** in-flight items (A55-split-view-clip next) use the old names; the gate must still read `## Files` and `Tier:`.

## Plan
- **After:** one card per item that grows Request > Findings > Plan > Go > Check > Hand test; Findings for every tier
  (scout for Low), reproduction required for bugs, plan with Now/After, test plan, gates menu and needs-from-you; Check
  adds a bounds check (requested vs approved vs done); status card defined in `docs/loop.md` and reminded every turn by a
  `UserPromptSubmit` hook listing the items in flight.
- **Must not change:** product code; gate behavior; branch, worktree, numbering and STATUS rules.
- **Test plan, code:** hook prints the reminder and lists only in-flight rows on a sample STATUS; settings JSON parses;
  the gate passes; no stale `brief.md` references.
- **Test plan, UI:** none (no product change). The next `/next-task` run is the live check.
- **Gates:** gate (build, selftest, scope against this list).
- **Needs from you:** go to merge.

## Files
- `docs/loop.md`
- `docs/agents.md`
- `AGENTS.md`
- `.coord/templates/item.md`
- `.coord/templates/brief.md`
- `.coord/templates/round.md`
- `.coord/templates/findings.md`
- `.coord/templates/review.md`
- `.claude/skills/next-task/SKILL.md`
- `.claude/skills/review-task/SKILL.md`
- `.claude/skills/merge-task/SKILL.md`
- `.claude/agents/design-reviewer.md`
- `.agents/skills/lens-review/SKILL.md`
- `.claude/settings.json`
- `tools/loop-card.ps1`
- `.coord/tasks/C26-item-card-loop.md`

## Contracts
None (no product behavior).

## Go
User, 2026-10-29: "first edit the current dev loop files to follow this plan."

## Check
- Gate: `GATE: PASS` (build 0 warnings, `SELFTEST PASS`, CONTRACTS 10 rows / 9 pass / 1 manual, seam 0, scope PASS).
- Hook: prints the card reminder (ASCII, the console code page mangled `→`/`✓`); on a sample STATUS lists only
  `A55-split-view-clip (building, Medium)`, skipping planned rows, groups and release markers. Settings JSON parses.
- Bounds: all within plan. Justified: `.coord/templates/brief.md` renamed to `item.md` (the card replaces the brief);
  `round.md` added for rework rounds, which the old loop wrote from the brief template.
- Noticed: Codex runs inside worktrees write `usage.tsv` and logs to the worktree (`codex-run.sh` ROOT), so caps and logs
  are lost on worktree removal; leftover worktree folders (C15-worktree-branch-cleanup). Proposals only.

## Hand test
None (not visible).
