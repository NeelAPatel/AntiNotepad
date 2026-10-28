Implemented **A43b-notes-session-r3** on `reorg/A43b-notes-session`; no commit made.

When the notes folder is unavailable, Settings now offers **“Use this folder.”** Success shows: “Notes folder set to <folder>. Quit and start AntiNotepad again to load it.” The new path validates an existing, readable folder and writes the pointer for the next start without reloading or changing the current session. The available-store `MoveNotes` path and Settings save preflight remain unchanged.

Build succeeded with 0 warnings and 0 errors. Selftest printed `NOTES SESSION use-folder passed` and `SELFTEST PASS`; the 89 existing LAYERS rows remain unchanged. The report is [A43b-notes-session-r3.md](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.coord/reports/A43b-notes-session-r3.md>).

Cleanup of `build/A43b-r3-check` was rejected by tool policy, so that directory remains. I did not launch the app; the manager hand check remains pending.

