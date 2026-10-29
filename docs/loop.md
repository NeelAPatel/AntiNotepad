# Dev loop (per task)

Who does what, what triggers each step, and what it leaves behind. Model routing and the Codex wrapper: `docs/agents.md`.
Purple steps are Claude, teal Codex, gray a script or the user (as in the C13-dev-loop-kit diagrams).

## Tiers
Set in the brief's `Tier:` line by the master at step 2. When unsure, pick the higher tier.
| Tier | Touches | Review | User hand test |
|---|---|---|---|
| Low | UI text, theme tokens, docs/tools, Calc rules with selftests, pure moves with LAYERS rows | gate only | only if visible |
| Medium | Editor features, Settings sections, anything with a new setting or shared state | gate + fresh Codex review | yes if visible |
| High | `Store`/`NotesSession`/notes folder, startup and quit, `Platform/` window styles and taskbar, timers and deferred work, save paths, memory | gate + Codex review + Claude design review | always |

## Steps
| # | Step | Who | Trigger | Leaves | STATUS state |
|---|---|---|---|---|---|
| 1 | Pick the next item | master (Opus) | user says "next", or the previous item merged | - | `briefing` |
| 2 | Write the brief, set the tier | master | same turn | `.coord/tasks/<ID-slug>.md` from `.coord/templates/brief.md` | `briefed (<tier>)` |
| 3 | Read-only impact audit (Medium, High) | Codex director | `bash .coord/tools/codex-run.sh director <ID-slug>-audit .coord/tasks/<ID-slug>-audit.md` | `.coord/reports/<ID-slug>-audit.md` | `audited` |
| 4 | User decides | user | master summarizes and asks | decision in STATUS | `approved` / `changed` / `dropped` |
| 5 | Implement | Codex director + Luna; director spawns `lens_reviewer` before reporting | implementation sub-session: `bash .coord/tools/codex-run.sh director <ID-slug> .coord/tasks/<ID-slug>.md` | code on `feat/<ID-slug>`, report with change list and behavior changes | `implementing` |
| 6 | Gate | `tools/gate.ps1` | first step of `/review-task <ID-slug>` (sub-session runs it when Codex exits) | `.coord/reports/<ID-slug>.gate.md` | `gated` or `rework rN` |
| 7a | Fresh Codex review (Medium, High) | Codex reviewer tier | `/review-task` | `.coord/reports/<ID-slug>.review.md` | `in review` |
| 7b | Design review (High) | Claude `design-reviewer` agent | `/review-task` after 7a | reply to the manager | `in review` |
| 7c | Manager verdict | sub-session manager (Sonnet) | `/review-task` reads 6, 7a, 7b | `## Manager verdict` in the task report, or a `<ID-slug>-rN` brief | `accepted` / `rework rN` / `ask-user` |
| 8 | Hand test | user, dev copy (`tools/dev.ps1`) | manager gives the report's hand-check list | pass or fail in STATUS | `hand-tested` |
| 9 | Merge | master | user's pass | `--no-ff` merge into dev, gate re-run on dev, STATUS, push | `done` |

Rules:
- A failed gate or a `rework` verdict goes back to step 5 as `<ID-slug>-r2`, `-r3` (STATUS `rework rN`). A third round goes
  to the user. A round's gate and review run under the round's name (`-Task <ID-slug>-rN`): the gate allows the Files of the
  base brief and every round's brief, and the reviews read that round's report plus the base report.
- Gate reports (`.coord/reports/*.gate.md`) are regenerated on every run and not committed; the result lines go into the
  task report's "Checks run" and "Manager verdict".
- Every bug fixed later adds a `CONTRACTS` row in its brief's "Contracts" section, so it cannot come back unseen.
- One ID = one loop item = one brief (user, 2026-10-28). An ID is created only as a STATUS plan row with a one-line Scope
  that says exactly what is in it; the brief may not grow past that scope (more work = a new row and ID). Applies to all
  items, old and new. In chat and reports, mention an item as `<ID>-<slug>` plus its scope, never a bare ID or a range.
- The plan table: `/roadmap` (Claude) or `pwsh -NoProfile -File tools/roadmap.ps1` (opens `.coord/roadmap.html`, which a
  Claude hook regenerates on every STATUS edit). `/next-task` and `/merge-task` keep each row's State current.
- Release markers in the STATUS plan: the master asks the user before tagging dev -> main. Only on the user's go.
- Design changes stay lab-first (`AGENTS.md`): steps 5-8 run on a lab build, and the user approves before the real change.

## Triggers in practice
Three commands drive the loop: `/next-task` (steps 1-5, master), `/review-task <ID-slug>` (steps 6-7, sub-session),
`/merge-task <ID-slug>` (step 9, master).
- **User, in the master session:** `/next-task` (or "next"), answers at step 4, "go" for implementation, the hand-test
  result, then `/merge-task <ID-slug>`; "release" at a marker.
- **Master, automatically:** launches the audit (3) and, after "go", starts the implementation sub-session.
- **Implementation sub-session, automatically:** launches Codex (5); when the run exits it runs `/review-task <ID-slug>`
  (6-7), relaunches rework itself, then hands the user the hand checks (8) and tells the master.
- **Any time, any terminal:** `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug>` (gate only).
- **CI (manual for now):** GitHub Actions workflow `gate`, run from the Actions tab; it runs `tools/gate.ps1 -Ci`.

## Files
| File | For |
|---|---|
| `.coord/templates/brief.md` | brief skeleton: Tier, Files, Contracts, Done means, report sections |
| `.coord/templates/review.md` | brief for the fresh Codex review |
| `tools/gate.ps1` | build, selftest (LAYERS, CONTRACTS), seam, scope, size |
| `product/AntiNotepad/SelfTest/Contracts.cs` | the `CONTRACTS` table of cross-feature promises |
| `.agents/skills/lens-review/SKILL.md` | Codex review checklist (logic, coupling, reliability, proof rule) |
| `.codex/agents/lens-reviewer.toml` | Codex role the director spawns before reporting |
| `.coord/tools/codex-run.sh` | `reviewer` tier for the fresh review run |
| `.claude/agents/design-reviewer.md` | Claude design reviewer (Opus, read-only) |
| `.claude/skills/next-task/SKILL.md` | `/next-task [ID-slug]`: steps 1-5 in the master session |
| `.claude/skills/review-task/SKILL.md` | `/review-task <ID-slug>`: steps 6-7 by tier |
| `.claude/skills/merge-task/SKILL.md` | `/merge-task <ID-slug>`: step 9 |
| `.github/workflows/gate.yml` | CI gate, manual trigger |
| `tools/roadmap.ps1`, `.claude/skills/roadmap/SKILL.md` | plan table from STATUS (`/roadmap`, `.coord/roadmap.html`) |
| `.claude/settings.json` | hook: regenerates `.coord/roadmap.html` after every STATUS edit |
