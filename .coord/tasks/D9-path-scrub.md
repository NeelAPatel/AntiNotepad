# D9-path-scrub (Codex build in this worktree; do not commit, you cannot write .git anyway)

Role: quick, mechanical text scrub with grep, one reason: replace personal strings in tracked text files.
Context: the repo goes public (branches main and dev). Owner decisions: keep everything, but scrub personal/local strings; remove `.coord/research` from the tip EXCEPT the folder `.coord/research/public-prep/` (leave that alone entirely, it is owner prep notes).

## Do
1. Find tracked text files (use `git ls-files`) containing: the owner's Windows home path (`C:\Users\Neel`), the owner's repo path (`F:\LocalVSProj`), `AppData\Local\Claude-Data`, Claude session UUIDs next to "session" or in `.claude/` and `.coord/tasks` text, `claude.ai/artifact`, `claude.ai/code`, absolute worktree or build paths from this machine, PIDs quoted as evidence, the owner's email. Skip `third_party/`, `product/` binaries, images, and `.coord/research/public-prep/`.
2. Replace with neutral text that keeps the meaning, e.g. `%USERPROFILE%`, `<repo root>`, `<worktree>`, `<session id>`, `(artifact link removed)`. Do not rewrite sentences beyond the replacement. Known hits: `.claude/skills/next-task/SKILL.md:42`; `.coord/tasks/Loop16-poteto-trigger.md:55`; `.coord/tasks/Loop17-chat-title-selfheal.md:10`; `.coord/reports/A21-save-delay-setting-r5.md:58`; `.coord/reports/A4f-paint-tokens.gate.md:16-20`; `.coord/reports/Loop2-loop-v3-1-sandbox-smoke.worker.codex.md:2`; `.coord/status-archive.md:40,112`; `.coord/tasks/Loop2-loop-v3-1.md:139`; `.coord/tasks/T1-roadmap-overview.md:47`; `.coord/tasks/T1-roadmap-overview.sample.json` (private repo name, lines 661 and 1060); `.coord/research/loop12-title-spike.md`, `.coord/tasks/Loop12-chat-title-scheme-r2.md`, `.coord/research/chat-title-scheme.md` (session ids). Find more with grep; the list is not complete.
3. `tools/roadmap.url` is only an artifact link: delete the file and remove references to it (grep for `roadmap.url`).
4. `AGENTS.md` line 6: change "Private GitHub NeelAPatel/AntiNotepad (origin); manager pushes after commits." to "GitHub NeelAPatel/AntiNotepad (origin); manager pushes after commits." Keep everything else in AGENTS.md. Do not touch README.md, CONTRIBUTING.md, LICENSE, THIRD-PARTY-NOTICES.md, docs/building-with-ai.md (another run writes them).
5. Delete the tracked folder `.coord/research/` EXCEPT `.coord/research/public-prep/`. Before deleting, grep tracked files outside it for links or references to `.coord/research/` and list them (do not rewrite them; report them).
6. Do not change product code. Do not run builds or the app.

## Done means (offline, Codex can run)
- A `git grep` for the home path, the repo path, `claude.ai/artifact`, `claude.ai/code/artifact` and `Claude-Data` outside `.coord/research/public-prep/` returns nothing.
- Final message lists: files changed (count and names), replacements made, the roadmap.url references found, the list of links into `.coord/research/` that now dangle, anything you were unsure about.
