# A43-app-context-plan (revision 2, 2026-10-26)

Planning only (investigator role; implementation starts later from a separate flow). Evidence: `.coord/reports/A43-app-context-impact-r2.md`
(Codex director re-audit on dev 756f139, after A39, A40, A41a, A41b, A42a, A42b landed). Round 1 (`A43-app-context-impact.md`)
is superseded where they differ. Spot-checked against the code: 75 `Program.` tokens; `Store.cs:382` creates the one `SettingsService`;
`Program.cs:346-349` reads `Program.Store/SettingsService`; `NotesLocation.cs:19-25` custom folder follows its own pointer.
Order: A39, A40, A41, A42 (done), **A43**, A44.

## Recommendation on handling
- **Still worth it, at a smaller scope than round 1.** Do not build the broad `AppRuntime`. A40/A41 already set the pattern
  (explicit `Store` + `SettingsService`, `ISettingsHost`, factories into `HotkeyRegistrar`); A43 finishes it with small pieces.
- **Split into two tasks**, in this order, each its own commit and its own user hand check:

| Task | Scope | Risk |
|---|---|---|
| `A43a-runtime-log-and-launch` | New `RuntimeLog` (owns `BackgroundLog`, lazy, empty-home no-op). Inject log + readonly `isDev` into `PadWindow`, an instance `TaskbarTrace`, and `TaskbarProxy`; pass `StartHidden`, `StartupNotice`, actual-renderer bool and the `Attach` callback into `App`. `Store`, notes-location and `MoveNotes` stay on `Program` for now. | Medium-high: crash/exit log lifetime, taskbar trace lifetime (28 `IsDev` reads in `TaskbarPresence`). |
| `A43b-notes-session` | Pass `Store` + `SettingsService` explicitly (App, `PadWindow`, `SearchOverlay`, `Timers` lambda). New `NotesSession`: same `Store`, `AppHome`, live `DataFolder`/`NotesLocationError`, `MoveNotes` moved verbatim. Read-only location view into `PadView`/`MotionPreview`; Settings reaches storage state through `ISettingsHost` or `NotesSession`. Delete all `Program` runtime statics. | High: data-folder handling, stale folder captures, move/save/error sequencing. |

- **Skip:** DI container; `AppInfo.IsDev` (it is runtime, not build identity); folding location/log/renderer into `SettingsService`
  (persisted settings only); other statics (`Anim.Reduced`, `PadView` document cache, `Note.LoadBody`, `SessionEnd`, ...);
  hotkey/clipboard/math/analysis code (needs no change); install scripts, migration, window frame styles; the A44 native seam.
- Doc fix to bundle later (not code): `docs/dev-loop.md:11` says custom `ANTINOTEPAD_DATA` bypasses the pointer, but code and selftest
  show it follows its own `location.json` after an in-app move.

## What
75 qualified `Program.` tokens in 8 files at baseline: `IsDev` 40 (28 `TaskbarPresence`, 12 `PadWindow*`), `Log` 8, `DataFolder` 6,
`Store` 5, `NotesLocationError` 4, `AppHome` 3, `FlushLogs` 2, `SettingsService` 2, and 1 each `Attach`, `StartHidden`,
`StartupNotice`, `SoftwareRendering`, `MoveNotes`. Qualified writes: zero (all assignments are inside `Program`).
- Constant vs runtime: nothing in `Program` is a pure constant. `IsDev` (`ANTINOTEPAD_DATA` set AND `ANTINOTEPAD_DEV=1`,
  `Program.cs:57-59`) is a runtime launch flag, distinct from `AppInfo.IsDevBuild`; it never moves to `AppInfo`.
- Final owners: `RuntimeLog` (log + flush + dispose), readonly launch values, explicit `Store`/`SettingsService`, `NotesSession`
  (folder/error/move). `Program` keeps `Main`, `--selftest`/`--quit` short circuits, env parsing, instance id, single-instance
  wiring, latch, `Attach`, final log disposal.
