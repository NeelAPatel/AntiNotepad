# A35a-data-safety-review: lens review of A35a-data-safety

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`.
Branch: `fix/A35a-data-safety` (already checked out; never switch, merge, commit or edit product files).
Tier: Medium

## Inputs
- Item card: `.coord/tasks/A35a-data-safety.md` (Request, Plan with "After" and "Must not change", Files, Contracts, Approve)
- Implementer report: `.coord/reports/A35a-data-safety.md`
- Gate report: `.coord/reports/A35a-data-safety.gate.md` (GATE: PASS)
- Diff: `git diff 6902a44...HEAD -- product`

## Do
Follow `.agents/skills/lens-review/SKILL.md` exactly, including the plan-bounds item. Look hard at: Ctrl+Delete and Ctrl+Backspace in both Search views (can any word-editing keystroke still delete a note?), delete-forever Undo ordering and persistence, the slot-clear toast withdrawal (does the clear itself or the Undo action trip it wrongly?), and save-card Enter with either button focused. You may build into `build/A35a-data-safety-review` and run `--selftest`, or add a temporary probe there to prove a finding; delete that folder after. Never change files under `product/`. Offline only; never launch AntiNotepad or run `--quit`.

## Write: `.coord/reports/A35a-data-safety.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
