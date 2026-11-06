# Loop7-top-level-rules: loop v5.1.0, director chats route and step back, need-to-know updates, LP chat names

Item card (`docs/loop.md`), fast lane (side task, `docs/loop.md` "Rules").

Tier: Low · docs-tools (rules: `docs/loop.md`)
Branch: `chore/Loop7-top-level-rules` in `.claude/worktrees/chore/Loop7-top-level-rules` (created at Approve)

## Request
User to Master 4, 2026-11-06: "Loop hub 2 needs the changes for rules" (four rules relayed by Master 4, user quotes
below). User to Loop Hub 2, 2026-11-06: "chat renaming: Loop v5 -> LP<vX.Y.Z> ; add this to changes too. v5.1" and
"More robust Haiku subchat instead, you review and work with it."

1. Top-level chats do not do item work themselves; they ask the user how to route it, default a Sonnet-or-lower
   sub-chat per change that runs Codex workers.
2. After dispatch the top level steps back. User: "Assume that when top level delegates, i am traveling WITH that
   delegation to work together with Sonnet etc. Otherwise expect that it finishes what its working on and updates you
   periodically."
3. Top-level updates are need-to-know. User: "Not everything needs to be told to them as ill ask them to self update and
   catch up, unless something im working on really impacts heavily on their domain. No need for in-the-process updating
   either, finish your task first, and only then decide if others need to be updated or not, or ask me." Replaces
   `docs/loop.md` "Masters of different areas tell each other in one line before a merge when both are active."
4. Live top-level chats: Master 4 (app and design, owns STATUS), Loop Hub 2 (`Loop` and `T`), Roadmap Hub 1 (roadmap);
   earlier masters are closed.
5. Chat names carry the loop version as `LP<vX.Y.Z>`, for example `Master 4 · LPv5.1.0 · design-tokens`, replacing
   `Loop v5`.
6. Loop version becomes **v5.1.0**.

## Findings
- **Kind and why:** process; the user changed how director chats route work, report and are named.
- **Now:**
  - `docs/loop.md:210` says `**Loop version: v5.0.0**`; lines 211-219 hold the history and the bump rule.
  - `docs/loop.md:221-229`: "The picture" names "today the app master and the loop chat"; "A master" ends with the
    tell-each-other line (228-229).
  - `docs/loop.md:234-236`: "Master name" format `Master <N> · Loop v<V> · <slug>`, example `Master 2 · Loop v5 · era-two-start`.
  - `AGENTS.md:26`: "Masters (directors): top-level Claude chats, one per area (app, loop)."
  - `tools/loop-card.ps1:30`: hook line starts `AntiNotepad loop v5.0.0:`.
  - `docs/glossary.md:25`: Loop version written `Loop v5.0.0`.
  - Wiki: `docs/wiki/Home.md:8,10`, `docs/wiki/Glossary.md:7,38`, `docs/wiki/Dev-Loop-Overview.md:9` (image alt text
    only), `docs/wiki/Parallel-and-Branches.md:64` (tell-each-other line), `docs/wiki/Who-Does-What.md` (masters list).
- **Where:** the files under Files.
- **Risk:** stale wording left in one file (loop-lint and a final grep catch it).
- **Source:** master scout.

## Plan
- **After:**
  - `docs/loop.md` "Sessions": version line v5.1.0 (user, 2026-11-06), history gains one clause for v5.0.0
    (2026-11-04, `Loop6-loop-v5-wiki`). Four new bullets after "A master", in plain words with the user's quotes
    (rules 1-4). The tell-each-other sentence is removed. "The picture" names the three live directors. "Master name"
    becomes `<Role> <N> · LP<vX.Y.Z> · <slug>`, example `Master 4 · LPv5.1.0 · design-tokens`; sub-session names
    unchanged.
  - `AGENTS.md` Roles: masters line names the three areas (app and design, loop, roadmap) and says they route item
    work to sub-chats instead of doing it.
  - `tools/loop-card.ps1:30`: `AntiNotepad loop v5.1.0:`; rest of the line unchanged.
  - `docs/glossary.md:25`: written `LPv5.1.0` in chat names, `v5.1.0` elsewhere.
  - Wiki: version text to v5.1.0 (Home, Glossary); Parallel-and-Branches line 64 says need-to-know updates after the
    task instead; Who-Does-What lists the three live directors and the routing rule. The diagram SVG is not redrawn
    (alt text may say v5.1.0).
- **Expected time:** about 30 minutes, request to merge.
- **Must not change:** phases, gates, tiers, branch rules, sub-session names, worked examples, Loop-History pages.
- **Scope questions:** None.
- **Test plan, code:** `rg -n "v5\.0\.0|Loop v5 ·|tell each other" AGENTS.md docs tools/loop-card.ps1` shows only
  history lines.
- **Test plan, UI:** None (not visible).
- **Gates:** `tools/gate.ps1 -Task Loop7-top-level-rules`: scope, loop-lint.
- **Needs from you:** Nothing.

## Files
- `docs/loop.md`
- `AGENTS.md`
- `tools/loop-card.ps1`
- `docs/glossary.md`
- `docs/wiki/Home.md`
- `docs/wiki/Glossary.md`
- `docs/wiki/Dev-Loop-Overview.md`
- `docs/wiki/Parallel-and-Branches.md`
- `docs/wiki/Who-Does-What.md`

## Contracts
None.

## Approve
User, 2026-11-06: rules via Master 4 ("Loop hub 2 needs the changes for rules"), then to Loop Hub 2: "v5.1" and "More
robust Haiku subchat instead, you review and work with it."

## For Codex
Not used: the user chose a Haiku sub-chat that edits the docs itself; Loop Hub 2 reviews its diff.

## Check
- **Gate:** `tools/gate.ps1 -Task Loop7-top-level-rules`: GATE: PASS (scope 9 files allowed, loop-lint PASS 41 files). Report: `.coord/reports/Loop7-top-level-rules.gate.md`.
- **Grep:** `rg -n "v5\.0\.0|Loop v5 ·|tell each other" AGENTS.md docs tools/loop-card.ps1`: only history lines remain (`docs/loop.md:210` history clause; `docs/wiki/Loop-History.md:22`; the SVG alt text in `docs/wiki/images/loop-v5-flow.svg`, not touched per the card).
- **Bounds:** request = plan = done. Nine Files edited, the four `docs/loop.md` changes and the wiki changes as Plan "After" lists them.
- **Misses:** none.
- **Rework r1: line wrap:** rewrapped the "The picture" and "Master name" paragraphs of `docs/loop.md` to about 120 characters; no words changed. Gate re-run: GATE: PASS.

## Hand test
None (not visible).
