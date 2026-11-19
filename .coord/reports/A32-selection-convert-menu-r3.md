# A32-selection-convert-menu-r3 report

## 1 Summary
Shared popup window setup and surface styling with the conversion submenu and added icons to its parent and three choices.

## 2 Change list
- `UI/PadMenu.cs`: shared transparent popup configuration and border/shadow; added a centered numbered-list text icon.
- `Editor/MenuEntry.cs`: added checklist, bullets, and numbered icon identifiers.
- `Editor/PadView.cs`: assigned icons to the conversion menu entries.
- `SelfTest/Lists.cs`: unchanged; the entry shape did not change.

## 3 Behavior changes
The submenu now uses the main menu transparency, border, radius, shadow and padding. Its parent and children show neutral icons. Conversion, selection and keyboard behavior are unchanged.

## 4 Out-of-scope files
None changed for this round. Existing item changes remain in the worktree.

## 5 Plan deviations
The standalone `dotnet format` was attempted before the gate but failed because the sandbox denied its build-host named pipe. The gate changed-line format check passed.

## 6 Contracts
The existing conversion contract remains unchanged; selftest reports 35 contracts, 34 passing and one manual. Popup appearance and icon legibility require hand verification.

## 7 Lens review
Accept; no proven findings. Reviewer inspected the diff and callers and ran `git diff --check`. Visual appearance remains a hand-check risk. Review preceded this report.

## 8 Checks run
- Standalone build: passed, zero warnings and errors; `--selftest`: SELFTEST PASS. Temporary build removed.
- `dotnet format product/AntiNotepad --include` on the three changed files: failed to connect to the build-host pipe (access denied).
- `pwsh -NoProfile -File tools/gate.ps1 -Task A32-selection-convert-menu-r3 -Offline`: PASS, including build, selftest, format, tokens, scope and seam. An initial concurrent run failed due to a shared intermediate DLL lock; sequential rerun passed.

## 9 Hand checks
Pending: in the dev copy, inspect submenu corners and shadow, and verify all four icons and numbered-icon legibility; confirm opening and selection behavior.
