# Loop19-review-task-chat-runnable: item chats can run their own review

Item card (`docs/loop.md`). Budget: 4 KB.

Tier: Low · loop-skills
State: approved
Branch: `chore/Loop19-review-task-chat-runnable` in `.claude/worktrees/chore/Loop19-review-task-chat-runnable` (created at Approve)

## Request
User, 2026-11-19 (via Loop Hub 4; number Loop19 confirmed by the user): "Open it as Loop19, validate that its still a bug, fix it, test it". Earlier user word: "Let chat run it".
- **Goal:** an item chat can run `/review-task` itself at Check, as `docs/loop.md` already says it does; a chat never offers a button for a skill only the user can type.
- **Done check:** (1) Findings first: confirm it is still a bug today (the skill's header has `disable-model-invocation: true` while `docs/loop.md` rows 6 and 87 say the item chat runs it); if it is not, drop. (2) After the fix, a throwaway read-only chat calls the Skill tool with `review-task` and it loads (transcript line "Launching skill: review-task"). (3) The new rule line exists and the typed-only skills (`/merge-task`, `/next-task`, `/scan-backlog`) still carry `disable-model-invocation: true`.
- **Proof:** the Findings evidence; the transcript line for (2); `git diff` header of the skills; gate output.
- **What we know:** Loop16 and Loop17 item chats could not run `/review-task` and ran the Codex review by hand. Pending idea row `idea: review-task-chat-runnable` in `.coord/pending.md`. Observed harness behavior: a skill with `disable-model-invocation: true` cannot be called by a chat.
- **Constraints:** Low tier. Remove the flag from `/review-task` only; it is read-only and never merges. Add one house-rule line (docs/loop.md or docs/loop-sessions.md, keep loop.md near its budget): a chat shows a typed-only command in a `text` block and waits, never a button. Check `.claude/skills/review-task/SKILL.md` allowed-tools still make sense for chat use. Codex may build, offline only; the live check (2) needs a throwaway read-only chat the user OKs at Approve. Stop at Approve.

## Findings
- **Kind and why:** bug (process); the loop's own docs promise a chat step the harness blocks, so every item chat falls back to a by-hand review.
- **Now:** confirmed 2026-11-19. `.claude/skills/review-task/SKILL.md` line 4 has `disable-model-invocation: true` (added by L4, commit 4c99023); `docs/loop.md` rows 6 and 87 say the item chat runs `/review-task`; `docs/playbook.md` line 43 says "the item chat (you may)" but line 32 lists it typed-only. The Skill tool listing in this very chat shows `roadmap` but none of `review-task`, `next-task`, `merge-task`, `scan-backlog`, so a chat cannot call it (Loop16 and Loop17 hit this).
- **Where / Risk:** skill header only. Body is read-only on product code and never merges. Risk: `allowed-tools` pre-approves only git; gate and Codex calls use the chat's normal permissions, as by hand today. `docs/loop.md` is already over its 8 KB budget, so the rule goes elsewhere.
- **Source:** own scout.

## Plan
- **After:** (1) drop `disable-model-invocation: true` from `review-task` only; `merge-task`, `next-task`, `scan-backlog` keep it. (2) `docs/loop-sessions.md`: one rule line, a chat shows a typed-only command in a `text` block and waits, never a button. (3) `docs/playbook.md` line 32: remove `/review-task` from the typed-only list and note the item chat runs it.
- **Expected time:** under an hour.
- **Must not change:** the other three skills' flags; `/review-task` body; no product code.
- **Scope questions:** None.
- **Test plan:** None (not visible). Checks: `git diff` shows only the flag line gone from `review-task` and still present in the other three; `tools/loop-lint.ps1` clean; live check below.
- **Gates:** Low tier defaults: `tools/gate.ps1` (docs-only, no selftest rows).
- **Needs from you:** OK to start a throwaway read-only chat after the edit: it calls the Skill tool with `review-task` on a dummy ID and must show "Launching skill: review-task" (then stops at the missing card). It edits nothing; archived after.

## Files
- `.claude/skills/review-task/SKILL.md`
- `docs/loop-sessions.md`
- `docs/playbook.md`

## Contracts
None (loop docs and skill header; no `CONTRACTS` rows).

## Approve
User, 2026-11-19: "Approved" (Plan as written, including the throwaway read-only chat live check).
