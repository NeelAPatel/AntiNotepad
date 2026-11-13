# A16-saved-label-fade: animate the top-left "Saved" text when it changes between notes

Item card (`docs/loop.md`). Budget: 4 KB.

Tier: Low · ui-motion
State: pending
Branch: `feat/A16-saved-label-fade` in `.claude/worktrees/feat/A16-saved-label-fade` (created at Approve)

## Request
2026-11-12, user (pasted list, Animations), verbatim: "Top left 'Saved' text changing between notes > expecting some sort of animation instead of snap to new text associated with the note"

Then: "Saved label fade" (chosen next from the running UI/UX/animation list in `.coord/pending.md`, idea saved-label-animation). Reuse the Windows 11 motion values logged in `docs/decisions.md` (A14-search-open-fade-up, A15-notes-edge-pills: 167 ms in, 83 ms out, 16 px rise, `Win11Enter`/`Win11Exit`) rather than inventing timings; the user cannot judge motion from numbers, so show the motion in a lab or the dev copy.

## Findings
- Label: `_saved` TextBlock in `_meta` (`UI/PadWindow.Layout.cs`). `UpdateChrome` clears `_meta` and builds a new TextBlock each call, so text snaps.
- Text: `"Saved " + Kit.Ago(note.Modified)`: just now, 3m ago, 2:14 PM (today), yesterday, Mon, Oct 3.
- Triggers: note switch / slot / split (Navigation), `Edited()` per edit, move note, timer done, focus pane, overlay close, bar shown (`RefreshSaved`, in place).
- Typing: `Modified` = now each edit, so text stays "just now"; it changes only on the first edit after a gap. No Saving state exists.
- Lab: `tools/playground/a16-saved-label-lab.html`.

## Plan
- **After:** when the label text changes on a note switch, the old text exits and the new enters, following the page direction: next sends old left and new in from the right, previous mirrored. Out 83 ms `Win11Exit`, in 167 ms `Win11Enter`, shift 16 px (`MotionTokens.OverlayOut/OverlayIn/OverlayRise`), opacity 1 to 0 and 0 to 1.
- **Must not change:** snap when text is equal; reduced motion (`Anim.Reduced`) snaps; typing never animates direction (user did not rule; default: edits update in place, no animation); show/hide speed.
- **Test plan:** pure helper (direction to offsets) with a `--selftest` check; hand: switch notes next/prev, direction matches page, rapid switching has no ghost text.
- **Needs from you:** hand test.

## Files
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/Motion/MotionTokens.cs`
- `product/AntiNotepad/SelfTest/Run.cs`
- `tools/playground/a16-saved-label-lab.html`

## Contracts
None new.

## Approve
2026-11-12, user: "Follow Page Direction and standard windows 11 timings."

## For Codex
Role: terra, a feature across a few files with timing. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then the Files; re-read before editing.
- Build exactly the Plan's "After"; no drive-by changes. `_meta` is rebuilt in `UpdateChrome`; keep the label alive across note switches (or animate old/new overlay) so a cancelled/rapid switch leaves one label. Find the direction used by `PadView.Arrive(direction)` on switch and reuse it.
- Build into `build/A16-saved-label-fade-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, delete the folder. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A16-saved-label-fade -Offline`.
- Never launch AntiNotepad or edit STATUS or this card. Offline checks only.
- Done means: label animates per After; selftest check exists and passes; 0 warnings, `SELFTEST PASS`, gate passes.

Report `.coord/reports/A16-saved-label-fade.md`, short: 1 Summary; 2 Change list; 3 Behavior changes; 4 Out-of-scope files; 5 Plan deviations; 6 Contracts; 7 Lens review; 8 Checks run; 9 Hand checks.
