Implemented A27. Settings search now creates row controls only after the existing title/description tags match, while keeping the 150 ms debounce and the page builders as the sole row definitions. The selftest compares results against the normal page builders.

- **Verification:** Release build succeeded with 0 warnings and 0 errors; `--selftest` printed `SELFTEST PASS`; `git diff --check` passed.
- **One-call UI-thread timings:** `palette` 13.811 → 1.570 ms; no match 12.321 → 0.960 ms.
- **Needs a live check:** Search appearance, nested-row indentation, and matched-control interaction in the dev copy. The app was not launched.
- The temporary `build/a27-check` folder remains because the shell sandbox rejected its removal.

The implementation and details are in [SettingsWindow.cs](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad-wt/A27-settings-search-build-matches/product/AntiNotepad/UI/SettingsWindow.cs:255>), [SettingsParts.cs](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad-wt/A27-settings-search-build-matches/product/AntiNotepad/UI/SettingsParts.cs:55>), [SelfTest.cs](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad-wt/A27-settings-search-build-matches/product/AntiNotepad/SelfTest.cs:56>), and the [A27 report](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad-wt/A27-settings-search-build-matches/.coord/reports/A27-settings-search-build-matches.md:1>).