**In short**

The dev loop turns a request into an agreed change, checked work, and one commit on `dev`.
You decide what gets built at Approve and try visible changes in a separate dev copy at Hand test.
An item card keeps the request, evidence, plan, and results together so another chat can pick up the work.
These are the eight phases in `docs/loop.md`, "Phases."

# AntiNotepad dev loop v3.1

Guide dated **2026-10-30**, for loop **v3.1** (`docs/loop.md`, "Sessions").

## Why use this instead of one long chat?

The loop gives each job a bounded brief and a worker suited to it: Haiku for launch-and-relay audits and backlog scouts,
Sonnet medium for build/check/test sub-chats, and a Codex role picked by the job for implementation (Luna medium, Terra medium, Luna xhigh or Sol medium)
(`docs/loop.md`, "Sessions"; `docs/agents.md`, "Launch"). This is how it assigns the cheapest capable worker per job;
the files specify routing, not a measured cost saving.

Scripts check the important hand-offs: approval and overlap before Build, the gate before acceptance, and a current gate
plus branch readiness before Merge. A human still judges scope and the screen; the scripts do not replace those decisions
(`tools/approve.ps1`, `tools/gate.ps1`, `tools/merge-preflight.ps1`; `docs/loop.md`, "Check").

The card makes agreement inspectable. Check compares what you requested, what you approved, and what was done. Unapproved
behavior changes stop for your answer instead of silently becoming part of the item (`docs/loop.md`, "Check: plan vs done").

## Start here

1. [Getting Started](Getting-Started): open a master chat and run your first item.
2. [Dev Loop Overview](Dev-Loop-Overview): the diagram and every phase's inputs, output, and stopping check.
3. [Who Does What](Who-Does-What): models, payloads, permissions, and launch commands.
4. [Parallel and Branches](Parallel-and-Branches): isolated worktrees or one branch at a time.
5. [Backlog Scans](Backlog-Scans): research several ideas before choosing one.
6. [Worked Example Loop2](Worked-Example-Loop2): actual reviews, fixes, commits, and the rehearsal.
7. [Tiers and Gates](Tiers-and-Gates), [Triage and Lessons](Triage-and-Lessons), [Where Files Live](Where-Files-Live),
   [Glossary](Glossary), and [Loop History](Loop-History): reference pages.

## Where the rules live

This wiki explains the repository rules; **`docs/loop.md` wins over the wiki**. `AGENTS.md` points to the per-item loop,
`docs/agents.md` defines Codex launches, `docs/dev-loop.md` protects installed and dev copies, and `docs/coding.md`
defines the standard applied to touched files. Slash-command instructions live in the relevant skill, for example
`.claude/skills/next-task/SKILL.md`. Read the rule file when an instruction here needs more detail.

## Known limits

- **The master model is not fixed.** `docs/loop.md`, "Sessions," fixes sub-chat models but names no master model.
- **No live backlog scan is recorded yet.** The scripts were rehearsed, but the rehearsal did not run `/scan-backlog`
  with Haiku scouts (`.coord/reports/Loop2-loop-v3-1-rehearsal.md`, "Summary" and "Hand checks for the user").
- **Nested-worktree role spawning remains unproven by that rehearsal.** Check 7 saw the `.codex/agents/` definitions
  and no trust warning; it did not demonstrate a runtime spawn of `lens_reviewer`
  (`.coord/reports/Loop2-loop-v3-1-rehearsal.md`, "Surprises," 2).
- **CI is manual.** The GitHub gate uses `workflow_dispatch`; a push alone does not trigger it
  (`.github/workflows/gate.yml`; `docs/loop.md`, "Triggers in practice").
- **Hand tests take turns.** Only one dev copy runs, oldest accepted item first, even when up to three items are being
  built in parallel (`docs/loop.md`, "Branches, worktrees and commits").

## Sources

- `docs/loop.md`: Phases; Check; Sessions; Branches; Triggers in practice.
- `AGENTS.md`, `docs/agents.md`, `docs/dev-loop.md`, `docs/coding.md`: project, launch, dev-copy, and coding rules.
- `.claude/skills/next-task/SKILL.md`: the master workflow.
- `tools/approve.ps1`, `tools/gate.ps1`, `tools/merge-preflight.ps1`: scripted hand-off checks.
- `.coord/reports/Loop2-loop-v3-1-rehearsal.md`: exercised behavior and limits.
- `.github/workflows/gate.yml`: manual CI trigger.
