using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using TS3CallsignHelper.Api;
using TS3CallsignHelper.Api.Dependencies;
using TS3CallsignHelper.Api.Exceptions;
using TS3CallsignHelper.Wpf.Services;
using TS3CallsignHelper.Wpf.Stores;

namespace TS3CallsignHelper.Wpf.ViewModels;
internal class RootViewModel : IViewModel {
  public override Type Translation => throw new NotImplementedException();
  public override Type View => throw new NotImplementedException();
  public override double InitialWidth => throw new NotImplementedException();
  public override double InitialHeight => throw new NotImplementedException();
  private readonly ILogger<RootViewModel>? _logger;

  private readonly NavigationStore _navigationStore;
  public IViewModel? RootContent => _navigationStore.RootContent;

  /// <summary>
  /// Requires <seealso cref="NavigationStore"/>
  /// </summary>
  /// <param name="dependencyStore"></param>
  public RootViewModel(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<LoggerService>()?.GetLogger<RootViewModel>();

    _navigationStore = dependencyStore.TryGet<NavigationStore>() ?? throw new MissingDependencyException(typeof(NavigationStore));

    _logger?.LogDebug("Registering event handlers");
    _navigationStore.RootContentChanged += OnRootContentChanged;
    _logger?.LogTrace("{Method} registered", nameof(OnRootContentChanged));

  }

  private void OnRootContentChanged() {
    OnPropertyChanged(nameof(RootContent));
  }

  public override void Dispose() {

    _logger.LogDebug("Unegistering event handlers");
    _navigationStore.RootContentChanged -= OnRootContentChanged;
    _logger.LogTrace("{Method} unregistered", nameof(OnRootContentChanged));

  }
}
