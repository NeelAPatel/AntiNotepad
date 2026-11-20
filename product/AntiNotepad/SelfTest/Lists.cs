using AntiNotepad.Model;

namespace AntiNotepad;

internal static partial class SelfTest
{
    private static bool ListEditingChecks(out string report)
    {
        var failures = new List<string>();
        var checks = 0;
        void CheckCase(bool passed, string name) { checks++; if (!passed) failures.Add(name); }

        void ToggleCase(string text, Editor.ListEditing.LineEdit expected, string name) =>
            CheckCase(Editor.ListEditing.ToggleCheckbox(text) == expected, name);
        void IndentCase(string text, bool deeper, Editor.ListEditing.LineEdit? expected, string name) =>
            CheckCase(Editor.ListEditing.Indent(text, deeper) == expected, name);
        void ContinueCase(string text, int lineNumber, NoteMode mode,
            Editor.ListEditing.ContinuationKind kind, string? insertion, string name)
        {
            var result = Editor.ListEditing.Continue(text, lineNumber, mode, Editor.ListEditing.ListKind.Bullet);
            CheckCase(result.Kind == kind && result.InsertText == insertion, name);
        }
        void RenumberCase(string[] lines, int lineNumber, bool fresh,
            Editor.ListEditing.NumberEdit[] expected, string name)
        {
            var result = Editor.ListEditing.Renumber(lines.Length, number => lines[number - 1], lineNumber, fresh);
            CheckCase(result.SequenceEqual(expected), name);
        }

        foreach (var (word, expected) in new[] { ("list", "number"), ("todo", "check"), ("checklist", "check"),
            ("bullet", "bullet"), ("bulletlist", "bullet"), ("numberlist", "number") })
        {
            CheckCase(Modes.Detect(word).Mode == NoteMode.List && Modes.ListKindOf(word, "number") == expected,
                $"{word} resolves its list kind");
        }
        foreach (var (kind, marker) in new[] { ("bullet", "- "), ("check", "[ ] "), ("number", "1. ") })
        {
            var result = Editor.ListEditing.Continue("plain", 2, NoteMode.List, Editor.ListEditing.KindOf(kind));
            CheckCase(result.InsertText == "\n" + marker, $"{kind} plain line continuation");
            CheckCase(Modes.ListKindOf("list:", kind) == kind, $"list follows {kind}");
        }
        CheckCase(Modes.ListKindOf("list", "invalid") == "bullet", "unknown list default falls back to bullet");
        CheckCase(Editor.Completion.ListDefaultName("check") == "checkboxes"
            && Editor.Completion.ListDefaultName("number") == "numbers"
            && Editor.Completion.ListDefaultName("bullet") == "bullets", "popup names list defaults");

        CheckCase(Editor.ListEditing.TryListNumber("123456789. item", out var nineDigits) && nineDigits == 123_456_789,
            "9-digit list marker accepted");
        CheckCase(!Editor.ListEditing.TryListNumber("1234567890. item", out _),
            "10-digit list marker rejected");
        CheckCase(Editor.ListEditing.IsListLine("- item") && !Editor.ListEditing.IsListLine("1234567890. item"),
            "list-line recognition validates numbered marker size");

        foreach (var (text, markerEnd) in new[] { ("- item", 2), ("  [x] item", 6), ("12) item", 4), ("- [ ] item", 6) })
        {
            CheckCase(Editor.ListEditing.IsAtTextStart(text, 0), $"before marker: {text}");
            CheckCase(Editor.ListEditing.IsAtTextStart(text, markerEnd - 1), $"inside marker: {text}");
            CheckCase(Editor.ListEditing.IsAtTextStart(text, markerEnd), $"text start: {text}");
            CheckCase(!Editor.ListEditing.IsAtTextStart(text, markerEnd + 1), $"after text start: {text}");
            CheckCase(Editor.ListEditing.TabDecision(text, markerEnd, false, false, false) == Editor.ListEditing.TabAction.Indent, $"Tab at start: {text}");
            CheckCase(Editor.ListEditing.TabDecision(text, markerEnd, true, false, false) == Editor.ListEditing.TabAction.Outdent, $"Shift+Tab at start: {text}");
            CheckCase(Editor.ListEditing.TabDecision(text, markerEnd + 1, false, false, false) == Editor.ListEditing.TabAction.InsertSpaces, $"Tab in text: {text}");
            CheckCase(Editor.ListEditing.TabDecision(text, markerEnd + 1, true, false, false) == Editor.ListEditing.TabAction.Swallow, $"Shift+Tab in text: {text}");
            CheckCase(Editor.ListEditing.TabDecision(text, markerEnd, false, true, false) == Editor.ListEditing.TabAction.InsertSpaces, $"Ctrl+Tab: {text}");
        }
        CheckCase(Editor.ListEditing.TabDecision("plain", 0, false, false, false) == Editor.ListEditing.TabAction.Default, "plain Tab stays default");
        CheckCase(Editor.ListEditing.TabDecision("- item", 0, false, false, true) == Editor.ListEditing.TabAction.Default, "Alt+Tab stays default");
        CheckCase(!Editor.ListEditing.IsAtTextStart("1234567890. item", 0), "invalid list marker is not text start");

        ContinueCase("- ab", 1, NoteMode.Plain, Editor.ListEditing.ContinuationKind.InsertAtCaret, "\n- ", "bullet continuation");
        ContinueCase("* ab", 1, NoteMode.Plain, Editor.ListEditing.ContinuationKind.InsertAtCaret, "\n* ", "star bullet continuation");
        ContinueCase("+ ab", 1, NoteMode.Plain, Editor.ListEditing.ContinuationKind.InsertAtCaret, "\n+ ", "plus bullet continuation");
        ContinueCase("9. ab", 1, NoteMode.Plain, Editor.ListEditing.ContinuationKind.InsertAtCaret, "\n10. ", "period-number continuation");
        ContinueCase("9) ab", 1, NoteMode.Plain, Editor.ListEditing.ContinuationKind.InsertAtCaret, "\n10) ", "parenthesis-number continuation");
        ContinueCase("[ ] ab", 1, NoteMode.Plain, Editor.ListEditing.ContinuationKind.InsertAtCaret, "\n[ ] ", "unchecked box continuation");
        ContinueCase("[x] ab", 1, NoteMode.Plain, Editor.ListEditing.ContinuationKind.InsertAtCaret, "\n[ ] ", "checked box continuation resets unchecked");
        ContinueCase("[X] ab", 1, NoteMode.Plain, Editor.ListEditing.ContinuationKind.InsertAtCaret, "\n[ ] ", "uppercase checked box continuation resets unchecked");
        ContinueCase("- [X] ab", 1, NoteMode.Plain, Editor.ListEditing.ContinuationKind.InsertAtCaret, "\n- [ ] ", "bullet-wrapped checkbox continuation resets unchecked");
        ContinueCase("  - ab", 1, NoteMode.Plain, Editor.ListEditing.ContinuationKind.InsertAtCaret, "\n  - ", "nested bullet continuation keeps indent");
        ContinueCase("  - ", 1, NoteMode.Plain, Editor.ListEditing.ContinuationKind.Outdent, null, "empty nested item outdents");
        ContinueCase("- ", 1, NoteMode.Plain, Editor.ListEditing.ContinuationKind.ClearLine, null, "empty root item clears");
        ContinueCase(" - ", 1, NoteMode.Plain, Editor.ListEditing.ContinuationKind.ClearLine, null, "one-space empty item clears instead of outdenting");
        ContinueCase("\t- ", 1, NoteMode.Plain, Editor.ListEditing.ContinuationKind.ClearLine, null, "tab-only one-character indent clears");
        ContinueCase(" \t- ", 1, NoteMode.Plain, Editor.ListEditing.ContinuationKind.Outdent, null, "mixed two-character indent outdents");
        ContinueCase("\t\t- ", 1, NoteMode.Plain, Editor.ListEditing.ContinuationKind.Outdent, null, "two-tab indent outdents");
        ContinueCase("- item", 1, NoteMode.Code, Editor.ListEditing.ContinuationKind.Decline, null, "Code mode declines list continuation");
        ContinueCase("", 1, NoteMode.List, Editor.ListEditing.ContinuationKind.InsertAtCaret, "\n- ", "List mode adds a bullet on the first blank line");
        ContinueCase("", 2, NoteMode.List, Editor.ListEditing.ContinuationKind.Decline, null, "List mode declines a later blank line");
        ContinueCase("plain", 2, NoteMode.List, Editor.ListEditing.ContinuationKind.InsertAtCaret, "\n- ", "List mode adds a bullet after text");
        ContinueCase("999999999. item", 1, NoteMode.Plain, Editor.ListEditing.ContinuationKind.Decline, null, "maximum list number declines continuation");
        ContinueCase("1234567890. item", 1, NoteMode.Plain, Editor.ListEditing.ContinuationKind.Decline, null, "ten-digit list number declines continuation");

        RenumberCase(["  8. a", "9. b", "9. c", "9. d"], 1, fresh: true,
            [new(1, 1), new(3, 10), new(4, 11)], "fresh numbering resets the indented level and grows following root numbers");
        RenumberCase(["8. a", "  7. b", "  7. c", "9. d"], 2, fresh: true,
            [new(2, 1), new(3, 2)], "nested level starts fresh at one and increments within its indent");
        RenumberCase(["8. a", "- bullet", "8. c", "9. d"], 1, fresh: false,
            [], "bullet resets numbered sequence at its indent");
        RenumberCase(["999999999. a", "999999999. b", "4. c"], 1, fresh: false,
            [], "number overflow resets without changing existing markers");
        RenumberCase(["8. a", "12. b", "plain", "12. c"], 1, fresh: false,
            [new(2, 9)], "renumber stops at an unindented plain-line boundary");

        ToggleCase("[ ]", new(1, 1, "x"), "unchecked checkbox edit");
        ToggleCase("[x]", new(1, 1, " "), "lowercase checked checkbox edit");
        ToggleCase("[X]", new(1, 1, " "), "uppercase checked checkbox edit");
        ToggleCase("[ ]task", new(1, 1, "x"), "checkbox without following space");
        ToggleCase("- task", new(0, 2, "[ ] "), "bullet checkbox edit");
        ToggleCase("+  task", new(0, 3, "[ ] "), "bullet checkbox edit consumes matched spaces");
        ToggleCase("  - task", new(2, 2, "[ ] "), "indented bullet checkbox edit");
        ToggleCase("plain", new(0, 0, "[ ] "), "plain line checkbox insertion");
        ToggleCase("", new(0, 0, "[ ] "), "empty line checkbox insertion");
        ToggleCase("12. item", new(0, 0, "[ ] "), "numbered line checkbox insertion");

        IndentCase("- item", true, new(0, 0, "  "), "indent inserts two spaces");
        IndentCase("   - item", true, new(0, 0, "  "), "indent preserves existing spaces");
        IndentCase("\t- item", true, new(0, 0, "  "), "indent before tab-leading line");
        IndentCase("- item", false, null, "outdent with zero spaces is a no-op");
        IndentCase(" - item", false, new(0, 1, ""), "outdent removes one leading space");
        IndentCase("  - item", false, new(0, 2, ""), "outdent removes two leading spaces");
        IndentCase("   - item", false, new(0, 2, ""), "outdent removes at most two of three spaces");
        IndentCase("\t - item", false, null, "outdent ignores tab-leading whitespace");

        report = failures.Count == 0 ? $"{checks} checks" : string.Join("; ", failures);
        return failures.Count == 0;
    }

