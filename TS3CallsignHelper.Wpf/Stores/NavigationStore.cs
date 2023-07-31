using System;
using TS3CallsignHelper.Wpf.ViewModels;

namespace TS3CallsignHelper.Wpf.Stores;
public class NavigationStore {

  public event Action? RootContentChanged;

  private ViewModelBase? _rootContent;
  public ViewModelBase? RootContent {
    get => _rootContent;
    set {
      if (_rootContent is ViewModelBase)
        _rootContent.Dispose();
      _rootContent = value;
      RootContentChanged?.Invoke();
    }
  }

}
