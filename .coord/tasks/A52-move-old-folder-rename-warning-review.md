# A52-move-old-folder-rename-warning-review: lens review of A52-move-old-folder-rename-warning

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`.
Branch: `feat/A52-move-old-folder-rename-warning` (already checked out; never switch, merge, commit or edit product files).
Tier: High

## Inputs
- Brief: `.coord/tasks/A52-move-old-folder-rename-warning.md`
- Implementer report: `.coord/reports/A52-move-old-folder-rename-warning.md`
- Gate report: `.coord/reports/A52-move-old-folder-rename-warning.gate.md`
- Diff: `git diff dev...HEAD -- product` plus uncommitted changes (`git diff -- product`)

## Do
Follow `.agents/skills/lens-review/SKILL.md` exactly. You may build into `build/A52-move-old-folder-rename-warning-review` and run `--selftest`, or add
a temporary probe there to prove a finding; delete that folder after. Never change files under `product/`.

## Write: `.coord/reports/A52-move-old-folder-rename-warning.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
