# A42b-note-analysis-file: move NoteAnalysis out of PadView.cs into its own file

Role: worker (gpt-6-luna xhigh). **Implementation.** Product `product/AntiNotepad`. Read `AGENTS.md`, then the plan
`.coord/tasks/A42b-note-analysis-file-plan.md` and `.coord/reports/A42b-note-analysis-file-impact.md`. Branch
`reorg/A42b-note-analysis-file` (already checked out; do not switch branches, do not commit: you cannot write to `.git`).
Behavior-neutral refactor, verbatim move. Line numbers below are from the current tree; re-read before editing.

## Files in scope
- New `Editor/NoteAnalysis.cs`, namespace `AntiNotepad.Editor`. Receives, **character for character** (including the
  `/// <summary>` line above it), `internal sealed class NoteAnalysis` from `Editor/PadView.cs` (~:877-1132) with its nested
  private records `CachedLine`, `TimerLine` and `internal static TimerKey`. Imports: `System.Runtime.CompilerServices`,
  `AvaloniaEdit.Document`, `AntiNotepad.Calc`, `AntiNotepad.Model` (implicit usings cover System/Linq/Collections; add nothing else,
  and no `AntiNotepad.Editor` using is needed). Use a file-scoped namespace or match how `Editor/MathNotes.cs` does it.
- `ChangedLineRange` (PadView.cs ~:493-500) moves into `NoteAnalysis` as the first member, body unchanged; the ONLY change is
  `private static` -> `internal static`.
- `Editor/PadView.cs`: delete the moved class, the old `ChangedLineRange`, the `PadView.TimerKey` shim (~:540) and
  `using System.Runtime.CompilerServices;` (line 3, now unused). `OnDocumentChanged` (~:459) becomes
  `NoteAnalysis.ChangedLineRange(doc, e)`. Touch no other import and no other line.
- `SelfTest.cs`: the two `Editor.PadView.TimerKey(...)` calls (~:1018-1019) become `Editor.NoteAnalysis.TimerKey(...)`; add the
  16 rows below to the `LayerRules` table right after the `ListEditing` rows (same schema/style as them).
- Nothing else. No behavior fixes, no scheduler/cache extraction, no import cleanup beyond the one using.

## LayerRules rows to add
```csharp
new("NoteAnalysis", "PadView", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Editor.PadView)),
new("NoteAnalysis", "UI", ExactFrom: typeof(Editor.NoteAnalysis)),
new("NoteAnalysis", "Platform", ExactFrom: typeof(Editor.NoteAnalysis)),
new("NoteAnalysis", "Motion", ExactFrom: typeof(Editor.NoteAnalysis)),
new("NoteAnalysis", "Theme", ExactFrom: typeof(Editor.NoteAnalysis)),
new("NoteAnalysis", "PadWindow", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(PadWindow)),
new("NoteAnalysis", "Program", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Program)),
new("NoteAnalysis", "Native", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Native)),
new("NoteAnalysis", "Window", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Avalonia.Controls.Window)),
new("NoteAnalysis", "Control", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Avalonia.Controls.Control)),
new("NoteAnalysis", "Dispatcher", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Avalonia.Threading.Dispatcher)),
new("NoteAnalysis", "Settings", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Settings)),
new("NoteAnalysis", "SettingsService", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(SettingsService)),
new("NoteAnalysis", "TextEditor", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(AvaloniaEdit.TextEditor)),
new("NoteAnalysis", "TextArea", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(AvaloniaEdit.Editing.TextArea)),
new("NoteAnalysis", "TextView", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(AvaloniaEdit.Rendering.TextView)),
```
Use the real spellings the existing rows use (adjust only if one does not compile). No rows for `Timers` or `TextDocument`
(both are required by the class). No `Avalonia.*` namespace-string rows (the matcher ignores them). Do not prove the guards
fail yourself; the manager does.

## Rules
- Verbatim: no edits inside the class, same visibility, same type and nested type names. US spelling. Do not launch or kill
  AntiNotepad. Do not commit.
- Build: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A42b-codex-check` (0 warnings), then
  `build/A42b-codex-check/AntiNotepad.exe --selftest` must print `SELFTEST PASS` with all existing lines (LAYERS, SETTINGS,
  HOTKEYS, CLIPBOARD, MATH NOTES, LIST EDITING, A29 checks) plus the new rows at 0 refs. Delete `build/A42b-codex-check` afterwards.

## Done means
- Build 0 warnings 0 errors; `SELFTEST PASS`.
- `grep -n "class NoteAnalysis" product/AntiNotepad/Editor/PadView.cs` and `grep -rn "PadView.TimerKey" product` find nothing;
  `grep -n "PadView" product/AntiNotepad/Editor/NoteAnalysis.cs` finds nothing.
- PadView.cs about 864 lines (from 1,132), NoteAnalysis.cs about 272.
- Short report at `.coord/reports/A42b-note-analysis-file.md`: files changed, PadView line count before/after, selftest LAYERS
  summary line, any deviation and why.
