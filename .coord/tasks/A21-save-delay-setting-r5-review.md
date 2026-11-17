# A21-save-delay-setting-r5-review: lens review of A21-save-delay-setting-r5

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`.
Branch: `feat/A21-save-delay-setting` (already checked out; never switch, merge, commit or edit product files).
Tier: High

## Inputs
- Item card: `.coord/tasks/A21-save-delay-setting.md` (Request, Plan with "After" and "Must not change", Files, Contracts, Approve), plus
  any round briefs `.coord/tasks/A21-save-delay-setting-rN.md`
- Implementer report: `.coord/reports/A21-save-delay-setting.md` and, after a rework, `.coord/reports/A21-save-delay-setting-r5.md`
- Gate report: `.coord/reports/A21-save-delay-setting-r5.gate.md`
- Diff: `git diff dev...HEAD -- product` plus uncommitted changes (`git diff -- product`)

## Do
Follow `.agents/skills/lens-review/SKILL.md` exactly, including the plan-bounds item. You may build into
`build/A21-save-delay-setting-review` and run `--selftest`, or add a temporary probe there to prove a finding; delete that folder
after. Never change files under `product/`.

## Write: `.coord/reports/A21-save-delay-setting-r5.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.

## Focus (item chat, 2026-11-16)
Round r5 fixed the three findings of `.coord/reports/A21-save-delay-setting-r4.review.md` (clear-during-first-write, split active-pane save, future-clock label cadence). Re-check those three with fresh probes, and look for new races in the changed Store and Note code: stale acknowledgements, a blank note written then reloaded, a note deleted while a write is queued, quit during a queued write. Spec: `.coord/tasks/A21-save-delay-setting-r3.md`.

## Files
- `.coord/reports/A21-save-delay-setting-r5.review.md`
