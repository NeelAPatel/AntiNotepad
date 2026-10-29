# A42c-checkbox-indent-audit: findings for A42c-checkbox-indent

Source audit of `feat/A42c-checkbox-indent`, HEAD `b6858bf`, 2026-10-29. All product paths below are relative to `product/AntiNotepad/`. No app, build, selftest or probe was run; visible behavior is inferred from the current code, not observed on screen. Only this report was written. STATUS and the item card remain untouched as directed.

## 1. Kind and why

**Polish/reorg, behavior-neutral.** Today's UI already provides checkbox toggle and list indentation; the requested structural extraction is still outstanding: `PadView.ToggleCheckbox` owns text decisions and mutation (`Editor/PadView.cs:657-676`), and `Indent` owns document mutation (`:770-781`). Moving these into testable Editor helpers reduces command responsibility in the control. This is not a bug fix and needs no new behavior or design. Request: `.coord/tasks/A42c-checkbox-indent.md`, Request section.

## 2. Now

| Path | Current behavior and what the user would see |
|---|---|
| Checkbox click | `CheckboxGenerator` substitutes a clickable invisible glyph; the renderer paints the box. Press marks the pointer event handled and calls `pad.ToggleCheckbox` on the captured line (`Editor/Elements.cs:59-75`). An empty box becomes checked; a checked box becomes empty. The renderer uses group 2 for empty/checked (`Editor/Renderers.cs:409-422`); checked text gets Done color and strikethrough (`Editor/Colorizers.cs:80-86`). Generation is disabled when `Decorate` is false (`Elements.cs:50,61`; `PadView.cs:67`). |
| Ctrl+Enter | Tunnel handler (`PadView.cs:136`) handles Control+Enter without Shift, including a nonempty selection; Timer mode calls `ToggleTimerAtCaret`, every other mode calls `ToggleCheckbox(CaretLine)` (`:687-691`). There is no Code-mode, fence or read-only guard in this command path; preserve the current dispatch rather than introducing one. |
| Toggle edit | Invalid line is ignored. Existing `[ ]` changes only its state character to `x`; `[x]` or `[X]` changes to a space, retaining indentation, optional bullet and following text (`:659-667`). Otherwise whitespace indentation is retained; a `-*+` bullet plus its whitespace becomes `[ ] `, or `[ ] ` is inserted before the body (`:671-674`). Examples: `[X] task` -> `[ ] task`; `[ ]task` -> `[x]task`; `  -   task` -> `  [ ] task`. Numbered markers are not stripped. |
| Tab / Shift+Tab | Only an empty selection on a recognized list line enters the custom branch. Tab adds exactly two ASCII spaces to the current line; Shift+Tab removes up to two leading ASCII spaces, leaving tabs intact. Other lines/selections fall through to AvaloniaEdit. `IsListLine` accepts the prefix grammar but rejects numbered markers longer than nine digits (`:700-703,764-779`; `Editor/ListEditing.cs:8-17`). The Tab branch has no Code-mode or Control/Alt exclusion; Shift alone decides direction. |
| Undo and caret | Toggle is one Replace or Insert, with no explicit caller `RunUpdate` (`:667,673-674`). Tab wraps indent **and** renumber in one `RunUpdate` (`:703`), intended to undo in one step. Neither command explicitly assigns the caret, moves focus or calls BringCaretToView: caret/selection track document edits through AvaloniaEdit. Preserve edit offsets and insertion/replacement choices; exact caret and viewport outcomes require the hand checks. Renumber receives a **new read of CaretLine after indent**, not a saved pre-edit line (`:703`). |
| Renumber reached by Tab | Walks the contiguous nonblank block of list lines or space-indented text. Counters are per indentation width; returning to shallower levels clears deeper counters; a nonnumbered marker clears that level. The first number is preserved unless the newly indented target opens a fresh level, then it starts at 1. Nine-digit parsing and the 999,999,999 boundary are retained; replacements fetch live line offsets, so digit-width changes do not stale later edits (`:788-812`). |

Delta against the old plan: A42a-math-notes has already introduced `MathNotes` and `ListEditing` (`Editor/MathNotes.cs:6-54`, `Editor/ListEditing.cs:6-19`); A42b-note-analysis-file has already moved `NoteAnalysis`, changed-range logic and TimerKey (`Editor/NoteAnalysis.cs:9-23,271`). `git diff 34e5dee HEAD -- product/AntiNotepad/Editor/PadView.cs` shows the command area retaining its algorithm with list parser references changed to `ListEditing`; startup binding now uses injected `INotesLocation` (`PadView.cs:80-82,275-286`). Current command references are :657, :690, :703, :770, :788, :816, not the original plan's offsets. The old proposed impact radius misses the regex consumers in Renderers and Colorizers listed above. No new extraction of analysis, math, bindings, links or split-pane layout is needed.

