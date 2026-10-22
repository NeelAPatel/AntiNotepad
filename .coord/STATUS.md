# STATUS (update at every dispatch / milestone / completion)

Phase: on dev. Function phase done (M9-function-contract-separation merged and signed off by the user 2026-10-21). Next: UI overhaul (user), UI-only, on feat/ui-overhaul in a new Opus session; base theme Quiet (Card too different). Gates for now: build + hidden selftest (conformance, boundary, pixel proofs); no --measure (user). Function phase first (user 2026-10-21): finish core function on the current UI, then M9-function-contract-separation, then the UI overhaul as UI-only work. UI-phase items are on hold.
Updated: 2026-10-21

## Running now
M8-antinote-ui-overhaul kickoff (2026-10-21, new Opus manager on feat/ui-overhaul): user answered (see decisions log 2026-10-21): AntiNote reference; identity = chrome-free + top/bottom hover bars + translucent background; Card retire; order A (F25 -> renders -> M6c -> M6d); ui-auditor + captures parked. Priority re-cut on hold until the new UI is mostly in place (user; many held items expected to drop). User notes received: docs/design/antinote-reference-2026-10-21.md (UI targets + function ideas). Feasibility (manager, 2026-10-21): Avalonia confirmed; translucency is the one unknown. F25a-resident-backdrop-probe DONE (Codex director, day-2026-10-21 run 3/3; manager re-gated: 0 warnings, selftest PASS, BOUNDARY + CONFORMANCE PASS; report .coord/reports/F25a-resident-backdrop-probe.md). F25-translucency-live-look DONE (user 2026-10-21): Acrylic base (pick: light, tint 90%), Mica option (0% still tinted), Solid fallback, full transparent dropped; GPU use to measure later. M8a-quiet-variant-renders DONE (attempt 3, Codex backup gpt-5.6-luna xhigh; attempts 1-2 failed: sandbox screen read denied, then compile errors). Manager ran glass check (PASS, inactive path, no focus taken) and 20 captures outside the sandbox; re-gated 0 warnings, selftest PASS. Renders docs/design/m8/ (index.html). User picked V1-compact-pill (2026-10-21). Next: M8b-retire-card-function, then M8c-v1-compact-shell. User 2026-10-21: run today on a fresh budget tag (eve-2026-10-21); one --measure run approved after the Acrylic shell lands. M8b-retire-card-function DONE (Codex director eve 1/3; manager re-gated 0 warnings, selftest PASS incl. SETTINGS/COMMAND smokes; Core.Smoke expectation updated by manager). M8c-v1-compact-shell DONE (Codex director eve 2/3; c1-c4 gated; manager re-gated 0 warnings, selftest PASS incl. REVEAL + PRESENTATION). --measure (approved): baseline median 29.97 ms / p95 44.5 ms, hidden idle CPU 0.8 %; heavy stage blocked by F54-measure-heavy-index-crash (MainViewModel.Current index out of range while notes are added; function task, P1). User physical check PASS; tagged baseline/m8c-v1-compact. Usage: Claude first for the rest of 2026-10-21 (user). F54-measure-heavy-index-crash DONE by manager (gate PASS). Held-item triage applied (user). M6c-design-tokens DONE (manager; step 1 ca91368 byte-identical renders, step 2 accessibility; selftest PASS incl. TOKENS). User test 2026-10-22 (docs/feedback/test-log-2026-10-21-v1-tokens.md): pass except F56, F57 (fixed, re-check), F58. M11-settings-window DONE (user: fine for now; F59 navigation-view overhaul later). Next: M6d-motion-system; F58; M7 data folder; --measure re-run needs user OK.
Previous: feat/function-contract-separation merged into dev 2026-10-21 (--no-ff). Next phase: UI overhaul (user), as UI-only work per docs/function-contract.md.

