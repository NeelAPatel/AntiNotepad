# A28-fence-cache-bounds-fix

You are the implementation worker (gpt-6-luna xhigh). Do the work yourself.

Repo: this folder, the git worktree on branch `feat/A28-editor-render-memory`, holding the uncommitted A28-editor-render-memory
change (brief `.coord/tasks/A28-editor-render-memory.md`, report `.coord/reports/A28-editor-render-memory.md`). Never switch
branch, merge or commit.

## Review result
Accepted as is: the chevron, colorizer, glyph, edge-fade and Renderers caches, the fence-cache pruning and the selftest style.

## Bug to fix (`Editor/Structure.cs`, `FenceCache` / `FenceColorizer.Get`)
The fast path `TryGet(openLine, doc.Version, language, palette)` trusts a cache hit by document version alone, but the cached
body was copied using the fence bounds of `pad.Structure` at that moment. For notes over 1500 lines, `PadView.OnDocumentChanged`
defers `Analyse()` by 140 ms, so the structure is stale after an edit:
1. User presses Enter inside a fence (doc version v2, structure still from v1).
2. A render colorizes: miss, copies the body using the v1 fence bounds, stores it under v2.
3. `Analyse()` rebuilds the structure for v2 (the fence's close line moved) and redraws.
4. `TryGet(open, v2, …)` hits and returns the highlight built from the stale body, so the fence's lines are colored from the
   wrong text until the next edit. The old code compared the real body text each time and never did this.
Fix: make a cache hit also require the same body bounds (e.g. include the fence's body end line / close line and closed state,
or the `from`/`to` offsets, in the entry and in `TryGet`/`Get`). Keep the one-copy-per-unchanged-fence behavior.
Add one case to the existing `FenceCacheChecks` in `SelfTest.cs`: same document version, same open line, different body bounds
must miss and rebuild.

## Small tidy (same files only)
`RenderPaint.Brush` and `ChevronPenSet` pass `null!` as the transform; use the constructor overloads without it (or a plain
`null` where the parameter is nullable), and pass `null` instead of an empty `ImmutableDashStyle` if `ImmutablePen` accepts it.
No visual change.

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a28-check`: 0 errors, 0 warnings;
  `--selftest` prints SELFTEST PASS. Delete `build/a28-check` and `build/a28-measure` if you can.
- Only `Editor/Structure.cs`, `Editor/Renderers.cs`, `Editor/ChevronGutter.cs` and `SelfTest.cs` change.
- Append a "Rework" section to `.coord/reports/A28-editor-render-memory.md`.
- Never launch or kill AntiNotepad; never touch `%LOCALAPPDATA%\AntiNotepad`, `Programs\AntiNotepad` or real Documents.
