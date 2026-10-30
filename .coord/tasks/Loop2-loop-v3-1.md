# Loop2-loop-v3-1: loop v3.1, parallel-safe and scripted (dormant v2.2 absorbed)

Item card (`docs/loop.md`). Run by the loop discussion chat (user, 2026-10-30); Master 2 waits for it before the backlog.

Tier: Low · docs-tools (rules: `docs/loop.md`); script steps add a Codex review (see Gates)
Branch: `chore/Loop2-loop-v3-1`, no worktree (user); one small commit per step; dev fast-forwards (see Approve)

## Request
User, 2026-10-30 (verbatim): "okay lets wake up the dormant v2.2. Assess the full loop end to end and see where the gaps
are and we'll discuss to upgrade v3 to v3.0.1" — then: "assess the loop with the intention of best version yet and come
up with a strong update patch for v3.1. (calling it 3.0.1 was my mistake)" — then: "everything is fine but all of this
needs to be built in a branch. Commits need to be minimal and well scoped." — then: "confirm. but theres no need for
worktrees here."

Decisions (question cards, 2026-10-30): this chat runs the items with Codex builders in sub-sessions; gate scope strict
by default; re-test after a catch-up automatically when incoming dev changes touch the item's Files; CI stays manual;
step commits are kept on dev (fast-forward).

## Findings
- **Kind and why:** process. v3 allows 3 items past Approve but nothing stops them colliding, and its checks are prose.
- **Now (master scout, 2026-10-30):**
  - `tools/v2.2/files-overlap.ps1` scans only `branch refs/heads/feat/*`; `tools/v2.2/catch-up.ps1` refuses unless the branch
    is `feat/<Task>`. v3 uses `feat/ fix/ refactor/ docs/ chore/`: both scripts are broken and nothing noticed.
  - Cap of 3 and "Files not overlapping" (`next-task` step 4, `docs/loop.md`): checked by eye.
  - No catch-up with dev: `/merge-task` step 3 meets dev first at the squash; conflicts only "stop and tell the user".
  - Hand tests "take turns" (one sentence); no queue or handoff.
  - `tools/gate.ps1` Scope is WARN unless `-StrictScope`, and no skill passes it; it also skips `.coord/`.
  - Findings audits run `--sandbox workspace-write` in the shared checkout (`.coord/tools/codex-run.sh`); "read-only" is
    only the brief's instruction.
  - `docs/loop.md`: version rule has major/minor only; "a third round goes to the user" vs rounds `-r2`/`-r3`; who builds
    `Loop`/`D`/`T` items is unwritten (Loop1, C30 were Claude-direct); the loop discussion chat's role is unwritten.
  - `.coord/lessons.md`: Codex review 0 proven findings on 3 Medium items; misses were plan/judgment (fixed by v2) and one
    env (NU1900). So: no new reviewers; scripts instead.
  - Codex project-role loading in nested worktrees: "not yet exercised" since C13.
- **Risk:** the gate, the Codex wrapper and the skills drive every item; a wrong script blocks or falsely passes the loop.
  Mitigation: each script step gets a Codex review, and 3.1i rehearses everything before merge.
- **Source:** master scout; full notes `loop-v3-assessment.md` and `loop-v3.1-proposal.md` in the session notes folder.

