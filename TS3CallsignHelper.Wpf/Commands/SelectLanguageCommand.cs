using Microsoft.Extensions.Logging;
using System.Globalization;
using TS3CallsignHelper.Api.Dependencies;
using TS3CallsignHelper.Wpf.Services;
using TS3CallsignHelper.Wpf.ViewModels;
using WPFLocalizeExtension.Engine;

namespace TS3CallsignHelper.Wpf.Commands;
public class SelectLanguageCommand : CommandBase {
  private readonly ILogger<SelectLanguageCommand>? _logger;
  private readonly MainViewModel _mainViewModel;
  private readonly CultureInfo _culture;

  public SelectLanguageCommand(MainViewModel mainViewModel, CultureInfo culture, IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<LoggerService>()?.GetLogger<SelectLanguageCommand>();
    _mainViewModel = mainViewModel;
    _culture = culture;
  }

  public override void Execute(object? parameter) {
    _logger?.LogDebug("Changing UI language to {Language}", _culture.Name);
    LocalizeDictionary.Instance.Culture = _culture;
    _mainViewModel.LanguageSelectorOpen = false;
  }
}
