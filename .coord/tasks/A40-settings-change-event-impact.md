# A40-settings-change-event-impact: read-only impact analysis

Role: director (gpt-6.1-sol medium; delegate searches to luna subagents if useful). **READ-ONLY.** Do not edit, create or delete
any file except your report `.coord/reports/A40-settings-change-event-impact.md`. Do not build, launch or kill AntiNotepad.
Product: `product/AntiNotepad`. Read `AGENTS.md` first. Branch `reorg/A40-settings-change-event` (off dev b54b476).

## Context
The user's pain: changing one thing silently breaks another, found much later. Audit A38-mvvm-audit (`git show
feat/A38-mvvm-audit:docs/A38-mvvm-audit.md`) rejected MVVM and proposed behavior-neutral refactors. This task,
`A40-settings-change-event`, is about `Settings` (`Model/Store.cs`, about 55 mutable JSON-serialized properties) having no
change notification. Today a change = mutate, then hand-call `PadWindow.SettingsChanged()` (`UI/PadWindow.Keys.cs`, called from
`UI/SettingsWindow.cs` and the `Store(Action)` helper in it); `PadView.ApplyLook` and `SmoothCaret.SettingsChanged` hang off that.
A missed call = stale UI. `SettingsWindow` also reaches into the pad (`CurrentPalette`, `Toast`, `Save`, `ResetSettings`,
`ShowResident`, `SettingsLostFocus`).
Goal under study: one owner of settings with a single `Changed` event (optionally saying what changed); subscribers react on their
own; `SettingsWindow` stops depending on `PadWindow` for settings; `Settings` stays a plain POCO for JSON.
Assume A39-motion-out-of-ui lands first (branch `reorg/A39-motion-out-of-ui`: moves `Anim`, `ChromeTokens`, `MenuEntry` out of
`UI`, adds a table-driven `LAYERS` selftest in `SelfTest.cs`). Read its brief with
`git show reorg/A39-motion-out-of-ui:.coord/tasks/A39-motion-out-of-ui.md`.

## Find and report (with file:line)
1. Every settings **read** (`Settings` instance, `Store.Settings`, static access) and **write** (property assignment, including
   object-initializer and reflection/JSON paths) per file; counts per file and per property group.
2. Every **propagation** call: `SettingsChanged`, `ApplyLook`, `ApplySettings`-style, palette rebuilds, anything that re-reads
   settings after a change (timers, hotkey re-registration, taskbar mode, tray, blur, top bar, caret, folder move, startup
   registration). For each setting property (or group), say which consumers react and whether they react via the manual call, on
   their own, or only on next show/restart. Flag settings changed with NO propagation call, and propagation with no change.
3. Every `SettingsWindow` -> `PadWindow` (and `MotionPreview`, `PadMenu`, other UI) member use; list members and call counts.
4. Load/save/reset paths in `Store`: first load, migration/defaults, `ResetSettings`, save (including the A30 background save:
   thread, debounce, locking), data-folder move, and what happens to `Settings` object identity (is the instance replaced or
   mutated in place? who holds references to the old one?).
5. Threading: which threads touch `Settings`; any cross-thread read during background save.
6. Selftests touching settings (`SelfTest.cs`): names and what they assert.
7. Hidden behavior: ordering between propagation steps, re-entrancy (a handler that changes a setting), save debouncing, `Anim.Reduced`
   / reduce-motion handling, palette rebuild cost versus show/hide speed (show/hide is the app's priority: say where a
   per-change event could add work to the show/hide path), settings changed while the pad is hidden or the settings window is
   open, and live preview in `MotionPreview`/Settings.

## Report (`.coord/reports/A40-settings-change-event-impact.md`)
- **Impact radius**: files, members, call counts.
- **Coupling map**: who reads/writes/propagates (table).
- **Risks and hidden behavior** from item 7, each with a concrete code reference.
- **1-3 design options** with a recommendation. Consider at least: (a) a `SettingsService` owning the instance with `Changed`
  (with/without a changed-property name or flags), (b) `Changed` on `Store` only, (c) `INotifyPropertyChanged`-style setters (note the JSON
  and POCO constraint). Say how each keeps `Settings` a plain POCO and how `Update(Action<Settings>)`-style mutation would
  notify once per batch.
- **Proposed `LAYERS`/selftest rules** that would catch regressions, using A39's table-driven mechanism (for example: UI
  settings windows must not reference `PadWindow`; only the settings owner may call the save; a guard that every `Settings`
  property setter outside the owner is flagged). Also propose a behavior selftest (change a setting, assert subscribers fired
  once).
- Anything you could not determine, stated plainly.

## Done means
Only the report file is new; `git status` shows nothing else changed. Short, structured, file:line references.
