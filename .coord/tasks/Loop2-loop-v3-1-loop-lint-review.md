# Loop2-loop-v3-1-loop-lint-review: lens review of Loop2-loop-v3-1-loop-lint

Role: reviewer (fresh run, read-only). Scripts and docs only; no product changes.
Branch: `chore/Loop2-loop-v3-1` (already checked out; never switch, merge, commit or edit files outside your report).
Tier: Low

## Inputs
- Item card: `.coord/tasks/Loop2-loop-v3-1.md`; step brief: `.coord/tasks/Loop2-loop-v3-1-loop-lint.md`
- Implementer report: `.coord/reports/Loop2-loop-v3-1-loop-lint.md`
- Gate report: `.coord/reports/Loop2-loop-v3-1.gate.md`
- Diff: `git diff -- tools docs .claude .coord/templates .agents .codex AGENTS.md` plus untracked `tools/loop-lint.ps1`
  and `tools/loop-lint.allow`

## Do
Follow `.agents/skills/lens-review/SKILL.md`, including the plan-bounds item (the brief's Files plus one-line fixes in files
on the card's `## Files` list; the card's own uncommitted Files edit is not yours to review). Focus: false hits and false
misses per rule (stale-path, stale-phase, stale-id, stale-version, feat-only, missing-path incl. Markdown links); the live-file
set and the excluded folders (docs/v2.2, docs/wiki, loop-v2.2-plan, .coord/tasks, .coord/reports, STATUS, lessons);
`tools/loop-lint.allow` matching (path|rule|substring, exact and only its hit; malformed line exit 2); exit codes 0/1/2;
the gate step's trigger condition (live loop file or AGENTS.md changed, always in `-Ci`, SKIP otherwise, deletions, first 40
lines) and that nothing else in `tools/gate.ps1` changed; that every one-line fix is correct and that no history line was
"fixed" (the allow entries are legitimate history mentions). Do not change files.

## Write: `.coord/reports/Loop2-loop-v3-1-loop-lint-review.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
