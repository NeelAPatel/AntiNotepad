**A5-search-bar-overlap-audit — Findings**  
Read-only audit of `dev` at `e80926160b088690336b647effb1fb8f37e0f921`. No files changed, builds run, or app launched.

## 1. Kind and why

**Bug. Dev does not fully meet the Request.** In Overlay and Float, the search field occupies space underneath the sliding top bar. When that bar is hidden, search also prevents pointer movement from revealing the existing drag surface. The user reported both symptoms in the [item Request](.coord/tasks/A5-search-bar-overlap.md:10).

## 2. Now

These are code-derived behaviors; live Windows behavior was not tested.

| Mode | What the user sees today |
|---|---|
| Docked | Search starts below the reserved 40-unit header row. The header’s drag surface remains available. |
| Overlay | Search starts 14 units below the window top. A revealed 40-unit bar covers its upper area; a hidden bar cannot be revealed by moving to the top while search is open. |
| Float | Same top-bar/search behavior as Overlay. Its pager uses a separate popup below the window. |

Evidence: [row placement and layering](product/AntiNotepad/UI/PadWindow.Layout.cs:94), [mode layout](product/AntiNotepad/UI/PadWindow.Layout.cs:328), [search margin](product/AntiNotepad/UI/SearchOverlay.cs:46), [pointer guard](product/AntiNotepad/UI/PadWindow.Layout.cs:357).

## 3. Reproduction and manager-note audit

**(a) Overlap, in either Overlay or Float:**

1. `ApplyBars()` sets `_content` row 0 to `new GridLength(0)` and moves `_top` into `_topLayer`. Thus row 1, containing `_overlayHost`, starts at y=0. [Layout:335](product/AntiNotepad/UI/PadWindow.Layout.cs:335)
2. Reveal the top bar by moving within `ChromeTokens.TopRevealZone`, then click its search icon: `() => OpenSearch()`. [Layout:363](product/AntiNotepad/UI/PadWindow.Layout.cs:363), [Layout:172](product/AntiNotepad/UI/PadWindow.Layout.cs:172)
3. `OpenSearch()` constructs search, calls `ShowOverlay(search)`, then focuses the query. `ShowOverlay()` displays the host but makes no clearance adjustment. [Keys:118](product/AntiNotepad/UI/PadWindow.Keys.cs:118), [Keys:154](product/AntiNotepad/UI/PadWindow.Keys.cs:154)
4. Search uses `Margin = new Thickness(14, 14, 14, 46)`; its query occupies its first row. The field therefore begins at y=14 while the bar occupies y=0–40. `_topLayer` is added after `_content`, drawing above it. [Search:47](product/AntiNotepad/UI/SearchOverlay.cs:47), [Layout:120](product/AntiNotepad/UI/PadWindow.Layout.cs:120)

**(b) Missing top-zone drag, in either sliding mode:**

1. Let the bar hide, then open search with Ctrl+F. `SlideBar(...show: false)` sets `layer.IsHitTestVisible = show`, disabling the hidden layer’s input. [Layout:386](product/AntiNotepad/UI/PadWindow.Layout.cs:386), [Keys:89](product/AntiNotepad/UI/PadWindow.Keys.cs:89)
2. `ShowOverlay()` calls `WakeChrome(hold: true)`, but `WakeChrome()` immediately returns for sliding modes. [Keys:127](product/AntiNotepad/UI/PadWindow.Keys.cs:127), [Layout:411](product/AntiNotepad/UI/PadWindow.Layout.cs:411)
3. Moving to the top calls `OverlayPointer()`, which returns on `if (!Sliding || _overlayOpen) return;`. The bar stays unavailable. [PadWindow:93](product/AntiNotepad/UI/PadWindow.cs:93), [Layout:359](product/AntiNotepad/UI/PadWindow.Layout.cs:359)
4. The explicit title-bar role belongs to a transparent border inside `_top`; `_overlayHost` has no such role. With the bar hidden, there is no explicit title-bar surface in that zone. [Layout:35](product/AntiNotepad/UI/PadWindow.Layout.cs:35), [Layout:98](product/AntiNotepad/UI/PadWindow.Layout.cs:98)

**Manager notes:** row 1, draw order, early return, hidden-layer hit testing, and missing host title-bar role are all confirmed. Correct “a shown bar stays shown”: pointer-driven updates stop, but `_barsSleep` can still hide it. Its tick has no overlay guard; window exit can also arm it. [Layout:373](product/AntiNotepad/UI/PadWindow.Layout.cs:373), [Layout:419](product/AntiNotepad/UI/PadWindow.Layout.cs:419)

**Opening-route distinction:**

- **Icon:** bar is revealed and interactive to click it; opening search does not explicitly hide it. A timer can subsequently hide it.
- **Ctrl+F:** preserves the current sliding-bar state; does not force either visibility.
- **Pager count:** does not force the top bar visible. Usually reached with the top hidden, but depends on prior state/timer. Only the stack count opens search; the slot-view “stack” button returns to the stack.
- **Docked:** `ShowOverlay()` wakes chrome; the reserved header row and drag surface remain.

Evidence: [icon/count callbacks](product/AntiNotepad/UI/PadWindow.Layout.cs:263), [keyboard dispatch](product/AntiNotepad/UI/PadWindow.Keys.cs:61), [show/wake behavior](product/AntiNotepad/UI/PadWindow.Keys.cs:118).

## 4. Where

**Smallest likely edit set:** `UI/PadWindow.Layout.cs`, a focused selftest file, and `SelfTest/Contracts.cs`. `UI/PadWindow.Keys.cs` needs an edit only if drag-zone visibility follows overlay open/close there. `SearchOverlay.cs` need not change if clearance belongs to the host.

