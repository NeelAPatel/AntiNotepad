# Loop17-chat-title-selfheal-r2: paused phase and done state in the hook

Rework round of `Loop17-chat-title-selfheal` (`docs/loop.md`). Read the item card first: Plan, Files and "For Codex" still apply unless this file says otherwise. Then read `.coord/reports/Loop17-chat-title-selfheal.review.md`.

Tier: Medium (same)
Role: terra, because two bounded fixes in one script plus tests

## Why this round
Proven review findings (cause `code`):
1. [major] `tools/title-hint.ps1`: pending rows with state `paused` or `parked` are skipped (e.g. `T15-ffmpeg-frame-capture-trial` in `.coord/pending.md`), and a STATUS row `paused` gives `phase=none`, so the "keep the stopped phase" rule in `docs/loop.md` cannot work. Fix: include paused/parked rows from pending and STATUS; for a paused/parked row (bare state, no phase word) take the phase from the item card `.coord/tasks/<ID-slug>.md` (search the main checkout and the item's worktree; the last heading among Request, Findings, Plan, Approve, Build, Check, Hand test, Merge with a non-empty body, same idea as `tools/session-where.ps1`; map Request/Findings->findings, Plan->plan, Approve/Build->build, Check->check, Hand test->hand-test, else none). Missing card: phase=none, no error.
2. [minor] canonical STATUS state `done` must give `dev=merged phase=none`.
Add self-tests using the real shapes: a bare `paused` row in STATUS and in pending with a canned card, a bare `done` row, a missing card. Keep ASCII output, fail-open, and the existing 11 cases.

## Change to the plan
None.

## Files
- `tools/title-hint.ps1`
- `docs/loop.md` only if a sentence must change to match; keep it no longer than now.

## Done means
- `pwsh -NoProfile -File tools/title-hint.ps1 -SelfTest` passes with the new cases.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop17-chat-title-selfheal-r2 -Offline` passes.

Report: `.coord/reports/Loop17-chat-title-selfheal-r2.md`: files changed, self-test output, injected line for a paused and a done example.
