# A22-wrap-hanging-indent-forksize: size a patch to AvaloniaEdit's wrap layout

Role: worker (gpt-6-luna xhigh), read-only, offline. Never launch AntiNotepad; write no files; final message is the report.
Read `AGENTS.md`, `.coord/tasks/A22-wrap-hanging-indent.md` and `.coord/reports/A22-wrap-hanging-indent.md` (probe: no product-only hook).

User question (2026-11-17): how big is a fork of AvaloniaEdit 12.0.0 (MIT, vendored into the repo) that gives soft-wrapped continuation lines a left indent, with caret, selection, hit-testing and drawing agreeing?

Idea to test: `TextView.BuildVisualLine` formats each text line with `TextFormatter.FormatLine(source, index, paragraphWidth, props, previousBreak)`. For continuation lines, format with `paragraphWidth - indent`, and add `indent` to X in every place that converts between visual X and text-line positions (drawing, `VisualLine.GetVisualPosition`/`GetTextLineVisualXPosition`, `GetVisualColumn` from point, `GetTextViewPosition`, selection rectangle builder, caret placement, `TextView.GetPosition`). Avalonia's formatter ignoring `Indent` is then irrelevant.

Answer, from the cached DLLs/IL (no network): 
1. Is the idea sound? Which exact methods compute or consume per-line X, and which need an indent? List by name with metadata tokens.
2. Rough size: methods to patch, lines changed, new files. Does a source fork even need the full AvaloniaEdit source (needs a network fetch of the 12.0.0 tag by the user/manager), or is a smaller approach possible (subclassing is blocked: sealed/private/internal; Harmony-style runtime patching: say if viable and why it is or is not acceptable)?
3. Risks: upgrade burden, license, build time, selftest and gate effects, AvaloniaEdit features we use (folding? search? check `product/AntiNotepad/Editor` usage).
4. Recommendation: fork, other route, or not worth it; effort S/M/L; end with `READY: yes` or `READY: no - <why>`.
