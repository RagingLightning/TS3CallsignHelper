using System;
using TS3CallsignHelper.API;

namespace TS3CallsignHelper.Wpf.Stores;
public class NavigationStore {

  public event Action? RootContentChanged;

  private IViewModel? _rootContent;
  public IViewModel? RootContent {
    get => _rootContent;
    set {
      if (_rootContent is IViewModel)
        _rootContent.Dispose();
      _rootContent = value;
      RootContentChanged?.Invoke();
    }
  }

}
