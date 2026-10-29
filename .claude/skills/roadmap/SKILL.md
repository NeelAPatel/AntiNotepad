---
name: roadmap
description: Show the AntiNotepad plan as a rendered table (run order, ID-slug, scope, tier, state) straight from the STATUS "Plan" section. Use when the user says "/roadmap", "roadmap", "show the plan" or "plan table".
---

# /roadmap

1. Run `pwsh -NoProfile -File tools/roadmap.ps1 -Fragment` (prints an HTML fragment; it also stays the source of the user's
   own copy, `.coord/roadmap.html`, which the hook regenerates on every STATUS edit).
2. Pass the output verbatim to `show_widget` (title `antinotepad_roadmap`). Do not rewrite, reorder or add rows: STATUS is
   the source of truth. If something in the table is wrong, fix STATUS and run again.
3. Below it, at most three lines: what is next and what needs the user. Name every item as `<ID>-<slug>` with its scope.
