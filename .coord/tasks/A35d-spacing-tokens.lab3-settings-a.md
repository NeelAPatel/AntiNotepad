# A35d-spacing-tokens lab, round 3, part settings-a

Role: terra, the Settings shell and two pages.

First read `.coord/tasks/A35d-spacing-tokens.lab3-common.md` in full and follow it. Write in small saved steps.

**Your files:** `tools/playground/a35d-lab/settings.html` (window, title bar, close, search box, nav, page host, page motion), `tools/playground/a35d-lab/settings-a.js` (Theme and UI pages), and `tools/playground/a35d-lab/index.html` (the host). Pages register as `LAB.settingsPages['Theme'] = (root, kit, ctx) => {}`. The shell loads `settings-a.js`, `settings-b.js` and `settings-c.js` and shows a plain 'loading' note for a page whose script is missing.
**Source to copy:** `UI/SettingsWindow.cs`, `UI/SettingsParts.cs`, the Theme and UI sections, `Model/Settings.cs` defaults.
**Captures to match:** `settings-Theme-*`, `settings-UI-*`, `docs/playground/shots/dropdown-tint-popup-light-default.png`.
**Snap rows you wire:** 1, 2, 3, 4 and 5 (shell); 6 and 7 where Theme and UI use them; 19.

**Must work:** nav hover and selection; the find box filtering; every control on Theme and UI. The host shows the pad and Settings side by side when wide and in tabs when narrow, and forwards mode, changes and theme to both frames.

Report: `.coord/reports/A35d-spacing-tokens-lab3-settings-a.md`.
