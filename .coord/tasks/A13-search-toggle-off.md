# A13-search-toggle-off: second click on the Search button closes search

Tier: Low · ui-toggle (fast lane: director scout, no audit, no lab)
State: approved
Branch: `fix/A13-search-toggle-off` in `.claude/worktrees/fix/A13-search-toggle-off` (created at Approve)

## Request
2026-11-12, user: clicking Search opens search; clicking Search again should turn it off. Row: `.coord/pending.md` A13-search-toggle-off ("Start 1 and 4 first").

## Findings
- **Kind and why:** polish/bug; the Search button is a one-way open.
- **Now:** the top-right button calls `OpenSearch()` (`UI/PadWindow.Layout.cs:232`). `OpenSearch` (`PadWindow.Keys.cs:158`) always builds a new `SearchOverlay` and `ShowOverlay`s it, replacing the open one (query reset, fade replays), so the second click looks like nothing. Same for the app-menu "Search notes" entry (`Layout.cs:212`) and Ctrl+F inside an overlay (`Keys.cs:57`). Close paths today: Esc (`Keys.cs:50`, `CloseOverlay`), the overlay's X button (`SearchOverlay.cs:81`), picking a result. Close is `CloseOverlay()` (`Keys.cs:136`), which restores panes/chrome/focus.
- **Where / Risk:** `PadWindow.Keys.cs`, `PadWindow.Layout.cs` only. Risk: low; A14-search-open-fade-up edits `ShowOverlay`/open animation, so keep this out of those lines.
- **Source:** director scout (this sub-chat).

## Plan
- **After:** new `ToggleSearch()` in `Keys.cs`: `if (_overlay is SearchOverlay) CloseOverlay(); else OpenSearch();`. The Search button uses it. Menu entry and Ctrl+F stay "open" (Ctrl+F on an open search keeps today's behavior), unless the user wants them toggling too.
- **Must not change:** open look and animation, Esc, X button, Help overlay, Trash view (a trash search counts as search: a click closes it).
- **Scope questions:** none blocking; Ctrl+F toggle off by default.
- **Test plan:** no selftest: overlays live in `PadWindow` (no selftest constructs one; adding one would need a window harness). Hand test: click Search (opens), click again (closes, panes back, cursor in note), click again (opens fresh), Esc still closes.
- **Gates:** `tools/gate.ps1` (build, selftests, format).
- **Needs from you:** Approve.

## Files
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`

## Contracts
- none (UI toggle, not a bug fix with a testable contract)

## Approve
2026-11-12, user: "approve" (plan as written: only the Search button toggles; Ctrl+F and menu keep opening).
