# Dev loop overview

I move each item from a request to a reviewed change and a separate release decision.

- **Request → Findings → Plan → Approve:** a director records my request; an item chat investigates current behavior and proposes a plan. I decide whether to approve it.
  - *Findings and a plan make the expected change and its checks reviewable before work starts.*
- **Build → Check → Hand test:** a worktree and item branch are created at approval. Codex implements; the item chat checks the result against the request and plan. I perform any required hand test.
- **Merge:** a director reviews and merges the item to `dev`. I separately decide when `main` moves for a release; releases are tagged.
- An item card records the request, findings, plan, approval and checks. Reports hold implementation and review evidence. See [[Roles and a Worked Example]].
- The [current loop](https://github.com/NeelAPatel/AntiNotepad/blob/main/docs/loop.md) and [playbook](https://github.com/NeelAPatel/AntiNotepad/blob/main/docs/playbook.md) have the detailed rules and exceptions. Contributors do not need to reproduce my loop; see [Contributing](https://github.com/NeelAPatel/AntiNotepad/blob/main/CONTRIBUTING.md).
