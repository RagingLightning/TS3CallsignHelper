using TS3CallsignHelper.Api.Dependencies;

namespace TS3CallsignHelper.Api;
public interface ICallsignHelperModule {
  void Load(IDependencyStore dependencyStore);
}

public interface ICallsignHelperModuleData {
  string Name { get; }
}
