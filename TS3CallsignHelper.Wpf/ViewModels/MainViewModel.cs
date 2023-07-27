using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace TS3CallsignHelper.Wpf.ViewModels;
internal class MainViewModel : ViewModelBase {
  public override string Name => "Main";

  public event Action<ViewModelBase> ViewModelAdded;
  public event Action<ViewModelBase> ViewModelRemoved;

  private readonly Dictionary<ViewModelBase, ContentControl> _activeViews;
  public IEnumerable<ViewModelBase> ActiveViews => _activeViews.Keys;

  private Canvas _canvas;

  public MainViewModel() {
    _activeViews = new Dictionary<ViewModelBase, ContentControl>();
  }

  public void SetCanvas(Canvas canvas) {
    _canvas = canvas;
  }

  public void RemoveView(ViewModelBase view) {
    _canvas.Children.Remove(_activeViews[view]);
    _activeViews.Remove(view);
    ViewModelRemoved?.Invoke(view);
  }

  public void AddView(ViewModelBase view) {
    var contentControl = new ContentControl();
    contentControl.Content = view;
    _activeViews.Add(view, contentControl);
    _canvas.Children.Add(contentControl);
    ViewModelAdded?.Invoke(view);
  }
}
