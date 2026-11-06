# A5-search-bar-overlap: search view never sits under the top bar, and the bar still drags the window

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build; Codex reads the whole card and
follows "For Codex". Codex never edits this file; Check and Hand test are filled by the sub-session afterwards.

Tier: Medium · bug-unproven-cause (rules: `docs/loop.md`)
Branch: `fix/A5-search-bar-overlap` in `.claude/worktrees/fix/A5-search-bar-overlap` (created at Approve)

## Request
User, 2026-11-05, A4n-scale-lab hand test (screenshot): "i think we permanently move the elements down enough so it doesnt overlap regardless of Docked mode or Overlay/Floating mode. I think this should be a fix/ to add to the backlog".
Same view: "in search view, you can no longer access the top bar header to move window arround".
Overlay: in Overlay mode, opening search lets the top bar (badge, Saved label, icons) cover the search field.
User, 2026-11-06 (via Master 4): ID A5; own worktree and branch; follows loop v5.1.0; merges to dev with the user's consent; a plan of action first.

## Findings
- **Kind and why:** bug (pre-existing, not from design tokens).
- **Now:** (manager read of the code, audit pending)
  - `_content` is a Grid: row 0 = bar (40), row 1 = panes + `_overlayHost` (`UI/PadWindow.Layout.cs:23,117`). Docked: search sits in row 1, so no overlap and the bar stays draggable.
  - Sliding modes (Overlay, Float): row 0 is 0 high (`Layout.cs:337`), so `_overlayHost` starts at y=0; `SearchOverlay` top margin is 14 (`UI/SearchOverlay.cs:47`) against a 40 px bar. The bar lives in `_topLayer`, drawn above `_content`, so when it slides in it covers the field.
  - While an overlay is open `OverlayPointer` returns early (`Layout.cs:359`), so the bar is not revealed or re-hidden by moves. A bar that is shown stays over the field. When it is hidden its layer is not hit-testable, and `_overlayHost` has no title-bar role, so there is nothing to drag.
