# A43-app-context-impact: read-only impact analysis of the `Program` statics

Role: director (gpt-6.1-sol medium). **READ-ONLY.** Edit no file except your report `.coord/reports/A43-app-context-impact.md`.
Do not build into the repo tree, do not launch or kill AntiNotepad (never `--quit`). Branch `reorg/A43-app-context`; product
`product/AntiNotepad`. Read `AGENTS.md` first.

## Context
Audit `A38-mvvm-audit` proposed `A43-app-context`: stop reading hidden globals from `Program`. Pure constants (e.g. `IsDev`)
go to `AppInfo`; runtime services (store, log, folders, startup notices) are passed explicitly (an `AppContext`-style object
or constructor parameters). No DI container. Behavior-neutral; the dev/installed split (`ANTINOTEPAD_DATA`, instance ids,
`--quit`, `--selftest`) must stay identical. Note: `Program.IsDev` is NOT a compile-time constant: it is
`ANTINOTEPAD_DATA` set AND `ANTINOTEPAD_DEV=1`, decided at runtime in `Program.RunApplication`; `AppInfo.IsDevBuild` is the
separate compile-time identity. Say so in the report and propose where each belongs.
Assume `reorg/A39-motion-out-of-ui` lands first (moves `Anim`, adds a table-driven `LAYERS` selftest): see
`git show reorg/A39-motion-out-of-ui:.coord/tasks/A39-motion-out-of-ui.md`. Sibling plans (A40 settings event, A41 padwindow
split, A42 padview commands, A44 platform seam) are on branches `reorg/A40-*`, `reorg/A41-*`, `reorg/A42-*`,
`claude/a44-platform-seam-*`; skim their briefs/diffs only to judge file overlap.

## Questions to answer (with file:line evidence, from the code, not from this brief)
1. **Usage table**: every `static` member of `Program` (`DataFolder`, `AppHome`, `NotesLocationError`, `StartupNotice`,
   `StartHidden`, `IsDev`, `SoftwareRendering`, `Store`, `Log`, `FlushLogs`, `MoveNotes`, `Attach`, any other): its writers and
   every reader (`file:line`), grouped per file with counts. Include indirect readers (e.g. `PadWindow` building `Timers`
   with a lambda over `Program.Store`; `TaskbarPresence` reading `Program.IsDev`).
2. **Initialization order** in `Program.Main`/`RunApplication`/`App.OnFrameworkInitializationCompleted`: for each member, when it
   becomes valid, what happens if read earlier (null `Store`, empty `AppHome` makes `Log` a silent no-op, etc.), and which
   members change **after** startup (`MoveNotes` rewrites `DataFolder`, `NotesLocationError`, swaps the store folder).
   Flag every reader that caches a value at construction and would go stale vs reads the live static each time.
3. **Threads**: which members are touched off the UI thread (A30 background save and `BackgroundLog`, search body readers,
   `ProcessExit`/`UnhandledException` handlers, `SingleInstanceService` callbacks) and what is locked.
4. **Selftest dependence**: does `SelfTest.cs` or anything it calls read `Program.*`? What does it see (all defaults:
   `IsDev` false, `Store` null)? Would a context object change what selftest exercises?
5. **Other static state** that is the same kind of hidden global: `Anim.Reduced`, the `PadView` static document cache
   (`Documents`, `CachedDocuments`), `Note.InvalidateSearchBody`/search caches, any `static` mutable field or `static` settings
   reader in `Model`, `Editor`, `UI`, `Platform`. List each with readers/writers and say whether it is in scope for
   `A43-app-context` (default: only `Program`; list the others as out-of-scope candidates).
6. **Proposed shape**: (a) exactly what moves to `AppInfo` (constants) vs what is runtime state; decide where the runtime
   `IsDev` lives (e.g. `AppInfo` static set once, vs a context property) and justify; (b) the passed object: name, members
   (immutable where possible; which parts must stay mutable, e.g. the notes folder after `MoveNotes`), who constructs it and
   when; (c) every constructor/method signature that changes (`PadWindow`, `SettingsWindow`, `SearchOverlay`, `PadView`,
   `TaskbarPresence`, `Timers`, `Tray`, ...), and how `Log` is reached from classes with no context today (a `Log` delegate or
   an `ILog`-like seam; keep it tiny); (d) what stays in `Program` (composition root: `Main`, instance id, `--quit`,
   `--selftest`, `Attach`, single-instance wiring) and how `MoveNotes` should be restructured without changing behavior.
7. **Risks**, each with a concrete failure scenario: startup order, single-instance and `--quit` routing to the correct
   instance (instance id decided before `--quit`), `NotesLocationError`/`Store.MarkUnavailable` flows, session-end flush
   (`PrepareForSessionEnd`, `FlushLogs`, `ProcessExit`), selftest, hot-path cost (show/hide speed: extra indirection or
   allocation per hotkey), dev vs installed isolation.
8. **Proposed guard rules** for the A39 table-driven `LAYERS` selftest, expressed as `from -> to` pairs or a new rule kind
   (e.g. "no type outside the composition root may reference `Program`", plus a check that `IsDev`-style runtime flags are not
   readable through a static other than the sanctioned one). Say which rules hold today and which only after A43. Note the IL scan
   needs `Program` to be recognizable (it is `internal static class AntiNotepad.Program`).
9. **File overlap and ordering**: files `A43-app-context` would touch vs the files A39, A40, A41, A42, A44 touch; recommend an order.
   Count files, and `Program.` references before (exact numbers per member, from grep) so success can be measured.

## Report format (`.coord/reports/A43-app-context-impact.md`)
Short, tables over prose: usage table; constants vs runtime state; init-order timeline; thread notes; selftest notes; other
statics (in/out of scope); proposed shape with signatures; risks; guard rules; overlap/order. Mark anything you could not
verify. Under 400 lines.

## Done means
Report written; no other file changed (`git status` shows only the report, and `.coord/logs`, which the wrapper writes).
