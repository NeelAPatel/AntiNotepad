**A14-search-open-fade-up is implemented.** All overlays use the approved fade and 16 px slide, with immediate close-state changes and cancellation for interrupted animations. The motion values and Microsoft Learn references are recorded in [decisions.md](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/feat/A14-search-open-fade-up/docs/decisions.md>).

The offline gate passed, including build and selftest. The specified entry curve evaluates to approximately **0.446 at 0.1**, so the selftest checks that value rather than the requested, incompatible `> 0.5` assertion.

The app was not launched. Live motion, reduced motion, rapid interruption, focus, themes, and Search entry points remain for hand testing. Details are in the [build report](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/feat/A14-search-open-fade-up/.coord/reports/A14-search-open-fade-up.md>).

