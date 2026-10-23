# Perception model: what a user experiences, in order

| Stage | User experience | What to check |
|---|---|---|
| 1. Arrival (~0-1 s) | Did it respond? Instant, or flicker, white flash, stutter? | Response time. About 100 ms feels instant; about 1 s breaks flow; 60 fps leaves about 16 ms per frame. |
| 2. First look | Where does the eye land? Is it obvious where to type? | Visual hierarchy, contrast, primary target prominence. |
| 3. Orientation | "Where am I, what is this, what can I do?" | Labels, state shown, discoverability of hidden or subdued controls. |
| 4. Doing | Typing, selecting, switching, undoing. | Visible feedback on every input, predictability, recoverable mistakes. |
| 5. Living with it | Fatigue, repetition, trust. | Consistency, colour and type comfort over hours, motion that does not become annoying. |

## Thresholds (rules of thumb, not cutoffs)
- 0.1 s: feels instantaneous; no special feedback needed beyond the result.
- 1 s: user notices the delay but keeps their train of thought.
- 10 s: attention limit.
- 16 ms per frame for 60 fps animation.
- One 2017 study of mouse input measured perception as low as 34-137 ms (mean 65 ms), so direct pointer feedback should aim under 100 ms.
- The 400 ms Doherty figure is from a secondary source; treat as unverified.

## Not verified by research
- The "first impression in ~50 ms" claim and Gestalt/visual-hierarchy sources did not turn up in search. Use them as working assumptions, not citations.

## Hotkey-summoned app specifics (inferred)
- Stage 1 dominates: the user is mid-thought in another app, so show-to-focus and first-paint quality matter most.
- A fade or settle must never delay focus or typing.
