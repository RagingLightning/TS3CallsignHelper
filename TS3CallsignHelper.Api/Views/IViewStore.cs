using System;
using System.Collections.Generic;
using TS3CallsignHelper.Api.DTO;

namespace TS3CallsignHelper.Api;
public interface IViewStore {
  public IEnumerable<ViewConfiguration> RegisteredViews { get; }
  public void Register(Type viewType, Type viewModelType, Type translationType);
  public void Unregister(Type viewModelType);
}
