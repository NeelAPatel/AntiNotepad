# D6-public-docs (Findings scout)
## Rules (all read-only)
- Read-only Findings scout. Do not edit, create, delete or commit anything; do not run the app, ports or network. Offline reads, git log/ls-files/tag, grep only.
- Output: your FINAL MESSAGE is the report (the sandbox cannot write files). Use the sections named below. Lead with a 5-line summary, then tables. Cite file paths. Mark each claim "verified" or "guess".
- Context: the owner is making the private GitHub repo NeelAPatel/AntiNotepad public. Product is a Windows 11 hotkey scratchpad (Avalonia 12, .NET 10) in product/AntiNotepad. It is AI-led (Claude + Codex), inspired by Antinote.io (author JFung granted permission for inspiration, branding and styling, subject to change on request). Contribution stance: ideas welcome, atomic fixes and patches welcome, UI ideas hardest for the owner, strong AI-leveraged testing ideas appreciated.
- Role: quick, read-and-grep inventory.

## Task
Findings and DRAFTS for public-facing docs. Read README.md, docs/*, product/AntiNotepad features (settings, shortcuts, calc, search, themes), third_party/, asset and font files, csproj package refs.
## Report sections
1. Summary
2. Feature inventory (what the app really does today; verified from code/selftests, not from old docs).
3. DRAFT README.md in full (hero line, what it is, features, install, usage/hotkeys, build from source, status/roadmap, contributing link, credits, disclosure, license). No screenshots invented; mark places for screenshots.
4. GitHub About: description (max 350 chars), 8-12 topic tags, website field suggestion.
5. DRAFT Credits and Disclosure text: JFung of Antinote.io (permission granted for inspiration, branding and styling, subject to change on request); AI-led with scrutiny, a personal learning project in AI models, agentic development and orchestration; link to wiki dev-loop pages.
6. DRAFT CONTRIBUTING.md (ideas welcome, atomic fixes and patches welcome, UI ideas wanted but owner finds them hardest, AI-leveraged testing ideas welcome; PR/issue expectations; how to build and run --selftest; mention dev-loop is owner-run).
7. Wiki page list with 2-3 line summary of each (public user guide, dev loop explained for AI-orchestration readers, architecture, decisions).
8. License analysis: dependencies and bundled assets (Avalonia, AvaloniaEdit in third_party, fonts, icons, any Antinote-derived art) with their licenses; options (MIT, Apache-2.0, PolyForm/other) with trade-offs given Antinote inspiration and branding; recommendation. This is information, not legal advice.
9. Open questions for the owner (max 5).
