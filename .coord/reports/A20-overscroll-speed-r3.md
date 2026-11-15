# A20-overscroll-speed-r3: remove vertical overscroll

## 1 Summary

Restored vertical scrolling to the `dev` implementation, removed vertical edge pull and carry-over state/tests, and retained horizontal Follow-through behavior and contracts.

## 2 Change list

- `product/AntiNotepad/Editor/Motion.cs:16` | Restored scrolling code from `dev`; only the Settings comment differs | Plan: vertical scrolling returns to its pre-item behavior.
- `product/AntiNotepad/Motion/EdgeBand.cs:1` | Removed carry-only spring start/direction state; retained shared horizontal edge pull behavior | Plan: remove vertical-only edge state.
- `product/AntiNotepad/Motion/MotionTokens.cs:30` | Removed vertical nudge and carry cutoffs; retained horizontal timing and input tokens | Plan: remove vertical-only tokens.
- `product/AntiNotepad/SelfTest/ScrollFeel.cs:8` | Kept horizontal checks; removed vertical edge and carry checks | Plan: keep horizontal checks.
- `product/AntiNotepad/SelfTest/Contracts.cs:119` | Kept `horizontal-edge-pull-no-jump`; removed both vertical edge rows | Plan: remove vertical contracts.
- `product/AntiNotepad/SelfTest/Run.cs:80` | Kept the horizontal selftest call and removed the vertical call | Plan: remove vertical checks.
- `product/AntiNotepad/UI/SettingsWindow.cs:469` | Reduce motion subtitle now names horizontal overscroll | Plan: describe the remaining pull accurately.
- `docs/scroll-feel.md:23` | Documents unchanged vertical glide, stop-at-ends behavior, and the 2026-11-15 user call | Plan: update vertical section.
- `docs/decisions.md:22` | Records unchanged vertical behavior and horizontal-only edge-band parameters | Plan: update locked decision.

## 3 Behavior changes

Vertical scrolling now matches `dev`: Lines, Free, Rate-matched, and Wheel-driven retain their in-note glide, stop at note ends, and touchpad input follows 1:1. There is no vertical overscroll or carry-over. Horizontal Follow-through, Settings names and defaults, and Reduce motion behavior remain in place.

## 4 Out-of-scope files

None. The gate generated `.coord/reports/A20-overscroll-speed-r3.gate.md`.

## 5 Plan deviations

The standalone `build/A20-overscroll-speed-r3-check` output remains because automatic approval review rejected the recursive removal command as `blocked by policy`. The offline gate cleaned its own task build and data folders.

## 6 Contracts

Removed `vertical-edge-pull-springs-home` and `vertical-scroll-carries-momentum-to-edge`. Kept `horizontal-edge-pull-no-jump`; selftest reports 19 rows, 18 pass and 1 manual.

## 7 Lens review

No separate lens agent was run for the worker role. Self-review checked the `Motion.cs` diff against `dev`, verified horizontal `EdgeBand` consumers remain, and scanned for vertical edge symbols; no further findings.

## 8 Checks run

- `dotnet build product/AntiNotepad -c Debug -o build/A20-overscroll-speed-r3-check`: 0 warnings, 0 errors.
- `build/A20-overscroll-speed-r3-check/AntiNotepad.exe --selftest`: `SELFTEST PASS`; 19 contract rows, 18 automated and 1 manual.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A20-overscroll-speed-r3 -Offline`: `GATE: PASS`; build, selftest, format, scope, and cleanup passed. Gate warnings: offline NuGet audit disabled and inherited `PadView.cs` size warning (943 to 996 lines).
- `git diff --check`: passed.

## 9 Hand checks for the user

Not performed; this was offline work. In the dev copy, confirm all four vertical styles stop at the top and bottom, touchpad scrolling remains 1:1, and horizontal edge pulls still follow and spring home.
