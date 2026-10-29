# A55-split-view-clip-audit: findings for A55-split-view-clip

## 1. Kind and why

**Bug:** pane content can move across the split boundary during horizontal note scrolling. The Request explicitly reports this overlap and asks for cropping, not a new divider design (`.coord/tasks/A55-split-view-clip.md:9-13`). Current layout provides no stationary per-pane clip around the moving editors (`product/AntiNotepad/UI/PadWindow.Layout.cs:90-98`).

## 2. Now

In today's source, split view places the left PadView, divider, and right PadView directly in three grid columns. Split uses `"*,1,*"`; single view uses `"*,0,0"` and hides the right pane/divider (`UI/PadWindow.Layout.cs:90-98`; `UI/PadWindow.Navigation.cs:21-35`). Product file references below are relative to `product/AntiNotepad/`.

**What the user would see:** horizontal scrolling pulls the active note sideways and fades it; when pulling the right note toward the left, its text can slide over the divider into the left note. The divider is a narrow painted Border, not a crop boundary (`UI/PadWindow.Layout.cs:61-65`; `UI/NoteSwipe.cs:143-152,177-183`). This matches the reported symptom; dev does not already enforce the requested crop. This is a source-derived description, not a live observation in this audit.

Ordinary vertical scrolling changes editor scroll offsets, while horizontal input/Shift+wheel goes through NoteSwipe and translates the entire active PadView (`Editor/Motion.cs:47-64,132`; `UI/PadWindow.Navigation.cs:272-282`). Keyboard note changes also translate the whole PadView by up to 34 units for 190 ms (`Editor/PadView.cs:441-450`). The existing divider contract tests bottom clearance and pager layering, not pane clipping (`SelfTest.cs:262-299`; `SelfTest/Contracts.cs:69-75`).

## 3. Reproduction (bugs)

**Exact code-path reproduction; no executable or visual probe run.** No build folder was created, and no app was launched or killed. This form of reproduction is allowed by `docs/loop.md:27-29`.

1. Enable split and focus the right editor: split assigns two equal pane columns around the divider; right-editor focus selects that pane (`UI/PadWindow.Navigation.cs:31-35`; `UI/PadWindow.Layout.cs:90-96`; `Editor/PadView.cs:139`).
2. Send a small negative horizontal wheel/touchpad delta. The window routes it to `new NoteSwipe(..., () => _active, Navigate)` (`UI/PadWindow.cs:92`; `UI/PadWindow.Navigation.cs:273-282`). Default swipe mode is `progress`, length 220, firing at 0.35 (`Model/Store.cs:298-300`).
3. For a fractional delta of -0.44, `Progress` calls `Advance(-44)`; progress is -0.2, below the commit threshold. `Advance` calls `Show(-0.2 * pane width, 0.76)` (`UI/NoteSwipe.cs:81-92,143-152`).
4. `Show` assigns a TranslateTransform to the right PadView itself and sets X to that negative displacement (`UI/NoteSwipe.cs:177-183`). At pane width 320, it moves 64 units left: the editor's normal 36-unit left inset becomes 28 units beyond the pane's left boundary (with full-width text settings and no gutter; `Editor/PadView.cs:882-900`). Text near that edge therefore enters the divider/left column.
5. There is no stationary clipping parent at that boundary: both PadViews are direct children of the common grid (`UI/PadWindow.Layout.cs:30,90-98`). An editor-internal clip, or ClipToBounds on the translated PadView itself, moves with the pane and cannot enforce the original column boundary. Spring-back and commit/arrival reuse the same pane transform (`UI/NoteSwipe.cs:186-225`; `Editor/PadView.cs:441-450`).

This establishes a horizontal overlap path. A distinct overlap caused solely by vertical scrolling is **not reproduced**; if that remains after the fix, obtain the user's exact gesture, bar style and screenshot/short recording before widening the cause or scope.

## 4. Where

Expected implementation files:

- `UI/PadWindow.Layout.cs`: wrap each PadView in a stationary, zero-inset clipping host in its existing column; keep divider placement. BuildLayout creates both panes through NewPane (`:90-98,124-126`).
- `UI/PadWindow.Navigation.cs`: keep the right host's visibility aligned with split state, so a hidden right PadView cannot leave a visible clipping host (`:21-35,146-182`).
- `SelfTest.cs` and `SelfTest/Contracts.cs`: focused layout/clipping capability check and new regression contract, alongside the existing divider check (`SelfTest.cs:262`; `SelfTest/Contracts.cs:69-75`). Final Files list should name these exact paths, not the card's placeholder.

