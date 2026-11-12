# <ID-slug>-audit: findings for <ID-slug>

Role: director (gpt-6.1-sol medium), read-only. Product `product/AntiNotepad`. You run in the main checkout on `dev`
(no item branch exists before Approve): never switch, merge, commit, or write any files. Never launch or kill AntiNotepad.
Read `AGENTS.md`, then the item card `.coord/tasks/<ID-slug>.md` (its Request is what the user asked for).

## Answer these (this is the Findings phase of `docs/loop.md`)
1. **Kind and why**: bug, feature, polish or process. What about today's behavior justifies the request?
2. **Now**: what the app does today in the area, with evidence (file:line, a probe or selftest result). For anything
   visible, say in plain words what the user would see on dev today. If dev already does what the Request asks, say so first.
3. **Reproduction (bugs)**: give an exact code path quoted step by step. Describe probes that need a build as
   steps for the user/manager to run; do not run builds or probes that write files. If you cannot, say "not reproduced"
   and what you would need from the user. Do not guess a cause.
4. **Where**: the files that would change, their callers, and shared state, timers or caches the change can reach.
5. **Risk**: the riskiest part, and which gate in .claude/skills/next-task/phases.md ("Gates menu") would catch it.
6. **Tier**: confirm or raise the card's tier, with the reason.
7. **Recommendation**: the smallest change that meets the Request, the "After" state, the code tests and UI checks that
   would prove it, open scope questions for the user, and whether to split the item.

## Final message: the report
Your final message is the report; never write it to a file. Use sections 1-7 above, short, each claim with its evidence.
End with `READY: yes` or `READY: no - <what is missing>`.
