# Process redesign (in discussion)

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
2. **Worktrees (proposed):**
   - One per item, in a folder that mirrors the branch: `.claude/worktrees/feat/A1-list-commands`.
   - Created at the approval step and removed at Merge. If leftover files are found, the merge stops and lists them
     rather than force-deleting.
   - At most 2 items building at once.
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
4. **Commits (in discussion; how the branch's commits land on dev: squash, tidy, or merge):**
   - One commit per item on `dev`, with its STATUS update inside it: no separate status commits.
   - An item that would change more than about 400 lines of product code is split into lettered steps, and each step
     is its own commit.
5. **Coding semantics:**
   - Comments: read like a guide or textbook. Concise and direct, written for a technical reader who is new to the project.
     No unexplained jargon, not oversimplified, not wordy. A comment should stay correct and in place when the code
     below it changes later.
   - Code separation (long term): split large files into focused files, sometimes replacing a file with a folder of the
     same name that holds the smaller files.
6. **Dev loop:**
   - Status card: **settled (2026-10-29).** A Markdown card, not a drawn widget (similar cost to plain text): `>> ID-slug`,
     a `|`-separated phase strip, Tier with a reason slug, Goal, Now, a divider, Waiting on you. Defined in `docs/loop.md`
     ("Status card"); the hook text matches.
7. **Claude widgets:** which ones the loop can use (AskUserQuestion, the inline visualization, the end-of-turn
   next-step box, the task list, Artifacts, ...).
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
