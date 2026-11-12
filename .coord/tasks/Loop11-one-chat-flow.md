# Loop11-one-chat-flow: one chat per item, you answer the chat doing the work

Item card (`docs/loop.md`). Loop rule change, asked for by the user (so the "same miss twice" bar is met by request).

Tier: Low · docs-tools
Branch: `chore/Loop11-one-chat-flow` in `.claude/worktrees/chore/Loop11-one-chat-flow` (created at Approve)

## Request
User, 2026-11-12: "still a bit more friction than i expected ... too many back forths to start on the build process ... sometimes master/subchat both have buttons they want me to click and i can never tell who has priority ... trust workspaces popups". Then: "I am thinking that master makes subchat -> findings -> tells master this is a low/medium/high task while continuing to work with me directly ... if it decides its medium it will let me know the path forward has changed and why?" Then: "draft and implement it".

## Findings
- Today the director owns Request, Findings, Plan, Approve and STATUS; a second chat builds. Each hop is a user stop and a director turn (director context is large, so relay turns are the costly ones).
- Docs (fetched 2026-11-12): on a subscription the main chat cache lasts 1 hour and cached reads cost 0.05x; idle past it re-prices the whole chat. Multi-agent costs about 4x to 15x a plain chat; fewer chats and hops are cheaper. In Medium/High, Codex does the reading, so the chat stays small and one chat can carry findings through build.
- Branches never edit STATUS (`merge-task/branches.md`), so item chats cannot show progress; the director relays milestones.
- `.claude/settings.json` keeps an `ask` rule on `mcp__ccd_session__start_session` as a soft guard for the one-level rule.

## Plan
Edit docs and skills only (no product code); keep `docs/loop.md` under 8 KB.
1. **One chat per item.** The director writes the card's Request and the pending/STATUS row, starts the item chat once, and steps back. The item chat does Findings, Plan, asks the user "Approve?" in that chat, runs `tools/approve.ps1`, builds, checks, runs the hand test and reports. The director merges.
2. **Tier is set by the item chat**, not the user: it sends the director one line (tier, reason) and keeps talking to the user. When unsure it picks the higher tier. If it raises the tier it tells the user at once: what changes, why, the extra cost. Medium/High: it runs the read-only Codex audit itself as a background run in the same chat; no new chat. A new chat only if the chat sat idle over about an hour with a large context, or the user says so.
3. **One asker.** The user answers the chat doing the work. The director asks the user only about merge, releases, rule changes and things outside one item. A chat shows its next-step buttons only for its own item decisions; the other side stays silent.
4. **Status rows.** The item chat edits only its own one-line row in the main checkout's `.coord/STATUS.md` by absolute path (the roadmap reads that file). It never touches other rows or sections. Director edits Now line and finished moves.
5. **Prompts.** Remove the `start_session` `ask` rule from `.claude/settings.json` (the text rule stays: a sub-chat never starts a chat). Check that `approve.ps1` works when run by the item chat; fix only if it does not.
6. Align every file that states the old ownership (grep for it): `docs/loop.md`, `docs/loop-sessions.md`, `docs/agents.md`, `docs/playbook.md`, `.claude/skills/{next-task,review-task,merge-task}`, `.coord/templates/findings.md`, `.claude/output-styles/antinotepad.md`, the STATUS rules. Bump Loop version to v5.4.0 (one place); STATUS loop line updated.
7. Gate and `tools/loop-lint.ps1` offline. Expected time: about 45 minutes. Not in scope: the trust-workspace popup (needs one screenshot of the popup first).

## Files
- `docs/loop.md`, `docs/loop-sessions.md`, `docs/agents.md`, `docs/playbook.md`
- `.claude/skills/next-task/phases.md`, `.claude/skills/review-task/phases.md`, `.claude/skills/merge-task/branches.md`
- `.coord/templates/findings.md`, `.claude/output-styles/antinotepad.md`, `.claude/settings.json`
- `.coord/tasks/Loop11-one-chat-flow.md`, `.coord/reports/Loop11-one-chat-flow.md`

## Contracts
None (no product code).

## Approve
User, 2026-11-12: "draft and implement it".
