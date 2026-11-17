# Vendored dependency upgrades

AvaloniaEdit is built from the maintained source fork in `third_party/AvaloniaEdit/`. The fork keeps upstream files verbatim except for the A22 patches listed in `third_party/AvaloniaEdit/PATCHES.md`; the app references its project directly. Its local `.gitignore` excludes generated `bin/` and `obj/` outputs.

## Upgrade AvaloniaEdit

1. Choose an upstream release and commit, then replace the vendored source from that exact revision. Retain its license and update `UPSTREAM.txt` with the version, commit, and copy notes.
2. Reapply the focused patches in `PATCHES.md`. Keep upstream formatting intact and add one `// AntiNotepad patch A22:` marker for each changed patch point. Update the method list and marker list in `PATCHES.md` so each marker appears exactly once.
3. Update the pinned version in `SelfTest/Editor.cs` (`AvaloniaEditForkChecks`) and the version recorded in `PATCHES.md`. The `avaloniaedit-fork-documented` selftest checks both values, the marker list, and the app's project reference.
4. Build the app and run `--selftest` offline. The wrap contract covers nested and top-level bullets, checkboxes, numbered items, indented and unindented text, multiple widths and fonts, caret position, selection geometry, and hit-testing. Run `tools/gate.ps1 -Task <ID-slug> -Offline` for the item that owns the upgrade.
5. Keep the local project targeting `net10.0`, with its `Avalonia` package aligned to the app and assembly signing disabled. Review the upstream diff and confirm the fork builds with 0 warnings. Keep any unavoidable vendored compiler warnings scoped to the fork project and document their reason; do not suppress warnings in product code.
