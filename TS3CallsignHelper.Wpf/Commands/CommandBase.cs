using System;
using System.Windows.Input;

namespace TS3CallsignHelper.Wpf.Commands;
public abstract class CommandBase : ICommand {
  public event EventHandler? CanExecuteChanged;

  public bool CanExecute(object? parameter) {
    return true;
  }

  public abstract void Execute(object? parameter);

  protected void OnCanExecuteChanged() {
    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
  }
}
