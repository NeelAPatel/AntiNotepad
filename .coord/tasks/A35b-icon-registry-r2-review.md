# A35b-icon-registry-r2-review: lens review of A35b-icon-registry-r2

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`.
Branch: `feat/A35b-icon-registry` (already checked out; never switch, merge, commit or edit product files).
Tier: Medium

## Inputs
- Item card: `.coord/tasks/A35b-icon-registry.md` (Request, Plan with "After" and "Must not change", Files, Contracts, Approve), plus
  any round briefs `.coord/tasks/A35b-icon-registry-rN.md`
- Implementer report: `.coord/reports/A35b-icon-registry.md` and, after a rework, `.coord/reports/A35b-icon-registry-r2.md`
- Gate report: `.coord/reports/A35b-icon-registry-r2.gate.md`
- Diff: `git diff 6902a44...HEAD -- product` plus uncommitted changes (`git diff -- product`)

## Do
Follow `.agents/skills/lens-review/SKILL.md` exactly, including the plan-bounds item. You may build into
`build/A35b-icon-registry-review` and run `--selftest`, or add a temporary probe there to prove a finding; delete that folder
after. Never change files under `product/`.

## Write: `.coord/reports/A35b-icon-registry-r2.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.

## Item-chat notes
- The item chat moved `UI/Icons.cs` to `Icons.cs` (root namespace, matching its folder) and replaced a `(char)0xE0A0`
  scanner dodge with `AppInfo.DevBadgeGlyph`; the scanner now also flags `(char)0xE...` casts.
- Item chat proved the selftest catches A29: Keyboard set to `0xF6ED` failed with "U+F6ED is
  ic_fluent_person_subtract_16_regular, expected ic_fluent_keyboard_20_regular" (reverted).
- Look hardest at: the contracts "deferred" row handling in `SelfTest/Contracts.cs` and `Run.cs`, menu color and pinned
  fill reference checks, and the scanner's string-content scan.
