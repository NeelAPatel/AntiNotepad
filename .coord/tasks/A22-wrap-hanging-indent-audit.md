# A22-wrap-hanging-indent-audit: findings for A22-wrap-hanging-indent

Role: director (gpt-6.1-sol medium), read-only. Product `product/AntiNotepad`. Main checkout on `dev`: never switch, merge, commit, or write files. Never launch or kill AntiNotepad. Offline only.
Read `AGENTS.md`, then `.coord/tasks/A22-wrap-hanging-indent.md` (its Request is what the user asked for).

## Scout notes from the item chat (verify, do not trust)
- `PadView.cs` (~line 93) sets `WordWrap = true`; no `WordWrapIndentation` or `InheritWordWrapIndentation` setting anywhere in `product/` or in git history (also searched archive/full-history-2026-10-29). So no earlier fix is visible by that name.
- `Editor/Elements.cs` `BulletGenerator` replaces the `-`/`*`/`+` char with a "•" glyph element (leading whitespace stays normal text); `CheckboxGenerator` similar.
- Avalonia.AvaloniaEdit 12.0.0 is used. Find (NuGet cache xml/pdb/source, or reflect on the dll offline) how AvaloniaEdit computes wrap indent for continuation lines (`InheritWordWrapIndentation`, `WordWrapIndentation`, `VisualLine` indentation visual column) and whether leading spaces alone should already indent.

## Answer these
1. **Kind and why**: bug, feature, polish or process.
2. **Now**: what the user sees on dev today for nested bullets (levels 1-3), top-level bullets, plain indented lines, numbered lists, checkboxes when soft-wrapped. Evidence. Explain why the user's screenshot shows continuation lines at the left edge even for indented bullets.
3. **Regression?**: was hanging indent ever there (git log -S on Editor files, archive tag, AvaloniaEdit version bumps in the csproj history)? What changed? If never, say so.
4. **Cause**: exact code path, quoted. If not proven, say "not proven" and what a probe would show.
5. **Where**: files that would change; the smallest fix (e.g. a ParagraphProperties/indent hook, option setting, or custom text-line indent); how bullet text column is measured with a proportional font and the "•" glyph width; interaction with caret movement/selection (must be unchanged), IndentGuideRenderer, and the scale/size colorizer.
6. **Testability**: can the indent computation be a pure function checkable by `--selftest` (see SelfTest/Editor.cs)? Propose the check.
7. **Risk and tier**: confirm or raise Medium.
8. **Recommendation**: smallest change, open scope questions (numbered lists, checkboxes, plain indented lines).

## Final message: the report
Sections 1-8, short, each claim with file:line evidence. End with `READY: yes` or `READY: no - <what is missing>`.
