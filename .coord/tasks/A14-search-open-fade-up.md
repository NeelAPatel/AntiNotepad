# A14-search-open-fade-up: Search fades up when it opens

Tier: Low · ui-motion (lab-first)
State: plan ready
Branch: `feat/A14-search-open-fade-up` in `.claude/worktrees/feat/A14-search-open-fade-up` (created at Approve)

## Request
2026-11-12, user: opening Search from the top-right cluster has no animation; wanted: it fades up from the bottom. Use Windows 11 / WinUI 3 easing and duration. Row: `.coord/pending.md` A14-search-open-fade-up ("Start 1 and 4 first").

## Findings
- **Kind and why:** polish; first of a motion list that will reuse the same Win11 values.
- **Now:** Search button -> `OpenSearch` (`PadWindow.Keys.cs:158`) -> `ShowOverlay` (`:120`). `ShowOverlay` already fades `_overlayHost` 0 to 1 in `MotionTokens.OverlayIn` = 120 ms via `Anim.Run` (cubic ease-out, `Motion/Anim.cs`). There is no movement, and the pad panes snap to opacity 0 at once, so it reads as "no animation". Help overlay and Trash use the same `ShowOverlay`.
- **Other timings:** content reveal 160 ms with 6 px up (`PadWindow.cs:314`), Settings page 160 ms with 10 px up (`SettingsWindow.cs:261`), note arrival 200 ms / 34 px. All use `Anim.EaseOut`; none use a Win11 curve.
- **Where / Risk:** `Motion/MotionTokens.cs`, `Motion/Anim.cs` (optional easing parameter), `UI/PadWindow.Keys.cs` `ShowOverlay` only. Show/hide speed is untouched (overlay, not the window). A13-search-toggle-off edits `ToggleSearch`/button lines, not `ShowOverlay`.
- **Source:** director scout (this sub-chat).

## Plan
- **After (values from Microsoft Learn "Timing and easing", checked 2026-11-12):** open = fade 0 to 1 plus slide up from 16 px below, **167 ms** (ControlFastAnimationDuration), Fast Out Slow In `cubic-bezier(0,0,0,1)`. Close = fade out plus slide down 16 px, **83 ms** (ControlFasterAnimationDuration), Slow Out Fast In `cubic-bezier(1,0,1,1)`; MS examples make exits shorter than entrances. Tokens for reuse: `Win11Faster 83`, `Win11Fast 167`, `Win11Normal 250`, `Win11Enter`, `Win11Exit` curves, `OverlayRise 16`. Close animation touches `CloseOverlay` (A13 area): ask the director. `Anim.Run` gets an optional easing; default stays cubic ease-out so other animations do not change. Reduced motion: no slide, as today.
- **Scope question:** apply to every overlay (Search, Help, Trash search) for consistency (recommended), or Search only.
- **Must not change:** open/close logic, Esc, X, window show/hide, other animations.
- **Test plan:** lab first (`tools/playground/a14-search-open-lab.html`). Selftest: easing curve endpoints (0 -> 0, 1 -> 1, monotonic). Hand test: open Search by button, menu, Ctrl+F; Reduce motion on; rapid open/close; both themes.
- **Gates:** `tools/gate.ps1` (build, selftests, token literal count).
- **Needs from you:** approve the lab values (duration, slide, curve), then Approve.

## Files
- `product/AntiNotepad/Motion/MotionTokens.cs`
- `product/AntiNotepad/Motion/Anim.cs`
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `tools/playground/a14-search-open-lab.html`
- `docs/decisions.md`
- `product/AntiNotepad/UI/PadWindow.cs`

## Contracts
- none (motion polish)

## Approve
2026-11-12, user: "yes use microsoft's open and close. Log it in our documents as well along with source being microsoft learn". Lab v2 values: open 167 ms (0,0,0,1), close 83 ms (1,0,1,1), 16 px slide. Scope: all overlays (my default; director may change). Also log the values in `docs/decisions.md`.

## For Codex
Role: terra, a few files with timing and interruption to reason about. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, `.claude/rules/coding.md`, then the Files; re-read before editing. Build exactly the Plan's "After"; scope is all overlays (Search, Help, Trash); no drive-by changes.
- Tokens in `Motion/MotionTokens.cs`: `Win11Faster 83`, `Win11Fast 167`, `Win11Normal 250` (ms), `OverlayRise 16` (px); `OverlayIn` = `Win11Fast`, new `OverlayOut` = `Win11Faster`. Curves: add `Anim.CubicBezier(x1,y1,x2,y2)` returning a `Func<double,double>` easing (solve x(t) by Newton/bisection); `Win11Enter` = (0,0,0,1), `Win11Exit` = (1,0,1,1). Comments name the source: Microsoft Learn "Timing and easing" (no item IDs in code).
- `Anim.Run` gets an optional easing parameter; default stays the current cubic ease-out so other animations are unchanged.
- `ShowOverlay`: cancel any running close, reset the host transform, then fade 0 to 1 and slide `OverlayRise` px up to rest (Win11Enter). Reduced motion: no slide (Anim already skips decorative runs).
- `CloseOverlay`: all state changes stay immediate (`_overlay = null`, panes back, pill, chrome, focus) so A13's `ToggleSearch` and Esc behave as now. Only the overlay host's removal is deferred: it fades to 0 and slides down `OverlayRise` px over `OverlayOut` (Win11Exit), host not hit-testable meanwhile, then clears children and hides. A `ShowOverlay` or second `CloseOverlay` during the fade must cancel and finish teardown cleanly (no stale overlay, no stuck transform/opacity). Check what `UpdateOverlayLayout()` does and keep layout correct during the fade.
- Selftest in `SelfTest/UI.cs`: `CubicBezier` endpoints (0 -> 0, 1 -> 1), monotonic for both curves, (0,0,0,1) fast start (value at t=0.1 above 0.5), (1,0,1,1) slow start (t=0.1 below 0.1).
- `docs/decisions.md`
- `product/AntiNotepad/UI/PadWindow.cs`: add a "Motion reference" entry: values above, source Microsoft Learn "Timing and easing" and "Motion in practice" (https://learn.microsoft.com/en-us/windows/apps/design/motion/timing-and-easing), user approval 2026-11-12 via the lab; 16 px slide is our choice (Microsoft gives no popup offset); the later Settings and window-motion items reuse the `Win11*` tokens.
- Done means (offline): build, `--selftest`, `tools/gate.ps1 -Offline -Task A14-search-open-fade-up`. Report `.coord/reports/A14-search-open-fade-up.md`: what changed, checks run, anything not verified. Do not launch the app.
