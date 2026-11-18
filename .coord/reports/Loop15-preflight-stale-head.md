# Loop15-preflight-stale-head

## Findings evidence
- `T17-multi-dev-instances`: no retained merge-preflight output; stale Head result is unproven. Its reports retain gate results only.
- `A24-save-delay-cleanup`: no retained merge-preflight output. The lesson at `.coord/lessons.md` says its card edit was missed on CRLF and preflight failed on empty Check, which is a different failure.
- `Loop13-drop-chat-titles`: implementation report says `CHECK: commits after the gate touch only the card, reports and STATUS`; the T5 allowance passed.
- `Loop14-codex-closing-files`: no retained merge-preflight output found; stale Head result is unproven.
- `A22-wrap-hanging-indent`, `A23-paste-keep-structure`: no retained merge-preflight output; stale Head result is unproven.
- Earlier A2, A5 and A7 failures predate T5's allowance. T16/A21 are named in the 2026-11-17 lesson, but their preflight outputs are gone, so the exact cause cannot be confirmed from those merges.

## Cause and failed rule
`tools/gate.ps1` appends `-dirty` to the report Head when tracked files outside its small dirty allowlist are modified. `Test-GateHead` accepts only a 40-hex Head, so an uncommitted card/report at gate time makes the gate report fail the Head check. The failed rule was `/review-task`'s missing clean-tree instruction at steps 2 and 6; `docs/dev-loop.md`'s dirty-file guidance does not tell the item chat to commit its card and reports before gating.

## Dry-run: AST-extracted Test-GateHead
The approved branch/worktree was left untouched. `docs/agents.md` says Codex cannot write to `.git`, so the requested throwaway local branch was not created; the AST-extracted function was exercised with a disposable PowerShell `git` stub returning equivalent ancestor and diff results:

```text
PASS card-only commit: True
PASS tools commit rejected: True
PASS -dirty Head rejected: True
```

The script now emits this specific diagnostic before invoking `Test-GateHead` for a dirty report:

```text
gate ran on a dirty tree: commit the card and re-run the gate
```

## Gate
`pwsh -NoProfile -File tools/gate.ps1 -Task Loop15-preflight-stale-head -Offline`: `GATE: PASS` (build, selftest, seam, tokens, format, scope, loop lint, size, cleanup). Existing budget warnings remain for docs/loop.md, lessons and this card.