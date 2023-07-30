using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using TS3CallsignHelper.Wpf.Stores;

namespace TS3CallsignHelper.Wpf.ViewModels;
internal class RootViewModel : ViewModelBase {
  public override Type Translation => throw new NotImplementedException();
  private readonly ILogger<RootViewModel> _logger;

  private readonly NavigationStore _navigationStore;
  public ViewModelBase RootContent => _navigationStore.RootContent;


  public RootViewModel(IServiceProvider serviceProvider) {
    _logger = serviceProvider.GetRequiredService<ILogger<RootViewModel>>();

    _navigationStore = serviceProvider.GetRequiredService<NavigationStore>();

    _logger.LogDebug("Registering event handlers");
    _navigationStore.RootContentChanged += OnRootContentChanged;
    _logger.LogTrace("{$Method} registered", nameof(OnRootContentChanged));

  }

  private void OnRootContentChanged() {
    OnPropertyChanged(nameof(RootContent));
  }

  public override void Dispose() {

    _logger.LogDebug("Unegistering event handlers");
    _navigationStore.RootContentChanged -= OnRootContentChanged;
    _logger.LogTrace("{$Method} unregistered", nameof(OnRootContentChanged));
    
  }
}
