---
name: roadmap
description: Open the AntiNotepad plan, the local roadmap page rendered from the STATUS "Plan" section (run order, ID-slug, scope, tier, state). Use when the user says "/roadmap", "roadmap", "show the plan" or "plan table".
---

# /roadmap

The roadmap is the local page `.coord/roadmap.html`. Claude hooks (`tools/roadmap.ps1 -Hook`, on every Edit/Write and at
the end of every turn) rebuild it from STATUS, and the page reloads itself every 3 seconds, so an open tab stays current.

1. Run `pwsh -NoProfile -File tools/roadmap.ps1 -Open` (rebuilds and opens the page).
2. Do not rewrite, reorder or add rows: STATUS is the source of truth. If something is wrong, fix STATUS.
3. In chat, at most three lines: what is next and what needs the user. Name every item as `<ID>-<slug>` with its scope.

Only when the user asks for it elsewhere (phone): publish `.coord/roadmap.html` to the Artifact URL in `tools/roadmap.url`
(read it first if this session has not).
