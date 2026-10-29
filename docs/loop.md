# Dev loop (per item)

Who does what, what triggers each phase, and what it leaves behind. Model routing and the Codex wrapper: `docs/agents.md`.
Every item has one **item card**, `.coord/tasks/<ID-slug>.md` (from `.coord/templates/item.md`), that grows phase by
phase: the request, what was found, the approved plan, and the check of what was done against both. Anyone can open the
card and see why the item exists, what was agreed and where it stands.

## Phases
`Request → Findings → Plan → Go → Build → Check → Hand test → Merge`

| # | Phase | Who | Leaves on the card | STATUS state |
|---|---|---|---|---|
| 1 | **Request**: the user's words, verbatim, and what they want changed | master | `## Request` | `request` |
| 2 | **Findings**: why the item exists (bug, feature, polish, process), where the change lands, what it does **now** with evidence; bugs need a reproduction | Codex audit in a Haiku sub-session (Medium, High); the master's own short scout (Low) | `## Findings`, audit report | `findings` |
| 3 | **Plan**: **after** state against "now", scope questions, test plan (code and UI), the gates this item runs and what each catches, what is needed from the user, Files, tier | master | `## Plan` | `plan ready` |
| 4 | **Go**: the user approves, changes or drops the plan | user | `## Go` with the user's words | `approved` / `dropped` |
| 5 | **Build**: Codex implements; the director spawns `lens_reviewer` before reporting | Sonnet sub-session runs Codex | code on `feat/<ID-slug>`, report | `building` / `rework rN` |
| 6 | **Check**: the planned gates, then the bounds check (request vs plan vs done) | sub-session (`/review-task`) | `## Check` | `checking` / `accepted` / `ask-user` |
| 7 | **Hand test**: the sub-session opens its build; the user runs the plan's UI checks | user | `## Hand test` with the user's words | `hand test` / `hand-tested` |
| 8 | **Merge**: `--no-ff` into dev, gate on dev, one STATUS commit, push, remove the worktree | master (`/merge-task`) | merge hash in STATUS | `done` |

### 2. Findings: what they must answer
- **Kind and why**: bug, feature, polish or process; what about today's behavior justifies the item.
- **Now**: what the app does today, with evidence: a code path with file:line, a selftest or probe result, or the user's
  description or screenshot. For anything visible, say what dev does today in plain words; if that already matches what
  the user asked for, stop and ask (A53-split-divider-height lost three commits to this).
- **Bugs: reproduce first.** A reproduction (probe, failing selftest, exact code path, or the user's steps confirmed in
  the dev copy) is required before building. No reproduction: stop and ask the user; never build a guess (A54 r1 could not
  reproduce and claimed no fix).
- **Where**: the files and callers the change reaches, shared state, the riskiest part.
- **Tier**: findings confirm or raise the tier. A bug whose cause is not proven by the findings is at least Medium.
- Low items get the master's own scout (a few `rg` and reads, at most about ten tool calls). Anything that needs more
  becomes a Codex audit, and the item is at least Medium.

### 3. Plan: what the user approves
- **After**: what it will do, written next to "Now" so the difference is the change.
- **Scope questions**: anything the request leaves open, asked now (A54 found "all URLs or only shortened ones?" only at
  the hand test).
- **Test plan**: selftest or `CONTRACTS` rows in code, and the exact UI checks for the hand test.
- **Gates**: which checks run and what each one catches for this item (menu below). The tier sets the defaults; the plan
  may add gates, never drop the tier's defaults.
- **Needs from you**: decisions, screenshots, a live check, or "Nothing".
- **Files**: exact paths (the gate reads `## Files`).
The master asks one question: go, change, or drop. Nothing is built before go.

### Gates menu
| Gate | Catches | Default for |
|---|---|---|
| `tools/gate.ps1` (build 0 warnings, selftest, LAYERS, CONTRACTS, seam, scope, size) | compile errors, broken promises, layering leaks, files outside the plan, god-file growth | every tier |
| Codex fresh review (`lens-review`) | logic, behavioral coupling, shared state and latches, timing and caches, missed inputs, cleanup | Medium, High |
| Claude `design-reviewer` | responsibility, boundaries, coupling between features, future cost | High |
| New `CONTRACTS` row | the fixed bug or new promise coming back unseen | every bug fix |
| Reproduction test | the bug is real and is the one fixed | every bug |
| Lab build | a design change the user has not approved yet | design changes (`AGENTS.md`) |
| Hand test | what only the screen shows | anything visible; always for High |

### 6. Check: plan vs done (bounds check)
After the gates, the sub-session compares three things and writes `## Check` on the card:
- **Requested** (`## Request`), **approved** (`## Plan`, `## Go`), **done** (the report's Change list, Behavior changes
  and Out-of-scope files, and `git diff --stat dev...HEAD`).
