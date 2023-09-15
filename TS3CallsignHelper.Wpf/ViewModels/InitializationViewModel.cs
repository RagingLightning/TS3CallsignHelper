using Microsoft.Extensions.Logging;
using System;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.Game.DTO;
using TS3CallsignHelper.Game.Services;
using TS3CallsignHelper.Wpf.Services;
using TS3CallsignHelper.Wpf.Stores;

namespace TS3CallsignHelper.Wpf.ViewModels;
internal class InitializationViewModel : IViewModel {
  private readonly ILogger<InitializationViewModel>? _logger;
  private readonly NavigationStore _navigationStore;
  private readonly IInitializationProgressService _initializationProgressService;
  private readonly IDependencyStore _dependencyStore;
  private readonly GuiMessageService _guiMessageService;
  public override Type Translation => typeof(Translation.InitializationView);
  public override Type View => throw new NotImplementedException();
  public override double InitialWidth => throw new NotImplementedException();
  public override double InitialHeight => throw new NotImplementedException();

  /// <summary>
  /// Requires <seealso cref="NavigationStore"/>, <seealso cref="IInitializationProgressService"/>
  /// </summary>
  /// <param name="dependencyStore"></param>
  /// <exception cref="MissingDependencyException"></exception>
  public InitializationViewModel(IDependencyStore dependencyStore, GuiMessageService guiMessageService) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<InitializationViewModel>();
    _guiMessageService = guiMessageService;

    _logger?.LogDebug("Initializing");
    _navigationStore = dependencyStore.TryGet<NavigationStore>() ?? throw new MissingDependencyException(typeof(NavigationStore));
    _initializationProgressService = dependencyStore.TryGet<IInitializationProgressService>() ?? throw new MissingDependencyException(typeof(IInitializationProgressService));
    _dependencyStore = dependencyStore;

    _logger?.LogInformation("{LoadingState}", "State_LogFile");
    Progress = $"{0.0:.00}%";
    Status = "State_LogFile";

    _initializationProgressService.ProgressChanged += OnProgressChanged;
  }

  public override void Dispose() {
    _initializationProgressService.ProgressChanged -= OnProgressChanged;
  }

  private void OnProgressChanged(Progress progress) {
    if (progress.Completed) {
      _logger?.LogInformation("{LoadingState}", "Complete");
      _navigationStore.RootContent = new MainViewModel(_dependencyStore, _guiMessageService);
      Dispose();
    }

    Progress = $"{progress.Value * 100:0.00}%";
    if (progress.Status != Status)
      _logger?.LogInformation("{LoadingState}", progress.Status);
    Status = progress.Status;
    Details = progress.Details;
  }

  private string? _progress;
  public string Progress {
    get {
      return _progress ?? string.Empty;
    }
    set {
      _progress = value;
      OnPropertyChanged(nameof(Progress));
    }
  }

  private string? _status;
  public string Status {
    get {
      return _status ?? string.Empty;
    }
    set {
      _status = value;
      OnPropertyChanged(nameof(Status));
    }
  }

  private string? _details;
  public string Details {
    get {
      return _details ?? string.Empty;
    }
    set {
      _details = value;
      OnPropertyChanged(nameof(Details));
      OnPropertyChanged(nameof(HasDetails));
    }
  }

  public bool HasDetails => !string.IsNullOrEmpty(_details);
}
