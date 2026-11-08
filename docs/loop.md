# Dev loop (per item)

Who does what, what triggers each phase, and what it leaves behind. Start/resume: .coord/STATUS.md; roadmap: /roadmap (http://localhost:47811). Routing: docs/agents.md; terms: docs/glossary.md; coding: docs/coding.md.

Every item has one **item card**, `.coord/tasks/<ID-slug>.md` (from `.coord/templates/item.md`), that grows phase by
phase: the request, what was found, the approved plan, and the check of what was done against both. Anyone can
open the card and see why the item exists, what was agreed and where it stands.

**Loop version: v5.3.0** (LP<vX.Y.Z> in master names). Phase, role or ownership changes bump major; new checks, scripts, routes or research steps bump minor; chat output only bumps patch (x.y.1). On a bump, the master updates STATUS and all live master names.

## Phases
`Request → Findings → Plan → Approve → Build → Check → Hand test → Merge`

| # | Phase | Who | Leaves on the card | STATUS state |
|---|---|---|---|---|
| 1 | **Request**: the user's words, verbatim, and what they want changed | master | `## Request` | `request` |
| 2 | **Findings**: why the item exists (bug, feature, polish, process), where the change lands, what it does **now** with evidence; bugs need a reproduction | `auditor` sub-session running a Codex audit (Medium, High); the master's own short scout (Low) | `## Findings`, audit report | `findings` |
| 3 | **Plan**: **after** state against "now", scope questions, test plan (code and UI), the gates this item runs and what each catches, what is needed from the user, Files, tier | master | `## Plan` | `plan ready` |
| 4 | **Approve**: the user approves, changes or drops the plan; on approval the branch and worktree are created | user | `## Approve` with the user's words | `approved` / `dropped` |
| 5 | **Build**: Codex implements; the director spawns `lens_reviewer` before reporting | `builder` sub-session runs Codex | code on `<prefix>/<ID-slug>`, report | `building` / `rework rN` |
| 6 | **Check**: the planned gates, then the bounds check (request vs plan vs done) | `reviewer` (`/review-task`; usually the builder session renamed) | `## Check` | `checking` / `accepted` / `ask-user` |
| 7 | **Hand test**: the sub-session opens its build; the user runs the plan's UI checks | user, with the `tester` | `## Hand test` with the user's words | `hand test` / `hand-tested` |
| 8 | **Merge**: commit review, rebase onto dev, `--no-ff` merge commit with the item's STATUS edits, gate on dev, push, delete the branch, remove the worktree | master (`/merge-task`) | the item's commits and its merge commit on dev | `done` |

"Approve" was "Go" before loop v3; old cards keep `## Go`.

`paused` (any state after Approve): the user set a started item aside. Its branch, worktree and card stay, it does not count against the 3-item cap, and resuming sets the earlier state back.

## Tiers
Set at Plan; findings may raise it. When unsure, pick the higher tier.
| Tier | Touches | Findings | Default gates |
|---|---|---|---|
| Low | UI text, theme tokens, docs/tools, Calc rules with selftests, pure moves with LAYERS rows | master scout | gate; hand test if visible |
| Medium | Editor features (input, cursor, selection, rendering), Settings sections, a new setting or shared state, any bug without a proven cause | Codex audit | gate + Codex review; hand test if visible |
| High | `Store`/`NotesSession`/notes folder, startup and quit, `Platform/` window styles and taskbar, timers and deferred work, save paths, memory | Codex audit | gate + Codex review + design review; hand test always |

## Status card (every reply about an item)
Every Claude reply about project work, master and sub-sessions alike, opens with a card. The default is one line:
```
**>> <ID-slug>** · ● <phase> · <what changed>
```
The **full card** is required only when the phase changes, at the first reply in a session, or when the reply carries a
decision block. Detail goes below the card.
```
**>> <ID-slug>** · <Tier> tier · ● <phase>
✓ Request | ✓ Findings | ● Plan | ○ Approve | ○ Build | ○ Check | ○ Hand test | ○ Merge
**Tier:** <Tier> · <reason slug>
**Goal:** <one line>
**Now:** <what is happening, one or two lines>

---

**Waiting on you:** <the decision block, or "nothing">
```
- The decision block (one decision per reply: question, background, decisions already taken, options table) and the
  body rules (plain words, glosses, tables) are in the output style `.claude/output-styles/antinotepad.md`.
- `✓` done, `●` current, `○` not reached, `–` skipped by tier (Findings is never skipped). Rework shows as
  `● Build (r2)`.
- **Sub-sessions add their role** to the first line: `**>> A1-list-commands** · Medium tier · ● Build · builder`. The
  master may leave it out.
- The tier reason is a slug from the Tiers table's "Touches" column: Low `ui-text`, `theme-tokens`, `docs-tools`,
  `calc-rules`, `pure-move`; Medium `editor-feature`, `settings-section`, `new-setting`, `shared-state`,
  `bug-unproven-cause`; High `store-notes`, `startup-quit`, `platform-window`, `timers-deferred`, `save-paths`, `memory`.
- Markdown: one line per field (each renders on its own line), and a blank line before `---` (without it the line above
  turns into a heading).
- Several items in flight: one card each, the one the reply is about first.
- **Quiet relays:** a sub-session note that changes nothing ("still running", a launcher exit already reported) gets at
  most one plain line from the master, with no card and no tool calls. A card is for a phase change or a question.
- A `UserPromptSubmit` hook (`tools/loop-card.ps1`) reminds every turn and lists the items in flight from STATUS.

## Widgets (Claude app features)
Priority (user, 2026-10-29): good development and a good understanding of the codebase first, then saving usage. Wasted
usage is back-and-forth to get things right, so use whatever cuts that:
- **Question cards** (`AskUserQuestion`) for decisions with clear options: scope questions, Approve, rework or accept.
- **Next-step box** (`wrap_up`) at the end of every reply that did work: a short recap and one question with buttons.
- **Task list** (`update_plan`) for the steps of the current phase.
- **Inline visuals** where a picture helps: layouts, flows, lab-first designs, Findings that are hard to say in words.
- The Markdown **status card** above stays text: it costs about the same as plain lines.

## Rules
- Every bug fixed adds a `CONTRACTS` row, so it cannot come back unseen.
- Every Codex run is launched by the item's own sub-session as a background run (a helper chat only when truly needed, on
  Haiku), named and modeled as in "Sessions" below. The master never launches Codex directly. While a run is going the
  sub-session's title starts with `[Codexing..] `; when its item is merged and cleaned up it becomes `[Closed] <ID> · merged · <slug>`.
