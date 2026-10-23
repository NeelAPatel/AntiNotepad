# T-015-alignment-fixes: F26, F31, F35

Director: Codex gpt-6.1-sol medium; do it yourself (small).
Branch: feat/alignment-fixes (manager handles git).
Naming rule: refer to items as <ID>-<slug> (F26-quiet-footer-count-alignment etc.) in your report.
Read: AGENTS.md, docs/architecture.md, docs/feedback/TRACKER.md, .coord/reports/T-014.md, docs-ui-audit/evaluation-lenses.md (read only; another session owns that folder), src/AntiQuickPad/Views/.
Write only in src/AntiQuickPad/Views/ (and Editor only if needed), docs/design/m6/built/ (re-renders), .coord/reports/T-015-alignment-fixes.md, .coord/status-T015.md. No Core/ViewModel changes, no git, no STATUS.md, no network.
Build: from src/AntiQuickPad `dotnet bin/Release/net10.0/AntiQuickPad.dll --quit`, then `$env:AVALONIA_TELEMETRY_OPTOUT='1'; dotnet build -c Release --no-restore -p:UseSharedCompilation=false`. Never kill processes.
Finish in ONE turn; end only when the report is written or blocked.

## Items (both shells, light + dark, scale 1 and 1.5)
- F26-quiet-footer-count-alignment: Quiet footer "n of m" sits higher than the ‹ › arrows. Centre text and icon glyphs on one optical midline (icon font glyph boxes differ from text line boxes; align by measured ink bounds, not layout boxes).
- F31-menu-grid-alignment: More menu still not uniform. User screenshot: the hover/selected row background starts further left (about 12 DIP from the edge) than the divider and item text (about 37 DIP); section labels, check column, item text, divider and hover row must share one inset system. Rule: row highlight inset = a fixed outer margin on both sides; check column and text start at fixed offsets inside it; labels and divider start at the text edge (or all at the row edge, pick one and apply everywhere). Equal row heights; label spacing consistent.
- F35-card-footer-vertical-centre: Card footer "n of m" (left) and "All changes saved" (right) are not vertically centred with the floating pager capsule. Put all three on the same centre line.

## Proof (pixels, not code reading)
For each item, render close-ups (both shells where relevant, light + dark, 1x and 1.5x) to docs/design/m6/built/T015/, and MEASURE: ink-bounds vertical centre of text vs icon/pager (<= 0.5 DIP), menu left edges of highlight/check/text/label/divider (report the x of each per row, show they follow the one rule). View every PNG. Selftest: add assertions for these measurements; --selftest exit 0; show-to-focus < 100 ms per shell.

## Done when
Build 0 warnings/0 errors, --selftest exit 0, report per item with measured numbers and PNG paths, list anything that needs the user's eye.
