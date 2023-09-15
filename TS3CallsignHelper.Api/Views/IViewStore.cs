using System;
using System.Collections.Generic;
using System.Windows.Controls;
using TS3CallsignHelper.API.DTO;

namespace TS3CallsignHelper.API;
public interface IViewStore {
  public IEnumerable<ViewConfiguration> RegisteredViews { get; }
  public void Register<TView, TViewModel, TTranslation>() where TView : UserControl where TViewModel : IViewModel;
  public void Unregister(Type viewModelType);
}
