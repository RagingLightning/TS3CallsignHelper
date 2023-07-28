using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace TS3CallsignHelper.Wpf.ViewModels;
internal class MainViewModel : ViewModelBase {
  public override string Name => "Main";
  private readonly ILogger<MainViewModel> _logger;

  public event Action<ViewModelBase> ViewModelAdded;
  public event Action<ViewModelBase> ViewModelRemoved;

  private readonly ObservableCollection<ViewModelBase> _activeViews;
  public IEnumerable<ViewModelBase> ActiveViews => _activeViews;

  public MainViewModel(ILogger<MainViewModel> logger) {
    _activeViews = new ObservableCollection<ViewModelBase>();
    _logger = logger;
  }

  public void RemoveView(CanvasContainerViewModel view) {
    _logger.LogInformation("Closing {$view}", view.CurrentViewModel);
    _activeViews.Remove(view);
    ViewModelRemoved?.Invoke(view);
  }

  public void AddView(CanvasContainerViewModel view) {
    _logger.LogInformation("Adding new {$view}", view.CurrentViewModel);
    var contentControl = new ContentControl();
    contentControl.Content = view;
    _activeViews.Add(view);
    ViewModelAdded?.Invoke(view);
  }
}
