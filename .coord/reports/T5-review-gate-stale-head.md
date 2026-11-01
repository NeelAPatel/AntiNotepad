# T5-review-gate-stale-head report

Built by the Claude builder (fast lane), no Codex.

## Summary
`tools/merge-preflight.ps1` no longer fails "stale Head" when only the item's card and reports were committed after the gate. The gate report's Head may now be an ancestor of the branch tip if every file changed between them is under `.coord/tasks/<ID-slug>` or `.coord/reports/<ID-slug>` (prefix, so `-rN` and `-review` files count). Anything else still fails "stale Head: re-run the gate".

## Changes
- New `Test-GateHead` function: exact-tip match returns true; otherwise the Head must be a 40-hex SHA, an ancestor of the tip, and `git diff --name-only --no-renames <Head> <tip>` must list only allowed paths (`^\.coord/(tasks|reports)/<Task>(?:[-.]|$)`).
- The Head check now calls it. When the Head is not the tip, preflight also prints `CHECK: commits after the gate touch only the card and reports`.
- Nothing else in the script changed.

## Tests
- Script parses (0 parse errors).
- Preflight needs the main checkout and a clean dev, so the Head logic was tested directly: `Invoke-Git` and `Test-GateHead` extracted from the script by AST and called against a throwaway local worktree and branch (never pushed, both removed afterwards), with a fake gate report whose Head was the starting commit:
  - Head equals tip: true
  - card edit + new `reports/<Task>-review.md` after the gate: true
  - another item's card (`.coord/tasks/OtherItem-x.md`) after the gate: false
  - `tools/gate.ps1` change after the gate: false
  - Head that is not in the repo: false
- Not run: a full end-to-end preflight.
