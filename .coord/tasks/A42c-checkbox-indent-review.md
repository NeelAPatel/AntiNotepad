# A42c-checkbox-indent-review: lens review of A42c-checkbox-indent

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`.
Branch: `feat/A42c-checkbox-indent` (already checked out; never switch, merge, commit or edit product files).
Tier: Medium

## Inputs
- Item card: `.coord/tasks/A42c-checkbox-indent.md` (Request, Plan with "After" and "Must not change", Files, Contracts, Go), plus
  any round briefs `.coord/tasks/A42c-checkbox-indent-rN.md`
- Implementer report: `.coord/reports/A42c-checkbox-indent.md` and, after a rework, `.coord/reports/A42c-checkbox-indent.md`
- Gate report: `.coord/reports/A42c-checkbox-indent.gate.md`
- Diff: `git diff dev...HEAD -- product` plus uncommitted changes (`git diff -- product`) and the untracked new file `product/AntiNotepad/Editor/DocumentCommands.cs`

## Do
Follow `.agents/skills/lens-review/SKILL.md` exactly, including the plan-bounds item. You may build into
`build/A42c-checkbox-indent-review` and run `--selftest`, or add a temporary probe there to prove a finding; delete that folder
after. Never change files under `product/`.

## Write: `.coord/reports/A42c-checkbox-indent.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
