# <ID-slug>: <one-line goal>

Role: <director (gpt-6.1-sol medium) | worker (gpt-6-luna xhigh)>. Product `product/AntiNotepad`.
Branch: `feat/<ID-slug>` (already checked out; never switch, merge or commit: you cannot write to `.git`).
Tier: <Low | Medium | High> (rules: `docs/loop.md`)
Read first: `AGENTS.md`, then the files below. Line numbers are from the current tree; re-read before editing.

## Goal
<What the user wants, in their words where possible. What must not change.>

## Files
Only these files may change. Touching any other file needs a line under "Out-of-scope files" in the report.
- `product/AntiNotepad/<path>`
- `product/AntiNotepad/SelfTest.cs` (or the matching `SelfTest/*.cs` part)

## Contracts
Behavior promises this task touches (`CONTRACTS` rows in the selftest). Keep them passing; add a row for any new promise.
- <e.g. "Store save on an unavailable folder succeeds as a no-op with LastError set">

## Constraints
- Smallest change that meets the goal. No drive-by refactors, renames, icon/token/style edits outside the goal.
- Build into `build/<ID-slug>-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug> -Offline`; the manager re-runs it online.
- Never launch or kill AntiNotepad. Never run `--quit`.

## Done means
- <observable result 1>
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug>` passes.

## Before you report (director only)
Spawn the `lens_reviewer` agent on your diff (it reads `.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why
not. Workers without spawning: run the same checklist yourself and say so.

## Report: `.coord/reports/<ID-slug>.md`
Keep it short. Use exactly these sections:
1. **Summary**: two or three sentences.
2. **Change list**: one row per changed block: `file:line-range | what changed | which goal line it serves`.
3. **Behavior changes**: every changed public method result or meaning, new or changed shared state, timing, default, or
   setting effect. Write "None" only if true; the review checks this against the diff.
4. **Out-of-scope files**: file and why, or "None".
5. **Contracts**: rows added or changed, or "None".
6. **Lens review**: findings and what you did with each.
7. **Checks run**: build, selftest, gate result lines.
8. **Hand checks for the user**: what to try in the dev copy, or "None (not visible)".
