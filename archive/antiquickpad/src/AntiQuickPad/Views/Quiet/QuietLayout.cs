using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AntiQuickPad.Views.Common;

namespace AntiQuickPad.Views.Quiet;

internal static class QuietLayout
{
    public static Control Build(ShellLayoutContext c)
    {
        var root = new Grid { Background = Brushes.Transparent, ClipToBounds = true };
        c.Tint = new Border { Background = new SolidColorBrush(c.PaletteCanvas, c.Look.TintAlpha), IsHitTestVisible = false };
        root.Children.Add(c.Tint);
        var t = c.T;
        // The mock has a 24 DIP editor inset plus the existing 24 DIP folding gutter.
        c.Place(root, c.Page, HorizontalAlignment.Stretch, VerticalAlignment.Stretch,
            new Thickness(t.Inset, t.TopBar, t.Inset, t.PageBottom));
        c.Hint.Margin = new Thickness(t.Inset * 2, t.TopBar + c.Look.GrabHeight, t.Inset, 0);
        c.Hint.HorizontalAlignment = HorizontalAlignment.Left;
        c.Hint.VerticalAlignment = VerticalAlignment.Top;
        root.Children.Add(c.Hint);

        // Always present, including when presentation bars are fully hidden.
        c.Place(root, c.DragRegion(new Border { Height = t.TopBar, Background = Brushes.Transparent }),
            HorizontalAlignment.Stretch, VerticalAlignment.Top, new Thickness(0));
        var actions = c.Row(c.Button("\uE718", "Pin", "window.pin"),
            c.Button("\uE710", "New note", "note.create"),
            c.Button("\uE712", "Settings", "settings.toggle"),
            c.Button("\uE8BB", "Hide", "app.hide"));
        var top = new Grid { Height = t.TopBar, Children = { actions } };
        actions.HorizontalAlignment = HorizontalAlignment.Right;
        c.Place(root, c.Bar(top), HorizontalAlignment.Right, VerticalAlignment.Top, new Thickness(0, 0, t.Inset, 0));

        var navigation = c.Row(c.Button("\uE76B", "Previous", "note.previous"), c.Count, c.Dots,
            c.Button("\uE76C", "Next", "note.next"));
        c.Save.TextTrimming = TextTrimming.CharacterEllipsis;
        c.Save.Margin = new Thickness(t.SaveLineSpacing, 0);
        c.Save.Bind(ToolTip.TipProperty, c.Save.GetObservable(TextBlock.TextProperty));
        var esc = new OpticalText { Height = t.Button.Height, OpticalOffset = .25, Text = "Esc to hide", FontSize = t.HintSize,
            Foreground = c.Muted, VerticalAlignment = VerticalAlignment.Center };
        var footer = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"), Height = t.BottomBar };
        footer.Children.Add(navigation); Grid.SetColumn(c.Save, 1); footer.Children.Add(c.Save);
        Grid.SetColumn(esc, 2); footer.Children.Add(esc);
        var pill = new Border { Height = t.BottomBar, CornerRadius = new CornerRadius(t.PillRadius), Child = footer };
        c.Place(root, c.Bar(pill), HorizontalAlignment.Stretch, VerticalAlignment.Bottom, new Thickness(t.Inset, 0, t.Inset, t.BottomGap));
        return root;
    }
}
