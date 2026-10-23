# A2-dead-code-tidy

You are a worker (gpt-6-luna xhigh). Repo: this folder, a git worktree on branch `feat/A2-dead-code-tidy`. Never switch branch,
merge or commit (.git is read-only for you). App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10).
Context: `docs/audit/A1-codebase-audit.md`, findings **A1-21-zero-caller-api** and **A1-23-leftover-icon-and-fence-state**.

## Goal
Remove dead code with no behavior change.
1. A1-21: before removing each member, grep the whole repo (including `SelfTest.cs`) to confirm it has no reference. Then
   remove it. The members: `Anim.After` (UI/Anim.cs), `Kit.Segmented` and `Kit.Switch` (UI/Kit.cs), `Palette.B` and
   `Palette.Brush` (Theme/Palette.cs), `Modes.CodeLanguages` (Model/Modes.cs), `Store.IsLoaded` and `Store.Folder`
   (Model/Store.cs), and the write-only `WasAbandoned` (Platform/SingleInstance.cs, removing its assignment too). Never remove
   a property that is serialized to JSON (anything on `StoreData`, `Note` or `Settings`). If a member turns out to be used,
   keep it and say so in the report.
2. A1-23: in `UI/Kit.cs` remove the `GlyphIcon.Snap` field and inline its always-true branch, so snapping is unconditional.
   Rewrite the stale icon-lab comment and fix the GlyphIcon summary so it describes current behavior: weight 0 draws the
   glyph once, and the faux-bold copies happen only when weight is above 0. Check what `Render` actually does before writing
   it. In `Editor/Structure.cs` remove the never-assigned `Fence? open` local and its checks; `openLine` stays the
   discriminator.
Files in scope: only those named above. Match the surrounding style and comment density.

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a2-check`: 0 errors, 0 warnings.
  `build/a2-check/AntiNotepad.exe --selftest` prints SELFTEST PASS. Then delete `build/a2-check`. Never launch or kill
  AntiNotepad otherwise.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Write a short report at `.coord/reports/A2-dead-code-tidy.md`: each member removed or kept (with its grep evidence), files
  changed, build and selftest result.
