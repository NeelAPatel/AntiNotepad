Implemented A31-no-windows-show-animation: the pad and taskbar stand-in now disable DWM transitions before first show. App motion remains unchanged.

- Build: 0 warnings/errors with `-p:NuGetAudit=false`; the exact requested build had two NU1900 network warnings.
- `SELFTEST PASS`; diff check passed.
- Windows behavior **needs a live check**.

[Report and checklist](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad-wt/A31-no-windows-show-animation/.coord/reports/A31-no-windows-show-animation.md>).

Automatic approval review rejected build-output cleanup as “blocked by policy”; `build/a31-check` remains.