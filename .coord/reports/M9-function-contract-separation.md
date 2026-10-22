# M9-function-contract-separation: overnight run report (2026-10-21)

Manager: Claude Opus 5.5 (unattended). Branch: feat/function-contract-separation. Scope tonight: parts a, d, e, then b. Part c (Editor/Carousel behaviour vs presentation split) remains.

## Progress
- Pre-check on unchanged branch (94c0f6f): Release build 0 warnings, --selftest PASS.

## Decisions made unattended
1. Docs (part a `docs/function-contract.md`, part e AGENTS.md rule) written by the manager, not Codex: the Codex budget is 6 runs (director 3, worker 2, backup 1), so all of it goes to code (F37-hidden-selftest-windows, part d, part b).
2. Codex runs in a workspace-write sandbox and does no git. The manager reviews and commits each step. For part b sub-steps inside one run, Codex snapshots the source tree under `.coord/scratch/` (gitignored) after each passing sub-step, so the manager can commit them one by one.
3. Gate runs set `ANTIQUICKPAD_ALIGNMENT_OUTPUT` to `.coord/scratch/align`, so the selftest's alignment PNGs never change tracked files under docs/design.
4. Command IDs use `area.verb` (`note.create`, `app.hide`, ...). "Required" is split into `yes` (each shell must expose a route) and `shared` (supplied by the resident host's keys, gestures, tray or system for every shell). Conformance passes if any route for an ID is live in the active shell.
5. ClassicShellWindow (the retained `--shell classic` fallback) is left out of the part b extraction to keep scope tight. It keeps its own lifecycle code.
6. F37-hidden-selftest-windows: accepted Codex's choices (measurement smoke dropped from selftest, since the gate runs real --measure; Classic maximize step dropped, since native maximize lands on a monitor). Reverted one Codex change: it had moved selftest runtime data into the repo's `.coord/scratch` via a `../../../../..` path. Restored `RuntimePaths.ForSelfTest()` (bin/selftest), which works outside the repo.
7. Part d: accepted `window.move` as a reported KNOWN-GAP rather than changing a layout (UI is on hold). It is Required=yes, but its only route is the native title-bar drag gesture, and the brief's extra rule wanted a toolbar, menu or hotkey route too. The gesture is still checked live in both shells. Shell routes for keys, tray and system are capability checks (factory or handler present), not physical input; the report says so.
8. Gate measure for part d: 22.9 ms median, p95 25.2-25.7 ms (baseline 22.6 / 24.5). Within budget and the gate.
9. Part b ran as ONE Codex director run (3/3) doing four sub-steps with a source snapshot after each passing gate, rather than one run per sub-step. That kept worker and backup runs in reserve for fixes; none were needed. Codex hit one build error in b4 (missing using) and fixed it on the first attempt.
10. The manager re-gated every snapshot on the desktop (build, selftest, conformance, two `--measure` runs each) before committing it. No revert was needed.
11. Reviewed the extracted code against the original for behaviour equivalence: same constructor subscription order, timers (350 / 130 ms), log lines, Navigating/Navigated order, save-failure paths, and the Activated/Deactivated/Closing/Closed handlers (now forwarded to the controller). The QuitResidentTest startup hide now goes through `IWindowPlatform.Hide`, which calls the same `Window.Hide`. Minor leftovers kept, to avoid behaviour risk: a `_binding` property alias in the window, unused `_count`/`_status`/`_pin` fields that predate tonight, and `ResidentController.HasAutosave` used only by the conformance resolver.
12. Updated `docs/architecture.md` with the Controller layer (Views -> Controller -> ViewModels -> Core).

13. F37 follow-up: `--probe-backdrop` restored to visible and interactive (no hiding, closes only on `--close-after-ms` or a user close). It is the window for the F25 live-look check, not a selftest. Re-gated: 0 warnings, selftest PASS.
14. Baseline note corrected: Classic also measures 21.5 ms tonight, so the 11 -> 22.6 ms change since M5 is not caused by the Quiet shell or by tonight's work. Unexplained; recorded as an open question.

## Results
| Step | Commit | Gate |
| --- | --- | --- |
| F37-hidden-selftest-windows | de8d991 | 0 warnings, selftest PASS, foreground unchanged |
| Baseline | 21c3e2f | 22.6 ms median / 24.5 ms p95 (Quiet) |
| a contract table | a417ed6 | doc |
| d conformance check | 1f8fbc6 | quiet/card PASS (24 commands, window.move KNOWN-GAP); measure 22.9 / 25.4 |
| e AGENTS rule | 0f0d85c | doc |
| b1 autosave | 00f4bb7 | PASS; measure 22.6-23.4 / 24.7-25.1 |
| b2 navigation | d782292 | PASS; measure 22.6-22.9 / 24.4-24.9 |
| b3 show/hide/focus/quit | 3aeaa85 | PASS; measure 22.4-22.7 / 24.6-25.0 |
| b4 lifecycle + command map | 49401b9 | PASS; measure 22.8-22.9 / 25.3-25.9 |

Reverted: nothing (inside F37 the manager undid one Codex path change; see decision 6). Remaining for M9: part c (Editor/Carousel behaviour vs presentation split). Physical checks still needed (see HANDOFF).

## Part c (2026-10-21, user awake, asked for part c now; no merge)
15. Codex budget tag `day-2026-10-21` for part c. The night tag's director runs were spent, and this is a new user request.
16. Part c keeps the public surface of `EditorFactory` and `CarouselController` as thin compositions, so callers, selftests and the conformance check stay unchanged. The pixel-identity gate uses `--render-shell` before/after renders on top of the selftest's pixel proofs.
17. Part c speed gate re-based on a same-session control. With the user active on the machine, c1 measured 28.8-29.1 ms median, over the night-baseline limit of 27 ms, and hide time also rose (12 -> 17 ms) though c1 doesn't touch hide. A control build of the unchanged pre-c1 code (git worktree `../AntiQuickPad-control`, HEAD 3e1eee8), measured right after, gave 27.5 ms median, 29.4-34.4 ms p95 and 19 ms hide. So the environment moved about 5 ms, and c1 is about 1.5 ms (5%) above control. New limit for part c: at most 20% over control, i.e. median 33 ms or less, p95 41 ms or less. The Codex run's exit 1 was a "model at capacity" error on its final message, after both sub-steps had passed their gates and the report was written. Its work was accepted after review.
18. Part c landed: c1 editor abf1a42 (27.3-27.6 ms median), c2 carousel 256e1b7 (27.1-27.8 ms median), against a same-session control of 27.5 ms. Codex reported all 28 render PNGs byte-identical before and after; selftest pixel proofs and conformance pass. All M9 parts (a-e) are now built. The physical check for part c is still needed (wheel/drag feel, checkbox hover/click, folding, keys).
19. F53-folded-chevron-overlap (user report 2026-10-21, pre-existing): ChevronMargin matched any visual line containing a fold start, so hidden nested folds drew chevrons and click targets on the collapsed line. It now matches only a visual line's first document line. The selftest assertion was shown to fail before the fix. Gate: 0 warnings, selftest + conformance PASS, 28.6 ms median (user's new limit is 30 ms).
20. M9h-ui-ready-hardening (user: do the simple separation follow-ups; next phase is the UI overhaul): h1 VM projections (02f9035), h2 bind by command ID (5b63296), h3 IL boundary check in selftest (b678b96, BOUNDARY PASS 218 types, no known violations). Codex reported renders byte-identical at each step.
21. Manager fix after review (own commit): the new projections raised 2 extra VM notifications per keystroke and 8 per navigation, and each re-ran the full status refresh (Card rebuilds up to 8 dot buttons). The controller now skips projection names, restoring the original refresh count.
22. --measure dropped from gates (user: no launching tests for now). An orphaned measure child from a disturbed h1 measure run was locking the build. It was quit gracefully via its single-instance channel (`--selftest-quit <id>`), not killed. A safety check also blocked a scripted tree reset; h2 already matched the tree, and h3 only overwrote files (verified no deletions).
23. Docs: UI-overhaul guide added to docs/function-contract.md ("Building a new shell").
24. Merged feat/function-contract-separation into dev with --no-ff (user 2026-10-21: finish commits and merge back to dev).
