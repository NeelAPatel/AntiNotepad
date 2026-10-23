# M11-settings-window (with M7-settings)

Done 2026-10-22 by the Claude manager (user: Claude first). Two parts, committed separately; only the pair builds.

## M11a-settings-function (function)
- `Core.Preferences` (material, tint, bars, hotkey, start with Windows, font size, marker opacity, strike); null = token default; `Normalized()` clamps. Stored inside settings.json (`WindowSettings.Preferences`); legacy files load defaults.
- `MainViewModel.Preferences` is merged into every settings save, so window saves (autosave, hide, pin) never wipe preferences (selftest proves it).
- Commands: `settings.toggle`, `settings.material`, `settings.tint`, `settings.bars`, `settings.hotkey`, `settings.hotkey-capture`, `settings.startup`, `settings.font-size`, `settings.marker-dim`, `settings.strike`. Retired: `menu.toggle`, `shell.quiet` (one shell). Theme, New/Delete note, Minimize and Quit routes moved to `settings:` (new route kind; counts for the `yes` rule). Doc table and code mirror updated together.
- Lifecycle: settings is owned by the pad and closes on hide, hotkey toggle, minimize, quit and window close; Esc closes settings first.
- Hotkey: validated, registered, stored; a gesture that cannot register leaves the old one active (rollback); `ANTIQUICKPAD_HOTKEY` still wins; the global hotkey is suspended while the capture box listens; tray tooltip follows.
- Start with Windows: `IStartupService` (HKCU Run, apphost path from `AppContext.BaseDirectory`); the stored value mirrors the real entry; the selftest uses an in-memory fake (registry never touched).

## M11b-settings-window (UI)
- `Views/Common/SettingsWindow.cs`: chrome-free glass window (same material and tint as the pad), 320 DIP wide, placed beside the pad (left if no room), owned by it. Sections: Note, Theme, Background (material + tint), Bars, Hotkey (+ Start with Windows), Editor (font size, marker dimness, strike), Window (Minimize, Quit). Sliders apply after 250 ms of rest. Token accent on Fluent sliders/toggles.
- ⋯ button is now "Settings" (`settings.toggle`); the in-tree More menu is gone.
- Material, tint and bars apply in place (no editor rebuild, no focus change); font size, marker and strike rebuild the editor without stealing focus from settings. "Always" bars keep the bars visible (F56).
- Proofs: menu raster proofs removed (menu gone); render state `menu-open` -> `settings-open` + `settings-window.png`; conformance fault probe moved from Minimize to Pin.

## Gate
Release 0 warnings; `--selftest` exit 0: BOUNDARY PASS (283), CONFORMANCE quiet PASS (31 commands), REVEAL, PRESENTATION, TOKENS, new `M11-settings-window SETTINGS PASS`, F37 hidden-window PASS (settings window hidden, never activated). Renders `.coord/scratch/M11/render-{light,dark}/settings-window.png`.

## Not done / open
- Data folder setting (M7) not built. F58 (keyboard reach to bars) still open. Live glass look of the settings window, real hotkey change and the real Run entry need the user's physical check. `--measure` not re-run.
