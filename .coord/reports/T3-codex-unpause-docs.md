# T3-codex-unpause-docs report

Built by the Claude builder session, no Codex (fast lane: a few doc lines).

## Summary
Removed the live "Codex is paused" rule from the three docs that carried it. Nothing else reworded.

## Change list
- `AGENTS.md` Roles: removed the "**Codex is paused** ... apply when it returns." bullet (2 lines).
- `docs/loop.md` Rules: removed the "**Codex is paused** ... wait for its return." bullet (2 lines).
- `docs/agents.md` header: removed the "**Paused** ..." paragraph (2 lines) and the extra blank line.
- No history line added (the card allowed either).

## Checks
- `rg -n -i "pause|when it returns|its return" AGENTS.md docs/loop.md docs/agents.md`: no matches.
- `tools/gate.ps1 -Task T3-codex-unpause-docs`: see the card's `## Check`.
- Not touched: STATUS "Working rules" (Master 2), T2's files.
