Implemented the title calculator, best-effort hook and script signals, merge instruction, documentation, and roadmap legend. The app still needs a `set_session_title` call for each emitted title; the title store records the emitted value, not whether the app accepted it.

`chat-title.ps1 -Check`, loop-lint, and `git diff --check` passed. Loop-lint reported budget warnings for `docs/loop.md`, lessons, and the item card.

The requested gate **was not run**: it launches `AntiNotepad.exe` for self-test, conflicting with the brief’s instruction never to launch AntiNotepad. Git Bash also could not start for syntax validation because of a `CreateFileMapping` access error. The sandbox could not write to the main checkout’s title store, so caller integration and exit behavior remain unverified.

The live title transitions, merge rename, and roadmap legend still need hand checks. Details are in the [Loop12 report](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/chore/Loop12-chat-title-scheme/.coord/reports/Loop12-chat-title-scheme.md>).