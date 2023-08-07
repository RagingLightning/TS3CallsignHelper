using System;
using System.Collections.Generic;
using TS3CallsignHelper.API.DTO;

namespace TS3CallsignHelper.API;
public interface IViewStore {
  public IEnumerable<ViewConfiguration> RegisteredViews { get; }
  public void Register(Type viewType, Type viewModelType, Type translationType);
  public void Unregister(Type viewModelType);
}
