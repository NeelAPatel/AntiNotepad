using Avalonia.Threading;
using AntiQuickPad.Shell;
using AntiQuickPad.ViewModels;

namespace AntiQuickPad.Controller;

internal sealed partial class ResidentController
{
    private readonly IResidentSurface _surface;
    private readonly MainViewModel _vm;
    private readonly IWindowPlatform _platform;
    private readonly ShellServices _services;
    private readonly DispatcherTimer _autosave = new() { Interval = TimeSpan.FromMilliseconds(350) };
    private string? _lastSaveError;
    public bool Binding { get; set; }
    public bool HasAutosave => _autosave.Interval == TimeSpan.FromMilliseconds(350);
    public ResidentController(IResidentSurface surface, MainViewModel viewModel, IWindowPlatform platform, ShellServices services)
    { _surface = surface; _vm = viewModel; _platform = platform; _services = services; }
    public void InitializeAutosave() => _autosave.Tick += (_, _) => SaveNotes();
    public void ScheduleSave()
    {
        if (Binding) return;
        _vm.UpdateCurrent(_surface.Editor.Text, _surface.Editor.CaretOffset);
        _surface.RefreshStatus();
        _autosave.Stop(); _autosave.Start();
    }

    public bool SaveNotes()
    {
        _autosave.Stop();
        try
        {
            _vm.UpdateCurrent(_surface.Editor.Text, _surface.Editor.CaretOffset); _vm.Save(); _surface.SaveWindowSettings();
            _lastSaveError = null; _surface.RefreshStatus(); return true;
        }
        catch (Exception ex)
        {
            _vm.Fail(ex); _lastSaveError = ex.Message; _surface.RefreshStatus(); _surface.Log($"save-error {ex}"); return false;
        }
    }

}
