# A40-settings-change-event: one owner and one change event for settings, and guard it

Role: worker (gpt-6-luna xhigh). Branch `reorg/A40-settings-change-event-impl` (stay on it). You cannot commit.
Product: `product/AntiNotepad`. Read `AGENTS.md` first. Design source (read fully, it is the spec):
`.coord/tasks/A40-settings-change-event-plan.md`, evidence `.coord/reports/A40-settings-change-event-impact.md`.
The plan was written before A39-motion-out-of-ui merged. Since then: `Anim` -> `Motion/`, `ChromeTokens` -> `Theme/`, the
context menu is shown via the host, and `SelfTest.cs` has a table-driven IL `LAYERS` check (`LayerRules`, `CheckLayers`).
Line numbers in the plan are stale: re-verify every call site with your own search before editing.

## Why
The user's pain: changing one thing silently breaks another, found much later. Settings are a plain object written from many
places with a hand-called `PadWindow.SettingsChanged()`; forget the call and the UI is stale or the change is not saved.
This task gives settings one owner and one change event, and makes `SettingsWindow` independent of `PadWindow`.
**Behavior-neutral**, except one flagged fix: `SearchGrid`, `SplitSlot` and `Split` changes get saved (they set no dirty flag today).
Show/hide speed is the priority: geometry-only (X/Y/Width/Height), split and search changes must NOT run the appearance chain.

## Design (fixed; option A of the plan)
- `Model/SettingsService.cs` (+ `SettingsChange` type) in `AntiNotepad.Model`, no UI/Editor references. `Settings` stays a JSON POCO,
  same shape and schema.
- API: `Current` (live POCO; identity changes only on `Replace`), `Update(Action<Settings>)` runs the mutation, diffs before/after
  (scalar compare plus a copied `DisabledLanguages` list; no JSON serialization per change) and raises **one**
  `Changed(SettingsChange)` after the outermost batch. `Replace(Settings)` (reset, undo, load) raises once with all flags.
  A nested `Update` from inside a handler is queued and emitted as one follow-up batch, never recursively. A no-op batch raises
  nothing. UI-thread only (document it; a cheap assertion is fine).
- `SettingsChange`: flags enum (Appearance, Palette, EditorLook, Analysis, Motion, Caret, Chrome, Hotkeys, WindowPolicy, Taskbar,
  Geometry, Split, Search, Persistence; adjust names to what the property mapping needs) plus the changed property names.
  Put the property -> flags mapping in one table in the service so a new setting is classified in one place; an unmapped
  property must default to Appearance|Persistence (safe), never to nothing.
- `PadWindow` subscribes with ONE ordered handler that keeps today's `SettingsChanged()` order exactly: Topmost, taskbar proxy,
  `ApplyTaskbarSetting`, `ApplyAppearance` (palette, `Anim.Reduced`, disabled-language hash before pane `ApplyLook`, chrome, bars,
  blur, overlay reopen, settings `SyncLook`), then `Edited()`. Flags decide which parts run: Geometry/Split/Search-only changes
  skip the appearance chain and only schedule the save.
- `SettingsWindow` + `SettingsParts` take the service plus a small host interface (e.g. `ISettingsHost`, in `UI/`, implemented by
  `PadWindow`) instead of `PadWindow`: palette source (`CurrentPalette`), `Toast`, `Icon`, hotkey status/register for both
  hotkeys (keeps "register first, commit only on success"), `ShowResident`, `OpenHelp`, `Save`, `ResetSettings`,
  `SettingsLostFocus`. `Set`/`Store` helpers call `service.Update`; the hand-called `SettingsChanged()` and its 3 call sites go away.
- Reset / Undo go through `Replace`: keep the preserved fields (Width/Height/X/Y/Split/SplitSlot, `Welcomed = true`), keep the
  retained-reference refresh (`PadView.Settings`, `MotionPreview` settings, both panes `ApplyLook`) and the hotkey re-registration
  exactly as `ResetSettings` does today.
- `Store.Data.Settings` is the service's `Current`; loading/normalizing in `Store` goes through the owner (document any in-place
  normalize in `Store.cs` as an owner-internal path). The A30 background writer keeps serializing a detached clone
  (`CopyForSave`); never raise `Changed` from the writer.
- NOT in scope: Folding / InlineMath partial propagation (applied only on Bind / Analyse today). Do not fix them; list them in
  the report as still open. Startup registration (HKCU) and the notes-folder pointer stay outside the event.

