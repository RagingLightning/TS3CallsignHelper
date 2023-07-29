using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading;
using WpfLocalization;

namespace TS3CallsignHelper.Wpf.Commands;
public class SelectLanguageCommand : CommandBase {
  private ILogger<SelectLanguageCommand> _logger;
  private CultureInfo _culture;

  public SelectLanguageCommand(CultureInfo culture, IServiceProvider serviceProvider) {
    _logger = serviceProvider.GetRequiredService<ILogger<SelectLanguageCommand>>();
    _culture = culture;
  }

  public override void Execute(object? parameter) {
    _logger.LogDebug("Changing UI language to {Language}", _culture.Name);
    Thread.CurrentThread.CurrentUICulture = _culture;
    LocalizationManager.UpdateValues();
  }
}
