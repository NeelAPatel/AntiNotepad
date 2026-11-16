# pstack adoption: what the user agreed (Loop Hub 3, 2026-11-15)

Source plan: `.coord/research/pstack-migration-plan.md` (numbers below are its "###" numbers). Nothing is applied yet. Item numbers are given only on the user's confirmation.

## Agreed (user, 2026-11-15)
| Plan # | Name | Decision |
|---|---|---|
| 3 | Finish line and "inconclusive" (card line + review bullet) | Agreed, do |
| 4 | "Safe because" line on cards | Agreed, do |
| 5 | Rule table (`docs/enforcement.md`, one AGENTS.md bullet) | Agreed, do |
| 7 | Feature map (Settings first, `feature-shape` lint) | Agreed, but WAIT before implementing |

## Not yet decided
| Plan # | Name | Status |
|---|---|---|
| 1 | No-comments policy (reverses the 2026-10-29 comments rule) | Open: full / partial / keep. Plan recommends full. Hold for last. |
| 2 | Comment ratchet lint | Depends on #1 |
| 6 | Contract warning on `fix/` branches (WARN only) | Open; director leaned do |
| 8 | Allow-list expiry dates | Open; director leaned skip |
| 9 | Fix-naming lint messages | Open; director leaned do, with #6 |
| L-e | Guard against detached Codex launches | Open; read `tools/guard-bash.ps1` first (grep found no pattern) |

## Director's suggested landing (not yet agreed)
- One text-only bundle card (#3, #4, #5, maybe #9); run about five items; compare gate misses and rework lines before and after; then decide on comments.
- One card per group so each reverts with one commit. No bulk comment sweep first.

## Revert notes
- Text pieces: revert the commit; old cards still work (no lint requires the new lines).
- New lints: remove the gate step; script is inert.
- Comments: policy revert is one commit; a bulk sweep would be hard to undo, so delete on touch only.

## Next
User will compact and resume the discussion; first open question: draft the bundle as one item?
