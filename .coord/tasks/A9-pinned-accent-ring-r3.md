# A9-pinned-accent-ring-r3: pinning must not fade the toolbar and pager

Rework round of `A9-pinned-accent-ring` (`docs/loop.md`). Read the item card `.coord/tasks/A9-pinned-accent-ring.md` first: its Plan, Files,
Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Medium, same as the card.
Role: terra, because it is a small fix across a few files with shared UI state.

## Why this round
User hand test, 2026-11-09: "everything works, but clicking the pin button specifically also triggers the Toolbar / Pager's Fade while Typing which is not expected."
Cause found by reading the code: `Settings.Pinned` (and `DimWhenPinned`) carry `Appearance | WindowPolicy | Persistence` in `Model/SettingsService.cs`. `PadWindow.HandleSettingsChanged` (`UI/PadWindow.Keys.cs`) calls `Edited()` when both Appearance and Persistence are set, and `Edited()` (`UI/PadWindow.cs`) calls `SleepChrome(soon: false)`, the same fade that typing triggers. So pinning, by button, menu or Ctrl+T, fades the top bar icons and pager as if the user had typed. This is existing behavior, but the user wants it gone.

## Change to the plan
Scope change approved by the user 2026-11-09 ("Fix it in A9"): pinning or unpinning (and changing Dim when pinned) must leave the toolbar and pager exactly as they are: no fade, no wake. Typing, other settings and every other chrome-fade path stay as today. Keep the ring update and the save scheduling for pin changes. Prefer the narrowest fix (for example, skip only the `SleepChrome` part of `Edited()` when the batch changed only Pinned/DimWhenPinned; inspect `SettingsChange` for how to tell which properties changed, and add to it only if needed).
New UI check for the hand test: with Fade chrome mode on, click the pin button and use Ctrl+T; the toolbar and pager stay visible, the ring still fades in and out; typing still fades the bars.

## Files
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/UI/PadWindow.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/Model/SettingsService.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest/Design.cs` (only if the design hash changes; it should not)
- plus the files the card and rounds r2 already allow

## Done means
Offline checks only (`docs/agents.md`, "What Codex can and cannot check"). Use a hard timeout (about 5 min) on `--selftest`.
- A capability check in the existing ring selftest pattern (it must stay the last contract and keep the Fluent/seeded-store setup): with chrome mode "fade", after a Pinned toggle through the settings service the chrome stays awake (icons opacity unchanged, `_chromeAwake` true), while a normal edit still puts it to sleep.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A9-pinned-accent-ring-r3 -Offline` passes.
- Delete `build/A9-check` and any gate output you created.

Report: `.coord/reports/A9-pinned-accent-ring-r3.md`, in the card's report format (Changed, Checks run, Not checked).
