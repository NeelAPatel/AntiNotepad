Read by /next-task when Findings, Plan or Approve needs detail.

### 2. Findings: what they must answer
- **Kind and why**: bug, feature, polish or process; what about today's behavior justifies the item.
- **Now**: what the app does today, with evidence: a code path with file:line, a selftest or probe result, or the user's
  description or screenshot. For anything visible, say what dev does today in plain words; if that already matches what
  the user asked for, stop and ask (A53-split-divider-height lost three commits to this).
- **Bugs: reproduce first.** A reproduction (probe, failing selftest, exact code path, or the user's steps confirmed in
  the dev copy) is required before building. No reproduction: stop and ask the user; never build a guess (A54 r1 could not
  reproduce and claimed no fix).
- **Where**: the files and callers the change reaches, shared state, the riskiest part.
- **Size**: a file the change touches that is over 600 lines gets the "one job?" check from `docs/coding.md`; a split it
  calls for is proposed as its own item, not done in passing.
- **Tier**: findings confirm or raise the tier. A bug whose cause is not proven by the findings is at least Medium.
- Low items get the master's own scout (a few `rg` and reads, at most about ten tool calls). Anything that needs more
  becomes a Codex audit, and the item is at least Medium.

**Backlog scans:** `/scan-backlog [slug ...]` runs one read-only Haiku scout per idea in a shared detached dev snapshot
and proposes an order for the user. Reports stay untracked in `.coord/research/<slug>.md`, stamped with the full scanned
SHA, until Approve moves and commits them with the card. At task time Findings reads the scan plus `tools/scan-delta.ps1`:
commits, changed Where files and their neighbors, and the related cards. More than half the Where files changed, or any
Where file deleted, requires a fresh scout or audit; Medium/High audits still run with the scan and delta as inputs.

### 3. Plan: what the user approves
- **After**: what it will do, written next to "Now" so the difference is the change.
- **Expected time**: request to merge, from similar runs in `.coord/logs/usage.tsv` and past cards. Over an hour is said
  plainly, with a smaller cut offered, even when the user wants the full scope (T1-roadmap-overview: expected 30
  minutes, took 4 h 45 min, never called).
- **Scope questions**: anything the request leaves open, asked now (A54 found "all URLs or only shortened ones?" only at
  the hand test).
- **Test plan**: selftest or `CONTRACTS` rows in code, and the exact UI checks for the hand test.
- **Gates**: which checks run and what each one catches for this item (menu below). The tier sets the defaults; the plan
  may add gates, never drop the tier's defaults.
- **Size**: more than about 400 lines of product code (generated files, tests and docs not counted) means lettered steps
  (`A1a`, `A1b`), each its own loop run and commit.
- **Needs from you**: decisions, screenshots, a live check, or "Nothing".
- **Files**: exact paths (the gate reads `## Files`).
The master asks one question: approve, change, or drop. Nothing is built, and no branch exists, before approval.

### Gates menu
| Gate | Catches | Default for |
|---|---|---|
| `tools/gate.ps1` (build 0 warnings, selftest, LAYERS, CONTRACTS, seam, tokens, format, scope, loop-lint, size) | compile errors, broken promises, layering leaks, rising design literal counts, new format problems on changed lines (rules: `.editorconfig`), files outside the plan (FAIL on item branches; WARN on dev, CI or `-LooseScope`), inconsistent loop files, god-file growth | every tier |
| Codex fresh review (`lens-review`) | logic, behavioral coupling, shared state and latches, timing and caches, missed inputs, cleanup | Medium, High |
| Claude `design-reviewer` | responsibility, boundaries, coupling between features, future cost | High |
| New `CONTRACTS` row | the fixed bug or new promise coming back unseen | every bug fix |
| Reproduction test | the bug is real and is the one fixed | every bug |
| Lab build | a design change the user has not approved yet | design changes (`AGENTS.md`) |
| Hand test | what only the screen shows | anything visible; always for High |

## Widgets (Claude app features)
Priority (user, 2026-10-29): good development and a good understanding of the codebase first, then saving usage. Wasted
usage is back-and-forth to get things right, so use whatever cuts that:
- **Question cards** (`AskUserQuestion`) for decisions with clear options: scope questions, Approve, rework or accept.
- **Next-step box** (`wrap_up`) at the end of every reply that did work: a short recap and one question with buttons.
- **Task list** (`update_plan`) for the steps of the current phase.
- **Inline visuals** where a picture helps: layouts, flows, lab-first designs, Findings that are hard to say in words.
- The Markdown **status card** above stays text: it costs about the same as plain lines.
