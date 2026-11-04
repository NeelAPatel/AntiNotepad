# Loop6-loop-v5-wiki: the loop is v5.0; the wiki and its flow diagram show v5

Item card (`docs/loop.md`), fast lane (side task, `docs/loop.md` "Rules"). `Loop` items keep a short card and a branch.

Tier: Low · docs-tools (rules: `docs/loop.md`)
Branch: `chore/Loop6-loop-v5-wiki` in `.claude/worktrees/chore/Loop6-loop-v5-wiki` (created at Approve)

## Request
User, 2026-11-04, in Loop Hub 2, with a screenshot of `docs/wiki/images/loop-v3-1-flow.svg`:
"okay in that case take a worktree off dev, create a subchat, and update the wiki + diagram in this format you just
presented with updated sections/bubbles. at this point on, chats and everyone must know we are on loop version v5.
updates to chat output are x.y.1 increment changes."

## Findings
- **Kind and why:** process and docs. The rules say v3.2 (`docs/loop.md:210`). The user now names the loop **v5**, and
  every chat must know it.
- **Wiki is out of date:**
  - The wiki is still dated v3.1: `docs/wiki/Home.md:8-10` and the flow diagram `docs/wiki/images/loop-v3-1-flow.svg`.
  - It predates Loop4-retro-fixes (Codex roles by job, the fast lane, masters per area, master notes).
  - It also predates T2 to T7 (the `paused` state, preflight accepting card-only commits, Format baseline for moved
    lines) and Loop5-reply-style (the one-line card and the output style).
- **Where versions are named:**
  - `docs/loop.md:210-218` (the version line and the "what bumps a version" rule);
  - `docs/glossary.md:25`;
  - `docs/wiki/Home.md`, `docs/wiki/Loop-History.md` (version table and "What changes a version"),
    `docs/wiki/Who-Does-What.md:30`;
  - the session titles `… · Loop v3 · …`.
  - The `UserPromptSubmit` hook (`tools/loop-card.ps1`) reaches every chat on every turn, so it is where "everyone
    knows" is enforced.
- **What the diagram shows wrong today:**
  - Codex boxes say fixed models ("SOL → LUNA director", "SOL MEDIUM audit"). Now the role is picked by the job:
    quick, terra, worker or director (`docs/agents.md`).
  - One "Main chat" lane. Now there is one master per area.
  - No fast lane for side tasks.
  - No "hand test only if visible" skip.
  - No `paused` state.
  - The hook box still describes the old full card.
- **Risk:**
  - Publishing the wiki to GitHub is outward-facing, so it waits for the user's go after Merge.
  - The worked examples are history and must not be rewritten.
- **Source:** master scout (Loop Hub 2).

