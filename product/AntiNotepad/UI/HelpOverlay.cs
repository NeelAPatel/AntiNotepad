using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AntiNotepad;
using AntiNotepad.Theme;

namespace AntiNotepad.UI;

/// <summary>F1 cheat sheet. Everything here works without the mouse.</summary>
internal sealed class HelpOverlay : ScrollViewer
{
    private static readonly (string Section, (string Keys, string What)[] Rows)[] Sheet =
    [
        ("Anywhere", [("Ctrl+Alt+Shift+S", $"Show / hide {AppInfo.Name} (change in Settings)"), ("Esc", "Hide (or close a panel)")]),
        ("Notes", [("Ctrl+N", "New note"), ("Ctrl+]  ·  Alt+→  ·  swipe", "Next note; past the last one starts a new note"), ("Ctrl+[  ·  Alt+←", "Previous note (in a split pane: the previous slot)"),
            ("Ctrl+Shift+Backspace", "Delete note (to trash, undoable)"), ("Alt+Shift+↑ / ↓", "Move note earlier / later"), ("Ctrl+F", "Search · Ctrl+G grid · Ctrl+T trash")]),
        ("Slots", [("Ctrl+1 … 9", "Open a permanent slot (again to go back)"), ("Ctrl+Shift+1 … 9", "Keep this note in a slot"), ("Ctrl+0", "Back to the stack"), ("Ctrl+\\", "Split: a slot beside your note")]),
        ("Writing", [("Ctrl+Enter", "Toggle a checkbox (or start a timer)"), ("Tab / Shift+Tab", "Indent list items"), ("/", "Commands: /date /sort /json /ocr …"),
            ("Ctrl+Shift+C", "Copy the whole note"), ("Ctrl+Shift+O", "Read text from a screen snip"), ("Ctrl+ + / −", "Text size"), ("Ctrl+T", "Keep on top")]),
        ("Line 1 modes", [("math", "Live answers, variables, units, %, sum"), ("list", "Every line is a checkbox"), ("timer", "25m focus · Enter starts"), ("code py", "Syntax colors"), ("clipboard", "Collects what you copy")]),
    ];

    public HelpOverlay(PadWindow window, Palette p)
    {
        Margin = new Thickness(10, 14, 10, 40);
        HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled;
        var panel = new StackPanel { Margin = new Thickness(8, 0, 14, 12) };
        panel.Children.Add(Kit.PanelHeader("Shortcuts", p, window.CloseOverlay));
        foreach (var (section, rows) in Sheet)
        {
            panel.Children.Add(Kit.Section(p, section));
            foreach (var (keys, what) in rows)
            {
                var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*"), Margin = new Thickness(8, 3) };
                var key = new Border
                {
                    Background = new SolidColorBrush(p.Ink, p.Dark ? .1 : .06), CornerRadius = new CornerRadius(5), Padding = new Thickness(7, 2), MinWidth = 150,
                    Child = new TextBlock { Text = keys, FontFamily = new FontFamily("Cascadia Mono, Consolas"), FontSize = 11.5, Foreground = new SolidColorBrush(p.Ink) }
                };
                var text = new TextBlock { Text = what, Foreground = new SolidColorBrush(p.Muted), FontSize = 12.5, Margin = new Thickness(12, 0, 0, 0), TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
                grid.Children.Add(key);
                Grid.SetColumn(text, 1); grid.Children.Add(text);
                panel.Children.Add(grid);
            }
        }
        Content = panel;
        Focusable = true;
        Avalonia.Threading.Dispatcher.UIThread.Post(() => Focus());
    }
}
