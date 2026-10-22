# T-010 completion checkpoint
Completed 2026-10-18. Harness and docs/report written. Timing baseline partially blocked by sandbox, exact gaps recorded.
Final Release build: exit 0, 0 warnings, 0 errors, 2.22 s; prescribed --quit ran before each build.
Final full --selftest: exit 0, including --measure --samples 3 real child + JSON and four 30 s idle windows; corrected proof PNGs all under src selftest directory.
Standalone --measure --samples 30: exit 0, graceful child exit; src/AntiQuickPad/measure-release.json. Cold start to frame callback 952.144 ms. All four idle RAM/CPU windows collected; 20 notes x 200 lines verified on disk.
Input gap: SendInput 0/6, Win32 error 5 (Access denied) in both first warmups; registration succeeded but no WM_HOTKEY/foreground focus; Esc not sent without focus. Retained n=0, <100 ms verdict UNVERIFIED. CPU after show ~one core reproduced; cause unresolved.
Read docs/perf.md for measurements/method and exact interactive rerun; .coord/reports/T-010.md for implementation/verification/scope accounting. Initial legacy selftest regenerated four T-009 PNGs before correction; disclosed in report.
No git/network/shared STATUS.md edits or process kills. No further agent work pending. Manager must run outside sandbox for keyboard/foreground timings and CPU comparison.