- Every difference gets one line: **within plan**, **justified** (why, and why the user does not need to decide), or
  **ask the user** (it changes behavior or scope the user did not approve). Ask-the-user items stop the item until answered.
- Side bugs noticed on the way go under "Noticed" as proposed follow-ups, never fixed in passing.
- A scope change from the user mid-item (A54 r3) is recorded on the card with the user's words and goes through Plan again
  for that round.

## Tiers
Set at Plan; findings may raise it. When unsure, pick the higher tier.
| Tier | Touches | Findings | Default gates |
|---|---|---|---|
| Low | UI text, theme tokens, docs/tools, Calc rules with selftests, pure moves with LAYERS rows | master scout | gate; hand test if visible |
| Medium | Editor features (input, cursor, selection, rendering), Settings sections, a new setting or shared state, any bug without a proven cause | Codex audit | gate + Codex review; hand test if visible |
| High | `Store`/`NotesSession`/notes folder, startup and quit, `Platform/` window styles and taskbar, timers and deferred work, save paths, memory | Codex audit | gate + Codex review + design review; hand test always |

## Status card (every reply about an item)
Every Claude reply about project work, master and sub-sessions alike, opens with this card. Keep it to these five lines;
detail goes below it.
```
**<ID-slug>** · <phase> · <tier>
`Request ✓ → Findings ✓ → Plan ● → Go → Build → Check → Hand test → Merge`
**Goal:** <one line>
**Now:** <what is happening, one or two lines>
**Waiting on you:** <the one decision or check, or "nothing">
```
- `✓` done, `●` current, `–` skipped by tier (Findings is never skipped), plain = not reached. Rework shows as `Build ● (r2)`.
- Several items in flight: one card each, the one the reply is about first.
- A `UserPromptSubmit` hook (`tools/loop-card.ps1`) reminds every turn and lists the items in flight from STATUS.

## Rules
- A failed gate, a `rework` verdict or a user scope change goes back to Build as `<ID-slug>-r2`, `-r3` (STATUS `rework rN`),
  with the round's brief listing what changed and why. A third round goes to the user. A round's gate and review run under
  the round's name (`-Task <ID-slug>-rN`): the gate allows the Files of the base card and every round's brief, and the
  reviews read that round's report plus the base report.
- Gate reports (`.coord/reports/*.gate.md`) are regenerated on every run and not committed; the result lines go into the
  card's `## Check`.
- Every bug fixed adds a `CONTRACTS` row, so it cannot come back unseen.
- Every Codex run happens in its own linked, visible sub-session that reports back to the master (user, 2026-10-29).
  A pure run that only executes a brief and returns (an audit, a read-only report) gets a **Haiku** sub-session; any run
  that needs reasoning (Build with `/review-task`, rework, Check) gets a **Sonnet medium** sub-session. Codex models and
  routing inside the run stay as in `docs/agents.md`. The master never launches Codex directly.
- Sub-sessions own the card's Build, Check and Hand test sections; the master owns Request, Findings, Plan, Go and STATUS.
  A sub-session that needs a user decision asks it with the card, and records the answer verbatim.
- **Numbering (user, 2026-10-29).** Giving a number brings an item into existence: only the user's confirmation of an
  idea and its scope assigns one. Until then it is an unnumbered plan row `idea: <slug>` with `step: <slug>` rows under it
  (or a Backlog line). One ID = one idea; its steps are lettered in run order, `A53a-<slug>`, `A53b-<slug>`, each one loop
  run with its own card and branch, lettered when confirmed. A small idea is a single run with no steps; more than about
  8 steps means split the idea. Review follow-ups become steps of the idea they serve. A numbers continue from A53 (A53 and
  later from before 2026-10-29 were dissolved; old reports keep their old meaning), C numbers from C23. A number is never
  reused, even if dropped. Always write `<ID>-<slug>` plus its scope, never a bare ID or a range.
- The plan table: the local page `.coord/roadmap.html` (`/roadmap` opens it). Claude hooks rebuild it in the main checkout
  on every Edit/Write and at each turn's end, so STATUS edits from any path (Edit, shell, merge) reach it, and the open page
  reloads itself every 3 seconds. The Artifact (`tools/roadmap.url`) is published only on request. `/next-task` and
  `/merge-task` keep State current.
- Release markers in the STATUS plan: the master asks the user before tagging dev -> main. Only on the user's go.
- Design changes stay lab-first (`AGENTS.md`): Build to Hand test run on a lab build, and the user approves before the real change.
- Process and tooling items (C-items) run the same phases. Findings can be one paragraph, but the card, the gate and the
  bounds check are not skipped.

