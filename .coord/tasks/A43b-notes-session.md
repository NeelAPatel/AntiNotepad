# A43b-notes-session: explicit Store/SettingsService + NotesSession owning the notes location

Role: worker (gpt-6-luna xhigh). **Implementation.** Product `product/AntiNotepad`. Read `AGENTS.md`, then the plan
`.coord/tasks/A43b-notes-session-plan.md` (incl. "Recommendation on handling") and the evidence
`.coord/reports/A43b-notes-session-impact.md`. Format reference for finished work: A43a (`RuntimeLog`, `AppLaunch`). Branch
`reorg/A43b-notes-session` (already checked out; do not switch branches, do not commit: you cannot write to `.git`).
Behavior-neutral refactor, highest-risk step of the series: it touches data-folder ownership and moving notes. Line numbers are
from the current tree; re-read before editing. Scope is exactly the plan; everything on its "Drop / leave alone" list stays
untouched (no `Store`/`NotesLocation`/settings-policy changes; do NOT fix the same-note refresh in `PadView.Bind` or in-flight search
after a move; no `RuntimeLog`/`TaskbarTrace` rewrite; no install/migration/native/frame work; no DI container, no `AppRuntime`, no
static `Current`).

## Two stages, one task
Do **stage 1 completely, build and run `--selftest` to PASS, then continue with stage 2**. Write the stage-1 result into the
report before starting stage 2 (the manager may gate stage 1 alone). If you run out of time, stop cleanly after stage 1 and say so.

### Stage 1: services injected
- `App` gets `Store` and `SettingsService` through its constructor via the existing factory (`Configure<App>(() => new App(...))`,
  `AppLaunch` stays). Extend the ctor to `App(AppLaunch, Store, SettingsService)` (stage 2 adds the session).
- `PadWindow` ctor gains `Store store, SettingsService settings`; the `_store` field initializer that reads `Program.Store`
  (`PadWindow.cs:23`) becomes ctor assignment. The `_timers` initializer moves into the ctor after `_store` is assigned and closes
  over the injected store, reading `store.Data.Stack/Slots` on each call (same lambda semantics, same `Timers` class).
- `SearchOverlay` ctor gains `Store store` (sole caller `PadWindow.Keys.cs`); the search context is still created per search.
- `SettingsWindow` keeps its other `Program.` reads for now (stage 2).
- `Program.RunApplication`: `Store` and `SettingsService` become locals passed to the factory; keep `Program.Store`/`SettingsService`
  statics only if stage 2 still needs them inside `Program` (they must be gone at the end of stage 2).
- Stage-1 gate: build 0 warnings, `--selftest` PASS, all 80 existing `LAYERS` lines unchanged.

### Stage 2: `NotesSession`
1. **`Model/INotesLocation.cs`** (`internal interface INotesLocation`): read-only `string DataFolder { get; }` and
   `string? NotesLocationError { get; }`. Use the exact types the current `Program` statics have. This is all `PadView` and
   `MotionPreview` see.
2. **`NotesSession.cs`** (root namespace `AntiNotepad`, `internal sealed class NotesSession : INotesLocation`): ctor
   `(Store store, string appHome, string dataFolder, string? notesLocationError, Action<string> log)`; get-only `Store`, `AppHome`;
   `DataFolder`/`NotesLocationError` with private setters, changing together only inside `MoveNotes`.
   `bool MoveNotes(string target, out string message)` is **moved verbatim from `Program.MoveNotes`** (`Program.cs` ~135-158): same
   guard -> `Store.Save` -> `NotesLocation.Move` -> `SwitchFolder` -> folder/error update -> warning log sequence, success-with-
   unavailable still returns true with the target + unavailable error, synchronous on the UI thread, no new lock/await/event,
   no new `Store`, no disposal. The only change allowed in the body: the warning goes to the injected `log` (`Program` passes
   `log.Write`), and static `Program.X` become the instance fields.
3. **`Program.RunApplication`**: path/service values (`dataFolder`, `appHome` final value after the second assignment, error, `Store`,
   `SettingsService`) are locals; the session is built after load, reconciliation, seeding and the renderer choice, next to
   `AppLaunch`, with the **final** `AppHome` and the same `Store`. Then `App(AppLaunch, Store, SettingsService, NotesSession)`.
   **Delete from `Program`**: `DataFolder`, `AppHome`, `NotesLocationError`, `Store`, `SettingsService`, `_runtimeLog` (the
   set-once log hand-off A43a added for `MoveNotes`), `MoveNotes`. Keep `_window`, `_pendingRequests`, `Attach`, `RequestShow/Quit`.
   Order of `RuntimeLog.SetHome`, migration, `Store` construction (`createDirectories` condition and fallback ctor), instance id /
   `usesCustomData` before `--quit`, `--selftest` early return, `--hidden` via `Contains`: unchanged.
