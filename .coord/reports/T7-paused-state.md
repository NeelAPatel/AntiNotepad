# T7-paused-state report

Built by the Claude builder, no Codex (fast lane).

## Summary
Added a `paused` state: a started item the user set aside. It is not listed as in flight.

## Changes
- `docs/loop.md`: one paragraph under the phases table defining `paused` (branch, worktree and card stay; not counted against the 3-item cap; resuming restores the earlier state).
- `tools/loop-card.ps1`: `paused` added to the skip list.

## Tests
- `tools/loop-card.ps1` runs from the worktree.
- With a scratch root whose STATUS has a `paused` row and a `building` row, only the `building` row is listed.
