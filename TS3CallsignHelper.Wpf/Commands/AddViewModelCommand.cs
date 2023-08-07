using System;
using System.CodeDom;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.Wpf.ViewModels;

namespace TS3CallsignHelper.Wpf.Commands;
public class AddViewModelCommand : CommandBase {

  private readonly MainViewModel _mainViewModel;
  private Func<IViewModel> _creator;
  public AddViewModelCommand(MainViewModel mainViewModel, Func<IViewModel> creator) {
    _mainViewModel = mainViewModel;
    _creator = creator;
  }
  public AddViewModelCommand(MainViewModel mainViewModel, Type viewModelType, IDependencyStore dependencyStore) {
    _mainViewModel = mainViewModel;
    _creator = () => (IViewModel) Activator.CreateInstance(viewModelType, dependencyStore);
  }

  public override void Execute(object? parameter) {
    _mainViewModel.AddView(_creator());
    _mainViewModel.ViewSelectorOpen = false;
  }
}
