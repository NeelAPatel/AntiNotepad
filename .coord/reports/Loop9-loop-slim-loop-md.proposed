# Dev loop (per item)

Who does what, when, and what each phase leaves behind. Start/resume: .coord/STATUS.md; roadmap: /roadmap (http://localhost:47811).

Every item has one **item card**, `.coord/tasks/<ID-slug>.md` (from `.coord/templates/item.md`), that grows phase by phase: request, findings, approved plan, then the check of what was done against both. Budget: 4 KB per card (logs go in reports).

**Loop version: v5.3.0** (LP<vX.Y.Z> in director names). Roles or ownership change: major; new checks, scripts, routes or research steps: minor; chat output only: patch. On a bump the merging director updates STATUS and all live director names.

## Phases
`Request → Findings → Plan → Approve → Build → Check → Hand test → Merge`

| # | Phase | Who | Leaves on the card | STATUS state |
|---|---|---|---|---|
| 1 | **Request**: the user's words, verbatim | director | `## Request` | `request` |
| 2 | **Findings**: why the item exists, where it lands, what it does **now** with evidence; bugs need a reproduction | `auditor` chat running a Codex audit (Medium, High); the director's own scout (Low) | `## Findings`, audit report | `findings` |
| 3 | **Plan**: **after** against "now", scope questions, test plan (code and UI), gates and what each catches, Files, tier | director | `## Plan` | `plan ready` |
| 4 | **Approve**: the user approves, changes or drops; approval creates the branch and worktree | user | `## Approve` (verbatim) | `approved` / `dropped` |
| 5 | **Build**: Codex implements; its director role spawns `lens_reviewer` before reporting | `builder` chat runs Codex | code on `<prefix>/<ID-slug>`, report | `building` / `rework rN` |
| 6 | **Check**: planned gates, then the bounds check (request vs plan vs done) | `reviewer` (`/review-task`; the builder chat renamed) | `## Check` | `checking` / `accepted` / `ask-user` |
| 7 | **Hand test**: the chat opens its build; the user runs the UI checks | user, with the `tester` | `## Hand test` (verbatim) | `hand test` / `hand-tested` |
| 8 | **Merge**: commit review, rebase onto dev, `--no-ff` merge commit with the item's STATUS edits, gate on dev, push, delete branch, remove worktree | director (`/merge-task`) | merge commit on dev | `done` |

`paused` (any state after Approve): the user set a started item aside; branch, worktree and card stay and it does not count against the 3-item cap.

## Tiers
Set at Plan; findings may raise it; when unsure pick the higher.
| Tier | Touches | Findings | Default gates |
|---|---|---|---|
| Low | UI text, theme tokens, docs/tools, Calc rules with selftests, pure moves | director scout | gate; hand test if visible |
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
- Reply body, decision block and glosses: `.claude/output-styles/antinotepad.md`. `✓` done, `●` current, `○` not reached, `–` skipped by tier; rework is `● Build (r2)`. Chats add their role on line one (`· builder`).
- Tier reason slugs: the "Touches" column of Tiers, as a lowercase slug (`docs-tools`, `editor-feature`, `store-notes`, ...).
- One line per field and a blank line before `---`. Several items in flight: one card each. A note that changes nothing gets one plain line, no card.
- The `UserPromptSubmit` hook (`tools/loop-card.ps1`) lists the items in flight.

## Rules
- Every bug fixed adds a `CONTRACTS` row.
- **One level, two chats per item:** one to investigate (scout or audit), one to build, test and wrap up. **A sub-chat never starts a chat** (no `start_session`, no helper or Codex-run chat) unless the user says so in that chat; a third chat only on the user's word.
- Codex runs are launched by the item's build chat as background runs in that chat; the director never launches Codex. Title starts `[Codexing..] ` while a run goes; after cleanup it becomes `[Closed] <ID> · merged · <slug>`.
- Sub-chats own Build, Check and Hand test on the card; the director owns Request, Findings, Plan, Approve and STATUS. A user decision is asked with the card and recorded verbatim.
- Release markers in the Plan: ask the user before moving `main`; only on their go. Design changes are lab-first (`AGENTS.md`).
- **Side tasks** (`T`, tooling that does not ship): fast lane, a few-line card or none, no audit, only the checks the change touches. `Loop` and `D` items keep a short card and a branch.
- **Who builds:** every item is built by Codex in a builder chat, role by the job (`docs/agents.md`); a docs-only change of a few lines may be made by the builder itself, and says so in the report. Codex checks are offline only.
- **Progress checks need evidence:** two samples about 30 s apart, never one timestamp.
- **Rule overrides:** the chat names the rule once; on the user's go it proceeds and logs the override in the item's `.coord/lessons.md` row.
- **Proposals go into STATUS at once:** every item or idea a chat names becomes an unnumbered `idea:` or `proposed` plan row when named.
- **Merging:** only a director merges to dev or moves `main`; a sub-chat suggests the merge and merges only branches it created.
- Loop rules change only when `.coord/lessons.md` shows the same miss twice or the user asks.
- **Hot-file budgets:** STATUS 8 KB, this file 8 KB, `.coord/lessons.md` 3 KB, item cards 4 KB (`tools/loop-lint.ps1` warns). Finished history goes to `.coord/status-archive.md` and `.coord/lessons-archive.md`.

## IDs and numbering
| Prefix | Covers | Example |
|---|---|---|
| `A` | App: product code and behavior, refactors and splits | `A1-list-commands` |
| `Loop` | The dev loop and agent processes | `Loop1-process-v3` |
| `D` | Documentation: comments, wiki, `.md` management | `D1-ui-region-names` |
| `T` | Tooling and process that is not AI | `T1-<slug>` |
| `L` | Level 4/5 loop automation | `L6-lv4-docs-diet` |
- An item fitting several prefixes takes the one it affects most. Always write `<ID>-<slug>`, never a bare ID or a range.
- **A number is given only on the user's confirmation** of an idea and scope; until then it is an unnumbered `idea: <slug>` row with `step: <slug>` rows (or a Backlog line).
- One ID = one idea; steps are lettered in run order (`A1a-<slug>`), each a loop run with its own card, branch and commit; more than about 8 steps: split the idea. Rework rounds add `-r<N>`.
- Eras (`docs/decisions.md`): an ID means one item within its era; numbers are never reused in an era. Slugs are lowercase.

## Branches and triggers
One `<prefix>/<ID-slug>` per item, made at Approve, in a worktree by default. Tidy buildable commits plus one `--no-ff` merge commit per item carrying STATUS and lessons. Detail: .claude/skills/merge-task/branches.md.
- `/next-task` (Request to Approve, starts Build; director), `/review-task <ID-slug>` (Check; build chat), `/merge-task <ID-slug>` (Merge; director). The user answers at Approve, reports the hand test, then says merge; "release" at a marker.
- The build chat launches Codex, runs `/review-task` on exit, relaunches rework, and only after acceptance runs `tools/dev.ps1`, verifies the dev badge and EXE/DATA/log paths, and hands over the UI checks. Never launch during review or rework.
- Any time: `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug>`. CI (manual): GitHub Actions `gate` runs `tools/gate.ps1 -Ci`.

## Where the detail lives
Phases: `.claude/skills/{next-task/phases.md,review-task/phases.md,merge-task/branches.md}`; chats, names, handover: docs/loop-sessions.md; coding: .claude/rules/coding.md (Codex: docs/coding.md); routing: docs/agents.md; build/install: docs/dev-loop.md; terms: docs/glossary.md; locked choices and history: docs/decisions.md.
