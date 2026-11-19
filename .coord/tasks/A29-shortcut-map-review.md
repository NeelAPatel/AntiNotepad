# A29-shortcut-map-review: lens review of A29-shortcut-map

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`.
Branch: `feat/A29-shortcut-map` (already checked out; never switch, merge, commit or edit product files). Offline only; never launch or kill AntiNotepad (build and `--selftest` into `build/A29-shortcut-map-review` only, 3-minute limit, delete the folder after).
Tier: Medium

## Inputs
- Item card: `.coord/tasks/A29-shortcut-map.md` (Request, Plan "After" and "Must not change", Files, Contracts, Approve).
- Reports: `.coord/reports/A29-shortcut-map-gatefix.md`. There is NO main implementer report (the run timed out), so you also judge the Change list, behavior changes and plan deviations yourself from the diff; list them in your report.
- Gate report: `.coord/reports/A29-shortcut-map.gate.md` (PASS).
- Diff: `git diff dev...HEAD -- product docs AGENTS.md`. Key research: `.coord/research/A29-key-inventory.md`, `.coord/research/A29-key-audit.md`.

## Do
Follow `.agents/skills/lens-review/SKILL.md` exactly, including the plan-bounds item. Prove findings with a probe in the review build folder only. Pay extra attention to:
1. Every default key in the card's Plan fires exactly once and with exact modifiers (Ctrl+Shift+, must not open Settings); no key now handled twice by window and editor.
2. Keys removed from defaults versus dev: list every one (Ctrl+P, Ctrl+K, Ctrl+PageUp/Down, Ctrl+, Ctrl+0, Ctrl+~, Ctrl+1..9, Alt+Shift+Up/Down, Ctrl+Shift+Backspace and others) and confirm none still fires by accident.
3. Typing speed: key lookup allocation-free and one lookup per key; nothing slow added to the text-input path.
4. Settings: save, load, clear, reset, bad or duplicate saved values fall back; Settings page Record/Clear/Reset/Reset all and the conflict prompt ("take it from X"); recorder refuses text-editing keys, Windows keys, Ctrl+Alt+Shift+1..9 and F1..F9.
5. Help, app menu labels and tooltips read the map (no hand-typed key left); `docs/shortcuts.md` table equals the map.
6. Must not change: global hotkeys and their Settings rows, save and shutdown code (A30), A31 and A32 files.
7. Coding standard (`docs/coding.md`): file headers, summaries, no history in comments.

## Write: `.coord/reports/A29-shortcut-map.review.md`
In the format the skill gives, plus a short "Change list and behavior changes" section for the missing report. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
