# Level 4 shared brief (L1-L7, written 2026-11-08)

Read this first, then your own item brief. Source note: `.coord/research/loop-native-features.md` (ideas and unverified items).

## Goal
Level 4 = Claude Code's own config enforces the AntiNotepad dev loop (hooks, agents, skills, worktree rules, status line)
instead of reminders and long prose. It must stay lightweight, reversible and easy to explain: the user wants to build a
habit on it. This is also the user's showcase project for "knows how to use agentic AI", so keep results measurable and
write down what you verified versus what you assumed. Level 5 (automation pipeline, rows L8-L15) is out of scope.

## Items and merge order (one at a time, rebase then --no-ff onto dev)
L1-lv4-native-guards > L2-lv4-worktree-rules > L3-lv4-agent-roster > L4-lv4-skill-diet > L5-lv4-status-line >
L6-lv4-docs-diet > L7-lv4-playbook. Slugs are lowercase (the tools require it). Several items touch `.claude/settings.json`:
keep your edits small and expect a rebase.

## First reply (user decision, 2026-11-08)
The user will read your first message before anything is built, to discuss it with you and then give the green light.
So before writing the card in full, open with a short explainer, no code:
- A table: Now (today) | After (when built) | What changes for the dev loop | What changes for the user.
- Bullets, concise: what you will build, which files, what could go wrong, how to undo, what you will check and how.
- Say what you verified versus assumed so far. A diagram only when a table cannot show it; otherwise tables and bullets only.
- Plain words, gloss each tool name in 3-6 words, one decision per message, and wait for the user's approval before
  creating the worktree or running Codex. The user may refine the plan with you first.
Nothing is built before that approval; the worktree comes at Approve.

## The rest of the system (need to know)
All seven items are being planned in parallel by sibling sub-chats; you do not coordinate with them. In one line each:
L1 hooks that guard merges, the installed copy and loop files (logging first); L2 worktree naming and base-on-dev checks;
L3 builder, reviewer and auditor agent files; L4 user-only skills, forked review, live git facts; L5 status line plus a
smaller per-turn hook; L6 split loop.md, path-scoped coding rules, trim AGENTS.md; L7 the habit playbook, written last.
Later (Level 5, not now): loop state tool, headless JSON verdicts, CI on dev, scheduled routines, one config file, metrics
page, write-up. Design so that your item does not block those, but do not build them. Where your item touches a sibling's
files, say so in your plan.

## Your role
You are the sub-session for ONE item. Sonnet, medium effort. You brief and check Codex; Codex writes the files
(`docs/agents.md`: Terra medium for hooks, Luna medium for the rest; run Codex from YOUR OWN chat as a background run, no helper chat; use at most one or two Haiku helper sub-chats only when a run truly needs them (user, 2026-11-08: tokens are tight); Codex checks
run offline; never ask Codex for port, server or browser checks). You commit on your branch (Codex cannot write to .git).
The user talks to you directly about your item; do not ask the directors for decisions. Follow `docs/loop.md` for phases,
the item card and the status-card line on replies; follow `docs/coding.md` for any file you touch.

## Phases for you
1. Plan (start here, in the main checkout, read-only except your own item card `.coord/tasks/<ID-slug>.md` and notes):
   write the card, a Plan with gates, tier, files touched and Expected time (over an hour: say so and offer a smaller cut).
   Verify any "unverified" doc claim your item depends on against the live docs or a tiny experiment before relying on it.
   Ask the user to approve in your chat (one decision at a time).
2. Approve: fill the card's `## Approve`, then from the main checkout run
   `pwsh -NoProfile -File tools/approve.ps1 -Task <ID-slug> -Prefix chore`. It creates the worktree
   `.claude/worktrees/chore/<ID-slug>` on branch `chore/<ID-slug>` from `dev`. It refuses past 3 items already building and
   when planned files overlap another item's: then tell the user and wait; do not work around it.
3. Build in that worktree with Codex, then Check with `/review-task <ID-slug>`, then hand the user a hand test.
4. Relay ONCE: when approve.ps1 passes, send one message to Loop Hub 2 (`local_a7df8d32-dc40-4783-8085-16a06f0cc43a`) saying
   `<ID-slug> is now building; please set its STATUS row to building`. No other director messages. Loop Hub 2 owns STATUS,
   merges and the roadmap row; you never edit STATUS and never merge.
5. After the user's hand test passes, tell the user it is ready for Loop Hub 2 to run `/merge-task <ID-slug>`.

## Rules for every Level 4 item
- Hooks and permission rules start logging-only or ask-only unless the item says otherwise; undo must be "delete one line".
- Windows is the target: hook commands use `"shell": "powershell"`; test them there. No sandbox exists on native Windows.
- Edit only project `.claude/` files in your branch. Never edit user-global Claude settings, CLAUDE.md or memory.
- Never touch the user's installed copy (`%LOCALAPPDATA%\Programs\AntiNotepad`). Never run `AntiNotepad.exe --quit` without
  `ANTINOTEPAD_DATA`. No dev instance launch unless the loop's hand-test step says so.
- App code and the gate scripts stay unchanged unless your item says so. US spelling in any UI text.
- Keep every doc you write short and plain. The reply style is `.claude/output-styles` (plain words, one decision).
- Record unverified assumptions in your card and report; mark each verified or not.
- Ask the user (not Loop Hub 2) to have the "Agentic Levels 4+5" top-level chat rename you if a rename helps.
- **Cap of 3 (user, 2026-11-08):** the approve cap stays at 3. Never raise it yourself (no `-Cap` flag, no script edit).
  Only the user may allow a bypass, one instance at a time, in words to you; then use `-Cap` for that run only and log
  the override in `.coord/lessons.md` through Loop Hub 2's usual path. Overlap refusals are never bypassed.
- **Closed marker (user, 2026-11-08):** when your item is merged into dev and cleaned up, rename your chat to
  `[Closed] <ID> · merged · <slug>` (L1 already did) and say nothing else.
- **Codexing marker (user, 2026-11-08):** while a Codex run you launched is still going, your own chat title starts with
  `[Codexing..] ` (rename yourself with the session title tool, session_id `self`, keeping the rest of the title). Set it
  right when you launch Codex; remove it as soon as Codex exits and you have its result. Keep it even while the user
  talks to you during the run. If the rename is refused or needs approval, tell the user once and carry on.
