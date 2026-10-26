# A40-settings-change-event-r2: migrate the remaining direct settings writers, add the setter guard

Role: worker (gpt-6-luna xhigh). Branch `reorg/A40-settings-change-event-impl` (stay on it). You cannot commit.
Product: `product/AntiNotepad`. Read `AGENTS.md`, then the original brief `.coord/tasks/A40-settings-change-event.md` (design, rules,
guards: the same rules apply), the plan `.coord/tasks/A40-settings-change-event-plan.md`, and the r1 report
`.coord/reports/A40-settings-change-event.md` (committed r1 = `SettingsService`, `ISettingsHost`, one ordered handler
`PadWindow.HandleSettingsChanged`, property -> flags table). Do not touch STATUS.md.

## Work
1. Migrate every remaining runtime writer of a `Settings` property to `_settingsService.Update(...)` (or the service reachable from
   that class), per the r1 report's inventory; re-verify with your own search (`grep` for `Settings\.\w+ =`, `\.Split`, `Pinned`...):
   - `UI/PadWindow.Keys.cs` zoom (`FontSize`): behavior-neutral; it must keep its current short path result (what it does today),
     now driven by the event; do not make zoom slower or run extra work beyond what the Appearance flag already means. If the shared
     appearance chain would be heavier than today's zoom path, report that instead of silently accepting it, and route zoom through
     `Update` anyway only if the cost is the same order as the font settings path (state your finding in the report).
   - `UI/PadWindow.cs` `RememberBounds` (X/Y/Width/Height): `Update`; Geometry-only, so no appearance chain, only the save schedule.
   - `UI/PadWindow.Navigation.cs`: `Split`, `SplitSlot` (conflict repair, selection, cycle), `Pinned` toggle. Split/SplitSlot
     are Split-only (no appearance chain). Pinned is WindowPolicy|Appearance as in the table.
   - `UI/SearchOverlay.cs` `SearchGrid` toggle: `Update`.
   - `UI/SettingsWindow.cs` `DisabledLanguages` Add/Remove: add an owner method on the service (e.g. `SetLanguageEnabled(string, bool)`)
     implemented with `Update`; the window calls it.
   - `Model/Welcome.cs` `Welcomed` seed write: through the service if it runs on a live service; if it runs before the service
     exists or on a detached object, leave it and list it as an exemption with the reason.
   Batch writes that belong together into one `Update` (e.g. split toggle plus slot) so one event is emitted.
2. **Deliberate behavior fix (the only one):** `SearchGrid`, `SplitSlot`, `Split` changes now get saved (they previously set no dirty
   flag). Everything else must stay as before. Do NOT fix Folding / InlineMath propagation.
3. **Setter guard** (extend the r1 table-driven `LAYERS` machinery with a member-token rule): scan IL of all app types for calls
   (`call`/`callvirt`) to `Settings.set_*` (and `List<string>` mutation of `Settings.DisabledLanguages` is covered by the owner
   method, no token rule needed). Allowed callers: `SettingsService` (and its nested/lambda types), JSON/default construction
   (`Settings` itself and its object initializers in `Model/Store.cs` defaults, `Welcome` if exempted), `SelfTest` fixtures, and
   `Update`/`Replace` lambdas. Because lambdas passed to `Update` live in compiler-generated nested types of the *caller*, the rule
   must allow a setter call only from lambdas that are passed to the service: simplest sound approach is an allow-list by
   caller-type + method name recorded in the table, reviewed by you; keep it small and explain it in the report. Output line:
   `LAYERS Settings.set_* outside owner 0`. Setters called from an object initializer (`new Settings { Width = ... }`) in
   `ResetSettings` must be allowed explicitly.
4. **Prove the guard fails**: temporarily add a direct `Settings.X = 1` style write in some UI class (outside an allowed caller),
   confirm the selftest goes red naming caller and setter, then remove it. Quote the failing line in the report.
5. Extend the `SETTINGS Changed` selftest only if needed (e.g. a Split-only update raises Split+Persistence, no Appearance; a
   `SetLanguageEnabled` call is detected).

## Rules
- Never launch or kill AntiNotepad. Build into `build/A40-check`, run `--selftest` there; delete the folder at the end if your
  policy allows, otherwise say so (the manager removes it). `AVALONIA_TELEMETRY_OPTOUT=1`. 0 warnings. No new packages.
- Match style; no unrelated renames or reformatting. Keep appearance-chain order, hotkey register-before-commit, reset/undo
  preserved fields, and the show/hide path free of extra work.

## Done means
- No remaining direct `Settings` property write outside the allowed set (selftest line above = 0).
- Build 0 warnings; `SELFTEST PASS` with every earlier `LAYERS` line, the new `LAYERS Settings.set_* outside owner 0` line and
  `SETTINGS Changed`.
- Report `.coord/reports/A40-settings-change-event-r2.md`: every writer migrated (file, old -> new), the exemptions, the guard
  allow-list and why, the guard proof, the zoom finding, Folding/InlineMath still open.
