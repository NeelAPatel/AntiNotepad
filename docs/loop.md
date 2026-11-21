# Dev loop (per item)

Who does what, and what each phase leaves behind. Start/resume: .coord/STATUS.md; roadmap: /roadmap (http://localhost:47811).

Every item has one **item card**, `.coord/tasks/<ID-slug>.md` (from `.coord/templates/item.md`) that grows phase by phase: request, findings, approved plan, then the check against both.

**Loop version: v5.5.0** (LP<vX.Y.Z> in director names). Roles or ownership change: major; new checks, scripts or routes: minor; chat output only: patch. On a bump the merging director updates STATUS and live director names.

**One chat per item.** The director writes the Request, starts the item chat and steps back; that chat runs Findings through Hand test and the user answers it. The director merges. Start it in the trusted main checkout (`use_worktree: false`); it works in its worktree by absolute path.

## Phases
`Request → Findings → Plan → Approve → Build → Check → Hand test → Merge`

| # | Phase | Who | Leaves on the card | STATUS state |
|---|---|---|---|---|
| 1 | **Request**: the user's words, verbatim; starts the item chat | director | `## Request` | `request` |
| 2 | **Findings**: why, where it lands, what it does **now** with evidence; bugs need a reproduction | item chat: own scout (Low), read-only Codex audit in the same chat (Medium, High) | `## Findings`, audit report | `findings` |
| 3 | **Plan**: **after** against "now", scope questions, test plan (code and UI), gates and what each catches, Files, tier | item chat | `## Plan` | `plan ready` |
| 4 | **Approve**: the user answers "Approve?" in the item chat; it runs `tools/approve.ps1` (branch and worktree) | user | `## Approve` (verbatim) | `approved` / `dropped` |
| 5 | **Build**: Codex implements; its director role spawns `lens_reviewer` before reporting | item chat runs Codex | code on `<prefix>/<ID-slug>`, report | `building` / `rework rN` |
| 6 | **Check**: planned gates, then the bounds check (request vs plan vs done) | item chat (`/review-task`) | `## Check` | `checking` / `accepted` / `ask-user` |
| 7 | **Hand test**: the chat opens its build; the user runs the UI checks | user, item chat | `## Hand test` (verbatim) | `hand test` / `hand-tested` |
| 8 | **Merge**: commit review, rebase onto dev, `--no-ff` merge commit with STATUS edits, gate on dev, push, clean up | director (`/merge-task`) | merge commit on dev | `done` |

`paused` (after Approve): the user set a started item aside; branch, worktree and card stay; it does not count against the 3-item cap.

## Tiers
Set by the item chat in Findings (one line to the director: tier, reason); when unsure pick the higher. Raising it: tell the user at once what changes, why, the extra cost.
| Tier | Touches | Findings | Default gates |
|---|---|---|---|
| Low | UI text, theme tokens, docs/tools, Calc rules with selftests, moves | own scout | gate; hand test if visible |
| Medium | Editor features, Settings sections, a new setting or shared state, any bug without a proven cause | Codex audit | gate + Codex review; hand test if visible |
| High | `Store`/`NotesSession`/notes folder, startup and quit, `Platform/` window styles and taskbar, timers, save paths, memory | Codex audit | gate + Codex review + design review; hand test always |

## Status card (every reply about an item)
One line by default: `**>> <ID-slug>** · ● <phase> · <what changed>`. The **full card** only when the phase changes, at a session's first reply, or with a decision block:
```
**>> <ID-slug>** · <Tier> tier · ● <phase>
✓ Request | ✓ Findings | ● Plan | ○ Approve | ○ Build | ○ Check | ○ Hand test | ○ Merge
**Tier:** <Tier> · <reason slug>
**Goal:** <one line>
**Now:** <one or two lines>

---

**Waiting on you:** <the decision block, or "nothing">
```
- Reply body, decision block and glosses: `.claude/output-styles/antinotepad.md`. `✓` done, `●` current, `○` not reached, `–` skipped by tier; rework is `● Build (r2)`.
- Tier reason: the "Touches" column as a slug (`docs-tools`, `editor-feature`).
- One line per field, a blank line before `---`; one card per item in flight. A note that changes nothing gets one plain line.

## Rules
- Every bug fixed adds a `CONTRACTS` row.
- **One chat per item, one level.** An item chat never starts a chat (no `start_session`, no helper or Codex-run chat) unless the user says so there. A second chat only if the first sat idle over an hour with a large context, or on the user's word.
- **Umbrellas:** with the user's go an umbrella chat may start children; dispatch requires a parent row and each child row (Scope `child of <ID-slug>:`), plus `.coord/tasks/<umbrella>-board.md` for queue, order and landed. Children rebase when ready and merge into their parent; only a director merges the umbrella to dev. Crosstalk is need-to-know: no acks or progress FYIs; message for coordination, a major result requested by the parent, or at the user's request. At review/merge ready print `READY: <ID-slug>. Go to <chat> and run /merge-task <ID-slug>` (or `/review-task` here).
- Codex runs are background runs in the item chat; the director never launches Codex.

Item chats set or clear their own title in the same step as their STATUS row (pending row before Approve): `<me emoji> [Codex word] [dev word] <ID-slug> ◆ <phase>`, empty fields left out. The `title-hint` hook gives ASCII words at each user turn; the chat writes ⁉️, brackets and ◆ and sets the title first. Codex words: Codexing, Codex failed, Codex stalled. Dev words: Merged, Dropped, Paused, Parked (Paused and Parked keep the phase; cleared on resume). Rework clears the me marker.

- **One asker:** the user answers the item chat; it shows next-step buttons only for its own item decisions. The director asks only about merges, releases, rule changes and cross-item matters. Decisions are recorded verbatim.
- **STATUS:** the item chat edits only its own item's row in the main checkout's `.coord/STATUS.md` (absolute path; the roadmap reads it), never other rows; branches never edit STATUS. The director edits Now and finished moves.
- Release markers: move `main` only on the user's go (`dev-loop.md`, "Release"). Design changes are lab-first (`AGENTS.md`).
- **Side tasks** (`T`, tooling): fast lane, a few-line card or none, no audit, only the checks the change touches. `Loop` and `D` items keep a short card and a branch.
- **Who builds:** Codex, role by the job (`docs/agents.md`); the item chat may write docs-only text itself and says so in the report. Codex checks are offline only.
- **Progress checks:** two samples 30 s apart.
- **Rule overrides:** name the rule once; on the user's go proceed and log it in `.coord/lessons.md`.
- **Proposals go into `.coord/pending.md` at once** as unnumbered rows. Approve moves the row to STATUS; Merge to `.coord/status-archive.md`.
- **Merging:** only a director merges to dev or moves `main`; the item chat suggests the merge.
- Loop rules change only when `.coord/lessons.md` shows the same miss twice or the user asks.
- **Hot-file budgets:** STATUS 8 KB, this file 8 KB, `.coord/lessons.md` 3 KB, cards 4 KB (`tools/loop-lint.ps1` warns); history goes to the `-archive.md` files.

## IDs and numbering
| Prefix | Covers | Example |
|---|---|---|
| `A` | App: product code and behavior, refactors and splits | `A1-list-commands` |
| `Loop` | The dev loop and agent processes | `Loop1-process-v3` |
| `D` | Documentation: comments, wiki, `.md` management | `D1-ui-region-names` |
| `T` | Tooling and process that is not AI | `T1-<slug>` |
| `L` | Level 4/5 loop automation | `L6-lv4-docs-diet` |
- An item fitting several prefixes takes the one it affects most. Always write `<ID>-<slug>`, never a bare ID.
- **A number is given only on the user's confirmation** of an idea and scope; until then it is an unnumbered `idea: <slug>` row with `step: <slug>` rows (or a Backlog line).
- One ID = one idea; steps are lettered in run order (`A1a-<slug>`), each a loop run with its own card, branch and commit; more than about 8 steps: split the idea. Rework rounds add `-r<N>`.
- Eras (`docs/decisions.md`): numbers are never reused within an era. Slugs are lowercase.

## Branches and triggers
One `<prefix>/<ID-slug>` per item, made at Approve, in a worktree. Tidy buildable commits plus one `--no-ff` merge commit per item. Detail: .claude/skills/merge-task/branches.md.
- `/next-task` (Request: director; Findings to Build: item chat), `/review-task <ID-slug>` (Check; item chat), `/merge-task <ID-slug>` (Merge; director). The user answers at Approve, reports the hand test, then says merge.
- The item chat launches Codex, runs `/review-task` on exit, relaunches rework, and only after acceptance runs `tools/dev.ps1`, verifies the dev badge and EXE/DATA/log paths, and hands over the UI checks. Never launch during review or rework.
- Any time: `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug>`. CI (manual): Actions `gate`.

## Where the detail lives
Phases: `.claude/skills/{next-task/phases.md,review-task/phases.md,merge-task/branches.md}`; chats, names: docs/loop-sessions.md; coding: .claude/rules/coding.md; routing: docs/agents.md; build/install: docs/dev-loop.md; terms: docs/glossary.md; decisions: docs/decisions.md.
