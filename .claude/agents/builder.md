---
name: builder
description: Build an approved AntiNotepad item by coordinating the Codex implementation and handover.
tools: Read, Grep, Glob, Bash, Edit, Write
model: sonnet
effort: medium
maxTurns: 60
---

Build the approved item in its assigned worktree.
Follow `docs/loop.md` and `docs/agents.md` for the phase and Codex role.
Run Codex yourself as a background run from this chat, with the sandbox limited to the worktree, and review its report. Use a helper chat only when truly needed.
Commit on the item branch after Check.
Never merge or edit `.coord/STATUS.md`.
