using Avalonia.Controls;
using AvaloniaEdit;
using AntiQuickPad.ViewModels;
namespace AntiQuickPad.Views;
/// <summary>A resident shell hosts one editor and binds its controls to the supplied view model.</summary>
public interface IEditorHost
{
    TextEditor Editor { get; }
}

public interface IShellView : IEditorHost
{
    Window Host { get; }
    MainViewModel ViewModel { get; }
    void ShowResident(string source, long started = 0);
    void HideResident();
    void Quit(int exitCode = 0, string source = "tray");
    void MeasurementHeavy();
}
