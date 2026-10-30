# T1-roadmap-overview-page

## Summary
Added the approved roadmap overview as a single live page and changed the server to read that page from its own worktree folder. The data check and offline gate pass; the current environment prevents `HttpListener` from binding, so the requested served-page probe remains unverified.

## Change list
| File and lines | Change | Plan line served |
|---|---|---|
| `tools/roadmap/index.html:8-218` | Approved layout, light/dark styling, responsive tables and drawer; removed mockup bar and example control. | Approved mockup minus mockup-only parts; one self-contained page; both themes and 375 px layout. |
| `tools/roadmap/index.html:221-327` | Added `/data`-driven state, derived era membership and a dynamic era selector defaulting to the newest era. | Poll live data; use `eras` from `/data`; show the `updated` field. |
| `tools/roadmap/index.html:328-424` | Kept the four data-driven views and included visible row fields in search text. | Plan, Work log, Branches & worktrees, Releases; searchable columns. |
| `tools/roadmap/index.html:425-623` | Added ETag polling, retry/offline status, redraw state preservation, safe delegated clicks, and retained drawer/legend state. | Redraw only on data change; preserve UI state; recover from fetch failures; avoid control-click errors. |
| `tools/roadmap-serve.ps1:304` | Resolve the page from `$PSScriptRoot/roadmap/index.html`; the existing response helper sends `Cache-Control: no-cache`. | Serve this worktree's page while keeping data-root resolution; no other server behavior changed. |

## Behavior changes
- The page starts in the newest configured era, displays era names from `/data`, and classifies completed Plan rows from matching log entries' era values. Active rows use the newest configured era.
- The page requests `/data` on load and on a three-second cadence, sends its last ETag, treats `304` as unchanged data, and redraws only when the JSON payload changes. Requests abort after 10 seconds; failures show `offline · retrying` and retry automatically.
- Data redraws retain the tab, era, filter, search and search caret, expanded Work log groups, open drawer and its expanded sections, open legend, and window scroll position. The top bar shows `Updated` plus the data's `updated` value.
- No product API or contract changed. Theme choice remains stored per browser.

## Out-of-scope files
None. Only the two assigned files are modified; the requested report is here.

## Plan deviations
- The `/` and `/data` HTTP probe could not run because the current PowerShell environment rejects `HttpListener.Start()` with `HttpListenerException (6): The handle is invalid`. The port was not occupied, and no server process remained. The server route and cache header were checked in code, but a live response was not observed.
- No headless browser is available in this step, so visual layout, browser console, and state persistence remain for the loop chat's hand check.

## Contracts
None.

## Lens review
- Findings (proven): None.
- Behavior changes check: Matches this report; no product methods or shared product state changed.
- Scope and plan check: Both changed files are listed in the step brief. The response helper already applies `Cache-Control: no-cache`; only the page path changed in the server.
- Unproven risks: Browser rendering and interaction behavior have not been exercised in a browser. Live route behavior remains unverified because `HttpListener` could not start in this environment.
- Checks run: JavaScript syntax parse passed; data check and offline gate passed; attempted server start failed as described above.
- VERDICT: accept (implementation); live route and UI hand checks remain pending.

## Checks run
- `pwsh -NoProfile -File tools/roadmap-data.ps1 -Check`: `ROADMAP DATA OK` (70 rows, 8 groups, 169 log entries, 5 lanes, 4 worktrees, 34 tags, 37 cards, 0 warnings).
- `pwsh -NoProfile -File tools/gate.ps1 -Task T1-roadmap-overview -Offline`: `GATE: PASS`; build had 0 warnings and 0 errors; `SELFTEST PASS`.
- Extracted inline script parsed with Node.js: `JavaScript syntax OK`.
- `git diff --check`: passed.
- `pwsh -NoProfile -File tools/roadmap-serve.ps1 -Port 47893`: failed to bind; direct listener probe confirmed `HttpListenerException (6): The handle is invalid`. No process remained on port 47893.

## Hand checks for the user
- Start the server on port 47893 and confirm `/` serves this worktree's page and `/data` responds; confirm the live pill and `Updated` value.
- In a browser, check the console on each tab with empty search and a no-match search in both eras and All eras; test light and dark themes and 375 px width.
- Open a Work log group, drawer and legend; change the source data and confirm tab, era, filter, search/focus, expanded row, drawer, legend and scroll stay in place.
