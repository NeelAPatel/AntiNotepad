# Loop2-loop-v3-1-loop-md-v3-1: step 3.1h, the rules text says Loop v3.1

Step brief of `Loop2-loop-v3-1` (read the card `.coord/tasks/Loop2-loop-v3-1.md` first, including "Round 2 scope
change"). Steps 3.1a-3.1g are committed; this step writes down what they mean as rules and fixes the open wording.
Docs, plus one small gate fix (After 8).

Tier: Low · docs-tools
Role: worker (gpt-6-luna xhigh)

## After
1. **`docs/loop.md` "Sessions":**
   - The version line: `**Loop version: v3.1** (2026-10-30, Loop2-loop-v3-1: approve and merge preflights, catch-up with
     dev, strict scope, read-only audits, backlog scans with a delta at task time, loop-lint; the dormant v2.2 drop
     absorbed). v3 was Loop1-process-v3 (...keep the existing v3 description...)`. Replace the sentence about v2.2 being
     planned and dormant with "v2.2 was never activated; v3.1 absorbed it (`docs/v2.2/README.md`)."
   - Version rule: "A merged item that changes the phases, the roles or who runs which phase bumps the major version (new
     master at the next handover); new checks, scripts, routes or research steps bump the minor version (v3.1) and need
     no new master."
   - New bullet after the master bullet: "**Loop discussion chat** (optional, at most one): observes and discusses the
     loop beside the master, proposes `idea:` rows for the master to number; it edits no STATUS, merges nothing and runs
     no item unless the user hands it one (Loop2-loop-v3-1 was such a hand-over)."
2. **`docs/loop.md` "Rules":**
   - Rework wording: "Max two rework rounds (`-r2`, `-r3`); a third rework (`-r4`) goes to the user." in place of "A third
     round goes to the user."
   - New bullet: "Who builds: every item, `Loop`, `D` and `T` included, is built by Codex in a builder sub-session; a
     docs-only change of a few lines may be made by the builder session itself and says so in the report."
   - New bullet: "**Loop freeze** (user, 2026-10-30): after v3.1, no new `Loop` items until about five app items have
     merged; after that, loop rules change only when `.coord/lessons.md` shows the same miss twice (the existing
     self-improvement rule)."
3. **`docs/loop.md` "Branches, worktrees and commits":**
   - In "What lands on dev": "STATUS rides whole with whichever item's commit lands next."
   - New bullet: "**No worktree** (only when the user says so, docs or tooling items): the branch is checked out in the
     main checkout (`tools/approve.ps1 -NoWorktree`) and nothing else runs until it merges."
4. **`.claude/skills/review-task/SKILL.md`** step 4: "Max two rework rounds; a third goes to the user." becomes "Max two
   rework rounds (`-r2`, `-r3`); a third rework (`-r4`) goes to the user."
5. **`docs/glossary.md`**, under "Work and the loop", one line each: **Catch-up** (merging dev into an item branch,
   `tools/catch-up.ps1`), **Overlap check** (`tools/files-overlap.ps1`), **Hot file** (shared file that only warns in the
   overlap check), **Preflight** (`tools/approve.ps1`, `tools/merge-preflight.ps1`), **Backlog scan** (`/scan-backlog`),
   **Research file** (`.coord/research/<slug>.md`, stamped with the dev commit), **Scan delta** (`tools/scan-delta.ps1`),
   **Loop-lint** (`tools/loop-lint.ps1`), **Loop freeze**. Under "People and sessions": **Loop discussion chat**.
6. **`docs/v2.2/README.md`**: status becomes **ABSORBED into loop v3.1** (2026-10-30, Loop2-loop-v3-1) with a short table
   mapping each v2.2 step to what v3.1 did (overlap check: 3.1a/3.1b; limit: 3.1b; hand-test queue: one rule in 3.1d;
   catch-up: 3.1a/3.1c; conflict triage: one row in 3.1d; conflict Codex round: dropped; rehearsal: 3.1i). The other
   files in `docs/v2.2/` and `docs/loop-v2.2-plan.md` stay as history, untouched.

7. **`docs/agents.md`** (user comment, 2026-10-30): the per-day numbers are the wrapper's own **run counter**, not a usage
   limit; Codex's real limits are its 5-hour and weekly usage limits. Reword the tier table header and the caps bullet
   to say "run counter" and name the real limits once; behavior unchanged.
8. **`tools/gate.ps1` LoopLint trigger** (3.1g follow-up): it reuses Scope's changed-file set, which drops `.coord/`, so a
   change only to `.coord/templates/*.md` or `.coord/tools/*.sh` does not trigger lint. Build the trigger's own set from
   the same git calls without that filter (a few lines); nothing else in the gate changes.
9. **`tools/loop-lint.allow`**: remove allow lines that no longer match anything after this step's edits (e.g. the
   "planned and **dormant**, stored in" line); keep the rest.

## Files
- `docs/loop.md`
- `docs/glossary.md`
- `.claude/skills/review-task/SKILL.md`
- `docs/v2.2/README.md`
- `docs/agents.md`
- `tools/gate.ps1`
- `tools/loop-lint.allow`

## Done means
- `pwsh -NoProfile -File tools/loop-lint.ps1` passes.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline` passes.
- `docs/loop.md` grows by no more than about 20 lines.

## Constraints
Work in this checkout (`chore/Loop2-loop-v3-1`; no worktree). Never switch branches, merge, commit or push; never edit
`.coord/STATUS.md`, the card, or `product/`. Never launch or kill AntiNotepad. US spelling; match the file's style.

Report: `.coord/reports/Loop2-loop-v3-1-loop-md-v3-1.md` with the item template's report sections.
