# A34-pill-bar-per-mode: the pill bar shows what each note mode needs

Item card (`docs/loop.md`). Claude fills Request to Approve before any build; Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB (logs go in reports).

Tier: Medium · lab first (visible)
State: request
Branch: `feat/A34-pill-bar-per-mode` in `.claude/worktrees/feat/A34-pill-bar-per-mode` (created at Approve, from dev's tip)

## Request
User via Master 7, 2026-11-20: start `idea: pill-bar-per-mode`, with its twin `idea: permanent-view-swap-button`. This is the first step of the bottom-bar revamp; `delete-note-button`, `bars-edge-reveal`, `status-pill-redesign` and `split-view-modes` are meant to come after it, so the lab should leave room for them.
- **Goal:** the pill bar shows pagination only in Standard Notes and dot selectors only in Permanent Notes; the Permanent selector leaves the bar, and a new top-right button swaps between Standard and Permanent view (twin idea; the lab decides whether it stays one item).
- **User expects (2026-11-20):** a localhost website of mockup ideas and recommendations. Expect this to take time and several rounds before any code.
- **Done check (after the lab is approved):** (1) each mode's bar matches the approved mockup; (2) the swap control switches views and shows the current one; (3) existing keys and the A29 shortcut table stay valid (add a swap action there if the lab includes one); (4) selftests cover mode-specific bar content; (5) gate passes.
- **Findings first:** inventory today's pill bar(s): what shows in each mode, which bars exist (bar 1 and 2), where Permanent selection, pagination, dots and the top-right area live in `UI/` and `Editor/`. Use the window part names in `docs/glossary.md` and note where `D1-ui-region-names` (paused) would help.
- **Lab (the main deliverable):** Codex builds a localhost page under `tools/playground/` (labs go to Codex, `docs/agents.md`) with several layout options per mode, a recommendation with reasons, and how each option leaves space for a delete button, edge reveal, a notice pill and split view's two bars. Show real screenshots of today's bars. Stop at the lab for the user to react; iterate until they approve one.
- **Constraints:** visible change, so lab-first and the user approves the lab before implementation; no slowdown of show/hide or typing; Codex offline only; follow `.claude/rules/coding.md`; hand test in your slot's dev copy only. A30 stays open: do not touch save or shutdown code.

## Findings
Medium confirmed (editor-chrome, shared view state). Audit: `.coord/logs/A34-pill-bar-per-mode.quick.last.md`.
- Top row = metadata + `_icons` (Search, Split, Pin, Settings, Hide), top-right is `_icons`; no view-swap button today (`PadWindow.Layout.cs:34-148`).
- One `_pill` (`BuildPill`, Layout.cs:496) serves both views: Standard = prev/next arrows + "n / count" label + slot dots; Permanent (`Data.ViewSlot>0`) = "stack" return control + slot dots. Selector is in the pill.
- `Settings.Bars` docked/overlay/float change placement and reveal only (Layout.cs:537-588). Float pill lives in a popup (`_floatPager`).
- View switch today: `ShowStack()` / `OpenSlot()` (Navigation.cs:127-149), keys in Keys.cs:81-108. Editor insets tied to bar style (PadView.cs:1033).
- Risk: hit testing and reveal timers when pill content differs per view; top-row width with a new button.
- Screenshots of today's bars need the dev copy on screen (computer use, needs the user's go); lab uses faithful HTML redraws until then.

## For Codex (lab)
Role: terra, a new lab page. Build `tools/playground/a34-pill-bar-lab.html` (single file, offline, no deps; match style of `a31-list-default-lab.html`) and add it to `tools/playground/index.html`. Read `.coord/tasks/A34-pill-bar-per-mode.md` and the Layout.cs parts above for exact looks (ChromeTokens).
- Section 0: "Today" faithful redraws of both bars (Standard, Permanent), labeled as redraws.
- Section 1: per mode, 4 options for the bottom pill (Standard: pagination only), (Permanent: dots only); each with a pros/cons line.
- Section 2: 4 options for the top-right swap button (placement in `_icons`, glyph/label showing the current view, state when active).
- Section 3: combined previews (Standard + Permanent, light/dark, docked/overlay/float) of the recommended set.
- Section 4: "Room for later": for each option, show how it leaves space for delete-note button, edge reveal, notice pill, split view's two bars.
- Recommendation box with reasons. Clickable swap toggles views in the mock.
Done means: file exists, valid HTML, no external requests, index link added. No product code, no .git. Report at `.coord/reports/A34-pill-bar-per-mode.md`: files, options list.

## User reactions (verbatim, 2026-11-20)
- R3: "leaning towards S1-P1/S3-P3 + T1"; wants Last modified timestamp, Delete button, Skip to first/last, New note on the bar; "New Note on bottom right, Delete on bottom left". Scope grows over `idea: delete-note-button`; decide at Plan.
- R4 (2026-11-20): wants icons for Delete and New note. Date slot = Created: when the note last became non-empty (erase all, 5 s soft save, type again: new Created). Last modified is redundant, so it becomes an optional Created slot (a Setting). Lab called unfriendly, poor quality, not matching the app. Open for Plan: re-stamping Note.Created is model/save-adjacent and may collide with A30 (do not touch save/shutdown); decide the hook at Plan.
- R5 (2026-11-20): hovers: Delete icon turns pastel red (palette Error), New note icon turns the selected accent; separators toggle for one-pill modes; defaults per bar style: Docked/Overlay = Corners, Float = One pill; Standard = S1 pill; Permanent = P1 (quiet), P3 quiet optional to test live. User wrote "E3 Corners": read as E1 corners.
- R6 (2026-11-20): agreed UX 1-4, 7, 8 (New note hidden in Permanent with its corner kept empty; Clear slot N tooltip, dimmed on empty; no delete confirm; red only on hover; corner buttons share the pager layer; taller dot hit area). OUT of scope: Skip first/last, Created date (user: "no need for date at this time"). Scope now: pager per mode + swap button T1 + Delete + New note corners. Split into steps: open (my advice: one item is now small enough).
- R7 (2026-11-20): corner Delete/New note buttons become pill-height squares (42 px, Corners layout only); count button hover target gets the arrows height (32, was 20 in ChromeTokens count Plain). User sent an MSPaint mockup.

