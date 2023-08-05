using System;
using TS3CallsignHelper.Api;
using TS3CallsignHelper.Wpf.Stores;

namespace TS3CallsignHelper.Wpf.Commands;
internal class NavigateCommand : CommandBase {

  private readonly NavigationStore _navigationStore;
  private readonly Func<IViewModel> _createViewModel;

  public NavigateCommand(Func<IViewModel> createViewModel, NavigationStore navigationStore) {
    _navigationStore = navigationStore;
    _createViewModel = createViewModel;
  }

  public override void Execute(object? parameter) {
    _navigationStore.RootContent = _createViewModel();
  }
}
