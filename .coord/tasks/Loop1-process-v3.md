# Loop1-process-v3: roll the 2026-10-29 process redesign into the loop docs, skills, templates and tools (loop v3)

Item card (`docs/loop.md`). Done by Claude directly (docs and tooling; no product code, no Codex), as C30 was.
First item named in the new scheme; it also writes that scheme down.

Tier: Low · docs-tools (rules: `docs/loop.md`)
Branch: `chore/Loop1-process-v3` in `.claude/worktrees/chore/Loop1-process-v3` (the new layout, used for the first time)

## Request
User, 2026-10-29 (verbatim): "Yes, start rollout", answering "Start the rollout of the new process docs?" after the
process-redesign summary. Every rule below was agreed in that discussion; the record is `.coord/process-redesign.md`.

## Findings
- **Kind and why:** process. The decisions live only in `.coord/process-redesign.md`; every rule file still describes
  loop v2.1 (`feat/` branches, `--no-ff` merges, `S<n>-status` commits, the "Go" phase, `A53`/`C23` counters, sub-session
  steps `scan`/`audit`/`build`/`review`/`test`, flat worktree folders).
- **Now (evidence, master scout):** old terms in `docs/loop.md` (26 hits), `.claude/skills/next-task` (9),
  `merge-task` (6), `review-task` (4), `.coord/templates/item.md` (4), `review.md` (2), `round.md`, `findings.md`,
  `AGENTS.md` (3), `.claude/agents/design-reviewer.md` (2), `.agents/skills/lens-review/SKILL.md`, `docs/agents.md`,
  `docs/dev-loop.md`, `tools/loop-card.ps1`. Scripts:
  - `tools/dev.ps1:235-237` only accepts a dev copy whose checkout sits **directly** in `.claude/worktrees/`; with
    `.claude/worktrees/<prefix>/<ID-slug>` it would refuse to replace a worktree's dev copy.
  - `tools/roadmap.ps1:12` skips any path under `.claude/worktrees/` (still right when nested).
  - `tools/gate.ps1` reads `## Files` and diffs `dev...HEAD`: branch-name independent. Its Format step already runs
    `dotnet format --verify-no-changes` on changed lines, so an `.editorconfig` plugs straight in.
- **Leftovers found:** five unregistered folders in `.claude/worktrees/` (A45, A53, A54, C20, C25; about 930 ignored
  files, git no longer knows them), left from the earlier clean-up.
- **Risk:** a missed old term leaves two rules in force; the skills drive the loop, so a stale skill step is the worst
  case. Mitigation: a final `rg` sweep for the old terms is a Done condition.
- **Source:** master scout (grep across docs, skills, templates, tools; `tools/dev.ps1`, `tools/gate.ps1` read).

## Plan
- **After:**
  1. **Loop v3** (major: phases, roles, branches and commit rules change). `docs/loop.md` rewritten where the rules
     changed, its length kept about the same:
     - Phases `Request → Findings → Plan → Approve → Build → Check → Hand test → Merge`; STATUS state `approved` stays.
     - IDs `A` / `Loop` / `D` / `T`, steps `A1a`, rework `-r<N>`, eras; counters start at 1 in the new era. Retired:
       `C`, `N`, `P`, `S<n>-status`.
     - Branches `<prefix>/<ID-slug>` (`feat/`, `fix/`, `refactor/` for A; `docs/` for D; `chore/` for Loop and T),
       created at Approve, deleted at Merge (local and origin). `main` fast-forwards at a release and gets a semver
       tag (`v0.7.0`, `-rcN`); no PRs for now.
     - Worktrees `.claude/worktrees/<prefix>/<ID-slug>`, created at Approve, removed at Merge; leftover files stop the
       merge and are listed, never force-deleted. At most 3 items past Approve.
     - Merge = squash: one commit per item on dev, `<ID-slug>: <what changed>` (72 characters or fewer) with a 3-6 line
       body (why, what, gate, hand test), carrying the STATUS edits made while it was in flight. STATUS edits with no
       item ride with the next item or one `chore: status` commit at day's end. Items over about 400 lines of product
       code split into lettered steps.
     - Names: master `Master <N> · Loop<ver> · <slug>`; sub-session `<ID> · <step> · <slug>`; roles `scout`,
       `auditor`, `builder`, `reviewer`, `tester` shown in sub-session cards (the old steps map onto them).
     - Status card strip uses `Approve`; widgets section (question cards, next-step box, task list, inline visuals).
  2. **New `docs/coding.md`:** the seven comment rules (with the pipeline line), formatting via `.editorconfig` checked on
     touch, files split by job (four reasons, 600-line review trigger, partial file / class / folder), folders mirror
     namespaces and a split item reorganizes (namespaces, `using`, LAYERS rules, `AGENTS.md` layout).
  3. **New `docs/glossary.md`:** one line per term (era, item, step, round, tier, gate, card, master, sub-session, roles,
     worktree, lab build, dev copy, installed copy, ...). UI region names come later in `D1-ui-region-names`.
  4. **New `.editorconfig`:** conventions the code already follows (no reformat); the gate's Format step reads it.
  5. **Skills, agents, templates, hook:** `/next-task`, `/review-task`, `/merge-task` (squash + STATUS in the commit,
     branch and worktree delete, leftover stop), `/roadmap` if needed, `design-reviewer`, `lens-review`, the four
     templates, `tools/loop-card.ps1` (strip with Approve; role line for sub-sessions).
  6. **Scripts:** `tools/dev.ps1` accepts `.claude/worktrees/<prefix>/<ID-slug>`.
  7. **Pointers:** `AGENTS.md` (rules and branch lines), `docs/agents.md`, `docs/dev-loop.md`, `docs/decisions.md`
     (one row per locked choice). The dormant v2.2 drop gets one "superseded in part by loop v3" note in
     `docs/loop-v2.2-plan.md` and `docs/v2.2/README.md` (its 2-item limit and `--no-ff` merges), nothing else.
