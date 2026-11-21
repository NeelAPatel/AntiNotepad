# A35d-spacing-tokens lab 3: settings-c

## 1. Files written
- `tools/playground/a35d-lab/settings-c.js`
- `.coord/reports/A35d-spacing-tokens-lab3-settings-c.md`

## 2. Controls that work
- System: both hotkey recorders, open-position choices and expander, taskbar segments, fresh-note and save-delay dropdowns, and the three window switches. Values are local lab state.
- Shortcuts: all 31 source definitions, change/save/cancel/clear, individual reset and reset-all, and a simplified conflict confirmation. The recorder displays key caps and captures keyboard events; global keys require a modifier and in-window shortcuts also accept unmodified function keys. Reserved-key validation and digit-family conflict detection are approximations, not the complete product rules.
- Storage: simulated open, folder entry, cancel and migration; no filesystem access or real folder picker. About: local shortcuts preview, restore and Undo. Restore covers this file's state and the Settings shell's shared Theme/UI state; **Editor/Feel state is private to settings-b.js and is not reset or restored**. The shortcuts preview and confirmation use local approximations, not the pad's real overlay/dialog.

## 3. Snap rows wired
- 5: shortcut recorder/reset control stacks and Storage action stack, 6 to 8 px.
- 6: System dropdown popup item spacing, 1 to 2 px, using the core dropdown and marked list.
- 19: Storage path field, 3 to 4 px, using core Fluent styling and marked field. Other controls here are SettingsParts custom controls rather than Fluent controls.

## 4. Captures and remaining differences
- `settings-System-0`: supplied capture has slot-specific Ctrl+Alt+Shift+1 and Ctrl+Alt+Shift+F1; lab uses product defaults Ctrl+Alt+Shift+S and None. Open-position tile art is approximated. Shared backdrop and controls are approximations; pixel equivalence is unverified.
- `settings-System-1` through `settings-System-5`: same lower-page content in the supplied images. Shared backdrop, controls, spacing and scrollbar have not been rendered or pixel-compared.
- `settings-Shortcuts-0` through `settings-Shortcuts-5`: source rows and text included; shared controls, scrolling, keycap layout, line wrapping and interactions have not been rendered or pixel-compared.
- `settings-Storage-0`: source path and content included; wrapped path field and simulated actions may differ from native presentation. Shared styling and geometry have not been rendered or pixel-compared.
- `settings-Storage-1` through `settings-Storage-5`: supplied images also show a separate pad overlapping the Settings window; this page does not reproduce that overlap. Other Storage differences are as above.
- `settings-About-0`: source content included; shared styling and geometry have not been rendered or pixel-compared.
- `settings-About-1` through `settings-About-5`: supplied images also show a separate pad overlapping Settings; this page does not reproduce that overlap. Other About differences are as above.

## 5. Local helpers
Page mounting/refresh, key caps and recorder, simulated folder flow, shortcut data and local help preview, and state snapshot for restore/Undo. Core lacks recorder, folder flow and shared global reset APIs.

## 6. Checks run
- `node --check tools/playground/a35d-lab/settings-c.js`: passed.
- Offline Node DOM-stub construction smoke: System 9 rows, Shortcuts 32, Storage 3, About 3. This is not a browser, visual, focus, keyboard or interaction test.
- Read the shell's `LAB.settingsPages(name)` call contract and used its `(root, kit, ctx)` signature. This file uses no DOM IDs; its class selectors were checked against `core.js` and `core.css`.
- Inspected all 24 supplied captures. No app, browser, server or port was launched, per the shared brief.