## Run scope (two sub-steps; this brief is gated after each)
**r1 (first run: do ONLY this)**
1. `SettingsService`, `SettingsChange`, composition: `Program.cs` / `Store` / `PadWindow` hold the service; `PadWindow`'s single
   ordered handler replaces `SettingsChanged()`; save scheduling attached once to `Changed` with Persistence.
2. `SettingsWindow`/`SettingsParts` on the host interface; no `PadWindow` token in those files; `Set`/`Store` use `Update`.
3. `ResetSettings` + Undo via `Replace`.
4. Guards (below): the two exact-type layer rules and the `SETTINGS Changed` check.
Leave the other direct writers (zoom, pin toggle, window bounds, split navigation, `SearchOverlay`, etc.) as they are for r1,
but they must still compile and behave as before.

**r2 (separate run, brief `-r2` written after r1 is gated; listed so you design for it)**
Migrate the remaining ~67 direct writes (zoom `PadWindow.Keys`, pin toggle, window bounds `PadWindow.cs`, split navigation
`PadWindow.Navigation`, `SearchOverlay` SearchGrid, `DisabledLanguages` via an owner method such as `SetLanguageEnabled`) to
`Update` with precise flags, and add the `Settings.set_*` member-token guard.

## Guards (extend the A39 table; keep every existing `LAYERS` line)
- Extend `LayerRule` so a rule can name exact types as well as namespaces (e.g. from `AntiNotepad.UI.SettingsWindow` to
  `AntiNotepad.UI.PadWindow`; from `AntiNotepad.Model.SettingsService` to `UI`/`Editor`). Nested and compiler-generated types of
  the named `from` type count (lambdas live in nested types). Output lines, one each:
  `LAYERS SettingsWindow->PadWindow 0 refs` (include `SettingsParts` and their nested types in the `from` set),
  `LAYERS SettingsService->UI/Editor 0 refs` (or two lines if cleaner).
- (r2) member-token rule: calls to `Settings.set_*` from anywhere except the allowed set (service methods and the `Update`
  lambdas that run inside it, JSON/default construction, `SelfTest` fixtures) must be 0: `LAYERS Settings.set_* outside owner 0`.
- `SETTINGS Changed` behavior check (a selftest in the style of the others), with two fake subscribers: `Update` changing
  FontSize + Theme fires exactly once with both names; a no-op update fires 0; a nested `Update` fires once more, not
  recursively; a `DisabledLanguages` list edit is detected; `Replace` fires once and `Current` identity is the new object; a
  detached subscriber is not called; a geometry-only update does not reach a fake appearance subscriber (flag test).
- **Prove each new guard fails**: for each new rule, temporarily add a violation (e.g. a `PadWindow` reference in
  `SettingsWindow`; a `UI` reference in `SettingsService`), confirm the selftest goes red and names it, then remove it. Say in the
  report that you did, with the failing line you saw. Never leave a violation in the tree.

## Rules
- Never launch or kill AntiNotepad (not even `--quit`). Build into `build/A40-check`, run `build/A40-check/AntiNotepad.exe
  --selftest` there, delete the folder at the end. `AVALONIA_TELEMETRY_OPTOUT=1` for builds. 0 warnings required. No new packages.
- Match surrounding style: comment density, naming. No unrelated renames, reformatting or cleanups.
- Keep: appearance-chain order, hotkey register-before-commit, reset/undo preserved fields, the 150 ms search timer / `rows` flag
  semantics in Settings.

## Done means (r1)
- `grep -rn "SettingsChanged()" product/AntiNotepad` finds no hand call into `PadWindow` (`SmoothCaret.SettingsChanged` in
  `Editor/Motion.cs` is a different thing and may stay); `grep -rn "PadWindow" product/AntiNotepad/UI/SettingsWindow*.cs
  product/AntiNotepad/UI/SettingsParts.cs` finds nothing.
- Build 0 warnings, 0 errors; `SELFTEST PASS` with all previous lines, the new `LAYERS` lines and the `SETTINGS Changed` line.
- Report at `.coord/reports/A40-settings-change-event.md`: files changed, the property -> flags table, how ordering and
  nesting work, guard proofs (what you violated, what failed), the Folding/InlineMath leftovers, what r2 still has to migrate
  (list every remaining direct write by file).
