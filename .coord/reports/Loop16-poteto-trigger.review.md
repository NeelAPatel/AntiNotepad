# Loop16-poteto-trigger lens review

## Findings (proven)
1. [major] .claude/skills/next-task/SKILL.md:40 - The dispatch instructions use lowercase "you are the item chat", but tools/poteto-gate.ps1:29 recognizes only case-sensitive "You are the item chat"; consequently this documented dispatch route bypasses validation even when both required opening lines are absent. Canned Brief input "you are the item chat for Loop16-poteto-trigger; begin Findings" exited 0, while the identical uppercase version exited 2. Smallest fix: capitalize the marker in the next-task template and add a regression check for the documented template's marker.
2. [minor] tools/poteto-gate.ps1:60 - The fenced fallback regex accepts the command as a substring, so a first response containing only a fenced `/pstack:poteto-mode-unavailable` passes without a Skill call or a usable fallback command. A canned first-turn transcript with that fenced text exited 0. Require the command at the start of a code-block content line with an argument boundary (whitespace or end of line) and cover the invalid-suffix case.

## Behavior changes check
Report's list vs the diff: matches the intended script behavior and documents LogOnly and project-directory logging; findings above qualify its enforcement claims. The report explicitly assigns documentation and live checks to the item chat. The card records subsequent live checks; those were not repeated or independently verified in this offline review.

## Scope and plan check
In plan: script, self-tests, dispatch playbook, next-task template, and playbook hook row serve the approved After behavior; card, report, and review brief are supporting item artifacts. No unrelated implementation hunks or unlisted plan deviations found. LogOnly is an explicitly reported addition supporting the approved rollout. Finding 1 prevents the next-task route from satisfying the planned malformed-brief refusal. No product code changed.

## Unproven risks
- None retained.

## Checks run
- `git diff dev...HEAD`, full script, changed documentation context, item card, implementer report, and existing gate report inspected; gate report is prior evidence, not a gate rerun.
- `pwsh -NoProfile -File tools/poteto-gate.ps1 -SelfTest`: exit 0, `POTETO GATE SELFTEST PASS`.
- Canned Brief probes: CRLF valid template exit 0; leading blank line exit 2 as required by exact line 1; uppercase malformed marker exit 2; lowercase next-task malformed marker exit 0 (finding 1).
- Canned Stop probes: isMeta plus tool_result without Skill/fallback exit 2; Skill plus isMeta/tool_result exit 0; second real user after missing first-turn launch exit 0; CRLF tilde-fenced fallback exit 0; invalid command suffix exit 0 (finding 2); ordinary chat exit 0.
- Active guard with no transcript path, malformed transcript, and logging root pointing at a file all exited 0. Existing self-test also covers garbage stdin and an unreadable transcript. Logging probes used CLAUDE_PROJECT_DIR under build/Loop16-poteto-trigger-review, removed after review.
- Offline only; no live chats, user-scope Claude files, product files, Git mutations, or STATUS edits. Only the requested report remains from this review.

VERDICT: rework
