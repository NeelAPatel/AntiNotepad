# A35-bar-insets-r3 report

## Route

Replaced the tall inline object on visual line 1 with `ScrollableTopInsetTextArea`, a `TextArea` subclass that implements
Avalonia's public `ILogicalScrollable` interface. It exposes a virtual 40 px leading segment in the scroll extent, maps
scroll offsets after that segment to the regular `TextView` offset, and arranges the existing TextArea template root lower
and shorter while any of the leading segment remains visible. This preserves AvaloniaEdit's native line construction,
selection, caret, and gutter machinery; no reflection, package fork, or line-height change is used.

This uses the installed AvaloniaEdit 12 public extension points: `TextEditor` accepts a `TextArea` through its protected
constructor and assigns that area to its template ScrollViewer; `TextArea` itself exposes its `TextView` and implements
logical scrolling by forwarding to that view. Sources: [TextEditor.cs](https://raw.githubusercontent.com/AvaloniaUI/AvaloniaEdit/12.0.0/src/AvaloniaEdit/TextEditor.cs),
[TextArea.cs](https://raw.githubusercontent.com/AvaloniaUI/AvaloniaEdit/12.0.0/src/AvaloniaEdit/Editing/TextArea.cs),
[TextView.cs](https://raw.githubusercontent.com/AvaloniaUI/AvaloniaEdit/12.0.0/src/AvaloniaEdit/Rendering/TextView.cs).

At logical offset 0, overlay/float pads have 40 px of blank space above line 1 inside the editor viewport. At offsets 20,
40, and 60, the remaining gap is 20, 0, and 0 px; from offset 40 onward, the `TextView` scrolls normally. Docked pads and
private previews set the inset to zero. Ctrl+Home and Ctrl+Shift+Home reset the adapter offset while leaving the native
key command and its caret/selection handling in control.

## Files changed

- `product/AntiNotepad/Editor/ScrollableTopInset.cs` — custom TextEditor/TextArea and virtual inset mapping.
- `product/AntiNotepad/Editor/Elements.cs` — removed `TopGapGenerator` and its tall inline object.
- `product/AntiNotepad/Editor/PadView.cs` — enables the inset only for overlay/float non-private pads; saves logical
  scroll position; top fade follows actual document scroll while bottom fade uses logical extent/viewport.
- `product/AntiNotepad/Editor/Renderers.cs` — documents that paper rendering combines the moved TextView origin with its
  document-local offset, so the inset is not counted twice.
- `product/AntiNotepad/UI/Chrome.cs` — adds `TopScrollableWhitespace`, derived from the 40 px docked row token.
- `product/AntiNotepad/UI/PadWindow.MaterialLab.cs` — blur refresh listens to adapter scroll changes, including movement
  through the blank segment.
- `product/AntiNotepad/SelfTest.cs` — checks token-derived inset mapping at offsets 0, 20, 40, and 60.

## Verification

- Director's product build: 0 warnings, 0 errors.
- Director's offscreen layout probe: PASS. The first-line origin moves 40 → 20 → 0 → 0 px at logical offsets 0/20/40/60;
  first-line metrics remain unchanged; gutter alignment, viewport mapping, selection/search rectangles, and caret
  MakeVisible behavior pass the probe.
- The synthetic Ctrl+Home event resets the adapter's logical offset to 0. The hidden fixture does not run AvaloniaEdit's
  native key bindings, so the resulting caret navigation remains unverified there.
- Isolated `--selftest`: **SELFTEST PASS**, exit 0; `ANTINOTEPAD_DATA` points inside the task check output. No pad GUI was launched or stopped.


## Director review and final gates

One gpt-6-luna xhigh worker implemented the change; the director reviewed the full product diff and the new adapter.
Review corrections included both style keys, preserving TextArea's default keyboard initialization, measuring the reduced
inner viewport, keeping the blank segment hit-testable, and fading only after actual document content scrolls offscreen.
The public logical-scroll adapter was chosen over fixed padding (which cannot scroll away) or inline elements (which
change line metrics). It retains the stock templates and adds no dependency.

Final build command: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A35-bar-insets-check -p:AntiNotepadDev=true -p:NuGetAudit=false`.
NuGet audit was disabled for this invocation only, matching the previous rounds. **0 warnings, 0 errors**.
[Build log](../../build/A35-bar-insets-r3-build.log), [selftest log](../../build/A35-bar-insets-r3-selftest.log).

The throwaway probe links the production adapter and Chrome tokens and uses a synthetic hidden Window without calling
Show or starting the pad. It drives the real editor ScrollViewer at offsets 0/20/40/60: reported viewport stays 250 DIP,
inner TextView viewport is 210/230/250/250, text and margin origins are 40/20/0/0, and inner offsets are 0/0/0/20.
Line 1 stays 24.68625 DIP high at this fixture's font size throughout. A short note that fits the viewport keeps all 40 DIP
of whitespace and has no scroll range. Probe source and results remain under `build/A35-r3-inset-probe`.
[Probe results](../../build/A35-r3-inset-probe/results.txt).

| Feature | Evidence and remaining manager check |
|---|---|
| Selection highlight | Production `BackgroundGeometryBuilder` rectangles for a line-1 segment stay within normal line height in the probe. Manager: select a heading on line 1 in both styles. |
| Current-line and other line backgrounds | Source review: stock TextView line metrics and layers are unchanged and arranged below the gap. Manager: visual highlight bounds. |
| Search highlights | Same native segment rectangle geometry verified; actual search UI remains a live check. |
| Caret and typing into view | Native BringCaretToView preserves the gap at the top, scrolls to line 50, then returns to line 1 in the probe. SmoothCaret uses document-local rectangles in the moved TextView. Actual typing/IME remains live. |
| Fold chevrons and gutter | Probe verifies a left margin moves with TextView at every tested offset; ChevronGutter uses normal VisualTop/TextLines[0].Height. Manager: actual first-heading chevron, fold/unfold, and gutter clicks. |
| Bullets and renumbering | Removed only the gap element generator; bullet generation and Return/Tab renumbering remain unchanged, with normal line-1 metrics. Manager: line-1 list and continuation/indent behavior. |
| Paper/grid | Reviewed translated TextView origin plus document-local offset: the gap is counted once, and row phase follows text. Manager: lined/dotted/grid while scrolling through the gap. |
| Hit testing and selection drag from gap | Full TextArea bounds draw transparent hit-test geometry. Upstream SelectionMouseHandler clamps negative TextView Y to zero and handles capture/drag on TextArea. Manager: actual click/Shift-click/drag from the gap and back into it. |
| PageUp and Home | Native command handling retained; inner viewport size verified. Manager: actual key navigation, especially around offsets 0-40 and with wrapped lines. |
| Ctrl+Home / Ctrl+Shift+Home | Synthetic key verifies adapter offset resets to zero without consuming the native caret/selection command. Hidden fixture does not execute native key bindings; manager must check actual caret/selection behavior. |
| Edge fades | Source review: top uses document offset (zero throughout the gap); bottom uses logical extent minus offset/viewport. Manager: fade onset immediately after gap and last-line visibility. |
| Blur top-band sampling | Both panes now refresh on virtual-offset events, including offsets 0-40 when TextView offset remains zero; crop remains translated from actual pane visuals. Manager: live scroll/typing freshness. |
| Docked and private/Settings previews | Source gate requires non-private overlay/float; private init updates the inset immediately. Probe verifies disabling the inset restores zero origin. Manager: switch styles and open Settings preview. |

At offset zero the gap is now outside every line: line-1 backgrounds and chevron use normal height; paper begins at the
translated text origin. First text still starts 40 DIP below the editor top (plus the existing editor margin outside it).
Logical scroll storage still includes the 40 DIP segment, matching the old inflated extent; no note migration is needed.
Ctrl+Home explicitly restores the leading whitespace even when TextView itself was already at document offset zero.

No commits, tags, installed-copy access, or window frame/show-hide changes. `git diff --check` passes.
Automatic approval review rejected recursive cleanup of the verified task-only `build/A35-bar-insets-check` path as
**"blocked by policy"**. The folder remains for manager cleanup; logs and the probe remain as evidence.

