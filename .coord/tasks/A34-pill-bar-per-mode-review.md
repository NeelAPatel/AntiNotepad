# A34-pill-bar-per-mode-review: lens review of A34-pill-bar-per-mode

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`.
Branch: `feat/A34-pill-bar-per-mode` (already checked out; never switch, merge, commit or edit product files).
Tier: Medium

## Inputs
- Item card: `.coord/tasks/A34-pill-bar-per-mode.md` (Request, Plan with After, Files, Approve), edge cases `.coord/research/A34-edge-cases.md`
- Implementer reports: `.coord/reports/A34-pill-bar-per-mode.md`, `.coord/reports/A34-pill-bar-per-mode-r1.md`
- Gate report: `.coord/reports/A34-pill-bar-per-mode.gate.md` (passes with -Offline)
- Diff: `git diff dev...HEAD -- product`
- Approved visual truth: `tools/playground/a34-pill-bar-lab.html`

## Do
Follow `.agents/skills/lens-review/SKILL.md` exactly, including the plan-bounds item. You may build into `build/A34-pill-bar-per-mode-review` and run `--selftest`; delete that folder after. Never change files under `product/`.
Look hardest at (the manager already saw these, prove or clear each):
1. `_toast.BoxShadow = _pill.BoxShadow` in the theme path now runs before the bar is configured and `_pill` has no shadow in Float or Quiet: does the toast lose its shadow?
2. Do the Settings toggles (Show Delete, Show New note, Slot dots) update the visible bar live (the settings change handler reaching `UpdateChrome` / `ConfigureBarStyle`)?
3. Reveal, fade and hit testing in all three bar styles: `_bar` replaces `_pill` everywhere (`_barsSleep`, `FadeChrome`, `FloatPager`, `PadWindow.TopBlur.cs` popup visual, overlay hide/show, `SleepChrome`). Any leftover `_pill` use that should be `_bar`?
4. Float: the popup child is `_bar`; width and separators when Delete or New note are off; the bar reserved corner widths in Docked/Overlay when New note is hidden in slot view (no shift on swap).
5. Split: `DecideBar` uses `_active.Note?.Slot`; swap target when the right pane shows the preferred slot; `SelectBarSlot` in the right pane; `CycleSplitSlot(0, slot)`.
6. Delete: debounce (500 ms), `DeleteCurrent` untouched, no save/Store/Note/shutdown code changed (A30).
7. Disabled Delete / New note states: `IsEnabled` with the Kit button visuals (does a dimmed state exist and read as dimmed?).
8. Coding standard: file headers, summaries, token use, no magic numbers, no history in comments.

## Write: `.coord/reports/A34-pill-bar-per-mode.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
