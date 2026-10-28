---
name: lens-review
description: Review an AntiNotepad diff for logic, behavioral coupling and reliability, keeping only findings you can prove. Use when asked to review a task's diff, when a brief says "lens review", or as the lens_reviewer agent.
---

# Lens review (AntiNotepad)

You review someone else's change. Your job is to find what will break **elsewhere or later**, not to restyle the code.
Past escaped bugs in this repo were all behavioral coupling, not syntax: a changed method result (A30 Save on an
unavailable folder), state that latched for good (A30 IsAvailable), a cache trusting stale input during deferred work (A28),
a missing-input startup crash (A43b), and a global window-style change that broke every bar style (A32/A34).

## Inputs
The brief (`.coord/tasks/<ID-slug>.md`), the implementer's report (`.coord/reports/<ID-slug>.md`), the gate report if present
(`.coord/reports/<ID-slug>.gate.md`) and the diff. Read the changed files around each hunk, and the callers of anything whose
signature or meaning changed (search with `rg`).

## Checklist (go through every item for every hunk)
1. **Scope**: does each hunk serve a goal line of the brief? Flag unrelated edits (icons, tokens, styles, renames, defaults).
2. **Contracts**: did any public or shared method change its result, exceptions, side effects or meaning? List every caller
   and say whether each still holds. Compare with the report's "Behavior changes"; a missing entry is a finding.
3. **Shared state**: new or changed fields, statics, settings or flags. Can a state be set and never reset (latch)? Is it
   read by another feature before it is valid?
4. **Timing and order**: debounces, timers, dispatcher posts, deferred analysis, async saves. Can something run before, after
   or during another step it depends on? Is any cache keyed on input that can go stale?
5. **Inputs and edge cases**: empty, missing, unavailable, huge, negative, CRLF vs LF, a folder that disappears or returns,
   first run, second run.
6. **Cleanup**: event handlers unsubscribed, timers stopped, windows and controls released, files closed; anything that grows
   without bound.
7. **Failure paths**: exceptions caught at the right level, user data never lost, quit and save still behave on failure.
8. **Layering**: no new dependency from `Model`/`Editor` up into `UI`; native calls stay in `Platform/` (LAYERS and the seam
   script catch compile-time leaks; you check intent and runtime coupling).
9. **Tests**: is each new promise covered by a `CONTRACTS` row or a small selftest check? Are test hooks kept out of product
   classes?

## Proof rule
A finding counts only with evidence: a failing build or selftest, a short probe you ran (in `build/<ID-slug>-review`, deleted
after), or an exact code path quoted with file:line showing the failure step by step. Anything else goes under "Unproven
risks", at most five, one line each. Never edit files under `product/`.

## Output format
```
# <ID-slug> lens review
## Findings (proven)
1. [blocker|major|minor] file:line - what breaks, how proven, suggested smallest fix
## Behavior changes check
Report's list vs the diff: matches | missing: ...
## Scope check
In scope | unrelated hunks: file:line ...
## Unproven risks
- ...
## Checks run
- build / selftest / probes, with result lines
VERDICT: accept | rework | ask-user
```
`rework` if any blocker or major finding. `ask-user` only for a design question the brief does not settle.
