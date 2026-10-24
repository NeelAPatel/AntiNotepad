Implemented **A13-timer-max-and-currency-error**. Timers now cap at 999h 59m 59s, including compact and clock forms. Mixed-currency failures carry a reason into math and inline result chips, which use the palette’s Error color and are not clickable.

The Release build succeeded with **0 warnings and 0 errors**, and `--selftest` passed. `NuGetAudit=false` was set in the build environment because restricted network access prevented fetching NuGet vulnerability metadata.

Report: [A13-timer-max-and-currency-error.md](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad-wt/A13-timer-max-and-currency-error/.coord/reports/A13-timer-max-and-currency-error.md>)

The environment blocked recursive removal of `build/a13-check`, so that verification output remains.