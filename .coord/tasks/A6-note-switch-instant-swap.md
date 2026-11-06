# A6-note-switch-instant-swap: switching notes plays an exit then arrival instead of an instant swap

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build; Codex reads the whole card and
follows "For Codex". Codex never edits this file; Check and Hand test are filled by the sub-session afterwards.

Tier: Medium · editor-feature (rules: `docs/loop.md`)
Branch: `fix/A6-note-switch-instant-swap` in `.claude/worktrees/fix/A6-note-switch-instant-swap` (created at Approve)

## Request
2026-11-02, A4e-motion-tokens hand test (user, relayed by Master 3): "okay lets put this issue as a potential bug on the roadmap".
STATUS row `idea: note-switch-instant-swap`: switching notes by scroll or swipe shows the next page at once for part of the
change, before the slide animation. Predates design tokens.
2026-11-06 (user via Master 4): numbered A6; own branch and worktree; a plan of action before any build; merges to dev with the user's consent.

## Findings
- **Kind and why:** bug (polish-level); the swipe pulls the old page, then the new page appears without the old one leaving.
- **Now (master scout, no Codex audit yet):**
  - `NoteSwipe.Fire()` (`UI/NoteSwipe.cs:211`) resets the pulled pane (`RenderTransform = null; Opacity = 1`) and calls `navigate(...)`.
  - `PadWindow.Navigate` (`UI/PadWindow.Navigation.cs:51`) -> `BindPanes(direction)` (`:21`) -> `_left.Bind(note)` swaps the text at once, then `_left.Arrive(direction)` (`Editor/PadView.cs:440`).
  - `Arrive` runs `Anim.Run(NoteArrive = 200 ms)`: shift 34 px to 0, opacity 0.25 to 1. `Anim.Run` applies `step(0)` synchronously (`Motion/Anim.cs:27`), so there is no one-frame full-opacity flash; the cause note's "swaps before the animation" is correct only in that the new text is bound before the arrival, which is by design.
  - What the user sees: the old page is dragged part of the way (up to about 40%, faded), then it vanishes (reset to centre inside the same UI turn) and the new page pops in 34 px off at 25% opacity. The old page never slides out, so the change reads as an instant swap. Same in the Settings preview (`UI/MotionPreview.cs:114`).
- **Reproduction:** read from code only; hand-confirm in the dev build at the start of Build (swipe past the trigger, watch the old page).
- **Where:** `UI/NoteSwipe.cs`, `Editor/PadView.cs` (`Arrive`), `UI/PadWindow.Navigation.cs` (`Navigate`, `BindPanes`), `Motion/MotionTokens.cs`, `UI/MotionPreview.cs` (same callback shape), `SelfTest/UI.cs` (NoteSwipe checks).
- **Risk:** wheel clicks fire repeatedly (re-arm after 110 ms in progress mode); a delayed swap must not drop or double a navigation. Split view and slot views also go through `BindPanes`.
- **Source:** master scout (a Codex audit was not run; see Plan "Scope questions").

## Plan
- **After:** the lab decides. Candidates: (1) exit then arrive: the pulled page finishes sliding out from where it is, then the new page arrives; (2) old page frozen as an image slides out while the new one arrives at once, no delay; (3) keep today's instant text swap but start the arrival from the pulled offset so the motion is continuous.
- **Expected time:** about 1.5 h from approve to merge (lab round included); no past run is closer.
- **Must not change:** swipe feel settings, trigger distances, wheel pay-out, Split and slot behavior, the 200 ms `NoteArrive` unless the lab says otherwise.
- **Scope questions:** which variant (picked at the lab); skip the Codex audit because the code path is proven by reading?
- **Test plan, code:** `NoteSwipe` selftest: a fired swipe calls `navigate` exactly once; a second fire during an exit completes the first navigation and is not lost. `CONTRACTS` row: a swipe never leaves the pane transformed or faded after the change settles.
- **Test plan, UI:** lab build with variants 1-3; user swipes (touchpad and Shift+wheel) in both directions, fast repeat swipes, split view, and the Settings preview; picks one.
- **Gates:** `tools/gate.ps1` (compile, selftest, format, scope); Codex fresh review (timers and re-entrancy); lab build (visible design change); hand test.
- **Needs from you:** pick the variant after the lab.

## Files
Only these files may change (exact paths; `tools/gate.ps1` reads this list). Any other file needs a line under
"Out-of-scope files" in the report.
- `product/AntiNotepad/UI/NoteSwipe.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/UI/MotionPreview.cs`
- `product/AntiNotepad/Motion/MotionTokens.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`

## Contracts
- A note switch never leaves the pane transformed or faded once the change settles.

## Approve
User, 2026-11-06: "Approve" (plan as shown: lab with three variants, Codex audit skipped).

## For Codex
Role: terra (medium), because the change touches timing and re-entrancy in the swipe path. Product `product/AntiNotepad`. The branch is already
checked out: never switch, merge or commit (you cannot write to `.git`). Read `AGENTS.md`, then the files above. Line
numbers are from the current tree; re-read before editing.

Constraints:
- Build exactly the Plan's "After" for the Files above. No drive-by refactors, renames, icon/token/style edits outside it.
- If the Findings turn out wrong, stop and report that instead of building something else.
- Build into `build/A6-note-switch-instant-swap-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A6-note-switch-instant-swap -Offline`; the manager re-runs it online.
- Never launch or kill AntiNotepad. Never run `--quit`. Never edit `.coord/STATUS.md` or this card.

Done means (offline checks only):
- The chosen variant (lab: all three behind one switch) is in place.
- The Plan's code tests exist and pass; build 0 warnings, `SELFTEST PASS`, the gate passes.

Report: `.coord/reports/A6-note-switch-instant-swap.md`, sections as in `.coord/templates/item.md`.

## Check
Round 1 (lab, three variants): gate PASS offline (after one format fix by the manager); online NuGet audit not run (no network in the manager shell).
User hand test of the lab, 2026-11-06: "Variant 2 is best. lock it in." Round 2 (`-r2`) removed the lab and kept variant 2 only.
Round 2: gate PASS offline; Codex fresh review `.coord/reports/A6-note-switch-instant-swap-r2.review.md`: no proven findings, VERDICT: accept.
Bounds check: requested (stop the instant swap) = done (old page slides out while the new one arrives); Files within the approved list; Plan deviation (variant 1 binds at once under a frozen image) justified, no user decision needed; lab removed as approved.
Noticed: none. Misses: round-2 brief lacked a Files list, so the first gate run failed Scope (cause plan, fixed).

## Hand test
Override (user, 2026-11-06, to Master 4, relayed by Master 4): "Merge A5 and A6, both into dev, then run a new build". Final variant-2 build not hand-tested on the branch; hand test moves to the dev build after merge (rule: hand test before Merge, docs/loop.md). The lab (three variants) was hand-tested by the user, who picked variant 2.
