# C27-session-naming: master and sub-session names, loop version, master handover

Item card (`docs/loop.md`). Done by Claude directly (process docs only).

Tier: Low
Branch: `feat/C27-session-naming` in `.claude/worktrees/C27-session-naming`

## Request
User, 2026-10-29: "cleanly shut down [AntiNotepad task execution plan], we'll make a new master chat to pick up where it
left off. It will still use the same artifact/local html roadmap file from that chat. It should know about the latest
changes made to the dev loop and always adhere to it for how it decides to create claude subagents and codex subagents.
We're aiming for well scoped agents orchestrated by the claude master chat. The master chats from now on will always
have a Master <increasing ID number> - <dev loop version> - slug for overall remaining work for its chat name. Subchat
names will be the same <ID slug>-<devloop step>-<task name slug> i.e scan/audit, build, review, test, etc as it needs."

## Findings
- **Kind and why:** process. No loop version, no naming rule for chats, and no handover procedure existed; sub-session
  model choice lived in one Rules bullet and the user's memory.
- **Now:** sub-sessions were titled freely ("A54-link-hover-click implementation", "A53-split-divider-height: implement
  and gate"); the master was "AntiNotepad task execution plan".
- **Where:** `docs/loop.md` (new "Sessions" section), `/next-task` start_session titles.

## Plan
- **After:** `docs/loop.md` "Sessions": loop version v2, one orchestrating master named `Master <N> - loop v<V> - <slug>`,
  one sub-session per step named `<ID>-<step>-<slug>` with its model (scan, audit Haiku; build, review, test Sonnet
  medium), build sessions rename themselves per step, same roadmap page, handover checklist and resume prompt.
- **Must not change:** phases, gates, branch and STATUS rules.
- **Test plan, code:** gate passes. **UI:** none.
- **Gates:** gate.
- **Needs from you:** nothing beyond the request.

## Files
- `docs/loop.md`
- `.claude/skills/next-task/SKILL.md`
- `.coord/tasks/C27-session-naming.md`

## Contracts
None.

## Go
User, 2026-10-29, the request above.

## Check
- Gate: `GATE: PASS` (build 0 warnings, `SELFTEST PASS`, seam 0, scope PASS).
- Bounds: within plan. The duplicate sub-session model bullet in Rules now points to "Sessions".

## Hand test
None (not visible).
