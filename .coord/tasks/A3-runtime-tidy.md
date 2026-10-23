# A3-runtime-tidy

You are a worker (gpt-6-luna xhigh). Repo: this folder, a git worktree on branch `feat/A3-runtime-tidy`. Never switch branch,
merge or commit (.git is read-only for you). App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10).
Context: `docs/audit/A1-codebase-audit.md`, findings A1-22, A1-24 and A1-25.

## Goal
Three small fixes. Keep each minimal and match the surrounding style.
1. **A1-22-json-options-per-command** (`Editor/Completion.cs` ~:180): replace the per-call
   `new JsonSerializerOptions { WriteIndented = true }` with one `private static readonly` instance. Output stays identical.
2. **A1-24-disabled-caret-still-ticks** (`Editor/Motion.cs` ~:225-237, `SmoothCaret`): when `Settings.CaretBlink` is off,
   don't keep `_blink` running. The caret stays solid and visible, and it is still repainted when it moves. When the setting
   changes while the editor is focused, blinking starts or stops without needing a refocus. Find how the setting reaches
   PadView (for example `ApplyLook`, or wherever Settings changes are pushed) and hook into that existing path; don't add
   a new event system. Keep the existing "solid for 600 ms after movement" behavior when blinking is on.
3. **A1-25-tray-empty-title-sentinel** (`Program.cs` ~:116, `Platform/Tray.cs` ~:31): stop using the display string
   `"empty"` as a sentinel. Pass occupied state explicitly, for example a list of `(string Title, bool Occupied)`. The menu
   text is unchanged: empty slots still read `empty` and stay disabled, and a slot whose real title is "empty" is enabled.
Files in scope: `Editor/Completion.cs`, `Editor/Motion.cs`, `Program.cs`, `Platform/Tray.cs`, plus at most a one-line hook in
`Editor/PadView.cs` if item 2 needs it.

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a3-check`: 0 errors, 0 warnings.
  `build/a3-check/AntiNotepad.exe --selftest` prints SELFTEST PASS. Then delete `build/a3-check`. Never launch or kill
  AntiNotepad otherwise.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Write a short report at `.coord/reports/A3-runtime-tidy.md`: what changed per finding, how the caret setting change is
  picked up, files changed, build and selftest result.