    private static bool ListCommandsChecks(out string report)
    {
        var failures = new List<string>();
        var checks = 0;
        void CheckCase(bool passed, string name) { checks++; if (!passed) failures.Add(name); }

        int CaretLine(AvaloniaEdit.Document.TextDocument document, AvaloniaEdit.Document.TextAnchor caret)
            => document.GetLineByOffset(caret.Offset).LineNumber;

        void DocumentCase(string initial, int lineNumber, int caretColumn,
            Action<AvaloniaEdit.Document.TextDocument, AvaloniaEdit.Document.TextAnchor> command,
            string expected, string name, int? expectedCaretLine = null, int? expectedCaretColumn = null)
        {
            var document = new AvaloniaEdit.Document.TextDocument(initial);
            var line = document.GetLineByNumber(lineNumber);
            var caret = document.CreateAnchor(line.Offset + caretColumn);
            caret.MovementType = AvaloniaEdit.Document.AnchorMovementType.AfterInsertion;
            var changes = 0;
            document.TextChanged += (_, _) => changes++;

            using (document.RunUpdate()) command(document, caret);
            CheckCase(document.Text == expected, name + " text");
            CheckCase(changes == (initial == expected ? 0 : 1), name + " one document update");
            if (expectedCaretLine is { } wantedLine)
                CheckCase(CaretLine(document, caret) == wantedLine, name + " post-edit caret line");
            if (expectedCaretColumn is { } wantedColumn)
            {
                var caretLine = document.GetLineByOffset(caret.Offset);
                CheckCase(caret.Offset - caretLine.Offset == wantedColumn, name + " post-edit caret column");
            }

            if (initial == expected) return;
            document.UndoStack.Undo();
            CheckCase(document.Text == initial, name + " one Undo restores text");
            CheckCase(!document.UndoStack.CanUndo, name + " no second Undo step");
        }

        void ContinuationCase(string initial, int lineNumber, int caretColumn, NoteMode mode,
            string expected, bool expectedContinued, string name, int? expectedCaretLine = null, int? expectedCaretColumn = null)
        {
            bool? continued = null;
            DocumentCase(initial, lineNumber, caretColumn, (document, caret) =>
            {
                continued = Editor.DocumentCommands.ContinueList(document, lineNumber, mode,
                    text => document.Insert(caret.Offset, text));
                if (continued.Value) Editor.DocumentCommands.Renumber(document, CaretLine(document, caret));
            }, expected, name, expectedCaretLine, expectedCaretColumn);
            CheckCase(continued == expectedContinued, name + " continuation result");
        }

        void IndentationCase(string initial, int lineNumber, int caretColumn, bool deeper, string expected, string name)
            => DocumentCase(initial, lineNumber, caretColumn, (document, caret) =>
            {
                Editor.DocumentCommands.Indent(document, lineNumber, deeper);
                Editor.DocumentCommands.Renumber(document, CaretLine(document, caret), fresh: deeper);
            }, expected, name);

        void RenumberDocumentCase(string initial, int lineNumber, bool fresh, string expected, string name)
            => DocumentCase(initial, lineNumber, 0,
                (document, _) => Editor.DocumentCommands.Renumber(document, lineNumber, fresh),
                expected, name);

        ContinuationCase("8. a\n9. b\n9. c\n9. d", 1, 4, NoteMode.Plain,
            "8. a\n9. \n10. b\n11. c\n12. d", true, "Enter continuation plus live-offset renumber", 2, 3);
        ContinuationCase("- ab", 1, 2, NoteMode.Plain,
            "- \n- ab", true, "mid-line Enter moves trailing text below the new marker", 2, 2);
        ContinuationCase("[x] ab", 1, 6, NoteMode.Plain,
            "[x] ab\n[ ] ", true, "checkbox Enter continuation at line end", 2, 4);
        ContinuationCase("8. root\n  2. \n9. next", 2, 5, NoteMode.Plain,
            "8. root\n9. \n10. next", true, "empty nested Enter outdents and runs both renumbers", 2, 3);
        ContinuationCase("    - ", 1, 6, NoteMode.Plain,
            "  - ", true, "empty deeply indented item removes exactly two spaces", 1, 4);
        ContinuationCase("\t- ", 1, 3, NoteMode.Plain,
            "", true, "empty one-tab item clears by character-count indent rule", 1, 0);
        ContinuationCase(" \t- ", 1, 4, NoteMode.Plain,
            "- ", true, "empty mixed-indent item removes exactly two characters", 1, 2);
        ContinuationCase("8. a\n9. ", 2, 3, NoteMode.Plain,
            "8. a\n", true, "empty root Enter exits list", 2, 0);
        ContinuationCase("999999999. item", 1, 15, NoteMode.Plain,
            "999999999. item", false, "maximum number declines document continuation");
        ContinuationCase("", 1, 0, NoteMode.List,
            "\n- ", true, "List mode first blank line creates its bullet", 2, 2);
        ContinuationCase("head\n\nlast", 2, 0, NoteMode.List,
            "head\n\nlast", false, "List mode declines a later blank line");

        CheckCase(Editor.PadView.ConvertMenu(false, _ => { }) is null, "no conversion menu without selection");
        CheckCase(Editor.PadView.ConvertMenu(true, _ => { })?.Children?.Count == 3, "selection has three conversion choices");

        foreach (var (kind, expected) in new[]
        {
            (Editor.ListEditing.ListKind.Checklist, "list\n[ ] a\n  [ ] b\n\n[ ] c"),
            (Editor.ListEditing.ListKind.Bullet, "list\n- a\n  - b\n\n- c"),
            (Editor.ListEditing.ListKind.Numbered, "list\n1. a\n  1. b\n\n1. c"),
        })
        {
            var initial = "list\n[x] a\n  - b\n\n3. c";
            var document = new AvaloniaEdit.Document.TextDocument(initial);
            Editor.DocumentCommands.ConvertToList(document, 1, 5, kind);
            CheckCase(document.Text == expected, $"convert {kind} prefixes, blanks, indent and keyword");
            document.UndoStack.Undo();
            CheckCase(document.Text == initial && !document.UndoStack.CanUndo, $"convert {kind} one undo");
        }
        var numbered = new AvaloniaEdit.Document.TextDocument("a\nb\n\nc\nd");
        Editor.DocumentCommands.ConvertToList(numbered, 1, 5, Editor.ListEditing.ListKind.Numbered);
        CheckCase(numbered.Text == "1. a\n2. b\n\n1. c\n2. d", "conversion numbering restarts after blank");

        var selected = new AvaloniaEdit.Document.TextDocument("1. a\nplain\n2. b");
        CheckCase(Editor.DocumentCommands.IndentListLines(selected, 1, 3, true)
            && selected.Text == "  1. a\nplain\n  1. b", "selected list lines indent while plain lines stay put");
        selected.UndoStack.Undo();
        CheckCase(selected.Text == "1. a\nplain\n2. b", "selected list indentation is one undo");
        CheckCase(!Editor.DocumentCommands.IndentListLines(selected, 2, 2, true), "plain selection stays unhandled");

        var groups = new AvaloniaEdit.Document.TextDocument("8. a\n- break\n7. b\n  plain\n6. c");
        Editor.DocumentCommands.IndentListLines(groups, 1, 5, true);
        CheckCase(groups.Text == "  1. a\n  - break\n  1. b\n  plain\n  2. c", "selected numbered groups restart within one block");
        var large = new AvaloniaEdit.Document.TextDocument(string.Join('\n', Enumerable.Repeat("- item", 2000)));
        var timer = System.Diagnostics.Stopwatch.StartNew();
        var handled = Editor.DocumentCommands.IndentListLines(large, 1, 2000, true);
        timer.Stop();
        CheckCase(handled && large.Text == string.Join('\n', Enumerable.Repeat("  - item", 2000)), "large selection indentation result");
        CheckCase(timer.Elapsed < TimeSpan.FromSeconds(2), "large selection indentation completes promptly");

        IndentationCase("8. a\n9. b\n9. c\n9. d", 1, 4, deeper: true,
            "  1. a\n9. b\n10. c\n11. d", "Tab indentation plus fresh renumber shares one Undo");
        IndentationCase("8. a\n  1. b\n9. c", 2, 7, deeper: false,
            "8. a\n9. b\n10. c", "Shift+Tab outdent plus renumber shares one Undo");
        RenumberDocumentCase("8. a\n12. b\nplain\n12. c", 1, fresh: false,
            "8. a\n9. b\nplain\n12. c", "document renumber respects block boundary");
        RenumberDocumentCase("8. root\n  5. child\n  5. child\n9. root\n  7. child\n10. root", 2, fresh: true,
            "8. root\n  1. child\n  2. child\n9. root\n  7. child\n10. root", "document renumber resets nested level and preserves sibling level");
        RenumberDocumentCase("8. a\n- bullet\n5. c\n5. d", 1, fresh: false,
            "8. a\n- bullet\n5. c\n6. d", "document bullet resets numbered sequence");
        RenumberDocumentCase("999999999. a\n999999999. b\n4. c", 1, fresh: false,
            "999999999. a\n999999999. b\n4. c", "document overflow reset leaves numbers unchanged");
        RenumberDocumentCase("head\r\n8. a\r\n12. b\r\n12. c", 2, fresh: false,
            "head\r\n8. a\r\n9. b\r\n10. c", "document renumber uses live offsets after a CRLF prefix");
        ContinuationCase("head\r\n8. a\r\n9. b\r\n9. c", 2, 4, NoteMode.Plain,
            "head\r\n8. a\n9. \r\n10. b\r\n11. c", true, "continuation preserves nonzero offset and mixed inserted LF", 3, 3);

        report = failures.Count == 0 ? $"{checks} checks" : string.Join("; ", failures);
        return failures.Count == 0;
    }

