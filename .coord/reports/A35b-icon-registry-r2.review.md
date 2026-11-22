# A35b-icon-registry-r2 lens review

## Findings (proven)

1. [major] tools/check-design-tokens.ps1:35 - Lowercase hexadecimal icon escapes bypass the new guard. `Find-Hits` uses case-sensitive `[regex]::Matches` at line 277, but both the Unicode-escape and char-cast alternatives require uppercase `[EF]`. I dot-sourced the unchanged scanner and passed source strings to its actual `Find-Hits` function with file `product/AntiNotepad/UI/Fixture.cs`: `var icon = "\uF6ED";` produced 1 icon-code hit; `var icon = "\uf6ed";`, `var icon = "\ue8b3";`, and `var icon = new string((char)0xe8b3, 1);` each produced 0. These are valid C# spellings of the same PUA characters, so ordinary lowercase source can reintroduce raw icons outside the registry without failing the scanner. Smallest fix: use `[EeFf]` in both alternatives (and support `0X` for the cast), with lowercase detection fixtures in `-SelfCheck`. This breaks the approved scanner promise, although the current migrated product contains no demonstrated rendering regression.

## Behavior changes check

Report's list vs the diff: matches. Typed font selection, the five approved Settings swaps, and removal of the unused faux-bold path are accounted for. Other migrated codes and size arguments match the base. The item-chat move to root `Icons.cs` and `AppInfo.DevBadgeGlyph` extraction match the supplied review notes; the implementer report's older `UI/Icons.cs` paths are stale.

Changed shared signatures and callers checked:

- `GlyphIcon(Icon, Color, double)`: `Kit.IconButton`, `PadMenu.Icon`, `PadWindow.UpdateSwapButton`, Settings search/sidebar construction, Settings expanders/dropdowns, and `SettingsWindow.Fl` all pass registry entries and retain their size/color arguments. All hold.
- `Kit.IconButton(Icon, ...)`: `Kit.PanelHeader`, `PadBar.Rebuild`/pager construction, `PadWindow.BuildSwapButton`/bar construction, `SearchOverlay`, `SettingsWindow`, and `SelfTest.Bar` all hold. The pinned caller supplies the exact `Icons.KeepOnTop` singleton, so the reference check still inserts `KeepOnTopFilled` beneath the outline with the previous alpha and size (`UI/Kit.cs:93-98`, `UI/PadWindow.Layout.cs:295-296`).
- `MenuEntry.Icon`: editor context-menu construction, `PadView.ConvertMenu` (null icons), the pad app menu, and design fixtures hold. `PadMenu.BuildActionRow` and `BuildMenuRow` both feed the typed value into `PadMenu.Icon`. New-note/pin references retain accent, delete retains error, other entries retain ink; disabling colored icons still selects ink (`UI/PadMenu.cs:271-280`).
- `SettingsWindow.Fl(Icon)` and sidebar tuples: all Settings callers hold; only the five approved code substitutions change the displayed glyph.
- `ContractsChecks`: sole caller `SelfTest.Run` still accumulates failures. The icon row is counted as deferred, then `Run.cs:133-135` unconditionally executes it and adds failure to the same list before the exit result. `RunExistingCheck` converts exceptions to a failed result. Deferral does not skip enforcement or turn failure into success.
- Dev badge: `AppInfo.FormatDevBadge` and `PadWindow.DevBadgeText` use the same extracted character; Cascadia existence checking and the fallback symbol remain unchanged.

## Scope and plan check

In plan. Reviewed every product hunk in `6902a44...HEAD` (HEAD `3fd57cec9b723b8bf00f5af51117015a0e07cd7d`), the scanner and baseline changes, and the empty uncommitted product diff. Registry/migration, glyph swaps, weight deletion, contract registration, font parser and scanner each serve the Plan's After or Test plan. No unrelated behavioral edits found. Root placement and badge extraction are explained by the supplied notes and preserve the layer boundary and badge. The scanner finding above leaves a planned acceptance promise incomplete.

## Unproven risks

- Visual comparison with the approved lab remains a hand test; no UI was launched during this review.

## Checks run

- Read the item card, round brief, findings detail, the r2 implementer report (the original `.coord/reports/A35b-icon-registry.md` is absent), supplied gate report, changed code and affected callers; applied all nine skill checklist items. No new product timers, subscriptions, async paths or persistent state were introduced.
- `pwsh -NoProfile -File tools/check-design-tokens.ps1 -SelfCheck`: `TOKENS SELFCHECK PASS`.
- Dot-sourced full token scan: `TOKENS PASS`; then the actual `Find-Hits` probe returned the 1/0/0/0 hit counts documented above. Probe ran in memory; no temporary folder or product file was created.
- `git diff --check`: exit 0. Initial worktree was clean; `git diff -- product` was empty.
- Supplied gate report reports build 0 warnings/errors and `SELFTEST PASS`, including 42 contract rows / 1 deferred. Build/selftest were not rerun by this reviewer; the A29 mutation failure described in the item-chat notes is supplied evidence, not a reviewer-run probe.
- Product files and Git state were not changed. Only this review report was written.

VERDICT: rework
