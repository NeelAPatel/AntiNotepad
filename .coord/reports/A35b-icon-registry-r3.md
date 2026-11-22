# A35b-icon-registry-r3

## 1 Summary

Updated the six requested registry entries and added `Stack` and `OpenTrash`. Updated the pad view swap and search trash button callers. The offline gate passes.

## 2 Change list

- `product/AntiNotepad/Icons.cs` | Updated Folding, Split, SidePane, StorageTab and Delete meaning; added Stack and OpenTrash to the registry and `Icons.All` | Plan: six requested registry changes and two new entries.
- `product/AntiNotepad/UI/PadWindow.Bar.cs` | The view switch now shows Grid for “Show slots” and Stack for “Show stack” | Plan: match the destination named by the tooltip.
- `product/AntiNotepad/UI/SearchOverlay.cs` | The notes view's trash button uses `Icons.OpenTrash`; the list/grid toggle remains unchanged | Plan: distinguish opening trash from deleting.

## 3 Behavior changes

Folding uses Fluent `0xE32F`, Split uses Segoe `0xE90D`, SidePane uses Fluent `0xE8BD`, and StorageTab uses Fluent `0xE466`; the NotesFolder icon is unchanged. The pad view switch now depicts its destination. Search's trash button depicts recycling, while the existing delete action keeps its behavior and its meaning now describes deletion only. The four Fluent glyph names were confirmed with the bundled font helper.

## 4 Out-of-scope files

None.

## 5 Plan deviations

None.

## 6 Contracts

No contract changes. The existing icon font and glyph-name contract passed in selftest.

## 7 Lens review

No separate lens review was run for this quick registry swap.

## 8 Checks run

- `python -I .coord/reports/A35-ux-pass-s2-fontcodes.py product/AntiNotepad/Assets/Fonts/FluentSystemIcons-Regular.ttf E32F E8BD E466 EF12` — all four codes exist and match the requested glyph names.
- `git diff --check` — passed.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A35b-icon-registry-r3 -Offline` — `GATE: PASS`; build had 0 warnings and 0 errors, selftest reported `SELFTEST PASS`, tokens, format, scope and cleanup passed. Offline NuGet audit was disabled as expected.

## 9 Hand checks for the user

Not run offline. In the worktree dev copy, verify the Folding, Split, Storage and SidePane icons; confirm the pad switch shows Grid for “Show slots” and Stack for “Show stack”; confirm Search still uses List/Grid for results and the recycle icon opens trash.
