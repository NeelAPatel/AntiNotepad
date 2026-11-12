# A9-pinned-accent-ring-r3

## Changed
- Pin and Dim when pinned settings batches keep the current chrome visibility while retaining appearance updates and save scheduling. Other appearance settings still sleep chrome.
- Ctrl+T bypasses the general keydown fade, so the pin shortcut also preserves chrome visibility.
- Extended the last ring contract with settings-service and keyboard checks for awake chrome, a normal edit, and pin changes while asleep. The Fluent and seeded-store setup remains intact.

## Checks run
- Offline build to `build/A9-check`: 0 warnings, 0 errors.
- Selftest with a 300-second timeout: `SELFTEST PASS`; 16 contract rows, 15 pass and 1 manual. Design hash unchanged.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A9-pinned-accent-ring-r3 -Offline`: PASS, including build, selftest, seam, tokens, format, scope and gate cleanup. Existing non-failing size warning: `PadWindow.Layout.cs` 606 to 636 lines. Offline NuGet audit disabled.
- Cleanup of the standalone build and captured outputs was blocked by the command policy; they remain in `build/A9-check`, `build/A9-selftest.out`, `build/A9-selftest.err`, `build/A9-gate.out`, and `build/A9-gate.err`. The gate report remains at `.coord/reports/A9-pinned-accent-ring-r3.gate.md`.

## Not checked
- No app was launched. Hand test Fade mode with the pin button and Ctrl+T, ring fade and typing; the remaining visual checks on the item card also require the dev copy.
