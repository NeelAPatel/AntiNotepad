# A35g-labels-tooltips-r2

## 1 Summary
Shortcut reset Undo is now visible beside the reset control in Settings.

## 2 Change list
Updated SettingsWindow and its existing UI selftest. The earlier approved string and design-dump changes remain as recorded in A35g-labels-tooltips.md; no design baseline changed in r2.

## 3 Behavior changes
A reset that removes overrides offers one adjacent Undo. Undo restores the saved overrides through SettingsService.Update. Another shortcut edit or reset replaces or clears it; page rebuilds preserve it. An unrelated refresh preserves it; an external override change invalidates it. No pad toast is raised.

## 4 Out-of-scope files
No product files outside the approved list changed in r2. The default gate compares with dev and includes 15 pre-existing sibling files outside the item scope.

## 5 Plan deviations
The requested gate failed its scope check on those sibling files; the gate with -Base HEAD passed. The standalone build initially passed with zero warnings; a later build reported two NU1900 offline vulnerability-audit warnings, while the final offline gate build had zero warnings.

## 6 Contracts
The existing settings-labels-reset-search contract now checks adjacent Undo, restoration, no pad toast, rebuild persistence, unchanged reset, and refresh invalidation. Hand testing remains necessary.

## 7 Lens review
Independent review found stale Undo after external settings replacement; fixed by checking the post-reset overrides on refresh. Re-review accepted with no remaining proven findings.

## 8 Checks
Task build and selftest passed, including zero setter ownership violations and SELFTEST PASS. Offline gate with -Base HEAD passed build (zero warnings), selftest, seam, tokens, format, scope and cleanup. The requested default-base gate failed only scope on 15 pre-existing sibling files; its other checks passed. Gate reported the existing SettingsWindow size warning and disabled NuGet audit offline. Task build folder cleaned by gate.

## 9 Hand checks
Pending in the slot dev copy: change a shortcut, Reset all and Undo beside it; repeat for an individual Reset; verify unchanged resets show no Undo and a later shortcut change or reset replaces it.