4. **Threading**: `App` -> `PadWindow(... , NotesSession notes)` -> `new SettingsWindow(..., notes)` (caller `PadWindow.Keys.cs`).
   `SettingsWindow` takes the `NotesSession` directly (`ISettingsHost` stays unchanged) and replaces its 5 `Program.DataFolder`,
   3 `Program.NotesLocationError`, 3 `Program.AppHome`, 1 `Program.Store`, 1 `Program.MoveNotes` reads. Keep the `SettingsWindow`
   preflight exactly as is (`_host.Save()`, `LastError` check, before-move snapshots of old folder/home/count/backup root, post-move
   backup text and `Refresh`). `PadView` (callers `PadWindow.Layout.cs`, `MotionPreview.cs`) and `MotionPreview` (caller
   `SettingsWindow.cs`) take an `INotesLocation` and read it **on every use** (no captured string): `PadView.cs` ~274-279 reads the
   live view. No reader may snapshot a folder/error where it reads the static today unless today's code already snapshots it.
5. **`SelfTest.cs`** (existing `LayerRule` table; do not duplicate or change the 80 existing `LAYERS` lines):
   - add rows: `new("Editor", "Program", ExactTo: typeof(Program))`, `new("UI", "Program", ExactTo: typeof(Program))`,
     `new("App", "Program", ExactFrom: typeof(App), ExactTo: typeof(Program))`; plus layer rows that fit the existing schema for
     `NotesSession`/`INotesLocation` (e.g. `Model`/`Editor` must not reference `NotesSession`; `INotesLocation` references nothing of
     UI/Editor/Platform); use the real spellings the existing rows use.
   - universal rule `new("*except Program", "Program", ExactTo: typeof(Program), AnyApplicationTypeExceptTarget: true)`: needs one new
     bool on `LayerRule` selecting every application type outside `Program` and its nested/closure types. Keep the scanner change minimal.
   - metadata allowance: the table's own `typeof(Program)` tokens must pass the guard. Extract rule construction to a named
     `CreateLayerRules()` and allow `OpCodes.Ldtoken` of `Program` only there (method-scoped); reject any `Program` field/method access
     and any `Program` `ldtoken` elsewhere.
   - static exposure check (reflection over static fields/properties incl. backing fields): no type outside `Program` exposes a static of
     type `Store`, `SettingsService`, `RuntimeLog` or `NotesSession`; assert `Program` itself has none. Documented bound: delegate/
     dictionary/object locators are not caught (say so in a comment).
   - env-read confinement: `ANTINOTEPAD_DATA`/`ANTINOTEPAD_DEV` are read only in `Program` (not `HOTKEY`/`RENDER`/tuning keys); match
     the way existing guards inspect IL strings (`ldstr`), minimal.
   - capability check **`NOTES SESSION move`** (temp root, printed in the output list like the others): real `MoveNotes` to an empty
     sibling returns true, same `Store`/`Data`/`SettingsService` references, `DataFolder == Store.Folder == target`, error null, pointer
     follows target, a later save lands in target; occupied target and nested target return false with folder/error/identity unchanged;
     an unavailable-store session refuses and creates nothing. Success-with-unavailable stays a source invariant (no new test seam).
     Use real `Store`/`NotesLocation` APIs; do not change them.
6. **`docs/dev-loop.md`** (only these two lines): line 11 notes-folder cell for the dev copy becomes
   "`build/testdata` initially; after an in-app move, the folder selected by `build/testdata/location.json` (`ANTINOTEPAD_DATA` bypasses
   Documents and first-run migration, and follows its own pointer)". Line 46 sentence becomes "`ANTINOTEPAD_DATA` uses the custom folder as
   app home, bypasses Documents and first-run migration, and follows that home's `location.json` after an in-app move."

## Rules
- Behavior-neutral: same instance ids, `--quit` routing, `--selftest` path, `ANTINOTEPAD_DATA`/`DEV` handling, migration, frame styles,
  log location/text, save -> log -> flush at session end, one `Changed` subscription. US spelling. No behavior fixes, no import cleanup
  beyond what the change needs.
- Never launch or kill AntiNotepad (never run `AntiNotepad.exe --quit`). Do not commit. Do not prove the guards fail yourself
  (the manager does).
- Build: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A43b-codex-check` (0 warnings, 0 errors), then
  `build/A43b-codex-check/AntiNotepad.exe --selftest` must print `SELFTEST PASS` with every existing line (LAYERS 80 + new rows,
  SETTINGS, HOTKEYS, CLIPBOARD, MATH NOTES, LIST EDITING, RUNTIME LOG, TASKBAR TRACE, A29 checks, ...) plus `NOTES SESSION move`.
  Delete `build/A43b-codex-check` afterwards (both after stage 1 and at the end is fine).

## Done means
- Build 0 warnings 0 errors; `SELFTEST PASS`.
- `rg -o 'Program\.\w+' product/AntiNotepad --glob '*.cs'`: baseline **21** (`DataFolder` 6, `Store` 5, `NotesLocationError` 4,
  `AppHome` 3, `SettingsService` 2, `MoveNotes` 1) -> **0** (anything left must be `Program`'s own bootstrap internals; list it). Report
  counts per token before, after stage 1, after stage 2.
- `rg 'static .*(Store|SettingsService|NotesSession|RuntimeLog)' product/AntiNotepad` finds nothing outside `Program` ancestry.
- Docs diff touches only the two `dev-loop.md` lines.
- Report at `.coord/reports/A43b-notes-session.md`: files changed per stage, the `Program.` counts, selftest summary lines for
  `NOTES SESSION move`/LAYERS/static exposure, a list of every reader that went from a static to the live view, any deviation and why.
