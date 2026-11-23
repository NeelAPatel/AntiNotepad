# T21-release-kit (Findings scout)
## Rules (all read-only)
- Read-only Findings scout. Do not edit, create, delete or commit anything; do not run the app, ports or network. Offline reads, git log/ls-files/tag, grep only.
- Output: your FINAL MESSAGE is the report (the sandbox cannot write files). Use the sections named below. Lead with a 5-line summary, then tables. Cite file paths. Mark each claim "verified" or "guess".
- Context: the owner is making the private GitHub repo NeelAPatel/AntiNotepad public. Product is a Windows 11 hotkey scratchpad (Avalonia 12, .NET 10) in product/AntiNotepad. It is AI-led (Claude + Codex), inspired by Antinote.io (author JFung granted permission for inspiration, branding and styling, subject to change on request). Contribution stance: ideas welcome, atomic fixes and patches welcome, UI ideas hardest for the owner, strong AI-leveraged testing ideas appreciated.
- Role: quick, read-and-grep inventory.

## Task
Findings for a proper public release kit. Read tools/install.ps1, tools/*, docs/dev-loop.md, the csproj, existing tags and any release notes.
## Report sections
1. Summary
2. Tag inventory: every tag with commit date and meaning; proposal: keep / delete / rename (semver vN.N.N only on main); one-liner commands to apply (do not run).
3. Current install/uninstall/upgrade/backup behavior: what exists, where data lives (%LOCALAPPDATA% / AppData), what an uninstaller must keep or remove, how a data backup/restore could work.
4. Packaging options compared in a table: self-contained zip, MSIX, Inno Setup, WiX, Velopack, winget manifest, dotnet tool; size, signing need, SmartScreen impact, auto-update, effort. Recommend one.
5. GitHub Releases plan: asset names, checksums, notes template, GitHub Actions workflow outline (tag push builds and attaches assets). Which parts need the owner (signing, secrets).
6. "Package" in GitHub sidebar: what could honestly go there (likely nothing or a NuGet/winget); recommendation.
7. Proposed step breakdown (A1a-style steps) with Low/Medium/High tier each.
8. Open questions for the owner (max 5).
