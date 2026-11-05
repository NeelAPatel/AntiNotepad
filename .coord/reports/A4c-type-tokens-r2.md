# A4c-type-tokens-r2 report

## Summary
Split the specified initializer entries across five UI files, preserving values and ordering. All five retain CRLF; formatter verification remains blocked by the sandbox.

## Change list
| File | Literals swapped (count) | Tokens used |
|---|---:|---|
| UI/HelpOverlay.cs | 0 | Existing MonoFamily, Caption |
| UI/PadWindow.Layout.cs | 0 | Existing Strong, Regular, Label |
| UI/SearchOverlay.cs | 0 | Existing Input, Small, MonoFamily, Strong |
| UI/SettingsParts.cs | 0 | Existing Display, Body, Strong |
| UI/SettingsWindow.cs | 0 | Existing Body, Small |

Paths are relative to product/AntiNotepad.

## Behavior changes
None.

## Out-of-scope files
Coordination reports are requested or gate-generated artifacts. No out-of-scope product files.

## Plan deviations
None to product changes. Direct format verification could not run: the build-host named pipe returned UnauthorizedAccessException. The gate's Format PASS is not reliable evidence here: tools/gate.ps1 parses diagnostics without checking the formatter exit code, so it can treat this failure as zero diagnostics.

## Contracts
None.

## Lens review
Worker self-review against the lens checklist: no proven findings. The five files match their pre-round versions after whitespace removal; no values, ordering, state, signatures, timing, dependencies or failure paths changed. No independent reviewer was spawned.

## Checks run
- Offline build: 0 warnings, 0 errors with NuGetAudit=false. Initial no-restore build had one NU1900 audit warning.
- Selftest: DESIGN dump 8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b 16160; SELFTEST PASS.
- Offline gate: GATE: PASS; Build, Selftest, Seam, Tokens, Format, Scope, Size, Cleanup PASS; LoopLint SKIP. Format caveat above applies.
- Direct dotnet format: failed to connect to build-host pipe, access denied; requires rerun outside this sandbox.
- CRLF verified, no bare LF; git diff --check passed. Whitespace-normalized comparison against HEAD passed for each of the five files.
- Round 1 inventory per its report: font-size 60 -> 10 scanner false positives, font-weight 38 -> 0, font-family 7 -> 0; this round changes no literals.
- The separate build/A4c-type-tokens-check folder remains: the attempted cleanup command was rejected by automatic execution policy.

## Hand checks for the user
In the dev build inspect pad text and headings, a math/list/code note, completion popup, search (Ctrl+F), help, pad menu, and three Settings sections. Confirm nothing looks different.
