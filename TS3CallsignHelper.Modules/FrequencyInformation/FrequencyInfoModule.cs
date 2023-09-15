using System.ComponentModel.Composition;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;

namespace TS3CallsignHelper.Modules.FrequencyInformation;
[Export(typeof(ICallsignHelperModule))]
[ExportMetadata("Name", "Frequency Info Module")]
public class FrequencyInfoModule : ICallsignHelperModule {

  [ImportingConstructor]
  public FrequencyInfoModule() { }

  public void Load(IDependencyStore dependencyStore) {
    var viewStore = dependencyStore.TryGet<IViewStore>() ?? throw new MissingDependencyException(typeof(IViewStore));

    viewStore.Register<FrequencyInfoView, FrequencyInfoViewModel, Translation.FrequencyInfoModule>();
  }
}
