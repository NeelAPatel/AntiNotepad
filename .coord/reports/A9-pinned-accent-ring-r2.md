# A9-pinned-accent-ring-r2

## Changed
- Updated `PinRingChecks` to install the Fluent/AvaloniaEdit styles, load a temporary store, seed welcome data, and build the pad using the same setup as `DesignDump`. It checks the ring's target opacity, initial click-through state, geometry, palette accent, and reduced-motion toggles, then restores motion state and deletes its temporary folder.
- Moved the ring contract to the end of `ContractsChecks`. The trace showed the ring check returning and cleaning up, followed by the existing `store-unavailable-save-noop` contract stalling when it ran next. With the ring check last, the remaining contracts and design check complete. This is an order-dependent shared selftest state issue; ring behavior was not changed.
- Compared the parent commit `1e3344de` dump with the ring build. The parent has 16,168 rows and the old hash. After normalizing the root Grid child index shifted by insertion, the only content differences are eight new ring Border rows, one per palette; all other control values match. Updated `DesignHash` to `1c28bf65fda59bce045469a798064053d9cadf488cda959aa99088ae2fe5fb4d`.

## Checks run
- Reproduced the round 1 stall with a 300-second bound: output stopped after `A18-search-speed timing`; the verified build process used 3.62 seconds of CPU before termination.
- Debug build to `build/A9-check`: 0 warnings, 0 errors.
- Final `--selftest`: exit 0 in 4.3 seconds; `CONTRACTS: 16 rows, 15 pass, 1 manual`; `DESIGN dump 1c28bf65fda59bce045469a798064053d9cadf488cda959aa99088ae2fe5fb4d 16176`; `SELFTEST PASS`.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A9-pinned-accent-ring-r2 -Offline`: PASS. Build, Selftest, Seam, Tokens, Format, Scope and Cleanup passed. LoopLint skipped because no loop files changed. Offline NuGet audit was disabled for this build. Size reported a non-failing warning: `PadWindow.Layout.cs` grew from 606 to 636 lines.
- Removed `build/A9-check`, the parent-comparison and diagnostic artifacts, and leftover round 1 gate output/data folders. The round 2 gate also removed its output and data folders.

## Not checked
- No app was launched. Visible fade, corner alignment, custom accents, both themes, split view, rapid toggles, Reduce motion, dragging, resizing and top-bar click behavior remain for the planned hand test.

## Summary
GATE: PASS (Size WARN on `PadWindow.Layout.cs` as described above).