- **Must not change:** product code; `tools/gate.ps1` logic; old reports, cards and `.coord/lessons.md` history;
  `docs/wiki/` (a follow-up D item, since it describes v2.1); STATUS plan IDs (renumbered at the era start, after
  the history rewrite).
- **Scope questions (answer at Approve):**
  1. Delete the five leftover folders in `.claude/worktrees/` (stale copies of merged branches)?
  2. Keep the wiki update out of this item as a later D item?
- **Test plan, code:** `pwsh -NoProfile -File tools/gate.ps1 -Task Loop1-process-v3` (build, selftest, Format with the
  new `.editorconfig` must still pass on an untouched tree); `tools/loop-card.ps1` run by hand prints the new reminder;
  `tools/dev.ps1` check: path test of the new worktree rule (a dry function call, no app launch).
- **Test plan, UI:** None (not visible).
- **Gates:** the gate (scope: only the Files below; build and selftest unaffected); `rg` sweep for `feat/`, `--no-ff`,
  `S<n>-status`, `→ Go`, `| Go |`, `Master <N> - loop` in live rule files must come back empty (history files exempt);
  bounds check against `.coord/process-redesign.md`. No Codex review (docs and tooling), no hand test.
- **Needs from you:** answers to the two scope questions; Approve.

## Files
- `AGENTS.md`
- `docs/loop.md`
- `docs/coding.md`
- `docs/glossary.md`
- `docs/agents.md`
- `docs/dev-loop.md`
- `docs/decisions.md`
- `docs/loop-v2.2-plan.md`
- `docs/v2.2/README.md`
- `.editorconfig`
- `.claude/skills/next-task/SKILL.md`
- `.claude/skills/review-task/SKILL.md`
- `.claude/skills/merge-task/SKILL.md`
- `.claude/skills/roadmap/SKILL.md`
- `.claude/agents/design-reviewer.md`
- `.agents/skills/lens-review/SKILL.md`
- `.coord/templates/item.md`
- `.coord/templates/findings.md`
- `.coord/templates/review.md`
- `.coord/templates/round.md`
- `tools/loop-card.ps1`
- `tools/dev.ps1`
- `.coord/process-redesign.md`

## Contracts
None (no product code).

## Approve
User, 2026-10-29 (verbatim): "Approve". The two scope questions were not answered, so the defaults hold: the five
leftover worktree folders are **not** deleted (deleting needs an explicit yes; they are listed in the report), and the
wiki update stays out of this item (a later D item).

## Check
- **Gate (`tools/gate.ps1 -Task Loop1-process-v3`):** `GATE: PASS`; build 0 warnings, `SELFTEST PASS`, `CONTRACTS: 13
  rows, 12 pass, 1 manual`, seam 0 violations, Format PASS (no changed product C#), Scope PASS (16 files, all allowed),
  Size PASS.
- **`.editorconfig`:** full-project `dotnet format --verify-no-changes` gives 339 findings (all WHITESPACE) with and
  without it, so it adds none. A first draft set `dotnet_sort_system_directives_first`, which enabled using-sorting
  (+35 IMPORTS findings); removed.
- **`tools/dev.ps1`:** parses with 0 errors; the allowed-root rule tested on sample paths: main True, nested
  `<prefix>/<ID-slug>` True, flat True, outside False, deeper False.
- **`tools/loop-card.ps1`:** run by hand, prints the Approve strip and the role line.
- **Sweep** for `feat/`, `--no-ff`, `S<n>-status`, `| Go |`, `Master <N> - loop`, `## Go` in the live rule files: only
  intentional hits remain (the prefix list, the "Go before v3" and era-one notes, `## Go` accepted on era-one cards).
- **Bounds check** against `.coord/process-redesign.md`:
  - Every agreed point is in `docs/loop.md`, `docs/coding.md`, `AGENTS.md` or the skills: within plan.
  - Master name written `Master <N> · Loop v<V> · <slug>` (with "v", e.g. `Loop v3`) instead of the agreed
    `Loop<ver>`: justified, so `Loop3` never reads as a `Loop` item ID next to `Loop1-process-v3`; easy to revert.
  - Before Approve the card and audit live untracked in the master checkout, and the auditor's Codex run is read-only
    there (`next-task`, `findings.md`): justified, it follows from "branch created at Approve" and was not discussed
    separately.
  - `docs/agents.md` and `.claude/skills/roadmap/SKILL.md` needed no change: within plan.
  - `.coord/process-redesign.md` was edited on dev after this branch was cut; its "rolled out" title rides with the
    STATUS edits at Merge instead of the branch: justified (avoids a squash conflict).
- **Noticed:** `docs/wiki/` still describes v2.1 (planned D item); STATUS "Working rules" names a loop v2 master (v3 is
  a major bump: the next master handover uses the new name); five leftover worktree folders kept (see Approve).
- **Misses:** None.

## Hand test
None (not visible).
