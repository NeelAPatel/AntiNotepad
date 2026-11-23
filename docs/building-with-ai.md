# Building with AI

## Why this exists

AntiNotepad is both a Windows scratchpad and an experiment in learning how AI models, agents and orchestration can build software under human scrutiny. I set direction, approve plans, review results and test the app by hand. The process is part of the project, not a claim that automation guarantees correctness.

## The loop in one page

An item begins with my request. A director routes it to one item chat, which investigates the current behavior, records findings and proposes a plan. I approve the plan before a worktree build. Codex implements; checks and review compare the result with the request and plan. I perform any needed hand test. A director reviews and merges the finished item to `dev`; release to `main` is a separate decision that stays with me. The [loop](loop.md), [agent roles](agents.md) and [playbook](playbook.md) give the current details. PStack and the agent configurations help organize the work, while cards and reports preserve its evidence.

## Loop eras

- **October 17–29, 2026: Era 0.** The early product and process grew quickly; on October 29 the history was rewritten and the process redesigned.
- **October 30–November 5, 2026: Era 1 and loop v3.** Fresh item counters, worktree and gate tooling, audits and a wiki made the process more explicit.
- **November 6–22, 2026: loop v5 and later refinement.** Merge policy, guard and worktree checks, a slimmer status record and a one-chat item flow evolved alongside product work. The [archive](../.coord/status-archive.md) records dated examples; [lessons](../.coord/lessons.md) records misses and checks.

## Milestone moments

Selected commit subjects and dates from the project history; history may be rewritten for publication.

| Date | Commit subject |
|---|---|
| 2026-10-22 | `overnight-spike: build - overnight rebuild of the app replaces v0.1` |
| 2026-10-23 | `R1-antipad-replaces-antiquickpad: build - retire AntiQuickPad` |
| 2026-10-23 | `N1-antinotepad-identity: build - AntiNotepad rename` |
| 2026-10-29 | `process-redesign: Markdown status card, attribution off, redesign notes` |
| 2026-10-29 | `Loop1-process-v3: loop v3 rules, coding standard, glossary, editorconfig` |
| 2026-11-20 | `A37-version-0-7-0: stamp the app as plain 0.7.0` |
| 2026-11-22 | `A38-version-0-7-1: stamp the app as 0.7.1` |

## Where to look

- [Item cards](../.coord/tasks/): requests, findings, plans and checks.
- [Reports](../.coord/reports/): implementation and review evidence.
- [Claude configuration](../.claude/), [Codex configuration](../.codex/) and [agent skills](../.agents/): roles, skills and instructions.
- [Roadmap tooling](../tools/roadmap/): the planning interface.
- [Wiki](https://github.com/NeelAPatel/AntiNotepad/wiki): a guided account of the app and process.

## Lessons learned

Use real transcript and status-row shapes in hook tests, including line endings and metadata. Check window-level handlers before assigning a shortcut. Stress parsing with deep nesting, large inputs and overlapping operations. Keep briefs, planned files and checks aligned. These examples and their specific failures are in the [lessons log](../.coord/lessons.md).

## What went wrong

The loop has needed rework: a paste change exposed deep-nesting and performance problems in review; shortcut assumptions missed window-level bindings; a self-test hung; and a hand test caught a default shortcut I did not want. Review, gates and human checks caught different problems, but none was sufficient alone. The record also includes flaky checks and incomplete reports, so a passing automated check is evidence, not a substitute for judgment.
