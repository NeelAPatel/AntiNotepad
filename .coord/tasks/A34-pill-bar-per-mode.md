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
- `product/AntiNotepad/UI/PadBar.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/UI/PadWindow.TopBlur.cs`
- `product/AntiNotepad/UI/Shortcuts.cs`
- `product/AntiNotepad/UI/Kit.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/Theme/ChromeTokens.cs`
- `product/AntiNotepad/Theme/Paint.cs`
- `product/AntiNotepad/Model/Settings.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Bar.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
- `docs/shortcuts.md`
- `AGENTS.md`
- `tools/playground/a34-pill-bar-lab.html`
- `tools/playground/app-mock-faithful-2026-11-20.html`
- `tools/playground/index.html`

## For Codex
Role: terra at CODEX_EFFORT=high (several UI files with shared state; Plan fixed by the user). Read this whole card, `.coord/research/A34-edge-cases.md`, `.claude/rules/coding.md`, `AGENTS.md`, `docs/glossary.md`. Work only in this worktree. Never commit, never launch or kill AntiNotepad, no ports or browser. Build into `build/A34-pill-bar-per-mode-check`, run `--selftest` there, delete the folder after. Set `AVALONIA_TELEMETRY_OPTOUT=1`.
Visual truth: the approved lab `tools/playground/a34-pill-bar-lab.html` (same dimensions: corner squares = `ChromeTokens.PillHeight` 42, count and arrows 32, Float one pill with separators, hover colors p.Error for Delete and p.Accent for New note, no red at rest, dots hit area 32 tall). Its script holds the behavior table; read it. Real behavior stays in the app's own methods (`Navigate`, `DeleteCurrent`, `NewNote`, `ShowStack`, `OpenSlot`, `PromoteToSlot`).

### Structure (user's container paradigm, record it in comments and glossary text)
The bottom bar has three named containers: left (Delete), center (the pager bar, today `_pill`), right (New note). Build them once; only the pager content is rebuilt in `UpdateChrome`. Docked/Overlay: the three containers sit in one bar panel that is the child of the root / `_bottomLayer` (replaces reparenting `_pill` alone; update `ApplyBars`, `FadeChrome`, `OverlayPointer`, `BarsSleepTick`, `FloatPager`, the overlay hide in `PadWindow.Keys.cs`, `PadWindow.TopBlur.cs`). Float: the left and right buttons are placed inside the pill in the popup, 32 px, with separators (`PagerSeparator*` tokens) between the groups. Put the bar code in a new partial `UI/PadWindow.Bar.cs` (BuildPill moves there), with a small pure record or static method deciding content from (view kind, active pane, toggles, note/slot state) so selftests need no window: Standard to pager, slot pane to dots only, New note hidden in slot panes (container kept, empty), Delete label "Delete note" or "Clear slot N", enabled flags (Delete off on a sole blank stack note and on an empty slot; New note off when already on the trailing blank note).

### Behavior and settings
- Standard pager: `‹ count ›` only (count button height 32, click opens Search as today). Slot panes: the 9 dots only, button height 32 (token), current/filled/empty looks as today. Split: one bar following the active pane (`_active`); right pane active = slot content with both panes' current slots marked.
- Swap button: first icon in `_icons`, icon only; tooltip names the destination ("Show slots" / "Show stack") plus the bound key from `_shortcuts`; Standard to slots opens the last-used slot (kept in memory only), else the first filled, else slot 1; slots to Standard calls `ShowStack()`. In split it acts on the left pane. New shortcut `swap-view` (name "Swap stack and slots", section "Slots", scope Pad, unbound) in `UI/Shortcuts.cs`, dispatch in `PadWindow.Keys.cs`, row in `docs/shortcuts.md`.
- Delete debounce: a second Delete within 500 ms is ignored (put the value in `ChromeTokens`). `DeleteCurrent` itself stays as is; do not touch save, `Store`, `Note` or shutdown code.
- Settings (`Model/Settings.cs`): `ShowDeleteButton` (true), `ShowNewNoteButton` (true), `SlotDots` ("pill" or "quiet"; quiet = the dots with no capsule background, border or shadow behind them). Rows in `UI/SettingsWindow.cs` next to Bar style, using the existing Row/Toggle/Seg helpers.
- `Kit.IconButton`: add an optional hover glyph color; Delete = `p.Error`, New note = `p.Accent`, only on hover and keyboard focus. Names, tooltips with shortcuts via `_shortcuts.GetLabel`, and `AutomationProperties.Name` on every new button.
- New design values go in `Theme/ChromeTokens.cs` (corner size, count height 32, dot button height 32, debounce). The pill's measurements otherwise stay as they are.

### Tests (new `SelfTest/Bar.cs` plus updates in `SelfTest/UI.cs`; small capability checks, same style as neighbors)
Content table (Standard / slot / split-right / toggles off / empty cases); containers present and parented per bar style (reflection on the new fields); delete debounce; swap target rule; settings defaults and JSON round trip with older files lacking the keys; `swap-view` in the shortcut table and docs table (existing A29 checks must stay green); tokens (count 32, corner = PillHeight); the fade and hit-test checks in `SelfTest/UI.cs` now cover the whole bar (they reflect `_pill` today). Register the new file wherever the selftest run lists parts.

### Done means (offline only)
`dotnet build` clean, `AntiNotepad.exe --selftest` from the check build all pass, `pwsh -NoProfile -File tools/gate.ps1 -Task A34-pill-bar-per-mode -Offline` passes (Format, Tokens, Layers, scope against `## Files`). Report at `.coord/reports/A34-pill-bar-per-mode.md` with sections: Changed files, What I decided, Selftest output summary, Gate result, Not verified (visuals, hover colors, reveal timing, 320 px, split: the manager checks these in the dev copy). Files outside `## Files`: ask in the report, do not edit.

