Implemented **A28-fence-cache-bounds-fix**. Fence cache hits now require matching body end line and closed state; a bounds change forces a fresh body copy. Added the same-version miss-and-rebuild selftest in [SelfTest.cs](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad-wt/A28-editor-render-memory/product/AntiNotepad/SelfTest.cs:750>). Also simplified the brush and pen constructors in [Renderers.cs](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad-wt/A28-editor-render-memory/product/AntiNotepad/Editor/Renderers.cs:15>) and [ChevronGutter.cs](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad-wt/A28-editor-render-memory/product/AntiNotepad/Editor/ChevronGutter.cs:44>).

The [Rework report](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad-wt/A28-editor-render-memory/.coord/reports/A28-editor-render-memory.md:67>) is appended.

- Release build: **0 warnings, 0 errors**
- `--selftest`: **SELFTEST PASS**
- `git diff --check`: passed
- Cleanup was attempted, but tool policy rejected recursive removal. `build/a28-check` and `build/a28-measure` remain.