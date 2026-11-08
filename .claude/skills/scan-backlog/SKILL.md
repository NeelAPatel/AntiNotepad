---
name: scan-backlog
description: Scan backlog ideas in a dev snapshot and propose their priority order.
disable-model-invocation: true
argument-hint: "[slug ...]"
---

# /scan-backlog [slug ...]

Master-session research before an item exists; no IDs, plans or fixes. Use the status card and widgets from
`docs/loop.md`. Scouts are Haiku, role `scout`, read-only, no Codex.

1. Read `.coord/STATUS.md`, Plan. Select the named idea slugs, or all `idea:` rows whose State is `needs you` or
   `planned`. Validate each slug against its row; if none match, ask the user. If more than about 10, ask the user
   to choose the batch before starting. Resolve the main checkout's absolute path.
2. Create one snapshot from the main checkout:
   `git worktree add --detach .claude/worktrees/scan/<YYYY-MM-DD> dev`.
   Record its full SHA with `git -C <absolute snapshot path> rev-parse HEAD`. If the path already exists, stop and
   report it; never reuse or remove an unknown snapshot. Create `.coord/research/` in the main checkout if needed.
3. Fill `.coord/templates/scan.md` for each idea. Start one linked Haiku sub-session per idea with `start_session`,
   `user_asked`, `use_worktree: false`, `wake_when_done`, titled `scan · <slug>`, role `scout`. Its brief names the
   absolute snapshot path, SHA, idea's Plan row and the absolute main-checkout report path. All reads are in the
   snapshot, using absolute paths (including any screenshots named by the row). The only write allowed is
   `.coord/research/<slug>.md` in the main checkout. If that report already exists, preserve it until the scout
   replaces it with a complete refreshed scan. End the scout's turn with the absolute file path.
   Tell the user in one line: snapshot path, SHA, and the scouts. Wait for their completion notes; do not poll.
4. When all scouts are back, verify each report exists with the snapshot SHA. Show one table: idea, kind,
   tier/size guess, Where files, overlaps and links, open questions. Propose an order and ask for the user's choice;
   put the order into STATUS only on the user's word. Research files remain untracked in the main checkout until
   their item is approved; do not stage or commit them here.
5. Once every scout has finished reading, run `git worktree remove <absolute snapshot path>` (never `--force`).
   On a failed scout, report the gap and wait for all others before cleanup. On a failed removal, report the path
   and reason; never delete it recursively. Keep all completed research files.
