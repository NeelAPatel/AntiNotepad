# Loop8-merge-policy: loop v5.2.0, rebase plus a merge commit per item, tidy commits that each build, worktree by default

Item card (`docs/loop.md`).

Tier: Medium · docs-tools (rules: `docs/loop.md`; scripts that gate merges change)
Branch: `chore/Loop8-merge-policy` in `.claude/worktrees/chore/Loop8-merge-policy` (created at Approve)

## Request
User to Master 4, 2026-11-06 (relayed to Loop Hub 2, user approved: "Yes, send it"):
- "i prefer #2's bubbles ... so that i can atleast trace out which incremental change broke something"
- "i'm more worried about slop commits than high number of comits"
- "let this be the standard as well. worktree preferred for parallelization unless explicitly said not to"

User to Loop Hub 2, 2026-11-06:
- On the commit count: "doesnt need to stay limited to 5, we can go by ear IMO. before merging, lets have a step to
  check the list of commits made on it and see if any of them can be squashed together, such as minute tiny changes, or
  a quick fix, etc." and "I'm okay with meaningful commits not commits for fixing 3 lines with a bit of back and forth".
- On routing: "routing wise use sonnet, haiku subchat should be used when the task is obvious enough that there will be
  no need for me to 'travel' with the delegation."

Scope addition, user to Loop Hub 2, 2026-11-06: "just these loop change version update and propogate the versions to
yourself, master4 and roadmap". Adds a merge-task step and a `docs/loop.md` line: when a merged item bumps the loop
version, the merging director updates STATUS and renames the three live director chats. STATUS itself is edited by Loop
Hub 2 at merge.