Callers/state reached: `_left`, `_right`, `_active` remain PadViews; BindPanes, Navigate, CycleSplitSlot and ToggleSplit continue binding/focusing those instances (`UI/PadWindow.Navigation.cs:21-35,52-74,146-182`). NoteSwipe owns settle timers, frame generation and spring-back; Arrive owns an animation handle (`UI/NoteSwipe.cs:25-32,114-140,186-215`; `Editor/PadView.cs:51,441-450`). These should remain unchanged. Documents/undo are cached per note, not per layout host (`Editor/PadView.cs:32,197-220`).

Top blur observes the actual pane transforms, visibility, scroll and lines, posts a deferred refresh, and samples TextViews through VisualBrush; it is a regression surface, not a proposed edit (`UI/PadWindow.TopBlur.cs:27-79,155-182,220-260`). MotionPreview is another PadView consumer with its own clipping frame; avoid changing global PadView behavior (`UI/MotionPreview.cs:110-111`).

## 5. Risk

The main risk is clipping at a moving boundary (giving a false fix), or changing host sizing/visibility so the right pane, focus or blur behaves incorrectly after toggling split. Keep motion on PadView and clipping on its stationary parent. Gate choices: reproduction test plus a new CONTRACTS row for fixed per-column clipping; `tools/gate.ps1` for compilation, selftest, existing divider promise, LAYERS/seam/scope/size; fresh Codex lens review for transform/visibility coupling; hand test for actual pixels, selection, caret and blur. These are the applicable Gates menu checks (`docs/loop.md:46-56`). A property-only selftest cannot establish rendered pixel containment.

## 6. Tier

**Confirm Medium.** This is editor rendering with a bounded layout fix (`.coord/tasks/A55-split-view-clip.md:6`; `docs/loop.md:67-74`). Existing timers/deferred work are downstream consumers, not proposed modifications. Raise to High if implementation needs to change their lifecycle or timing rather than retaining it.

## 7. Recommendation

Smallest change: stationary ClipToBounds hosts around the two existing PadViews, no padding/background, same columns and divider. Synchronize right-host visibility with split state. Use a small production layout helper if needed so selftest exercises the same host construction. Do not merely clip the common grid or the moving PadViews: neither fixes the internal divider boundary (section 3).

**After:** each note still follows the same swipe/arrival motion inside its allotted pane, but its content disappears at that pane's fixed edge and never paints over the divider or other pane. Divider height, pager ordering, note bindings, gestures and timings retain their current behavior (evidence for current behavior: sections 2-4).

**Code checks:** add a `split-pane-clipping` CONTRACTS row exercising production host construction. Arrange two hosts around the actual divider; verify column bounds, stationary clipping ancestors and right-host visibility in split/single states; apply positive/negative child transforms and verify the host boundary stays fixed. Include a rendered containment assertion if the existing test environment supports it; otherwise explicitly retain visual containment as a manual part of the contract. Preserve `split-divider-above-pager`. Run the planned gate/build/selftest during Build/Check, not this read-only audit (`docs/loop.md:46-56`; `SelfTest/Contracts.cs:69-75`).

**UI checks in the dev copy:** use dense notes with text near both edges; focus each pane in turn, pull toward and away from the divider, cancel a partial swipe and commit one. Check progress and detent modes, Shift+wheel and touchpad input, then keyboard arrival. Verify no text/caret/selection/decorations cross the divider; both panes remain editable and independently scrollable. Toggle split off/on, resize, and check docked/overlay/float bars (including revealed top blur and pager). These checks target the moving pane, navigation and blur consumers identified in sections 2-4.

**Scope questions:** none blocking the bounded horizontal clipping fix. The recorded word “scrolling” does not specify gesture direction; retain vertical scrolling as a hand check and request exact steps only if a separate vertical-only symptom persists (section 3). **Split item:** no; one layout boundary fix with its regression contract. No new visual design is proposed; any redesign would require the separate lab approval in `AGENTS.md`.

Audit verification: branch is `feat/A55-split-view-clip`; initial `git status --short` and `git diff --stat` were empty. Source inspection only; no build, selftest, gate or live UI result is claimed. Only this report was written.

READY: yes
