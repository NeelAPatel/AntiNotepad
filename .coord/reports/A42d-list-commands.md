# A42d-list-commands

## Summary
Moved list recognition, continuation and numbering decisions into ListEditing, with DocumentCommands applying the edits at live line offsets. PadView retains input routing, both update scopes, post-edit caret reads, insertion plus immediate caret scrolling, and timer fall-through.

## Change list
| file:line-range | what changed | which Plan line it serves |
|---|---|---|
| product/AntiNotepad/Editor/PadView.cs:1-3 | Removed unused globalization import | After: move numbering |
| product/AntiNotepad/Editor/PadView.cs:674-694 | Delegate list commands within existing Enter/Tab routing and update scopes | After: keep routing, scopes and post-edit caret reads |
| product/AntiNotepad/Editor/PadView.cs:752-757 | Removed list command implementations; retained thin Insert | After: move recognition, continuation and Renumber |
| product/AntiNotepad/Editor/ListEditing.cs:3-132 | Added continuation result, recognition, pure continuation and block numbering decisions | After: pure decisions |
| product/AntiNotepad/Editor/DocumentCommands.cs:2-63 | Apply continuation and per-line digit replacements with live offsets; retain nested inner renumber | After: document application; Must not change: edit order |
| product/AntiNotepad/SelfTest.cs:66-69 | Run document-command checks | Test plan, code |
| product/AntiNotepad/SelfTest.cs:736-789 | Pure continuation and numbering cases | Test plan, code |
| product/AntiNotepad/SelfTest.cs:814-919 | Real document fixtures, anchor-based post-edit reads and single Undo checks | Test plan, code |
| product/AntiNotepad/SelfTest/Contracts.cs:93-98 | Added list-commands-preserved | Contracts |

## Behavior changes
None. New internal helpers expose the existing decisions and edits for headless checks; they add no shared mutable state, timing, settings or defaults. Numbering reads only the surrounding block and applies small edits in order. Existing public methods retain their meanings.

## Out-of-scope files
This requested report and the generated `.coord/reports/A42d-list-commands.gate.md` are reporting artifacts. No product files outside Files changed.

## Plan deviations
None.

## Contracts
Added `list-commands-preserved`; retained `checkbox-indent-preserved` and all existing rows. Result: 13 rows, 12 pass, 1 manual.

## Lens review
Fresh `lens_reviewer` verdict: accept; no proven findings. Reviewed every changed block, callers, document-change handling, edit order/live offsets, both nested Enter renumbers, update scopes, post-edit caret reads, insertion/scroll callback, timer fall-through, state, layering and checks. Behavior-change list matches the diff; scope is in plan. Live caret/scroll behavior remains a hand check.

## Checks run
- Task build with AVALONIA_TELEMETRY_OPTOUT=1 and NuGet audit disabled for sandbox: succeeded, 0 warnings, 0 errors.
- Task executable --selftest: LIST EDITING 48 checks; LIST COMMANDS 102 checks; CONTRACTS: 13 rows, 12 pass, 1 manual; SELFTEST PASS.
- `git diff --check`: passed. Task check output folder deleted after verification.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A42d-list-commands -Offline`: GATE: PASS. Build/selftest/seam/format/scope/cleanup PASS; all existing LAYERS bans pass. Offline WARN (NuGet audit off) and Size WARN (SelfTest.cs 3089 -> 3247 lines for planned tests); PadView shrank by 75 lines. Online audit remains manager-owned.

## Hand checks for the user
Not run: Codex did not launch or stop the app. Manager should hand off the identified dev copy for these approved checks:
1. End/mid-line Enter on `- ab`, `9. ab`, `9) ab`, `[x] ab`: same markers, suffix moves below, caret after new marker.
2. Empty nested Enter outdents; empty root Enter ends the list.
3. Tab/Shift+Tab with 9 -> 10; one Ctrl+Z reverses each complete Enter or Tab.
4. Shift+Enter, Enter with selection and Enter with completion open retain normal behavior.
5. Bottom-of-long-note Enter keeps caret visible; long-list typing feels unchanged.
6. Timer note Enter after `10s test` still starts the timer; Ctrl+Enter still toggles it.
