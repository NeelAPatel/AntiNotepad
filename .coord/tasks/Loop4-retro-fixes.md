# Loop4-retro-fixes: a lighter, faster loop from the T1-roadmap-overview retro

Tier: Low · docs-tools
Branch: `chore/Loop4-retro-fixes` in `.claude/worktrees/chore/Loop4-retro-fixes`

## Request
User, 2026-10-31: "i am thinking of letting you take over in a branch and wrap up whatever is left from a dev loop and
docs and roadmap server/file standpoint. You know enough of how i operate to make assumptions now. Surgical edits but
know that if you dont explicitly mention to me in clear concise and well structured output, i will not know about it,
let alone understand its impact."
Then: "Our goal is a reliable dev loop that is lightweight, and smart for the AntiNotepad project development, its tech
stack, and its overall code quality. What i dont need is a complex pipeline of gates and checks where we spend more time
dealing with checks and scripts than actual development."
And: "for side tasks such asworking on roadmap stuff, the pipeline can be completely or partially ignored/skipped as its
not a critical piece of the project".

## Findings
- **Kind and why:** process. T1-roadmap-overview, a side task the user expected to take ~30 min, took 4 h 45 min from
  request to merge.
- **Now:** where the time went:
  - Luna xhigh on every run: ~85 min of Codex thinking, with commands only ~13% of Codex time
    (`.coord/logs/usage.tsv`, T1 logs).
  - Briefs asked Codex for live server checks its sandbox cannot do: "could not bind" in every T1 log.
  - Run caps that should not exist (`.coord/tools/codex-run.sh`).
  - No size call at Plan time.
  - Two rework rounds, one from a wrong rule in the master's brief.
  - Three context resets in the loop chat.
- **Source:** the retro in the loop chat, 2026-10-30/31; `.coord/lessons.md` T1 rows.

## Plan
- **After:**
  - Codex runs are triaged by job (Luna medium / Luna xhigh / Terra medium) with no run caps.
  - Codex gets offline checks only.
  - Status checks need evidence.
  - Plans state the expected time.
  - Side tasks take a fast lane.
  - The org model is written down: directors and sub-chats, merge permissions, overrides logged.
  - Directors keep a gitignored notes file that is reloaded after a compaction.
  - `approve.ps1` moves all planning files.
  - The auditor-commit contradiction is fixed.
  - Proposals go into STATUS at once (Loop3-proposals-in-status).
  - The wiki matches.
- **Must not change:** the phases, the gate script, the product, the roadmap server.
- **Scope questions:** none (the user: "good enough to go"; assumptions listed in the hand-over table).
- **Test plan:**
  - `tools/loop-lint.ps1` passes.
  - The gate passes.
  - `CODEX_DRY=1` shows each role's model.
  - The compact hook prints a director file.
- **Needs from you:** read the change table, then yes or no to merge.

## Files
- `.coord/tools/codex-run.sh`
- `docs/agents.md`
- `docs/loop.md`
- `docs/glossary.md`
- `AGENTS.md`
- `.coord/templates/item.md`
- `.coord/templates/round.md`
- `.gitignore`
- `.claude/settings.json`
- `tools/director-notes.ps1`
- `tools/approve.ps1`
- `tools/loop-lint.allow`
- `.claude/skills/next-task/SKILL.md`
- `.claude/skills/review-task/SKILL.md`
- `.claude/skills/scan-backlog/SKILL.md`
- `.claude/skills/merge-task/SKILL.md`
- `docs/wiki/Who-Does-What.md`
- `docs/wiki/Dev-Loop-Overview.md`
- `docs/wiki/Getting-Started.md`
- `docs/wiki/Where-Files-Live.md`
- `docs/wiki/Tiers-and-Gates.md`
- `docs/wiki/Parallel-and-Branches.md`
- `docs/wiki/Glossary.md`
- `docs/wiki/Home.md`

## Contracts
None (no product code).

## Approve
User, 2026-10-31: "good enough to go." Rule named once: "Codex implements" (`docs/loop.md`, "Who builds"). The loop chat
writes these edits itself at the user's request; logged in `.coord/lessons.md` at Merge.

## Check
- **Gate (offline):** PASS (build 0 warnings, `SELFTEST PASS`, seam, format, scope, loop-lint 39 files, size).
- **`CODEX_DRY=1`:** each role prints its model and effort:
  - `quick`: gpt-6-luna medium
  - `worker`: gpt-6-luna xhigh
  - `terra`: gpt-6.1-terra medium
  - `director`: gpt-6.1-sol medium
  - `backup`: gpt-5.6-luna xhigh
  - `reviewer`: gpt-6.1-sol medium

  `CODEX_EFFORT=high` overrides the effort; an unknown role exits 2.
- **`tools/director-notes.ps1`:**
  - Run from the worktree, it printed a test note from the main checkout's `.coord/directors/`.
  - With no notes, it prints nothing.
- **`tools/approve.ps1` match:** the card, `.mockup.html`, `.sample.json` and step briefs move; `T9-xy`-style names
  do not.
- **Bounds:**
  - Within plan.
  - `docs/wiki/Home.md` was added to Files (one stale model line).
  - `Where-Files-Live`, `Tiers-and-Gates`, `Parallel-and-Branches`, `/scan-backlog` and `loop-lint.allow` needed no
    change.
- **Round 2** (user, 2026-10-31: "udpate them ... fix these and merge"; the starship skip: "yep thats fine"):
  - **`idea: loop-md-contradictions`:**
    - v2 attribution and the gate row already agree today. `docs/loop.md` "Sessions" names C26/C28, matching
      `docs/wiki/Loop-History.md`; the Gates menu matches the steps `tools/gate.ps1` runs.
    - Rework counting is reworded in `docs/loop.md` and `/review-task`: round 1 is the first build, `-r2` and `-r3`
      run without asking, and `-r4` needs the user.
    - The parallel-items line now says that up to 3 items build at once, hand tests take turns, and nothing runs in
      parallel without worktrees.
  - **Starship:**
    - `codex-run.sh` sets `ANTINOTEPAD_CODEX=1` for Codex.
    - The user's profile (outside the repo; backup `Microsoft.PowerShell_profile.ps1.bak-2026-10-31`) skips starship
      when that or `CODEX_SANDBOX_NETWORK_DISABLED` is set.
    - Checked: a normal shell's prompt is starship; with the variable set it is not.
    - Whether Codex passes the variable through is seen on its next run.
- **Misses:** the loop chat's cleanup command deleted the worktree's tracked `.coord/reports/`. It was restored from
  git at once; cause `judgment`.

## Hand test
Not visible (docs and tooling). The user read the change table and replied, 2026-10-31: "fix these and merge".
