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

## Contracts
- none (motion polish)

## Approve
2026-11-12, user: "yes use microsoft's open and close. Log it in our documents as well along with source being microsoft learn". Lab v2 values: open 167 ms (0,0,0,1), close 83 ms (1,0,1,1), 16 px slide. Scope: all overlays (my default; director may change). Also log the values in `docs/decisions.md`.
