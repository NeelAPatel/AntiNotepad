# STATUS

Updated: 2026-10-23
Product: AntiNotepad (`product/AntiNotepad`).

## Now
- C1-remove-archive merged into dev (2026-10-23): `archive/` deleted; old AntiQuickPad and spike notes live in git history
  and the tags `v0.1`, `antipad-v*`, `baseline/m8c-v1-compact`. Next: C2 stale-branch cleanup (awaiting user's list check).
- N1-antinotepad-identity and N2-canonical-install-and-dev-badge merged into dev (2026-10-23). AntiNotepad 0.5.0 installed at
  `%LOCALAPPDATA%\Programs\AntiNotepad` via tools/install.ps1; notes migrated to `%LOCALAPPDATA%\AntiNotepad` (backup
  `%LOCALAPPDATA%\Antipad-backup-2026-10-23`); old Antipad builds removed.
- N3-release-and-update-plan: proposal on `feat/N1-antinotepad-identity` (docs/release-plan.md), not merged. On hold (user): publishing,
  GitHub repo, license, signing, portable data defaults, Inno Setup terms.

## Open
- Settings window retention and private memory after closing remain unresolved from earlier work.
- Optional: choose final bar styles and scroll/swipe modes.
