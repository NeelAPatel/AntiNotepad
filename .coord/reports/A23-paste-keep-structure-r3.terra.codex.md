Implemented the HTML paste conversion for inline formatting, headings, links and aligned tables. Exact selftests for captures D and E pass. Capture C retains its words and list structure, with bold and code formatting added under the new rules.

The build completed with **0 warnings**, selftests passed, and the offline gate passed. Live clipboard, UI and undo behavior remain for hand testing. The execution policy rejected removal of the separate `build/A23-check` folder.

The report is at [A23-paste-keep-structure-r3.md](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/feat/A23-paste-keep-structure/.coord/reports/A23-paste-keep-structure-r3.md>).