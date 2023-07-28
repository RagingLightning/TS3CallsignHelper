using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TS3CallsignHelper.Game.Stores;

namespace TS3CallsignHelper.Game.Extensions;
public static class HostConfigureExtensions {

  public static IHostBuilder ConfigureGameStores(this IHostBuilder builder) {
    return builder.ConfigureServices((hostcontext, services) => {
      services.AddSingleton<GameStateStore>();
    });
  } 
}
