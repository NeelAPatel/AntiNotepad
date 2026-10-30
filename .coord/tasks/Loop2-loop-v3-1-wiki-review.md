# Loop2-loop-v3-1-wiki-review: accuracy review of Loop2-loop-v3-1-wiki

Role: reviewer (fresh run, read-only for everything but your report). Docs only; no product changes.
Branch: `docs/Loop2-loop-v3-1-wiki` (already checked out; never switch, merge, commit or edit files outside your report).
Tier: Low

## Inputs
- Step brief: `.coord/tasks/Loop2-loop-v3-1-wiki.md` (its Request quotes the user's bar: no claim without a file)
- Implementer report: `.coord/reports/Loop2-loop-v3-1-wiki.md`
- The pages: `docs/wiki/*.md` (changed by the last commit; the brief lists the expected pages)
- The sources the pages cite: `docs/loop.md`, `docs/agents.md`, `docs/glossary.md`, the skills, `.coord/tools/codex-run.sh`,
  `tools/*.ps1`, `.coord/templates/*.md`, `.coord/tasks/Loop2-loop-v3-1*.md`, `.coord/reports/Loop2-loop-v3-1*.md`, `git log`

## Do
Follow `.agents/skills/lens-review/SKILL.md`, with this one instruction added: **verify every factual claim in
`docs/wiki/*.md` against the cited file, and list any claim that has no source or that a source contradicts.** Especially:
numbers (caps, run counts, rework limits, the 7 proven findings, commit hashes), exact quotes (brief openings, handover
prompt), models and efforts, sandbox flags, script exit codes and output lines, command names, and anything that says a
thing "runs", "checks" or "blocks". Also check plan bounds (only the brief's Files changed, SVGs untouched) and that the
Known limits and the "no plans or intentions" rule hold. Do not change any file except your report.

## Write: `.coord/reports/Loop2-loop-v3-1-wiki-review.review.md`
In the format the skill gives, with unsourced or contradicted claims listed as findings (page, quoted claim, the file that
contradicts it or the missing source). End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
