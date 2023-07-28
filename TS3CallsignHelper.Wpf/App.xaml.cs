using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using TS3CallsignHelper.Game.Extensions;
using TS3CallsignHelper.Game.LogParsers;
using TS3CallsignHelper.Game.Stores;
using TS3CallsignHelper.Wpf.Extensions;
using TS3CallsignHelper.Wpf.ViewModels;

namespace TS3CallsignHelper.Wpf;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {

  private readonly IHost _host;

  public App() {
    IConfigurationBuilder configBuilder = new ConfigurationBuilder();
    if (File.Exists("serilog.json"))
      configBuilder = configBuilder.AddJsonFile("serilog.json");
    else
      configBuilder.AddJsonStream(GetType().Assembly.GetManifestResourceStream(GetType().Namespace + ".serilog.json"));
    var config = configBuilder.Build();

    config.CleanFolder(3);

    Log.Logger = new LoggerConfiguration()
      .ReadFrom.Configuration(config.ApplyTimestamp())
      .CreateLogger();


    try {
      _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureGameStores()
            .ConfigureServices((hostcontext, services) => {

              services.AddSingleton<IGameLogParser, GameLogParserPlacesTwo>();

              services.AddSingleton<MainViewModel>();
              services.AddSingleton(s => new MainWindow() {
                DataContext = s.GetRequiredService<MainViewModel>()
              });
            })
            .Build();
    }
    catch (Exception ex) {
      Log.Fatal(ex, "An exception occurred during application initialization");
    }



  }

  protected override void OnStartup(StartupEventArgs e) {
    try {
      _host.Start();

      var mainv = _host.Services.GetRequiredService<MainWindow>();
      var mainvm = _host.Services.GetRequiredService<MainViewModel>();
      mainv.Show();
      mainvm.AddView(new CanvasContainerViewModel(mainvm, new CallsignInformationViewModel(_host.Services.GetRequiredService<GameStateStore>())));

      var logParser = _host.Services.GetRequiredService<IGameLogParser>();
      logParser.Init(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"), "FeelThere Inc_\\Tower! Simulator 3"));
      logParser.Start();

      base.OnStartup(e);
    }
    catch (Exception ex) {
      Log.Fatal(ex, "An exception occurred during application startup");
    }
  }

  protected override void OnExit(ExitEventArgs e) {
    _host.Dispose();

    base.OnExit(e);
  }
}