## Branches, worktrees and commits (user, 2026-10-29)
- **Every item gets a branch:** `feat/<ID-slug>` off the latest dev, created at Request, including audit-only, docs and
  tooling items. Rework rounds stay on the same branch.
- **Worktree when anything else is checked out or in flight:** the master's checkout stays on dev, so any item a
  sub-session or Codex works on lives in `.claude/worktrees/<ID-slug>` (`git worktree add .claude/worktrees/<ID-slug> -b
  feat/<ID-slug> dev`). Sub-sessions share the master's folder (`use_worktree: false`) and work only inside that path, with
  absolute paths; Codex runs from the worktree's own `.coord/tools/codex-run.sh`. Several items can run at once only if
  their Files lists do not overlap. After Check accepts, the implementation sub-session runs `tools/dev.ps1` from the
  worktree immediately before the hand checks; it verifies the dev badge and EXE/DATA/log paths.
- **What lands on the branch:** the card, audit, reports and code, committed there (Codex cannot commit: the sub-session
  manager does). Push the branch to origin freely.
- **What lands on dev:** only the item's `--no-ff` merge plus one `S<n>-status:` commit per merge. Between merges the
  master edits `.coord/STATUS.md` on dev but leaves it uncommitted (the roadmap reads the working copy); that STATUS
  commit carries every STATUS change since the last merge. Branches never edit STATUS, so merges never conflict on it.
  No other direct commits to dev; urgent one-line fixes also go through a branch.
- **Clean-up:** after the merge, remove the worktree; keep the branch until the next release marker, then delete merged
  branches (local and origin) on the user's go.

## Triggers in practice
Three commands drive the loop: `/next-task` (Request to Go, then starts Build; master), `/review-task <ID-slug>` (Check;
sub-session), `/merge-task <ID-slug>` (Merge; master).
- **User, in the master session:** a request or `/next-task` (or "next"); the answer at Go; the hand-test result; then
  `/merge-task <ID-slug>`; "release" at a marker.
- **Master, automatically:** writes the card, gets Findings (scout or audit sub-session), writes the Plan and asks; after
  "go", starts the Build sub-session.
- **Build sub-session, automatically:** launches Codex; when it exits runs `/review-task <ID-slug>` (gates, bounds check),
  relaunches rework itself, then, only after acceptance, starts its worktree dev copy with `tools/dev.ps1`, verifies the
  dev badge and EXE/DATA/log paths, hands the user the plan's UI checks and tells the master. Never launch during review or rework.
- **Any time, any terminal:** `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug>` (gate only).
- **CI (manual for now):** GitHub Actions workflow `gate`, run from the Actions tab; it runs `tools/gate.ps1 -Ci`.

## Files
| File | For |
|---|---|
| `AGENTS.md` (loaded by `CLAUDE.md` and by Codex) | project rules; points here |
| `.coord/templates/item.md` | the item card: Request, Findings, Plan, Go, Codex brief, Check, Hand test |
| `.coord/templates/findings.md` | brief for the read-only Codex audit (Findings) |
| `.coord/templates/review.md` | brief for the fresh Codex review |
| `tools/gate.ps1` | build, selftest (LAYERS, CONTRACTS), seam, scope, size |
| `product/AntiNotepad/SelfTest/Contracts.cs` | the `CONTRACTS` table of cross-feature promises |
| `.agents/skills/lens-review/SKILL.md` | Codex review checklist (logic, coupling, reliability, plan bounds, proof rule) |
| `.codex/agents/lens-reviewer.toml` | Codex role the director spawns before reporting |
| `.coord/tools/codex-run.sh` | Codex wrapper: tiers, failover, caps (`docs/agents.md`) |
| `.claude/agents/design-reviewer.md` | Claude design reviewer (Opus, read-only) |
| `.claude/skills/next-task/SKILL.md` | `/next-task [ID-slug]`: Request, Findings, Plan, Go, start Build |
| `.claude/skills/review-task/SKILL.md` | `/review-task <ID-slug>`: Check (gates, bounds check, verdict), hand over |
| `.claude/skills/merge-task/SKILL.md` | `/merge-task <ID-slug>`: Merge |
| `tools/loop-card.ps1` | `UserPromptSubmit` hook: status-card reminder and the items in flight |
| `.github/workflows/gate.yml` | CI gate, manual trigger |
| `tools/roadmap.ps1`, `tools/roadmap.url`, `.claude/skills/roadmap/SKILL.md` | local roadmap page from STATUS (`/roadmap`); Artifact on request |
| `.claude/settings.json` | hooks: roadmap rebuild (PostToolUse, Stop), status card (UserPromptSubmit) |
