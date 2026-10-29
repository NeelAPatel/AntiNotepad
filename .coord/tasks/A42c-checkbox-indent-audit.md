# A42c-checkbox-indent-audit: findings for A42c-checkbox-indent

Role: director (gpt-6.1-sol medium), read-only. Product `product/AntiNotepad`. Branch `feat/A42c-checkbox-indent` is checked out:
never switch, merge, commit, or edit anything outside `.coord/reports/`. Never launch or kill AntiNotepad.
Read `AGENTS.md`, then the item card `.coord/tasks/A42c-checkbox-indent.md` (its Request is what the user asked for).

## Answer these (this is the Findings phase of `docs/loop.md`)
1. **Kind and why**: bug, feature, polish or process. What about today's behavior justifies the request?
2. **Now**: what the app does today in the area, with evidence (file:line, a probe or selftest result). For anything
   visible, say in plain words what the user would see on dev today. If dev already does what the Request asks, say so first.
3. **Reproduction (bugs)**: reproduce it with a probe in `build/A42c-checkbox-indent-audit` (deleted after) or an exact code path
   quoted step by step. If you cannot, say "not reproduced" and what you would need from the user. Do not guess a cause.
4. **Where**: the files that would change, their callers, and shared state, timers or caches the change can reach.
5. **Risk**: the riskiest part, and which gate in `docs/loop.md` ("Gates menu") would catch it.
6. **Tier**: confirm or raise the card's tier, with the reason.
7. **Recommendation**: the smallest change that meets the Request, the "After" state, the code tests and UI checks that
   would prove it, open scope questions for the user, and whether to split the item.

## Write: `.coord/reports/A42c-checkbox-indent-audit.md`
Sections 1-7 above, short, each claim with its evidence. End with `READY: yes` or `READY: no - <what is missing>`.

## Background for this item
A behavior-neutral refactor (kind: polish/reorg), not a bug: "Now" must describe today's checkbox toggle (click via
`Editor/Elements.cs:74`, Ctrl+Enter `Editor/PadView.cs:690`) and Tab/Shift+Tab indent (`Editor/PadView.cs:703,770`),
including undo grouping, caret placement and `Renumber`. The earlier plan is `.coord/tasks/A42-padview-commands-plan.md`
(step 3, A42c) and `.coord/tasks/A42-padview-commands-impact-r2.md`; it predates A42a/A42b/A43-A55 merges, so re-check every
line number and claim against today's tree (this is the delta audit the A42 rule asks for). Also say where the
`CheckboxGenerator.Box` regex owner should live and which LAYERS rows and selftests would prove behavior is unchanged.
Keep A42d-list-commands (Enter/ContinueList/Renumber) out of scope unless the split cannot be made without it.
