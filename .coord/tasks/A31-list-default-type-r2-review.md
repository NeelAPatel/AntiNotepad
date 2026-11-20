# A31-list-default-type-r2-review: lens review of A31-list-default-type-r2

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`.
Branch: `feat/A31-list-default-type` (already checked out; never switch, merge, commit or edit product files).
Tier: Medium

## Inputs
- Item card: `.coord/tasks/A31-list-default-type.md` (Request, Plan with "After" and "Must not change", Files, Contracts, Approve), plus
  any round briefs `.coord/tasks/A31-list-default-type-rN.md`
- Implementer report: `.coord/reports/A31-list-default-type.md` and, after a rework, `.coord/reports/A31-list-default-type-r2.md`
- Gate report: `.coord/reports/A31-list-default-type-r2.gate.md`
- Diff: `git diff dev...HEAD -- product` plus uncommitted changes (`git diff -- product`)

## Do
Follow `.agents/skills/lens-review/SKILL.md` exactly, including the plan-bounds item. You may build into
`build/A31-list-default-type-review` and run `--selftest`, or add a temporary probe there to prove a finding; delete that folder
after. Never change files under `product/`.

## Write: `.coord/reports/A31-list-default-type-r2.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
