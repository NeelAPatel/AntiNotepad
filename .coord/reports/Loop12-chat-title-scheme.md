# Loop12-chat-title-scheme report

## 1 Summary
Implemented the script-computed title fallback. Desktop renames still require the chat or director to call the app tool.

## 2 Change list
- `tools/chat-title.ps1:1` | title formatting, card state, store, offline check | Script
- `tools/loop-card.ps1:5` | working signal on prompt | User sends a message
- `.coord/tools/codex-run.sh:57` | waiting/Codex and exit signals | Codex launch / exit
- `tools/gate.ps1:615` | blocked signal on failure | Gate fails
- `.claude/skills/merge-task/SKILL.md:60` | director retitles after cleanup | Merge cleanup
- `docs/loop.md:55`, `docs/loop-sessions.md:43`, `.claude/skills/next-task/SKILL.md:85`, `docs/wiki/Who-Does-What.md:38` | title rules | Docs
- `tools/roadmap/index.html:218` | collapsible legend | Roadmap legend
- `.gitignore:7` | ignore title store | Script

## 3 Behavior changes
Hooks and scripts print changed titles; the chat performs the rename. The store records the emitted title, not confirmation that the app accepted it. The cold badge has no sweeper.

## 4 Out-of-scope files
None. This requested report is an output artifact. The spike and card were not edited.

## 5 Plan deviations
Gate was not run: its selftest launches AntiNotepad.exe, explicitly forbidden by the brief. Bash syntax validation failed to start because Git Bash returned CreateFileMapping access denied. The main checkout title store is outside this sandbox's writable root, so live caller/store integration could not be tested here. No live chat or roadmap test was attempted.

## 6 Contracts
No product contracts changed. Caller failure handling is implemented as best effort; actual exit behavior remains to be checked in a normal environment.

## 7 Lens review
Skipped: not a director run, as instructed.

## 8 Checks run
- `pwsh -NoProfile -File tools/chat-title.ps1 -Check`: PASS.
- `pwsh -NoProfile -File tools/loop-lint.ps1`: PASS; budget warnings for loop.md, lessons, and the item card.
- `git diff --check`: PASS.
- `bash -n .coord/tools/codex-run.sh`: could not start (CreateFileMapping access denied).
- Gate: not run for the reason above.

## 9 Hand checks for the user
In an item chat, verify working, needs-you, waiting with Codex badge, successful exit, blocked failure, and parked transitions; verify repeated identical events produce no new line. After merge cleanup verify the director applies `[Merged]`. Check the roadmap legend visually. Run the gate in an authorized environment after resolving the no-launch constraint.
