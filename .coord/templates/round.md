# <ID-slug>-rN: <what this round fixes>

Rework round of `<ID-slug>` (`docs/loop.md`). Read the item card `.coord/tasks/<ID-slug>.md` first: its Plan, Files,
Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: <same as the card, or raised: why>
Role: <quick | terra | worker | director>, because <one-line reason>

## Why this round
<Exactly what failed or changed: the failing gate lines, the proven review findings, or the user's scope change in their
own words with the date.>

## Change to the plan
<"None" for a fix within the plan. For a scope change: the new "After" and UI checks, approved by the user at Approve.>

## Files
Files added for this round only (the gate allows the card's Files plus these).
- `product/AntiNotepad/<path>`

## Done means
Offline checks only (`docs/agents.md`, "What Codex can and cannot check").
- <observable result>
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug>-rN -Offline` passes.

Report: `.coord/reports/<ID-slug>-rN.md`, in the card's report format.
