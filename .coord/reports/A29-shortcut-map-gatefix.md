# A29-shortcut-map-gatefix

## 1. Gate failures and fixes

- **Format:** `Shortcuts.cs` had LF endings and whitespace changes. Ran `dotnet format whitespace` in folder mode for that file; the gate's final format check passed all 14 changed C# files.
- **Tokens:** Replaced the new numeric `Thickness` arguments in Help and HotkeyRecorder with existing `ChromeTokens` values. The displayed spacing values are unchanged. `check-design-tokens.ps1` passes; the baseline was not updated.
- **Design hash:** Kept `0e2298aa4bc2a43b082973c1fd201a8b77e9ecbf3d24e9cef31666ba45ec7eaa`; the final selftest passes its design check.

## 2. Files changed

- `product/AntiNotepad/UI/Shortcuts.cs`
- `product/AntiNotepad/UI/HelpOverlay.cs`
- `product/AntiNotepad/UI/HotkeyRecorder.cs`
- This report.

## 3. Final gate and selftest

- `[PASS] Selftest: exe exit 0; contains SELFTEST PASS`
- `SELFTEST PASS`
- `GATE: PASS`

## 4. Remaining failures

None. The gate reported offline NuGet-audit and file-size warnings; neither failed the gate.
