Implemented **A11-storage-settings-tab** and wrote the requested [report](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad-wt/A11-storage-settings-tab/.coord/reports/A11-storage-settings-tab.md>).

Storage now contains the notes-folder card and the Recovered notes and App data rows. It shows pending, refused, moved, and unavailable states inline. The pad’s missing-note and unavailable-folder hints use the requested text and dim error color. About no longer contains file-path controls.

**Verification:** Release build passed with 0 warnings and 0 errors when `NuGetAudit=false` was set; the default vulnerability-feed lookup had returned `NU1900` because `api.nuget.org` was unreachable. Selftest passed, and `git diff --check` passed. `.coord/STATUS.md` and the audit doc were left untouched.