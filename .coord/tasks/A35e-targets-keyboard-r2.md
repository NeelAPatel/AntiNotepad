# A35e rework round 2 (fixes the Codex review)

Role: terra, because four proven bugs in two files. Read `AGENTS.md`, `.coord/tasks/A35e-targets-keyboard.md`, the review at `.coord/logs/A35e-targets-keyboard.reviewer.last.md`. Never commit or switch branches. Files: `UI/SettingsWindow.cs`, `UI/PadWindow.Layout.cs`, `SelfTest/UI.cs`, `SelfTest/Contracts.cs`, `SelfTest/Design.cs` (re-pin only if the design dump really changes).

1. Palette/theme/accent change (reviewer 1): focus is lost when `Set` runs `Restyle` synchronously. Restore focus on that path too.
2. Shortcut Reset (reviewer 2): buttons without an automation name fall to the first control (Reset all). Make restoration key on a stable identity: the focused control's automation name if it has one, else its index among the page's focusable controls (same page, same order). Never fall back to "first control" when something was focused; use the first control only when nothing was.
3. Leaving search through the current section's rail item (reviewer 3): restore rail focus after `_find.Text = ""` too.
4. Reveal handle (reviewer 4): an in-flight reveal animation overwrites the instant handle reset. Keep the `Anim.Handle` for the top and bottom reveal and cancel it before an instant show/hide and in `ApplyBars` mode changes (also for the float fade).
One shared helper for 1-3 (capture before the rebuild, restore after) rather than three copies. Add selftest coverage for the identity fallback (named, unnamed index, nothing focused) and the cancel-on-mode-change; contract rows updated.

Done means: build 0 warnings, `--selftest` passes (`AVALONIA_TELEMETRY_OPTOUT=1`, build into `build/A35e-targets-keyboard-check`), `pwsh -NoProfile -File tools/gate.ps1 -Task A35e-targets-keyboard -Offline -Base docs/A35-ux-pass` passes except scope on the lab files. Append a "Round 2" section (1-9, short) to `.coord/reports/A35e-targets-keyboard.md`. Never launch the app or edit STATUS or cards.
