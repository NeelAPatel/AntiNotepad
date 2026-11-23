# D12-wiki-drafts (Codex docs in this worktree; do not commit)

Role: terra, writing and reasoning across docs. Sandbox workspace-write.
Inputs: `.coord/research/public-prep/D10-about-and-wiki.md` (outline of 8 pages), `README.md`, `CONTRIBUTING.md`, `docs/building-with-ai.md`, `docs/loop.md`, `docs/agents.md`, `docs/playbook.md`, `docs/dev-loop.md`, `docs/decisions.md`, `docs/glossary.md`, `.coord/lessons.md`, `product/AntiNotepad` (verify every feature claim from code and self-tests; never copy claims from old docs unverified).

## Owner decisions (fixed)
- Voice is FIRST PERSON ("I", "my"). Never write "the owner".
- Style: clean, crystal-clear bullets; where there is a thought process, a sub-bullet in italics. Not textbook prose. US spelling. Plain words, short sentences.
- Two-part project: the notepad, and the learning growth (AI-led development, dev loops, pstack). AI-led with scrutiny; for my own learning of AI models, agentic development and orchestration.
- Credit: Jfung of Antinote.io gave permission for inspiration, branding and styling, subject to change on request. Mention it on the Home page in one sentence.
- First release is a portable unsigned zip; it exists from the first release. Do not claim installers or signed builds.

## Write (a new folder `docs/wiki/`, one Markdown file per wiki page, GitHub wiki filenames)
`Home.md`, `_Sidebar.md`, `Getting-Started.md`, `Writing-and-Organizing.md`, `Settings-and-Shortcuts.md`, `Dev-Loop-Overview.md`, `Roles-and-a-Worked-Example.md`, `Building-with-AI-and-Lessons.md`, `Architecture-and-Data.md`, `Decisions-and-Trade-Offs.md`, following the outline in D10. Link between pages with wiki links `[[Page Name]]`; link to repo files with full `https://github.com/NeelAPatel/AntiNotepad/blob/main/<path>` URLs. Each page: a one-line summary, then bullets. For the worked example, follow one real item through its card and reports under `.coord/tasks` and `.coord/reports`, citing paths, never old commit hashes.

## Constraints
- Write only inside `docs/wiki/` and the report below. Do not edit other files.
- Do not link into `.coord/research/` (it is removed from the public tree). No home paths, repo paths, claude.ai artifact URLs.
- Mark screenshot spots with `<!-- screenshot: ... -->`; do not invent screenshots.
- Every feature or shortcut claim traced to a file in the report.

## Report `.coord/reports/D12-wiki-drafts.md`
Files written, claim-to-source table (short), links checked, items I must confirm. Final message: the same in brief.
