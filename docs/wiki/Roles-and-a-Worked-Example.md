# Roles and a worked example

I set direction and make decisions while a director, item chat and Codex share the work.

- I supply the request, approve the plan, hand-test when needed and decide on release.
- A director routes the item and merges accepted work. The item chat investigates, plans, briefs Codex and checks the result. Codex implements and reports its offline checks. The [role guide](https://github.com/NeelAPatel/AntiNotepad/blob/main/docs/agents.md) describes the current setup.
- **Example: A21-save-delay-setting.** The [item card](https://github.com/NeelAPatel/AntiNotepad/blob/main/.coord/tasks/A21-save-delay-setting.md) records my request for a choice of save delay, the findings about save timing, the plan and my approval.
- The [initial worker report](https://github.com/NeelAPatel/AntiNotepad/blob/main/.coord/reports/A21-save-delay-setting.worker.codex.md) recorded a build and a self-test that had hung; the final check and multiple rework rounds are recorded in the card.
- My first hand test failed on the blank-note and saved-label behavior. A second exposed a label clock issue. The card records my third hand test as passing.
  - *The automated checks, review and my hand tests provided different evidence. The card also leaves a separate sign-out/shutdown live check open; a passing hand test does not prove every path.*
- See [[Dev Loop Overview]] for the full sequence and [[Building with AI and Lessons]] for what changed after misses.
