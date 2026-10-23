using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls.Chrome;
using Avalonia.Automation;

namespace AntiQuickPad.Views.Common;

internal sealed class ShellLayoutContext(ResidentShellWindow owner, ShellPalette palette, Control page)
{
    public Look Look => palette.Look;
    public Tokens.DesignTokens T => palette.Tokens;
    public Control Page => page;
    public IBrush Ink => new SolidColorBrush(Look.Foreground);
    public IBrush Muted => new SolidColorBrush(Look.Muted);
    public IBrush Surface => new SolidColorBrush(palette.Colors.Surface);
    public Color PaletteCanvas => palette.Canvas;
    public IBrush Canvas => new SolidColorBrush(palette.Canvas);
    public IBrush Rule => new SolidColorBrush(palette.Rule);
    public TextBlock Count { get; } = new OpticalText { Height = palette.Tokens.Button.Height, OpticalOffset = .25 };
    public TextBlock Save { get; } = new OpticalText { Height = palette.Tokens.Button.Height, OpticalOffset = .25 };
    public StackPanel Dots { get; } = new() { Orientation = Orientation.Horizontal, Spacing = palette.Tokens.DotSpacing, VerticalAlignment = VerticalAlignment.Center };
    public Control Hint { get; } = new TextBlock { IsVisible = false, IsHitTestVisible = false,
        Text = "A fresh thought.", FontFamily = new FontFamily(palette.Look.EditorFont), FontSize = palette.Look.FontSize,
        Foreground = new SolidColorBrush(palette.Look.Muted), VerticalAlignment = VerticalAlignment.Top };
    public bool HasPrevious => owner.ViewModel.HasPrevious;
    public bool HasNext => owner.ViewModel.HasNext;
    public List<Control> Bars { get; } = [];
    // The single tint layer over the backdrop (preferences change it in place).
    public Border? Tint { get; set; }
    public Control? PreviousPeek { get; set; }
    public Control? NextPeek { get; set; }
    public Dictionary<string, Button> Buttons { get; } = [];
    public Dictionary<string, string> CommandIds { get; } = [];
    public TextBlock Text(string value, double size, IBrush? brush = null, FontWeight? weight = null) => new()
    { Text = value, FontSize = size, Foreground = brush ?? Ink, FontWeight = weight ?? FontWeight.Normal, VerticalAlignment = VerticalAlignment.Center };
    public Button Button(string glyph, string label, string commandId)
    {
        var b = new Button { Content = new OpticalText { Height = T.Button.Height, OpticalOffset = .25, Text = glyph, FontFamily = new FontFamily(T.IconFont), FontSize = T.IconSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
            Width = T.Button.Width, Height = T.Button.Height, Padding = new Thickness(0), CornerRadius = new CornerRadius(T.ButtonRadius), BorderThickness = new Thickness(0),
            Background = Brushes.Transparent, Foreground = Ink, VerticalAlignment = VerticalAlignment.Center };
        FocusRing(b, Look, T);
        ToolTip.SetTip(b, label); AutomationProperties.SetName(b, label);
        b.Command = owner.Command(commandId);
        Buttons[label] = b; CommandIds[label] = commandId;
        return b;
    }
    // F51: a view-owned accent ring for keyboard (and programmatic) focus, independent of Fluent defaults; pointer focus shows none.
    public static void FocusRing(Button b, Look look, Tokens.DesignTokens t)
    {
        b.GotFocus += (_, e) =>
        {
            if (e.NavigationMethod == NavigationMethod.Pointer) return;
            b.BorderThickness = new Thickness(t.FocusRing); b.BorderBrush = new SolidColorBrush(look.Accent);
        };
        b.LostFocus += (_, _) => { b.BorderThickness = new Thickness(0); b.BorderBrush = Brushes.Transparent; };
    }
    public StackPanel Row(params Control[] controls)
    { var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = T.ActionSpacing, VerticalAlignment = VerticalAlignment.Center }; foreach (var c in controls) row.Children.Add(c); return row; }
    public Control Bar(Control control)
    {
        Bars.Add(control);
        return control;
    }
    public Control DragRegion(Control control)
    {
        // Native non-client hit testing preserves Windows drag, double-click maximize and snap docking.
        WindowDecorationProperties.SetElementRole(control, WindowDecorationsElementRole.TitleBar);
        return control;
    }
    public void Place(Grid root, Control control, HorizontalAlignment horizontal, VerticalAlignment vertical, Thickness margin)
    { control.HorizontalAlignment = horizontal; control.VerticalAlignment = vertical; control.Margin = margin; root.Children.Add(control); }
}



