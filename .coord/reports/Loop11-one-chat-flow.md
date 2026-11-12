# Loop11-one-chat-flow report
Docs-only text written by the builder chat directly (no Codex).

## Change list
- `docs/loop.md` v5.4.0 (8108 B): one chat per item, item chat sets tier, one asker, own STATUS row.
- `next-task` skill: new 1a (director starts item chat), audit runs in the same chat, Build continues in it.
- `review-task`, `merge-task/branches`, `agents`, `loop-sessions`, `playbook`, `AGENTS.md`, `decisions`, `glossary`, templates, output style aligned.
- `.claude/settings.json`: `start_session` ask rule removed.

## Behavior changes
Loop ownership only; no product code. `approve.ps1` unchanged.

## Plan deviations
Files list widened (gate Scope). `catch-up.ps1` comment still says v5.3.0 (tool, left alone). `auditor` agent file kept.
