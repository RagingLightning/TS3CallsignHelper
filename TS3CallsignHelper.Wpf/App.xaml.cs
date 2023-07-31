using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using TS3CallsignHelper.Common.Services;
using TS3CallsignHelper.Game.Extensions;
using TS3CallsignHelper.Game.LogParsers;
using TS3CallsignHelper.Game.Services;
using TS3CallsignHelper.Game.Stores;
using TS3CallsignHelper.Wpf.Extensions;
using TS3CallsignHelper.Wpf.Models;
using TS3CallsignHelper.Wpf.Stores;
using TS3CallsignHelper.Wpf.ViewModels;
using WPFLocalizeExtension.Engine;

namespace TS3CallsignHelper.Wpf;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
  [DllImport("Kernel32")]
  public static extern void AttachConsole();

  [DllImport("Kernel32")]
  public static extern void FreeConsole();

  private readonly IHost _host;
  private IConfigurationRoot? _serilogConfig;

  public App() {
    AttachConsole();

    IConfigurationBuilder configBuilder = new ConfigurationBuilder();
    if (File.Exists("serilog.json")) {
      Console.WriteLine("using serilog.json");
      Debug.WriteLine("using serilog.json");
      configBuilder = configBuilder.AddJsonFile("serilog.json");
    }
    else {
      configBuilder.AddJsonStream(GetType().Assembly.GetManifestResourceStream(GetType().Namespace + ".serilog.json"));
    }
    _serilogConfig = configBuilder.Build();

    Log.Logger = new LoggerConfiguration()
      .ReadFrom.Configuration(_serilogConfig.ApplyTimestamp())
      .CreateLogger();

    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);

    Log.Information("Application initialization");
    try {
      _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureGame()
            .ConfigureWpf()
            .ConfigureServices((hostcontext, services) => {
              //Log Parser
              services.AddSingleton<IGameLogParser, GameLogParserPlacesTwo>();
              //Main Window
              services.AddSingleton(s => new MainWindow() {
                DataContext = s.GetRequiredService<RootViewModel>()
              });

            })
            .Build();
    }
    catch (Exception ex) {
      Log.Fatal(ex, "An exception occurred during application initialization");
    }
  }

  protected override void OnStartup(StartupEventArgs e) {
    Log.Information("Application startup");
    try {
      _host.Start();
      LoggingService.Initialize(_host.Services);

      var filesToKeep = _host.Services.GetRequiredService<OptionsStore>().BackupLogFiles;
      Log.Verbose("Cleaning log folder to no more than {Backup} files", filesToKeep);
      _serilogConfig?.CleanFolder(filesToKeep);

      Log.Debug("Initializing OptionsStore");
      var optionsStore = _host.Services.GetRequiredService<OptionsStore>();

      Log.Debug("Initializing GameStateStore");
      var gameStateStore = _host.Services.GetRequiredService<GameStateStore>();

      Log.Debug("Initializing NavigationStore");
      var navigationStore = _host.Services.GetRequiredService<NavigationStore>();
      navigationStore.RootContent = new InitializationViewModel(navigationStore, _host.Services.GetRequiredService<InitializationProgressService>(), gameStateStore);

      Log.Debug("Preparing Localization");
      if (InterfaceLanguageModel.SupportedLanguages.Any(s => Thread.CurrentThread.CurrentUICulture.Name == s))
        LocalizeDictionary.Instance.Culture = Thread.CurrentThread.CurrentUICulture;
      else
        LocalizeDictionary.Instance.Culture = new CultureInfo("en-US");
      LocalizeDictionary.Instance.SetCurrentThreadCulture = true;

      Log.Debug("Opening the main window");
      var mainWindow = _host.Services.GetRequiredService<MainWindow>();
      mainWindow.Show();

      var logParser = _host.Services.GetRequiredService<IGameLogParser>();
      var logFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"), "FeelThere Inc_\\Tower! Simulator 3");
      Log.Debug("Starting log parser at {Path}", logFolder);
      logParser.Init(logFolder);
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

  private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
    _host.Dispose();
    Log.Fatal((Exception) e.ExceptionObject, "An unhandled exception occurred");
  }
}