### Round note (manager, 2026-11-20)
Run 1 stopped without changes because it asked about `PadWindow.TopBlur.cs`. It is now in `## Files` (edit it as needed for the popup blur that uses `_pill` as its visual). Do not stop to ask scope questions: every file you need is in `## Files`; `docs/glossary.md` is NOT in scope (the container terms live in code comments only). Implement the whole Plan, build, run selftests and the gate, then report.

## Check
Round: A34-pill-bar-per-mode (build), r1 (design snapshot), r9 (review rework). Reports: `.coord/reports/A34-pill-bar-per-mode.md`, `-r1.md`, `-r9.md`, `.review.md`, `.gate.md`.
- **Gate:** PASS offline (build 0 warnings, selftest 36 of 37 contract rows pass and 1 manual, seam, tokens, format, scope, size). The online gate only failed on the NuGet vulnerability lookup (env, no network).
- **Codex review:** `VERDICT: rework` with 1 major (disabled Delete and New note not dimmed), 2 minor (save-failure card shadow lost in Float and Quiet; missing summaries). Risks cleared by the reviewer with probes: toast shadow, live settings, reveal/fade/hit testing, Float widths and reserved corners, split view, Delete debounce, A30 untouched.
- **Rework r9:** all three fixed (shared icon-button dimming at `Paint.DisabledIcon` 0.5, `PillShadow(p)` helper used by pill, toast and card, summaries and `Kit.cs` header); I reviewed the diff and re-diffed the design dump: only the Delete button face Opacity 0.5, once per palette (8), so I updated the pinned `DesignHash` (manager one-line fix).
- **Bounds:** within plan: three containers, mode-specific pager, corner buttons (42 / 32 in Float), swap button first in `_icons`, `swap-view` shortcut, 3 settings, debounce, dot and count targets (32), split follows the active pane. Justified: `PadWindow.TopBlur.cs`, `SelfTest/Design.cs`, `Theme/Paint.cs` joined `## Files` (popup blur visual, pinned snapshot, disabled token). No change to save, Store, Note or shutdown code.
- **Report format:** Codex's first report lacks Change list / Behavior changes / Plan deviations headings (r9 has them); not worth a round, noted.
- **Misses:** (1) card had no `## Files` section, so approve.ps1 failed: found at Approve, cause plan, caught by tools/files-overlap.ps1, should have caught: card writer, lesson: add `## Files` bullets when writing the Plan. (2) Brief named `PadWindow.TopBlur.cs` outside Files; Codex run 1 stopped: cause plan, lesson: Files list is the brief's file list, check them against each other. (3) Codex edits could not be gate-formatted offline (a `dotnet format` whitespace line twice): cause code, lesson: run `dotnet format whitespace --include <file>` before reporting.
- **Noticed:** none beyond the lab; `tools/playground/index.html` is the token playground, not a lab index (the earlier link edit was reverted).
- **Not verified (hand test):** pointer reveal transitions, hover colors (red / accent), corner squares vs window corner radius, 320 px fit, Float popup and Overlay reveal, split view, light and dark.

## Hand test
User, 2026-11-20 (verbatim, first pass): Delete safety and Undo "works"; Standard view, corner hover colors and swap "works". Dimmed states: "the states between active and dim isn't animated. it just snaps between them." Toggle: "the toggle between standard and permanent notes is trippy. I click and then the toolbars fade out when it should just stay there. If im hovering over any of the buttons on the toolbars, they should stay visible." "Scrolling shouldn't kick me out of permanent notes. instead it should scroll among the slots of permanent notes." Split: "swap button shoudl change active pane. but only one instance of permanent can occur at a time. standard standard / standard permenant / permanent standard: 3 valid situations", "two bars should be shown and delete/new note should follow the active container".
Triage: first three = code (r10 brief). Split items = scope change, proposed as its own lab-first item (right pane can show the stack, two bars, per-pane swap).

## Plan addendum (user, 2026-11-20)
User (verbatim): "separate item but we'll do the ground work of making this new bottom bar future proof as much as possible". Scope: after r10, a structure round (r11) with no visible change: the bottom bar becomes one `PadBar` object per pane (new `UI/PadBar.cs`) that owns its three named containers (left, center, right) and registers its controls as ordered items (Delete, pager bar, New note) so later items (split's second bar, status pill, bars-edge-reveal) add or move controls without restructuring. The bar reads nothing from `_active`: it gets a context record (pane view, other pane's slot, toggles, note state, palette, labels) and injected action callbacks. `PadWindow` holds a list of bars (one today), placement code (Docked, Overlay, Float) handles N bars, swap takes a pane argument. Pure refactor: same pixels and behavior; selftests prove the registry and context; the pinned design hash must stay unchanged. `UI/PadBar.cs` joins `## Files`.
