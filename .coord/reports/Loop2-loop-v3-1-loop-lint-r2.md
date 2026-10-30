# Loop2-loop-v3-1-loop-lint-r2

## Summary
Fixed escaped Markdown punctuation resolution and false success when lint checks no files. Checkout lint and the offline gate pass.

## Change list
| Block | Change | Plan |
|---|---|---|
| tools/loop-lint.ps1:17-18 | Require an existing filesystem directory | Rework Root validation |
| tools/loop-lint.ps1:25-27 | Throw when no live files are found | Rework zero-file refusal |
| tools/loop-lint.ps1:67 | Decode all ASCII punctuation escapes, retaining space handling, before URI decoding | Rework Markdown resolution |

## Behavior changes
Valid escaped punctuation in relative Markdown destinations now resolves to the intended file. A file supplied as Root or a directory without live files returns ERROR/exit 2 instead of PASS/exit 0. Existing title, parentheses, space and URI handling remains in place.

## Out-of-scope files
None. This requested report was added; the required gate regenerated its normal Loop2-loop-v3-1.gate.md report. Existing checkout edits were preserved.

## Plan deviations
None.

## Contracts
None (no product change).

## Lens review
Fresh lens_reviewer: accept, no proven findings or unproven risks. Independently verified punctuation matching, existing escaped destinations, checkout lint, file-root rejection and gate failure propagation.

## Checks run
- Actual-script throwaway probe: escaped agents\.md and dev\-loop.md links PASS/exit 0; absent escaped destination reports missing-path/exit 1.
- Existing balanced/escaped parentheses, double/single/parenthesized titles, escaped/angle-bracket spaces and URI decoding PASS/exit 0.
- Actual file Root (tools/loop-lint.ps1) and a directory with zero live files both ERROR/exit 2.
- All 32 ASCII punctuation escapes decode correctly in memory. Throwaway files and directories deleted; no probe artifacts remain.
- Checkout: LOOP-LINT: PASS (37 files), exit 0.
- Required offline gate: GATE PASS; LoopLint PASS (37 files); Build exit 0, zero warnings/errors; SELFTEST PASS; Seam, Scope and Cleanup PASS. NuGet audit disabled for this offline build only.
- git diff --check passed. No commits, pushes, branch switches, merges, STATUS/card/product edits or installed-app actions.

## Hand checks for the user
None (not visible).
