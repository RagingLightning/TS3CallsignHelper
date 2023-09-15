using System;
using System.Collections.Generic;
using System.Windows.Controls;
using TS3CallsignHelper.API.DTO;
using TS3CallsignHelper.API;

namespace TS3CallsignHelper.Wpf.Stores;
internal class ViewStore : IViewStore {
  public IEnumerable<ViewConfiguration> RegisteredViews => _registeredViews;
  private readonly List<ViewConfiguration> _registeredViews = new();

  public void Register<TView, TViewModel, TTranslation>() where TView : UserControl where TViewModel : IViewModel {
    _registeredViews.Add(new ViewConfiguration(typeof(TView), typeof(TViewModel), typeof(TTranslation)));
  }

  public void Unregister (Type viewModelType) {
    _registeredViews.RemoveAll(p => p.ViewModelType == viewModelType);
  }
}
