# A5-search-bar-overlap-audit: findings for A5-search-bar-overlap

Role: director (gpt-6.1-sol medium), read-only. Product `product/AntiNotepad`. You run in the master checkout on `dev`
(no item branch exists before Approve): never switch, merge, commit, or write any files. Never launch or kill AntiNotepad.
Read `AGENTS.md`, then the item card `.coord/tasks/A5-search-bar-overlap.md` (its Request is what the user asked for).

## Answer these (this is the Findings phase of `docs/loop.md`)
1. **Kind and why**: bug, feature, polish or process. What about today's behavior justifies the request?
2. **Now**: what the app does today in the area, with evidence (file:line). Say in plain words what the user sees on dev
   today in Docked, Overlay and Float modes. If dev already does what the Request asks, say so first.
3. **Reproduction (bugs)**: exact code path quoted step by step for (a) the top bar covering the search field in Overlay and
   Float, (b) the window not being draggable from the top zone while search is open. Check the manager's notes on the card
   (`_overlayHost` in row 1 of `_content`, `_topLayer` drawn above it, `OverlayPointer` returning early when an overlay is
   open, hidden `_topLayer` not hit-testable, no title-bar role on the host) and confirm or correct each. Also: is the bar
   shown or hidden when search is opened from the icon, from Ctrl+F, from the pager count? Do not run builds that write files.
4. **Where**: files that would change, callers, shared state, timers. Do HelpOverlay, the save-failure prompt and the
   settings/toast share `_overlayHost` and the same problem? Does UI scale (`_scaler`) change any numbers?
5. **Risk**: the riskiest part, and which gate in `docs/loop.md` ("Gates menu") would catch it.
6. **Tier**: confirm or raise the tier (Medium), with the reason.
7. **Recommendation**: smallest change meeting the Request in all three bar modes (for example a top margin of
   `ChromeTokens.DockedRowHeight` on `_overlayHost` in Sliding modes, plus a drag zone), the "After" state, code tests
   (selftest or CONTRACTS rows) and UI checks that prove it, open scope questions, whether to split. Lab-first: say what a
   lab build or mockup for the user would show.

## Final message: the report
Your final message is the report; never write it to a file. Use sections 1-7 above, short, each claim with its evidence.
End with `READY: yes` or `READY: no - <what is missing>`.
