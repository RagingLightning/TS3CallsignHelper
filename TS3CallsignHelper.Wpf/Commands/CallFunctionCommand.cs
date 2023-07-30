using System;

namespace TS3CallsignHelper.Wpf.Commands;
class CallFunctionCommand : CommandBase {

  private readonly Action _action;

  public CallFunctionCommand(Action action) {
    _action = action;
  }

  public override void Execute(object? parameter) {
    _action();
  }

}