    private static bool CheckboxIndentChecks(out string report)
    {
        var failures = new List<string>();
        var checks = 0;
        void CheckCase(bool passed, string name) { checks++; if (!passed) failures.Add(name); }

        void DocumentCase(string initial, int lineNumber, Action<AvaloniaEdit.Document.TextDocument, int> command,
            string expected, string name)
        {
            var document = new AvaloniaEdit.Document.TextDocument(initial);
            var changes = 0;
            document.TextChanged += (_, _) => changes++;
            command(document, lineNumber);
            CheckCase(document.Text == expected, name + " text");
            CheckCase(changes == (initial == expected ? 0 : 1), name + " one document edit");
            if (initial == expected) return;

            document.UndoStack.Undo();
            CheckCase(document.Text == initial, name + " one Undo restores text");
            CheckCase(!document.UndoStack.CanUndo, name + " no second Undo step");
        }

        void RegexCase(string text, string indent, string state, int stateOffset, int matchLength, string name)
        {
            var match = Editor.ListEditing.Checkbox.Match(text);
            CheckCase(match.Success && match.Groups.Count == 3 && match.Length == matchLength
                && match.Groups[1].Value == indent
                && match.Groups[2].Value == state && match.Groups[2].Index == stateOffset,
                name + " regex groups");
        }

        DocumentCase("lead\n[ ]task", 2, Editor.DocumentCommands.ToggleCheckbox, "lead\n[x]task", "toggle at nonzero LF offset");
        DocumentCase("[x] task", 1, Editor.DocumentCommands.ToggleCheckbox, "[ ] task", "toggle lowercase checked box");
        DocumentCase("lead\r\n[X] task", 2, Editor.DocumentCommands.ToggleCheckbox, "lead\r\n[ ] task", "toggle at nonzero CRLF offset");
        DocumentCase("head\n- [ ] task", 2, Editor.DocumentCommands.ToggleCheckbox, "head\n- [x] task", "toggle checkbox after bullet");
        DocumentCase("head\n-  task", 2, Editor.DocumentCommands.ToggleCheckbox, "head\n[ ] task", "toggle replaces bullet and spacing");
        DocumentCase("", 1, Editor.DocumentCommands.ToggleCheckbox, "[ ] ", "toggle checkbox on empty line");
        DocumentCase("12. item", 1, Editor.DocumentCommands.ToggleCheckbox, "[ ] 12. item", "toggle checkbox on numbered line");
        DocumentCase("* [ ] task", 1, Editor.DocumentCommands.ToggleCheckbox, "* [x] task", "toggle checkbox after star bullet");
        DocumentCase("*   task", 1, Editor.DocumentCommands.ToggleCheckbox, "[ ] task", "toggle checkbox replaces multi-space star bullet");
        DocumentCase("\t*  task", 1, Editor.DocumentCommands.ToggleCheckbox, "\t[ ] task", "toggle checkbox after tab-indented bullet");
        DocumentCase("head\r\n  plain", 2, Editor.DocumentCommands.ToggleCheckbox, "head\r\n  [ ] plain", "toggle inserts after line indent");
        DocumentCase("head\n- item\nnext", 2, (doc, line) => Editor.DocumentCommands.Indent(doc, line, true), "head\n  - item\nnext", "indent at nonzero LF offset");
        DocumentCase(" - item", 1, (doc, line) => Editor.DocumentCommands.Indent(doc, line, true), "   - item", "indent preserves one existing space");
        DocumentCase("  - item", 1, (doc, line) => Editor.DocumentCommands.Indent(doc, line, true), "    - item", "indent preserves two existing spaces");
        DocumentCase("head\r\n - item", 2, (doc, line) => Editor.DocumentCommands.Indent(doc, line, false), "head\r\n- item", "outdent at nonzero CRLF offset");
        DocumentCase("  - item", 1, (doc, line) => Editor.DocumentCommands.Indent(doc, line, false), "- item", "outdent removes two spaces");
        DocumentCase("head\n   - item", 2, (doc, line) => Editor.DocumentCommands.Indent(doc, line, false), "head\n - item", "outdent removes at most two spaces");
        DocumentCase("head\n- item", 2, (doc, line) => Editor.DocumentCommands.Indent(doc, line, false), "head\n- item", "outdent without spaces is a no-op");
        DocumentCase("\t- item", 1, (doc, line) => Editor.DocumentCommands.Indent(doc, line, false), "\t- item", "outdent preserves tab-leading line");
        DocumentCase(" \t- item", 1, (doc, line) => Editor.DocumentCommands.Indent(doc, line, false), "\t- item", "outdent removes space before tab");
        DocumentCase("head\n[ ] task", 0, Editor.DocumentCommands.ToggleCheckbox, "head\n[ ] task", "invalid checkbox line is a no-op");
        DocumentCase("head\n[ ] task", 3, Editor.DocumentCommands.ToggleCheckbox, "head\n[ ] task", "checkbox line past document is a no-op");

        RegexCase("  [x] task", "  ", "x", 3, 5, "indented checkbox");
        RegexCase("- [ ] task", "", " ", 3, 5, "bullet checkbox");
        RegexCase("+ [X]", "", "X", 3, 5, "uppercase bullet checkbox");

        report = failures.Count == 0 ? $"{checks} checks" : string.Join("; ", failures);
        return failures.Count == 0;
    }
}
