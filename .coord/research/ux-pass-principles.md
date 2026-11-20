# ux-pass: principles behind the A34 UX suggestions

Kept with `idea: ux-pass` (pending.md) at the user's request, 2026-11-20 ("These two messages was what i mentioned"). Source: the A34-pill-bar-per-mode chat's UX review. Longer edge-case reasoning: `.coord/research/A34-edge-cases.md` (arrives with the A34 merge).

| A34 suggestion | Principle behind it |
|---|---|
| Keep the right corner empty so the bar does not shift | Layout stability (consistency): controls should not move between states |
| "Clear slot 3", dim when empty | Match between system and real world (Nielsen): labels say what really happens; dimming is visibility of system status |
| Undo instead of confirmation | User control and freedom: recovery beats an "Are you sure?" dialog |
| Delete red only on hover, kept away from the arrow | Error prevention and Fitts's law (target distance and size); color as a signal, not decoration |
| Corner buttons share the pager's layer | Consistency, and Gestalt common fate: things that appear together belong together |
| Taller hit area on dots | Fitts's law and minimum target size ("hit slop") |
| Swap button names its destination | Recognition over recall; signifiers (Don Norman) |
| Dropping the date at 320 px | Progressive disclosure: show less until it is needed |

## Method for the whole-app pass
Heuristic evaluation: run two lists against every screen, Nielsen's 10 usability heuristics and Fitts's law with target-size minimums. First step: list each screen's destructive actions and unlabeled icon-only buttons (plus targets under 32 px and controls that move between states).
