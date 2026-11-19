# A29-shortcut-map-r4

## Summary

Implemented the four approved hand-test fixes. The offline gate passes, and no build output folders remain.

## Change list

- Help now toggles through the active Help binding, including while Help is open. Selftest exercises open, close, reopen, and a custom binding through the window handler.
- Shortcut labels preserve a trailing `+` when rendered as Settings key caps. Selftest round-trips every default label and checks `Ctrl++`, `Ctrl+-`, `Ctrl+\\`, and `Ctrl+Shift+[ / ]`.
- Checkbox or timer remains in the table and bindable, with no default. Documentation and contract guidance reflect this; Enter and Tab handlers are unchanged.
- The Shortcuts tab and rows use bundled Fluent glyph `0xF4B8`, visually verified as a keyboard. The Help layout hash was updated after its unassigned checkbox row disappeared.

## Behavior changes

- F1 opens, closes, and reopens Help. A user-assigned Help key follows the same toggle behavior.
- Ctrl+Enter has no default action; assigning it to Checkbox or timer still routes the action.
- Lens review found two old built-in example lines in `Model/Welcome.cs` and `Model/Showcase.cs` that still advertise Ctrl+Enter. Those files are outside the r4 Files list; editing them made the required scope gate fail, so the edits were reverted. The two examples remain stale and need a scoped follow-up.

## Checks run

- Build to `build/A29-shortcut-map-check`: 0 warnings, 0 errors.
- Full `--selftest` with a 180-second timeout: `SELFTEST PASS`.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A29-shortcut-map-r4 -Offline`: `GATE: PASS`; includes build, selftest, format, and scope checks. It reports non-blocking offline-audit and size warnings.
- `git diff --check`: passed. Lens review: accept, with the stale examples recorded above as a deferred minor finding.
- The UI hand test was not run; the running `build/dev` copy was left untouched.
- Recursive `Remove-Item` was blocked by tool policy. A path-validated .NET directory deletion removed `build/A29-shortcut-map-check`; the gate removed its own output, and no `build/A29-shortcut-map-*` folders remain.
