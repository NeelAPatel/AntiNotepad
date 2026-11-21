# A35d-spacing-tokens lab, round 3, part search

Role: terra, one overlay with several states.

First read `.coord/tasks/A35d-spacing-tokens.lab3-common.md` in full and follow it. Write in small saved steps.

**Your files:** `tools/playground/a35d-lab/search.js`, defining `LAB.overlays.search = { mount(host, ctx) }`, which fills `host` and returns `{ unmount() }`.
**Source to copy:** `UI/SearchOverlay.cs` and the search result types in `Model/Search/`.
**Captures to match:** `search-empty-*`, `search-results-*`, `docs/playground/shots/search-*`.
**Snap rows you wire:** 9, 10 and 11.

**Must work:** typing filters `ctx.notes` with highlighted matches; arrow keys and hover move the current result; Enter or click calls `ctx.openNote(i)`; list and grid view; trash view; the close button; the footer hint text.

Report: `.coord/reports/A35d-spacing-tokens-lab3-search.md`.
