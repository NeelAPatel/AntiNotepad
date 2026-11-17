# T17-multi-dev-instances · launcher report

## Changed
- Added a pure six-color slot allocator and `-SelfTest`; launcher ownership comes from slot mutexes, while a main-checkout registry remembers worktree colors.
- Replaced only a verified owner from this worktree. Quit and fallback checks use the selected slot; legacy dev identity is quit only when its image belongs to this worktree. Starts receive slot, show/hide, new-note and absolute data environment values.
- Added read-only `dev-list.ps1`; extended `press-hotkey.ps1 -Slot/-NewNote`; gate and merge preflight clear `ANTINOTEPAD_SLOT` with the other dev variables.

## Files
- `tools/dev.ps1`
- `tools/dev-list.ps1`
- `tools/press-hotkey.ps1`
- `tools/gate.ps1`
- `tools/merge-preflight.ps1`

## Checks run
- `pwsh -NoProfile -File tools/dev.ps1 -SelfTest` — PASS (`DEV-SLOTS SELFTEST PASS`).
- PowerShell parser on all five changed scripts — PASS.
- `pwsh -NoProfile -File tools/dev-list.ps1` — PASS; six slots displayed vacant in this snapshot.
- `pwsh -NoProfile -File tools/gate.ps1 -Task T17-multi-dev-instances -Offline` — PASS. Gate reported existing card/docs budget warnings and a size warning for `PadWindow.Layout.cs`.
- `git diff --check` — PASS.

## Not run
- Live dev launch, relaunch, `--quit`, hotkey presses, seven-slot refusal with real owners, or installed-copy before/after inspection. Codex is restricted to offline checks; user hand test remains required.

## Risks
- Slot mutex ownership and central registry writes were not exercised live. Unknown owners are kept occupied and cannot be replaced.
- Saved per-worktree `Settings.Hotkey` or `NewNoteHotkey` values can take precedence over launcher defaults; verify the displayed keys during hand test.
- Installed-copy non-interference is guarded by distinct dev ids, explicit custom DATA and installed-image rejection, but needs the planned user hand test for live proof.
