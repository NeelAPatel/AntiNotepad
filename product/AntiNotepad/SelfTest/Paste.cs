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
        var captureCPlain = "Both are told, and both messages were delivered, though neither is confirmed read.\n\nMaster 6 got the facts and three asks:\nResolve the A23 card from the branch version at merge.\nAdd only an item’s own files on later merges.\nWatch for the untracked A21 lab pages, which could be swept in the same way.\nPStack Manager was asked to plan a permanent fix. I suggested these as starting points:\nA merge preflight that fails when a dev commit touches the cards of an item that has no branch.\nA staged-files allowlist at merge.\napprove.ps1 accepting a card that is already tracked.\nA home for the untracked lab pages.";
        var captureCHtml = "<p dir=\"ltr\">Both are told, and both messages were delivered, though neither is confirmed read.</p><ul dir=\"ltr\"><li><strong>Master 6</strong> got the facts and three asks:<ul dir=\"ltr\"><li>Resolve the A23 card from the branch version at merge.</li><li>Add only an item’s own files on later merges.</li><li>Watch for the untracked A21 lab pages, which could be swept in the same way.</li></ul></li><li><strong>PStack Manager</strong> was asked to plan a permanent fix. I suggested these as starting points:<ul dir=\"ltr\"><li>A merge preflight that fails when a dev commit touches the cards of an item that has no branch.</li><li>A staged-files allowlist at merge.</li><li><code dir=\"ltr\">approve.ps1</code> accepting a card that is already tracked.</li><li>A home for the untracked lab pages.</li></ul></li></ul>";
        var captureCExpected = "Both are told, and both messages were delivered, though neither is confirmed read.\n\n- Master 6 got the facts and three asks:\n  - Resolve the A23 card from the branch version at merge.\n  - Add only an item’s own files on later merges.\n  - Watch for the untracked A21 lab pages, which could be swept in the same way.\n- PStack Manager was asked to plan a permanent fix. I suggested these as starting points:\n  - A merge preflight that fails when a dev commit touches the cards of an item that has no branch.\n  - A staged-files allowlist at merge.\n  - approve.ps1 accepting a card that is already tracked.\n  - A home for the untracked lab pages.";
        Check(PasteEngine.Upgrade(Snapshot(captureCPlain, captureCHtml)) == captureCExpected, "captured paragraph and list");
        Check(PasteEngine.Upgrade(Snapshot("A\nB", "<ul><li>A</li></ul><p>B</p>")) == "- A\n\nB", "paragraph after list");
        Check(PasteEngine.Upgrade(Snapshot("A\nB\nC", "<p>A</p><p>B</p><ul><li>C</li></ul>")) == "A\n\nB\n\n- C", "paragraphs before list");
        Check(PasteEngine.Upgrade(Snapshot("Before\nItem\nAfter", "<div>Before<ul><li>Item</li></ul>After</div>")) == "Before\n\n- Item\n\nAfter", "list inside div");
        Check(PasteEngine.Upgrade(Snapshot("Title\nA", "<h2>Title</h2><ul><li>A</li></ul>")) == "Title\n\n- A", "heading and list");
        Check(PasteEngine.Upgrade(Snapshot("A\nB\nC", "<p>A<br>B</p><ul><li>C</li></ul>")) == "A\nB\n\n- C", "paragraph line break");
        Check(PasteEngine.Upgrade(Snapshot("A\nB", "<p>A</p><ul><li>B</li></ul><p>")) is null, "unclosed block");
        Check(PasteEngine.Upgrade(Snapshot("A\nB", "<p>A</p><ul><li>C</li></ul>")) is null, "word mismatch");
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
