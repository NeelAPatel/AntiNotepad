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
    private static readonly (string Section, (string Keys, string What)[] Rows)[] Modes =
    [
        ("Line 1 modes", [("math", "Live answers, variables, units, %, sum"), ("list", ""), ("timer", "25m focus · Enter starts"), ("code py", "Syntax colors"), ("clipboard", "Collects what you copy")]),
    ];

    internal static string ListDescription(string value) => value switch
    {
        "check" => "Every line gets a checkbox",
        "number" => "Every line is numbered",
        _ => "Every line gets a bullet",
    };

    public HelpOverlay(PadWindow window, Palette p)
    {
        Margin = new Thickness(10, 14, 10, 40);
        HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled;
        var panel = new StackPanel { Margin = new Thickness(8, 0, 14, 12) };
        panel.Children.Add(Kit.PanelHeader("Shortcuts", p, window.CloseOverlay));
        foreach (var group in ShortcutMap.HelpRows(window.Shortcuts).GroupBy(row => row.Section))
        {
            panel.Children.Add(Kit.Section(p, group.Key));
            foreach (var row in group)
            {
                var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*"), Margin = new Thickness(ChromeTokens.MetaSpacing, ChromeTokens.BadgeTopPadding + ChromeTokens.BadgeBottomPadding) };
                var key = new Border
                {
                    Background = new SolidColorBrush(p.Ink, Paint.HelpWash(p.Dark)),
                    CornerRadius = new CornerRadius(Shape.Key),
                    Padding = new Thickness(ChromeTokens.BadgeHorizontalPadding, ChromeTokens.BadgeBottomPadding),
                    MinWidth = 150,
                    Child = new TextBlock
                    {
                        Text = row.Keys,
                        FontFamily = new FontFamily(Typography.MonoFamily),
                        FontSize = Typography.Caption,
                        Foreground = new SolidColorBrush(p.Ink)
                    }
                };
                var text = new TextBlock { Text = row.Help, Foreground = new SolidColorBrush(p.Muted), FontSize = Typography.Label, Margin = new Thickness(ChromeTokens.FirstItemInset, 0, 0, 0), TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
                grid.Children.Add(key);
                Grid.SetColumn(text, 1); grid.Children.Add(text);
                panel.Children.Add(grid);
            }
        }
        foreach (var (section, rows) in Modes)
        {
            panel.Children.Add(Kit.Section(p, section));
            foreach (var (keys, what) in rows)
            {
                var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*"), Margin = new Thickness(ChromeTokens.MetaSpacing, ChromeTokens.BadgeTopPadding + ChromeTokens.BadgeBottomPadding) };
                var key = new Border
                {
                    Background = new SolidColorBrush(p.Ink, Paint.HelpWash(p.Dark)),
                    CornerRadius = new CornerRadius(Shape.Key),
                    Padding = new Thickness(ChromeTokens.BadgeHorizontalPadding, ChromeTokens.BadgeBottomPadding),
                    MinWidth = 150,
                    Child = new TextBlock
                    {
                        Text = keys,
                        FontFamily = new FontFamily(Typography.MonoFamily),
                        FontSize = Typography.Caption,
                        Foreground = new SolidColorBrush(p.Ink)
                    }
                };
                var text = new TextBlock { Text = keys == "list" ? ListDescription(window.Settings.ListDefault) : what, Foreground = new SolidColorBrush(p.Muted), FontSize = Typography.Label, Margin = new Thickness(ChromeTokens.FirstItemInset, 0, 0, 0), TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
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
