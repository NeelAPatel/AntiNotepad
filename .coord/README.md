# .coord

This folder is how the AI agents in my dev loop keep themselves on track: what's in flight, what was planned, what was built and what the reviews found. You don't need it to build, use or contribute to AntiNotepad.

- **`STATUS.md`:** what is in flight right now.
- **`pending.md`:** the backlog.
- **`tasks/`:** one card per change: the request, findings, plan and result.
- **`reports/`:** what Codex built and what the reviews found.
- **`lessons.md`:** misses and what I changed because of them.
- **`templates/`, `tools/`:** card templates and the Codex launcher the loop uses.
- **`research/`, `logs/`, `directors/`:** scratch space for the loop; mostly empty or local-only here.

**Names you'll see in cards and reports:** "Master N", "Loop Hub N" and "Roadmap Hub N" are my top-level director chats, one Claude Code chat per area (the app, the loop, the roadmap). A director writes the request and merges; one item chat does the work. See [Who Does What](../docs/wiki/Who-Does-What.md).

*If you're curious how an AI-led change goes from request to merge, start with [building with AI](../docs/building-with-ai.md) instead of reading cards in order.*
