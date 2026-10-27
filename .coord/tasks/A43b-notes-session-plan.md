# A43b-notes-session-plan (2026-10-26)

Planning only (investigator role; implementation starts later from a separate flow). Evidence: `.coord/reports/A43b-notes-session-impact.md`
(Codex director re-audit on dev ab4d353, after A43a merged). Spot-checked against the code: 21 `Program.` tokens
(`DataFolder` 6, `Store` 5, `NotesLocationError` 4, `AppHome` 3, `SettingsService` 2, `MoveNotes` 1), `Program.MoveNotes` body
(`Program.cs:135-158`), the existing `Configure<App>(() => new App(launch))` factory (`Program.cs:108`), and the two stale lines in
`docs/dev-loop.md` (11 and 46). Parent plan: `A43-app-context-plan.md`. Order: A39-A42b and A43a done, **A43b next**, then A44.

## Recommendation on handling
- **Still worth doing, as scoped.** A43a removed the log/launch half; what is left is one coherent seam: explicit `Store`/`SettingsService`
  plus live notes-location ownership. No `AppRuntime`, no DI container.
- **One task, not two.** Splitting off "explicit Store/SettingsService injection" removes only 6 of 21 tokens and leaves the risky 15-token
  location seam, at the cost of a second hand-check round. Do it as two review stages inside one task: (1) services injection, (2) `NotesSession`.
- **Settings route:** inject `NotesSession` directly into `SettingsWindow`; leave `ISettingsHost` unchanged (no Store exposed through the host).
  `MotionPreview` and `PadView` get only a read-only location view.
- **Wording fix to the scope:** "delete all `Program` runtime statics" means the service/location statics (`DataFolder`, `AppHome`,
  `NotesLocationError`, `Store`, `SettingsService`, `_runtimeLog`, `MoveNotes`). `_window`, `_pendingRequests`, `Attach`, `RequestShow/Quit` stay in
  `Program`: they are single-instance bootstrap wiring and moving them is churn with no gain.
- **Drop / leave alone:** `RuntimeLog`/`TaskbarTrace` rewrite; changes to `Store`, `NotesLocation`, settings policy; fixing the same-note
  no-refresh in `PadView.Bind` or in-flight search after a move (both existing behavior); install/migration/native/frame work; other statics.
- **Bundle the doc fix** (`docs/dev-loop.md`, in scope): line 11, notes-folder cell for the dev copy becomes
  "`build/testdata` initially; after an in-app move, the folder selected by `build/testdata/location.json` (`ANTINOTEPAD_DATA` bypasses Documents and
  first-run migration, and follows its own pointer)". Line 46 sentence becomes "`ANTINOTEPAD_DATA` uses the custom folder as app home, bypasses
  Documents and first-run migration, and follows that home's `location.json` after an in-app move."
- **A44-platform-seam** afterwards: re-audit its branch against merged A43b (its Editor-only scanner premise and `Program`-in-`GlobalHotkey`/`Tray`
  claims are stale). Do not fold it in here.

## What
- **`NotesSession`** (new `NotesSession.cs`, root namespace, `internal sealed`, implements `INotesLocation`): ctor
  `(Store store, string appHome, string dataFolder, string? notesLocationError, Action<string> log)`; get-only `Store`/`AppHome`; private-set
  `DataFolder`/`NotesLocationError`; `bool MoveNotes(string target, out string message)` moved verbatim (only the warning sink changes to the
  injected `log`). No static `Current`, no new `Store`, no disposal, no async.
- **`INotesLocation`** (new `Model/INotesLocation.cs`): read-only `DataFolder` and `NotesLocationError`. This is all `PadView`/`MotionPreview` see.
- `Program.RunApplication`: service/path values become locals; the session is built after load, reconciliation, seeding and the renderer
  choice (`Program.cs:95-106`), next to `AppLaunch`, with the final `AppHome` (the second assignment, `:78`) and the same `Store`.
- Signature changes (each has one caller unless noted):
  `App(AppLaunch, Store, SettingsService, NotesSession)` through the existing factory;
  `PadWindow(...)` + `NotesSession notes` (sole caller App); `_timers` initializer moves into the ctor after `_store` is assigned and closes over the
  injected store, reading `store.Data.Stack/Slots` on each call;
  `SettingsWindow(...)` + `NotesSession notes` (caller `PadWindow.Keys.cs:169`);
  `SearchOverlay(...)` + `Store` (caller `PadWindow.Keys.cs:156`; context still created per search);
  `PadView(...)` + `INotesLocation` (callers `PadWindow.Layout.cs:115`, `MotionPreview.cs:110`);
  `MotionPreview(...)` + `INotesLocation` (caller `SettingsWindow.cs:71`).
