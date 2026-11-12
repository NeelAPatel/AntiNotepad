# A10-hotkey-recorder-review: lens review of A10-hotkey-recorder

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`.
Branch: `feat/A10-hotkey-recorder` (already checked out; never switch, merge, commit or edit product files).
Tier: Medium

## Inputs
- Item card: `.coord/tasks/A10-hotkey-recorder.md` (Request, Plan with "After" and "Must not change", Files, Contracts, Approve, Rework r2 and r3 sections)
- Implementer reports: `.coord/reports/A10-hotkey-recorder-r1.md` (round 1) and `.coord/reports/A10-hotkey-recorder.md` (latest, round 3)
- Gate report: `.coord/reports/A10-hotkey-recorder.gate.md` (GATE: PASS)
- Diff: `git diff dev...HEAD -- product`

## Do
Follow `.agents/skills/lens-review/SKILL.md` exactly, including the plan-bounds item. You may build into
`build/A10-hotkey-recorder-review` and run `--selftest`, or add a temporary probe there to prove a finding; delete that folder
after. Never change files under `product/`. Never launch AntiNotepad; no ports, servers, browser or screen checks.
Focus: key mapping and round trip with `GlobalHotkey.HotkeyGesture.Parse`, recording state handling in `UI/HotkeyRecorder.cs` (focus loss, Save/Enter/Esc/Cancel/Clear, failure keeps session open), the Esc guard in `UI/SettingsWindow.cs`, and that registration, stored text format and defaults are unchanged.

## Write: `.coord/reports/A10-hotkey-recorder.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
