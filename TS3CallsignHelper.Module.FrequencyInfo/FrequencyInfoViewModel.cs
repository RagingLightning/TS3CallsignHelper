using System;
using System.Diagnostics;
using TS3CallsignHelper.Api;
using TS3CallsignHelper.Api.Dependencies;
using TS3CallsignHelper.Module.FrequencyInfo.Views;

namespace TS3CallsignHelper.Module.FrequencyInfo;
class FrequencyInfoViewModel : IViewModel {
  public override Type Translation => typeof(Translation.FrequencyInfo);

  public override Type View => typeof(FrequencyInfoView);

  public override double InitialWidth => 600;

  public override double InitialHeight => 120;

  public FrequencyInfoViewModel(IDependencyStore dependencyStore) {
    Debug.WriteLine("LOADED");
  }
}
