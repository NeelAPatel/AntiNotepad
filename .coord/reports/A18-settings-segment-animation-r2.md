# A18-settings-segment-animation-r2 report

## 1 Summary
Rebuilt Settings selectors resume the highlight from its recent measured position and glide to the new selection. Reduced motion snaps.

## 2 Change list
- `product/AntiNotepad/UI/SettingsWindow.cs` | Per-window selector position cache | rebuild handoff.
- `product/AntiNotepad/UI/SettingsParts.cs` | Save before picking; restore fresh differing selections after layout; prevent detached animations from overwriting the new owner | rebuild glide.
- `product/AntiNotepad/SelfTest/Design.cs` | Pure start-position checks for fresh, stale, missing, same value, reduced motion and reversed clock | test plan.

## 3 Behavior changes
The highlight resumes across row and palette rebuilds for up to 1000 ms, using the existing 167 ms enter motion. The cache belongs to the Settings window and is keyed by option values. Reduced motion snaps. Hover and pick order are unchanged.

## 4 Out-of-scope files
None.

## 5 Plan deviations
None. The lab and design snapshot did not need changes.

## 6 Contracts
Existing selftests pass (18 contract rows: 17 pass, one manual). The new decision is covered by the design selftest; no contract file was in the authorized files.

## 7 Lens review
Applied the checklist to the change, including rebuild timing, stale animation ownership and reduced motion. No independent lens reviewer was run; the item chat should perform its normal review.

## 8 Checks run
- Offline build with `NuGetAudit=false`: 0 warnings, 0 errors. An initial restore attempted NuGet audit and produced two network warnings; the offline rebuild passed cleanly.
- `--selftest`: SELFTEST PASS; design hash unchanged.
- `tools/gate.ps1 -Task A18-settings-segment-animation-r2 -Offline`: GATE PASS. Offline audit and size warnings; gate build 0 warnings.
- `git diff --check`: pass.
- Gate removed its own build and data folders. Cleanup of `build/A18-settings-segment-animation-check` was rejected by sandbox policy; it remains.

## 9 Hand checks for the user
In the dev copy, switch Theme and selectors that rebuild rows (including Bars, Show, Scrolling style and Swipe style); check the glide, rapid retargeting, narrow wrapping, hover fade and reduced-motion snap.
