# A32-selection-convert-menu-review: lens review of A32-selection-convert-menu

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`.
Branch: `feat/A32-selection-convert-menu` (already checked out; never switch, merge, commit or edit product files).
Tier: Medium

## Inputs
- Item card: `.coord/tasks/A32-selection-convert-menu.md` (Request, Plan with "After" and "Must not change", Files, Contracts, Approve), plus
  any round briefs `.coord/tasks/A32-selection-convert-menu-rN.md`
- Implementer report: `.coord/reports/A32-selection-convert-menu.md` and, after a rework, `.coord/reports/A32-selection-convert-menu.md`
- Gate report: `.coord/reports/A32-selection-convert-menu.gate.md`
- Diff: `git diff dev...HEAD -- product` plus uncommitted changes (`git diff -- product`)

## Do
Follow `.agents/skills/lens-review/SKILL.md` exactly, including the plan-bounds item. You may build into
`build/A32-selection-convert-menu-review` and run `--selftest`, or add a temporary probe there to prove a finding; delete that folder
after. Never change files under `product/`.

## Write: `.coord/reports/A32-selection-convert-menu.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.

## Focus
Submenu popup in `UI/PadMenu.cs` (open/close on hover, click, Right, Left, Esc; does it stay open when the pointer moves to another row; does it close with the parent), selection line range at selection edges (selection ending at a line start, empty last line), numbering that spills into adjacent unselected list lines, undo as one step.