- Unchanged: `Timers`, `Tray`, `AppLaunch`, `RuntimeLog`, `TaskbarProxy/Trace`, `Store`, `NotesLocation`, `SettingsService`, `ISettingsHost`, `AppInfo`.

## Why
- `PadWindow.cs:23` reads `Program.Store` inside a field initializer; `PadView.cs:274-279` and 8 places in `SettingsWindow.cs` read the folder/error
  statics that `MoveNotes` rewrites. Nothing in the code says which readers must stay live and which may snapshot, so a refactor that captures
  early silently shows the old folder after a move.
- A failed or half-finished move hides in globals: today `Program.DataFolder` and `Program.NotesLocationError` are two independent public
  setters-in-effect; the session makes them change together, only inside `MoveNotes`.
- After this task nothing outside `Program` can read startup state, so a startup-order change fails to compile instead of failing late.

## How
Branch `reorg/A43b-notes-session` off `dev` (merge latest `dev` first, recount `rg -o 'Program\.\w+'`, refresh this plan). Codex worker; manager gates.
1. Stage 1, services: `App`, `PadWindow`, `SearchOverlay` take `Store` (and `SettingsService` for App/Pad) explicitly; timers lambda uses the injected store.
   Build gate.
2. Stage 2, session: add `INotesLocation`, `NotesSession`; move `MoveNotes` verbatim; thread the session to `App` -> `PadWindow` -> `SettingsWindow`,
   and the view to `PadView`/`MotionPreview`; delete the statics and `_runtimeLog`; keep `SettingsWindow` preflight (`_host.Save()`, `LastError`
   check, before-move snapshots of old folder/home/count/backup root, post-move backup text and `Refresh`) exactly as is.
3. Guards (`SelfTest.cs`, existing `LayerRule` table, 79 rows now + the settings-setter line = 80 `LAYERS` lines; do not duplicate existing rows):
   - already present, keep: `Platform`, `Model`, `RuntimeLog` -> `Program`.
   - add: `new("Editor", "Program", ExactTo: typeof(Program))`, `new("UI", "Program", ExactTo: typeof(Program))`,
     `new("App", "Program", ExactFrom: typeof(App), ExactTo: typeof(Program))`.
   - add the universal rule `new("*except Program", "Program", ExactTo: typeof(Program), AnyApplicationTypeExceptTarget: true)`; needs one new
     bool on `LayerRule` selecting every application type outside `Program` and its nested/closure types.
   - metadata allowance: the table's own `typeof(Program)` tokens must pass. Extract rule construction into a named `CreateLayerRules()` and allow
     `OpCodes.Ldtoken` of `Program` only there (method-scoped, before `AddType` loses operand provenance); reject any `Program` field/method access, and any
     `Program` `ldtoken` elsewhere.
   - static exposure check (reflection over static fields/properties incl. backing fields): none outside `Program` expose `Store`, `SettingsService`,
     `RuntimeLog`, `NotesSession`; assert `Program` itself has none. Documented bound: delegate/dictionary/object locators are not caught.
   - DATA/DEV env reads confined to `Program` (currently `Program.cs:51,54`); not `HOTKEY`/`RENDER`/tuning keys.
4. Capability check `NOTES SESSION move` (temp root): real `MoveNotes` to an empty sibling returns true, same `Store`/`Data`/`SettingsService` references,
   `DataFolder == Store.Folder == target`, error null, pointer follows target, a later save lands in target; occupied/nested target returns false
   with folder/error/identity unchanged; an unavailable-store session refuses and creates nothing. The success-with-unavailable-switch path stays a
   source invariant (no new test seam).
5. Docs: the two `docs/dev-loop.md` edits above. Status separate commit.

Must stay identical: guard -> `Store.Save` -> `NotesLocation.Move` -> `SwitchFolder` -> folder/error update -> warning log; two saves (host and session);
success-with-unavailable still returns true with the target + unavailable error; synchronous on the UI thread, no new lock, await or event; `usesCustomData`
decides the instance id before `--quit`; sole `--selftest` returns before any bootstrap; `--hidden` via `Contains`; `Store` ctor `createDirectories`
condition and the fallback constructor; one `Changed` subscription; save -> log -> flush at session end; `RuntimeLog` owned by `Main`.

