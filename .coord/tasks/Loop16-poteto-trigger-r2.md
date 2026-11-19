# Loop16-poteto-trigger-r2: two review findings in the poteto gate

Rework round of `Loop16-poteto-trigger` (`docs/loop.md`). Read the item card `.coord/tasks/Loop16-poteto-trigger.md` first: its Plan, Files, Contracts and "For Codex" section still apply unless this file says otherwise. Review: `.coord/reports/Loop16-poteto-trigger.review.md`.

Tier: Medium (same as the card)
Role: quick, because both fixes are a few lines in one script and its self-test

## Why this round
Proven review findings (`code` cause):
1. [major] `tools/poteto-gate.ps1:29` matches "You are the item chat" case-sensitively, but `/next-task` writes "you are the item chat" (lowercase), so that route skips validation. Make the marker match case-insensitively (`-imatch` or `IndexOf` with OrdinalIgnoreCase). Add a self-test: lowercase marker with a bad line 1 is refused (exit 2).
2. [minor] `tools/poteto-gate.ps1:60`: the fenced-fallback regex accepts `/pstack:poteto-mode` as a substring, so fenced `/pstack:poteto-mode-unavailable` passes. Require the command at the start of a code-block content line followed by whitespace or end of line. Add a self-test for the suffix case (exit 2) and keep the existing fenced pass cases (with an argument after the command, CRLF).

## Change to the plan
None.

## Files
- `tools/poteto-gate.ps1`

## Done means
Offline checks only.
- `pwsh -NoProfile -File tools/poteto-gate.ps1 -SelfTest` passes, including the two new cases.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop16-poteto-trigger-r2 -Offline` passes.

Report: `.coord/reports/Loop16-poteto-trigger-r2.md`, short: Summary; Change list; Checks run. Never commit, never edit the card or STATUS.
