# A20-overscroll-speed: one smooth feel for swipe, scroll and overscroll

## 1 Summary

Implemented Follow-through horizontal note switching and shared edge-band overscroll for horizontal and vertical motion. Settings now names the axes separately; Rate-matched and Follow-through are defaults. Offline build and selftests pass. Live hand test remains.

## 2 Change list

- `product/AntiNotepad/UI/NoteSwipe.cs:10` | projection-based page switch, wheel lock, edge pull/return | Plan: horizontal Follow-through.
- `product/AntiNotepad/UI/PadWindow.Navigation.cs:289` | navigation availability at note-stack edges | Plan: first/last note resistance.
- `product/AntiNotepad/UI/MotionPreview.cs:115` | preview uses the new swipe and edge behavior | Plan: preview shows both axes.
- `product/AntiNotepad/UI/SettingsWindow.cs:453` | vertical label and one-option horizontal style | Plan: Settings > Feel.
- `product/AntiNotepad/Editor/Motion.cs:115` | shared vertical edge pull across editor and viewer surfaces | Plan: overscroll on every vertical style.
- `product/AntiNotepad/Editor/PadView.cs:455` | swipe velocity and spring transition | Plan: horizontal pull/arrival.
- `product/AntiNotepad/Motion/MotionTokens.cs:31` | locked motion parameters | Plan: values live in Motion tokens.
- `product/AntiNotepad/Motion/EdgeBand.cs:5` | shared resistance, spring, and notch gain | Plan: band, hold, spring.
- `product/AntiNotepad/Model/Settings.cs:41` | Follow-through setting alias; existing `Swipe` key stores the choice | Plan: defaults and compatibility.
- `product/AntiNotepad/SelfTest/ScrollFeel.cs:8` | horizontal/vertical overscroll checks | Plan: scroll-feel selftests.
- `product/AntiNotepad/SelfTest/Contracts.cs:120` | two new contract rows | Plan: no-jump and spring-return contracts.
- `product/AntiNotepad/SelfTest/Run.cs:80` | registers new checks | Plan: selftests.
- `product/AntiNotepad/SelfTest/UI.cs:271` | adapts swipe and transition checks | Plan: horizontal behavior.
- `product/AntiNotepad/SelfTest/App.cs:186` | checks new defaults | Plan: Rate-matched and Follow-through.
- `product/AntiNotepad/SelfTest/Design.cs:22` | refreshes the Settings design snapshot | Plan: Settings UI change.
- `docs/scroll-feel.md:1` | records locked horizontal and vertical values | Plan: parameter specification.
- `docs/decisions.md:22` | records A20 defaults and behavior | Plan: decision record.

## 3 Behavior changes

Horizontal input commits by projection, holds a wheel-repeat lock, and springs the edge pull home. Vertical Lines, Free, Rate-matched, and Wheel-driven retain their in-note glide and share top/bottom hold and spring behavior. Reduce motion bypasses both. The existing persisted `Swipe` key stores Follow-through; vertical default remains `rate`.

## 4 Out-of-scope files

The strict gate reports these nine scope hits. `docs/decisions.md` is listed in the card but was not parsed because it is the second inline path on one bullet. The `SelfTest/` directory is listed, but the gate compares it as an exact file path. `tools/playground/a20-compare.html` and `tools/playground/a20-styles-lab.html` were committed on the task branch before implementation as the approved source lab; they were not modified in this work. The task card and lab files were left untouched.

- `docs/decisions.md`
- `product/AntiNotepad/SelfTest/App.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
- `product/AntiNotepad/SelfTest/Run.cs`
- `product/AntiNotepad/SelfTest/ScrollFeel.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `tools/playground/a20-compare.html`
- `tools/playground/a20-styles-lab.html`

## 5 Plan deviations

No behavior deviation. The exact strict gate command reports `FAIL` on scope for the hits above; rerunning with `-LooseScope` reports `GATE: PASS`. The gate also warns that `PadView.cs` grew from 943 to 996 lines. The requested build-check folder remains because automatic sandbox review rejected its recursive removal; `dotnet clean` had only partially cleared it.

## 6 Contracts

Added `horizontal-edge-pull-no-jump` and `vertical-edge-pull-springs-home`. Selftest reports 20 contract rows: 19 pass and 1 manual.

## 7 Lens review

Not run; this item assigns Codex the worker role. The director-only reviewer instruction does not apply.

## 8 Checks run

- Build to `build/A20-overscroll-speed-check`: 0 warnings, 0 errors.
- `--selftest`: `SELFTEST PASS`.
- Exact requested offline gate: build, selftest, seam, tokens, and format passed; gate failed on scope.
- Offline gate with `-LooseScope`: `GATE: PASS`; scope and size are warnings, offline NuGet audit is disabled.
- `git diff --check`: passed.

## 9 Hand checks for the user

Use the dev copy. Check wheel and touchpad at the first and last notes; top and bottom of a long note in Lines, Free, Rate-matched, and Wheel-driven; Reduce motion; and both axes in Settings > Feel preview. Confirm edge pulls return without a snap and caret/selection scrolling remains intact. Not performed here; the app was not launched.
