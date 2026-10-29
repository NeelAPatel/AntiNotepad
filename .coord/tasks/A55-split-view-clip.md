# A55-split-view-clip: clip each split pane at the divider

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Go before any build; Codex reads the whole card and
follows "For Codex". Codex never edits this file; Check and Hand test are filled by the sub-session afterwards.

Tier: Medium (bug without a proven cause; editor rendering) (rules: `docs/loop.md`)
Branch: `feat/A55-split-view-clip` in `.claude/worktrees/A55-split-view-clip`

## Request
User, 2026-10-28 (A44a-native-facade hand test, recorded in STATUS Backlog as old A57): "in split view, scrolling text
overlaps across the divider; the divider should crop the visual of each pane."
Plan row Scope (user order, 2026-10-29): "In split view, text scrolls across the divider; clip each pane at the divider."
User go to start, 2026-10-29: "Go, start it".
User at Plan, 2026-10-29, with screenshots in `.coord/tasks/A55-split-view-clip-shots/`: "Image 1: Right side trying to
swipe to next page / Image 2: Left side trying to swipe to previous page / Expectation: The grapic should not go over the
line / Image 3 is how i expect it to look" (1-right-swipe-next.png, 2-left-swipe-prev.png, 3-expected.png).

## Findings
- **Kind and why:** bug; in split view a pane's content can paint over the divider into the other pane, and nothing crops
  it at the pane's edge.
- **Now:** both PadViews and the divider sit directly in one three-column grid (`UI/PadWindow.Layout.cs:90-98`, columns
  `"*,1,*"` in split). Horizontal swipe / Shift+wheel and keyboard note arrival move the whole PadView with a
  TranslateTransform (`UI/NoteSwipe.cs:143-152,177-183`; `Editor/PadView.cs:441-450`). No stationary parent clips at the
  column edge, so a pulled right note slides over the divider into the left one. The divider is only a painted 1-px Border.
- **Reproduction (bugs):** exact code path (no probe run): split on, right pane focused, small negative horizontal delta ->
  `NoteSwipe.Progress` -> `Show(-0.2 * width)` -> at width 320 the pane moves 64 px left, 28 px past its column edge
  (audit section 3). Confirmed by the user's screenshots (2026-10-29): right pane swiped toward next overlaps the left
  pane; left pane swiped toward previous overlaps the right pane. Both are sideways swipes, so scope question 1 is
  answered: sideways only. Image 3 shows the expected crop at the divider.
- **Where:** `UI/PadWindow.Layout.cs` (pane construction), `UI/PadWindow.Navigation.cs` (right pane visibility on split
  toggle). Untouched but affected: NoteSwipe timers/spring-back, PadView arrival animation, top blur (reads pane transforms
  and visibility, `UI/PadWindow.TopBlur.cs`), MotionPreview (own clip frame).
- **Risk:** clipping on the moving element instead of a stationary parent (looks fixed, is not); host visibility or
  sizing out of step with split state (right pane, focus or top blur wrong after toggling split).
- **Source:** `.coord/reports/A55-split-view-clip-audit.md` (READY: yes, f17b938).

## Plan
- **After:** each pane sits in its own stationary clipping host in its existing column. Swipe and arrival motion stay
  exactly as they are, but a pane's text, caret, selection and decorations stop at its column edge and never paint over the
  divider or the other pane. Single view: the right host hides with the right pane.
- **Must not change:** divider look, height and its gap above the pager (`split-divider-above-pager`); swipe/arrival
  distances, timings and fades; note binding, focus and per-note undo; top blur; MotionPreview; single-view look. No
  padding or background on the hosts.
- **Scope questions:** (1) Answered by the user's screenshots: sideways swipes in both panes; up/down stays a hand check
  only. (2) Should the outer window edges clip the same way in single view? Recommendation: no, out of scope (not asked).
