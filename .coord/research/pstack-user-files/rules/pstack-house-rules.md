# pstack house rules. setup-pstack never touches this file; it overrides pstack-models.md and pstack skill defaults.
# Builder rule (all repos):
# If the repo's AGENTS.md or CLAUDE.md names another agent as the implementer (for example "Codex implements"),
# that agent writes all product code. pstack's delegate, swarm, hillclimb and arena-for-code steps then mean:
# brief and launch that agent the way the repo's own docs describe, and never write product code with Claude subagents.
# Offload rule, "Claude decides, the implementer digs": when the implementer has a read-only role, it also runs
# work that is mostly reading many files or running commands to produce a report: code walkthroughs (how, why),
# blast-radius with its proof scripts, correct sweeps, deslop, Findings scouting, writing and running tests.
# Brief it with the pstack skill's text and a short report format; Claude judges the report.
# Claude keeps talking with the user, decisions, plans, approval, design choices, the final verdict on reports,
# merges, routing, and any check that needs the running app, a port, a browser or the screen.
# Floor: a question answerable from about 5 files or fewer stays with Claude; a handoff costs more than it saves.
# Escalation: the user's advisor (Opus) is the step up; keep pstack roles on inherit-parent, never also escalate pstack.
# The repo's merge and branch rules also override pstack's Opening a PR, Babysit and Shipping steps.
# Tiny fixes: when a fix is a few lines in known places (one or two files, no shared timing or state)
# and a Haiku subagent would cost less than an implementer run, give it to a Haiku subagent instead.
# The Haiku fix still passes the repo's own checks. When unsure which is cheaper, use the implementer.
# Second opinion: for arena judging and a second interrogate reviewer, use the implementer agent's
# review pass (the repo's docs say how), not a second Claude model.
# Fable: never pick Fable for any helper or panel. If this chat runs on Fable, helpers run on Opus.
# Only the user picks Fable, by asking in chat.
# 'defaults: 2' in pstack-models.md is a format version marker, not a helper count; each panel runs one helper per listed entry, except the arena cross-judge pool (one judge, picked in order) and architect (at least two runners).
# Approval: if the repo has a plan approval step (for AntiNotepad-style loops, an "## Approve" section on the item card),
# stop at it and wait. "Just do it" and "Never block on the human" apply only to read-only investigation and to
# reversible work inside an approved plan. No other "go" word or command exists; the repo's approval step is the signal.
# Failing repro first: if the repo requires every commit to pass its checks, land the repro and the fix in one commit,
# and paste the repro failing before the fix, then passing, in the report.
# Writing (default): replies keep the repo's output style for layout (cards, tables, one decision) and use pstack's
# writing rules for sentences. Briefs, commit messages and docs use pstack's writing rules.
# "pstack voice": when the user says this, reply in pure pstack style until they say "house style".
# deslop: there is no pstack /deslop here; use the user skill `deslop` (diffs against the item's base branch).
# Skill names: always write a skill or slash command as code with its slash and prefix, e.g. `/pstack:how`, never as a plain word ("how"), so it reads as a command.
# After compaction: if this chat ran /pstack:poteto-mode before, reload that skill before continuing, then re-read the item card.
