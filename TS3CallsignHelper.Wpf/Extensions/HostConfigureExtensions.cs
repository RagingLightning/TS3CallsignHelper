using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TS3CallsignHelper.Wpf.Commands;
using TS3CallsignHelper.Wpf.Stores;
using TS3CallsignHelper.Wpf.ViewModels;

namespace TS3CallsignHelper.Wpf.Extensions;
internal static class HostConfigureExtensions {

  public static IHostBuilder ConfigureWpf(this IHostBuilder builder) {
    return builder.ConfigureServices((host, services) => {
      //Stores
      services.AddSingleton<NavigationStore>();
      services.AddSingleton(s => new OptionsStore(host.Configuration.GetValue<string>("OptionsFile"), s));
      //Commands
      services.AddSingleton<PayPalDonateCommand>();
      //ViewModels
      services.AddSingleton<RootViewModel>();
    });
  }
}
