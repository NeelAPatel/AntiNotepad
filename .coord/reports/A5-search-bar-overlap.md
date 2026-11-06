## Summary
Search and Help now clear a reserved 40-logical-unit band in Overlay and Float. The band uses the title-bar role below chrome, remains available while the bar is hidden, and releases input when overlays close or a save-failure prompt replaces them. Docked keeps its existing reservation.

## Change list
file:line-range | what changed | which Plan line it serves
--- | --- | ---
product/AntiNotepad/UI/PadWindow.Layout.cs:1 | File purpose header | Coding standard on touch
product/AntiNotepad/UI/PadWindow.Layout.cs:37 | Retain the independent overlay drag band | After: hidden-bar dragging
product/AntiNotepad/UI/PadWindow.Layout.cs:96-119 | Build the title-bar band; apply sliding clearance and Search/Help-only drag eligibility | After; scope choices; preserve prompt dismissal
product/AntiNotepad/UI/PadWindow.Layout.cs:150 | Add band above content and below chrome | Code test: layer ownership
product/AntiNotepad/UI/PadWindow.Layout.cs:381 | Reapply overlay layout when bar mode changes | Code test: mode switch
product/AntiNotepad/UI/PadWindow.Keys.cs:1 | File purpose header | Coding standard on touch
product/AntiNotepad/UI/PadWindow.Keys.cs:126,141 | Refresh band on overlay open, replacement, and close | After; preserve prompt input ownership
product/AntiNotepad/SelfTest/Contracts.cs:21-26 | Add only the new contract row | Contracts
product/AntiNotepad/SelfTest/Design.cs:12-13,22 | Import chrome/input types and refresh the control-tree snapshot hash for the added band | Code tests and existing design snapshot gate
product/AntiNotepad/SelfTest/Design.cs:122-170 | One focused layout-helper check | Code test: clearance, title-bar role, layer order, mode round trip and overlay lifecycle

## Behavior changes
- New per-window `_overlayDragBand` has a transparent hit surface, height `DockedRowHeight`, top alignment, and title-bar role. It is inserted between content and chrome, under the existing scaler.
- `ShowOverlay` now reserves 40 logical units above the shared host in sliding modes and enables the band only for Search/Help. A save-failure prompt shares clearance (its available centering area is smaller) but disables the band so ordinary outside-click handling remains available.
- `CloseOverlay` hides the band. Opening or replacing an overlay recalculates eligibility. `ApplyBars` recalculates margin and visibility after a mode change; returning to Docked removes the extra margin and hides the band.
- Internal layout helpers expose construction/application to the existing selftest pattern. No public signatures, timer policy, motion, defaults, settings values, or native window styles changed.
- The pinned design snapshot hash changes because the control tree now includes the band and subsequent sibling indices shift; no design tokens changed.

## Out-of-scope files
None relative to the approved intent. The strict gate flags `product/AntiNotepad/SelfTest/Design.cs` because it accepts exact paths only, while the card explicitly allows one area file under `product/AntiNotepad/SelfTest/`. The managing session must replace that directory entry with the selected file; Codex left the card untouched as instructed.

## Plan deviations
None. The existing design snapshot hash was refreshed as necessary bookkeeping for adding the planned control; the focused check lives in that same selftest area file.

## Contracts
Added `sliding-overlay-top-band`: "Sliding bar modes reserve the top band above overlays and keep it a window-drag surface." No other rows changed.

## Lens review
Fresh `lens_reviewer`: **accept**, no proven findings. Behavior list matches the diff; input ownership, overlay replacement/closure, mode transitions, blur hit testing and prompt dismissal inspected. Native dragging/pointer routing and actual settings mode changes remain hand checks; the focused automated check exercises production layout helpers.

## Checks run
- Offline build: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A5-search-bar-overlap-check -p:NuGetAudit=false`: Build succeeded, 0 warnings, 0 errors.
- Captured `--selftest`: `CONTRACTS: 14 rows, 13 pass, 1 manual`; `SELFTEST PASS`.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A5-search-bar-overlap -Offline`: **GATE: FAIL solely on Scope**, for the directory-versus-file card mismatch above. Build PASS (0 warnings/errors), Selftest PASS, Seam PASS, Tokens PASS, Format PASS. LoopLint SKIP (no loop changes). Offline WARN is expected; Size WARN is 575 -> 604 lines in Layout (the same layout/input responsibility; no unrelated split).
- `git diff --check`: pass.
- Cleanup could not complete: command policy rejected recursive deletion of the verified, ordinary directories `build/A5-search-bar-overlap-check`, `build/A5-search-bar-overlap-gate`, and `build/A5-search-bar-overlap-gate-data`. They remain for managing-session cleanup; no further deletion attempts after the repeated policy failure. No app launched, stopped, installed, or quit; no commits, branch switches, or STATUS/card edits.

## Hand checks for the user
- Docked, Overlay, Float: open Search by icon, Ctrl+F, and pager count, with sliding bar initially shown and hidden. Query must clear the bar; type/select text and use search controls.
- Drag the top band with Search open, wait beyond the hide delay, then drag again. Esc and reopen. Switch modes with Search open, including return to Docked; no double reservation.
- Repeat Help clearance and dragging. Save-failure prompt: outside clicks (including the reserved band) still dismiss; Retry still works.
- Repeat at UI scale 0.8, 1, and 2. Visible placement, native dragging and actual pointer routing remain hand-test evidence; offline checks exercise control structure and helper layout only.
