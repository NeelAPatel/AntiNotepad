# STATUS

Updated: 2026-10-25
Product: AntiNotepad (`product/AntiNotepad`), version label 0.6.0 (untested).

## Working rules right now
- Codex is used up: implementation goes to Haiku subagents, one at a time, until the user says otherwise.
- Computer Use is off until the user's explicit go-ahead; live checks are done by the user by hand in the dev copy.
- Never run `AntiNotepad.exe --quit` without `ANTINOTEPAD_DATA`: it targets the installed copy's instance.

## Where things are
- `main`: tag `antinotepad-v0.6.0-untested-r5` (2026-10-25): A29-incremental-analysis (A1-8),
  A30-background-save-and-log (A1-14), A32-native-show-hide-animation (Windows' own open/close and minimize/restore
  animations; replaces A31), A33-taskbar-dead-code. User-tested: A29, A30, A32 ("good enough"). All audit findings fixed.
- Installed copy: r3 (through A27) until the user upgrades to r5 with `tools/install.ps1`.
- Finished briefs, reports and the A1 audit doc were removed from the tree on 2026-10-25; they live in git under tag
  `archive/coord-2026-10-25`. Locked choices stay in `docs/decisions.md`.

## Now
- A33-taskbar-dead-code (Haiku) merged into dev 2026-10-25 (no behavior change); in r5.
- A34-native-window-frame (Haiku, own worktree) in flight: A32 lost Windows 11's rounded corners and shadow. Fix that and
  make the pad a fully native window per the 2026-10-25 "Native window" decision (snap, maximize, snap layouts, Alt+Tab while
  open; closed stays invisible). Lab copy for the user before merging.
  Returned (0 warnings, SELFTEST PASS): explicit DWM rounded corners + NC rendering, 1 px DWM frame margin for the shadow on
  the solid background only (acrylic/mica unverified), maximized state kept across hide/minimize, maximized/snapped sizes not
  remembered. Snap-layout hover skipped (top-right is the Hide button); Win+Z kept. Risk: a finished timer reopening a
  maximized pad may take focus. Running as a lab dev copy from `.claude/worktrees/agent-afd62cb5c203eaf28` (branch
  `worktree-agent-afd62cb5c203eaf28`); not merged. 11:46: no installed copy was running (user likely mid-upgrade).

## Open (needs the user)
- Upgrade the installed copy to r5 (`tools/install.ps1` from Windows Terminal).
- Live checks not yet done: A30 Windows restart/sign-out saving an edit typed just before; A32 Alt+Tab presence in Never
  mode and any transient taskbar button.
- A32 issues: parked by the user ("good enough but there are issues"; list not given).
- N3-release-and-update-plan: proposal at tag `proposal/N3-release-plan`; discussed in a separate chat ("Production readiness
  for antinotepad"): publishing, GitHub repo, license, signing, version label, installer.

## Backlog (someday)
- Settings window runs about 90 threads (memory is stable, freed 5 of 5 cycles).

## Keep
- Backups made from Claude shells live only in `%LOCALAPPDATA%\Packages\Claude_pzs8sxrjxfjjc\LocalCache\Local\`:
  `AntiNotepad-backup-20261024-155646` and `Antipad-backup-2026-10-23`. Don't delete.
- Real backups from upgrades: `%LOCALAPPDATA%\AntiNotepad-backup-20261024-215228`, `Programs\AntiNotepad.previous-*`.
