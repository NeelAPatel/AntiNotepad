# Loop17-chat-title-selfheal implementation report

Changed `tools/title-hint.ps1`, `.claude/settings.json`, `docs/loop.md`, `docs/loop-sessions.md`, and the `next-task` and `merge-task` skill files. The hook reads the main checkout's rows and Codex run records, emits ASCII hints, and fails open. Item chats use the matching hint; paused and parked items resume by clearing the development word and updating the row. Audit, rework and review run suffixes are recognized.

Self-test:
```text
TITLE HINT SELFTEST PASS (hand test, running, stalled, failed, rework, check, paused resume, review running, review failed, merged, garbage)
```

Canned examples (in the handover's order):
```text
title-hint A28-emoji-render-gaps: me=needs-you codex=none dev=none phase=hand-test
title-hint A28-emoji-render-gaps: me=none codex=codexing dev=none phase=build
title-hint A28-emoji-render-gaps: me=none codex=none dev=none phase=check
title-hint A28-emoji-render-gaps: me=needs-you codex=failed dev=none phase=build
title-hint A28-emoji-render-gaps: me=none codex=none dev=paused phase=build
title-hint A28-emoji-render-gaps: me=none codex=none dev=merged phase=none
```

Offline gate: PASS (build, selftest, seam, tokens, format, scope, loop lint, size and cleanup). The first run failed scope because the card had multiple paths per line; the approved paths were split onto separate lines and the gate passed on rerun. Loop lint warned that docs/loop.md, .coord/lessons.md and the item card exceed their budgets.

Lens review: accepted; an audit/rework/review run-name finding was fixed and covered by self-tests. Live throwaway chat and color check: pending the further user OK.