## Success looks like
- `rg -o 'Program\.\w+' product/AntiNotepad --glob '*.cs'`: **21 -> 0** outside `Program.cs` (and `App`'s 3 reads gone: the whole count is 0 except
  `Program`'s own internals).
- `rg 'static .*(Store|SettingsService|NotesSession|RuntimeLog)' product/AntiNotepad` finds none outside `Program` ancestry.
- Build into `build/A43b-check`: 0 errors, 0 warnings.
- `--selftest` PASS: all existing 80 `LAYERS` lines unchanged, the new `Program` rows and universal rule at 0 refs, `NOTES SESSION move` PASS, all other
  capability lines unchanged. The manager shows probes fail and removes them: a `Program.Store` read in `App`, in an Editor method and in a new root type;
  a `Program` field read in SelfTest; a static `Store` field outside `Program`.
- Docs diff touches only the two `dev-loop.md` lines.
- Hand checks below pass.

## Impact radius
Nine existing product files: `Program.cs` (258 lines), `UI/PadWindow.cs` (524), `UI/PadWindow.Keys.cs` (285), `UI/PadWindow.Layout.cs` (493),
`UI/SettingsWindow.cs` (799), `UI/SearchOverlay.cs` (396), `Editor/PadView.cs` (864), `UI/MotionPreview.cs` (299), `SelfTest.cs` (2,049); plus
`docs/dev-loop.md`. New: `NotesSession.cs`, `Model/INotesLocation.cs`. No change to `RuntimeLog.cs`, `TaskbarPresence.cs`, `ISettingsHost.cs`.
No visible change intended.

## Risks and the user's hand checks (dev fixtures only; installed copy never touched)
| Risk | Failure scenario | Hand check |
|---|---|---|
| Data folder / isolation | Dev run reads or writes the real folder; wrong executable or `DATA` | Launch with `tools/dev.ps1`; confirm absolute disposable `DATA`, dev hotkey D and badge, Storage app-home and notes paths inside the fixture, log in the dev home. A DATA-only run has no badge by design |
| `MoveNotes` | Cached path/error stays old, replaced `Store` strands tray/timers, pointer written early | Unique note + slot + running timer; move to an empty sibling; Storage path/Open/recovered and backup text correct; search, tray, timer still work; edit, save, restart reloads the moved text and the log stays in the original app home; move a second time (catches a captured first path) |
| Refusal / unavailable | Occupied/nested target accepted, or a missing pointer target gets created | Occupied/nested target refuses with no pointer or path change; restart with a pointer to a missing folder: it stays absent, read-only hint and Settings error actions as before; a failed Save blocks the move |
| Stale views | Preview or search shows the old folder | After a move: open Settings preview, run a fresh search, switch notes; same-note bind keeps its existing behavior |
| Startup order | A second service or session built before `Load` | Restart: saved theme/material/split intact; change a setting, save, reopen; renderer restart hint matches the actual launch |
| Single instance | Request dropped before attach | Rapid second launch with the same dev data: one dev tray and the pad shows; repeat with a `--hidden` primary |
| `--quit` | Dev quit reaches the installed copy | Set a disposable `ANTINOTEPAD_DATA` (DEV unset, then `=1`) and quit: only the dev copy exits, installed keeps running. Never quit without `ANTINOTEPAD_DATA` |
| Session-end flush (A30, still pending) | Final edit lost or `session-end` line missing | Type a unique last edit, then a user-controlled sign-out/restart; reopen the fixture, edit present and `session-end save=ok` in the home log. Ordinary Quit does not prove this |
| Regression surface | Hotkey, taskbar, tray, previews change | Show/hide by hotkey and taskbar, tray slots, Settings previews feel unchanged (timing is unmeasured by the audit) |

## Depends on / conflicts with
- Needs A43a merged (done). No other task in flight on `PadWindow*`, `SettingsWindow.cs`, `PadView.cs`, `SelfTest.cs`.
- A44-platform-seam follows: shares `PadWindow*`, `SettingsWindow.cs`, `SelfTest.cs`; re-baseline it, serialize `SelfTest.cs` edits, and carry the universal and
  static-exposure guards forward.
