# STATUS

Updated: 2026-10-24
Product: AntiNotepad (`product/AntiNotepad`).

## Now
- A14 to A21 merged into dev (2026-10-24, HEAD after A17). Dev builds with 0 warnings and passes SELFTEST. Manager reviewed
  each; no live UI check was possible this round (no Computer Use), so every item below still needs one.
  - Audit batch 1: A14-swipe-close-cancel (A1-13), A15-startup-quit-pending (A1-15), A16-staged-upgrade-rollback (A1-17),
    A17-tool-script-checks (A5-4, A5-5).
  - Audit batch 2 (Sol director + luna): A18-search-speed. A1-11 fixed (background, debounced note search). A1-12 only
    partly: the manager dropped Sol's duplicated Settings row catalog and used a 150 ms debounce; a proper fix is open.
  - A19-demo-slot-notes: demo notes in slots 1-5, first run opens the welcome note (slot 5); fresh-install feel defaults are
    Rate-matched scrolling and Progress switching.
  - A20-taskbar-icon-setting (Sol): Settings > System > Taskbar icon: While visible / Always / Never (default).
  - A21-dev-identity (Sol): dev builds are "AntiNotepad (dev)" with a blue icon; installed builds unchanged.
  - Needs Computer Use: swipe/scroll feel, demo notes on screen, Ctrl+F and Settings search typing, all three taskbar modes,
    dev vs prod icons/names in the taskbar and Task Manager, and the new installer's first real run.
- Version 0.6.0 "untested" (V06-untested-version): dev merged into `main`, tag `antinotepad-v0.6.0-untested` (2026-10-24).
  - Dev copy relaunched on fresh data (0.6.0-untested, "AntiNotepad (dev)", blue icon); demo notes seeded, opens slot 5.
    The previous dev copy stopped unexplained when prod was quit (no crash or exit logged); watch for it.
  - Prod upgraded to 0.6.0-untested by the user (build/prod-upgrade.ps1 from Windows Terminal): old data in
    `%LOCALAPPDATA%\AntiNotepad-backup-20261024-215228`, old install in `Programs\AntiNotepad.previous-*`; fresh notes in
    `Documents\AntiNotepad` with the demo notes.
  - Live test (computer use, 2026-10-24): demo notes, scrolling, Ctrl+F, dev name/icon (Task Manager, tray, Settings button) pass.
    Taskbar setting fails: the real pad never gets a button (While visible = Never; Always only while hidden); the row sits inside
    the Open at expander. Dispatched A22-taskbar-button-fixes (Sol). An elevated Task Manager in front blocks computer-use input.
  - A22-taskbar-button-fixes merged and retested live: While visible, Always, Never, click-to-show, Settings row icon and the
    timing log pass. Remaining bug: clicking a focused pad's button doesn't hide it. A23-taskbar-click-hides merged into dev (builds, SELFTEST PASS); its live click check is pending (screen was locked).
  - Backups made from Claude sessions exist only in `%LOCALAPPDATA%\Packages\Claude_pzs8sxrjxfjjc\LocalCache\Local\`:
    `AntiNotepad-backup-20261024-155646` (stale Oct 22-23 data) and `Antipad-backup-2026-10-23`. Kept; don't delete.
- A6-audit-decisions merged into dev (2026-10-24), together with A5-audit-refresh. It applies the user's 8 audit decisions
  through A7-notes-folder-and-recovery, A8-save-failure-quit-and-x64, A9-input-limits-and-timers and
  A10-ocr-repeat-and-target, plus test feedback via A11-storage-settings-tab, A12-ocr-always-snip-and-lists and
  A13-timer-max-and-currency-error. The user approved it in the dev copy. Dev builds with 0 warnings and passes SELFTEST.
  See `docs/audit/A1-codebase-audit.md`.
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
  `%LOCALAPPDATA%\Programs\AntiNotepad` via tools/install.ps1; notes migrated to `%LOCALAPPDATA%\AntiNotepad` (backup made from a Claude shell, so it lives only in
  the Claude package store; see above); old Antipad builds removed.
- N3-release-and-update-plan: proposal kept as tag `proposal/N3-release-plan` (docs/release-plan.md), not merged. On hold (user): publishing,
  GitHub repo, license, signing, portable data defaults, Inno Setup terms.

## Open
- Settings window retention: A1 measured `freed=True` in 5 of 5 plain cycles (private memory flat at about 157 MB). A1-3's preview
  timer path is fixed in A9 and A1-13 in A14.
- Optional: choose final bar styles (scroll/swipe defaults chosen in A19).
