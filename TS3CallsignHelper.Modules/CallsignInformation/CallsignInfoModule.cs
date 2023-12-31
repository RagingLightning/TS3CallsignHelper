﻿using System.ComponentModel.Composition;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;

namespace TS3CallsignHelper.Modules.CallsignInformation;
[Export(typeof(ICallsignHelperModule))]
[ExportMetadata("Name", "Callsign Info Module")]
public class CallsignInfoModule : ICallsignHelperModule {
  [ImportingConstructor]
  public CallsignInfoModule() { }
  public void Load(IDependencyStore dependencyStore) {
    var viewStore = dependencyStore.TryGet<IViewStore>() ?? throw new MissingDependencyException(typeof(IViewStore));

    viewStore.Register<CallsignInfoView, CallsignInfoViewModel, Translation.CallsignInfoModule>();
  }
}
