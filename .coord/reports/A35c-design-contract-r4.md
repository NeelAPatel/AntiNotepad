# A35c-design-contract-r4: Run report

## Summary

Finished the contract rules, extension instructions and known gaps; removed the superseded scale proposal and updated its references. Current code values take precedence over the old proposal, including the 32 px pager target height. The playground measurement and token-source references also needed updating to satisfy the request for all non-archive docs.

## Checks

- `check-design-contract.ps1` and `check-design-tokens.ps1`: pass.
- Offline gate against `docs/A35-ux-pass`: build (zero warnings), selftest, seam, tokens, contract, format, loop lint and size pass. Scope fails on `docs/playground/measure.md` and `docs/playground/tokens.json`: both contain references to the deleted file but are outside the rework briefs' Files lists. Cleanup was skipped by the gate on failure. The requested gate therefore does not pass.
- No product behavior changed. Markdown marker contrast, Clear-tint hint, B-9 and file-size follow-ups remain documented gaps.
