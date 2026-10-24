# A16-staged-upgrade-rollback

Implemented the staged upgrade rollback for the AntiNotepad installer.

## Files changed

- `tools/install.ps1`
- `tools/uninstall.ps1`
- `docs/dev-loop.md`

## Finding and fix

`tools/install.ps1` previously copied staged files over the installed directory and left a partial install in place if copying or the installed selftest failed. It now moves an existing install to a timestamped sibling backup before copying. Failures after that point remove the partial new files, restore the backup, and relaunch the previous app only when its installed process was running before the upgrade. A failed first install removes its partial directory. Successful upgrades remove older matching backups and keep only the latest previous version. The installer restores the prior Run key value and Start menu shortcut state if later setup fails.

The success output now reads `NotesFolder` from `%LOCALAPPDATA%\AntiNotepad\location.json` and falls back to app home if the pointer cannot be read. `tools/uninstall.ps1` removes timestamped previous-install backups and refuses to delete one if the recorded notes folder is inside it or the pointer is unreadable.

The test-only `-InstallRoot` mode accepts only a direct `build/A16-staged-upgrade-rollback-<suffix>` child. It skips installed-process checks and `--quit`, all Start menu shortcut and Run key access or changes, and app launch. `-TestFailSelfTest` injects a failure after copy and before invoking the published executable's selftest.

## Checks

- PowerShell `Parser.ParseFile` passed for `tools/install.ps1` and `tools/uninstall.ps1`.
- `tools/install.ps1 -DryRun` printed the plan without publishing or accessing processes or the registry.
- The test-only run published Release into a temporary staging directory under `build/`, copied to `build/A16-staged-upgrade-rollback-check`, then injected the selftest failure. It exited 1 as expected, printed that previous files were restored, restored the fixture's old `AntiNotepad.exe` marker and `prior.marker`, and left no backup directory. The installer removed its staging directory.
- `git diff --check` passed.
- No app selftest was invoked: the test-only switch deliberately fails immediately before that process call. No real install, user app home, or real notes folder was accessed.

## Needs a live check

- Confirm the installed app's normal `--quit` and actual process-path detection on an upgrade, including that rollback relaunches only an app that was running beforehand.
- Confirm Start menu and enabled Run-at-login behavior on a real install, and visually confirm the app starts after a successful upgrade.
- Confirm uninstall removes the retained backup while preserving notes outside that backup.

The fixture files were removed after the rollback check. The shell blocked removal of the now-empty fixture directory, which remains at `build/A16-staged-upgrade-rollback-check`.
