# Loop21-umbrella-items report

## 1 Summary
Implemented umbrella rows, startup/title hints, parent-base merge checks, survival check and loop rules. Not ready for merge: the required strict gate fails on the roadmap renderer outside the card's Files list; the chain and independent Codex review remain outstanding.

## 2 Change list
- `tools/poteto-gate.ps1`: named item briefs require a STATUS/pending row, with log-only handling and checks (Plan 1).
- `tools/roadmap-data.ps1`, `tools/roadmap/index.html`: expose the parent from Scope and render nested, collapsible rows (Plan 1).
- `tools/start-card.ps1`, `.claude/settings.json`: fail-open SessionStart reminder (Plan 2).
- `tools/title-hint.ps1`: transcript title comparison and missing-row card hint (Plan 3).
- `docs/loop.md`, `AGENTS.md`, `.coord/templates/item.md`: umbrella rules, board, crosstalk, READY, card fields and v5.5.0 (Plan 1, 4).
- `tools/merge-preflight.ps1`, `.claude/skills/merge-task/`: parent-base preflight and merge directions (Plan 5).
- `tools/survival.py`, `tools/gate.ps1`: generalized survival check and optional gate (Plan 5).

## 3 Behavior changes
Item dispatch can be blocked when a named row is missing; log-only suppresses the block. SessionStart and title hooks give additional hints. Roadmap rows with child Scope are nested under their parent. A child can be checked and merged against a parent branch; optional survival flags missing product lines from prior matching merge subjects.

## 4 Out-of-scope files
`tools/roadmap/index.html`: required to render folding; the listed data script alone cannot change the page's folding behavior. The strict scope gate flags this path. This report and gate report are generated item reports.

## 5 Plan deviations
The required strict gate fails scope until the item chat resolves the renderer's Files-list discrepancy. The `-LooseScope` diagnostic gate passes with that warning, but is not the required strict pass. The chain test needs the user's separate go and has not run. Independent Codex review remains to be done by the item chat. Hooks and nested roadmap behavior have not been exercised in live chats or a browser; start-card guesses from the worktree or transcript and may not identify a main-checkout item at startup before its first prompt. Title comparison depends on a readable transcript with custom-title entries. Failed early survival selftest attempts left inaccessible scratch directories in the system temp folder and `build/`; the corrected selftest passes and cleans its own scratch directory.

## 8 Checks run
- `poteto-gate.ps1 -SelfTest`, `title-hint.ps1 -SelfTest`, `start-card.ps1 -SelfTest`, `merge-preflight.ps1 -SelfTest`, `python -I tools/survival.py --selftest`: pass.
- `roadmap-data.ps1 -Check`: pass, 180 rows, zero warnings.
- `gate.ps1 -Task Loop21-umbrella-items -Offline`: fail, Scope (`tools/roadmap/index.html`); build, product selftest, seam, tokens, format, LoopLint and size pass.
- Same gate with `-LooseScope`: pass with scope warning. This overwrote the generated gate report with the diagnostic run.
- `git diff --check`: pass.

## 9 Hand checks
After scope resolution and review, inspect roadmap parent/child folding across three levels, session start/resume/compact cards and title changes. Run the three-level throwaway chat chain only on the user's separate go, including rows, titles, rebases and merges up the chain.
