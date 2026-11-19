# Loop17-chat-title-selfheal-r2 report

Changed `tools/title-hint.ps1`: include paused and parked pending rows, recover the phase of a bare paused or parked state from the item card in its worktree or the main checkout, and map `done` to `dev=merged phase=none`. A missing card yields `phase=none`. Added fixture checks for both row sources, done, missing cards and a parked worktree card; the original 11 cases remain.

Self-test output:

```text
TITLE HINT SELFTEST PASS (hand test, running, stalled, failed, rework, check, paused resume, review running, review failed, merged, status paused, done, pending paused, missing card, worktree parked, garbage)
```

Canned injected examples:

```text
title-hint A28-emoji-render-gaps: me=none codex=none dev=paused phase=check
title-hint A28-emoji-render-gaps: me=none codex=none dev=merged phase=none
```

Offline gate: PASS. Loop lint reported existing budget warnings for docs/loop.md, .coord/lessons.md and the original item card. See .coord/reports/Loop17-chat-title-selfheal-r2.gate.md.
