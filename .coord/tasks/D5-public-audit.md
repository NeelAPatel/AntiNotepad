# D5-public-audit (Findings scout)
## Rules (all read-only)
- Read-only Findings scout. Do not edit, create, delete or commit anything; do not run the app, ports or network. Offline reads, git log/ls-files/tag, grep only.
- Output: your FINAL MESSAGE is the report (the sandbox cannot write files). Use the sections named below. Lead with a 5-line summary, then tables. Cite file paths. Mark each claim "verified" or "guess".
- Context: the owner is making the private GitHub repo NeelAPatel/AntiNotepad public. Product is a Windows 11 hotkey scratchpad (Avalonia 12, .NET 10) in product/AntiNotepad. It is AI-led (Claude + Codex), inspired by Antinote.io (author JFung granted permission for inspiration, branding and styling, subject to change on request). Contribution stance: ideas welcome, atomic fixes and patches welcome, UI ideas hardest for the owner, strong AI-leveraged testing ideas appreciated.
- Role: quick, read-and-grep inventory.

## Task
Audit every tracked top-level path on main (.agents, .claude, .codex, .coord, .github, docs, tools, third_party, product, root files, .editorconfig, .gitattributes, .gitignore, AGENTS.md, CLAUDE.md, README.md, .openchamber if tracked) and decide for a PUBLIC repo.
## Report sections
1. Summary
2. Table: path | what it is | keep on main / move to dev-only / remove from tracked / rewrite | reason | risk (secrets, personal paths, private names, emails, machine paths such as F:\ or C:\Users). 
3. Sensitive-content scan: grep tracked files for emails, absolute user paths, tokens/keys, private URLs; list hits with file:line.
4. History concerns: anything in git history (archive tags, big files, bundles) that a public flip would expose; size of .git; recommended tag/branch cleanup (do not run it).
5. Proposed public tree (short) and what main-only means given the dev/main branching in docs/loop.md.
6. Open questions for the owner (max 5).