- **Reproduction (bugs):** exact code paths in the audit, section 3 (code-derived; live behavior is the user's screenshot). Confirmed: row 1 starts at y=0 in sliding modes, `_topLayer` draws above, search field starts at y=14 under the 40-unit bar; hidden bar layer is not hit-testable, `OverlayPointer` returns early while an overlay is open, `_overlayHost` has no title-bar role. Correction: a shown bar can still be hidden by `_barsSleep` (no overlay guard).
- **Where:** `UI/PadWindow.Layout.cs` (host clearance, drag band), selftest + `SelfTest/Contracts.cs`; `UI/PadWindow.Keys.cs` only if the band follows overlay open/close; `SearchOverlay.cs` need not change. Help overlay and the save-failure prompt share the host: same overlap for Help; the prompt dismisses on outside clicks (collision risk with a drag band). Settings and toast are separate. Keep clearance in logical units (the scaler already scales it).
- **Risk:** input ownership in the top band (drag vs icon clicks vs prompt dismissal); gates: lens-review, new CONTRACTS row, hand test.
- **Source:** `.coord/reports/A5-search-bar-overlap-audit.md` (READY: yes; tier Medium confirmed)

## Plan
- **After:** in Overlay and Float the search view starts below a reserved 40-unit band (query at about y=54, like Docked), so the bar never covers it whether shown or hidden. That band always drags the window, even with the bar hidden. Docked is unchanged.
- **Expected time:** about 1.5-2 h from approval to merge (one Codex run, review, hand test); similar small UI fixes in `.coord/logs/usage.tsv`. Under the hour is not realistic because of the three-mode hand test.
- **Must not change:** Docked layout; bar reveal/hide timers and motion; save-failure prompt dismissal on outside click; native window styles.
- **Scope questions:** (1) Help overlay gets the same clearance and drag band (recommended). (2) The band is a blank always-on drag area; the visible sliding bar itself is not made reachable during search.
- **Test plan, code:** one selftest plus a CONTRACTS row: Docked has no double reservation, Overlay/Float reserve `DockedRowHeight`, the band has the title-bar role and sits below the chrome layers, mode switch with search open returns to the right layout.
- **Test plan, UI:** Docked, Overlay, Float x open by icon, Ctrl+F, pager count x bar shown and hidden: field never covered; drag from the top band; wait past the hide delay; Esc and reopen; Help overlay; save-prompt outside-click still dismisses; UI scale 0.8 / 1 / 2.
- **Gates:** `tools/gate.ps1` (build, selftest, scope); Codex lens-review (shared state, hit testing); new CONTRACTS row; lab mockup approved before the build; hand test.
- **Needs from you:** approve the layout mockup in the plan and the two scope answers; the hand test.

## Files
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/PadWindow.Keys.cs` (only if needed)
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest/Design.cs`

## Contracts
- New: "Sliding bar modes reserve the top band above overlays and keep it a window-drag surface."

## Approve
User, 2026-11-06: "Approve" (plan as shown, including the scope choices: Help gets the same clearance and drag band; save-prompt outside-click dismissal kept; blank always-on drag band).

## For Codex
Role: director (`docs/agents.md`), because the fix touches input ownership and hit testing and the bug cause is unproven. Product `product/AntiNotepad`. The branch is already
checked out: never switch, merge or commit (you cannot write to `.git`). Read `AGENTS.md`, then the files above and the audit `.coord/reports/A5-search-bar-overlap-audit.md`. Line
numbers are from the current tree; re-read before editing.

Constraints:
- Build exactly the Plan's "After" for the Files above. No drive-by refactors, renames, icon/token/style edits outside it.
- If the Findings turn out wrong (the bug does not reproduce, the cause is elsewhere, the plan cannot work), stop and
  report that instead of building something else.
- Build into `build/A5-search-bar-overlap-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A5-search-bar-overlap -Offline`; the manager
  re-runs it online.
- Never launch or kill AntiNotepad. Never run `--quit`. Never edit `.coord/STATUS.md` or this card.
- `A6-note-switch-instant-swap` runs in parallel and also edits `SelfTest/Contracts.cs`: add only your own row, touch no other lines there.

Done means (offline checks only: build, selftest, gate, script checks, files; never ports, servers, browser or screen):
- In Overlay and Float modes the search view (and the Help overlay) starts below a reserved band of `DockedRowHeight` (40 units), so the top bar never covers it whether shown or hidden. Docked is unchanged (no double reservation).
- That band is a window-drag surface (title-bar role) even with the bar hidden, sits below the chrome layers, and the save-failure prompt still dismisses on outside click.
- A mode switch with search open returns to the right layout.
- The Plan's code tests exist and pass: one selftest plus the CONTRACTS row "Sliding bar modes reserve the top band above overlays and keep it a window-drag surface."; build 0 warnings, `SELFTEST PASS`, the gate passes.

Before you report (director only): spawn the `lens_reviewer` agent on your diff (it reads
`.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why not.

Report: `.coord/reports/A5-search-bar-overlap.md`, short, with exactly these sections:
1. **Summary**: two or three sentences.
2. **Change list**: one row per changed block: `file:line-range | what changed | which Plan line it serves`.
3. **Behavior changes**: every changed public method result or meaning, new or changed shared state, timing, default, or
   setting effect. Write "None" only if true; the review checks this against the diff.
4. **Out-of-scope files**: file and why, or "None".
5. **Plan deviations**: anything done differently from the Plan, and why, or "None".
6. **Contracts**: rows added or changed, or "None".
7. **Lens review**: findings and what you did with each.
8. **Checks run**: build, selftest, gate result lines.
9. **Hand checks for the user**: the Plan's UI checks, plus anything new, or "None (not visible)".

## Check
Round r2 (whitespace-only rework of round 1). Reviewer: the builder session, renamed.
- **Gate (r2, `-Offline`):** GATE PASS: build 0 warnings, SELFTEST PASS (CONTRACTS 14 rows, 13 pass, 1 manual), seam, tokens, format, scope PASS. Size WARN: Layout 575 -> 604 lines (same layout/input job, accepted). The online run fails Build only on NU1900 (dotnet cannot reach the NuGet vulnerability feed here, though nuget.org answers 200): cause `env`; the same code builds with 0 warnings when the audit is off.
- **Review:** Codex `lens_reviewer`, spawned by the director on round 1: accept, no proven findings. r2 is whitespace only, so no second review.
- **Contracts:** `sliding-overlay-top-band` exists and passes; the focused layout check is in `SelfTest/Design.cs`.
- **Bounds check:**
  - Search and Help clear a 40-unit band in Overlay and Float; Docked unchanged: within plan.
  - Band has the title-bar role, sits below chrome, and is off for the save-failure prompt so outside-click dismissal stays: within plan.
  - Design snapshot hash refreshed for the added control: justified (forced by the new control; no token changed).
  - `Design.cs` as the one area file: within plan.
  - No out-of-scope files; no timer, motion, settings or window-style change.
- **Noticed:** None.
- **Misses:**
  - Scope fail at gate r1 | cause `plan`: Files listed the `SelfTest/` directory | caught by gate Scope | should have caught: Plan (exact paths) | lesson: name the exact area file in Files.
  - Format fail at gate r1 | cause `code`: two properties per line (`PadWindow.Layout.cs:101-102`) | caught by gate Format | should have caught: Codex's own format check | lesson: brief "one property per line" for initializers.
  - Scope fail at gate r2 | cause `plan`: round brief said "none" under Files, the gate needs a list | caught by gate Scope | should have caught: round template | lesson: always list a file under round Files.

## Hand test
Build checked: dev copy from the worktree, `build/dev/AntiNotepad.exe` (PID 26072), DATA `build/testdata`, app log `build/testdata/antinotepad.log`; badge shows branch `fix/A5-search-bar-overlap` (read from code, not from the screen).
User, 2026-11-06: "perfect, its good enough" (Passed).
