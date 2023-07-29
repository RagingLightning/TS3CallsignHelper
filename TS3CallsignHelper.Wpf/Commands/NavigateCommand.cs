using System;
using TS3CallsignHelper.Wpf.Stores;
using TS3CallsignHelper.Wpf.ViewModels;

namespace TS3CallsignHelper.Wpf.Commands;
internal class NavigateCommand : CommandBase {

  private readonly NavigationStore _navigationStore;
  private readonly Func<ViewModelBase> _createViewModel;

  public NavigateCommand(Func<ViewModelBase> createViewModel, NavigationStore navigationStore) {
    _navigationStore = navigationStore;
    _createViewModel = createViewModel;
  }

  public override void Execute(object? parameter) {
    _navigationStore.RootContent = _createViewModel();
  }
}
