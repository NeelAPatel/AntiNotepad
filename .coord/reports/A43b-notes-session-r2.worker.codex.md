Implemented **A43b-notes-session-r2**. Unavailable Store paths now get a valid in-memory selection, and the new selftest covers both missing-folder startup paths, no-write saving, and loading data after switching to a valid folder.

Build succeeded with **0 warnings and 0 errors**. Selftest exited 0 with `SELFTEST PASS`, all 89 `LAYERS` lines, and `UNAVAILABLE STORE startup`.

One existing limitation remains: Settings’ migration flow still rejects an unavailable source. I left it unchanged as instructed; the report documents the gate. Recursive removal of `build/A43b-r2-check` was blocked by tool policy, so that folder remains. No commit was made.

Report: [A43b-notes-session-r2.md](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.coord/reports/A43b-notes-session-r2.md>)