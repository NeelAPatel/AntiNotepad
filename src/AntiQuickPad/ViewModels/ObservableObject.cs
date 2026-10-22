using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
namespace AntiQuickPad.ViewModels;
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void Changed([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}
public sealed class RelayCommand(Action execute) : ICommand
{
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => execute();
    public event EventHandler? CanExecuteChanged { add { } remove { } }
}

// Note selection is parameterized; invalid parameter types are rejected at the binding boundary.
// M11a-settings-function: a typed parameter command (settings values).
internal sealed class ValueCommand<T>(Action<T> execute) : ICommand
{
    public bool CanExecute(object? parameter) => parameter is T;
    public void Execute(object? parameter) => execute(parameter is T value ? value : throw new ArgumentException($"Expected {typeof(T).Name}.", nameof(parameter)));
    public event EventHandler? CanExecuteChanged { add { } remove { } }
}

internal sealed class IndexCommand(Action<int> execute) : ICommand
{
    public bool CanExecute(object? parameter) => parameter is int;
    public void Execute(object? parameter) => execute(parameter is int index ? index : throw new ArgumentException("Note index must be an int.", nameof(parameter)));
    public event EventHandler? CanExecuteChanged { add { } remove { } }
}
