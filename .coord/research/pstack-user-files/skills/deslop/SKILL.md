---
name: deslop
description: Find and remove AI-generated code slop in the current branch's diff (needless comments, odd defensive code, silencing casts, deep nesting). Use for /deslop, "deslop it", or the pre-review cleanup pstack's poteto-mode asks for.
---

# Remove AI code slop

Adapted for Claude Code from the `deslop` skill in cursor/plugins `cursor-team-kit` (MIT licence).

## Scope

Diff the branch against its base, not blindly against main:
- Repo says main is releases only and items branch off another line (for example dev): use that line.
- Otherwise use the default branch.
- Command: `git diff $(git merge-base HEAD <base>)...HEAD`. Only lines this branch added are in scope.

## Look for

- Comments that restate the code, narrate the change, or break the repo's comment rules (read them first if the repo has any).
- Defensive checks, try/catch or null guards that are abnormal for trusted code paths here.
- Casts, `!`, `dynamic`, `any` or pragma suppressions used only to silence the compiler or analyzers.
- Deep nesting that early returns would flatten.
- Anything else inconsistent with the file and its neighbours (naming, helpers that duplicate existing ones, dead code).

## Who edits

- If the repo names another agent as the implementer (for example "Codex implements"), report only: list each finding as `file:line: what, why, the fix`, ready to paste into that agent's next brief. A tiny fix may follow the house rule's Haiku exception.
- Otherwise edit directly.

## Guardrails

- Keep behaviour unchanged unless fixing a clear bug; name any such bug separately.
- Minimal, focused edits. No rewrites, no new abstractions.
- Re-run the repo's own checks after edits.
- Final summary: 1-3 sentences.
