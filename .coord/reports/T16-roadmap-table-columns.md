# T16-roadmap-table-columns — Codex report

Implemented the approved Plan table changes in `tools/roadmap/index.html`: tier and type badges, type search, eight-column group/release rows, and a narrower branch/era cell with full branch and worktree in the tooltip. Updated the Plan legend; other tabs retain their branch badges.

Checks: `tools/gate.ps1 -Task T16-roadmap-table-columns -Offline` passed (offline NuGet audit disabled for the build); extracted JavaScript passed `node --check`; `git diff --check` passed. The tracked diff is limited to the HTML file. Visual hand testing remains for the item chat on port 47821.
