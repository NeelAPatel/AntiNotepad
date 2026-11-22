# A35d-spacing-tokens build step 4

## 1 Summary
Updated the design scale reference with the current spacing and stroke scales, current ChromeTokens values including the 32 px pager dot button height, and reasons for the named exceptions.

## 2 Change list
- `docs/design-scale.md:151` | Documented Space and stroke token values | Step 4
- `docs/design-scale.md:247-253` | Corrected pager height and documented newer ChromeTokens values | Step 4
- `docs/design-scale.md:305-338` | Added named exceptions with reasons, including editor insets, hint offsets, artwork, fitted columns, samples, typography and layout sizes | Step 4

## 3 Behavior changes
None. Documentation only.

## 4 Out-of-scope files
None.

## 5 Deviations
None.

## 6 Checks run
- Offline gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A35d-spacing-tokens -Base 3b6ecb5 -Offline` — PASS (exit 0).
- Build: PASS, 0 warnings, 0 errors. Selftest: `SELFTEST PASS`.
- DESIGN dump: not run; step 4 is documentation only.
- TOKENS: `TOKENS PASS`.
