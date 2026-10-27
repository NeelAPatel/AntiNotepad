# A43-app-context-impact-r2: re-audit of the `Program` statics after A39-A42b

Role: director (gpt-6.1-sol medium). **READ-ONLY.** Edit no file except your report `.coord/reports/A43-app-context-impact-r2.md`.
Do not build into the repo tree, do not launch or kill AntiNotepad (never `--quit`). Branch `reorg/A43-app-context` (dev 756f139
merged); product `product/AntiNotepad`. Read `AGENTS.md`, then `.coord/tasks/A43-app-context-plan.md` and
`.coord/reports/A43-app-context-impact.md` (round 1, written before A39-A42b landed). Verify everything against the current code.

## What changed since round 1
A39-motion-out-of-ui (`Motion/Anim.cs`, `Theme/ChromeTokens.cs`, IL `LAYERS` table in `SelfTest.cs`), A40-settings-change-event
(`Model/SettingsService.cs`, one `Changed` event, composed in `Program.cs`, `UI/ISettingsHost.cs`), A41a-hotkeys
(`Platform/HotkeyRegistrar.cs`, must not reference `Program`), A41b-clipboard-policy, A42a-math-notes, A42b-note-analysis-file.
Selftest now has 73 `LAYERS` rows plus SETTINGS, HOTKEYS, CLIPBOARD, MATH NOTES, LIST EDITING lines. A quick count already shows 75
`Program.` tokens (new: `Program.SettingsService` x2; `SettingsWindow.cs` now 14 refs).

## Questions (file:line evidence from current code)
1. **Recount** every `Program.*` static member: writers, readers per file/line, totals per member (round 1: 73). Highlight new ones.
2. **Startup/composition now**: how `SettingsService` is created and handed out; how `HotkeyRegistrar` is wired and what it reads
   instead of `Program`; the current order in `Program.Main`/`RunApplication`/`App.OnFrameworkInitializationCompleted`; any
   new static state; what each of A40-A42b already injects (a pattern A43 should copy, e.g. `ISettingsHost`).
3. **Constants vs runtime state** per member, re-decided. Reconfirm `IsDev` (`ANTINOTEPAD_DATA` + `ANTINOTEPAD_DEV=1`) is
   runtime and distinct from `AppInfo.IsDevBuild`. Can `SettingsService` or `ISettingsHost` carry some of this instead of a new
   `AppRuntime`? Should `AppRuntime` still exist, or should smaller pieces be passed (location state, log, isDev)?
4. **Does the round-1 shape and order still hold?** Signatures that changed (`PadWindow`, `SettingsWindow`, `PadView`,
   `SearchOverlay`, `MotionPreview`, `TaskbarProxy`, `App`); which become simpler or harder; whether to split into two tasks
   (e.g. `A43a-` log + IsDev/Taskbar, `A43b-` store + notes-location state + `MoveNotes`) with scope, risk, order for each;
   what to skip.
5. **Isolation path**: trace `ANTINOTEPAD_DATA`, `usesCustomData`, instance id, `--quit`, `--selftest`, `--hidden`, data folder
   and notes-location resolution, migration, and which of these a refactor could touch. List the exact checks that would catch a
   mix-up of dev and installed copies.
6. **Riskiest parts**, each with a concrete failure scenario and how a user could hand-check it: startup order, single instance,
   `--quit` routing, data-folder handling/`MoveNotes`, session-end flush.
7. **New LAYERS rows** in the existing table form (read the current rule table in `SelfTest.cs` and write rows exactly in its
   shape, plus any new rule kind needed such as an exact-type "only Program may reference Program"), saying which hold today.
8. **File overlap** with the current tree: list the files A43 would touch and their current sizes; mention A44-platform-seam.

## Report format (`.coord/reports/A43-app-context-impact-r2.md`)
Tables over prose, under 300 lines: recount table (before/after vs round 1), composition notes, constants vs runtime, shape and
split recommendation, isolation path, risks with hand checks, new LAYERS rows, overlap. Mark anything unverified.

## Done means
Only the report is written (`git status` shows nothing else except `.coord/logs`).
