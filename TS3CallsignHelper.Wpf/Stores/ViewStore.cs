using System;
using System.Collections.Generic;
using System.Windows.Controls;
using TS3CallsignHelper.API.DTO;
using TS3CallsignHelper.API;

namespace TS3CallsignHelper.Wpf.Stores;
internal class ViewStore : IViewStore {
  public IEnumerable<ViewConfiguration> RegisteredViews => _registeredViews;
  private readonly List<ViewConfiguration> _registeredViews = new();

  public void Register(Type viewType, Type viewModelType, Type translationType) {
    if (!viewType.IsAssignableTo(typeof(UserControl)))
      throw new ArgumentException($"{viewType} does not inherit from {typeof(UserControl)}", nameof(viewType));
    if (!viewModelType.IsAssignableTo(typeof(IViewModel)))
      throw new ArgumentException($"{viewType} does not inherit from {typeof(IViewModel)}", nameof(viewModelType));
    _registeredViews.Add(new ViewConfiguration(viewType, viewModelType, translationType.Assembly.FullName.Split(',')[0].Trim(), translationType.Name));
  }

  public void Unregister (Type viewModelType) {
    _registeredViews.RemoveAll(p => p.ViewModelType == viewModelType);
  }
}
