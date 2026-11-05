# A4d-shape-tokens-r2 report

## Summary
Reflowed the initializer entries identified in the round 1 Format report. Comparison with the pre-rework files confirms whitespace-only changes, but Format still needs an online verification because the standalone formatter could not connect to its build-host pipe.

## Change list
| File | Literals swapped (count) | Tokens used |
|---|---:|---|
| `product/AntiNotepad/UI/PadWindow.Layout.cs` | 0 | Existing Shape tokens; whitespace only |
| `product/AntiNotepad/UI/SearchOverlay.cs` | 0 | Existing Shape tokens; whitespace only |
| `product/AntiNotepad/UI/SettingsParts.cs` | 0 | Existing Shape tokens; whitespace only |
| `product/AntiNotepad/UI/SettingsWindow.cs` | 0 | Existing Shape tokens; whitespace only |

## Behavior changes
None.

## Out-of-scope files
`.coord/reports/A4d-shape-tokens-r2.md`: requested report. `.coord/reports/A4d-shape-tokens-r2.gate.md`: generated gate report.

## Plan deviations
The formatter could not run in the sandbox: its build host failed to connect to a named pipe with `UnauthorizedAccessException`. The reported initializer lines were reflowed manually. The requested check-build folder was already absent.

## Contracts
None.

## Lens review
Self-reviewed using the lens checklist as a worker: no proven behavioral findings. The four files match their pre-rework snapshots after whitespace removal; there are no changes to tokens, shared state, timing, inputs, cleanup or layering. Format verification remains unproven in this sandbox.

## Checks run
- Offline gate: Build PASS (0 warnings, 0 errors), Selftest PASS, Seam PASS, Tokens PASS, Format reported PASS, Scope FAIL, LoopLint SKIP, Size PASS, Cleanup SKIP; GATE FAIL. Scope fails because the rework brief's Files section references the original card rather than listing backticked paths.
- The gate's Format PASS is not reliable evidence here: its code does not check the formatter exit code. Standalone `dotnet format whitespace --no-restore --verify-no-changes` for the four files exited 1 on named-pipe access before verification. Re-run Format online.
- Gate build selftest: `DESIGN dump 8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b 16160`; `SELFTEST PASS`.
- CRLF verified on all four reworked files, with zero bare LF. Byte comparison after removing whitespace matched the pre-rework snapshot for each file. `git diff --check` PASS.
- Tokens PASS; round 1 inventory before to after: radius 25 to 6 (size arguments), shadow 4 to 0, alpha 7 to 5, color 8 to 10 (scanner artifacts); unchanged in this rework.

## Hand checks for the user
In a dev build, inspect the pad menu, an open Settings dropdown, a switch, theme cards, search (Ctrl+F), and a toast for unchanged appearance. Also review the save-failure dialog and open dropdown popup, which are outside the design dump.
