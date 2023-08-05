using System;
using System.Collections.Generic;
using TS3CallsignHelper.Api.Dependencies;

namespace TS3CallsignHelper.Wpf.Stores;
internal class DependencyStore : IDependencyStore {

  private readonly Dictionary<Type, object> dependencies = new();
  public T Add<T>(T instance) {
    dependencies.Add(typeof(T), instance);
    return instance;
  }

  public T? TryGet<T>() {
    return (T?) (dependencies.TryGetValue(typeof(T), out object result) ? result : null);
  }
}
