// Checks clipboard list conversion and its plain-text safeguards.
using AntiNotepad.Editor;

namespace AntiNotepad;

internal static partial class SelfTest
{
    private static bool PasteChecks(out string report)
    {
        var failures = new List<string>();
        void Check(bool condition, string name) { if (!condition) failures.Add(name); }
        var plain = "test from ai chat\ntest 2 from ai chat\ntest 3\ntest2 again\ntest from ai chat\ntest 2\ntest3\ntest 2";
        var html = "<ul data-pm-slice=\"3 1 []\"><li><p>test from ai chat</p><ul><li><p>test 2 from ai chat</p><ul><li><p>test 3</p></li></ul></li><li><p>test2 again</p></li></ul></li></ul><ol><li><p>test from ai chat</p><ol><li><p>test 2</p><ol><li><p>test3</p></li></ol></li><li><p>test 2</p></li></ol></li></ol><p></p>";
        var expected = "- test from ai chat\n  - test 2 from ai chat\n    - test 3\n  - test2 again\n1. test from ai chat\n  1. test 2\n    1. test3\n  2. test 2";
        ClipSnapshot Snapshot(string? text, string? markup) => new(text, markup, [], false, []);
        Check(PasteEngine.Upgrade(Snapshot(plain, html)) == expected, "captured list");
        Check(PasteEngine.Upgrade(Snapshot("- Alpha\n- Beta\n  - Nested item\n- Gamma\n\n1. First\n2. Second\n   1. Nested step", "<p>- Alpha</p>")) is null, "markdown source");
        Check(PasteEngine.Upgrade(Snapshot("plain", "<p>plain</p>")) is null, "plain text");
        Check(PasteEngine.Upgrade(Snapshot("A & B\nC", "<ul><li>A &amp; B<ol><li>C</li></ol></li></ul>")) == "- A & B\n  1. C", "mixed and entities");
        Check(PasteEngine.Upgrade(Snapshot("A", "<ul><li></li><li>A</li></ul>")) == "- \n- A", "empty item");
        var fragment = "<ul><li>é</li></ul>";
        var header = "Version:1.0\r\nStartFragment:0000000000\r\nEndFragment:0000000000\r\n";
        var prefix = header + "<html>";
        var start = System.Text.Encoding.UTF8.GetByteCount(prefix);
        var end = start + System.Text.Encoding.UTF8.GetByteCount(fragment);
        header = $"Version:1.0\r\nStartFragment:{start:D10}\r\nEndFragment:{end:D10}\r\n";
        Check(PasteEngine.Upgrade(Snapshot("é", header + "<html>" + fragment + "</html>")) == "- é", "byte offsets");
        Check(PasteEngine.Upgrade(Snapshot("  A", "<ul><li>A</li></ul>")) is null, "indent guard");
        Check(PasteEngine.Upgrade(Snapshot("A", "<div style=\"white-space: pre-wrap\"><ul><li>A</li></ul></div>")) is null, "pre style guard");
        Check(PasteEngine.Upgrade(Snapshot("A", "<pre><ul><li>A</li></ul></pre>")) is null, "preformatted");
        Check(PasteEngine.Upgrade(Snapshot("A", "<ul><li>A")) is null, "bad html");
        Check(PasteEngine.Upgrade(new(null, null, [], true, [])) is null, "image placeholder");
        Check(PasteEngine.Upgrade(new(null, null, [], false, ["file"])) is null, "files placeholder");
        report = string.Join(", ", failures);
        return failures.Count == 0;
    }
}
