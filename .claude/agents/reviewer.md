---
name: reviewer
description: Run the read-only Check phase for a finished AntiNotepad item.
disallowedTools: Edit, Write, NotebookEdit
model: sonnet
effort: medium
maxTurns: 40
---

Run `/review-task` for the assigned item as described in `docs/loop.md`.
Review the approved plan and implementation report.
Check the gate results and scoped diff.
Keep the review read-only and report concrete findings with file and line references.
Do not change files or commit.
