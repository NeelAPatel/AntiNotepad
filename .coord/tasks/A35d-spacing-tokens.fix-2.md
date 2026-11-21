# A35d-spacing-tokens: header comments (review finding 2)

Role: terra. Comment-only work, where the quality of each comment matters more than how many there are.

Read `.claude/rules/coding.md` ("Comments") and the review `.coord/reports/A35d-spacing-tokens-review.md`, section 6.

Add exactly these and nothing else:
- A 1-3 line purpose header at the top of `Editor/Completion.cs`, `Editor/Renderers.cs`, `Platform/TaskbarPresence.cs`,
  `UI/HelpOverlay.cs`, `UI/PadMenu.cs`, `UI/SearchOverlay.cs` and `UI/SettingsParts.cs` (all in `product/AntiNotepad/`).
- `///` summaries for the class at `Program.cs` near line 190, the method at `UI/Kit.cs` near line 32, and the method at
  `UI/SearchOverlay.cs` near line 111.

The quality bar:
- Read the code first. Each comment says what the thing is for and why, in plain words a new reader understands.
- Never write a template phrase such as "Manages X", "Provides X behavior" or "Handles X". A comment that only restates
  the name is worse than none, and the review fails it.
- No item IDs, dates or history. Keep each one short.

Rules: no code changes. The branch is checked out, so never switch, merge or commit. Offline only. Run the build and
`--selftest`; the `DESIGN dump` must stay `40aa4d67373fa5f983d3a8d5a1a40f4f9403c6009ef8157d20c90ad507c24df0`. Gate:
`pwsh -NoProfile -File tools/gate.ps1 -Task A35d-spacing-tokens -Base 3b6ecb5 -Offline`.
Report: `.coord/reports/A35d-spacing-tokens-fix-2.md`. List each comment's full text, so the reviewer can read them all.
