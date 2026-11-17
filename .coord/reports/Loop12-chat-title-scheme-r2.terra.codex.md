**Loop12-chat-title-scheme-r2 is implemented.** The title script now resolves items through the session registry and reads phases from main-checkout STATUS before falling back to worktree cards. The hook passes its session ID, and the chat-start instruction registers the item.

The requested checks passed: `chat-title.ps1 -Check`, loop-lint, and `git diff --check`. Loop-lint reported three existing budget warnings. The sync check used temporary fixtures; the gate and live app-data sync were not run, as requested.

The [report](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/chore/Loop12-chat-title-scheme-r2/.coord/reports/Loop12-chat-title-scheme-r2.md>) includes the case table and hand checks.