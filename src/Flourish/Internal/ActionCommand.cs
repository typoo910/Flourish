using System.Windows.Input;

namespace Flourish.Internal;

internal sealed class ActionCommand : ICommand
{
    private readonly Action action;

    public ActionCommand(Action action)
    {
        this.action = action;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        action();
    }
}
