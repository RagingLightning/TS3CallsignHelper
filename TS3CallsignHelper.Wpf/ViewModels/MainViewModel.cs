using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace TS3CallsignHelper.Wpf.ViewModels;
internal class MainViewModel : ViewModelBase {
  public override string Name => "Main";

  public event Action<ViewModelBase> ViewModelAdded;
  public event Action<ViewModelBase> ViewModelRemoved;

  private readonly ObservableCollection<ViewModelBase> _activeViews;
  public IEnumerable<ViewModelBase> ActiveViews => _activeViews;

  private Canvas _canvas;

  public MainViewModel() {
    _activeViews = new ObservableCollection<ViewModelBase>();
  }

  public void RemoveView(ViewModelBase view) {
    //_canvas.Children.Remove(_activeViews[view]);
    _activeViews.Remove(view);
    ViewModelRemoved?.Invoke(view);
  }

  public void AddView(ViewModelBase view) {
    var contentControl = new ContentControl();
    contentControl.Content = view;
    _activeViews.Add(view);
    //_canvas.Children.Add(contentControl);
    ViewModelAdded?.Invoke(view);
  }
}
