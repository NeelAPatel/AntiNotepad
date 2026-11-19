# A29-shortcut-map-r3-review: re-review of the rework rounds

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`. Branch `feat/A29-shortcut-map` (checked out; never switch, merge, commit or edit product files). Offline only; never launch or kill AntiNotepad. Build and `--selftest` only into `build/A29-shortcut-map-review2` with a 3-minute limit, then delete that folder (PowerShell `Remove-Item -Recurse -Force` if bash `rm` is refused).
Tier: Medium

## Inputs
- Card `.coord/tasks/A29-shortcut-map.md`; first review `.coord/reports/A29-shortcut-map.review.md` (verdict rework, 5 findings); round briefs `.coord/tasks/A29-shortcut-map-r2.md`, `-r3.md`; reports `.coord/reports/A29-shortcut-map-r2.md`, `-r3.md`; gate `.coord/reports/A29-shortcut-map.gate.md` (PASS).
- Diff of the rework: `git diff 973f281..HEAD -- product docs` (973f281 is the commit the first review read).

## Do
Follow `.agents/skills/lens-review/SKILL.md`, limited to the rework diff plus its callers. Prove with probes where you can:
1. Each of the 5 first-review findings is truly fixed (Shift+Backspace refused and other editor double bindings reserved; paired override fallback keeps Search and New note; slot toast uses the effective Open slot key; no test hook left in product code; summaries present).
2. The r3 Search toggle: Ctrl+F opens, the same key closes while the overlay has focus, then opens again; the Search aliases behave the same; Esc and the toolbar button still work; nothing else in the overlay's keys changed.
3. No new regression: typing path allocation-free, exact modifiers still enforced, dev-slot keys (Ctrl+Alt+Shift+1..9, F1..F9) refused.

## Write: `.coord/reports/A29-shortcut-map.review2.md`
In the lens-review format, short. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
