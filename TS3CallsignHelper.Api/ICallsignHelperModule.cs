using System;
using TS3CallsignHelper.API.Dependencies;

namespace TS3CallsignHelper.API;
public interface ICallsignHelperModule {
  void Load(IDependencyStore dependencyStore);
}

public interface ICallsignHelperModuleData {
  string Name { get; }
}