## Done
- M9h-ui-ready-hardening (2026-10-21): VM projections, bind by command ID, selftest IL boundary check (BOUNDARY PASS), refresh-count fix; UI-overhaul guide in docs/function-contract.md. Merged to dev with all of M9 and F37, F53.
- M9 part c (2026-10-21, Codex director day-2026-10-21 run 1/3): Editor/Behaviour vs Editor/Presentation (c1 abf1a42), CarouselMotion vs CarouselView (c2 256e1b7). Renders byte-identical; selftest + conformance PASS; measure 27.1-27.8 ms vs same-session control 27.5 ms (machine in use; night baseline 22.6). Needs user physical check.
- M9-function-contract-separation parts a, b, d, e (overnight 2026-10-21, branch feat/function-contract-separation, last commit 49401b9 + bookkeeping): docs/function-contract.md (24 command IDs, owners, routes, ownership); Controller/ResidentController owns lifecycle, debounce, navigation, focus/cue, show/hide/quit, key + command map; selftest conformance check quiet/card PASS; AGENTS rule. Every step gated: 0 warnings, selftest PASS, measure 22.4-23.4 ms median vs 22.6 baseline. Part c remains. Pending user physical check. Report .coord/reports/M9-function-contract-separation.md.
- F37-hidden-selftest-windows (2026-10-21 night, Codex director run 1/3): test/render/design windows off-screen + DWM-cloaked, never activated, not in taskbar; selftest asserts it (foreground unchanged). Measurement smoke and Classic maximize step dropped from selftest. Manager kept selftest data in bin/selftest. Gate: 0 warnings, --selftest PASS. Awaiting user check.
- T-005 (M3 editor: markdown, python fences, checkbox widgets, Tab/Shift+Tab, folding; M4 carousel: wheel notch = 1 page 220 ms, grab-strip drag 1:1 + snap, n/count indicator, Ctrl+Shift+Delete). Manager re-verified in alt output: 0 warnings, --selftest PASS, show-to-focus 13-15 ms scripted.
- T-004 (M0 skeleton, M1 shell/hotkey/tray/single-instance, M2 notes persistence). Report .coord/reports/T-004.md. Manager re-verified: Release build 0 warnings, --selftest PASS, internal show-to-focus 7-9 ms (scripted, not physical hotkey-to-paint).

## Decisions
- Speed: show-to-focus regression gate = median at most 30 ms (user 2026-10-21; budget stays < 100 ms); hotkey Ctrl+Alt+Shift+A (override: ANTIQUICKPAD_HOTKEY env); one notch = one page.
- Build needs AVALONIA_TELEMETRY_OPTOUT=1 inside the Codex sandbox.

## Waiting on user
- M1 physical check: hotkey show/hide PASS, single instance PASS, tray works well (user 2026-10-21). Relaunch persistence works (user 2026-10-21, "for now").
- Product checklist: docs/product-checklist.md (view of the tracker, UI-agnostic).
- T-007 + strike follow-up: user PASS (all items).

## Priority (re-cut 2026-10-21: function phase first, UI phase on hold)
Big task: M9-function-contract-separation (user's name: "Function contract separation").
P0: user physical check PASSED 2026-10-21 (hotkey, focus, autosave/relaunch, wheel/drag, folding, tray); F37 verified live by manager; then merge feat/function-contract-separation to dev (user say-so); M9 part c done 2026-10-21, awaiting physical check; next P1 after merge: M7-settings, F44-delete-placement-and-undo (undo half).
P1: M7-settings (storage + commands only), F44-delete-placement-and-undo (undo part).
P2: F14-wheel-threshold (needs user spec), F15 (user decides), M10-search-across-notes (not in scope yet).
On hold for the UI phase (rework on the separated format): M6b, M6c, M6d, M8, F25, F52. Folded into those rows: F4, F5, F21-F23, F26, F31, F33, F35, F36, F38-F43, F45-F51, F9, F17. Originals in docs/feedback/held-ui-items.md.

## Superseded priority (2026-10-18)
P0 now
1. F37-hidden-selftest-windows (tests must not pop windows over the user's pad).
2. P1 behaviour bugs that survive any UI: F49-menu-focus-hover-double-highlight, F50-last-lines-clipped, F40-empty-note-caret-vs-hint, F46-slide-landing-hitch-and-jump.
3. F25-translucency-live-look: 5-minute user check of the Mica/Acrylic probe; decides the overhaul's base look.
P1 foundations for the overhaul
4. M6c-design-tokens incl. F42-target-size, F43-contrast, F51-focus-visible.
5. M6d-motion-system incl. F45-slide-timing, F47-boundary-feedback, F21-23, F33, F5.
6. Overhaul design brief (antinote-style), built on tokens + motion.
P2 later
7. M7-settings (F9, F17), F14-wheel-threshold (needs user spec), F44-delete-placement-and-undo.
Deferred to overhaul: F26, F31, F35, F36, F38, F39, F41, F48, M6b.
