# Loop13-drop-chat-titles: remove the chat-title scheme

Item card (`docs/loop.md`). Budget: 4 KB.

Tier: Low · docs-tools
State: request
Branch: `chore/Loop13-drop-chat-titles` in `.claude/worktrees/chore/Loop13-drop-chat-titles` (created at Approve)

## Request
User, 2026-11-17 (via Master 6 and PStack Manager): "remove the chat title prefix thing" (called a flop). Confirmed with the number by the user in Loop Hub 4, 2026-11-17: "Both need to be done".
- **Goal:** the Loop12 title scheme is gone: `tools/chat-title.ps1`, `.coord/titles/`, every call to it and every doc line describing it.
- **Done check:** `git grep -il chat-title` finds nothing outside archived history (`.coord/logs`, `.coord/reports`, `.coord/research`, `.coord/status-archive.md`, `.coord/lessons.md` history); `tools/gate.ps1 -Offline` passes; no new "is gone" tests (the grep is the check).
- **Proof:** the grep output (empty) and gate output in the report; a list of files touched.
- **What we know:** Loop12 and its r2 are merged on dev (df5d174, 18ff078). Callers seen: `.claude/skills/merge-task/SKILL.md`, `.claude/skills/next-task/SKILL.md`, `tools/gate.ps1`, `tools/loop-card.ps1`, `.coord/tools/codex-run.sh`, `docs/loop.md`, `docs/loop-sessions.md`, `docs/wiki/Who-Does-What.md`, roadmap legend if present, hooks in `.claude/settings.json` if any. Restore the pre-Loop12 title text rules where they were replaced (`docs/loop.md:55` "[Codexing..]" style and loop-sessions R-rules) only if the user's chats still need them; otherwise state plainly that titles are free-form. Ask the user at Approve which.
- **Constraints:** Low tier, loop tools and docs only; no product code; drop the Loop12 pending/STATUS leftovers and `Loop12-chat-title-scheme-r3` idea; Codex may build, offline checks only. Another item (Loop14) edits `merge-task` and `codex-run.sh`; whichever merges second rebases.

## Findings (item chat, 2026-11-17)
Live users of the scheme (`git grep -il chat-title`, minus archive dirs): `tools/chat-title.ps1` (203 lines), `.gitignore:7` (`.coord/titles/`), `tools/gate.ps1:617` (blocked title on fail), `tools/loop-card.ps1:8` (working title per prompt), `.coord/tools/codex-run.sh:57-66` (`title` calls), `merge-task` SKILL line 60, `next-task` SKILL line 85, `docs/loop.md:57-59`, `docs/loop-sessions.md:43-44`, `docs/wiki/Who-Does-What.md:38,47,57,68`, roadmap legend `tools/roadmap/index.html:218`. Strings only: `product/AntiNotepad/SelfTest/App.cs:215-216` and `tools/playground/t17-badge-lab.html:63` use the slug `Loop12-chat-title-scheme` as sample branch text. Also `.coord/tasks/Loop12-*` cards (3 files) and `[Codexing..]` mentions in the L16 card/pending row (history, left). No hooks in `.claude/settings.json` mention titles. `.coord/titles/` is untracked runtime data.

## Plan
Remove: `tools/chat-title.ps1`, the `title` calls in `codex-run.sh`, the two script calls in `gate.ps1` and `loop-card.ps1` (keep the session-id read only if still used), the `.gitignore` line, the roadmap legend, and every title/`SET TITLE` sentence in the docs and the two skills. Delete the local `.coord/titles/` folder (untracked). Sample-slug strings: swap to `Loop12-sample-slug` in App.cs (string-only selftest edit; I propose this over exempting it) and the lab. Loop12 task cards: git-move to `.coord/tasks/archive` if that folder exists, else exempt them as card history.
Replaces the old text? Pre-Loop12 rules (`<ID> · review · <slug>` renames, `[Codexing..]`) are not restored: docs state "chat titles are free-form; the status card carries the phase". The "Your call" for Approve: that choice.
Dropped: Loop12 leftovers in STATUS "Last finished" stay (history); no `r3` idea row exists in pending.
Expected time: about 30 min build, 10 min check. Tier Low.
Files: ~11 tools/docs/skills files, 1 deleted script, 2 string-only edits.
Test plan: `git grep -il chat-title` empty outside archive dirs; `tools/gate.ps1 -Offline` PASS; no new selftests.
Risk: Loop14 edits `merge-task` and `codex-run.sh`; second merger rebases.

## Approve
User, 2026-11-17: "Approve"

## For Codex
Role: terra (a dozen small doc and script edits across files; reasoning needed, not xhigh). Do the Plan exactly. Do not commit. No item IDs in comments.
Docs say plainly: chat titles are free-form; the status card carries the phase (put this once in `docs/loop.md`, drop it from other docs). Keep `loop-card.ps1` working (read stdin only if still used). Delete `tools/chat-title.ps1` and the untracked `.coord/titles/` folder. Leave `.coord/logs`, `.coord/reports`, `.coord/research`, `.coord/status-archive.md`, `.coord/lessons.md`, `.coord/tasks/Loop12-*` and STATUS "Last finished" alone. Swap the sample slug `Loop12-chat-title-scheme` for `Loop12-sample-slug` in `product/AntiNotepad/SelfTest/App.cs` (both lines, recompute the expected badge text: `Loop12-3:04` and `Loop12 - sample-slug - 3:04 PM`) and `tools/playground/t17-badge-lab.html`. Another item edits `merge-task` SKILL and `codex-run.sh`; touch only the title lines.
Done means (offline only): `git grep -il chat-title` lists only files under the archive dirs above plus `.coord/tasks/Loop12-*` and `.coord/tasks/Loop13-*`; build into `build/Loop13-drop-chat-titles-check` and run `--selftest` there (all pass); `tools/gate.ps1 -Offline -Task Loop13-drop-chat-titles` passes; delete the build folder after.
Report `.coord/reports/Loop13-drop-chat-titles.md`: Changed (file list), Tests run (output tails), the grep output, Risks.

## Files
- `tools/chat-title.ps1`
- `tools/gate.ps1`
- `tools/loop-card.ps1`
- `.coord/tools/codex-run.sh`
- `.claude/skills/merge-task/SKILL.md`
- `.claude/skills/next-task/SKILL.md`
- `docs/loop.md`
- `docs/loop-sessions.md`
- `docs/wiki/Who-Does-What.md`
- `tools/roadmap/index.html`
- `.gitignore`
- `product/AntiNotepad/SelfTest/App.cs`
- `tools/playground/t17-badge-lab.html`