## Plan
Lab approved by the user 2026-11-20 (final: `tools/playground/a34-pill-bar-lab.html`, kept as `app-mock-faithful-2026-11-20.html`). Edge cases and the UX pass: `.coord/research/A34-edge-cases.md`.

**Now:** one `_pill` at bottom center for both views (`BuildPill`, Layout.cs:496): `‹ count › | 9 dots`; count box 20 px tall; no Delete or New note button; top-right `_icons` has no view swap.

**After:**
- **Standard:** pill = `‹ count ›` only. **Permanent (and any slot pane):** pill = the 9 dots only, taller hit area. Count button hover target 32 px (was 20), same as its arrows.
- **Corner buttons** (Delete bottom-left, New note bottom-right): Docked/Overlay = pill-height squares (42) in the same layer as the pill (they fade, slide and hide together); Float = both inside the single floating pill (32 px, separators on). Delete hover = palette error red; New note hover = accent.
- **Swap button** (icon only, top-right, first in `_icons`): shows the current view; Standard to Permanent opens the last-used slot (else first filled, else slot 1); Permanent to Standard calls `ShowStack()`. New shortcut action `swap-view` (unbound by default) in the A29 table.
- **Rules:** New note hidden in a slot view but its corner stays reserved (no shift on swap). Delete reads "Clear slot N" in a slot and is dimmed when there is nothing to delete; no confirm, Undo toast stays. Split: one bar, follows the active pane (right pane = slot content; Delete and New note act on the active pane); swap changes the left pane only. 2 x 2 split bars belong to `split-view-modes`.
- **Settings (new):** Show Delete button (on), Show New note button (on), Slot dots: Pill / Quiet (Pill; Quiet = no capsule behind the dots, the lab's P3; drop it if you want fewer settings).
- **Out of scope:** Skip first/last, Created date (user, 2026-11-20); bars-edge-reveal; status-pill-redesign; save and shutdown code (A30).

**Edge cases (decided, see research file):** double-click on Delete is debounced (a second delete within 500 ms is ignored, so one slip cannot trash two notes); New note dimmed when already on the trailing blank note; Delete dimmed on a sole blank note and on an empty slot; bar buttons persistent, only the pager content is rebuilt (show/hide speed); Search open hides the whole bar; narrow 320 px fits (246 px of bar); window corner radius vs 42 px corner squares checked at hand test.

**Tests (small --selftest checks):** pure bar-content table (Standard / slots / split-right / toggles / empty cases); button parents per bar style; delete debounce; swap target rule; settings defaults and round trip; `swap-view` in the shortcut table (A29 checks); tokens (count 32, corner = PillHeight); fade/hit-test of the whole bar (updates the `_pill` reflection checks in `SelfTest/UI.cs`).

**Gates:** `tools/gate.ps1 -Task A34-pill-bar-per-mode` + Codex lens review; hand test in my slot dev copy (all 3 bar styles, split, 320 px, theme light/dark, fade chrome). Tier: Medium (editor chrome, new settings).

**Files:** `UI/PadWindow.Layout.cs`, new `UI/PadWindow.Bar.cs` (BuildPill and corner buttons move out; Layout.cs is 819 lines), `UI/PadWindow.Navigation.cs` (SwapView, delete debounce), `UI/PadWindow.Keys.cs`, `UI/Shortcuts.cs`, `docs/shortcuts.md`, `UI/Kit.cs` (hover color), `Theme/ChromeTokens.cs`, `Model/Settings.cs`, `UI/SettingsWindow.cs`, `SelfTest/UI.cs` (+ a Bar selftest part), `AGENTS.md` (layout line). Not touched: `Store`, `Note`, save, shutdown.

## Approve
User, 2026-11-20 (verbatim): "Plan approved. Just FYI i look at this new layout as: Bottom bar: left container (Delete button), center container (Pill bar aka pager bar), right container (New note button). That container paradigm will help in the future when we want to expand on the bottom bar's layouting."
Build note: model the bottom bar as three named containers (left, center, right) so later items can add controls without restructuring; Float keeps the same three groups inside the one pill. Glossary: add the terms (bottom bar, left/center/right container, pager bar) in D1-ui-region-names.

## Files
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/PadWindow.Bar.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/UI/Shortcuts.cs`
- `product/AntiNotepad/UI/Kit.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/Theme/ChromeTokens.cs`
- `product/AntiNotepad/Model/Settings.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Bar.cs`
- `docs/shortcuts.md`
- `AGENTS.md`
- `tools/playground/a34-pill-bar-lab.html`
- `tools/playground/app-mock-faithful-2026-11-20.html`
- `tools/playground/index.html`
