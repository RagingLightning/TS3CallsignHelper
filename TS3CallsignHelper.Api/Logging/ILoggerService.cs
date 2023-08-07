using Microsoft.Extensions.Logging;

namespace TS3CallsignHelper.API.Logging;
public interface ILoggerService {
  public ILogger<T>? GetLogger<T>();
}
