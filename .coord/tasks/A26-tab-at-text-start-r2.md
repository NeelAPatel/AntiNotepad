# A26-tab-at-text-start-r2: level keys become Ctrl+[ and Ctrl+]

Rework round of `A26-tab-at-text-start` (`docs/loop.md`). Read the item card `.coord/tasks/A26-tab-at-text-start.md` first: its Plan, Files,
Contracts and "For Codex" section still apply unless this file says otherwise. Also read your first report `.coord/reports/A26-tab-at-text-start.md`.

Tier: Medium, same as the card
Role: terra, because it is a key swap across the same few files

## Why this round
Your first run found Alt+Shift+Left/Right is taken by note navigation in `UI/PadWindow.Keys.cs`. User, 2026-11-18: Alt+Left and Alt+Right stay note switching; "Ctrl+[ and Ctrl+]" change list level from anywhere; "override or remove other shortcuts, its okay because ill be reworking them anyway".

## Change to the plan
- Replace the Alt+Shift+Left/Right level keys with Ctrl+[ (outdent) and Ctrl+] (indent), plain Ctrl only (Ctrl+Shift+[ / ] stays fold). Same behavior otherwise: from any caret spot, every list line in a selection, then renumber. Non-list lines: do nothing, leave the key unhandled.
- In `UI/PadWindow.Keys.cs`, stop Ctrl+[ and Ctrl+] from navigating notes (keep Ctrl+PageUp/PageDown and Alt+arrows), so the editor receives them. Do not touch other keys.
- `UI/HelpOverlay.cs`: remove Ctrl+] and Ctrl+[ from the Notes rows (keep Alt+arrows and swipe); Writing row says "Ctrl+[ / Ctrl+]" "Change list level anywhere". Drop the Alt+Shift+arrows row. Update `SelfTest/Design.cs` hash as before.
- Update the `list-tab-text-start` contract text and hand-check line to Ctrl+[ / Ctrl+]. Add a selftest if the key routing in PadWindow can be checked without a window; otherwise state that in the report.

## Files
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/UI/HelpOverlay.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest/Lists.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
- `docs/decisions.md`

## Done means
- Ctrl+] indents and Ctrl+[ outdents list lines from any caret position; Tab rules from round 1 unchanged.
- Build 0 warnings (`AVALONIA_TELEMETRY_OPTOUT=1`, build into `build/A26-tab-at-text-start-r2-check`, delete after), `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A26-tab-at-text-start-r2 -Offline` passes. Never launch the app, never commit, never edit the card.

Report: `.coord/reports/A26-tab-at-text-start-r2.md`, in the card's report format.
