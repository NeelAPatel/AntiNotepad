# STATUS

Updated: 2026-10-24
Product: AntiNotepad (`product/AntiNotepad`).

## Now
- Computer Use is off until the user's explicit go-ahead (2026-10-25); live checks are done by the user by hand.
- Dev merged into main, tag antinotepad-v0.6.0-untested-r4 (2026-10-25, A28). Installed copy still on r3 until the user
  upgrades.
- Dispatched 2026-10-25, each a Sol director (gpt-6.1-sol medium) with luna xhigh subagents in its own worktree under
  `../AntiQuickPad-wt/`, branch `feat/<ID-slug>`. Manager reviews, sends fixes back until acceptable, then merges:
  - A29-incremental-analysis (A1-8): re-analyse only the edited region, identical to a full rebuild.
  - A30-background-save-and-log (A1-14): saves and log writes on a background writer; shutdown/quit flush, A8 failure prompt.
  - A31-no-windows-show-animation (user request): Windows' own open/close animation never plays on pad show/hide; the app's
    own fade and drop-down slide stay.
  A30 and A31 both touch `UI/PadWindow.cs`; the manager resolves overlap at merge.
  - A31 returned and passed review (DWMWA_TRANSITIONS_FORCEDISABLED on the pad and the taskbar stand-in at creation; 3 small
    edits, no selftest since it isn't checkable headless). Merged into dev (0 warnings, SELFTEST PASS); dev copy relaunched.
    Waiting on the user's live test.
  - A29 returned and passed review (line-text cache per document line, structure reused when changed lines keep their
    signature, full rebuild otherwise; math and timer keys stay global; 19,000 randomized edits matched a full rebuild;
    1500-line keystroke 0.51 -> 0.08 ms, 5000-line 1.35 -> 0.27 ms). Merged into dev (0 warnings, SELFTEST PASS); dev copy
    relaunched with A29 + A31. Waiting on the user's live test.
  - A30 returned: background writer and log queue accepted, 0 warnings, SELFTEST PASS. Rejected for two regressions: an
    unavailable notes folder became a save failure (toast on every hide, Quit and `--quit` blocked, so installs would time
    out), and a failed save made Flush block Load/RemoveOrphans/SwitchFolder (no way out via Settings > Storage).
    A30-save-unavailable-fix (Sol + luna) fixed both. Review found one more: a save with the folder briefly missing set
    IsAvailable=false for good (OneDrive/drive returning would never save again). Manager removed that line;
    A30-folder-returns-fix (luna) updated the selftest and added a folder-returns case. Merged into dev (2026-10-25; SelfTest
    conflict with A29 resolved by keeping both methods; merged tree 0 warnings, SELFTEST PASS). Dev copy relaunched with
    A29 + A30 + A31. Waiting on the user's live test of all three.
  - User test (2026-10-25): A29 and A30 pass; closed, A1-8 and A1-14 marked fixed, worktrees and branches removed.
    A31 is the opposite of what the user wanted (manager misread the request): they want Windows' native open animation on
    show and the native close/minimize animation on hide, not the app's own fade and not none. A31's worktree is removed;
    its change stays on dev until A32 reverts it.
  - A32-native-show-hide-animation dispatched (Sol + luna, worktree `../AntiQuickPad-wt/A32-native-show-hide-animation`):
    revert A31, make Windows animate the pad's show and hide natively, drop the app's own show fade. Relaunched on the
    user's route spec (open/close: start, tray, hotkey; minimize/restore: minimize, taskbar).
  - A32 returned (0 warnings, SELFTEST PASS): the pad keeps caption/sysmenu/min/max style bits (no visible title bar via
    Avalonia's extended client area), is a real unowned app window whose taskbar tab is controlled with ITaskbarList, and the
    stand-in is used only for closed + Always. Big redesign of A22-A25, so it is NOT merged: running as a lab dev copy from
    the A32 worktree, waiting on the user's approval. Risks to check: title bar/frame look, transient taskbar button, Alt+Tab
    presence in Never mode.
  - Incident 2026-10-25: while swapping dev copies the manager ran `build/dev/AntiNotepad.exe --quit` without
    ANTINOTEPAD_DATA, which targets the installed copy's instance. Prod kept running (still 00:33 start). Rule recorded.
- A28-editor-render-memory (A1-7, A1-9, A1-10; Sol director + 3 luna workers, worktree `../AntiQuickPad-wt/A28-editor-render-memory`)
  returned 2026-10-25: 0 warnings, SELFTEST PASS. Review accepted the render caches; one bug found: in notes over 1500 lines the
  fence color cache can keep a stale code-block body after an edit (hit by document version alone). A28-fence-cache-bounds-fix
  (luna xhigh) fixed it (hits also require the same body bounds). Merged into dev (2026-10-25, 0 warnings, SELFTEST PASS).
  Done: the user tested it live in the dev copy and it works; A1-7, A1-9, A1-10 marked fixed; worktree and branch removed.
  Not yet in main or the installed copy.
- A27-settings-search-build-matches done (2026-10-24): the proper A1-12 fix. Settings search builds only matching rows; rows
  create controls lazily, so the page methods stay the single definition (no duplicated catalog). Codex luna xhigh in its own
  worktree; its selftest harness was rejected and removed by A27-settings-search-rework. Merged into dev (0 warnings, SELFTEST
  PASS); the user tested it live in the dev copy and it works. Worktree and branch removed. Codex timing: one search about
  14 ms -> 1.6 ms. Dev merged into main, tag antinotepad-v0.6.0-untested-r3 (2026-10-25). Prod reinstalled by the user
  (00:33) with r3 and running.
- A14 to A21 merged into dev (2026-10-24, HEAD after A17). Dev builds with 0 warnings and passes SELFTEST. Manager reviewed
  each; no live UI check was possible this round (no Computer Use), so every item below still needs one.
  - Audit batch 1: A14-swipe-close-cancel (A1-13), A15-startup-quit-pending (A1-15), A16-staged-upgrade-rollback (A1-17),
    A17-tool-script-checks (A5-4, A5-5).
  - Audit batch 2 (Sol director + luna): A18-search-speed. A1-11 fixed (background, debounced note search). A1-12 only
    partly: the manager dropped Sol's duplicated Settings row catalog and used a 150 ms debounce; fixed properly in A27.
  - A19-demo-slot-notes: demo notes in slots 1-5, first run opens the welcome note (slot 5); fresh-install feel defaults are
    Rate-matched scrolling and Progress switching.
  - A20-taskbar-icon-setting (Sol): Settings > System > Taskbar icon: While visible / Always / Never (default).
  - A21-dev-identity (Sol): dev builds are "AntiNotepad (dev)" with a blue icon; installed builds unchanged.
  - Needs Computer Use: swipe/scroll feel, demo notes on screen, Ctrl+F and Settings search typing, all three taskbar modes,
    dev vs prod icons/names in the taskbar and Task Manager, and the new installer's first real run.
- Version 0.6.0 "untested" (V06-untested-version): dev merged into `main`, tag `antinotepad-v0.6.0-untested` (2026-10-24).
  - Dev copy relaunched on fresh data (0.6.0-untested, "AntiNotepad (dev)", blue icon); demo notes seeded, opens slot 5.  - Prod upgraded to 0.6.0-untested by the user (build/prod-upgrade.ps1 from Windows Terminal): old data in
    `%LOCALAPPDATA%\AntiNotepad-backup-20261024-215228`, old install in `Programs\AntiNotepad.previous-*`; fresh notes in
    `Documents\AntiNotepad` with the demo notes.
  - Live test (computer use, 2026-10-24): demo notes, scrolling, Ctrl+F, dev name/icon (Task Manager, tray, Settings button) pass.
    Taskbar setting fails: the real pad never gets a button (While visible = Never; Always only while hidden); the row sits inside
    the Open at expander. Dispatched A22-taskbar-button-fixes (Sol). An elevated Task Manager in front blocks computer-use input.
  - A22-taskbar-button-fixes merged and retested live: While visible, Always, Never, click-to-show, Settings row icon and the
    timing log pass. Remaining bug: clicking a focused pad's button doesn't hide it. A23 failed live; A24-taskbar-click-diagnose added a dev-only `taskbar:` trace, which showed Windows 11 deactivates the pad
    with no counterpart on a taskbar click. A25-taskbar-click-foreground-loss fixed it: live test passes (focused click hides,
    hidden click shows, inactive click focuses). Prod reinstalled by the user (23:07, has the A22-A25 fixes).
  - At 23:07:44 the user started a restart/sign-out and both copies vetoed it (event 10001), then exited: prod and dev are not
    running. Cause: the pad's and stand-in's Closing handlers cancel closes. A26-allow-windows-shutdown merged and verified on dev: a dev-only simulated
    WM_QUERYENDSESSION is accepted (reply=1), saves in 1.7 ms including an edit typed just before, and exits; Alt+F4 still hides.
    Prod reinstalled by the user (23:26) with A26 and running. Dev merged into main, tag antinotepad-v0.6.0-untested-r2.
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
- Audit findings: none open (A1-8 and A1-14 fixed in A29 and A30, 2026-10-25).
- N3 release plan: being discussed in a separate chat ("Production readiness for antinotepad").

## Backlog (someday, not in the current release plan)
- Settings window runs about 90 threads (A1 evidence); memory is stable (freed 5 of 5 cycles). A big-ticket look someday.

## Closed by the user (2026-10-25)
- Final bar styles: non-issue. The dev copy's unexplained stop when prod quit: dropped.
