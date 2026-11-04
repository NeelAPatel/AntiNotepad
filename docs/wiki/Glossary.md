**In short**

An item is one agreed change; a phase is one stage of doing it. A chat, an AI run, and a script have different responsibilities. These definitions follow `docs/glossary.md` and `docs/loop.md`, so the same words mean the same thing across cards and reports.

# Glossary

Loop version is **v5.0.0**: major changes cover phases or roles, minor changes add checks or routes, and patch changes affect chat output only (`docs/loop.md`, "Sessions"; `docs/glossary.md`).

## Work and checks

Definitions in this table come from `docs/glossary.md`, “Work and the loop,” with operational detail from `docs/loop.md`, “Phases,” “Rules,” and “IDs and numbering.”

| Term | Plain meaning |
|---|---|
| Item | One confirmed change named ID-slug, with a card and one commit on dev |
| Prefix | A for app, Loop for the dev loop, D for docs, T for non-AI tooling |
| Idea | Unnumbered proposal; the user's scope confirmation assigns its number |
| Step | Lettered part of an idea; each confirmed step is a loop run with its own card and commit |
| Era | Period between counter resets; old IDs retain their meaning within their era |
| Card | The item record, made from `.coord/templates/item.md` |
| Phase | Request, Findings, Plan, Approve, Build, Check, Hand test, or Merge |
| Approve | User's yes to Plan; branch creation follows. Older cards say Go |
| Round | Another Build after failure, suffixed -r2 or -r3; -r4 goes to the user |
| Tier | Low, Medium, or High risk; sets minimum gates |
| Gate | Automatic check; “the gate” is `tools/gate.ps1` |
| Bounds check | Compare requested, approved, and done; explain every difference |
| Triage | Choose code, test, plan, env, conflict, or judgment and route the failure |
| Misses / Scores | Failure lessons and per-item check results in `.coord/lessons.md` |
| Preflight | Scripted checks before branch creation or Merge |
| Catch-up | Merge local dev into an item branch, never rebase |
| RETEST | Catch-up output saying whether prior hand checks need repeating |
| Overlap check | Compare planned Files across item checkouts |
| Hot file | Shared file whose overlap warns instead of failing |
| Backlog scan | Read-only research in a detached dev snapshot |
| Scan delta | Compare earlier research with current dev before using it |
| Loop-lint | Check live loop instructions and references for inconsistency |
| Loop freeze | Lifted; loop rules change after a repeated miss or at the user's request |
| Loop version | v5.0.0 is the process version, separate from a Loop item ID; major changes alter phases/roles, minor changes add checks/routes, patch changes affect chat output only |

RETEST detail comes from `tools/catch-up.ps1` and `.claude/skills/review-task/SKILL.md`, step 5. See [Parallel and Branches](Parallel-and-Branches) and [Backlog Scans](Backlog-Scans).

## People and sessions

These definitions follow `docs/glossary.md`, “People and sessions,” `docs/loop.md`, “Sessions,” and `docs/agents.md`, “Launch.”

| Term | Plain meaning |
|---|---|
| Master | Top-level Claude chat for one area (app, loop); plans, dispatches and alone merges to dev and main |
| Master notes | `.coord/directors/<slug>.md`, gitignored; reloaded into the master after a context compaction |
| Sub-session / sub-chat | Linked Claude chat for one item's step; sidebar title and status card show its role |
| Scout / auditor | Scout reads evidence; auditor launches and relays a read-only Codex Findings run |
| Builder / reviewer / tester | Sonnet medium roles for Build, Check, and Hand test; normally one chat renamed as it advances |
| Codex director | Headless Sol medium run that delegates implementation and obtains lens review |
| Codex role | `quick`, `terra`, `worker`, `director`, `reviewer`, `backup`: model and effort picked by the job (`docs/agents.md`) |
| Lens reviewer | Codex subagent checking proven logic, coupling, and reliability problems |
| Fresh reviewer | Separate headless Codex run used at Check for Medium/High |
| Design reviewer | Read-only Opus Claude subagent checking architecture for High |
| Hook | Script automatically invoked by a configured chat event |

See [Who Does What](Who-Does-What) for exact models, launch commands, payloads, and permissions.

## Git and the app

Definitions follow `docs/glossary.md`, “Git” and “The app on this machine”; data details are in `docs/dev-loop.md`, “Dev copy.”

| Term | Plain meaning |
|---|---|
| dev | Integration branch receiving one squashed commit per item |
| main | Releases only; fast-forwarded and tagged on the user's go |
| Item branch | Separate line of work, with a type prefix and ID-slug |
| Worktree | Separate checkout folder for an item branch |
| Squash merge | Combine the branch's changes into one integration commit |
| Release tag | Semver name such as v0.7.0; -rcN marks a build not yet hand-tested |
| Dev copy | Disposable app build with separate identity and app home; only one runs at a time |
| Installed copy | User's real app and notes; never used for development |
| Hand test | User tries the plan's checks in the dev copy |
| Lab build | Preview used to approve a design before the real change |
| Selftest | App's built-in headless capability checks, invoked with --selftest |
| LAYERS / CONTRACTS | Selftest tables for dependency boundaries and promises between features |
| Seam | Boundary between platform code and the rest, checked by `tools/check-platform-seam.ps1` |

## Sources

- `docs/glossary.md` — canonical terms.
- `docs/loop.md` — phases, sessions, tiers, numbering, freeze, version rule, and branches.
- `docs/agents.md` — models and run types.
- `docs/dev-loop.md` — dev and installed copies.
- `.coord/templates/item.md`, `.coord/lessons.md`, `tools/gate.ps1`, `tools/check-platform-seam.ps1` — concrete records and checks.
- `tools/catch-up.ps1`, `.claude/skills/review-task/SKILL.md` — RETEST and handover.
