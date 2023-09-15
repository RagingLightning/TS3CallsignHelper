using System;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Services;

namespace TS3CallsignHelper.Wpf.Commands;
internal class NavigateCommand : CommandBase {
  private readonly Func<IViewModel> _createViewModel;
  private readonly INavigationService _navigationService;

  public NavigateCommand(Func<IViewModel> createViewModel, INavigationService navigationService) {
    _createViewModel = createViewModel;
    _navigationService = navigationService;
  }

  public override void Execute(object? parameter) {
    _navigationService.Navigate(_createViewModel());
  }
}
