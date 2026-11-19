# A32-selection-convert-menu lens review

## Findings (proven)
1. [major] product/AntiNotepad/UI/PadMenu.cs:178 - The conversion submenu stays open when the pointer moves onto another parent-menu row. Exact code path: entering Convert to list sets `submenu.IsOpen = true` (179-180); leaving that row has no close handler, and entering an ordinary sibling only changes its hover paint (`AddHover`). The only submenu close paths are choosing a child (177), detaching the opener (178), or Left/Esc routed through the child panel (189-194). Hovering Settings, Undo, or another parent row does none of these, so the conversion choices remain displayed for an inactive row. Smallest fix: track the active submenu in the parent menu and close it when a sibling row becomes active, while allowing the pointer to travel from the opener into its children. Cover that transition in the hand check.

## Behavior changes check
Report's list vs the diff: matches. Selection-only conversion, prefix replacement, retained indentation and first-line keyword, blank skipping, and block numbering are present. The report identifies pointer/keyboard verification as pending.

Shared/new method callers:
- `MenuEntry` gains an optional trailing `Children` argument. Existing construction in `PadView.RequestMenu`, `PadWindow.Layout`, `SelfTest.Design`, and `MenuEntry.Separator` preserves argument meanings and defaults. `PadMenu.BuildMenuRow` is the new consumer; existing leaf actions retain their click-and-close path. `BuildActionRow` still consumes the same four existing editing entries.
- `ConvertLine`: called by `DocumentCommands.ConvertToList`; its nullable edit fits the existing `Apply` helper.
- `ConvertToList`: called by the `RequestMenu` conversion callback and `SelfTest.Lists`; line edits and subsequent block renumbering share one `RunUpdate` undo group.
- `ConvertMenu`: called by `RequestMenu` and `SelfTest.Lists`; three choices with selection, null without. The automated no-selection check tests the factory, as disclosed; the actual menu construction is also guarded by `hasSelection`.

Numbering can change adjacent unselected numbered lines. Probe: selecting only line 2 of `7. before\nselected\n23) after` produces `7. before\n8. selected\n9) after`; one Undo restores all three lines. This follows the approved existing block-renumbering rule (`DocumentCommands.cs:65-70`, `ListEditing.cs:145-165`).

## Scope and plan check
In plan. All seven changed product files map to the approved conversion, submenu, or selftest/contract work. No uncommitted product changes; no round rework briefs present. No unrelated hunks or unlisted deviations found. Existing menu entries, save/shutdown code, and typing handlers are unchanged; first-line mode keywords are explicitly skipped. The menu factory coverage deviation is disclosed.

## Unproven risks
- Native popup behavior still needs the dev-slot hand check: entering/clicking a child must not trigger the parent's light dismissal before its action executes.
- Parent-close cleanup is wired through opener `DetachedFromVisualTree` (PadMenu.cs:178); actual native popup detachment and child dismissal were not exercised.
- Right opens and focuses the first child; Left/Esc close when routed through the child panel. Actual focus transfer, key routing, and the hover-open/focus-still-in-parent case remain unverified in a displayed menu.

## Checks run
- Read item card, implementer report, supplied gate report, complete `dev...HEAD` product diff, uncommitted product diff, changed-file context, and callers. Reviewed plan bounds, contracts, state, timing, edge inputs, cleanup, failure paths, layering, and coverage across the hunks.
- Ran an offline temporary probe compiling the actual `ListEditing.cs`, `DocumentCommands.cs`, and `Modes.cs` with the existing AvaloniaEdit assembly. Build/run exit 0; `PROBE PASS: 11 checks`.
- Probe passes: LF/CRLF selection ending at a line start excludes the next line; select-all with an empty last line excludes that empty line; selecting the final newline targets its preceding line; CRLF preserved; blank-separated numbering restarts; adjacent unselected numbering follows existing block rules; all three conversions preserve indentation, whitespace-only lines, and the first-line keyword; every exercised document conversion restores the exact original in one Undo with no remaining undo entry.
- Supplied gate evidence (not rerun): `GATE: PASS`, build 0 warnings/errors, `SELFTEST PASS`, seam/tokens/format/scope pass.
- No installed/dev app launched; no displayed-menu hand test performed. Temporary `build/A32-selection-convert-menu-review` probe folder removed after execution. Product files unchanged by this review.

VERDICT: rework