## Plan
- **After** (one commit per step, in this order; nearly all touch hot files, so no parallel steps):
  | Step | After |
  |---|---|
  | 3.1a fix-loop-scripts | `files-overlap.ps1`, `catch-up.ps1` accept any `<prefix>/<ID-slug>`, move to `tools/`; hot files (`Contracts.cs`, `SelfTest*.cs`, `docs/loop.md`, the three skills) warn only |
  | 3.1b preflights | `tools/approve.ps1`: cap 3 (from STATUS), overlap, then branch (+ worktree when used), move and commit the card. `tools/merge-preflight.ps1`: card phases filled, no open "ask the user", branch clean, dev an ancestor of HEAD, round's gate PASS, dev index clean (no staged changes, `SQUASH_MSG`, `MERGE_HEAD`). Skills call them; `loop-card.ps1` warns past 3 |
  | 3.1c catch-up-points | The item's session runs `catch-up.ps1` before Check and before Merge, pushes, re-runs the gate; merge, never rebase; re-test when incoming files touch the item's Files |
  | 3.1d conflicts-and-queue (trimmed, round 2) | One `conflict` row in `/review-task` 4a: the item's session resolves it on the branch and re-runs the gate; a semantic clash (the other item changed what this plan relies on) goes back to the master as `plan`. One sentence: one dev copy, so hand tests take turns, oldest accepted first. Lessons header gains `conflict` |
  | 3.1e scan-backlog (new, round 2; replaces hand-test-queue) | `/scan-backlog`: one detached read-only snapshot at a dev commit, one Haiku scout per backlog idea writing `.coord/research/<slug>.md` (kind, Now, Where, tier/size guess, risk, links to other items, `Scanned at: <sha>`), then one priority table with overlaps for the user. `tools/scan-delta.ps1 -Slug`: commits since the scan, changed files in the item's Where list and folders, merged cards to read. `/next-task` Findings starts from the scan plus the delta (full rescan when Where was rewritten) |
  | 3.1f gate-and-audit-hardening | Scope FAIL by default on item branches (dev, `-Ci` exempt; `-LooseScope` override); audits run read-only (`codex-run.sh` sandbox argument, report via `--output-last-message`, `findings.md` updated) |
  | 3.1g loop-lint | `tools/loop-lint.ps1`: stale terms in live loop files, relative links resolve, scripts named by skills exist; gate step when Files touch loop files |
  | 3.1h loop-md-v3.1 | `docs/loop.md` "Loop v3.1"; version rule (major = phases, roles, session rules; minor = checks, scripts, routes); rework wording made exact (`-r2`, `-r3` run, a fourth round asks the user); who builds `Loop`/`D`/`T` items; loop discussion chat line; "STATUS rides whole with the next commit"; loop freeze (no new `Loop` items until a few app items ship; then rules change only on a repeated miss in `lessons.md`); worktree optional for docs/tooling items when the user says so; new scripts in the Files table; glossary terms; `docs/agents.md` sandbox line |
  | 3.1i rehearsal (trimmed, round 2) | One throwaway item end to end off this branch (approve, catch-up, strict scope failing a stray file, merge preflight refusing a behind branch then passing, lint) plus one scan and delta run on two ideas; Codex roles in a nested worktree. Results here; throwaway branches deleted |
  | close | `docs/v2.2/` reduced to an "absorbed into v3.1" note; **visual flow diagram with examples for the user**; once the user is satisfied, wiki pages updated (published on the user's go); STATUS edits; loop freeze rule (3.1h) |
- **Round 2 scope change (user, 2026-10-30):** after "are we at all going overboard" and the advice to trim, the user
  described parallel work as "at most i might develop a couple small features at a time with worktrees, or investigate
  upcoming development with worktrees so i have my 'research' ready for when the actual task arrives", explained batch
  read-only Haiku audits for prioritizing, and on the scan-plus-delta advice said: "sure if you think this benefits the
  dev loop. Once we're done with all of 3.1x, i do expect you to provide a visual diagram of the flow with examples and
  once im satisfied, update the wiki". So 3.1d and 3.1i are trimmed, 3.1e becomes scan-backlog, close adds the diagram.
- **Must not change:** phases, roles, tiers, session names, status card, product code.
- **Scope questions:** answered above (Decisions). Open: none.
- **Test plan, code:** each script step runs its script against real cases on this branch (listed in its commit body);
  3.1i is the end-to-end test. Gate on every step.
- **Test plan, UI:** None (not visible).
- **Gates:** `tools/gate.ps1 -Task Loop2-loop-v3-1` each step (scope against Files below); Codex fresh review
  (`lens-review`) on steps 3.1a, b, c, e, f, g (scripts that drive the loop: false pass, wrong refusal, path handling);
  bounds check at Check; rehearsal before merge.
- **Needs from you:** Approve; the wiki publish at the end.

## Files
- `tools/v2.2/files-overlap.ps1`
- `tools/v2.2/catch-up.ps1`
- `tools/files-overlap.ps1`
- `tools/catch-up.ps1`
- `tools/approve.ps1`
- `tools/merge-preflight.ps1`
- `tools/loop-lint.ps1`
- `tools/loop-lint.allow`
- `tools/scan-delta.ps1`
- `.claude/skills/scan-backlog/SKILL.md`
- `.coord/templates/scan.md`
- `tools/loop-card.ps1`
- `tools/gate.ps1`
- `.coord/tools/codex-run.sh`
- `.claude/skills/next-task/SKILL.md`
- `.claude/skills/review-task/SKILL.md`
- `.claude/skills/merge-task/SKILL.md`
- `.coord/templates/item.md`
- `.coord/templates/findings.md`
- `.coord/templates/round.md`
- `.agents/skills/lens-review/SKILL.md`
- `.coord/lessons.md`
- `docs/loop.md`
- `docs/glossary.md`
- `docs/agents.md`
- `AGENTS.md`
- `docs/v2.2/README.md`
- `docs/v2.2/ACTIVATION.md`
- `docs/v2.2/CHANGELOG.md`
- `docs/v2.2/DISCUSSION.md`
- `docs/loop-v2.2-plan.md`
- `docs/wiki/Home.md`
- `docs/wiki/Dev-Loop-Overview.md`
- `docs/wiki/Who-Does-What.md`
- `docs/wiki/Loop-History.md`
- `docs/wiki/Glossary.md`
- `docs/wiki/Tiers-and-Gates.md`
- `docs/wiki/Where-Files-Live.md`
- `docs/wiki/Triage-and-Lessons.md`
- `docs/wiki/Loop-v2.2-Plan.md`
- `docs/wiki/_Sidebar.md`

## Contracts
None (no product code).

## Approve
User, 2026-10-30 (question card): "Approve (Recommended)". On the checkout: "no parallel chats will be happening. you
have full ownership of repo for this task. no need to message any other chat". So the main checkout switches to
`chore/Loop2-loop-v3-1` until merge (no worktree); STATUS edits stay uncommitted and land in the branch's last commit.

## For Codex
Per step: a step brief `.coord/tasks/Loop2-loop-v3-1-<step>.md` names the step's After, its files (subset of Files above),
and its Done means. Role: director (gpt-6.1-sol medium). Never commit, never switch branches, never edit STATUS or this
card, never touch `product/`. Report `.coord/reports/Loop2-loop-v3-1-<step>.md` in the item template's sections.

## Check
<Filled per step and at the end.>

## Hand test
None (not visible).