- Signatures: `PadWindow(Store, SettingsService)` gains narrow log/isDev/location dependencies and builds `_timers` in its ctor;
  `SettingsWindow(SettingsService, ISettingsHost)` keeps its shape (extend host or add `NotesSession` + renderer bool);
  `PadView` and `MotionPreview` add a read-only location view; `SearchOverlay` adds `Store`; `TaskbarProxy` adds `isDev` + `TaskbarTrace`;
  `App` gets an injected internal ctor/factory (Avalonia 12 overload unverified). `Timers`, `Tray`, `HotkeyRegistrar` unchanged.

## Why
Hidden dependencies break distant code on a startup-order or folder change, with no compiler help.
- `PadWindow.cs:23` builds `Timers` with a lambda over `Program.Store`; it works only because `Program.Store` is set before the window
  exists (otherwise `null!`).
- `Program.Log` silently does nothing while `AppHome` is empty, so a log line added before folder resolution vanishes.
- `PadView.cs:274-279` and `SettingsWindow.cs:592-617` read `DataFolder`/`NotesLocationError`, which `MoveNotes` rewrites at runtime; a
  constructor-time copy would show the old folder after a move.
- `Platform/TaskbarPresence.cs` reads `Program.IsDev` 28 times: the Platform layer depends on the app entry point (a rule A41a
  already imposes on `HotkeyRegistrar`).
Explicit parameters make each a compile error; a new `LAYERS` row makes a regression a selftest failure.

## How
Per task, branch `reorg/A43a-...` / `reorg/A43b-...` off `dev` (A43b after A43a merged). Each: merge latest `dev`, recount
`rg -o 'Program\.\w+'`, refresh the plan; Codex worker implements; manager gates (diff review, build, selftest, prove a guard fails).
1. **A43a:** `RuntimeLog`; `Program` creates it before `ProcessExit`/crash handlers (handlers capture it); keep provisional-home,
   lazy creation and never retarget a created logger. `TaskbarTrace(bool isDev, Action<string> log)` replaces the static
   `Trace/TracePadAction/PadActionContext` (pure static decision helpers stay). Pass `isDev` to pad, proxy, layout badge. Keep
   save -> log -> flush order at session end and the `ProcessExit` flush. Add rows for what is removed (below).
2. **A43b:** build `NotesSession` after `Store.Load`, seeding and renderer choice (`Program.cs:101-114`), before Avalonia starts; move
   `MoveNotes` in verbatim (guard, Save, `NotesLocation.Move`, `SwitchFolder` before updating state, warning log, success-with-unavailable).
   Same `Store` and same `SettingsService` identity throughout; one `Changed` subscription stays in the composition root. Delete
   `Program` statics. Add the universal rule and one temp-folder move capability check.
3. Unchanged in both: instance split (`usesCustomData` decides instance id before `--quit`, `IsDev` independent), `--selftest` returning
   before any initialization, `--hidden` via `Contains`, migration, pointer format, hotkey path (readonly reads only).