## 3. Reproduction (bugs)

Not applicable: no defect is claimed. The exact existing command paths are click `Elements.cs:74` -> `PadView.cs:657` -> one edit at :667/:673/:674; keyboard `PadView.cs:687-690` -> same facade (or timer); Tab `:700` -> outer update `:703` -> `Indent :770` -> post-edit `CaretLine` -> `Renumber :788`. This source trace establishes the refactor baseline; it is not a GUI reproduction or proof of library caret/undo behavior.

## 4. Where

Proposed implementation Files list:

- `Editor/PadView.cs`: retain the public toggle facade and key routing; replace toggle/indent bodies with adapter calls. Keep the existing outer Tab update and `Renumber` invocation (`:657-703,770-781`).
- `Editor/ListEditing.cs`: pure per-line edit decisions and the one canonical compiled checkbox regex, moved from `Editor/Elements.cs:46` unchanged. Group 1 stays indentation, group 2 stays state. Keep `ListPrefix` distinct (`ListEditing.cs:8`), and leave the different whole-note grammar in `Editor/TextCommands.cs:8-9` alone.
- New `Editor/DocumentCommands.cs`: accept TextDocument and scalar line/direction inputs; obtain the current line, apply one relative edit at its live offset, preserve toggle's invalid-line no-op. Own neither editor/caret nor update grouping. No document snapshots, static mutable state, timers or subscriptions.
- `Editor/Elements.cs`, `Editor/Renderers.cs`, `Editor/Colorizers.cs`: direct regex-owner rewires only (`Elements.cs:53,64`; `Renderers.cs:409`; `Colorizers.cs:80`). Do not leave a renderer-owned regex alias. Existing click call stays intact.
- `SelfTest.cs`: extend `ListEditingChecks` and LAYERS, add focused real-document checks. Existing ListEditing coverage is only the two parser cases (`:722-733`); it does not establish toggle, indent or undo behavior. If registering the preservation promise below, also list `SelfTest/Contracts.cs` explicitly.

Shared reach: edits use the same document cached per note with undo limit 400 (`PadView.cs:201-215`), or the private preview document (`:263-292`). Document changes invalidate NoteAnalysis before scheduling; over 1,500 lines they accumulate pending ranges with a 140 ms timer, otherwise analysis runs immediately (`:455-491`). Analysis receives timers and the inline-math scalar (`:508`); Edited notifies the host (`:488`, `UI/PadWindow.Layout.cs:144`), which has a save timer and QueueSave (`UI/PadWindow.cs:87,392`). Checkbox rendering also uses its paint cache (`Renderers.cs:395-403`). These are downstream effects, not extraction targets: preserve edit count/order and lifecycle. Timer Ctrl+Enter remains in PadView (`:690,707-713`).

## 5. Risk

The highest risk is preserving document-edit semantics across the new boundary: splitting the Tab undo group, capturing caret too early, replacing a whole line instead of the existing small edit, or changing regex captures shared by input, hit testing and painting (`PadView.cs:663-674,703,811`; `Elements.cs:65-66`; `Renderers.cs:411,417`; `Colorizers.cs:81-85`).

Use the `docs/loop.md` Gates menu: `tools/gate.ps1` catches compile/selftest/LAYERS/CONTRACTS/seam/scope/size failures; fresh Codex `lens-review` checks grouping, offsets, missed consumers and new state; Hand test checks actual caret, undo, click geometry, colors and viewport. LAYERS proves boundaries, not behavior. No lab or High-tier design review is required for the proposed behavior-neutral scope. No gate was run in this Findings audit.

## 6. Tier

**Confirm Medium**, as the current card states: this reshapes editor input/caret/document commands and their undo boundary, rather than merely moving a type unchanged (`PadView.cs:657-703,770-781`; `docs/loop.md`, Tiers). Downstream save/analysis notification already exists; leave those systems untouched. Introducing timers, save-path changes or deferred work would require reconsidering scope/tier.

## 7. Recommendation

