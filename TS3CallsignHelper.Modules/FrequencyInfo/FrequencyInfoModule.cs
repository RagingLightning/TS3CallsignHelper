using System.ComponentModel.Composition;
using TS3CallsignHelper.Api;
using TS3CallsignHelper.Api.Dependencies;
using TS3CallsignHelper.Api.Exceptions;

namespace TS3CallsignHelper.Modules.FrequencyInfo;
[Export(typeof(ICallsignHelperModule))]
[ExportMetadata("Name", "Frequency Info Module")]
public class FrequencyInfoModule : ICallsignHelperModule {

  [ImportingConstructor]
  public FrequencyInfoModule() { }

  public void Load(IDependencyStore dependencyStore) {
    var viewStore = dependencyStore.TryGet<IViewStore>() ?? throw new MissingDependencyException(typeof(IViewStore));

    viewStore.Register(typeof(Views.FrequencyInfoView), typeof(ViewModels.FrequencyInfoViewModel), typeof(Translation.FrequencyInfoModule));
  }
}
