# Loop v2.2: discussion summary

Source: user request 2026-10-29 (item `C30-loop-docs-v22-plan`), master session notes, scout of `docs/loop.md`, the three
skills, `tools/gate.ps1`, and the A55 card. Full detail: `docs/loop-v2.2-plan.md`.

## Question
How should the loop handle (1) several items at once, (2) keeping a branch current with dev, (3) merge conflicts?

## What we found
- Parallel items were allowed "only if Files lists do not overlap" but nothing enforced it.
- No step brought a branch up to date with dev; `/merge-task` stops on any conflict.
- `tools/gate.ps1:215-222` diffs `dev...HEAD` (from the merge base), so merging dev into a branch keeps scope clean.
  The pre-existing gate tests the branch alone, so a clash between two items first showed at the gate on dev.
- Only one dev copy exists (C25), so hand tests serialize. Codex caps are shared across worktrees (C28).
- `Contracts.cs` and `SelfTest.cs` are in nearly every bug fix's Files list: a strict overlap rule would block almost all.

## Decisions proposed (user not yet asked to confirm each)
| Topic | Direction |
|---|---|
| Overlap | Script check; contracts/self-test files warn, every other shared file blocks |
| Concurrency | At most 2 items past Go; reading phases (Request, Findings, Plan) may overlap another item's Build |
| Hand tests | A queue, one at a time |
| Catch-up | Merge dev into the branch (never rebase: branches are pushed); before Check and before Merge; the item's own session does it; re-run the gate |
| Conflicts | Mechanical: session by hand. Small textual: Codex round on marker files only. Semantic: back to the master (cause `plan`). Judgment: ask the user |
| Cost | Scripts first (zero model tokens), model judgment only on exceptions |

## Rejected
Rebase (force pushes), auto-resolving conflicts with a tool (hides semantic clashes), a dev copy per item (risk to real
data, C25), more reviewers (look at the Scores table first).

## Open questions for the user
1. Is this v2.2 or v3? (Session-rule changes bump the major version by the current rule.)
2. Limit of 2 items in flight: right number?
3. Is re-hand-testing after a catch-up decided by the user per item, or always when Files overlap with incoming changes?
4. Start order (suggested: files-overlap-check, catch-up-script, hand-test-queue).

## Docs inconsistencies noted at the time (not settled)
v2 credited to C26 in `docs/loop.md` but C27's commit also says "loop v2"; "third round goes to the user" vs "max two
rework rounds"; gate row in the Files table omits format; the parallel-items rule does not mention the single dev copy;
C-items done by Claude directly vs "Codex does the work".
