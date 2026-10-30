# <ID-slug>: <one-line goal>

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build; Codex reads the whole card and
follows "For Codex". Codex never edits this file; Check and Hand test are filled by the sub-session afterwards.

Tier: <Low | Medium | High> · <reason slug> (rules: `docs/loop.md`)
Branch: `<feat|fix|refactor|docs|chore>/<ID-slug>` in `.claude/worktrees/<prefix>/<ID-slug>` (created at Approve)

## Request
<The user's words, verbatim, with the date. Screenshots: their paths.>

## Findings
- **Kind and why:** <bug | feature | polish | process>; <what about today justifies it>
- **Now:** <what it does today, with evidence: file:line, probe or selftest result, the user's description>
- **Reproduction (bugs):** <how it was reproduced, or "not reproduced: asked the user">
- **Where:** <files, callers, shared state the change reaches>
- **Risk:** <the riskiest part>
- **Source:** <master scout | `.coord/reports/<ID-slug>-audit.md`>

## Plan
- **After:** <what it will do, next to "Now">
- **Must not change:** <behavior, look or files that stay as they are>
- **Scope questions:** <asked at Approve and answered there, or "None">
- **Test plan, code:** <selftest checks and `CONTRACTS` rows, named>
- **Test plan, UI:** <the exact hand checks, or "None (not visible)">
- **Gates:** <each gate from the menu in `docs/loop.md` and what it catches for this item>
- **Needs from you:** <decisions, screenshots, live checks, or "Nothing">

## Files
Only these files may change (exact paths; `tools/gate.ps1` reads this list). Any other file needs a line under
"Out-of-scope files" in the report.
- `product/AntiNotepad/<path>`
- `product/AntiNotepad/SelfTest.cs` (or the matching `SelfTest/*.cs` part)

## Contracts
`CONTRACTS` rows this item touches (`product/AntiNotepad/SelfTest/Contracts.cs`). Keep them passing; add a row for any new
promise and for every bug fixed.
- <e.g. "Store save on an unavailable folder succeeds as a no-op with LastError set">

## Approve
<The user's answer, verbatim, with the date: go, or the scope change.>

## For Codex
Role: <director (gpt-6.1-sol medium) | worker (gpt-6-luna xhigh)>. Product `product/AntiNotepad`. The branch is already
checked out: never switch, merge or commit (you cannot write to `.git`). Read `AGENTS.md`, then the files above. Line
numbers are from the current tree; re-read before editing.

Constraints:
- Build exactly the Plan's "After" for the Files above. No drive-by refactors, renames, icon/token/style edits outside it.
- If the Findings turn out wrong (the bug does not reproduce, the cause is elsewhere, the plan cannot work), stop and
  report that instead of building something else.
- Build into `build/<ID-slug>-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug> -Offline`; the manager
  re-runs it online.
- Never launch or kill AntiNotepad. Never run `--quit`. Never edit `.coord/STATUS.md` or this card.

Done means:
- <observable result 1, from the Plan's "After">
- The Plan's code tests exist and pass; build 0 warnings, `SELFTEST PASS`, the gate passes.

Before you report (director only): spawn the `lens_reviewer` agent on your diff (it reads
`.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why not. Workers without spawning: run the same
checklist yourself and say so.

Report: `.coord/reports/<ID-slug>.md`, short, with exactly these sections:
1. **Summary**: two or three sentences.
2. **Change list**: one row per changed block: `file:line-range | what changed | which Plan line it serves`.
3. **Behavior changes**: every changed public method result or meaning, new or changed shared state, timing, default, or
   setting effect. Write "None" only if true; the review checks this against the diff.
4. **Out-of-scope files**: file and why, or "None".
5. **Plan deviations**: anything done differently from the Plan, and why, or "None".
6. **Contracts**: rows added or changed, or "None".
7. **Lens review**: findings and what you did with each.
8. **Checks run**: build, selftest, gate result lines.
9. **Hand checks for the user**: the Plan's UI checks, plus anything new, or "None (not visible)".

## Check
<Filled by `/review-task`: gate result lines, review verdicts, then the bounds check, one line per difference between
Request, Plan and done: within plan | justified (why) | ask the user. "Noticed": side bugs as proposed follow-ups.
"Misses": one line per failure (found at, cause, caught by, should have caught, lesson), or "None".>

## Hand test
<Filled by the sub-session: the build checked (dev badge text), and the user's result, verbatim.>
