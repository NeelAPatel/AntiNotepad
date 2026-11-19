# Loop17-chat-title-selfheal lens review

## Findings (proven)
1. [major] tools/title-hint.ps1:17 - Real paused rows cannot heal with their retained phase. This filter skips every pending `paused`/`parked` row, including the existing `T15-ffmpeg-frame-capture-trial` row in `.coord/pending.md:9`, so its worktree receives no hint. If the row instead remains in STATUS with the documented state `paused` (`docs/loop.md:25`), lines 28-34 produce `dev=paused phase=none`: no branch recovers the stopped phase. This breaks the explicit phase-retention rule at `docs/loop.md:57`. The "paused resume" self-test at line 81 uses `paused building`, never a pending row or the canonical bare state, and only checks that the paused word remains. Smallest fix: include paused/parked pending rows and define a durable source for their stopped phase, then test the actual row shape and the documented resume instruction to clear the closing word and update the row.
2. [minor] tools/title-hint.ps1:29 - The canonical merged state `done` loses its closing word. `docs/loop.md:23` names `done` as the Merge state; for that input line 29 leaves `dev=none`, and line 35 clears the phase, yielding `dev=none phase=none` rather than `dev=merged phase=none`. The merged self-test at line 88 uses the noncanonical state `merged`, hiding this mismatch. Map `done` to `merged` and test `done`. This applies while a done row is present; the separate director instruction still handles normal row removal at merge.

## Behavior changes check
The report's prose describes the new hook and doc behavior, although it has no separately labeled Behavior changes section. The diff matches its ASCII, fail-open and audit/rework/review-suffix claims. Its paused/resume claim is incomplete as proven above. Its live-check sentence is stale relative to the card's later Check entry; this review did not repeat that live check.

The new command is called by the existing UserPromptSubmit group. No existing method signature changes. The hook reads rows, logs and ledger without writing them; the chat remains responsible for title and row edits. The normal entry point catches errors and exits 0; IDs are restricted to ASCII and all other emitted fields are fixed ASCII words. No retitling API or script was added.

The runner's `.coord/logs/<task>.<role>.log` and seven-column, tab-separated ledger (`.coord/tools/codex-run.sh:41,50`) match the parser's filename and column positions. Success/failure completion, log freshness, 20-minute stall hints and the requested rework me-marker clearing have corresponding code paths and canned checks. Normal findings/plan/build/check/hand-test states match the phase table, apart from the closing-state issue above.

## Scope and plan check
In plan: all six scoped files serve the approved hook, registration, title rule or skill guidance. No unrelated hunks or unlisted scope expansion found. The doc explicitly requires title and row changes in the same step; the two skill lines add the planned director guidance. No product changes, dependencies, persistent cache, background process or retitling script was introduced. The approved pause/phase behavior remains incomplete (finding 1). Hot-file budget warnings are already recorded by the supplied gate; no new gate was run.

## Unproven risks
- A restart/failover that creates its new log within the previous ledger entry's second can be hidden by the one-second comparison margin until another log write; the existing tests do not exercise that boundary.
- Self-tests do not exercise main-checkout resolution, worktree filtering, pending precedence or missing/unreadable input files; these paths received source inspection only.

## Checks run
- `pwsh -NoProfile -File tools/title-hint.ps1 -SelfTest` - exit 0: `TITLE HINT SELFTEST PASS (hand test, running, stalled, failed, rework, check, paused resume, review running, review failed, merged, garbage)`.
- Read-only inspection of `git diff dev...HEAD -- tools .claude docs`, changed-file context, item card, implementation/gate reports, handover, runner, loop-card and current pending rows. Findings use exact code paths; no additional executable probes, ports, product tests or gate runs.
- Supplied gate report: PASS with budget warnings; historical evidence only, not rerun here.

VERDICT: rework
