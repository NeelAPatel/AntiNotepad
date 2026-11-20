// Capability checks for bottom-bar decisions and settings compatibility.
using System.Text.Json;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using AntiNotepad.Model;
using AntiNotepad.Theme;
using AntiNotepad.UI;

namespace AntiNotepad;

internal static partial class SelfTest
{
    /// <summary>Checks bar decisions, registered item order, actions, and basic settings compatibility.</summary>
    private static bool BarChecks(out string report)
    {
        var stack = PadBar.DecideBar(BarContextForTest(0, true, true, false, 2, false));
        var slot = PadBar.DecideBar(BarContextForTest(3, true, true, true, 2, false));
        var blank = PadBar.DecideBar(BarContextForTest(0, true, true, true, 1, true));
        var hidden = PadBar.DecideBar(BarContextForTest(0, false, false, false, 2, false));
        var settings = JsonSerializer.Deserialize<Settings>("{}")!;
        settings.ShowDeleteButton = false;
        settings.SlotDots = "quiet";
        var round = JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(settings))!;
        var iconButton = Kit.IconButton(Glyph.Delete, "Delete", Palette.All[0], () => { }, hoverColor: Palette.All[0].Error);
        var face = (Avalonia.Controls.Border)iconButton.Content!;
        var enabledOpacity = face.Opacity * ((GlyphIcon)face.Child!).Opacity;
        iconButton.IsEnabled = false;
        var disabledOpacity = face.Opacity * ((GlyphIcon)face.Child!).Opacity;
        iconButton.IsEnabled = true;
        var restoredOpacity = face.Opacity * ((GlyphIcon)face.Child!).Opacity;
        var clicks = 0;
        var actions = new BarActions(() => clicks++, () => { }, () => { }, () => { }, () => { }, _ => { });
        var titleCalls = 0;
        var slots = Enumerable.Range(1, 9).Select(i => new BarSlotContext(i == 9, () => { titleCalls++; return $"Title {i}"; })).ToArray();
        var stackContext = BarContextForTest(0, true, true, false, 2, false) with { Slots = slots };
        var lazyBar = new PadBar(BarPane.Left, stackContext, actions);
        lazyBar.Rebuild(stackContext, actions);
        var stackTitlesLazy = titleCalls == 0;
        lazyBar.Update(stackContext with { ViewKind = BarViewKind.Slot, ViewSlot = 1 }, actions);
        var dots = ((StackPanel)lazyBar.Pager.Child!).Children;
        var slotTitles = titleCalls == 8 && Enumerable.Range(1, 9).All(i =>
            Equals(ToolTip.GetTip(dots[i - 1]), $"Slot {i} · {(i == 9 ? "empty" : $"Title {i}")}"));
        var first = new PadBar(BarPane.Left, BarContextForTest(0, true, true, false, 2, false), actions);
        var firstHost = new Grid();
        firstHost.Children.Add(first.Root);
        var second = new PadBar(BarPane.Right, BarContextForTest(0, true, true, false, 2, false), actions);
        var secondHost = new Grid();
        secondHost.Children.Add(second.Root);
        var before = new Border();
        var slotOnly = new Border();
        first.RegisterItem(new("test-before", BarItemContainer.Left, -1, before));
        first.RegisterItem(new("test-slot-only", BarItemContainer.Left, 2, slotOnly,
            static context => context.ViewKind == BarViewKind.Slot,
            static context => !context.IsBlank));
        var leftRow = (StackPanel)first.Containers[0].Child!;
        var orderedIds = first.Items.Where(item => item.Container == BarItemContainer.Left).Select(item => item.Id)
            .SequenceEqual(["test-before", "delete", "test-slot-only"]);
        var orderedControls = ReferenceEquals(leftRow.Children[0], before)
            && ReferenceEquals(leftRow.Children[1], first.Items.Single(item => item.Id == "delete").Control)
            && ReferenceEquals(leftRow.Children[2], slotOnly);
        var deleteButton = (Button)first.Items.Single(item => item.Id == "delete").Control;
        deleteButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        var initiallyHidden = !slotOnly.IsVisible;
        first.Update(BarContextForTest(2, true, true, false, 2, false), actions);
        var shownForSlot = slotOnly.IsVisible;
        var twoBarsParented = ReferenceEquals(first.Root.Parent, firstHost)
            && ReferenceEquals(second.Root.Parent, secondHost)
            && first.Pane == BarPane.Left && second.Pane == BarPane.Right;
        var ok = enabledOpacity == 1 && disabledOpacity == Paint.DisabledIcon && restoredOpacity == enabledOpacity
            && !stack.Slots && stack.ShowNew && stack.NewEnabled && stack.DeleteEnabled
            && slot.Slots && !slot.ShowNew && !slot.DeleteEnabled && slot.DeleteName == "Clear slot 3"
            && !blank.DeleteEnabled && !blank.NewEnabled && !hidden.ShowDelete && !hidden.ShowNew
            && orderedIds && orderedControls && initiallyHidden && shownForSlot && twoBarsParented && clicks == 1 && stackTitlesLazy && slotTitles
            && !round.ShowDeleteButton && round.ShowNewNoteButton && round.SlotDots == "quiet"
            && JsonSerializer.Deserialize<Settings>("{}")!.ShowDeleteButton
            && PadWindow.CanDeleteAt(1000, 1500) && !PadWindow.CanDeleteAt(1000, 1499)
            && PadWindow.SwapTarget(3, [false, true]) == 3 && PadWindow.SwapTarget(0, [false, true]) == 2
            && PadWindow.SwapTarget(0, [false, false]) == 1
            && PadWindow.SwapTarget(1, [true, true], 1) == 2
            && ChromeTokens.PagerCountButtonHeight == 32 && ChromeTokens.PagerDotButtonHeight == 32
            && ChromeTokens.BarCornerSize == ChromeTokens.PillHeight
            && ShortcutMap.Definitions.Any(d => d.Id == "swap-view" && d.DefaultKey == "");
        report = ok ? "bar decisions and settings" : "bar decisions or settings failed";
        return ok;
    }

    /// <summary>Builds a small bar snapshot for pure content checks.</summary>
    private static BarContext BarContextForTest(int slot, bool showDelete, bool showNew, bool empty, int stackCount, bool trailingBlank) => new(
        BarPane.Left,
        slot > 0 ? BarViewKind.Slot : BarViewKind.Stack,
        slot,
        0,
        stackCount,
        0,
        empty,
        trailingBlank,
        Enumerable.Repeat(new BarSlotContext(true, static () => ""), 9).ToArray(),
        showDelete,
        showNew,
        "pill",
        BarStyle.Docked,
        Palette.All[0],
        static _ => "");

    /// <summary>Checks slot navigation and pointer-held docked chrome on a real window without showing it.</summary>
    private static bool BarInteractionChecks(PadWindow pad, Settings settings, out string report)
    {
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
        var type = typeof(PadWindow);
        var bar = ((List<PadBar>)type.GetField("_bars", fields)!.GetValue(pad)!)[0];
        var root = bar.Root;
        var top = (Control)type.GetField("_top", fields)!.GetValue(pad)!;
        var icons = (Control)type.GetField("_icons", fields)!.GetValue(pad)!;
        var awake = type.GetField("_chromeAwake", fields)!;
        var timer = (DispatcherTimer)type.GetField("_chromeSleep", fields)!.GetValue(pad)!;
        var sleep = type.GetMethod("SleepChrome", fields)!;
        var swap = type.GetMethod("SwapView", fields)!;
        var swapButton = (Button)type.GetField("_swapButton", fields)!.GetValue(pad)!;
        var pointer = typeof(InputElement).GetProperty("IsPointerOver")!;
        var failures = new List<string>();
        void Expect(bool ok, string message) { if (!ok) failures.Add(message); }
        void Point(Control control, bool value) => pointer.SetValue(control, value);
        try
        {
            settings.Split = false;
            pad.OpenSlot(1);
            if (pad.Data.ViewSlot != 1) pad.OpenSlot(1);
            pad.Navigate(1);
            Expect(pad.Data.ViewSlot == 2, "slot 1 to 2");
            pad.OpenSlot(9);
            pad.Navigate(1);
            Expect(pad.Data.ViewSlot == 1, "slot 9 to 1");
            pad.Navigate(-1);
            Expect(pad.Data.ViewSlot == 9, "slot 1 to 9");
            pad.ShowStack();
            Expect(pad.Data.ViewSlot == 0, "stack reachable");
            swap.Invoke(pad, [BarPane.Left]);
            Expect(pad.Data.ViewSlot == 9, "swap remembers cycled slot");
            settings.Split = true;
            settings.SplitSlot = 1;
            pad.Navigate(1);
            Expect(pad.Data.ViewSlot == 2, "skip split slot");
            settings.Split = false;
            settings.Chrome = "fade";
            settings.Bars = "docked";
            foreach (var control in new[] { root, top })
            {
                Point(control, true);
                sleep.Invoke(pad, [false, false]);
                Expect((bool)awake.GetValue(pad)! && icons.IsHitTestVisible && root.IsHitTestVisible && timer.IsEnabled,
                    "pointer keeps chrome awake");
                if (ReferenceEquals(control, top))
                {
                    swapButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
                    Expect((bool)awake.GetValue(pad)! && icons.IsHitTestVisible && root.IsHitTestVisible, "swap keeps chrome awake");
                }
                Point(control, false);
            }
        }
        finally
        {
            Point(root, false);
            Point(top, false);
            timer.Stop();
        }
        report = failures.Count == 0 ? "slot navigation and chrome hover" : string.Join("; ", failures);
        return failures.Count == 0;
    }
}
