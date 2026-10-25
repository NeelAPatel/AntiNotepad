# A28-editor-render-memory

You are the director (gpt-6.1-sol medium). You plan, split and check the work; delegate the implementation to gpt-6-luna xhigh
subagents (how many, and how to split, is your call). If subagent spawning is not available to you in this run, say so at the
top of your report and do the work yourself.

Repo: this folder, a git worktree on branch `feat/A28-editor-render-memory` (off `dev`). Never switch branch, merge or commit
(.git is read-only for you and your subagents); the manager (a Claude session) reviews the diff and integrates it. Your
subagents share this one worktree, so give each one a disjoint set of files.
App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10). Read first: `AGENTS.md`, `docs/dev-loop.md`,
`docs/decisions.md`, and in `docs/audit/A1-codebase-audit.md` findings A1-7, A1-9 and A1-10 (table rows, "technical notes",
and the file checklist). Match the surrounding style, naming and comment density. First-party .NET/Avalonia only, no new
packages.

## Findings
1. **A1-7-stale-fence-cache** (`Editor/Structure.cs` ~:176). The code-block color cache is keyed by the fence's opening line
   number; keys for fences that moved or were deleted stay until the note is reopened. Direction: prune entries no current
   fence uses after each structure update.
2. **A1-9-fence-copy-per-colored-line** (`Editor/Structure.cs` ~:209). `Get(fence)` copies the whole code-block text for every
   colored line, before the cache check. Direction: validate the cache once per colorizing pass (or by a cheap version stamp),
   copying the text only when it actually changed.
3. **A1-10-render-path-allocation** (`Editor/Renderers.cs` ~:111, `Editor/ChevronGutter.cs`, `Editor/Colorizers.cs`, the edge
   fade, `UI/Kit.cs` glyph icons). Answer chips, fold chevrons, edge fade and glyph icons create a new `FormattedText`, brush or
   pen on every render. Direction: cache these by what they depend on (palette, font, size, DPI/scale, text) and invalidate on
   theme, palette, font or scale change. Only the hot per-frame paths; leave one-off allocations alone.

## Constraints
- No visible change: colors, glyphs, chip text, layout, fades and hover states look exactly as today in light and dark, at
  every UI scale, and update at once when the theme, palette, accent, font or scale changes.
- Caches are bounded and owned by the view/renderer that uses them (released with it); no static growth across notes.
- Mutable brushes handed to Avalonia must not be shared and later mutated in a way that changes other users; prefer immutable
  brushes/pens for cached ones.
- Files in scope: `Editor/Structure.cs`, `Editor/Renderers.cs`, `Editor/ChevronGutter.cs`, `Editor/Colorizers.cs`, the file
  that draws the edge fade, `UI/Kit.cs` (glyph icon drawing only), `SelfTest.cs` (ONE new method at the END plus one call
  line in `Run`). Anything else: say why in the report. No change to the store, settings, search or windows.

## Selftest
Small capability checks only (no reflection into Avalonia internals, no test-only fields or hooks in product classes, no
copy of the old code path kept for comparison):
- after moving and deleting fences, the fence color cache holds only current fences;
- colorizing a code block of N lines copies its text at most once per pass while unchanged;
- a cached chip/chevron/glyph resource is reused across two renders and replaced after a palette or scale change.
If a check needs a running platform or window, leave it out and list it under live checks instead.

## Measure
Give before/after numbers for allocations or time per render in the report (a temporary local harness is fine, but don't
leave it in product code).

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a28-check`: 0 errors, 0 warnings.
  `build/a28-check/AntiNotepad.exe --selftest` prints SELFTEST PASS. Delete `build/a28-check` if you can.
- Never launch AntiNotepad any other way, never kill it, and never read or write `%LOCALAPPDATA%\AntiNotepad`,
  `%LOCALAPPDATA%\Programs\AntiNotepad` or the real Documents folder.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A28-editor-render-memory.md`: how you split the work (which subagents, which files), files
  changed, design as built, before/after numbers, selftest list, what a person must check live (marked "needs a live check":
  code-block colors while moving/editing fences, answer chips, fold chevrons, edge fade, toolbar icons, after theme/palette/
  scale switches), anything not done.
