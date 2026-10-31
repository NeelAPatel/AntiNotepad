# Glossary

One line per term, as used in the docs, cards, comments and chat. Add a term the first time a doc needs it.
UI region names (the parts of the window) come with `D1-ui-region-names`.

## Work and the loop
- **Item**: one unit of work with an ID and a slug, `A1-list-commands`; it has one card and one commit on dev.
- **ID prefix**: `A` app, `Loop` dev loop and agents, `D` docs, `T` non-AI tooling (`docs/loop.md`, "IDs and numbering").
- **Slug**: the short kebab-case name after the ID; never written without its ID.
- **Step**: a lettered part of a large item, `A1a`, `A1b`; each is its own loop run and commit.
- **Round**: one more Build after a failure, `A1-list-commands-r2`.
- **Idea**: an unnumbered plan row (`idea: <slug>`); it gets an ID only when the user confirms its scope.
- **Era**: the time between two counter resets. Era one ran 2026-10-17 to 2026-10-29.
- **Card**: the item's file `.coord/tasks/<ID-slug>.md`, filled phase by phase.
- **Phase**: one of Request, Findings, Plan, Approve, Build, Check, Hand test, Merge.
- **Approve**: the user's yes to a Plan; it creates the branch and worktree. Called "Go" before loop v3.
- **Tier**: Low, Medium or High risk; it sets the default gates. Its reason is a slug (`docs/loop.md`, "Tiers").
- **Gate**: an automatic check; `tools/gate.ps1` is "the gate".
- **Bounds check**: the Check step that compares what was requested, approved and done.
- **Hand test**: the user trying the change by hand in the dev copy.
- **Lab build**: a build that shows a design before the user approves it.
- **Triage**: naming one cause for a failure (`code`, `test`, `plan`, `env`, `conflict`, `judgment`) and routing it.
- **Status card**: the Markdown block every reply about an item opens with.
- **Loop version**: the version of these rules, written `Loop v3.1`; not to be confused with a `Loop` item ID.
- **Catch-up**: merging `dev` into an item branch with `tools/catch-up.ps1`.
- **Overlap check**: compares an item's Files with other in-flight items (`tools/files-overlap.ps1`).
- **Hot file**: a shared file that only warns in the overlap check.
- **Preflight**: the checks before an item branch (`tools/approve.ps1`) or merge (`tools/merge-preflight.ps1`).
- **Backlog scan**: a read-only snapshot scout run with `/scan-backlog`.
- **Research file**: `.coord/research/<slug>.md`, stamped with the dev commit it scanned.
- **Scan delta**: checks scanned research against later dev commits (`tools/scan-delta.ps1`).
- **Loop-lint**: checks live loop instructions and file references (`tools/loop-lint.ps1`).
- **Loop freeze**: the pause on new `Loop` items and repeat-miss rule (`docs/loop.md`, "Rules").

## People and sessions
- **Master**: a top-level Claude chat for one area (app, loop) that plans, dispatches and alone merges, named
  `Master <N> · Loop v<V> · <slug>`; a director in the user's picture.
- **Master notes**: `.coord/directors/<slug>.md`, gitignored; reloaded after a context compaction.
- **Sub-session**: a linked Claude chat for one step of one item, named `<ID> · <step> · <slug>`.
- **Role**: what a sub-session does: `scout`, `auditor`, `builder`, `reviewer`, `tester`; shown in its status card.
- **Codex role**: `quick`, `terra`, `worker`, `director`, `reviewer`, `backup`; model and effort picked by the job
  (`docs/agents.md`).
- **Fast lane**: side tasks (`T`, tooling that does not ship) skip the steps that do not help (`docs/loop.md`, "Rules").
- **Lens reviewer**: the Codex reviewer a director spawns before reporting.
- **Design reviewer**: the read-only Claude reviewer for High-tier items.

## Git
- **dev**: the integration branch; one commit per item.
- **main**: releases only; fast-forwarded to dev at a release and tagged.
- **Item branch**: `<prefix>/<ID-slug>` with `feat/`, `fix/`, `refactor/`, `docs/` or `chore/`.
- **Worktree**: a second checkout of the repo for one item, `.claude/worktrees/<prefix>/<ID-slug>`.
- **Squash merge**: all of a branch's commits become one commit on dev.
- **Release tag**: semver, `v0.7.0`; `-rc1` for a build not yet hand-tested.

## The app on this machine
- **Installed copy**: the user's real AntiNotepad in `%LOCALAPPDATA%\Programs\AntiNotepad`; never touched by dev work.
- **Dev copy**: the build `tools/dev.ps1` runs with its own data folder and a dev badge; one runs at a time.
- **Selftest**: `AntiNotepad.exe --selftest`, the app's built-in checks.
- **LAYERS / CONTRACTS**: selftest tables: which namespace may use which, and promises between features.
- **Seam**: the boundary between `Platform/` and the rest, checked by `tools/check-platform-seam.ps1`.
