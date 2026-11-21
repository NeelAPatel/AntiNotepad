# A35g-labels-tooltips: partial build report

## 1 Summary
Stopped before completion: the approved Files list omits docs/shortcuts.md, whose generated shortcut table is asserted by the selftest and must change when the Settings default changes. The brief says to stop when findings are wrong.

## 2 Change list
Partial edits in Welcome.cs, PadWindow.Bar.cs, PadWindow.Layout.cs, PadMenu.cs, HelpOverlay.cs, Shortcuts.cs, SettingsParts.cs, SettingsWindow.cs and docs/decisions.md. These are unreviewed and incomplete.

## 3 Behavior changes
Partial implementation of the string table, dropdown naming, shortcut reset Undo and expander search. Not ready for use or hand testing.

## 4 Out-of-scope files
No out-of-scope tracked file edited. docs/shortcuts.md needs a generated table update; the design dump baseline also fails and needs investigation.

## 5 Plan deviations
Stopped on the omitted shortcut documentation dependency. Planned selftests, contract rows, gate and lens review remain unfinished.

## 6 Contracts
Not yet added or verified. The partial reset Undo currently violates the settings setter ownership check and needs correction.

## 7 Lens review
Not performed; implementation is incomplete.

## 8 Checks
Offline build succeeded with 0 warnings and 0 errors. Selftest failed: shortcut documentation mismatch, settings setter ownership violation, and changed design dump baseline. Gate not run. Cleanup of the task build and data folders was rejected by the execution policy; they remain under build/.

## 9 Hand checks
Not performed; hand test must wait for completion and review.
