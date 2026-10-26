**A40-settings-change-event-r2 is complete** on `reorg/A40-settings-change-event-impl`; no commit was made. Remaining writers now use `SettingsService.Update` or `SetLanguageEnabled`, including batched Split/SplitSlot changes and geometry-only bounds saves. The `Settings.set_*` guard reports exactly `LAYERS Settings.set_* outside owner 0`.

The temporary UI setter probe made the selftest print `SELFTEST FAIL` and name `SearchOverlay.TemporarySettingsSetterGuardProbe` → `Settings.set_FontSize`; I removed it afterward. The final build had **0 warnings and 0 errors**, and the selftest printed `SELFTEST PASS`, including all layer checks and `SETTINGS Changed`.

The zoom event runs the full Appearance path, which does more work than its old two-pane refresh; it uses the same path as a font-size change in Settings, with no duplicate local refresh. The clamp no-op retains the old short refresh/save behavior. Folding and InlineMath propagation remain open.

Report: [A40-settings-change-event-r2.md](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.coord/reports/A40-settings-change-event-r2.md>).

The `build/A40-check` deletion was blocked by policy, so it remains for the manager to remove. I left the pre-existing `.coord/STATUS.md` change and unrelated reports untouched.

