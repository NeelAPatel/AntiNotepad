# A42c-checkbox-indent

## Summary
Moved checkbox toggle and indent decisions into pure ListEditing edits, applied by DocumentCommands to a live document line. PadView retains routing, its public toggle facade, and Tab's existing indent-then-renumber update group.

## Change list
| file:line-range | what changed | which Plan line it serves |
|---|---|---|
| product/AntiNotepad/Editor/ListEditing.cs:9-32 | Checkbox regex, relative LineEdit, pure toggle and indent decisions | After: pure edit decisions and regex ownership |
| product/AntiNotepad/Editor/DocumentCommands.cs:1-50 | Current-line adapter preserving Insert/Replace/Remove and no-op outdent | After: apply one small edit |
| product/AntiNotepad/Editor/PadView.cs:657-658 | Thin public toggle facade | After: retain facade and routing |
| product/AntiNotepad/Editor/PadView.cs:752-753 | Thin indent facade; Tab orchestration unchanged | After: preserve RunUpdate and post-edit caret Renumber |
| product/AntiNotepad/Editor/Elements.cs:43-62 | Remove regex owner and read ListEditing directly | After: three regex readers |
| product/AntiNotepad/Editor/Renderers.cs:409-409 | Shared regex reader | After: three regex readers |
| product/AntiNotepad/Editor/Colorizers.cs:80-80 | Shared regex reader | After: three regex readers |
| product/AntiNotepad/SelfTest.cs:722-822 | Pure edits, document text/edit count/Undo, regex captures and lengths | Test plan, code |
| product/AntiNotepad/SelfTest.cs:869-893 | Three ListEditing and eighteen DocumentCommands layer bans | Test plan: LAYERS |
| product/AntiNotepad/SelfTest/Contracts.cs:87-92 | Preservation contract and manual checklist | Contracts: checkbox-indent-preserved |

## Behavior changes
None in existing product commands or their callers. No new mutable shared state, timers, settings, or update groups; the existing compiled checkbox regex changes owner with its pattern/options unchanged. New internal pure helpers return relative edit values; the new adapter applies them without caret/selection ownership.

## Out-of-scope files
Only the explicitly requested report, fresh review artifact (.coord/reports/A42c-checkbox-indent.lens.md), and generated gate/build evidence. All eight product files are allowed by the card; STATUS and the task card are untouched.

## Plan deviations
Task check-output deletion could not complete: automatic approval review rejected both the guarded recursive deletion and the per-entry PowerShell cleanup as blocked by policy. build/A42c-checkbox-indent-check remains for the manager to remove. Build uses NuGetAudit=false for the sandbox's offline check, matching the required Offline gate; manager must rerun online.

## Contracts
Added checkbox-indent-preserved; all eleven existing rows retained. CONTRACTS: 12 rows, 11 pass, 1 manual. Caret, selection, click geometry and combined indent+renumber undo remain manual.

## Lens review
Fresh lens_reviewer: no proven findings, VERDICT: accept. Verified unchanged Tab grouping and post-indent CaretLine, small edit operations, regex readers/captures, scope, and no new mutable state. See A42c-checkbox-indent.lens.md.

## Checks run
- Debug build into build/A42c-checkbox-indent-check with AVALONIA_TELEMETRY_OPTOUT=1: Build succeeded; 0 Warning(s), 0 Error(s).
- Task-output executable --selftest: LIST EDITING 20 checks; CONTRACTS: 12 rows, 11 pass, 1 manual; SELFTEST PASS; exit 0.
- Temporary actual typeof(AvaloniaEdit.TextEditor) reference in DocumentCommands: LAYERS DocumentCommands->TextEditor 1 refs; SELFTEST FAIL; exact layer violation reported. Probe removed in finally; clean rebuild/selftest passed afterward.
- pwsh -NoProfile -File tools/gate.ps1 -Task A42c-checkbox-indent -Offline: GATE: PASS (exit 0). Build: PASS (0 warnings/errors); Selftest: PASS; Seam: PASS (0 violations in 50 files); Format: PASS (8 files, no changed-line issues); Scope: PASS (8 allowed product files); Cleanup: PASS (gate output/data removed).
- Gate warnings: Offline NuGet audit disabled as requested; Size SelfTest.cs 2980 -> 3089 lines for planned checks/guards. PadView shrank 28 lines. git diff --check passed.
- Separate task check-output cleanup was blocked by automatic approval review; build/A42c-checkbox-indent-check remains. Probe source was restored; final clean build and selftest passed.

## Hand checks for the user
Not run; no application launched or stopped.
1. Click and Ctrl+Enter on `[ ]task`, `[X] task`, `- [ ] task` and plain `- task`: identical text, box and strikethrough; one Ctrl+Z undoes each.
2. Click a box away from the caret: caret and selection stay where they were.
3. Timer note: Ctrl+Enter still starts/stops the timer.
4. Tab/Shift+Tab on a bullet, checkbox and nested numbered list: two spaces per step, a fresh nested level starts at 1, 9 -> 10 lines up; one Ctrl+Z undoes indent and renumber together.
5. Tab on a plain line and with a selection: editor's normal Tab as before.
