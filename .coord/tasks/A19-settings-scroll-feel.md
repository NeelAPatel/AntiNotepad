# A19-settings-scroll-feel: Settings scrolling should follow the scroll style chosen in the Feel tab

Item card (`docs/loop.md`). Budget: 4 KB.

Tier: Medium · ui-scroll
State: approved
Branch: `feat/A19-settings-scroll-feel` in `.claude/worktrees/feat/A19-settings-scroll-feel` (created at Approve)

## Request
2026-11-12, user (pasted list, Animations, Settings), verbatim: "Settings scroll matching Feel tab" (the list row reads: scrolling inside Settings ignores the scroll style chosen in the Feel tab; make it follow).

Then: "start settings scroll feel". The Feel tab holds the scroll style choices; find how the note editor applies them and make the Settings window scroll the same way. If a visible change needs judging, show it in a lab page or the dev copy (the user cannot judge motion from numbers).

## Findings
- Feel setting: Settings.Scrolling (lines|free|rate|wheel) plus LinesPerNotch, ScrollDistance, ScrollBoost, FreeFriction (Model/Settings.cs). Applied only by `SmoothScroll` (Editor/Motion.cs), which is built around a TextEditor: it tunnels PointerWheelChanged, runs a per-frame glide/coast/spring/fly, and writes the editor's inner ScrollViewer.Offset.
- Settings today: one plain ScrollViewer (`_scroll`, UI/SettingsWindow.cs) holds every page (all sections, search results); stock Avalonia wheel step, no glide. Page switch and keepScroll rebuilds set `_scroll.Offset` directly.
- Also unsmoothed, same cause: HelpOverlay and SearchOverlay scrollers (out of scope; the class change makes them a one-line follow-up).
- The Feel tab preview (UI/MotionPreview.cs) already uses the same SmoothScroll on a sample pad, so the feel is user-approved; only the host differs.

## Plan
After: wheel/touchpad scrolling in Settings glides with the chosen style and tuning, same as the note editor; changing the style in Feel takes effect at once.
How: SmoothScroll talks to a tiny surface (extent, viewport, offset, line height) instead of a TextEditor; one adapter for TextEditor (unchanged behavior), one for a ScrollViewer; Settings attaches it to `_scroll`. Line height for Settings = a Typography token (about one row).
Must not change: editor scroll feel (all 4 styles), Feel preview, scrollbar drag, keyboard/Page keys, Settings page-switch and keepScroll offset restore (a running glide must stop on content swap), Reduced-motion (jumps).
Test plan: --selftest check that the surface adapters clamp/offset right; dev copy hand test: each of 4 styles in Settings, touchpad vs wheel, switch page mid-glide, reduced motion.
Lab: no new look is designed (reuses the approved Feel behavior), so the dev copy is the lab; no HTML page.
Needs from you: Approve.

## Files
- `product/AntiNotepad/Editor/Motion.cs` (SmoothScroll takes a scroll surface)
- `product/AntiNotepad/UI/SettingsWindow.cs` (attach to `_scroll`)
- `product/AntiNotepad/SelfTest/Editor.cs` (surface adapter check)
- `.coord/reports/A19-settings-scroll-feel.md` and `.terra.codex.md` (reports)

## Contracts
None new.

## Approve
2026-11-13, user: "Approve"

## For Codex
Role: terra (docs/agents.md), because a reasoned change across 3 files. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then the Files.
- Build exactly the Plan's After; no drive-by changes. Build into `build/A19-settings-scroll-feel-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, delete it. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A19-settings-scroll-feel -Offline`.
- Never launch AntiNotepad, never edit STATUS or this card. Offline checks only.
- Done means: Settings wheel scroll uses SmoothScroll with the Feel style; editor behavior unchanged; build 0 warnings, SELFTEST PASS, gate passes.
Report `.coord/reports/A19-settings-scroll-feel.md` per docs/agents.md.

## Check
Gate at branch tip: GATE PASS offline (build 0 warnings, selftest, seam, tokens, format, scope, size). Diff within plan: 3 files; Codex added a nested-scroll guard (editor/viewer inside Settings keeps its own wheel) and frame cancellation on content swap, both justified by the Feel preview and page rebuilds. Settings line height uses Typography.PanelTitle (18 px), so Lines mode steps a bit shorter than in the editor (24 px). Noticed: Help and Search overlays still scroll stock. Misses: None.

## Hand test
(pending)
