# A4a-token-scan-r2: fix the scanner (rework of round 1)

Same item card as `.coord/tasks/A4a-token-scan.md`: read it fully (Plan, Files, For Codex, Done means). Round 1 left files
in the tree (tools/check-design-tokens.ps1, tools/design-tokens-baseline.tsv, tools/gate.ps1 Tokens step, docs/loop.md,
docs/coding.md, the inventory TSV) and a self-reported report `.coord/reports/A4a-token-scan.md`. Keep what works; fix:

1. **String contents**: patterns must not match text inside ordinary string literals (a string holding `new CornerRadius(5)`
   is not a radius). Only these string contents stay visible: the argument of `new FontFamily(...)`/`Typeface(...)` and
   `#RRGGBB[AA]` color strings. Implement a real masker: blank string contents (keep quotes), except those two cases; handle
   verbatim `@"..."`, raw `"""..."""`, interpolated `$"..."` (holes `{...}` are code, including nested strings/comments inside
   holes), char literals. Add SelfCheck fixtures for each (string with radius text ignored, hole with nested string, raw string).
2. **Missed literal forms** (inventory is far too low: 169 hits vs ~520 in the A59 inventory). Add and count: `FontSize = n` is
   covered, but also positional/size parameters named like `size`/`fontSize`, `FontWeight.X` enum uses (category font-weight),
   `FontWeight = FontWeight.X`, `Color.Parse("#...")`, `Color.FromArgb/FromRgb(...)` any numeric arg, `Colors.<Name>` is NOT
   counted, `BoxShadow` constructions with numeric args (`new BoxShadow { Blur = ... }` and positional), `Opacity =` values other
   than 0/1 including expressions with a literal (e.g. `opacity * 0.5`), `TimeSpan.From...` for tween vs timer (Duration,
   `*Ms` literals passed to Tween/animation helpers in Editor/Motion.cs and UI/NoteSwipe.cs), `LetterSpacing`, `MinWidth/Height/
   Width/Height/Spacing`, `Margin/Padding/BorderThickness`, `new Thickness(...)` (all of spacing). Sample the product files in
   `UI/` and `Editor/` yourself (grep) and make sure every visible design literal kind shows up; list in the report any kind you
   deliberately leave out and why.
3. Regenerate baseline and inventory; the baseline must equal `-Update` output. Re-run: -SelfCheck, default check, scratch-edit
   probe (revert), build with the dedicated folder (then leave the folder; the manager removes it), offline gate.
4. Report: overwrite `.coord/reports/A4a-token-scan.md` with the exact sections from the card, final state only (no "not ready"
   unless something is truly still wrong). Counts per category and top 10 files as before.

Do not touch dev, STATUS, product files, or check-platform-seam.ps1. Do not commit.
