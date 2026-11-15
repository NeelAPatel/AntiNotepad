# Scroll feel: locked parameters

Locked by the user 2026-11-14/15 in the styles lab (macOS / iOS style), wheel input. Reference implementation:
`tools/playground/a20-styles-lab.html` (engine functions `inputH`, `stepH`, `tryCommit`, `startCommit`, `notchGain`,
`inputV`, `stepV`); side-by-side check: `tools/playground/a20-compare.html`. Item: `A20-overscroll-speed`.

## Horizontal (switching notes): one option, "Follow-through"
| Parameter | Value | Meaning |
|---|---|---|
| Pager follows input | 1:1 while a neighbor note exists | No resistance mid-stack. |
| Resistance band | only at the real first/last edge; ceiling = view width | `b = (1 - 1/(x*0.55/D + 1)) * D`, D = view width. |
| Input smoothing | 0 ms (touchpad/drag), glide-in 70 ms for a wheel click | Wheel clicks arrive as one big step. |
| Quiet (input has stopped) | 90 ms | Then release is decided. |
| Commit rule | projection: `x + v * 499 ms >= 0.5 * page width` | Decided early while touchpad inertia decelerates, else at quiet. |
| Release velocity | mean speed over the last 60 ms (at least 16 ms span) | `v = 0` if the last event is over 80 ms old. |
| Slide | critically damped spring, period 420 ms, damping 1.0 | Starts with the release velocity; page index flips at commit start. |
| Rest of a committed gesture | ignored until 150 ms quiet or a fresh push | The touchpad inertia tail ("spent"). |
| Wheel click, neighbor exists | one click = one page; clicks within 260 ms ignored | |
| Wheel click at an edge | nudge `max(160, 2*96) px * notchGain`, adds if same direction | Glide-in 70 ms, hold 220 ms, then eases back with the spring. |
| `notchGain` | `0.2 + 0.8 * clamp((1000/gap - 2.5)/9, 0, 1)`, gap = smoothed time between clicks (max 400 ms) | One slow click = small nudge; a quick roll = full pull. |
| Return from the edge | release from rest after the hold; spring period 420 ms | Never snaps: the pulled position is kept and springs home. |

## Vertical (scrolling inside a note): unchanged from `dev`
- Lines, Free, Rate-matched and Wheel-driven keep their existing glide inside the note; scrolling stops at either end without overscroll or carried momentum.
- Touchpad input follows 1:1 in pixels. Rate-matched remains the default.
- User call, 2026-11-15: remove the vertical overscroll and keep Windows 11 style smooth scrolling vertically.

## Settings > Feel
- "Scroll style" is renamed "Vertical Scroll Style" (Lines | Free | Rate-matched | Wheel-driven), default Rate-matched.
- New "Horizontal Scroll Style" with one option, "Follow-through", the default.
- The live preview in Feel shows both.

## Not locked (parked)
Smooth with Line Snap, Complete Smooth and Velocity glide (vertical lab experiments) are not built; nothing here
depends on them.
