# A9-pinned-accent-ring

## Changed
- Added the 4 px named ring token, Shape.Card corners, palette accent brush and Paint.FocusBorder target opacity.
- Added a non-hit-test Border over the chrome, with explicit stacking above the pager when it is reparented. Sibling layout, pin icon, toast, dimming and show/hide code are unchanged.
- Added a cancellable 250 ms fade from current opacity, instant initial state and Reduce motion handling. Appearance updates refresh the accent; appearance batches apply the ring after Reduce motion is synchronized, avoiding a second update in HandleSettingsChanged.
- Added the pinned-accent-ring contract and a small real-pad capability check for opacity targets, input pass-through, geometry, accent and reduced-motion toggles.
- Changed only the seven allowed product files. Card and master-owned STATUS remain untouched. No commits or app launch.

## Checks run
- Restored from the local NuGet cache using an empty local source and NuGetAudit=false; no remote source needed.
- Debug build into build/A9-check: PASS, 0 warnings, 0 errors.
- --selftest and tools/gate.ps1 -Task A9-pinned-accent-ring -Offline: incomplete selftest twice, with no further output after A18-search-speed and near-idle CPU across samples. Ended only the two known check processes after verifying their executable paths. No SELFTEST PASS or CONTRACTS summary was reached; the stall was not localized.
- Gate: Build, Seam, Tokens and Format PASS. Selftest FAIL after bounded termination. Scope FAIL because tools/playground/a9-ring-lab.html is already committed on this branch but absent from the build card's Files list when compared with dev. Size WARN: PadWindow.Layout.cs 606 -> 636 lines (same chrome/layout responsibility).
- git diff --check: PASS.
- Independent lens review: no additional ring behavior defect found; rework required for the design baseline dependency below.
- Cleanup was attempted with resolved workspace containment checks, but automatic approval review rejected the recursive cleanup command as blocked by policy. Temporary build/A9-check, gate output/data and build/offline-nuget may remain. The gate itself retained its outputs on failure.

## Not checked
- Visible fade, rounded-corner alignment, custom accent/theme appearance, split view, fast toggles, drag/resize, icon clicks and show/hide timing require the planned hand test. No dev or installed app was launched.
- New capability checks did not reach a reported result before selftest termination.
- Additional file needed: product/AntiNotepad/SelfTest/Design.cs. Its DesignDump captures the entire logical tree, including the newly added Border; its fixed DesignHash therefore needs a reviewed snapshot update. This file is outside the approved Files list and was not edited. The mismatch is established by source inspection, not a completed selftest output.
- The manager must reconcile the already approved lab HTML with the gate's branch-wide scope, and authorize the design-baseline file before a fully passing gate is possible.
