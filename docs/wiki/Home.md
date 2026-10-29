# AntiNotepad dev loop: the guide

This wiki explains, in plain words, how AntiNotepad gets changed: who asks, who plans, who builds, who checks, and how a
change ends up in the app. You do not need to read code to follow it.

**The one-sentence version:** every change is one *item* with one *card*; it walks through eight phases, and nothing is
built until you say "go" and nothing is merged until you have tried it on screen.

## Start here
1. [Dev Loop Overview](Dev-Loop-Overview): the eight phases in one picture.
2. [Who Does What](Who-Does-What): you, the master chat, helper sessions, Codex and scripts.
3. [Worked Example A55](Worked-Example-A55): a real item followed from request to merge.
4. [Tiers and Gates](Tiers-and-Gates): how risky a change is and which checks it gets.
5. [Triage and Lessons](Triage-and-Lessons): what happens when something fails, and how the loop learns.
6. [Where Files Live](Where-Files-Live): a map of the folders.
7. [Loop History](Loop-History): how the loop got to its current shape.
8. [Loop v2.2 Plan](Loop-v2.2-Plan): what is planned next (parallel items, catching up with dev, conflicts).
9. [Glossary](Glossary): every unusual word.

## Where the rules really live
This wiki is a readable copy. The rules themselves are in the repository: `docs/loop.md` (the law for the loop),
`AGENTS.md` (project rules), `docs/agents.md` (how Codex is launched), `docs/dev-loop.md` (the dev copy and installing).
If this wiki and `docs/loop.md` ever disagree, `docs/loop.md` wins; please say so and the wiki gets fixed.

*Written for loop v2.1, 2026-10-29. Source of the pages: `docs/wiki/` in the repository.*
