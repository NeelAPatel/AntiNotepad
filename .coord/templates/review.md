# <ID-slug>-review: lens review of <ID-slug>

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`.
Branch: `feat/<ID-slug>` (already checked out; never switch, merge, commit or edit product files).
Tier: <Medium | High>

## Inputs
- Brief: `.coord/tasks/<ID-slug>.md`
- Implementer report: `.coord/reports/<ID-slug>.md`
- Gate report: `.coord/reports/<ID-slug>.gate.md`
- Diff: `git diff <base>...HEAD -- product` plus uncommitted changes (`git diff -- product`)

## Do
Follow `.agents/skills/lens-review/SKILL.md` exactly. You may build into `build/<ID-slug>-review` and run `--selftest`, or add
a temporary probe there to prove a finding; delete that folder after. Never change files under `product/`.

## Write: `.coord/reports/<ID-slug>.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
