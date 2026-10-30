# Loop2-loop-v3-1-loop-md-v3-1

## Summary
Documented the v3.1 loop rules, glossary and absorbed v2.2 history, and corrected the run-counter wording. Updated the gate so LoopLint sees changed `.coord/templates/` and `.coord/tools/` paths that Scope filters out. No product files changed.

## Change list
| File:line-range | What changed | Plan line |
|---|---|---|
| `docs/loop.md:141-163` | Exact rework limit, builder ownership and Loop freeze | After 2 |
| `docs/loop.md:186-199` | v3.1 version and versioning rule; Loop discussion chat | After 1 |
| `docs/loop.md:232-249` | No-worktree rule and STATUS rides with the next item commit | After 3 |
| `.claude/skills/review-task/SKILL.md:46` | Exact rework limit | After 4 |
| `docs/glossary.md:24-42` | v3.1 label and requested terms | After 5 |
| `docs/v2.2/README.md:1-29` | Absorbed status and step mapping table | After 6 |
| `docs/agents.md:10,21-23` | Wrapper run counter and Codex usage limits | After 7 |
| `tools/gate.ps1:518-529` | Independent LoopLint path set includes `.coord/` | After 8 |
| `tools/loop-lint.allow:7-12` | Added the exact absorbed-v2.2 history exception; removed the obsolete dormant-path exception | After 9 |

## Behavior changes
Loop process rules now state the rework, builder, versioning, discussion-chat, freeze, no-worktree and STATUS handoff rules. The gate now runs LoopLint when eligible `.coord/templates/*.md` or `.coord/tools/*.sh` paths change, even though Scope omits those paths. No application behavior changed.

## Out-of-scope files
None touched. Existing `.coord/STATUS.md` changes and unrelated untracked history/task files were left untouched.

## Plan deviations
None.

## Contracts
None.

## Lens review
Worker self-review against the lens checklist: no proven findings. No separate lens reviewer was used.

## Checks run
- `pwsh -NoProfile -File tools/loop-lint.ps1` — `LOOP-LINT: PASS (37 files)`.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline` — `GATE: PASS`; build 0 warnings, `SELFTEST PASS`, Scope PASS (18 changed files allowed), LoopLint PASS, Size PASS. Offline NuGet audit was disabled as expected.
- `docs/loop.md` grew by 11 lines.
- `git diff --check` on the seven scoped files — passed.

## Hand checks for the user
None (not visible).