Also folded in (Loop Hub 2's STATUS idea rows, same scripts): `idea: preflight-stale-head`, `idea: round-brief-files`.

## Findings
- **Kind and why:** process. Squash merges hide which incremental change broke something, so the user wants per-item
  history kept and slop commits removed.
- **Now:**
  - `docs/decisions.md:10-14` (2026-10-29, Loop v3): "one squashed commit per item on dev ... Rejected: keeping
    `--no-ff` merges plus status commits (bloated history)".
  - `docs/loop.md` "Branches, worktrees and commits" (from line 272):
    - "`dev` is integration: one commit per item";
    - "No worktree (only when the user says so, docs or tooling items)";
    - "What lands on dev: one squashed commit per item", with `git merge --squash`, STATUS riding in it, "or one `chore:
      status` commit at the end of the day";
    - clean-up "`git branch -D`, since a squash does not mark it merged";
    - "Catching up with dev: merge, never rebase (item branches are pushed)". This conflicts with the new rule: rebasing
      a pushed item branch needs `git push --force-with-lease` on that branch only.
  - `docs/loop.md:21`: phase table, row 8 Merge, "squash onto dev".
  - `.claude/skills/merge-task/SKILL.md` steps 3 and 7: squash, and `-D` because of squash.
  - `tools/merge-preflight.ps1`: checks card, catch-up and gate Head. It still stops on "stale Head" after
    T5-review-gate-stale-head (A5 after a STATUS-only dev catch-up, A7 after a card commit following the gate;
    `.coord/lessons.md`).
  - `.coord/templates/round.md`: the round brief's Files list may say "none". A5 r2 and A6 r2 failed gate Scope on it.
  - Squash wording in the wiki and glossary:
    - `docs/glossary.md:53` ("Squash merge");
    - `docs/wiki/Dev-Loop-Overview.md:144`;
    - `docs/wiki/Getting-Started.md:58`;
    - `docs/wiki/Glossary.md:68,72`;
    - `docs/wiki/Parallel-and-Branches.md:54,88,103`;
    - `docs/wiki/Where-Files-Live.md:23`.
  - No "clerk brief" file exists; the clerk follows `docs/loop.md`, so fixing the rule there covers it.
  - Routing (`docs/loop.md` "Sessions", "Routing" bullet, Loop7) names Sonnet-or-lower but not when Haiku fits.
- **Where:** the files under Files.
- **Risk:**
  - A merge script that rewrites history wrongly. Mitigations: rebase only the item branch, never dev; force-push only
    the item branch, with `--force-with-lease`.
  - A per-commit build check that is slow. Mitigation: it runs once at merge, and docs-only commits skip the build.
- **Source:** master scout.

## Plan
- **After:**
  - **Merge (docs, skill):**
    1. **Commit review** in the item chat, before it asks for a merge. List `git log dev..<branch>`. Fold tiny and
       back-and-forth commits into the commit they belong to (`git commit --fixup` plus `git rebase -i --autosquash`
       style; non-interactive `GIT_SEQUENCE_EDITOR=:` since `-i` is not available). Messages say what changed: no
       wip/fix/try-again. Lab variants and debug code are removed. No fixed count: one commit per meaningful change.
    2. Rebase the item branch onto dev and push it with `--force-with-lease` (item branch only).
    3. The master runs `git merge --no-ff --no-commit <branch>`, then adds the STATUS and lessons edits, then commits
       once. The message carries why, what, gate and hand test, as squash messages did.
    4. No separate `chore: status` commits. STATUS rides in the next merge commit.
    5. Delete the branch with `git branch -d`, which now works because the branch is merged.
  - **Catch-up rule:** rebase onto dev, replacing "merge, never rebase".
  - **Every commit builds:** the merge preflight runs `git rebase --exec` with build plus `--selftest` over
    `dev..<branch>` in the item worktree. Commits that touch no product code are skipped. A failure names the commit.
    Keep it light: one build per commit, no other gate steps.
  - **Worktree by default:** every item gets a branch and worktree at Approve. `-NoWorktree` only when the user says
    so, and no longer suggested for docs or tooling items.
  - **Stale Head (idea row):** preflight ignores commits after the gate that touch only the card, reports or STATUS,
    including catch-up merges that bring only those files. With rebase, the gate runs after the final rebase, so
    the gate-Head check compares against the branch tip after tidying.
  - **Round brief Files (idea row):** `.coord/templates/round.md` makes Files required, never "none".
  - **Routing line:** Sonnet when the user will refine the ask in that chat; Haiku when the brief fully specifies the
    job (audits, tests).
  - `docs/decisions.md`: a new 2026-11-06 entry that replaces the squash rule, with the user's quotes. The old entry
    gets a "superseded" note.
  - Loop version **v5.2.0**: version line, hook line in `tools/loop-card.ps1`, `docs/glossary.md`, wiki Home and
    Glossary. Wiki squash wording updated. The wiki is not republished without the user's go.
- **Expected time:** about 1.5 hours, request to merge.
- **Must not change:** phases, gates other than preflight, tiers, the 3-item cap, `main` release rules, sub-session names.
- **Scope questions:** None.
- **Test plan, code:**
  - `tools/merge-preflight.ps1` on a scratch branch with three commits (one tiny fixup) after tidying: PASS.
  - The same with one commit that breaks the build: FAIL naming that commit.
  - A docs-only branch skips the build.
  - Stale Head: a card-only commit after the gate still PASSes.
  - Run the merge itself on the item's own branch at Merge, as the first real use.
- **Test plan, UI:** None (not visible).
- **Gates:** `tools/gate.ps1 -Task Loop8-merge-policy`: scope, loop-lint, format. A Codex review of the preflight change
  (read-only, offline).
- **Needs from you:** Nothing until Merge, where this item is the first one merged the new way.

## Files
- `docs/decisions.md`
- `docs/loop.md`
- `docs/glossary.md`
- `.claude/skills/merge-task/SKILL.md`
- `.claude/skills/review-task/SKILL.md`
- `tools/merge-preflight.ps1`
- `tools/catch-up.ps1`
- `tools/approve.ps1`
- `tools/loop-card.ps1`
- `.coord/templates/round.md`
- `.coord/templates/item.md`
- `docs/wiki/Home.md`
- `docs/wiki/Glossary.md`
- `docs/wiki/Dev-Loop-Overview.md`
- `docs/wiki/Getting-Started.md`
- `docs/wiki/Parallel-and-Branches.md`
- `docs/wiki/Where-Files-Live.md`
- `docs/wiki/Triage-and-Lessons.md`
- `docs/wiki/Who-Does-What.md`

## Contracts
None.

## Approve
User, 2026-11-06: merge policy and worktree standard approved via Master 4 ("Yes, send it"; "let this be the standard as
well"); refinements and routing to Loop Hub 2 ("we can go by ear", commit review step, "routing wise use sonnet").

## For Codex
Script changes (`tools/merge-preflight.ps1`, `tools/catch-up.ps1`, `tools/approve.ps1`) by Codex, role by job (Terra
medium suggested); the Sonnet sub-chat writes the docs and skill text itself or via Codex, its choice.

## Check
Round 1 (Codex Terra medium, scripts; the builder wrote docs and skills). Reviewer: the builder session.
- **Gate:** `tools/gate.ps1 -Task Loop8-merge-policy` PASS (build 0 warnings, selftest, seam, tokens, format, scope,
  loop-lint, size); re-run PASS at the final tip after the tidy-up.
- **Tests on scratch clones (nothing touched dev or main; real origin never pushed to):**
  - preflight docs-only branch: all commits skipped, PASS.
  - product comment commit: built and `--selftest` passed (1 built, others skipped), PASS.
  - commit that breaks the build: FAIL naming that commit.
  - card-only and STATUS-only commits after the gate: PASS; a docs commit after the gate: FAIL stale Head.
  - merge commit on the branch: FAIL.
  - catch-up: UP TO DATE; CLEAN rebase with RETEST line; `-Push` moves only the item branch (`--force-with-lease`);
    CONFLICT exits 2 and leaves the rebase open; a second run refuses.
- **Codex fresh review (read-only, Sol medium):** verdict rework with 4 findings, all fixed and re-tested: `-Push` now
  uses an explicit `refs/heads/<b>:refs/heads/<b>` refspec and `--no-follow-tags` (a configured push destination could
  hit dev); scratch path refused if it is a branch checkout; build inputs outside `product/` (`Directory.*`,
  `global.json`, `nuget.config`) count as build-affecting; a leftover scratch folder prints a WARN.
- **Bounds check (request = plan = done):**
  - Merge rule, commit review, rebase catch-up, `--no-ff` merge, `-d` cleanup, worktree default, routing line, v5.2.0,
    decisions entry, round-brief Files, stale Head, per-commit builds: within plan.
  - Preflight uses a detached scratch worktree and a loop instead of `git rebase --exec` (read-only toward the item
    branch): justified, same result.
  - `tools/approve.ps1` already defaulted to a worktree, so only comments changed: within plan.
  - Two wiki pages (`Triage-and-Lessons`, `Who-Does-What`) added to Files for squash/merge wording: justified.
  - Scope addition from Loop Hub 2 (user: "just these loop change version update and propogate the versions to
    yourself, master4 and roadmap"): merge-task step 8 and a `docs/loop.md` line: within the recorded scope.
  - `docs/wiki` not republished; STATUS not edited: as instructed.
- **Noticed:** (1) `catch-up.ps1` prints "push with -Push" in the CLEAN line even when `-Push` was given. (2) Historic
  wiki pages (Loop-History, worked examples) still say squash; left as history. (3) The first built commit of a branch
  costs a full restore plus build (a few seconds each in tests); a very long branch will take a while.
- **Misses:** found at: scratch test, the per-commit selftest always failed. Cause: `code` (the app is a GUI-subsystem
  exe, so `& $exe` returns no output). Caught by: the builder's scratch test; Codex's offline checks could not run the
  clone. Should have caught: the brief should have said "run the exe as gate.ps1 does". Lesson: copy the gate's process
  runner whenever a script launches the app.

## Hand test
None (not visible). The first merge under the new rule is this item's own.
