# A35e build run 1: targets, sizes, shared focus ring, chips and swatches

Role: terra, because normal UI work across a few files. Read `AGENTS.md`, `.claude/rules/coding.md`, `.coord/tasks/A35e-targets-keyboard.md`, the lab `tools/playground/a35ef-lab/comparison.js` (After values) and `.coord/reports/A35-ux-pass-s3.md` (B-5, A-2, A-5, B-4, B-1). The branch is checked out: never commit or switch.

Build, using tokens (`Space`, `ChromeTokens`, `Shape`), no new raw literals; `tools/check-design-tokens.ps1` must not rise:
1. B-5: Settings segment buttons (`SettingsParts.cs` ~244), language/toggle pills (`Kit.cs` ~201), `TextButton` (~394): min height 32 (`ChromeTokens.ButtonSize`), gap between neighbors 4 (`Space.Xs`).
2. A-2/A-5: slot dots (`PadBar.cs` ~223): every dot button 24 wide x 32 (new token `PagerDotTargetWidth` = 24; remove the selected/unselected button widths), 0 spacing between dot buttons (put the nine in their own inner row so `PagerRowSpacing` still applies to the other items), dot 8 px, selected 16 px, centered. Selecting a dot must not move the others.
3. B-4: accent swatches: 32 x 32 button targets, 20 px visible dot, no spacing.
4. B-1: palette chips and accent swatches become `Kit.Plain` buttons (Tab, Space, Enter select; current accessible names kept, add ", selected" state like `Toggles`). Selected ring unchanged.
5. Shared focus ring on `Kit.Plain`: when `IsKeyboardFocusWithin` and the focus came from the keyboard (`:focus-visible`), draw 2 px (`Shape.Thick`) `p.Ink` at `Paint.FocusBorder` outside the face with a 2 px gap, as in the lab. Pointer clicks must not leave a ring. Keep the existing neutralised Fluent fills in `Kit.InputStyles`.
6. Selftests in `SelfTest/UI.cs` + `Contracts.cs` rows: min control height >= 32 for segment/text button; dot pitch equals target width and is unchanged when selection changes; chips and swatches are focusable buttons; Plain shows a ring on keyboard focus only (if headless focus is not testable, assert the style/class hook instead and say so).

Done means: build into `build/A35e-targets-keyboard-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, `--selftest` passes (delete the folder), `pwsh -NoProfile -File tools/gate.ps1 -Task A35e-targets-keyboard -Offline -Base docs/A35-ux-pass` passes except the scope check for files of run 2. Never launch the app or edit STATUS or cards. Report `.coord/reports/A35e-targets-keyboard.md` with sections 1-9 from the card template (short).
