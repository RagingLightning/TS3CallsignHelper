using System;
using System.ComponentModel.Composition;
using TS3CallsignHelper.Api;
using TS3CallsignHelper.Api.Dependencies;
using TS3CallsignHelper.Api.Exceptions;

namespace TS3CallsignHelper.Modules.PlaneState;
[Export(typeof(ICallsignHelperModule))]
[ExportMetadata("Name", "Plane State Module")]
public class PlaneStateModule : ICallsignHelperModule {

  [ImportingConstructor]
  public PlaneStateModule() { }
  public void Load(IDependencyStore dependencyStore) {
    var viewStore = dependencyStore.TryGet<IViewStore>() ?? throw new MissingDependencyException(typeof(IViewStore));

    viewStore.Register(typeof(Views.PlaneStateView), typeof(ViewModels.PlaneStateViewModel), typeof(Translation.PlaneStateModule));
  }
}