**Smallest change / After:** ListEditing plans a single nullable relative edit (offset, removed length, inserted text); DocumentCommands applies it to one current line. PadView retains its toggle facade and key dispatch, with indent delegated inside the **existing** Tab update followed by **existing** Renumber on the post-edit caret line. Users see the same box, text, indentation, undo and caret behavior. Keep `IsListLine`, `ContinueList`, `Renumber` and `Insert` unchanged in PadView; the latter's document insertion and BringCaretToView (`:856-859`) belong to A42d-list-commands. No further split is needed; renumber need not move to extract indent.

**Boundary proof (planned):** retain all existing ListEditing bans (`SelfTest.cs:769-784`: UI, Platform, Motion, Theme, PadWindow, PadView, Program, DesktopPlatform, Window, Control, Dispatcher, TextDocument, Timers, Settings, SettingsService, CheckboxGenerator). Add exact TextEditor/TextArea/TextView bans, as NoteAnalysis already does (`:798-800`). For DocumentCommands copy those bans except **TextDocument is allowed**, and add the same TextEditor/TextArea/TextView and CheckboxGenerator bans. Example live-schema rows:

```csharp
new("DocumentCommands", "UI", ExactFrom: typeof(Editor.DocumentCommands)),
new("DocumentCommands", "PadView", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(Editor.PadView)),
new("DocumentCommands", "TextView", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(AvaloniaEdit.Rendering.TextView)),
new("ListEditing", "TextEditor", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(AvaloniaEdit.TextEditor)),
```

Use namespace rows only for AntiNotepad namespaces and ExactTo for external types: matcher `SelfTest.cs:1199-1209` does not provide a general external-namespace ban. Review remaining Avalonia/AvaloniaEdit dependencies explicitly. During implementation prove representative added guards fail on an actual temporary IL reference and revert it; an unused using is insufficient. Boundary rows alone cannot prove unchanged UI behavior.

**Code checks (planned, not executed):**

- Pure exact-edit cases derived from `PadView.cs:659-674`: empty/checked/uppercase state; `[ ]task`; optional bullet; multiple bullet spaces; whitespace indentation; plain, empty and numbered lines; invalid line no-op at adapter boundary. Verify unchanged regex captures/length consumed by all three visual consumers.
- Indent cases from `:774-779`: two-space insert; outdent zero/one/two/three spaces; tab-only/mixed leading whitespace; edits at nonzero document offsets and both LF/CRLF without changing delimiters. Real TextDocument adapter checks assert exact resulting text and one Undo restoring each changed toggle/indent fixture.
- Keep full gate/selftests, especially existing number-parser checks (`SelfTest.cs:727-730`) and A29 bounds (`:2787-2788`: copied <= 8, analyzed < 32). A standalone TextDocument test of indent does **not** exercise private PadView.Renumber: do not report it as proof of combined Tab behavior. Verify combined indent+renumber through the hand test and unchanged orchestration review; do not widen into A42d merely to expose its method.
- Recommend one preservation CONTRACTS row, `checkbox-indent-preserved`, pointing to these focused checks plus manual routing/caret/combined-undo checks, using the existing row schema (`SelfTest/Contracts.cs:7-12,19-26`). This documents the refactor promise; no bug contract is being claimed. Register checks once, avoiding duplicate execution.

**UI checks (user/manager, dev copy only):** click and Ctrl+Enter on `[ ]task`, `[X] task`, a bullet checkbox and an ordinary bullet; verify identical box geometry, strikethrough, source text and one Ctrl+Z. Click a box away from the current caret and compare caret/selection with the pre-change build. Verify Timer Ctrl+Enter still toggles a timer and Ctrl+Shift+Enter follows the existing path. Tab/Shift+Tab a bullet, checkbox and numbered nested list; verify two-space behavior, start-at-1 on a fresh nested level, preserved starting numbers and 9 -> 10 offsets; one Ctrl+Z restores both indentation and renumbering. Check root outdent, tab-leading lines, nonempty selection and non-list fallback. Check caret mid-line/end-line, scrolling near the bottom, Code mode and fenced text against the baseline. Switch notes to check independent undo, and make an edit in a 5,000-line note to check responsiveness/analysis. These checks close the source-only evidence limits above.

**Open scope questions:** none for the stated behavior-neutral extraction. Manager must replace the card's placeholder Files and approve the concrete plan before Build (`.coord/tasks/A42c-checkbox-indent.md`, Files/Plan/Go; `docs/loop.md`, Plan). Preserve existing edge behavior, including modifier/mode handling, rather than treating it as a defect in this item. **Split:** keep A42d-list-commands separate; no coupled move is necessary.

READY: yes