New `LAYERS` rows (existing `LayerRule` table shape; A43a adds what holds then, A43b the rest):
```csharp
new("Platform", "Program", ExactTo: typeof(Program)),        // holds after A43a
new("Model", "Program", ExactTo: typeof(Program)),           // holds today
new("Editor", "Program", ExactTo: typeof(Program)),          // after A43b
new("UI", "Program", ExactTo: typeof(Program)),              // after A43b
new("App", "Program", ExactFrom: typeof(App), ExactTo: typeof(Program)),  // after A43b
new("*except Program", "Program", ExactTo: typeof(Program), AnyApplicationTypeExceptTarget: true), // final universal rule
```
The universal rule needs one small `LayerRule` extension (select every application type outside `Program`'s declaring-type ancestry)
and an operand-aware allowance for the `typeof(Program)` `ldtoken` operands inside SelfTest's own rule-table initializer only.
Separate rule kinds (need IL/field checks, keep minimal): no static field/property outside `Program` exposing `Store`,
`SettingsService`, `RuntimeLog` or `NotesSession`; `ANTINOTEPAD_DATA`/`DEV` reads only in `Program` (not `HOTKEY`/`RENDER`).
Reflection or string-built lookups remain an uncheckable bound.

## Success looks like
- `rg -o 'Program\.\w+' product/AntiNotepad --glob '*.cs'`: **75 -> 0** outside `Program.cs` after A43b (after A43a: `IsDev`, `Log`,
  `FlushLogs`, `StartHidden`, `StartupNotice`, `SoftwareRendering`, `Attach` = 0 outside).
- `--selftest` PASS: existing 73 `LAYERS` lines unchanged, new `Program` rows all 0 refs, `SETTINGS`/`HOTKEYS`/`CLIPBOARD`/`MATH NOTES`/
  `LIST EDITING` unchanged; a deliberate `Program.Store` read in App, in Editor and in a new root type each fails the guard, then is removed.
- New capability check (A43b): temp-folder move keeps the same `Store` reference and updates folder/error together.
- Build into `build/A43x-check`: 0 errors, 0 warnings.
- Live hand checks by the user in the dev copy (`tools/dev.ps1`), listed under Risks.

## Impact radius
10 existing product files: `Program.cs`, `Platform/TaskbarPresence.cs`, `UI/PadWindow.cs`, `PadWindow.Keys.cs`, `PadWindow.Layout.cs`,
`UI/SettingsWindow.cs`, `UI/SearchOverlay.cs`, `Editor/PadView.cs`, `UI/MotionPreview.cs`, `SelfTest.cs` (+ optional `UI/ISettingsHost.cs`);
new `RuntimeLog.cs`, `NotesSession.cs`. Untouched: `AppInfo.cs`, `SettingsService`, `Store`, `NotesLocation`, `HotkeyRegistrar`,
`MathNotes`, `ListEditing`, `NoteAnalysis`. No user-visible change intended.

## Risks and the user's hand checks (dev fixtures only, installed copy never touched)
| Risk | Failure scenario | Hand check |
|---|---|---|
| Startup order | A second `SettingsService` or a runtime built before `Load` gives defaults or the wrong renderer | Restart; theme/material/split intact; change a setting, save, reopen; restart hint matches the actual renderer |
| Single instance | Request dropped before attach, or new locking blocks startup | Launch twice quickly with the same dev data; one tray icon; second launch shows the pad; repeat with `--hidden` primary |
| `--quit` routing | Instance id taken from `IsDev` sends a DATA-only quit to the installed copy | With a disposable `ANTINOTEPAD_DATA`, `DEV` unset then `=1`, quit; only the dev copy exits and the installed copy keeps running. Never quit without `ANTINOTEPAD_DATA` |
| Data folder / `MoveNotes` | Cached path or error stays old; replaced `Store` strands tray/timers; pointer written too early | Move to an empty sibling; Storage paths, Open, recovered, tray slots, search and timers work; save and restart reload the moved notes; occupied/nested target rejected; missing pointer target stays read-only |
| Session-end flush | Log disposed before the save outcome, or queued writes abandoned | Type a unique edit, sign out or restart; reopen the fixture, edit present and `session-end save=ok` in the dev log (the still-pending A30 check) |
| Crash/exit log | Handler captures an unassigned logger; log moves home after a notes move | Log stays in the app-home file before and after a move; early-crash path covered by a synthetic capability check, not a manual crash |
| Taskbar trace | Per-call trace or reset timestamp loses click-cause correlation; callback order changes activation | Repeat taskbar hide/show, hotkey show, blur hide, hidden start; no bounce or focus regression |
| Isolation | Dev run touches real data | Dev hotkey D and badge present; both Storage paths under the disposable root; log in the dev home |
| Avalonia factory | `App` factory overload may not exist as assumed | Verify at implementation; fallback is one set-once hand-off confined to `Program.cs`, which the guard must still accept |

## Depends on / conflicts with
- **A39, A40, A41a/b, A42a/b:** landed; A43 reuses the `LAYERS` scanner/table and the `ISettingsHost`/factory pattern. No overlap left except shared files (`SelfTest.cs`, `PadWindow*`, `SettingsWindow.cs`, `PadView.cs`).
- **A44-platform-seam (after A43):** shares `TaskbarPresence.cs`, `PadWindow*`, `SettingsWindow.cs`, `SelfTest.cs`. Re-audit its plan (branch `1cb4e33`) after A43b; its Editor-only scanner and `Program`-in-`GlobalHotkey`/`Tray` claims are stale. Do not fold the native seam into A43.
- **A43a before A43b;** serialize edits to `SelfTest.cs` and the three `PadWindow*` files; no other task in flight on them.
