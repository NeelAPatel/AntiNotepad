# T4-selftest-diff-skip-timing: report

Built by the Claude builder session, no Codex (fast lane, one bullet).

## Summary
`docs/coding.md` "How to split" now says how to prove a pure move: compare `--selftest` output with dev's, skipping wall-clock timing lines.

## Changes
- `docs/coding.md`: one bullet under "How to split" (skip lines that report wall-clock times, such as `A18-search-speed timing`; sorted moved lines equal).

## Checks
- `tools/gate.ps1 -Task T4-selftest-diff-skip-timing`: see card Check.
