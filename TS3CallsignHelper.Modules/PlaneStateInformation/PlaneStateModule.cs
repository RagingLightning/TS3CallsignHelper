using System.ComponentModel.Composition;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;

namespace TS3CallsignHelper.Modules.PlaneStateInformation;
[Export(typeof(ICallsignHelperModule))]
[ExportMetadata("Name", "Plane State Module")]
public class PlaneStateModule : ICallsignHelperModule {

  [ImportingConstructor]
  public PlaneStateModule() { }
  public void Load(IDependencyStore dependencyStore) {
    var viewStore = dependencyStore.TryGet<IViewStore>() ?? throw new MissingDependencyException(typeof(IViewStore));

    viewStore.Register(typeof(PlaneStateView), typeof(PlaneStateViewModel), typeof(Translation.PlaneStateModule));
  }
}
