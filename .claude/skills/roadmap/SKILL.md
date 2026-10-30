---
name: roadmap
description: Open the AntiNotepad project overview served live from STATUS and Git. Use when the user says "/roadmap", "roadmap", "show the plan" or "plan table".
---

# /roadmap

The roadmap is served live from the main checkout at `http://localhost:47811`.

1. From the main checkout, run `pwsh -NoProfile -File tools/roadmap-serve.ps1 -Background -Open` (starts or reuses the
   server, then opens `http://localhost:47811`).
2. Do not rewrite, reorder or add rows: STATUS is the source of truth. If something is wrong, fix STATUS.
3. In chat, at most three lines: what is next and what needs the user. Name every item as `<ID>-<slug>` with its scope.

Only when the user asks for it elsewhere (phone): run `tools/roadmap-data.ps1 -Snapshot <scratch file>` and publish that
file to the Artifact URL in `tools/roadmap.url` (read it first if this session has not).
