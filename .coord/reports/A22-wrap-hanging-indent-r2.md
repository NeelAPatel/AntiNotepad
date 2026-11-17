# A22-wrap-hanging-indent-r2

## Summary
Fixed wrapped result-chip positioning, delayed indent parsing until a line wraps, and aligned inline objects with continuation rows. Nested the layout-test loops to correct their formatting.

## Change list
- ResultRenderer derives TextEnd from the visual line's public X helper.
- TextView calls the indent provider only after the first row proves the line wraps; inline object arrangement includes the continuation offset. PATCHES.md records the new marker.
- Expanded windowless layout checks with a tab prefix, a folded wrapped line, over-half-width fallback, and result-chip text end. Added a result-chip contract; the fork documentation contract checks the added marker.

## Behavior changes
Wrapped result chips clear the translated text end; inline objects use the same continuation origin as text. Indents wider than half the view fall back to zero. The tab-prefix layout passed; tab stops within continuation text were not separately verified, so their origin remains a hand-check item.

## Out-of-scope files
No PadWindow, Settings, Store, STATUS, or card edits. The gate wrote its own report and build outputs.

## Plan deviations
No app launch or screenshot, per offline instructions. The direct dotnet format command failed with access denied connecting to its build-host pipe; loops were manually nested and the gate's Format step passed. Cleanup of build/A22-check was rejected by execution policy; the directory remains.

## Contracts
wrap-indent-text-column and wrapped-result-chip-text-end pass, including 108 layout combinations; avaloniaedit-fork-documented passes. The selftest reports 21 pass and one manual contract.

## Checks run
- Offline build: 0 warnings, 0 errors; SELFTEST PASS.
- pwsh -NoProfile -File tools/gate.ps1 -Task A22-wrap-hanging-indent-r2 -Offline: Build, Selftest, Seam, Tokens, Format pass; Scope fails on 247 vendored paths because the card's directory entry is treated as an exact path. Size warns for SelfTest/Editor.cs; cleanup skipped on gate failure. The first concurrent gate run had a build file lock; the sequential rerun passed Build.
- git diff --check passed (line-ending warning only).

## Hand checks for the user
In the dev copy, check nested lists, numbered items, checkboxes and plain indentation at several widths, with caret, selection and hit-testing. Check chips on wrapped lines, inline objects, folded lines, and tabs within continuation text; verify tab stops against the intended row origin. Capture the requested before/after example if available.
