# A20-overscroll-speed-r2: carry scroll momentum into the overscroll

## 1 Summary

Vertical animated arrivals now carry outward speed into the existing edge spring from zero displacement. Lines, Free, Rate-matched, and Wheel-driven keep their in-note motion; Reduce motion bypasses carry-over.

## 2 Change list

- `product/AntiNotepad/Editor/Motion.cs:362` | Transfers final Lines speed and Coast, Spring, and Fly arrival speed into the edge return | Plan: vertical carry-over.
- `product/AntiNotepad/Motion/EdgeBand.cs:21` | Starts a return at zero displacement with incoming velocity and preserves its outward phase before settling | Plan: continuous edge spring handoff.
- `product/AntiNotepad/Motion/MotionTokens.cs:39` | Names the existing 15, 5, and 4 px/s stop cutoffs | Plan: use existing thresholds.
- `product/AntiNotepad/SelfTest/ScrollFeel.cs:75` | Checks four styles, cutoff speeds, initial displacement and speed, wheel interruption, and Reduce motion | Plan: carry-over selftest.
- `product/AntiNotepad/SelfTest/Contracts.cs:131` | Adds the vertical carry-over contract row | Plan: regression contract.
- `docs/scroll-feel.md:30` | Records the vertical carry-over rule and Lines speed calculation | Plan: scroll-feel documentation.

## 3 Behavior changes

At a boundary, the incoming speed starts the same critically damped edge spring from shown = 0. Wheel input during the return continues through `CurrentPull` and `Push`. Speeds below each style's existing cutoff and Reduce motion do not carry.

## 4 Out-of-scope files

None.

## 5 Plan deviations

No implementation deviation. The standalone `build/A20-overscroll-speed-r2-check` output remains because automatic approval review rejected its recursive removal after its exact path was resolved and verified inside this worktree's `build` directory. The gate removed its own task build and data folders.

## 6 Contracts

Added `vertical-scroll-carries-momentum-to-edge`. The horizontal no-jump and vertical spring-home A20 contracts still pass. Selftest reports 21 rows: 20 pass, 1 manual.

## 7 Lens review

No separate lens agent was run for the worker role. Self-review covered threshold selection, outward direction, return interruption, Reduce motion, and zero-position handoff; no additional findings.

## 8 Checks run

- Build to `build/A20-overscroll-speed-r2-check`: 0 warnings, 0 errors.
- `--selftest`: `SELFTEST PASS`; 21 contract rows, 20 pass and 1 manual.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A20-overscroll-speed-r2 -Offline`: `GATE: PASS`.
- Gate notes: offline NuGet audit disabled; inherited `PadView.cs` size warning (943 to 996 lines).
- `git diff --check`: passed.

## 9 Hand checks for the user

Not performed; this was offline work. In the dev copy, reach the top and bottom of a long note with a fast wheel roll in Lines, Free, Rate-matched, and Wheel-driven. Confirm the existing scroll momentum flows into one edge return. Repeat with Reduce motion on.
