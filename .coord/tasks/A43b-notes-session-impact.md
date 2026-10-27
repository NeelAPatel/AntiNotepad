# A43b-notes-session-impact: re-audit before implementing A43b

Role: director (gpt-6.1-sol medium). **READ-ONLY.** Edit no file except your report `.coord/reports/A43b-notes-session-impact.md`.
Do not build into the repo tree; do not launch or kill AntiNotepad (never `--quit`). Branch `reorg/A43-app-context` (dev ab4d353
merged). Product `product/AntiNotepad`. Read `AGENTS.md`, `.coord/tasks/A43-app-context-plan.md` (revision 2, the A43b section),
`.coord/reports/A43-app-context-impact-r2.md`, and `.coord/tasks/A43a-runtime-log-and-launch.md` plus its report if present
(A43a is merged: `RuntimeLog`, `TaskbarTrace`, `AppLaunch`; `Program.` tokens 75 -> 21; 80 LAYERS rows; `RUNTIME LOG` and
`TASKBAR TRACE` selftests). Verify everything against the current code.

## A43b scope to re-audit
Pass `Store` + `SettingsService` explicitly to `App`, `PadWindow`, `SearchOverlay` and the `Timers` lambda; a new `NotesSession`
owning `Store`, `AppHome`, live `DataFolder`/`NotesLocationError` and `MoveNotes` (moved verbatim); a read-only location view into
`PadView`/`MotionPreview`; Settings reaching storage state through `ISettingsHost` or `NotesSession`; delete all `Program`
runtime statics. In scope too: the `docs/dev-loop.md:11` fix (custom `ANTINOTEPAD_DATA` follows its own `location.json`).

## Questions (file:line evidence)
1. **Remaining `Program.` uses**: re-map all 21 (member, file:line, live vs cached read, thread). List what `Program` still owns
   after A43a (statics, `_runtimeLog`, `AppLaunch`, `_launch`, anything new) and what A43a changed around the A43b seam.
2. **Current composition order** in `Program.RunApplication`/`App`: where `AppHome`, `DataFolder`, `NotesLocationError`, `Store`,
   `SettingsService` become valid; the second `AppHome` assignment; the `Store` fallback on constructor failure; how
   `AppLaunch` is built and whether `NotesSession` fits next to it or should be part of it.
3. **`MoveNotes` sequencing**: guard -> `Store.Save` -> `NotesLocation.Move` -> `SwitchFolder` -> folder/error update -> warning log;
   what `SettingsWindow.MoveNotes` (`SettingsWindow.cs` ~660-710) does before and after (host Save, `LastError`, Refresh, backup text
   from `oldFolder`/`appHome` snapshots); success-with-unavailable-store; thread requirements. What must stay identical.
4. **Stale-capture audit** per reader (live vs captured): `PadView.Bind` (same-note early return), `MotionPreview` private `PadView`,
   `SettingsWindow` rows, `PadWindow` timers/tray closures, `SearchOverlay` (search context snapshot), anything else.
5. **Single instance / `--quit` / `--selftest` / `--hidden`** and the data-folder path (`usesCustomData`, migration, `NotesLocation.Resolve`,
   `createDirectories`): what A43b could disturb and the exact checks that would catch a dev/installed mix-up. Confirm the
   `docs/dev-loop.md:11` discrepancy and give the exact wording fix.
6. **Session-end flush**: `PrepareForSessionEnd`, `RuntimeLog` flush/dispose order after A43a, `ProcessExit`; anything A43b touches.
7. **Shape**: `NotesSession` members, constructor, who builds it and when; narrow read-only interface for `PadView`/`MotionPreview`;
   `SettingsWindow` route (extend `ISettingsHost` vs inject `NotesSession`) with a recommendation; every signature change and
   caller; how `App` gets `Store`/`SettingsService` (injected ctor or `AppLaunch`); the Avalonia 12 app-factory question (check
   how A43a already injects launch values into `App` and reuse that mechanism).
8. **LAYERS rows** in the existing table form (read the current rule table in `SelfTest.cs`): the `Program` rows from the plan
   that now hold or only hold after A43b, the universal "only Program references Program" rule and the small `LayerRule` extension
   it needs, the `typeof(Program)` `ldtoken` allowance in SelfTest's rule initializer, and the static-service-exposure rule.
   Also a focused selftest capability for `NotesSession.MoveNotes` on a temp folder (same `Store` reference, folder/error updated together).
9. **Risks and user hand checks** (dev fixtures only) for data folder, `MoveNotes`, session-end flush, startup order, stale
   captures, single instance, `--quit`; include the still-pending A30 session-end check.
10. **Overlap and recommendation**: files to touch with current sizes; whether A43b is still worth doing as scoped, can be smaller,
   or should split (e.g. `A43b1` explicit `Store`/`SettingsService` injection vs `A43b2` `NotesSession`/`MoveNotes`); what to drop;
   interplay with A44-platform-seam.

## Report (`.coord/reports/A43b-notes-session-impact.md`)
Tables over prose, under 300 lines; mark anything unverified. Done means only the report is written.
