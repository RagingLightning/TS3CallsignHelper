using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using TS3CallsignHelper.API.Logging;

namespace TS3CallsignHelper.Wpf.Services;
internal class LoggerService : ILoggerService {
  private readonly IServiceProvider _serviceProvider;

  public LoggerService(IServiceProvider serviceProvider) {
    _serviceProvider = serviceProvider;
  }
  public ILogger<T>? GetLogger<T>() {
    return _serviceProvider.GetRequiredService<ILogger<T>>();
  }
}
