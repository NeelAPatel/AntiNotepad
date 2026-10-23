using Avalonia;
using Avalonia.Controls;

namespace AntiQuickPad.Shell;

/// <summary>Window adapter for shells that need visibility, pin, and saved geometry operations.</summary>
public sealed class AvaloniaWindowPlatform : IWindowPlatform
{
    private readonly Window _window;

    public AvaloniaWindowPlatform(Window window) => _window = window ?? throw new ArgumentNullException(nameof(window));

    public bool IsVisible => _window.IsVisible;

    public bool Pinned
    {
        get => _window.Topmost;
        set => _window.Topmost = value;
    }

    public WindowBounds Bounds
    {
        get => new(_window.Width, _window.Height, _window.Position.X, _window.Position.Y);
        set
        {
            _window.Width = value.Width;
            _window.Height = value.Height;
            _window.Position = new PixelPoint(value.X, value.Y);
        }
    }

    public void Show()
    {
        _window.WindowState = WindowState.Normal;
        _window.Show();
    }

    public void Hide() => _window.Hide();

    public void Activate() => _window.Activate();
}
