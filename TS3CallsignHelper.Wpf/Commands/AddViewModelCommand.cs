using Microsoft.Extensions.Logging;
using System;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.Wpf.Services;
using TS3CallsignHelper.Wpf.Translation;
using TS3CallsignHelper.Wpf.ViewModels;

namespace TS3CallsignHelper.Wpf.Commands;
public class AddViewModelCommand : CommandBase {
  private readonly ILogger<AddViewModelCommand>? _logger;
  private readonly MainViewModel _mainViewModel;
  private Func<IViewModel> _creator;
  public AddViewModelCommand(MainViewModel mainViewModel, Type viewModelType, IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<AddViewModelCommand>();
    _mainViewModel = mainViewModel;
    _creator = () => (IViewModel) Activator.CreateInstance(viewModelType, dependencyStore);
  }

  public override void Execute(object? parameter) {
    try {
      _mainViewModel.AddView(_creator());
      _mainViewModel.ViewSelectorOpen = false;
    }
    catch (MissingDependencyException ex) {
      GuiMessageService.Instance?.ShowError(ExceptionMessages.AddView_MissingDependency);
      _logger?.LogError(ex, "Error adding module");
    }
    catch (Exception ex) {
      GuiMessageService.Instance?.ShowError(ExceptionMessages.AddView_Exception);
      _logger?.LogError(ex, "Error adding module");
    }
  }
}
