Variant 2 is now the sole note-swipe behavior, including in release builds. The offline build completed with **0 warnings**, and the selftest passed. I also ran the lens-review checklist myself; it found no proven code issues.

The worktree is **not yet ready to merge** under the stated done criteria:

- The offline gate failed its Scope check because the round-2 brief has no backticked paths under `Files`. Build, Selftest, Seam, Tokens and Format passed. I left the brief untouched as instructed.
- Two pre-existing “Scroll Lab” comments remain in product files outside the approved file list. The note-switch lab selector and its wording are removed.
- The sandbox rejected deletion of `build/A6-note-switch-instant-swap-check`, so that folder remains.

The full findings and hand-check guidance are in the [round-2 report](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/fix/A6-note-switch-instant-swap/.coord/reports/A6-note-switch-instant-swap-r2.md>). I did not launch or kill AntiNotepad, edit the cards or STATUS, or commit.