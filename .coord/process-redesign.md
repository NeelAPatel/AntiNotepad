# Process redesign (settled 2026-10-29; rolled out as loop v3 by Loop1-process-v3)

Opened 2026-10-29. Goal: settle branches, worktrees and naming on `dev` first, then clean the history. No history rewrite
and no commits for this until the user says so.

## Done so far (2026-10-29)
- Only `main` and `dev` exist, locally and on GitHub. Every other branch was merged into dev and then deleted
  (`feat/A38-mvvm-audit`, the only unmerged one, was dropped on the user's say-so; its commit was d3af353).
- All extra worktrees were removed, and so were the tools' checkpoint refs (`refs/monocode/*`, `refs/codex/*`).
- The 19 tags are still there. They keep the old history alive until the rewrite.

## Naming: agreed so far
| Prefix | Covers | Example |
|---|---|---|
| `A` | App: product code and behavior, including refactors | `A1-list-commands` |
| `Loop` | The dev loop and agent processes; may carry a version (`Loop1.1`, `LoopV1`) | `Loop1-squash-merge` |
| `D` | Documentation: code comments, wiki, `.md` file management | `D1-naming-doc` |
| `T` | Tooling and process that isn't AI (scope to be defined) | `T1-...` |

- When an item fits more than one prefix, it gets the prefix of the area it affects most.
- Steps are lettered in order: `A1a`, `A1b`, `A1c`.
- Counters can be reset and reprioritized at any time. Each reset ends an **era**, and the coming reset ends the first
  one. An ID only means one item within its era.
- Retired: `C`, `N`, `P` and the `S<N>-status` commits.
- Rework rounds keep `-r<N>` (`A1-list-commands-r2`).

## Topics to discuss
1. **Branches (agreed 2026-10-29, except where noted):**
   - `main` holds releases only: at a release it fast-forwards to `dev` and gets a tag, so there are no merge commits.
   - `dev` is integration: each item lands as one squashed commit, merged locally.
   - Item branches use the standard type prefixes, so they group into folders: `feat/`, `fix/`, `refactor/`, `docs/`,
     `chore/`. The prefix follows the kind of change, not the ID letter. Proposed mapping: A is `feat/`, `fix/` or
     `refactor/`; D is `docs/`; Loop and T are `chore/`. Example: `feat/A1-list-commands`.
   - An item branch is created at the approval step and deleted at Merge, locally and on GitHub.
   - **No pull requests for now.** They return with the release-pipeline topic (9).
2. **Worktrees (agreed 2026-10-29):**
   - One per item, in a folder that mirrors the branch: `.claude/worktrees/feat/A1-list-commands`.
   - Created at the approval step and removed at Merge. If leftover files are found, the merge stops and lists them
     rather than force-deleting.
   - At most 3 items building at once (user's pick).
   - The "Go" phase becomes **Approve** (agreed 2026-10-29; "yes" and "go" are too common to mean approval). It is the
     user's approval of the Plan, when the branch and worktree are created and Build starts. The rename goes into
     docs, skills, the hook and the roadmap together with the other changes.
3. **Naming:**
   - IDs: the scheme above. Commit subject `<ID-slug>: <what changed>` (72 characters or fewer); body of 3-6 short lines
     covering why, what changed, the gate result and the hand test. Claude attribution lines are off
     (`.claude/settings.json`, 2026-10-29).
   - Release tags: plain semver, `v0.7.0`, with `-rc1`, `-rc2` for untested builds (agreed).
   - Master chat: `Master <N> · Loop<ver> · <slug>` (agreed; the version helps when diagnosing across masters).
   - Sub-session: `<ID> · <step> · <slug>`, for example `A1 · build · list-commands` (agreed).
   - Subagents and sub-sessions: a fixed list of roles (`scout`, `auditor`, `builder`, `reviewer`, `tester`), shown in
     their status cards (agreed: required for sub-sessions; the master may skip it).
   - Concept names: `docs/glossary.md`, one line per term (agreed).
   - UI region names: item `D1-ui-region-names`, a labeled screenshot plus a glossary section, built lab-first (agreed).
4. **Commits (agreed 2026-10-29):**
   - One commit per item on `dev`. STATUS edits made while the item is in flight stay uncommitted on dev and land
     inside that item's commit at Merge. STATUS edits with no item (backlog ideas) ride with the next item, or one
     `chore: status` commit at the end of the day if nothing lands. No separate status commits otherwise.
   - An item that would change more than about 400 lines of product code is split into lettered steps (`A1a`, `A1b`),
     each its own commit. Generated files, tests and docs don't count toward the 400.
5. **Coding semantics:**
   - Comments (agreed 2026-10-29; goes into a new `docs/coding.md` and the builder brief): read like a guide for a
     technical reader new to the project.
     1. Each file opens with 1-3 lines: what it is for and where it fits.
     2. Each class and non-trivial method gets a `///` summary of what and why, not how.
     3. Inline `//` only for the non-obvious: a Win32 quirk, a timing reason, a trap.
     4. No item IDs, dates or "changed from X" in code; git holds history.
     5. Define a term on first use, or use the glossary word.
     6. One to three lines; anything longer goes in `docs/`.
     7. A multi-stage method that must stay one method opens with its stages as a pipeline line,
        `// parse -> resolve units -> format`, and each stage is a block separated by a blank line and headed by its
        slug (on the line before, or at the end of the line).
   - Rollout: on touch. Any file an item changes is brought up to the standard; no big pass first.
   - Formatting (agreed): a `.editorconfig` (first-party .NET, read by `dotnet format`); the gate checks only the files
     an item changed, so there is no whole-codebase reformat commit.
   - Code separation (agreed 2026-10-29): split by job, not line count. A part is separate when it changes for
     different reasons, is a pipeline stage with clear input and output, sits in a different layer (Win32 / UI /
     model), or is a self-contained set with its own types. Keep together what shares heavy private state. Over 600
     lines triggers a "one job?" check in Findings; no hard cap. Partial files (`PadView.EdgeFade.cs`) when parts share
     state, a separate class when one stands alone. Planned splits: `SelfTest.cs`, `Calc/Calculator.cs`,
     `Model/Store.cs` (A items, Low tier pure-move); `Editor/PadView.cs` on touch only.
   - Folders: namespace mirrors folder (`Model/Search/` is `AntiNotepad.Model.Search`). When 3 or more files share a
     job, or a folder passes about 12 files, they move into a subfolder. A split item does its own reorganizing:
     moves files, updates namespaces and `using` lines, the selftest layer rules (which match namespaces) and the code
     layout lines in `AGENTS.md`; the Plan lists the moves and the build gate catches any missed reference.
6. **Dev loop:**
   - Status card: **settled (2026-10-29).** A Markdown card, not a drawn widget (similar cost to plain text): `>> ID-slug`,
     a `|`-separated phase strip, Tier with a reason slug, Goal, Now, a divider, Waiting on you. Defined in `docs/loop.md`
     ("Status card"); the hook text matches.
7. **Claude widgets (agreed 2026-10-29):** use everything that helps. Priority order (user): good development and
   good understanding of the codebase first, then usage preservation. Bad usage is back-and-forth to get things right;
   the loop should cut that, not cut features. In use: question cards for decisions with clear options, the
   next-step box with buttons at the end of each reply, the task list for the steps of the current phase, inline
   visuals where a picture helps (layouts, flows, lab-first designs), plus the Markdown status card.
8. **History rewrite:** option B (tidy, then land) chosen 2026-10-29; full dry-run preview in `.coord/history-preview.md`
   (154 commits, final files identical to dev). User edits: keep #1; #2 renamed "confirmed Avalonia stack via spike
   testing across stacks"; 10-17 to 10-22 03:16 squashed to one commit "v0.1 AntiQuickPad built and discarded"; the
   overnight spike (2026-10-22 03:21-06:53) kept as its own commit (today's code descends from it); R1 and V05
   renamed. All old tags deleted and remade: `v0.1.0`, `milestone/overnight-rebuild`, `v0.5.0`, `v0.6.0-rc1` to `rc9`,
   each release tag checked to carry exactly the files it shipped. Kept at 154 commits (user, 2026-10-29: "it gives
   better granularity"; one-per-item, about 90, was declined). Runs only after 1-7 are settled and on the user's
   explicit go; the dry run is re-run on the final `dev` first, since any later commit changes the preview.
9. **Release pipeline (later):** from `main` to packaged releases, with GitHub Actions, tests and builds. Pull requests
   come back here (`dev` to `main` at the least).
