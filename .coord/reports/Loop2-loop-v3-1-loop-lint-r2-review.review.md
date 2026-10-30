# Loop2-loop-v3-1-loop-lint-r2 lens review

## Findings (proven)
None.

## Behavior changes check
Report's list vs the diff: matches. `tools/loop-lint.ps1` now requires an existing directory, refuses a zero-file live inventory with exit 2, and decodes only backslash escapes for ASCII punctuation (plus the existing escaped-space handling) before URI decoding. The existing exit-0/exit-1 behavior remains intact for a live checkout and lint problems; no callers, shared state, deferred work, cleanup, or layering behavior changed.

## Scope and plan check
In plan | unrelated hunks or unlisted deviations: none. The inspected changes are limited to the three rework requirements in `tools/loop-lint.ps1`.

## Unproven risks
- None.

## Checks run
- Actual checkout: `pwsh -NoProfile -File tools/loop-lint.ps1` returned `LOOP-LINT: PASS (37 files)`, exit 0.
- Actual file root (`tools/loop-lint.ps1`) returned `LOOP-LINT: ERROR`, exit 2; an existing empty directory returned the no-live-files error, exit 2.
- Disposable actual-script probes: existing escaped period, hyphen, space and parentheses; titles; empty/external links; Windows-style paths with backslashes before letters, digits and a Unicode letter; encoded separators/fragments; and a valid doubled trailing backslash all passed. Missing escaped, Windows-style, encoded and trailing-backslash destinations returned `missing-path`, exit 1.
- `%5C%2D` resolving to a path with a separator followed by `-` confirmed Markdown escape decoding occurs before URI decoding. The disposable probe directory was removed.
- No branch changes, commits, product edits, STATUS/card edits, or application actions were made.

VERDICT: accept
