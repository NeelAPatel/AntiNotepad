# <ID-slug>: <one-line goal>

Item card (`docs/loop.md`). Claude fills Request to Approve before any build; Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB (logs go in reports).

Tier: <Low | Medium | High> · <reason slug>
Branch: `<feat|fix|refactor|docs|chore>/<ID-slug>` in `.claude/worktrees/<prefix>/<ID-slug>` (created at Approve)

## Request
<The user's words, verbatim, with the date. Screenshot paths.>

## Findings
- **Kind and why:** <bug | feature | polish | process>; <why today justifies it>
- **Now:** <what it does today, with evidence (file:line, probe, user description); bugs: how reproduced>
- **Where / Risk:** <files and callers reached; the riskiest part>
- **Source:** <master scout | `.coord/reports/<ID-slug>-audit.md`>

## Plan
- **After:** <what it will do>
- **Expected time:** <request to merge; say so when over an hour>
- **Must not change:** <what stays as is>
- **Scope questions:** <asked and answered at Approve, or "None">
- **Test plan:** <selftest checks and `CONTRACTS` rows; UI: the exact hand checks, or "None (not visible)">
- **Gates:** <each gate from `docs/loop.md` and what it catches here>
- **Needs from you:** <decisions, live checks, or "Nothing">

## Files
Only these files may change (exact paths; `tools/gate.ps1` reads this list). Others need an "Out-of-scope files" line in the report.
- `product/AntiNotepad/<path>`

## Contracts
`CONTRACTS` rows touched (`product/AntiNotepad/SelfTest/Contracts.cs`): keep them passing, add one per new promise and per bug fixed.
- <row>

## Approve
<The user's answer, verbatim, with the date.>

## For Codex
Role: <quick | terra | worker | director> (`docs/agents.md`), because <reason>. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then the Files; re-read before editing.
- Build exactly the Plan's "After" for the Files above; no drive-by changes. If the Findings are wrong, stop and report that.
- Build into `build/<ID-slug>-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, delete the folder. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug> -Offline`.
- Never launch or kill AntiNotepad, never run `--quit`, never edit `.coord/STATUS.md` or this card. Offline checks only (no ports, servers, browser).
- Done means: <observable result from "After">; the Plan's tests exist and pass; build 0 warnings, `SELFTEST PASS`, gate passes.
- Director role: run the `lens_reviewer` agent on your diff and fix what it proves; workers apply the same checklist.

Report `.coord/reports/<ID-slug>.md`, short: 1 Summary; 2 Change list (`file:lines | what | Plan line`); 3 Behavior changes (public results, shared state, timing, defaults; "None" only if true); 4 Out-of-scope files; 5 Plan deviations; 6 Contracts; 7 Lens review; 8 Checks run; 9 Hand checks for the user.

## Check
<`/review-task`: gate lines, review verdicts, bounds check (within plan | justified | ask the user), "Noticed" follow-ups, "Misses" (found at, cause, caught by, should have caught, lesson) or "None".>

## Hand test
<Sub-session: build checked (dev badge text) and the user's result, verbatim.>
