Implemented A20-taskbar-icon-setting with While visible, Always, and Never; default is Never. The tray remains available. The root cause was the pad’s unconditional `ShowInTaskbar = false`.

Release build: 0 warnings, 0 errors. Selftest: `SELFTEST PASS`. Live Windows behavior still needs checking.

Details and checklist: [A20 report](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad-wt/A20-taskbar-icon-setting/.coord/reports/A20-taskbar-icon-setting.md>).

Automatic approval review blocked cleanup by policy; `build/a20-check` remains.