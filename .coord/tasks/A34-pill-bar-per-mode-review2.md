# A34-pill-bar-per-mode-review2: lens review of rounds r10 and r11

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`. Branch `feat/A34-pill-bar-per-mode` (checked out; never switch, merge, commit or edit product files). Tier: Medium.

## Inputs
Card `.coord/tasks/A34-pill-bar-per-mode.md` (Plan, Plan addendum, Hand test), briefs `-r10.md` and `-r11.md`, reports `-r10.md`, `-r11.md`, earlier review `.coord/reports/A34-pill-bar-per-mode.review.md`. Diff to review: `git diff 533073b..HEAD -- product` (r10: animated dimming, chrome kept awake under the pointer, slots cycle in Navigate; r11: PadBar refactor, pure move expected).

## Do
Follow `.agents/skills/lens-review/SKILL.md` including the plan-bounds item. You may build into `build/A34-pill-bar-per-mode-review2` and run `--selftest`; delete it after if allowed. Look hardest at:
1. r10 `SleepChrome`/pointer handling: can the docked chrome now get stuck visible (pointer leaves while a timer was cancelled), or still fade while hovering? Check every caller of `SleepChrome(soon:false)` and the exit path; Sliding styles untouched?
2. r10 `Navigate` in a slot: wrap 9 to 1 and 1 to 9, split skip of the right slot, `_lastSlot`, `NoteSwipe`/`CanNavigate` consistency, `Alt+Left/Right` tooltips and help text, A29 shortcut table equals docs.
3. r10 animated dim: cancellation on rapid flips, initial state not animated, reduced motion, disabled hover leftovers (stuck hover background after an enable flip).
4. r11 is it a pure refactor: same visual tree (design hash unchanged is proof), same behavior in Docked, Overlay, Float; `_bars` iteration in `ApplyBars`, `FloatPager`, fade, sleep, overlay hide/show and popup blur with one bar; any leftover single-bar assumption that would break a second bar.
5. Performance (show/hide speed is the priority): `BarContextFor` builds a snapshot with 9 slot contexts and `Note.Title` on every `UpdateChrome`; does anything run on the show/hide path, and is the cost per navigation acceptable?
6. Coding standard and no save/Store/Note/shutdown changes (A30).

## Write: `.coord/reports/A34-pill-bar-per-mode.review2.md` in the skill's format. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
