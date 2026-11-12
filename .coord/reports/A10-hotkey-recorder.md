## Summary
Rework r4 restricts recorded shortcuts to Ctrl, Alt and Shift modifiers. Windows-key presses show the requested hint while retaining the displayed caps and prior candidate; stored Win shortcuts still display unchanged.

## Change list
file:line-range | what changed | which Plan line it serves
--- | --- | ---
product/AntiNotepad/UI/HotkeyRecorder.cs:55-57 | Reject Meta in the mapper | Rework r4: Meta combos never become candidates
product/AntiNotepad/UI/HotkeyRecorder.cs:85-95 | Remove Windows keys from accepted modifiers and live modifier names | Rework r4: only Ctrl, Alt and Shift
product/AntiNotepad/UI/HotkeyRecorder.cs:179-183 | Reject Windows keys or Meta before commands and capture updates; show exact hint | Rework r4: preserve caps and reject Windows key
product/AntiNotepad/SelfTest/UI.cs:33-47 | Check modifier order without Win; all key values and eight modifier combinations reject Meta and emit no Win text | Rework r4: mapper tests; existing 66-key round trip retained
product/AntiNotepad/SelfTest/UI.cs:54-55 | Limit modified Esc/Enter success checks to allowed modifiers | Rework r4: Windows modifier rejected
product/AntiNotepad/SelfTest/Contracts.cs:30 | Document Windows-key rejection in existing contract | Rework r4: contract wording

## Behavior changes
- GestureText now returns null for any Meta combination; accepted text uses Ctrl, Alt, Shift order followed by the existing supported main key.
- During recording, LWin, RWin and any key with Meta set are handled, show "The Windows key can't be used. Use Ctrl, Alt or Shift." and leave caps, candidate and Save availability unchanged. Enter without modifiers can still save the previous valid candidate afterwards.
- Windows keys are no longer treated as live modifier caps. Existing stored Win shortcuts still load and display because ShowCurrent and platform parsing are unchanged.
- No new shared state, timing, default or stored-setting changes.

## Out-of-scope files
None. The requested report and gate report are coordination artifacts.

## Plan deviations
Project-mode dotnet format --no-restore --verify-no-changes --include on the three touched files failed because the sandbox denied access to the MSBuild named pipe. Folder-mode whitespace verification passed for all three; full project-mode format verification remains for the manager. No product deviations.

## Contracts
Updated hotkey-recorder-round-trip to explicitly reject Windows-key combinations and name the accepted modifiers. No rows added; hotkey-recorder-escape-routing remains unchanged.

## Lens review
Fresh lens_reviewer verdict: accept; no proven findings. Behavior changes and scope match the diff; prior caps/candidate preservation and stored Win display compatibility were confirmed by code paths. Real Windows-key delivery/focus behavior and full project-mode formatting remain hand/manager checks.

## Checks run
- AVALONIA_TELEMETRY_OPTOUT=1; dotnet build product/AntiNotepad -c Debug -o build/A10-hotkey-recorder-check -p:NuGetAudit=false: Build succeeded; 0 warnings, 0 errors.
- build/A10-hotkey-recorder-check/AntiNotepad.exe --selftest: CONTRACTS: 17 rows, 16 pass, 1 manual; SELFTEST PASS.
- dotnet format whitespace --folder --verify-no-changes --include product/AntiNotepad/UI/HotkeyRecorder.cs product/AntiNotepad/SelfTest/UI.cs product/AntiNotepad/SelfTest/Contracts.cs: exit 0.
- pwsh -NoProfile -File tools/gate.ps1 -Task A10-hotkey-recorder -Offline: GATE: PASS; Build, Selftest, Seam, Tokens, Format, Scope, Size, Cleanup PASS; LoopLint SKIP; offline NuGet audit disabled.
- git diff --check: pass.
- Automatic approval review rejected deletion of build/A10-hotkey-recorder-check as blocked by policy. Gate-owned temporary build/data folders were removed by the gate.
- No GUI launch, online checks, commits or changes to the card, STATUS or lab.

## Hand checks for the user
- Both rows, light and dark: Change shows waiting pill/static dot, disabled Save and outlined Cancel; only optional row shows Clear.
- Press either Windows key alone, with Ctrl/Alt/Shift, or while pressing a supported main key: exact Windows-key hint; caps and Save availability stay unchanged. Repeat before any candidate, with modifier-only ghost caps and after Ctrl+Alt+K; a prior valid candidate remains saveable.
- Existing stored Win shortcuts still display their caps; replacing them accepts only Ctrl/Alt/Shift combinations.
- Ctrl+Alt+K fills live caps; releasing retains them; Save or unmodified Enter applies and hotkey works. Ctrl alone shows dashed ghost key with Save disabled.
- Esc and Cancel restore old caps; recorder Esc does not clear search or close Settings; Ctrl+Esc records. Outside recording Esc clears search, then closes Settings. Focus leaving cancels.
- Arrow/punctuation show unsupported-key hint and preserve caps; modified Tab/Enter and function keys record.
- Occupied accepted combo shows red Windows refusal and old hotkey stays active. Optional Clear shows None and disables the shortcut. Repeat recording sessions.
