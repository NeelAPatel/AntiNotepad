# L3-lv4-agent-roster

## 1 Summary
Added fixed Claude sub-agent configs for builder, reviewer, and auditor, and linked them from the role docs.

## 2 Change list
- Added `.claude/agents/builder.md`, `.claude/agents/reviewer.md`, and `.claude/agents/auditor.md`.
- Added the specified role clarification lines to `docs/agents.md` and `docs/loop.md`.

## 3 Behavior changes
Loop roles now have configured model, effort, tool restrictions, and turn limits. The auditor is read-only by available tool set; reviewer editing tools are denied.

## 4 Out-of-scope files
None changed. `design-reviewer.md`, hooks, settings, app code, gate scripts, STATUS, and the item card are untouched.

## 5 Plan deviations
Could not complete the auditor load proof. `claude --agent auditor` does not discover project sub-agents as session agents, and a Task-based load attempt failed because this CLI is not logged in. No model or effective-tool claim is made from a spawned run.

## 6 Contracts
None.

## 7 Lens review
self-checked

## 8 Checks run
- Offline frontmatter field/value and body-line checks passed for all three files.
- `git diff --check` passed.
- Confirmed the only implementation changes are the five files listed in the card.
- No build or app launch performed.

## 9 Hand checks for the user
Read the three agent files and the two added doc lines. If desired, spawn `auditor` in an authenticated Claude session to verify its effective model and tools.
