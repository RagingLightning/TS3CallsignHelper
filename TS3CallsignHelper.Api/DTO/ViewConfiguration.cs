using System;
using System.Reflection;

namespace TS3CallsignHelper.API.DTO;
public class ViewConfiguration {
  public Type ViewType { get; }
  public Type ViewModelType { get; }
  public string TranslationAssembly { get; }
  public string TranslationDictionary { get; }

  public ViewConfiguration(Type viewType, Type viewModelType, Type translationType) {
    ViewType = viewType;
    ViewModelType = viewModelType;
    TranslationAssembly = translationType.Assembly.FullName.Split(',')[0].Trim();
    TranslationDictionary = translationType.Name;
  }
}
