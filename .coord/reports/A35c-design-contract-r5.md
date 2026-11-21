# A35c-design-contract-r5: Run report

## Summary

Added size exceptions with current line-count ceilings and reasons. The checker uses the gate's newline counting, reports when a ceiling can be lowered after a shrink, and rejects growth past a ceiling or any unlisted product C# file above 600 lines. The checkout also had three oversized self-test files beyond the five named in the card, so all eight are recorded. Updated the contract's Size rule and Known gaps.

## Checks

- `pwsh tools/check-design-contract.ps1`: pass.
- `pwsh tools/gate.ps1 -Task A35c-design-contract-r5 -Offline -Base docs/A35-ux-pass`: pass (build, selftest, seam, tokens, contract, format, scope, loop lint, size, cleanup).
