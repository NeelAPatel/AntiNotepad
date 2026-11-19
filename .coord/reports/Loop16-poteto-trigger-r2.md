# Loop16-poteto-trigger-r2

## Summary
Fixed both poteto gate review findings.

## Change list
- Brief marker matching is ordinal case-insensitive; self-test rejects a malformed lowercase-marker brief with exit 2.
- Fenced fallback requires `/pstack:poteto-mode` at the start of a content line, followed by whitespace or end of line; self-test rejects the unavailable suffix while retaining the argument/CRLF pass case.

## Checks run
- `pwsh -NoProfile -File tools/poteto-gate.ps1 -SelfTest` — PASS.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop16-poteto-trigger-r2 -Offline` — PASS.
