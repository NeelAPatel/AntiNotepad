# Coding standard

How AntiNotepad code is written and laid out (user, 2026-10-29). It applies **on touch**: any file an item changes is
brought up to this standard in that item, and the Check phase looks for it. There is no whole-codebase pass.

## Comments
Comments read like a short guide for a technical reader who is new to the project: concise and direct, no unexplained
jargon, not oversimplified, not wordy. A comment must stay correct and in place when the code below it changes later.

1. **File header.** Each file opens with 1-3 lines: what the file is for and where it fits.
2. **Summaries.** Each class and non-trivial method gets a `///` summary of what it does and why, not how.
3. **Inline `//` only for the non-obvious:** a Win32 quirk, a timing reason, a trap the next reader would fall into.
4. **No history in code:** no item IDs, dates or "changed from X". Git and the item card hold history.
5. **One word per idea:** define a term on first use, or use the word from `docs/glossary.md`.
6. **Short:** one to three lines. Anything longer goes in `docs/` and the comment points there.
7. **Pipeline line.** A method with several stages that must stay one method opens with its stages in order, and each
   stage is a block separated by a blank line and headed by its slug (on the line before, or at the end of its first
   line):

```csharp
/// <summary>Turns one math line into its result chip text.</summary>
private static string Evaluate(string line)
{
    // lex -> parse -> resolve units -> format

    // lex
    var tokens = Lex(line);

    // parse
    var tree = Parse(tokens);

    var value = Resolve(tree);   // resolve units

    // format
    return Format(value);
}
```

## Formatting
- Rules live in `.editorconfig` at the repo root: UTF-8, spaces, 4-space indent for C# and PowerShell, file-scoped
  namespaces, braces on their own line, `var`, PascalCase types, members and static readonly fields, `_camelCase`
  private fields.
- The gate's Format step (`tools/gate.ps1`, `dotnet format`) checks only the files an item changes and fails only on
  problems in changed lines. Style preferences are suggestions, shown in the editor, not gate failures.
- No reformat-only commits: formatting is fixed on touch.

## Files: one job each
Split by **job**, not by line count. A part of a file is a separate job when at least one of these holds:
1. **It changes for different reasons**: unit-table edits and parser fixes are different kinds of items.
2. **It is a pipeline stage** with a clear input and output: lex, parse, evaluate, format.
3. **It sits in a different layer**: Win32 and interop, UI, model.
4. **It is a self-contained set** with its own types, such as the search cache.

Keep parts together when they share a lot of private state, or when neither can be read without the other.

- **Line count is a trigger, not a cap.** A file over **600 lines** gets a "does this do one job?" check in Findings.
  The answer is written on the card; a split it calls for becomes its own planned item (Low tier, `pure-move`), never a
  drive-by.
- **How to split:**
  - partial files named `<Type>.<Part>.cs` (like `UI/PadWindow.Layout.cs`) when the parts share state;
  - a separate class in its own file when a part stands alone;
  - a folder once three or more files belong together.
- One main type per file, and the file is named after it.

## Folders and namespaces
- The namespace mirrors the folder: `Model/Search/` is `AntiNotepad.Model.Search`.
- When three or more files share a job, or a folder passes about **12 files**, they move into a subfolder.
- An item that splits or moves files does all of the reorganizing in the same item, and its Plan lists every move:
  - move the files and rename their namespaces;
  - fix every `using` line (C# resolves by namespace, not file path; the build gate catches any reference missed);
  - update the selftest LAYERS rules, which match namespaces, so the new sub-namespace stays under its parent's rule;
  - update the code layout lines in `AGENTS.md`.

## Planned splits (era two)
Found 2026-10-29; each becomes an A item when the user confirms it.
| File | Lines | Jobs found | Plan |
|---|---|---|---|
| `SelfTest.cs` | 3,247 | dozens of independent checks | a `SelfTest/` folder by area (`SelfTest/Contracts.cs` exists) |
| `Calc/Calculator.cs` | 1,383 | units, dates, lexer, parser, formatting | five files in `Calc/` |
| `Model/Store.cs` | 1,006 | search (about 400 lines), `Note`, `Settings`, `Store` | `Model/Search/`, `Note.cs`, `Settings.cs`, `Store.cs` |
| `Editor/PadView.cs` | 826 | one pane: edge fade, pointer and links, keys and timers | partial files, on touch only |
