# A21b-save-scheduler-extract-review: lens review of A21b-save-scheduler-extract

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`.
Branch: `refactor/A21b-save-scheduler-extract` (already checked out; never switch, merge, commit or edit product files).
Tier: High (save paths)

## Inputs
- Item card: `.coord/tasks/A21b-save-scheduler-extract.md` (Request, Plan "After" and "Must not change", Files, Approve)
- Implementer report: `.coord/reports/A21b-save-scheduler-extract.md`
- Gate report: `.coord/reports/A21b-save-scheduler-extract.gate.md`
- Diff: `git diff dev...HEAD -- product`
- Focus: `Note.ResolveEmptyEdit` is equivalent to the two copied blocks it replaced in `ScheduleNoteSave` and `SaveImmediately` (compare with `git show dev:product/AntiNotepad/UI/PadWindow.cs`); `SaveTiming` moved byte-for-byte; no save trigger (switch, hide, quit, session end, minimize, tick) lost its save.

## Do
Follow `.agents/skills/lens-review/SKILL.md` exactly, including the plan-bounds item. You may build into `build/A21b-save-scheduler-extract-review` and run `--selftest` (`AVALONIA_TELEMETRY_OPTOUT=1`, offline), then delete that folder. Never change files under `product/`.

## Write: `.coord/reports/A21b-save-scheduler-extract.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
