# A31-list-default-type-r2 lens review

## Findings (proven)
None.

## Behavior changes check
Report's list vs the diff: matches.

- `list` follows the normalized, persisted `ListDefault`, initially bullets. `todo`/`checklist`, `bullet`/`bulletlist`, and `numberlist` resolve to their fixed styles. Completion and plain-line continuation consume the same keyword resolution.
- Existing prefixed-line continuation, empty-item exit/outdent, numbering bounds, and Code-mode decline retain their existing branches. Other keywords retain their definitions and detection rules.
- Caller audit: `ListEditing.Continue` is called by `DocumentCommands.ContinueList` and `SelfTest/Lists.cs`; both supply a kind. `DocumentCommands.ContinueList` is called by `PadView.OnKeyDown` (supplies the live setting) and the document selftests (use the bullet default). These callers still satisfy the changed signatures.
- `Modes.ListKindOf` is consumed by document continuation, completion, and focused selftests. `KindOf`/`Marker` serve those same editor paths; `ListDefaultName` serves completion and its selftest. No missed caller found.
- Shared `Modes.Detect` consumers remain compatible: `Note.Text`, `Note.SetMigratedText`, and `Note.MakeTitle`; `Store.WriteSnapshot`; `Store.Search`; `NoteAnalysis.Update`; `PadView`'s document text-change callback; `DocumentCommands.ConvertToList`; completion; and selftests. Recognizing the new list keywords also gives those notes the existing list title/snippet and keyword-line treatment, as intended by keeping them in `NoteMode.List`. `Modes.Keywords` has no other product consumer beyond detection/resolution and completion; no other `ModeKeyword` construction or deconstruction callers were found.
- The Settings control uses `SettingsService.Update`; its persistence-only flag reaches `PadWindow.HandleSettingsChanged`'s existing scheduled-save path. Editors read the current shared Settings object at Enter/completion time. No new timer, subscription, deferred cache, or state latch is introduced.
- r2 changes only switch-arm line breaks in `Completion.cs` and `ListEditing.cs`; it adds no behavior change.

## Scope and plan check
In plan. Every product hunk serves the approved After, Settings control, or test plan. Save/shutdown implementation and existing prefixed-line logic are untouched. The Settings row matches the approved lab's labels and choices.

The string representation in Model, mapped to the existing editor enum, is a listed deviation and preserves the Model-to-Editor boundary without adding another enum. The original report's out-of-scope-file and scope-gate commentary is historical: all four paths now appear in the current card's Files list, and the supplied r2 gate passes Scope. The field-count and design-hash updates support the approved control; no unrelated design/default changes were found.

The `list-default-kind` contract and focused selftests cover keyword resolution, all three plain-line continuation styles, popup names, default/invalid settings, and persistence round trips. Existing document checks retain prefixed-line, undo, and caret coverage. Test hooks were not added to product classes.

## Unproven risks
- Settings layout, popup interaction, and live changes still need the planned hand checks in the slot dev copy; this review did not launch the app.

## Checks run
- Read the item card, r2 brief, original/r2 implementer reports, supplied r2 gate report, coding rules, changed-file context, and shared callers; applied all nine lens checklist items to the product hunks.
- Reviewed `git diff dev...HEAD -- product` (13 files) and `git diff -- product` (empty); working tree was clean before writing this report.
- Inspected commit `26659ac`'s r2 product diff: only three switch expressions were split across lines.
- Verified current HEAD `315fc98f2d5f23171b906a95ce24d6f28104496f` has no product differences from gate-tested `26659ace09d59806b09bcb433fd69a3cf1679215`.
- `git diff --check`: PASS.
- Supplied gate evidence, not rerun here: GATE PASS; Build 0 warnings/0 errors; SELFTEST PASS; CONTRACTS 37 rows, 36 pass/1 manual; Seam, Tokens, Format, Scope, Size, and gate cleanup PASS. NuGet audit was disabled for the offline build.
- No build, runtime probe, or GUI launch performed; no review build folder created. Product files were not edited.

VERDICT: accept
