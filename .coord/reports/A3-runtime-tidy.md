# A3-runtime-tidy report

- **A1-22-json-options-per-command:** Reused a static `JsonSerializerOptions` instance for pretty JSON output; formatting is unchanged.
- **A1-24-disabled-caret-still-ticks:** The caret stops its blink timer and stays visible when blinking is disabled. `PadView.ApplyLook` calls `SmoothCaret.SettingsChanged`, so settings changes while focused start or stop blinking immediately. Caret movement still invalidates the layer, and enabled blinking keeps the 600 ms solid period after movement.
- **A1-25-tray-empty-title-sentinel:** Tray slots now carry title and occupied state separately. Empty slots still display `empty` and are disabled; a real title of `empty` stays enabled.

Files changed: `Editor/Completion.cs`, `Editor/Motion.cs`, `Editor/PadView.cs`, `Program.cs`, `Platform/Tray.cs`, and this report.

Validation: Release build succeeded with 0 warnings and 0 errors (`AVALONIA_TELEMETRY_OPTOUT=1`; process-local `NuGetAudit=false` avoids `NU1900` from the unreachable vulnerability feed). `build/a3-check/AntiNotepad.exe --selftest` printed `SELFTEST PASS`. Cleanup of `build/a3-check` was blocked by workspace policy, including a direct attempt to remove a single generated file, so the output directory remains.