- Sub-sessions own the card's Build, Check and Hand test sections; the master owns Request, Findings, Plan, Approve and
  STATUS. A sub-session that needs a user decision asks it with the card, and records the answer verbatim.
- Release markers in the STATUS plan: the master asks the user before moving `main`. Only on the user's go.
- Design changes stay lab-first (`AGENTS.md`): Build to Hand test run on a lab build, and the user approves before the real change.
- **Side tasks take the fast lane** (user, 2026-10-31: side tasks such as roadmap work may skip the pipeline "as its not
  a critical piece of the project"). `T` items and tooling that does not ship in the product skip whatever does not help:
  the card can be a few lines or none, no audit, no Codex when the Claude session can do it in minutes, and only the
  checks the change touches. A master still lands it as one commit on dev and gives the user a short change list.
- `Loop` and `D` items keep a short card and a branch (they change how everyone works); Findings can be one paragraph.
- **Who builds:** every item, `Loop`, `D` and `T` included, is built by Codex in a builder sub-session, with the role
  picked by the job (`docs/agents.md`); a docs-only change of a few lines may be made by the builder session itself and
  says so in the report. Codex "Done means" lists offline checks only; live servers, browser and screen checks belong to
  the Claude session (`docs/agents.md`, "What Codex can and cannot check").
- **Progress checks need evidence:** "is it done?" is answered from two samples about 30 seconds apart, never from one
  timestamp (`docs/agents.md`).
- **Rule overrides:** when the user asks for something a rule forbids, the chat names the rule once; if the user still
  says go, it proceeds and logs the override in the item's `.coord/lessons.md` row, so repeated overrides show up as a
  rule worth changing.
- **Proposals go into STATUS at once:** every item or idea a chat names (a follow-up, a "Noticed" side bug, a backlog
  idea) becomes an unnumbered `idea:` or `proposed` plan row when it is named, not later at Merge.
- **Merging:** only a master merges to dev or moves `main`. A sub-session suggests the merge to its master (or the user
  tells the master directly) and merges only into branches it created itself, such as catching up from dev.
- **Loop freeze** (user, 2026-10-31): lifted for `Loop4-retro-fixes`; after it, loop rules change only when
  `.coord/lessons.md` shows the same miss twice (the existing self-improvement rule) or the user asks.
- The plan table: the local server page `http://localhost:47811` (`/roadmap` opens it), live from STATUS and Git. The
  `UserPromptSubmit` hook starts the server when needed; the Artifact (`tools/roadmap.url`) is published only on request.
  `/next-task` and `/merge-task` keep State current.

## IDs and numbering (user, 2026-10-29)
| Prefix | Covers | Example |
|---|---|---|
| `A` | App: product code and behavior, refactors and file splits included | `A1-list-commands` |
| `Loop` | The dev loop and agent processes | `Loop1-process-v3` |
| `D` | Documentation: code comments, wiki, `.md` file management | `D1-ui-region-names` |
| `T` | Tooling and process that is not AI | `T1-<slug>` |
| `L` | Level 4/5 loop automation | `L6-lv4-docs-diet` |
- An item that fits more than one prefix takes the prefix of the area it affects most.
- **Giving a number brings an item into existence:** only the user's confirmation of an idea and its scope assigns one.
  Until then it is an unnumbered plan row `idea: <slug>` with `step: <slug>` rows under it (or a Backlog line).
- One ID = one idea. Its steps are lettered in run order, `A1a-<slug>`, `A1b-<slug>`, each one loop run with its own card,
  branch and commit, lettered when confirmed. A small idea is a single run with no steps; more than about 8 steps means
  split the idea. Review follow-ups become steps of the idea they serve. Rework rounds add `-r<N>`.
- **Eras:** counters can be reset and reprioritized at any time; each reset ends an era, and an ID only means one item
  within its era. Era one (2026-10-17 to 2026-10-29) used `A`, `C`, `N`, `P` and `S<n>-status`, all retired; old cards
  and reports keep their old meaning. Within an era a number is never reused, even if dropped.
- L numbering restarts at `L1` on 2026-11-08: L1-L7 Level 4, L8-L15 Level 5; slugs are lowercase.
- Always write `<ID>-<slug>` plus its scope, never a bare ID or a range.

## Sessions: master, sub-sessions, names (user, 2026-10-29)
**Loop version: v5.3.0** (LP<vX.Y.Z> in master names). A merged item that changes the phases, the roles or who runs which phase bumps the major version (new master at the next handover); new checks, scripts, routes or research steps bump the minor version and need no new master. Changes to chat output only (reply style, status card or hook wording) bump the patch version (`x.y.1`).

- **The picture** (user, 2026-10-30): the user is the CEO; masters are directors, one per area (today three live
  directors: Master 4 for app and design, Loop Hub 2 for `Loop` and `T`, Roadmap Hub 1 for the roadmap; earlier masters
  are closed); sub-sessions are managers who hire Codex for one item or phase and let it go when done. It is a guide,
  not a wall: the user talks to either level directly. Testing an item or changing its scope goes to its sub-session;
  backlog and cross-item matters go to the master. Assume the master always finds out; it may give
  its view through the sub-session but does not override what the user asked there.
- **A master** owns its area's STATUS rows and the roadmap, runs Request to Approve and Merge, checks Files overlap
  before dispatching (`tools/files-overlap.ps1`), and starts sub-sessions. It never runs Codex, builds or hand-test
  launches itself; its own reading is the Low-tier scout.
- **Routing:** directors do not do item work themselves. They ask the user how to route it; the default is a
  Sonnet-or-lower sub-chat per change that runs Codex workers. Sonnet when the user will refine the ask in that chat
  (user, 2026-11-06: "use sonnet"); Haiku when the brief fully specifies the job, such as audits and tests, so the user
  has no need to "travel" with the delegation.
- **Step back after dispatch.** Once a director dispatches, it steps back. User: "Assume that when top level delegates,
  i am traveling WITH that delegation to work together with Sonnet etc. Otherwise expect that it finishes what its
  working on and updates you periodically."
- **Need-to-know updates, after the task, or ask the user.** No in-process updates. A director finishes its task first,
  then decides whether others need an update, or asks the user. User: "Not everything needs to be told to them as ill
  ask them to self update and catch up, unless something im working on really impacts heavily on their domain."
- **Live directors:** Master 4 (app and design, owns STATUS), Loop Hub 2 (`Loop` and `T`), Roadmap Hub 1 (roadmap).
  Earlier masters are closed.

## Branches
One <prefix>/<ID-slug> per item, made at Approve. Worktree per item by default. Tidy buildable commits plus one --no-ff merge commit per item carrying STATUS/lessons. Only a master merges to dev or moves main.

## Triggers in practice
Three commands drive the loop: `/next-task` (Request to Approve, then starts Build; master), `/review-task <ID-slug>`
(Check; sub-session), `/merge-task <ID-slug>` (Merge; master).
- **User, in the master session:** a request or `/next-task` (or "next"); the answer at Approve; the hand-test result;
  then `/merge-task <ID-slug>`; "release" at a marker.
- **Master, automatically:** writes the card, gets Findings (scout or audit sub-session), writes the Plan and asks; after
  approval, creates the branch and worktree and starts the Build sub-session.
- **Build sub-session, automatically:** launches Codex; when it exits runs `/review-task <ID-slug>` (gates, bounds check),
  relaunches rework itself, then, only after acceptance, starts its worktree dev copy with `tools/dev.ps1`, verifies the
  dev badge and EXE/DATA/log paths, hands the user the plan's UI checks and tells the master. Never launch during review or rework.
- **Any time, any terminal:** `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug>` (gate only).
- **CI (manual for now):** GitHub Actions workflow `gate`, run from the Actions tab; it runs `tools/gate.ps1 -Ci`.

## Where the detail lives
| Detail | File |
|---|---|
| Findings, Plan, Gates, Widgets | .claude/skills/next-task/phases.md |
| Check, triage, lessons, scores, rework | .claude/skills/review-task/phases.md |
| Branches, catch-up | .claude/skills/merge-task/branches.md |
| Sessions and file inventory | docs/loop-sessions.md |
| Coding rules | .claude/rules/coding.md (Codex reads docs/coding.md) |
| Agent routing | docs/agents.md |
| Build/dev/install | docs/dev-loop.md |
| Terms | docs/glossary.md |
| Decisions/version history | docs/decisions.md |
