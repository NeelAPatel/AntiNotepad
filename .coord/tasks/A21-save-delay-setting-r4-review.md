# A21-save-delay-setting-r4-review: lens review of A21-save-delay-setting-r4

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`.
Branch: `feat/A21-save-delay-setting` (already checked out; never switch, merge, commit or edit product files).
Tier: High

## Inputs
- Item card: `.coord/tasks/A21-save-delay-setting.md` (Request, Plan with "After" and "Must not change", Files, Contracts, Approve), plus
  any round briefs `.coord/tasks/A21-save-delay-setting-rN.md`
- Implementer report: `.coord/reports/A21-save-delay-setting.md` and, after a rework, `.coord/reports/A21-save-delay-setting-r4.md`
- Gate report: `.coord/reports/A21-save-delay-setting-r4.gate.md`
- Diff: `git diff dev...HEAD -- product` plus uncommitted changes (`git diff -- product`)

## Do
Follow `.agents/skills/lens-review/SKILL.md` exactly, including the plan-bounds item. You may build into
`build/A21-save-delay-setting-review` and run `--selftest`, or add a temporary probe there to prove a finding; delete that folder
after. Never change files under `product/`.

## Write: `.coord/reports/A21-save-delay-setting-r4.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.

## Focus (item chat, 2026-11-16)
User's hand test of the earlier build failed: blank note said "Saved just now", edits kept saying "just now", bars appeared every N seconds on a save. Rules are in `.coord/tasks/A21-save-delay-setting-r3.md` (spec and edge cases 1 to 13). Check especially: a save only changes label text and never wakes the bars; per-note deadlines (split view, two notes); safety write; write-failure retry; blank never-saved note never writes; the label table; saving does not touch undo; minimize and every note switch save at once.

## Files
- `.coord/reports/A21-save-delay-setting-r4.review.md`
