# A42d-list-commands-review: lens review of A42d-list-commands

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`.
Branch: `feat/A42d-list-commands` (already checked out; never switch, merge, commit or edit product files).
Tier: Medium

## Inputs
- Item card: `.coord/tasks/A42d-list-commands.md` (Request, Plan with "After" and "Must not change", Files, Contracts, Go), plus
  any round briefs `.coord/tasks/A42d-list-commands-rN.md`
- Implementer report: `.coord/reports/A42d-list-commands.md` and, after a rework, `.coord/reports/A42d-list-commands.md`
- Gate report: `.coord/reports/A42d-list-commands.gate.md`
- Diff: `git diff dev...HEAD -- product` plus uncommitted changes (`git diff -- product`)

## Do
Follow `.agents/skills/lens-review/SKILL.md` exactly, including the plan-bounds item. You may build into
`build/A42d-list-commands-review` and run `--selftest`, or add a temporary probe there to prove a finding; delete that folder
after. Never change files under `product/`.

## Write: `.coord/reports/A42d-list-commands.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.

## Focus
Edit order and live offsets in DocumentCommands.Renumber (plan computed before applying vs. old line-by-line), undo scopes, post-edit CaretLine read timing, timer Enter fall-through, Outdent/ClearLine equivalence with old ContinueList (incl. List-mode empty item), uncommitted changes are the build under review.
