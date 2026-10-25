# A28-editor-render-memory

Completed 2026-10-25 on `feat/A28-editor-render-memory`. No branch switch, commit, merge, normal app launch, process termination or installed-data access. STATUS and the audit were left unchanged as requested.

## Split and files

Director reviewed the implementation, measured resources, built, ran selftest and wrote this report. Three gpt-6-luna / xhigh workers shared the worktree with disjoint ownership:

| Worker | Changed files under product/AntiNotepad |
|---|---|
| a28_fence_cache | Editor/Structure.cs; SelfTest.cs |
| a28_chip_chevron | Editor/Renderers.cs; Editor/ChevronGutter.cs |
| a28_glyph_fade | Editor/Colorizers.cs; Editor/PadView.cs; UI/Kit.cs (GlyphIcon only) |

No other product files changed. PadView contains the edge fade and the required one-line pruning call after structure rebuild. This report is the only additional deliverable; the temporary measurement harness and build output remain under build because cleanup was rejected by tool policy.

## Design as built

- Fence cache keys are pruned against current fence opening lines after every structure rebuild. Binding resets the cache. Removed/replaced/reset highlighter payloads are disposed. Entries are bounded by current fences after each structure update.
- A document-version/language/palette hit returns before computing body offsets, delegates or copies. A new version copies the body once per cached fence, compares text, and retains highlighting when unchanged. Palette-only changes reuse stored body text. Unsupported definitions are cached too.
- Chip FormattedText keys include text, font, culture name, size, color and render scale; typeface is cached by font. Immutable paint resources and timer geometries are cached by drawing inputs. Existing labels, layout, opacity and dimensions are preserved.
- Renderers own bounded insertion-order caches: chip text 128, chip brushes 16, timer paints 8, timer geometries 16; paper brushes/pens 8 each; divider pens 4; checkbox paints/geometries 8 each. These additional resources are hot paths in the allowed Renderers.cs.
- Each gutter owns one three-pen immutable set, replaced on palette or render-scale change. Folded/hover states select the same semantic colors as before.
- Each note/math colorizer owns a color-keyed immutable brush cache capped at 24 entries. Changed palette colors select new resources immediately; brushes do not depend on font/scale.
- Each GlyphIcon retains one FormattedText and immutable brush, refreshed on render-scale change. Glyph/font/size/color are fixed constructor inputs; existing palette changes rebuild toolbar icons. Weighted drawing keeps the five original offsets without allocating an array.
- Each PadView holds one immutable fade mask keyed by effective alpha bytes and band fraction. Changed scroll/resize inputs refresh it; identical stops reuse it. Its black alpha stops do not depend on theme/accent/font. Disabling the fade or releasing the view clears the cache.

All caches are instance-owned; there is no growing static resource cache or later mutation of shared cached brushes/pens. Unrelated allocations and editor analysis were left alone.

## Measurement

Temporary .NET 10 Release console harness with the product's Avalonia 12.1.4 / AvaloniaEdit 12.0.0 versions. It linked the actual ChevronGutter.cs and Palette.cs plus a minimal PadView compile stub. Before used the original mutable brush/pen construction expression; after used the production ChevronPens cache. Each case warmed up 20,000 times, then ran 1,000,000 iterations with Stopwatch and GC.GetAllocatedBytesForCurrentThread, retaining returned resources in a sink.

| Isolated resource workload | Before | After |
|---|---:|---:|
| One visible chevron resource | 1,944 bytes; 769.73 ns | approximately 0 bytes; 39.99 ns |
| Resource preparation per frame with 20 visible chevrons | 38,880 bytes; 11,255.26 ns | approximately 0 bytes; 115.85 ns |

This measures the resource-preparation portion of a render including cache lookup, excluding enumeration, drawing submission, composition and GPU work. It is not a full-window frame-time claim. Tiny negative sub-byte results from fixed harness-overhead subtraction were rounded to zero. Live chip/glyph layout and full-app performance were not measured.

Initial harness restore hit the restricted NuGet network path. Offline restore from the existing local package cache succeeded. No package versions or repository NuGet settings changed. The final required build restored successfully from available local state.

## Verification

- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a28-check`: succeeded, **0 warnings, 0 errors**. The first build caught two nullable Typeface value-type errors; the worker fixed them before the successful build.
- `build/a28-check/AntiNotepad.exe --selftest`: exit 0, **SELFTEST PASS**.
- `git diff --check`: passed.
- SelfTest.cs adds exactly one method at the end and one call in Run. Checks: two fences cached; 100 unchanged accesses preserve body-copy/build counts; a new version with identical body copies once and retains its payload; moving/deleting fences removes obsolete keys and retains only the current moved fence; chevron resource reuse and replacement after palette/scale changes; colorizer brush reuse and replacement after a color change.
- Checks use production cache APIs without reflection, test-only hooks, windows, or a retained old comparison path. They do not execute the live Avalonia colorization/render pipeline.

Cleanup: both `build/a28-check` and `build/a28-measure` remain. Their absolute paths were verified inside this worktree's build directory, but the tool policy rejected the recursive Remove-Item command (blocked by policy). No alternate deletion route was attempted. The manager can remove these disposable folders locally.

## Needs a live check

All items below **need a live check** in the dev copy. Normal app launch was prohibited in this run:

- Code-block colors while moving/deleting/editing fences, including open fences, multiline syntax and multiple languages.
- Answer chips in inline/math notes and all timer states; alignment, updates, narrow windows and hover/click behavior.
- Fold chevrons expanded/folded, pointer hover and exit.
- Edge fade at top/bottom/middle, first/last lines, scrolling, resizing and toggling the setting.
- Toolbar glyph appearance, pixel snapping, natural/weighted drawing and hover states.
- Repeat in light/dark after theme, palette, accent, font and UI-scale switches, and between monitors with different display scales. Confirm immediate updates and matching appearance.
- Paper patterns, dividers and checkbox marks in the same conditions, since their hot resources in Renderers.cs were cached too.

No implementation was deferred. Cleanup was blocked as described above. Live visual equivalence, chip/glyph render-resource identity and end-to-end frame performance remain unverified.

## Rework — A28-fence-cache-bounds-fix

- Fixed the deferred-analysis race by storing each entry's body end line and closed state. `TryGet` now requires both bounds to match alongside document version, language and palette. `Get` reuses the stored body text only when its version and bounds match; if bounds changed within the same document version, it copies the newly bounded body before deciding whether the existing highlighter can be retained.
- Added one `FenceCacheChecks` case with the same opening line and document version but changed body bounds. It asserts the fast lookup misses, the body is copied, and changed body text rebuilds the highlight.
- Simplified `RenderPaint.Brush` and `ChevronPenSet` construction by using the transform-free brush overloads and null dash styles; no drawing inputs or visual settings changed.
- Rework verification: the requested Release build succeeded with **0 warnings, 0 errors**, and `build/a28-check/AntiNotepad.exe --selftest` exited 0 with **SELFTEST PASS**.
- Cleanup was attempted for `build/a28-check` and `build/a28-measure`, but the tool policy rejected the recursive removal command. Both folders remain; no alternate removal route was attempted.
