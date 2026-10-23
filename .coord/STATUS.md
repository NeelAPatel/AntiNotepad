# STATUS

Updated: 2026-10-23
Product: AntiNotepad (`product/AntiNotepad`).

## Now
- A6-audit-decisions in progress (2026-10-23) on `feat/A6-audit-decisions` (off `feat/A5-audit-refresh`): it applies the user's
  8 audit decisions. A1-27 is done here (atlas untracked). Four luna xhigh workers run in `../AntiQuickPad-wt/`:
  A7-notes-folder-and-recovery, A8-save-failure-quit-and-x64, A9-input-limits-and-timers, A10-ocr-repeat-and-target.
  The manager reviews each and merges it into A6; the new UI is shown in the dev copy before anything reaches dev.
- A5-audit-refresh done (2026-10-23) on `feat/A5-audit-refresh`, not merged (waits for the user). It re-audits the tree and
  rewrites `docs/audit/A1-codebase-audit.md` as a plain-language document. 25 findings open (7 High, 14 Medium, 4 Low), 7 fixed,
  5 new (A5-1 to A5-5). The doc lists 8 decisions with recommendations. Next: the user answers them.
- A2-dead-code-tidy, A3-runtime-tidy, A4-tooling-tidy merged into `feat/A1-codebase-audit` (2026-10-23) after manager review.
  They cover the 7 safe A1 fixes (A1-18, 21, 22, 23, 24, 25, 26). The merged branch builds with 0 warnings and passes SELFTEST.
  Its worktrees are removed. Dev-copy check passed: the caret stays solid with blinking off (6/6 captures; the blinking-on
  control was dark in 2/6), and the user confirmed the tray slot menu (a slot titled "empty" is enabled).
  Merged into dev with the audit.
- A1-codebase-audit merged into dev (2026-10-23): `docs/audit/A1-codebase-audit.md`,
  27 findings (6 High, 13 Medium, 8 Low). The 7 safe fixes are done; the user picks which of the other 20 become follow-up tasks.
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
