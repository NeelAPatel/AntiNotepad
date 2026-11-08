# L6-lv4-docs-diet-r2

## Summary
Restored the requested legacy loop rules and wording in the core. Behavior changes: None.

## Change list
- Restored phase rows and notes, tier table, status card, retained core rules, IDs/numbering, core session bullets, and triggers from `dev:docs/loop.md`.
- Kept v5.3.0, the L-prefix additions, and the detailed session material in `docs/loop-sessions.md`.
- Reduced `docs/coding.md` to its two-line pointer.

## Behavior changes
None.

## New byte sizes
- `docs/loop.md`: 15,684 bytes.
- `docs/coding.md`: 125 bytes.

## Files
- `docs/loop.md`
- `docs/coding.md`
- `.coord/reports/L6-lv4-docs-diet-r2.md`

## Checks run
- `pwsh -NoProfile -File tools/loop-lint.ps1` — PASS.
- `pwsh -NoProfile -File tools/gate.ps1 -Task L6-lv4-docs-diet-r2 -Offline` — FAIL: the rework report was missing `## Files` when the gate first ran; rerun after adding this list.
