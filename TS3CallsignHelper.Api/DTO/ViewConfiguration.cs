using System;

namespace TS3CallsignHelper.API.DTO;
public class ViewConfiguration {
  public Type ViewType { get; }
  public Type ViewModelType { get; }
  public string TranslationAssembly { get; }
  public string TranslationDictionary { get; }

  public ViewConfiguration(Type viewType, Type viewModelType, string translationAssembly, string translationDictionary) {
    ViewType = viewType;
    ViewModelType = viewModelType;
    TranslationAssembly = translationAssembly;
    TranslationDictionary = translationDictionary;
  }
}
