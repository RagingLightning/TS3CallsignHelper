using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using TS3CallsignHelper.Api;
using TS3CallsignHelper.Api.Stores;
using TS3CallsignHelper.Api.Logging;
using TS3CallsignHelper.Game.LogParsers;
using TS3CallsignHelper.Game.Services;
using TS3CallsignHelper.Game.Stores;
using TS3CallsignHelper.Wpf.Extensions;
using TS3CallsignHelper.Wpf.Models;
using TS3CallsignHelper.Wpf.Services;
using TS3CallsignHelper.Wpf.Stores;
using TS3CallsignHelper.Wpf.ViewModels;
using WPFLocalizeExtension.Engine;

namespace TS3CallsignHelper.Wpf;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
  [LibraryImport("Kernel32")]
  private static partial void AttachConsole();

  [LibraryImport("Kernel32")]
  private static partial void FreeConsole();

  private readonly IHost? _host;
  private readonly IConfigurationRoot? _serilogConfig;

  public App() {
    AttachConsole();

    IConfigurationBuilder configBuilder = new ConfigurationBuilder();
    if (File.Exists("serilog.json")) {
      Console.WriteLine("using serilog.json");
      Debug.WriteLine("using serilog.json");
      configBuilder.AddJsonFile("serilog.json");
    }
    else {
#pragma warning disable CS8604 // Mögliches Nullverweisargument.
      configBuilder.AddJsonStream(GetType().Assembly.GetManifestResourceStream(GetType().Namespace + ".serilog.json"));
#pragma warning restore CS8604 // Mögliches Nullverweisargument.
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
            .Build();
    }
    catch (Exception ex) {
      Log.Fatal(ex, "An exception occurred during application initialization");
    }
  }

  protected override void OnStartup(StartupEventArgs e) {
    Log.Information("Application startup");
    try {
      if (_host is null) throw new Exception(".Net hosting failed to initialize");
      _host.Start();
      DependencyStore dependencyStore = new DependencyStore();
      dependencyStore.Add<ILoggerService>(new LoggerService(_host.Services));

      Log.Debug("Initializing OptionsStore");
      var optionsStore = dependencyStore.Add<OptionsStore>(new OptionsStore("Options.json", dependencyStore));

      var filesToKeep = optionsStore.BackupLogFiles;
      Log.Verbose("Cleaning log folder to no more than {Backup} files", filesToKeep);
      _serilogConfig?.CleanFolder(filesToKeep);

      dependencyStore.Add<IInitializationProgressService>(new InitializationProgressService());

      Log.Debug("Initializing AirportDataStore");
      dependencyStore.Add<IAirportAirlineService>(new AirportAirlineService(dependencyStore));
      dependencyStore.Add<IAirportAirplaneService>(new AirportAirplaneService(dependencyStore));
      dependencyStore.Add<IAirportFrequencyService>(new AirportFrequencyService(dependencyStore));
      dependencyStore.Add<IAirportGaService>(new AirportGaService(dependencyStore));
      dependencyStore.Add<IAirportScheduleService>(new AirportScheduleService(dependencyStore));
      dependencyStore.Add<IAirportDataStore>(new AirportDataStore(dependencyStore));

      Log.Debug("Initializing GameLogParser");
      var gameLogParser = dependencyStore.Add<IGameLogParser>(new GameLogParserPlacesTwo(dependencyStore));

      Log.Debug("Initializing GameStateStore");
      dependencyStore.Add<IGameStateStore>(new GameStateStore(dependencyStore));

      Log.Debug("Initializing NavigationStore");
      var navigationStore = dependencyStore.Add<NavigationStore>(new NavigationStore());
      navigationStore.RootContent = new InitializationViewModel(dependencyStore);

      Log.Debug("Preparing Localization");
      LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
      if (InterfaceLanguageModel.SupportedLanguages.Any(s => Thread.CurrentThread.CurrentUICulture.Name == s))
        LocalizeDictionary.Instance.Culture = Thread.CurrentThread.CurrentUICulture;
      else
        LocalizeDictionary.Instance.Culture = new CultureInfo("en-US");

      Log.Debug("Loading Modules");
      dependencyStore.Add<IViewStore>(new ViewStore());
      var moduleStore = dependencyStore.Add(new ModuleStore(dependencyStore));
      moduleStore.LoadModules();

      Log.Debug("Opening the main window");
      var mainWindow = new MainWindow() { DataContext = new RootViewModel(dependencyStore) };
      mainWindow.Show();

      var logFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"), "FeelThere Inc_\\Tower! Simulator 3");
      Log.Debug("Starting log parser at {Path}", logFolder);
      gameLogParser.Init(logFolder);
      gameLogParser.Start();

      base.OnStartup(e);
    }
    catch (Exception ex) {
      Log.Fatal(ex, "An exception occurred during application startup");
    }
  }

  protected override void OnExit(ExitEventArgs e) {
    _host?.Dispose();

    base.OnExit(e);
  }

  private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
    _host?.Dispose();
    Log.Fatal((Exception) e.ExceptionObject, "An unhandled exception occurred");
  }
}
