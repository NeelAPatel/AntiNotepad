# Loop v2.2: activation edits (NOT APPLIED)

Apply only when the user triggers v2.2 (`docs/v2.2/README.md`). Text below is the replacement or addition per target,
in CHANGELOG order. Line numbers are deliberately not given: re-read the target first.

## step: files-overlap-check
- `git mv tools/v2.2/files-overlap.ps1 tools/files-overlap.ps1`.
- `tools/gate.ps1`: after the Scope step add a step `Overlap` that runs the script for `$Task` (skip with `-Ci` and for
  `-rN` rounds, which use the base task) and maps exit 1 to FAIL, warnings to WARN.
- `.claude/skills/next-task/SKILL.md` step 3, append: "Run `pwsh -NoProfile -File tools/files-overlap.ps1 -Task <ID-slug>`.
  A FAIL means a second item shares a file with one in flight: ask the user whether to wait for the first merge or re-plan Files."
- `docs/loop.md` "Branches, worktrees and commits": replace "Several items can run at once only if their Files lists do
  not overlap." with "... Several items can run at once only if `tools/files-overlap.ps1` passes (the contracts and
  self-test files only warn)."

## step: in-flight-limit-and-board
- `docs/loop.md` Rules, add: "At most 2 items past Go (Build, Check, Hand test) at a time. Request, Findings and Plan of
  another item may run meanwhile; its Go waits for the overlap check. The limit is raised only on the user's word."
- STATUS "Now": an **In flight** table (item, phase, tier, branch, sub-session, waiting on), rewritten at each phase change.
- `tools/loop-card.ps1`: print a warning line when more than 2 items are past Go.

## step: hand-test-queue
- `docs/loop.md` Hand test row and `/review-task` step 5, add: "`tools/dev.ps1` replaces the one dev copy, so hand tests
  run one at a time, oldest accepted first. An accepted item waits (one quiet line) until the master says 'your turn'.
  Items with no UI checks skip the queue."
- Master rule: after a hand-test result is recorded, tell the next queued sub-session its turn has come.

## step: catch-up-script
- `git mv tools/v2.2/catch-up.ps1 tools/catch-up.ps1`; add its row to the `docs/loop.md` Files table.

## step: catch-up-points
- `/review-task` step 2, prepend: "If `dev` is not an ancestor of HEAD, run `pwsh -NoProfile -File tools/catch-up.ps1 -Task
  <ID-slug>`, push, then continue with the gate. CONFLICT: triage (step 4a, cause `conflict`)."
- `/merge-task` step 2, append: "If `dev` moved since Check, ask the item's session to catch up and re-run the gate; the
  user decides whether the incoming dev changes touching the item's Files need a new hand test. The master never merges
  dev into a feature branch."
- `docs/loop.md`: one paragraph "Catching up with dev" (merge, never rebase; two points; the session does it; gate after).

## step: conflict-triage
- `/review-task` 4a table, add the row: `conflict` | mechanical (both sides appended): session fixes by hand |
  small textual: `<ID-slug>-rN` round limited to marker files | semantic: stop, tell the master (re-plan, Go again) |
  judgment: ask the user.
- `.coord/lessons.md` header: add `conflict` to the causes and `catch-up`, `overlap-check` to the check names.

## step: conflict-codex-round
- `.coord/templates/round.md`: a "Conflict round" variant (Files = only the conflicted files; the session already ran the
  merge and left the markers; Codex edits markers only and never commits).
- `.agents/skills/lens-review/SKILL.md`: for a merge-resolution diff, review only the resolved hunks and the other item's CONTRACTS rows.

## step: rehearsal
- Two throwaway docs items with an overlapping hot file and one real overlap; prove the FAIL/WARN, a CLEAN catch-up and
  a forced CONFLICT; record results in a card.

## Finally
Bump the loop version text in `docs/loop.md` "Sessions" (v2.2, or v3 plus a new master if the user says so), update
`docs/wiki` (Home note, Loop-History row, Dev-Loop-Overview, Who-Does-What) and republish the wiki.
