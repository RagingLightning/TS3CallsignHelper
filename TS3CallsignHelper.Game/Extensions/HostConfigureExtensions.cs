using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TS3CallsignHelper.Game.Services;
using TS3CallsignHelper.Game.Stores;

namespace TS3CallsignHelper.Game.Extensions;
public static class HostConfigureExtensions {

  public static IHostBuilder ConfigureGame(this IHostBuilder builder) {
    return builder.ConfigureServices((host, services) => {
      // Stores
      services.AddSingleton<GameStateStore>();
      // Services
      services.AddSingleton<InitializationProgressService>();
    });
  } 
}
