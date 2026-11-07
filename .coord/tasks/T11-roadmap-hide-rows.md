# T11-roadmap-hide-rows: hide and show Plan rows on the roadmap page

Item card (`docs/loop.md`), fast lane (side task, `docs/loop.md` "Rules"). Cheapest run: no review step, no tester.

Tier: Low · docs-tools (side tool, does not ship), roadmap portal
Branch: `chore/T11-roadmap-hide-rows`

## Request
User to Roadmap Hub 1, 2026-11-06:
- "when hovering on a small white space area on the left side of a row, reveal a Hide button.
  Hide = Hide from All tab, it will only show up in any of the other tabs such as active/planned/proposed etc.
  Consequentially, on those tabs, I want a Show button instead. Goal: Lets me hide and show items as i please"
- "All tab's count should show <total visible> - hidden eye symbol <hidden items>"
- "Hiding applicability: standalone rows, no parent or child items; child items; Parent items can be hidden if theres no
  child items left to work on, done, wont fix etc,"
- "Use the dev loop in the most minimally usage cost manner. roadmap is NOT a primary app ... simply make a worktree and
  branch to work on this and launch the webapp iwth dev-worktree-<slug> somewhere in the header so i can compare before
  and after."

## Findings
- **Kind and why:** feature; the user wants to declutter the Plan view's "All" filter.
- **Now:** `tools/roadmap/index.html` Plan tab has filter chips (`state.filter`: `all`, `active`, and per-kind filters;
  counts in the Plan view near line 340). Parent rows are `idea:` rows with `step:` rows under them. Theme choice is
  already kept in `localStorage` (`roadmap-theme`), the pattern to follow. The page redraws every 3 s from `/data`
  without losing state.
- **Where:** `tools/roadmap/index.html`; `tools/roadmap-serve.ps1` only for the header label.
- **Risk:** low; page-only state.

## Plan
- **After:**
  - Each Plan row gets a narrow gutter on its left; hovering it reveals a small button. On the "All" filter it says
    **Hide**; on every other filter (active, planned, proposed, ...) a hidden row still shows and its button says
    **Show** (visible rows on those filters need no button, or show Hide, builder's choice; keep it quiet).
  - A hidden row disappears from "All" only. Clicking Show un-hides it.
  - Who can be hidden: standalone rows (no parent, no children) and child (`step:`) rows always; a parent row only when
    none of its children is still open (all `done`, `dropped`, or otherwise closed). Rows that cannot be hidden show no
    button.
  - Hidden IDs are kept in `localStorage` (key e.g. `roadmap-hidden`), read and written inside try/catch; they survive
    the 3 s redraw and server restarts. Per-browser only (decided by Roadmap Hub 1; the user may ask for repo storage).
  - The "All" chip count reads `<visible> · 👁 <hidden>` style: total visible, then an eye symbol with the hidden count
    (use a slashed-eye glyph or small inline SVG; omit the hidden part when 0).
  - Header label: `roadmap-serve.ps1` gets an optional `-Label <text>` that the page shows as a small badge in the header
    (sent through `/data` or a header); with no label nothing changes. Dev runs use
    `-Label dev-worktree-roadmap-hide-rows`.
- **Expected time:** about 30 minutes.
- **Must not change:** data JSON beyond the optional label, other tabs, filters' existing counts other than All, the
  3 s poll, port defaults, the live reload from T10-roadmap-live-reload.
- **Test plan, code:** Codex offline: page script passes `node --check` on the extracted inline script; serve script
  parses.
- **Test plan, UI:** builder starts the worktree server on port 47820 with the label and gives the user the link; the
  user compares it with the live page on 47811. That look is the only check.
- **Gates:** `tools/gate.ps1 -Task T11-roadmap-hide-rows -Offline`: scope, loop-lint.
- **Needs from you:** a look at the test page.

## Files
- `tools/roadmap/index.html`
- `tools/roadmap-serve.ps1`

## Contracts
None.

## Approve
User to Roadmap Hub 1, 2026-11-06: the request above ("simply make a worktree and branch to work on this") and routing
"Haiku + Codex".

## For Codex
Role: quick (gpt-6-luna medium), a contained page change in two known files. Offline checks only; Codex cannot bind ports
(the builder runs the test server). Edit only the two files under `## Files`; do not commit (Codex cannot write to .git).

Done means:
- Hide/Show gutter button and the All-count eye work as in `## Plan`; hidden ids in `localStorage` key `roadmap-hidden`,
  all access in try/catch.
- Page's inline script passes `node --check` (extract the `<script>` body to a temp file outside the repo).
- `tools/roadmap-serve.ps1` parses (`[System.Management.Automation.Language.Parser]::ParseFile`), and `-Label` is optional.
- Report at `.coord/reports/T11-roadmap-hide-rows.md` with sections: Changed, Checks run, Open questions.

## Check
- Codex (quick, Luna medium) built the change; report `.coord/reports/T11-roadmap-hide-rows.md`.
- Builder re-ran `tools/gate.ps1 -Task T11-roadmap-hide-rows -Offline`: GATE PASS (Build, Selftest, Seam, Tokens, Format,
  Scope, LoopLint, Size).
- Test server on http://localhost:47820 with label `dev-worktree-roadmap-hide-rows`: HTTP 200. Live page 47811 also 200
  and was not touched.
- Parent/child grouping uses row order within a STATUS group (no explicit parent field in the data); noted in the report.

## Hand test
Passed. User, 2026-11-06, relayed by Roadmap Hub 1 in their words: "testing is complete. i approve". Confirmed again in
the builder chat: "yes, run the merge prep". Checks covered on http://localhost:47820 next to http://localhost:47811:
- Hover the left gutter of a standalone row on All: Hide appears; click it, the row leaves All.
- Open another filter (Active, Planned, Proposed): the row shows with Show; click it, it returns to All.
- The All count reads visible count with the eye and hidden count.
