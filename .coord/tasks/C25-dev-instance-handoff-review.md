# C25-dev-instance-handoff-review: lens review of C25-dev-instance-handoff

Role: reviewer (fresh run, read-only). Repo tooling task; product `product/AntiNotepad` is unchanged.
Branch: `feat/C25-dev-instance-handoff` (already checked out; never switch, merge, commit or edit files other than your report).
Tier: High

## Inputs
- Brief: `.coord/tasks/C25-dev-instance-handoff.md`
- Accepted audit: `.coord/reports/C25-dev-instance-handoff-audit.md` (its Risks and safety table are requirements)
- Implementer report: `.coord/reports/C25-dev-instance-handoff.md`
- Gate report: `.coord/reports/C25-dev-instance-handoff.gate.md`
- Diff: uncommitted changes (`git diff`) in `tools/dev.ps1`, `tools/dev.sh`, `docs/*`, `.claude/skills/*`.

## Do
Follow `.agents/skills/lens-review/SKILL.md`. Focus: the installed copy must never be touched (every `--quit` has explicit
ANTINOTEPAD_DATA; force-close only the mutex-named owner with a verified repo `build/dev/AntiNotepad.exe` image, never under
`%LOCALAPPDATA%\Programs\AntiNotepad`); launcher mutex serialization and release in finally; staged helper; vacancy check;
launch verification; `-Fresh` only after shutdown; bash parity. Do NOT launch, quit or kill AntiNotepad. Static reading and
parse checks only.

## Write: `.coord/reports/C25-dev-instance-handoff.review.md`
In the skill's format. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
