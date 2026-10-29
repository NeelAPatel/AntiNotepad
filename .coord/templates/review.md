# <round>-review: lens review of <round>

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`.
Branch: `feat/<ID-slug>` (already checked out; never switch, merge, commit or edit product files).
Tier: <Medium | High>

## Inputs
- Item card: `.coord/tasks/<ID-slug>.md` (Request, Plan with "After" and "Must not change", Files, Contracts, Go), plus
  any round briefs `.coord/tasks/<ID-slug>-rN.md`
- Implementer report: `.coord/reports/<ID-slug>.md` and, after a rework, `.coord/reports/<round>.md`
- Gate report: `.coord/reports/<round>.gate.md`
- Diff: `git diff <base>...HEAD -- product` plus uncommitted changes (`git diff -- product`)

## Do
Follow `.agents/skills/lens-review/SKILL.md` exactly, including the plan-bounds item. You may build into
`build/<ID-slug>-review` and run `--selftest`, or add a temporary probe there to prove a finding; delete that folder
after. Never change files under `product/`.

## Write: `.coord/reports/<round>.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
