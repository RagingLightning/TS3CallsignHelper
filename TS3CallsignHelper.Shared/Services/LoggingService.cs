using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TS3CallsignHelper.Common.Services;
public static class LoggingService {
  private static IServiceProvider? _serviceProvider;

  public static void Initialize(IServiceProvider serviceProvider) {
    if (_serviceProvider != null) throw new InvalidOperationException();
    _serviceProvider = serviceProvider;
  }

  public static ILogger<T>? GetLogger<T>() => _serviceProvider?.GetRequiredService<ILogger<T>>();
}
