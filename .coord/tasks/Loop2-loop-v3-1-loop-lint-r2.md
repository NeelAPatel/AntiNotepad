# Loop2-loop-v3-1-loop-lint-r2: escaped Markdown link punctuation, and a file passed as -Root

Rework round of `Loop2-loop-v3-1-loop-lint` (`docs/loop.md`). Read the item card `.coord/tasks/Loop2-loop-v3-1.md` and the
step brief `.coord/tasks/Loop2-loop-v3-1-loop-lint.md` first; they still apply unless this file says otherwise.

Tier: Low · docs-tools
Role: director

## Why this round
Proven review findings (`.coord/reports/Loop2-loop-v3-1-loop-lint-review.review.md`):
1. `tools/loop-lint.ps1` ~line 65: Markdown backslash escapes of ASCII punctuation in a link destination
   (`[x](docs/agents\.md)`, `[x](docs/dev\-loop.md)`) are not decoded before the path is resolved, so a valid link reports
   `missing-path`.
2. `tools/loop-lint.ps1` ~line 17: `-Root <a file>` enumerates nothing and prints `LOOP-LINT: PASS (0 files)`, exit 0.

## Change to the plan
None. Fix within the step:
- Decode `\` + ASCII punctuation to the punctuation (CommonMark backslash escapes) before URI decoding and resolution,
  keeping the existing paren/title/space handling.
- Require `-Root` to be an existing directory; otherwise throw through the existing exit-2 path. Also make a run that
  finds 0 live files exit 2 (a lint that checked nothing must not PASS).

## Files
Edit only `tools/loop-lint.ps1`.

## Done means
- In-memory or throwaway probe (leave no artifacts): `[x](docs/agents\.md)` and `[x](docs/dev\-loop.md)` in a live file
  pass; a link to an absent file still fails; `-Root tools/loop-lint.ps1` exits 2; a directory with no live files exits 2;
  the checkout still prints `LOOP-LINT: PASS (37 files)`.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline` passes with `LoopLint: PASS`.

Constraints: no commit, push, branch switch or merge; never edit STATUS, the card, product.
Report: `.coord/reports/Loop2-loop-v3-1-loop-lint-r2.md` (short, step report format).
