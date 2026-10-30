# T1-roadmap-overview-page: step T1b, the live overview page

Step brief of `T1-roadmap-overview` (read the card `.coord/tasks/T1-roadmap-overview.md` first, especially "Design").
Committed so far: T1a (`tools/roadmap-data.ps1`, `tools/roadmap-serve.ps1`, `tools/roadmap/eras.json`), gate PASS, live
refresh probe PASS (a STATUS edit showed in `/data` after 3.3 s).

Tier: Low · docs-tools
Role: worker (gpt-6-luna xhigh)

## Why this step
The server answers `/data` but `/` has no page yet. The user approved the design in three mockup rounds; the approved
page is `.coord/tasks/T1-roadmap-overview.mockup.html` with the data shape in `.coord/tasks/T1-roadmap-overview.sample.json`.

## After
1. `tools/roadmap/index.html`: the approved mockup, kept as close as possible in layout, colors, wording and behavior.
   - Remove the mockup-only parts: the yellow "Mockup for T1-roadmap-overview" bar, the "Show example: busy day"
     button, `REAL`/`BUSY` and the `ex`/"example" badge code.
   - Data: no inline data. Fetch `/data` on load and every 3 s (`If-None-Match` with the last `ETag`; a `304` changes
     nothing). Redraw only when the data changed, and keep across redraws: tab, era, filter, search text and focus,
     expanded Work log rows, an open drawer (redrawn for the same item), the legend's open state and the window scroll.
   - Use `eras` from `/data` for the era select and for era membership instead of the hard-coded `8d18f72`; the default
     selection is the newest era.
   - Live pill: "live" while fetches succeed; "offline · retrying" after a failed fetch; back to live on its own.
   - "updated" in the top bar: the `updated` field.
   - Clicking anywhere that is not a control must not throw; no console errors on any tab, with an empty search, with a
     search that matches nothing, in both eras and "All eras".
   - No CDN or external request; one self-contained file; US spelling; light and dark both work; no horizontal page
     scroll at 375 px.
2. `tools/roadmap-serve.ps1`: serve the page from the script's own folder (`$PSScriptRoot/roadmap/index.html`), not
   from the data root, so a worktree serves its own page while reading the main checkout's data. Send it with
   `Cache-Control: no-cache`. Nothing else in the server changes.

## Files
- `tools/roadmap/index.html`
- `tools/roadmap-serve.ps1`

## Done means
- Served by `tools/roadmap-serve.ps1 -Port 47893` from this worktree: `/` returns the page; a headless check is not
  available, so list in the report what you verified by reading the code against each bullet above.
- `pwsh -NoProfile -File tools/roadmap-data.ps1 -Check` still prints `ROADMAP DATA OK`.
- `pwsh -NoProfile -File tools/gate.ps1 -Task T1-roadmap-overview -Offline` passes.
The loop chat then checks the page in a browser (console, both themes, phone width, state kept across a data change).

## Constraints
Work in this worktree (`chore/T1-roadmap-overview`). Never switch branches, merge, commit or push; never edit
`.coord/STATUS.md`, the card, the references, or anything under `product/`. Never launch or kill AntiNotepad. Stop any
server you start. No new libraries. Comments and formatting follow `docs/coding.md`.

Report: `.coord/reports/T1-roadmap-overview-page.md` with the item template's report sections.
