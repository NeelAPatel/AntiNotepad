# A35e build run 2: recorder, F6, focus restore, Delete gap, menu order, edge cue

Role: terra. Read `AGENTS.md`, `.claude/rules/coding.md`, `.coord/tasks/A35e-targets-keyboard.md`, the run 1 report `.coord/reports/A35e-targets-keyboard.md`, lab `comparison.js`. Never commit or switch branches.

Build (tokens only, no new raw literals):
1. B-2: `HotkeyRecorder.cs`: while recording, Backspace alone calls Clear (only when empty is allowed); the hint text reads "Enter saves, Esc cancels, Backspace clears" (when Clear is not allowed, omit the last part). Backspace is no longer reported as an invalid key.
2. A-3: default key F6 moves focus to the other split pane and back (new `ShortcutAction`, bindable like the others; update `Shortcuts.cs` defaults, `docs/shortcuts.md`, the shortcuts list in Settings picks it up through the registry). Only when split view is on; otherwise it does nothing. Check F6 is unbound elsewhere.
3. B-7: after a Settings page change or a rows-changing option, focus returns to the control that was focused (the selected rail item for page changes; the same option by its accessible name for row rebuilds), falling back to the first focusable control in the page.
4. A-9: float bar: Delete/Clear is at least 24 px from the arrow group (separator margins via tokens, as in the lab).
5. C-5: `PadMenu.cs` action row order: Delete, Cut, Copy, Paste.
6. A-7: hidden bars: a 24 x 2 faint handle (Faint, `Shape` round) centered at the reveal edge of the hidden overlay/float bar; it fades out when the bar is shown, and is not drawn in Standard docked mode (bar always visible). No new animation: reuse the bar's existing fade; motion is out of scope.
7. Selftests and `Contracts.cs` rows for 1-3 and 5; 4 and 6 by token values.

Done means: build 0 warnings, `--selftest` passes (build into `build/A35e-targets-keyboard-check`, delete it, `AVALONIA_TELEMETRY_OPTOUT=1`); `pwsh -NoProfile -File tools/gate.ps1 -Task A35e-targets-keyboard -Offline -Base docs/A35-ux-pass` passes. Update the card's `## Files` only by listing extra files you touched in the report under "Out-of-scope files". Never launch the app or edit STATUS or cards. Append to `.coord/reports/A35e-targets-keyboard.md` a "Run 2" section, sections 1-9, short.