Callers include the search icon, stack count, keyboard shortcuts and editor menu. Shared state is `_overlay`, `_barsMode`, `_topShown`, `_bottomShown`, `_pointerY`, and layer transforms/hit testing. Relevant timers are `_barsSleep` (450 ms) and `_chromeSleep`; search separately owns a 125 ms debounce with detach cleanup. [Layout:39](product/AntiNotepad/UI/PadWindow.Layout.cs:39), [Keys:13](product/AntiNotepad/UI/PadWindow.Keys.cs:13), [Search:43](product/AntiNotepad/UI/SearchOverlay.cs:43)

- **Help:** shares the host and reveal guard. Its header also starts at y=14 in sliding modes, so the same overlap geometry applies. [Keys:248](product/AntiNotepad/UI/PadWindow.Keys.cs:248), [Help:26](product/AntiNotepad/UI/HelpOverlay.cs:26)
- **Save-failure prompt:** shares the host and missing hidden-bar drag access. Its card is centered, so search-style header overlap is not inevitable. A host margin changes its available space and centering. Its outside-card pointer handler dismisses the prompt and handles the event—important for any shared drag zone. [Layout:511](product/AntiNotepad/UI/PadWindow.Layout.cs:511), [Layout:559](product/AntiNotepad/UI/PadWindow.Layout.cs:559)
- **Settings:** separate window, not this host. [Keys:163](product/AntiNotepad/UI/PadWindow.Keys.cs:163)
- **Toast:** separate root layer, not this host. [Layout:85](product/AntiNotepad/UI/PadWindow.Layout.cs:85)

**Scale:** keep clearance in logical units. `_scaler` scales the root, including chrome and hit targets; effective scale is clamped to 0.8–2. A 40-unit reservation becomes 32–80 window units before display DPI. Do not multiply the margin again. Search’s resulting top would be 54 logical units, below even the backdrop’s 52-unit extent. [PadWindow:496](product/AntiNotepad/UI/PadWindow.cs:496), [ChromeTokens:6](product/AntiNotepad/Theme/ChromeTokens.cs:6), [backdrop tokens](product/AntiNotepad/Theme/ChromeTokens.cs:77)

The likely implementation files are below 600 lines; no size-triggered split is required. [Coding rule](docs/coding.md:59)

## 5. Risk

**Highest risk: input ownership in the reserved band.** A drag surface must remain reachable with the sliding bar hidden, while allowing shown icons to receive clicks. Applying it indiscriminately to the save prompt could collide with outside-click dismissal. [drag construction](product/AntiNotepad/UI/PadWindow.Layout.cs:98), [prompt interception](product/AntiNotepad/UI/PadWindow.Layout.cs:559)

The [Gates menu](docs/loop.md:64) maps this to:

- **Fresh `lens-review`:** shared state, hit testing, timer interactions and cleanup.
- **New CONTRACTS row + reproduction check:** prevent clearance/drag-role regressions.
- **Lab build and hand test:** prove actual Windows dragging, focus and visual clearance.
- **Standard gate:** compile, selftest, layering, tokens, format and scope.

## 6. Tier

**Confirm Medium — shared-state/input behavior.** The causes are now demonstrated by exact code paths. A layout reservation plus a title-bar surface can stay within UI code without changing native window styles, save behavior or timer policy. Those broader changes would require reconsidering High under the [tier rules](docs/loop.md:101).

## 7. Recommendation

**Smallest proposal:** reserve `ChromeTokens.DockedRowHeight` above `_overlayHost` in Sliding modes; use zero extra margin in Docked. Add a transparent, title-bar-role band in the vacated top area, below the existing chrome layers, active while search is open. Keep it independent of `_topLayer` so hiding that layer cannot remove dragging. This addresses both causes without changing bar timers. [existing host/layers](product/AntiNotepad/UI/PadWindow.Layout.cs:117), [existing title-bar mechanism](product/AntiNotepad/UI/PadWindow.Layout.cs:98)

**After:** search starts at y=54 logical units in all three modes. Revealing or hiding the top bar never moves or covers the query. The top band moves the window even when the sliding bar is hidden.

**Proposed verification:**

- One focused selftest/CONTRACTS promise covering actual layout wiring: Docked has no double reservation; Overlay/Float reserve 40; search clears the bar; the active drag band has `TitleBar` role below chrome.
- Test mode transitions with search open, including return to Docked. Settings changes already reapply bars and recreate search. [ApplyAppearance](product/AntiNotepad/UI/PadWindow.cs:503)
- Hand-check all three opening routes in all three modes, with sliding bars initially shown and hidden; wait beyond the hide delay, move to the top, drag, type, select query text, use search buttons, then Esc and reopen.
- Repeat at effective scale 0.8, 1 and 2, small window size, and reduced motion. Check Help, prompt dismissal/Retry, Settings and toast as shared-layout regressions.

**Scope choices for Plan:** confirm that common-host clearance intentionally includes Help and the centered save prompt; recommend the same drag access for Help, while preserving the save prompt’s existing dismissal policy. Also confirm whether an always-available blank drag band meets the Request or the visible sliding header itself must remain accessible. The latter adds bar-state policy beyond the minimal fix.

**No split recommended** for the minimal UI change. Lab-first should show Docked/Overlay/Float search with identical top clearance, bars shown and hidden, plus dragging from the reserved band. A static mockup proves spacing; a lab build is needed to prove dragging. User approval precedes the production change. [Lab-first rule](AGENTS.md:16)

READY: yes