## Plan
- **After:**
  1. **Version scheme** in `docs/loop.md` "Sessions":
     - **Loop version: v5.0.0** (user, 2026-11-04; v4 skipped by the user's call), counting from v3.2 plus
       Loop5-reply-style.
     - The rule becomes three parts:
       - **major**: phases, roles or who runs a phase;
       - **minor**: new checks, scripts, routes or research steps;
       - **patch** (`x.y.1`): changes to chat output only (the reply style, the status card, the hook wording).
     - The history line keeps v3.2 and earlier.
     - The master-name example becomes `Master 2 · Loop v5 · <slug>`.
  2. **Every chat sees it:**
     - The hook line in `tools/loop-card.ps1` starts `AntiNotepad loop v5.0.0:`.
     - `docs/glossary.md:25` writes the version as `Loop v5.0.0`, and the new patch level is explained there.
  3. **Wiki pages refreshed to v5.0.0:**
     - Pages: `Home.md`, `Dev-Loop-Overview.md`, `Getting-Started.md`, `Who-Does-What.md`, `Tiers-and-Gates.md`,
       `Parallel-and-Branches.md`, `Triage-and-Lessons.md`, `Where-Files-Live.md`, `Glossary.md`, `Loop-History.md`
       (a v5 row and the three-part rule) and `_Sidebar.md` if links change.
     - Each page matches `docs/loop.md`, `docs/agents.md` and `.claude/output-styles/antinotepad.md` as they are on
       dev. Sources are cited the way the pages already do.
     - Worked examples (`Worked-Example-*.md`), `Backlog-Scans.md` and `Roadmap-Portal.md` are history or still
       current; they change only if a link breaks.
  4. **New diagram `docs/wiki/images/loop-v5-flow.svg`**, in the same format as `loop-v3-1-flow.svg`: phase columns,
     the same role lanes, the same box style (lane tag, title, mono subline), and dashed red return routes. Bubbles
     updated:
     - **Codex boxes** say "CODEX · role by job" (quick / terra / worker / director), with the audit read-only.
     - **The master lane** reads "Masters (one per area)".
     - **A dashed fast-lane route** for side tasks: Plan → builder edits it itself, with no Codex.
     - **The hand test** shows "only if visible".
     - **The hook box** says "one-line card + output style".
     - **The approve script** says "cap, overlap, branch".
     - **Merge** notes "masters tell each other one line".
     - A small `paused` note.
     - `Dev-Loop-Overview.md` points to the new file. The old SVG stays for `Loop-History.md`.
     - The two small SVGs (`loop-v3-1-one-branch.svg`, `loop-v3-1-parallel-worktrees.svg`) get v5 copies only if their
       content changed; otherwise they are linked as they are.
- **Expected time:** about 1.5 hours, request to merge, which is over an hour. A smaller cut is the version lines, hook
  and diagram only, with the text pages later; the user asked for the full refresh.
- **Must not change:** worked examples and other history, rule meaning in `docs/loop.md` beyond the version lines,
  product code.
- **Scope questions:** None. Session titles are renamed by each master, not by this item.
- **Test plan, code:**
  - `tools/loop-card.ps1` prints the v5.0.0 line;
  - `rg -n "v3\.1|v3\.2" docs/wiki` shows only history lines;
  - the new SVG parses as XML;
  - gate loop-lint passes.
- **Test plan, UI:**
  - The builder opens the new SVG in the browser pane at full scale and checks that no text overlaps or is clipped;
  - the user looks at the diagram.
- **Gates:** `tools/gate.ps1 -Task Loop6-loop-v5-wiki`: scope, loop-lint, format.
- **Needs from you:** a look at the new diagram, then a go to republish the GitHub wiki after Merge.

## Files
- `docs/loop.md`
- `docs/glossary.md`
- `tools/loop-card.ps1`
- `docs/wiki/Home.md`
- `docs/wiki/Dev-Loop-Overview.md`
- `docs/wiki/Getting-Started.md`
- `docs/wiki/Who-Does-What.md`
- `docs/wiki/Tiers-and-Gates.md`
- `docs/wiki/Parallel-and-Branches.md`
- `docs/wiki/Triage-and-Lessons.md`
- `docs/wiki/Where-Files-Live.md`
- `docs/wiki/Glossary.md`
- `docs/wiki/Loop-History.md`
- `docs/wiki/_Sidebar.md`
- `docs/wiki/images/loop-v5-flow.svg`
- `docs/wiki/images/loop-v5-one-branch.svg`
- `docs/wiki/images/loop-v5-parallel-worktrees.svg`

## Contracts
None.

## Approve
User, 2026-11-04: "okay in that case take a worktree off dev, create a subchat, and update the wiki + diagram in this
format you just presented with updated sections/bubbles."

## For Codex
Role: terra (`docs/agents.md`), because it is a multi-file docs refresh plus one SVG edit that needs reasoning across
the rules. Product files are not touched. The branch is already checked out: never switch, merge or commit (you cannot
write to `.git`). Read `AGENTS.md`, `docs/loop.md`, `docs/agents.md`, `.claude/output-styles/antinotepad.md`, then the
files above. Line numbers are from the current tree; re-read before editing.

Constraints:
- Build exactly the Plan's "After" for the Files above.
- Start `loop-v5-flow.svg` from a copy of `loop-v3-1-flow.svg`: keep its grid, lanes, colors and box style, and change
  only the bubbles and routes the Plan names.
- Never rewrite worked examples or history rows.
- Never launch or kill AntiNotepad. Never edit `.coord/STATUS.md` or this card.

Done means (offline checks only: never ports, servers, browser or screen):
- `pwsh -NoProfile -File tools/loop-card.ps1` prints a line starting `AntiNotepad loop v5.0.0:`.
- `[xml](Get-Content docs/wiki/images/loop-v5-flow.svg -Raw)` parses.
- `rg -n "v3\.1|v3\.2" docs/wiki docs/loop.md docs/glossary.md` shows only history lines.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop6-loop-v5-wiki -Offline` passes.

Report: `.coord/reports/Loop6-loop-v5-wiki.md`, short: Summary, Change list, Plan deviations, Checks run, Hand checks.

## Check
- Gate: PASS (scope, loop-lint, format, size, seam, selftest).
- Review: Low tier, no Codex review planned; bounds check only.
- **within plan:** version scheme, hook line, glossary, 10 wiki pages, and `loop-v5-flow.svg` match the Plan "After". The sidebar and the two small diagrams were left alone (content unchanged).
- **justified:** builder fixed two small layout issues in the SVG: the third line of the Codex "implementation" box touched the box edge, and the fast-lane label sat on its own route. Both are now clear.
- Diagram viewed in the browser pane (pane scales it down; geometry also checked in the source): no overlaps or clipping, every route has an arrow, new bubbles present (role by job, masters, fast lane, only if visible, one-line card, paused note).
- Noticed: none.
- Misses: none.

## Hand test
User, 2026-11-04, on the v5 flow diagram: "Looks good"
