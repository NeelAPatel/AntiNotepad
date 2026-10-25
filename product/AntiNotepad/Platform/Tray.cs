using Avalonia;
using Avalonia.Controls;
using AntiNotepad;

namespace AntiNotepad.Platform;

/// <summary>Tray icon: click shows the pad; menu has the few actions that make sense from outside.</summary>
internal sealed class Tray : IDisposable
{
    private readonly TrayIcon _tray;

    public Tray(Action show, Action newNote, Action settings, Action quit, Func<IReadOnlyList<(string Title, bool Occupied)>>? slotTitles = null, Action<int>? openSlot = null)
    {
        var menu = new NativeMenu();
        void Add(string label, Action action) { var item = new NativeMenuItem(label); item.Click += (_, _) => action(); menu.Items.Add(item); }
        Add($"Show {AppInfo.DisplayName}", show);
        Add("New note", newNote);
        if (slotTitles is not null && openSlot is not null)
        {
            // Filled slots, refreshed each time the menu opens.
            var slots = new NativeMenu();
            var slotsItem = new NativeMenuItem("Slots") { Menu = slots };
            menu.Items.Add(slotsItem);
            void Fill()
            {
                slots.Items.Clear();
                var titles = slotTitles();
                for (var i = 0; i < titles.Count; i++)
                {
                    var n = i + 1;
                    var slot = titles[i];
                    var item = new NativeMenuItem($"{n}  {(slot.Occupied ? slot.Title : "empty")}") { IsEnabled = slot.Occupied };
                    item.Click += (_, _) => openSlot(n);
                    slots.Items.Add(item);
                }
            }
            Fill();
            menu.NeedsUpdate += (_, _) => Fill();
        }
        Add("Settings", settings);
        menu.Items.Add(new NativeMenuItemSeparator());
        Add($"Quit {AppInfo.DisplayName}", quit);
        _tray = new TrayIcon { ToolTipText = AppInfo.DisplayName, Icon = CreateIcon(), Menu = menu, IsVisible = true };
        _tray.Clicked += (_, _) => show();
        TrayIcon.SetIcons(Application.Current!, new TrayIcons { _tray });
    }

    public string Tooltip { set => _tray.ToolTipText = value; }

    public void Dispose() { _tray.IsVisible = false; _tray.Dispose(); }

    // Same multi-size icon as the exe (tools/make-icon.ps1), so Windows picks the sharp 16/32 px frame.
    private static WindowIcon CreateIcon() => new(Avalonia.Platform.AssetLoader.Open(new Uri(AppInfo.IconUri)));
}
