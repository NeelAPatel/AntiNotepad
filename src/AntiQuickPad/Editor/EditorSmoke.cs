using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Rendering;
using CoreText = AntiQuickPad.Core.Text;

namespace AntiQuickPad.Editor;

internal static class EditorSmoke
{
    /// <summary>Small deterministic M3 smoke checks, called by the app selftest.</summary>
    public static void RunSmoke()
    {
        var fixture = "# Heading\n**bold** *italic* ~~gone~~\n    - [ ] task\n```python\ndef f(): # comment\n    return 'ok'\n```\n";
        var document = new TextDocument(fixture);
        var scanner = new MarkdownScanner(document);
        var lines = scanner.Lines;
        if (lines[0].HeadingLevel != 1 || lines[1].Bold.Count != 1 || lines[1].Italic.Count != 1 || lines[1].Strike.Count != 1 ||
            lines[2].CheckboxOffset < 0 || !lines[4].IsPython || lines[4].CommentOffset < 0 || scanner.GetFoldings().Count < 2)
            throw new InvalidOperationException("Markdown scanner fixture classification failed.");
        var sections = new TextDocument("# One\nbody\n\n\noutside\n# Two\nbody\n## Child\nchild\n# Three\nend");
        var sectionFolds = new MarkdownScanner(sections).GetFoldings();
        if (sectionFolds[0].EndOffset != sections.GetLineByNumber(2).EndOffset ||
            sectionFolds[1].EndOffset != sections.GetLineByNumber(9).EndOffset ||
            sectionFolds[2].EndOffset != sections.GetLineByNumber(9).EndOffset)
            throw new InvalidOperationException("Heading blank-pair / next-heading boundary failed.");
        var nested = new TextDocument("# Section\n```\na\n\n\nb\n```\ntail\n\n\noutside");
        var nestedFolds = new MarkdownScanner(nested).GetFoldings();
        if (nestedFolds.Count != 2 || nestedFolds[0].EndOffset != nested.GetLineByNumber(8).EndOffset ||
            nestedFolds[1].EndOffset != nested.GetLineByNumber(6).EndOffset)
            throw new InvalidOperationException("Independent fence folding failed.");
        foreach (var (input, expected) in new (string, string)[]
        {
            ("- text", "- text\n- "), ("  - text", "  - text\n  - "), ("\t* text", "\t* text\n\t* "),
            ("- [ ] x", "- [ ] x\n- [ ] "), ("- [x] x", "- [x] x\n- [ ] "),
            ("1. text", "1. text\n2. "), ("9. text", "9. text\n10. "),
            ("- ", ""), ("* ", ""), ("1. ", ""), ("  - [ ] ", "  "), ("- [x] ", "")
        })
        {
            var list = new TextDocument(input);
            var newCaret = EditorOperations.ContinueList(list, list.TextLength);
            if (newCaret != list.TextLength || list.Text.Replace("\r\n", "\n") != expected)
                throw new InvalidOperationException($"List Enter failed for '{input}'.");
            list.UndoStack.Undo();
            if (list.Text != input || list.UndoStack.CanUndo) throw new InvalidOperationException("List Enter was not one undo step.");
        }
        foreach (var (input, expected, outdent) in new (string, string, bool)[]
        {
            ("- ", "    - ", false), ("* ", "    * ", false), ("- [ ] ", "    - [ ] ", false),
            ("8. ", "    1. ", false), ("3. parent\n    1. ", "3. parent\n4. ", true),
            ("    - [ ] ", "- [ ] ", true), ("\t* ", "* ", true)
        })
        {
            var item = new TextDocument(input);
            var tab = EditorOperations.ApplyTab(item, item.TextLength, null, null, outdent);
            if (item.Text != expected || tab.CaretOffset != item.TextLength) throw new Exception($"Empty list indent failed: {input}");
            item.UndoStack.Undo();
            if (item.Text != input || item.UndoStack.CanUndo) throw new Exception("Empty list indent must be one undo step.");
        }
        Console.WriteLine("T007 indent PASS: 7 empty marker indent/outdent/numbering cases and single undo.");
        var checkbox = document.GetLineByNumber(3).Offset + lines[2].CheckboxOffset;
        var caret = document.TextLength;
        if (EditorOperations.ToggleCheckbox(document, checkbox, ref caret) != "[x]" || document.GetText(checkbox, 3) != "[x]" || !document.UndoStack.CanUndo)
            throw new InvalidOperationException("Checkbox toggle was not an undoable text edit.");
        document.UndoStack.Undo();
        var taskLine = document.GetLineByNumber(3);
        var result = EditorOperations.ApplyTab(document, checkbox, null, null, outdent: true);
        if (!result.Changed || document.GetText(taskLine.Offset, 5) != "- [ ]")
            throw new InvalidOperationException("Shift+Tab outdent smoke failed.");
        var selected = new TextDocument("    one\n    two");
        EditorOperations.ApplyTab(selected, selected.TextLength, 0, selected.TextLength, true);
        if (selected.Text != "one\ntwo") throw new InvalidOperationException("Selection outdent failed.");
        var typed = new MarkdownScanner(new TextDocument("python:\nreturn '# string' # comment"));
        var fenced = new MarkdownScanner(new TextDocument("```\npython:\nreturn 1\n```"));
        if (!typed.Lines[1].IsPython || typed.Lines[1].CommentOffset != 18 || !fenced.Lines[2].IsPython)
            throw new InvalidOperationException("Python language marker classification failed.");
    }

}
