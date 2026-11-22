# A35d-spacing-tokens: review fixes

Role: terra. Two small fixes from the review, across one script and the touched files' comments.

Read `AGENTS.md`, `.claude/rules/coding.md`, the item card `.coord/tasks/A35d-spacing-tokens.md` and the review
`.coord/reports/A35d-spacing-tokens-review.md`. Fix both findings in section 5 and section 6, and nothing else.

1. **Scanner.** In `tools/check-design-tokens.ps1`, the `stroke` category also counts literal thickness arguments of
   `RenderPaint.Pen(...)`. Add a hit fixture and a no-hit (token) fixture. Recount with `-Update`. The Renderers pens
   at the review's lines are exceptions 4 (checkbox), or literals to move to `Shape` stroke tokens if equal to Thin or
   Thick. Move the equal ones to tokens; leave the rest as counted literals and list them.
2. **Coding standard on touch.** Add a 1-3 line purpose header to each file the review lists. Add `///` summaries to
   the class and methods it names, and to any other class or non-trivial method without one in the files this item
   changed (`git diff --name-only 3b6ecb5 -- product`). Comments only: no code changes in this part.

Rules: the branch is checked out, so never switch, merge or commit. Never launch AntiNotepad. Offline only. Build into
`build/A35d-spacing-tokens-check`, then run `--selftest`. The `DESIGN dump` hash must stay
`40aa4d67373fa5f983d3a8d5a1a40f4f9403c6009ef8157d20c90ad507c24df0`. Gate:
`pwsh -NoProfile -File tools/gate.ps1 -Task A35d-spacing-tokens -Base 3b6ecb5 -Offline`.

Report: `.coord/reports/A35d-spacing-tokens-fix-1.md`. Sections: 1 Summary; 2 Change list; 3 Literals left counted;
4 Checks run.
