# A35g-labels-tooltips

## 1 Summary
Implemented the approved string table and Settings behavior. The pager count remains unchanged.

## 2 Change list
Updated swap and action-tile tooltips, dev badge name, welcome and Help text, save-failure copy, Feel wording, Settings dropdown names, Settings shortcut and generated shortcut table. Updated the design hash after comparing before and after dumps.

## 3 Behavior changes
Split changes refresh the swap label; dropdown names follow picks; resetting one or all shortcuts shows Undo and restores overrides through SettingsService; an expander title match includes its rows in Settings search. Settings defaults to Ctrl+,; Help list reflects the selected list type.

## 4 Out-of-scope files
No files outside the approved list changed by this run. The default gate compares against dev and includes 15 already-committed sibling files from the branch base, outside this item's Files list.

## 5 Plan deviations
The exact requested gate fails its scope step because its default base is dev; reran with -Base HEAD to scope the current implementation. The planned hand test remains for the user. No change to the item card or status.

## 6 Contracts
Added settings-labels-reset-search: dropdown name after a pick, single and all reset Undo, search expander rows, split-aware swap labels, Settings default key and Help list descriptions. The Undo writes through _settings.Update.

## 7 Lens review
Independent lens reviewer: accept, no proven product findings; reviewed scope, callers, state, notifications, search, reset callbacks and design dump. Screen-reader announcements and toast usability remain hand checks.

## 8 Checks
- Task build: 0 warnings, 0 errors; SELFTEST PASS; settings setter violations 0; 47 contracts: 45 pass, 1 manual, 1 deferred.
- Requested offline gate: FAIL only Scope, due to 15 sibling files already committed on this branch relative to dev; build, selftest, seam, tokens, format and loop lint passed.
- Offline gate with -Base HEAD: PASS, including Scope (13 approved files), build, selftest, seam, tokens, format and cleanup. Size warning: UI.cs grew while already over 600 lines. Offline NuGet audit disabled.
- Design dumps: 18,048 to 18,080 lines. All non-Help lines identical. For each palette (contrast-dark, contrast-light, graphite, ink, midnight, paper, sage, snow), changed Help paths are StackPanel indices 14, 15, 16, 17, 30, 31 and 36: group/row reordering and four added control lines for the new Settings shortcut. No other dump lines changed. Hash f08e0c448f3f7a400dd73fe7710bd51e05f6545782c40b3caa6ed5c24466aa79 -> eab0a1acf5b71b93ed6167cafca21bd9c188bb3eb6669d67876ffca43ee43d5e.
- Task build and comparison files removed.

## 9 Hand checks
Pending user hand test in the slot dev copy: swap with split on; action row tiles; Settings Feel; Reset all and Undo; search vertical. Check dropdown screen-reader names and reset toast usability.
