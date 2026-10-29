# A55-split-view-clip-review: lens review of A55-split-view-clip

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`.
Branch: `feat/A55-split-view-clip` (already checked out; never switch, merge, commit or edit product files).
Tier: Medium

## Inputs
- Item card: `.coord/tasks/A55-split-view-clip.md` (Request, Plan with "After" and "Must not change", Files, Contracts, Go), plus
  any round briefs `.coord/tasks/A55-split-view-clip-rN.md`
- Implementer report: `.coord/reports/A55-split-view-clip.md` and, after a rework, `.coord/reports/A55-split-view-clip.md`
- Gate report: `.coord/reports/A55-split-view-clip.gate.md`
- Diff: `git diff dev...HEAD -- product` plus uncommitted changes (`git diff -- product`)

## Do
Follow `.agents/skills/lens-review/SKILL.md` exactly, including the plan-bounds item. You may build into
`build/A55-split-view-clip-review` and run `--selftest`, or add a temporary probe there to prove a finding; delete that folder
after. Never change files under `product/`.

## Write: `.coord/reports/A55-split-view-clip.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