- **Test plan, code:** new CONTRACTS row `split-pane-clipping`: built through the production layout code, both panes have a
  stationary clipping parent in columns 0 and 2 around the divider; the right host's visibility follows split on/off; a
  translated pane does not move its host's bounds. Pixel containment stays a manual part of the row if the selftest cannot
  render. `split-divider-above-pager` keeps passing.
- **Test plan, UI (hand test in the dev copy):** dense notes with text near both edges in both panes.
  0. Repeat the user's screenshots 1 and 2 (same notes): the result looks like screenshot 3 on both sides.
  1. Right pane focused: slowly pull it toward the divider (touchpad and Shift+wheel), let it spring back: no text crosses
     the divider. Same for the left pane pulled toward the right.
  2. Commit a swipe in each pane, and change notes with the keyboard: arrival motion looks as before, cropped at the divider.
  3. Caret and a selection near the inner edge while pulling: cropped too.
  4. Plain up/down scrolling in each pane: nothing crosses.
  5. Split off and on, resize the window: right pane, focus, divider and pager look as before; top blur still shows.
- **Gates:** `tools/gate.ps1` (build 0 warnings, selftest, LAYERS, CONTRACTS, seam, scope = the Files below, size);
  reproduction + new CONTRACTS row (the overlap path, and that it cannot come back unseen); Codex fresh lens review
  (transform vs host coupling, visibility in step with split, blur and timer consumers untouched); hand test (only the
  screen shows the crop). Not lab-first: a bug fix restoring the intended look, no new design.
- **Needs from you:** an answer to scope question 1 (and 2 if you disagree), then go.

## Files
Only these files may change (exact paths; `tools/gate.ps1` reads this list). Any other file needs a line under
"Out-of-scope files" in the report.
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/SelfTest.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`

## Contracts
- Keep `split-divider-above-pager` passing.
- New `split-pane-clipping`: in split view each pane is clipped at its own column edge by a stationary host; a translated
  pane never paints over the divider or the other pane; the right host hides in single view.

## Go
User, 2026-10-29: "Go" (plan as written; scope question 1 answered by the screenshots: sideways only; scope question 2
left at the recommendation: single-view outer edges out of scope).

## For Codex
Role: director (gpt-6.1-sol medium), delegating to a worker (gpt-6-luna xhigh). Product `product/AntiNotepad`. The branch is already
checked out: never switch, merge or commit (you cannot write to `.git`). Read `AGENTS.md`, then the files above. Line
numbers are from the current tree; re-read before editing.

Constraints:
- Build exactly the Plan's "After" for the Files above. No drive-by refactors, renames, icon/token/style edits outside it.
- If the Findings turn out wrong (the bug does not reproduce, the cause is elsewhere, the plan cannot work), stop and
  report that instead of building something else.
- Build into `build/A55-split-view-clip-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A55-split-view-clip -Offline`; the manager
  re-runs it online.
- Never launch or kill AntiNotepad. Never run `--quit`. Never edit `.coord/STATUS.md` or this card.

Done means:
- Each PadView has a stationary clipping parent in its split column; swipe/arrival transforms stay on the PadView and are
  unchanged; the right host's visibility follows split state.
- CONTRACTS `split-pane-clipping` exists and passes (or is marked manual for pixel containment only, with the structural
  checks passing); `split-divider-above-pager` still passes.
- The Plan's code tests exist and pass; build 0 warnings, `SELFTEST PASS`, the gate passes.

Before you report (director only): spawn the `lens_reviewer` agent on your diff (it reads
`.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why not. Workers without spawning: run the same
checklist yourself and say so.

Report: `.coord/reports/A55-split-view-clip.md`, short, with exactly these sections:
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
<Filled by `/review-task`: gate result lines, review verdicts, then the bounds check, one line per difference between
Request, Plan and done: within plan | justified (why) | ask the user. "Noticed": side bugs as proposed follow-ups.>

## Hand test
<Filled by the sub-session: the build checked (dev badge text), and the user's result, verbatim.>
