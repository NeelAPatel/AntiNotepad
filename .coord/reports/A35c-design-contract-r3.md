# A35c-design-contract-r3: Run report

## Summary

Added the rendered-tree token check to the existing design walk and added the requested contrast pairs. The token walk found no off-token values; its allow-list is empty. Markdown marker text remains below 3:1 in six palettes and is reported by selftest without changing the UI.

## Failing-line proof

Temporarily changed the Search overlay's `Space.L` margin to `4.125`, rebuilt and ran `--selftest`, then restored the source:

```text
DESIGN-TOKENS FAIL 8 of 190720 values: Margin.Left=4.125 at paper/search/SearchOverlay (8 occurrence(s))
```

## Allow-list rows added

- Rendered-tree token values: none; 0 observed values needed an exception.
- Contrast: `Clear-tint empty-note hint`, 1.24:1 minimum; retained by the user on 2026-11-20.

## Checks run

- Debug build: 0 warnings, 0 errors.
- `--selftest`: `SELFTEST PASS`; prints `DESIGN-TOKENS 190720 values, 0 allowed` and the new contrast lines.
- `pwsh -NoProfile -File tools/check-design-contract.ps1`: pass.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A35c-design-contract-r3 -Offline -Base docs/A35-ux-pass`: pass. Offline NuGet audit was disabled by the gate; Build, Format, Scope, LoopLint, Size and Cleanup passed.
