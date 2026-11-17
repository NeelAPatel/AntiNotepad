# T17-multi-dev-instances — app-side report

## Changed

- Added the ordered Blue, Red, Green, Purple, Orange, Silver slot table, with unique `AntiNotepad-dev-<color>` identities and show/hide and new-note shortcut strings. Slot identity is selected only when custom data is present; no slot keeps `AntiNotepad-dev`, and installed identity remains `AntiNotepad`.
- Added the initial `ANTINOTEPAD_NEWNOTE_HOTKEY` override for slotted copies when the saved setting is empty. A saved setting wins, and clearing it later turns the shortcut off.
- Updated badge branch parsing and timestamp labels, slot color and light-theme text contrast, and the left-expanding cross-fade.
- Generated and embedded six slot icons. The Blue icon is byte-identical to the existing dev icon.

## Files

- App identity/startup/hotkeys: `product/AntiNotepad/AppInfo.cs`, `Program.cs`, `Platform/HotkeyRegistrar.cs`
- Badge/tokens: `UI/PadWindow.Layout.cs`, `Theme/ChromeTokens.cs`, `Theme/DevSlotTokens.cs`, `Motion/MotionTokens.cs`
- Selftest/contracts: `SelfTest/App.cs`, `SelfTest/Contracts.cs`
- Icons/resources: `AntiNotepad.csproj`, `tools/make-dev-icon.ps1`, and `antinotepad-dev-{blue,red,green,purple,orange,silver}.ico`

## Checks run

- Offline build: pass, 0 warnings and 0 errors.
- `--selftest`: pass; six icon resources load and both new identity contract rows pass.
- `git diff --check`: pass.
- `tools/gate.ps1 -Task T17-multi-dev-instances -Offline`: **FAIL on Scope only**. Build, selftest, seam, tokens, format and loop-lint pass. Size reports a warning for `PadWindow.Layout.cs` growing from 765 to 821 lines.
- Blue icon SHA-256 matches `antinotepad-dev.ico`: `D43C02E2B577DAF5EEB21F8C75FF58807130F728DF48829095B08CC1C934A4E9`.

## Not run

- Live launching, global hotkeys, badge/tray appearance, installed-copy before/after checks, and multi-worktree hand tests. Per the card, Codex did not launch the app or run `tools/dev.ps1`.

## Risks

- Scope checks 12 paths absent from the approved card's `Files` list: the six generated icons, `AntiNotepad.csproj`, `MotionTokens.cs`, `SelfTest/Contracts.cs`, `Theme/DevSlotTokens.cs`, and the already-present badge-lab and showcase pages. The card says Codex must not edit it, so this report leaves that approval record untouched.
- Launcher allocation/replacement remains Part 2; these app changes do not allocate slots by themselves.
- Cleanup of `build/T17-check` and the gate's task outputs was denied by execution policy; those generated folders remain.
