# A32-selection-convert-menu report

## 1 Summary
Implemented selection conversion to checklist, bullet, and numbered lists with one-step undo.

## 2 Change list
Added pure line conversion, grouped document edits and renumbering, optional submenu entries and popup rendering, selftests and a contract row.

## 3 Behavior changes
A selection exposes Convert to list. Nonblank selected lines receive the chosen prefix, existing list prefixes are replaced, indentation and a first-line mode keyword are preserved, and numbering follows existing block rules.

## 4 Out-of-scope files
None changed in product. Gate generated its task report.

## 5 Plan deviations
The no-selection automated check exercises the menu factory rather than constructing the editor context menu. Pointer and keyboard behavior needs the planned hand check.

## 6 Contracts
Added convert-list-undo-blanks, backed by list command checks and a manual UI check.

## 7 Lens review
Reviewer found missing no-selection coverage; added a menu factory check after review. No other proven findings. Submenu interaction remains for hand testing.

## 8 Checks run
Dedicated build succeeded with zero warnings initially; after the gate's restore, a later build emitted two offline NuGet audit warnings. Dedicated selftest: SELFTEST PASS. Offline gate: PASS, build zero warnings and selftest PASS. An earlier gate attempt collided with the concurrent dedicated build; the sequential rerun passed. Dedicated check folder removed.

## 9 Hand checks
Pending user: in the dev slot select mixed lines with a blank, convert each kind and undo once; verify no submenu without selection, hover, Right, Left and Esc, and existing entries. No app launched for hand testing.
