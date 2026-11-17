# A23-paste-keep-structure-r4-review: lens review of A23-paste-keep-structure-r4

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`.
Branch: `feat/A23-paste-keep-structure` (already checked out; never switch, merge, commit or edit product files).
Tier: Medium

## Inputs
- Item card: `.coord/tasks/A23-paste-keep-structure.md` (Request, Findings, Plan, Files, Contracts, Approve), plus round briefs `-r2`, `-r3`, `-r4` in `.coord/tasks/`
- Implementer reports: `.coord/reports/A23-paste-keep-structure.md` and `-r2`, `-r3`, `-r4` equivalents
- Gate report: `.coord/reports/A23-paste-keep-structure-r4.gate.md`
- Diff: `git diff dev...HEAD -- product`
- Focus: `Editor/PasteEngine.cs`, `PasteHtml.cs`, `PasteTable.cs`, the paste hook in `Editor/PadView.cs`. The user's hand test passed (lists, tables, inline marks, toast, with A22 merged in). Look for: crashes or hangs on odd HTML (deep nesting, huge tables), paste when selection or document changed mid-await, Undo grouping, async paste races, guard gaps that would rewrite code or markdown source, wrong width counts, performance on a large clipboard (paste speed matters).

## Do
Follow `.agents/skills/lens-review/SKILL.md` exactly, including the plan-bounds item. You may build into `build/A23-review` and run `--selftest`, or add a temporary probe there to prove a finding; delete that folder after. Never change files under `product/`.

## Write: `.coord/reports/A23-paste-keep-structure-r4.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
