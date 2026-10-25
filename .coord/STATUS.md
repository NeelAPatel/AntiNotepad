# STATUS

Updated: 2026-10-25
Product: AntiNotepad (`product/AntiNotepad`), version label 0.6.0 (untested).

## Working rules right now
- Codex is back (user, 2026-10-25): director Sol medium delegating to Luna xhigh; Claude briefs, reviews, commits.
- Computer Use is off until the user's explicit go-ahead; live checks are done by the user by hand in the dev copy.
- Never run `AntiNotepad.exe --quit` without `ANTINOTEPAD_DATA`: it targets the installed copy's instance.

## Where things are
- `dev` reverted 2026-10-25 to just after A30 (user): A31, A32 and A33 reverted, A34 dropped. Product code = r4 +
  A29-incremental-analysis + A30-background-save-and-log; windowing identical to r4 (Windows corners/shadow, bars, app's own
  show fade, A22-A25 taskbar stand-in). Reason: A32/A34 forced caption-window styles onto the frameless pad, losing corners
  and shadow and breaking Docked/Overlay/Float. 0 warnings, SELFTEST PASS.
- `main`: tag `antinotepad-v0.6.0-untested-r6` (2026-10-25), the user-approved good version = reverted dev (r4 windowing
  + A29 + A30). Installed copy: r5 (broken frame) until the user reinstalls r6 with `tools/install.ps1`.
- Finished briefs, reports and the A1 audit doc were removed from the tree on 2026-10-25; they live in git under tag
  `archive/coord-2026-10-25`. Locked choices stay in `docs/decisions.md`.

## Now
- A35-bar-insets merged into dev (fad7d61, user-approved live): design tokens + insets, ink-centered glyphs, scrollable
  top gap, top-bar blur behind; one commit per feature. Lab history kept on branch `backup/A35-bar-insets-lab`.
  A35 branches and worktree deleted (user). Installed copy not upgraded.
- A36-drop-pill-lab: dispatched to Codex Luna on `feat/A36-drop-pill-lab` (worktree `.claude/worktrees/A36-drop-pill-lab`):
  remove the dev-only Lab row and pill blur code; Solid pill ships (user).
- Native-window work (A32/A34) parked; any retry must not change the window's frame styles and
  goes lab-first. A34 (lab) was dropped: thick native border, bars broken in all three styles.

## Open (needs the user)
- Reinstall the installed copy with r6 (`tools/install.ps1` from Windows Terminal) to get the r4 window back.
- Live check not yet done: A30 Windows restart/sign-out saving an edit typed just before.
- N3-release-and-update-plan: proposal at tag `proposal/N3-release-plan`; discussed in a separate chat ("Production readiness
  for antinotepad"): publishing, GitHub repo, license, signing, version label, installer.

## Backlog (someday)
- Settings window runs about 90 threads (memory is stable, freed 5 of 5 cycles).

## Keep
- Backups made from Claude shells live only in `%LOCALAPPDATA%\Packages\Claude_pzs8sxrjxfjjc\LocalCache\Local\`:
  `AntiNotepad-backup-20261024-155646` and `Antipad-backup-2026-10-23`. Don't delete.
- Real backups from upgrades: `%LOCALAPPDATA%\AntiNotepad-backup-20261024-215228`, `Programs\AntiNotepad.previous-*`.
