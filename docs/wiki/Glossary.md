# Glossary

| Word | Plain meaning |
|---|---|
| **Item** | One change, with an ID and short name: `A55-split-view-clip`. Always written in full, never just "A55". |
| **ID-slug** | The item's name: an ID (letter + number, e.g. `A55`) plus a dash and a short description. |
| **Idea / step** | A not-yet-confirmed plan row. An idea may have steps (`A53a-...`, `A53b-...`), each one loop run. Numbers are given only when you confirm the scope. |
| **Item card** | The one text file per item (`.coord/tasks/<ID-slug>.md`) that records every phase. |
| **Phase** | One of the eight stages: Request, Findings, Plan, Go, Build, Check, Hand test, Merge. |
| **Status card** | The five-line header every Claude reply about an item starts with: item, phase strip, goal, now, waiting on you. |
| **Master** | The one Claude chat you talk to; runs the planning and merging, never the building. |
| **Helper session (sub-session)** | A Claude chat started by the master for one job on one item: `audit`, `build`, `review`, `test`, `scan`. |
| **Codex** | A second AI that writes code. A *director* plans the work and may hand it to a *worker*. |
| **Lens review** | A fresh Codex read of the finished code against a checklist (logic, coupling, timing). |
| **Design review** | A Claude reviewer for High-tier items: boundaries and future cost. |
| **Tier** | Low, Medium or High: how risky the change is, and so how many checks it gets. |
| **Gate** | `tools/gate.ps1`: the script that builds the app and runs the automatic checks. |
| **Selftest** | The app's built-in test mode (`--selftest`). Small checks of capability, not full coverage. |
| **CONTRACTS** | Promises between features, written as test rows ("the divider stays above the pager"). Every bug fix adds one. |
| **LAYERS** | Rules about which code areas may call which. |
| **Seam** | The boundary between app code and Windows platform code, guarded by a script. |
| **Bounds check** | Comparing what was requested, approved and done; every difference gets a line. |
| **Triage** | Naming one cause for a failure (`code`, `test`, `plan`, `env`, `judgment`) and sending it to the right place. |
| **Misses / Scores** | The lessons file: one line per failure, one row per item of how each check performed. |
| **Rework round** | Another Build pass after a failure: `<ID-slug>-r2`, `-r3`. |
| **Branch** | A separate line of work for one item: `feat/<ID-slug>`. |
| **Worktree** | A separate folder where a branch is checked out, so items in flight do not share files. |
| **`dev`** | The branch where finished items are combined. **`main`** gets tagged releases only. |
| **Merge (no-ff)** | Joining a branch into `dev` while keeping a visible merge point. |
| **Dev copy** | The disposable test copy of the app with its own data. One at a time. Your installed copy is never touched. |
| **Hand test** | You trying the change in the dev copy against a list of checks. |
| **Lab build** | A preview of a design change you have not approved; no real change until you do. |
| **STATUS** | `.coord/STATUS.md`, the "where are we" page, owned by the master. |
| **Roadmap** | The plan as a table (`/roadmap`), built from STATUS. |
| **Loop version** | v1 (kit), v2 (item card, names), v2.1 (cheap checks). A big rule change bumps the major version and brings a new master. |
| **Wiki** | This set of pages. Written for reading; the rules live in `docs/loop.md`. |
