# A42d-list-commands-audit: findings for A42d-list-commands

Role: director (gpt-6.1-sol medium), read-only. Product `product/AntiNotepad`. Branch `feat/A42d-list-commands` is checked out:
never switch, merge, commit, or edit anything outside `.coord/reports/`. Never launch or kill AntiNotepad.
Read `AGENTS.md`, then the item card `.coord/tasks/A42d-list-commands.md` (its Request is what the user asked for).

## Answer these (this is the Findings phase of `docs/loop.md`)
1. **Kind and why**: bug, feature, polish or process. What about today's behavior justifies the request?
2. **Now**: what the app does today in the area, with evidence (file:line, a probe or selftest result). For anything
   visible, say in plain words what the user would see on dev today. If dev already does what the Request asks, say so first.
3. **Reproduction (bugs)**: reproduce it with a probe in `build/A42d-list-commands-audit` (deleted after) or an exact code path
   quoted step by step. If you cannot, say "not reproduced" and what you would need from the user. Do not guess a cause.
4. **Where**: the files that would change, their callers, and shared state, timers or caches the change can reach.
5. **Risk**: the riskiest part, and which gate in `docs/loop.md` ("Gates menu") would catch it.
6. **Tier**: confirm or raise the card's tier, with the reason.
7. **Recommendation**: the smallest change that meets the Request, the "After" state, the code tests and UI checks that
   would prove it, open scope questions for the user, and whether to split the item.

## Write: `.coord/reports/A42d-list-commands-audit.md`
Sections 1-7 above, short, each claim with its evidence. End with `READY: yes` or `READY: no - <what is missing>`.

## Background for this item
A behavior-neutral refactor (kind: polish/reorg), not a bug. A42c-checkbox-indent (merged 178d4c0) already moved the
checkbox toggle and Tab indent edits into `Editor/ListEditing.cs` (pure decisions) and `Editor/DocumentCommands.cs`
(applies one edit to a TextDocument line); follow that pattern. A42d covers what A42c left in `Editor/PadView.cs`:
`IsListLine` (:746), `Renumber` (:760), `ContinueList` (:788), `Insert` (:828, incl. BringCaretToView) and the Enter
routing (:675-680). "Now" must describe today's Enter on bullets, numbers and checkboxes (mid-line and end-of-line, checked
becomes unchecked, empty nested item outdents, empty root exits, 999,999,999 falls through), Tab renumber incl. 9 -> 10,
undo grouping (one Ctrl+Z per Enter/Tab), caret anchoring and the caret staying visible at the bottom of a long note.
Earlier plan: `.coord/tasks/A42-padview-commands-plan.md` (A42d) and `.coord/tasks/A42-padview-commands-impact-r2.md`;
it predates many merges, re-check every claim against today's tree, and read A42c's card and report for the pattern.
Hard boundary: the timer start on Enter in timer notes (`PadView.cs` right after ContinueList, `StartTimerOnLine`) keeps
today's behavior exactly; the user filed changing it as a separate idea (timer-enter-no-autostart). Say whether `Renumber`
should move (the old plan rated A42d highest risk: caret anchors, undo grouping, offsets after digit-width change), what
stays in PadView, and which selftests (real TextDocument, exact text, one Undo) prove nothing changed.
