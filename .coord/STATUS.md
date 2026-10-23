# STATUS

Updated: 2026-10-23
Product: AntiNotepad (`product/AntiNotepad`).

## Now
- A1-codebase-audit done (2026-10-23) on `feat/A1-codebase-audit`, not merged (waits for the user): `docs/audit/A1-codebase-audit.md`,
  27 findings (6 High, 13 Medium, 8 Low). No fixes made. Next: the user picks the findings that become follow-up tasks.
- C1-remove-archive merged into dev (2026-10-23): `archive/` deleted; old AntiQuickPad and spike notes live in git history
  and the tags `v0.1`, `antipad-v*`, `baseline/m8c-v1-compact`.
- C2-stale-branch-cleanup merged into dev (2026-10-23): only `main` and `dev` remain. Icon Lab (`claude/icon-lab`) and the
  old UI auditor were deleted untagged (labs get remade if needed).
- N1-antinotepad-identity and N2-canonical-install-and-dev-badge merged into dev (2026-10-23). AntiNotepad 0.5.0 installed at
  `%LOCALAPPDATA%\Programs\AntiNotepad` via tools/install.ps1; notes migrated to `%LOCALAPPDATA%\AntiNotepad` (backup
  `%LOCALAPPDATA%\Antipad-backup-2026-10-23`); old Antipad builds removed.
- N3-release-and-update-plan: proposal kept as tag `proposal/N3-release-plan` (docs/release-plan.md), not merged. On hold (user): publishing,
  GitHub repo, license, signing, portable data defaults, Inno Setup terms.

## Open
- Settings window retention: A1 measured `freed=True` in 5 of 5 plain cycles, with private memory flat at about 157 MB (a one-time
  native cost of about 35 MB). `freed=False` likely needs a trigger: A1-3-preview-timer-retains-settings or
  A1-13-progress-swipe-close-retention.
- Optional: choose final bar styles and scroll/swipe modes.
