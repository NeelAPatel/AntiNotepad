using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AntiQuickPad.Shell;

/// <summary>Current Avalonia tray implementation, kept behind the shell service contract.</summary>
public sealed class AvaloniaTrayService : ITrayService
{
    private TrayIcon? _tray;
    private bool _initialized;

    public void Initialize(Action toggle, Action quit, Action show, string hotkeyStatus)
    {
        ArgumentNullException.ThrowIfNull(toggle);
        ArgumentNullException.ThrowIfNull(quit);
        ArgumentNullException.ThrowIfNull(show);
        ArgumentNullException.ThrowIfNull(hotkeyStatus);
        if (_initialized) throw new InvalidOperationException("The tray service has already been initialized.");
        _initialized = true;

        var menu = new NativeMenu();
        var toggleItem = new NativeMenuItem("Show / Hide");
        toggleItem.Click += (_, _) => toggle();
        menu.Items.Add(toggleItem);
        var quitItem = new NativeMenuItem("Quit");
        quitItem.Click += (_, _) => quit();
        menu.Items.Add(quitItem);
        _tray = new TrayIcon
        {
            ToolTipText = $"AntiQuickPad · {hotkeyStatus}",
            Icon = CreateTrayIcon(),
            Menu = menu,
            IsVisible = true
        };
        _tray.Clicked += (_, _) => show();
        TrayIcon.SetIcons(Application.Current!, new TrayIcons { _tray });
    }

    public void SetHotkeyStatus(string hotkeyStatus) { if (_tray is not null) _tray.ToolTipText = $"AntiQuickPad · {hotkeyStatus}"; }

    public void Dispose()
    {
        if (_tray is null) return;
        _tray.IsVisible = false;
        _tray.Dispose();
        _tray = null;
    }

    private static WindowIcon CreateTrayIcon()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write((ushort)0); writer.Write((ushort)1); writer.Write((ushort)1);
        writer.Write((byte)16); writer.Write((byte)16); writer.Write((byte)0); writer.Write((byte)0);
        writer.Write((ushort)1); writer.Write((ushort)32); writer.Write(40 + 16 * 16 * 4 + 64); writer.Write(22);
        writer.Write(40); writer.Write(16); writer.Write(32); writer.Write((ushort)1); writer.Write((ushort)32);
        writer.Write(0); writer.Write(16 * 16 * 4); writer.Write(0); writer.Write(0); writer.Write(0); writer.Write(0);
        var color = Look.Default.Accent;
        for (var i = 0; i < 256; i++)
        {
            writer.Write(color.B); writer.Write(color.G); writer.Write(color.R); writer.Write((byte)255);
        }
        writer.Write(new byte[64]);
        stream.Position = 0;
        return new WindowIcon(stream);
    }
}
