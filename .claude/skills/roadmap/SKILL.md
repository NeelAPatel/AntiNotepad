---
name: roadmap
description: Open the AntiNotepad plan, the roadmap Artifact rendered from the STATUS "Plan" section (run order, ID-slug, scope, tier, state). Use when the user says "/roadmap", "roadmap", "show the plan" or "plan table".
---

# /roadmap

The roadmap is an Artifact at the URL in `tools/roadmap.url`. A Stop hook (`tools/roadmap.ps1 -Hook`) regenerates
`.coord/roadmap.html` after every turn and asks for a republish when the plan changed, so it is normally current.

1. Run `pwsh -NoProfile -File tools/roadmap.ps1` (regenerates `.coord/roadmap.html`; cheap). If this session has not read
   or published the artifact yet, read it (`action: "read"` with the URL), then publish `.coord/roadmap.html` with that `url`
   and run `pwsh -NoProfile -File tools/roadmap.ps1 -Mark` so the hook stops asking.
   Otherwise `open` the URL.
2. Do not rewrite, reorder or add rows: STATUS is the source of truth. If something is wrong, fix STATUS and republish.
3. In chat, at most three lines: what is next and what needs the user. Name every item as `<ID>-<slug>` with its scope.
