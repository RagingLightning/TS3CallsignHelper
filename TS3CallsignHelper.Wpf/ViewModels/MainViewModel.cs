using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Events;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.API.Services;
using TS3CallsignHelper.API.Stores;
using TS3CallsignHelper.Wpf.Commands;
using TS3CallsignHelper.Wpf.Models;
using TS3CallsignHelper.Wpf.Services;

namespace TS3CallsignHelper.Wpf.ViewModels;
public class MainViewModel : IViewModel {
  private readonly ILogger<MainViewModel>? _logger;
  private readonly IGameStateStore _gameStateStore;
  public override Type Translation => typeof(Translation.MainView);
  public override Type View => typeof(Views.MainView);
  public override double InitialWidth => throw new NotImplementedException();
  public override double InitialHeight => throw new NotImplementedException();

  public event Action<IViewModel>? ViewModelAdded;
  public event Action<IViewModel>? ViewModelRemoved;

  private readonly ObservableCollection<IViewModel> _activeViews;
  public IEnumerable<IViewModel> ActiveViews => _activeViews;

  internal MainViewModel(IDependencyStore dependencyStore, GuiMessageService guiMessageService) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<MainViewModel>() ?? throw new MissingDependencyException(typeof(ILoggerService));
    _gameStateStore = dependencyStore.TryGet<IGameStateStore>() ?? throw new MissingDependencyException(typeof(IGameStateStore));
    var navigationService = dependencyStore.TryGet<INavigationService>() ?? throw new MissingDependencyException(typeof(INavigationService));
    var viewStore = dependencyStore.TryGet<IViewStore>() ?? throw new MissingDependencyException(typeof(IViewStore));

    guiMessageService.ViewModel = this;

    _activeViews = new ObservableCollection<IViewModel>();

    DonateCommand = new PayPalDonateCommand();
    SettingsCommand = new NavigateCommand(() => throw new NotImplementedException(), navigationService);

    SetGroundPosCommand = new SetPositionCommand(_gameStateStore, PlayerPosition.Ground);
    SetTowerPosCommand = new SetPositionCommand(_gameStateStore, PlayerPosition.Tower);
    SetDeparturePosCommand = new SetPositionCommand(_gameStateStore, PlayerPosition.Departure);

    _logger?.LogDebug("Registering event handlers");
    _gameStateStore.GameSessionStarted += OnGameSessionStarted;
    _logger?.LogTrace("{Method} registered", nameof(OnGameSessionStarted));
    _gameStateStore.GameSessionEnded += OnGameSessionEnded;
    _logger?.LogTrace("{Method} registered", nameof(OnGameSessionEnded));

    _availableViews = new ObservableCollection<ViewConfigurationModel>();
    foreach (var view in viewStore.RegisteredViews) {
      (object key, DataTemplate template) = DataTemplateService.CreateTemplate(view.ViewModelType, view.ViewType);
      Application.Current.Resources.Add(key, template);
      _availableViews.Add(new ViewConfigurationModel($"{view.TranslationAssembly}:{view.TranslationDictionary}:Name", new AddViewModelCommand(this, view.ViewModelType, dependencyStore)));
    }

    _availableLanguages = new ObservableCollection<InterfaceLanguageModel>();
    foreach (string lang in InterfaceLanguageModel.SupportedLanguages)
      _availableLanguages.Add(new InterfaceLanguageModel(lang, new SelectLanguageCommand(this, new CultureInfo(lang), dependencyStore)));

  }

  public override void Dispose() {
    _logger?.LogDebug("Unegistering event handlers");
    _gameStateStore.GameSessionStarted -= OnGameSessionStarted;
    _logger?.LogTrace("{Method} unregistered", nameof(OnGameSessionStarted));
    _gameStateStore.GameSessionEnded -= OnGameSessionEnded;
    _logger?.LogTrace("{Method} unregistered", nameof(OnGameSessionEnded));
  }

  public void RemoveView(CanvasContainerViewModel viewContainer) {
    _logger?.LogInformation("Closing {$view}", viewContainer.CurrentViewModel);
    _activeViews.Remove(viewContainer);
    ViewModelRemoved?.Invoke(viewContainer);
    viewContainer.Dispose();
  }

  public void AddView(IViewModel view) {
    var container = new CanvasContainerViewModel(this, view);
    var contentControl = new ContentControl {
      Content = container
    };
    _activeViews.Add(container);
    ViewModelAdded?.Invoke(container);
  }
  public CommandBase DonateCommand { get; }
  public CommandBase SettingsCommand { get; }
  public CommandBase SetGroundPosCommand { get; }
  public CommandBase SetTowerPosCommand { get; }
  public CommandBase SetDeparturePosCommand { get; }

  private readonly ObservableCollection<ViewConfigurationModel> _availableViews;
  public IEnumerable<ViewConfigurationModel> AvailableViews => _availableViews;
  public ViewConfigurationModel? SelectedView { get; set; }

  private bool _viewSelectorOpen;
  public bool ViewSelectorOpen {
    get {
      return _viewSelectorOpen;
    }
    set {
      _viewSelectorOpen = value;
      OnPropertyChanged(nameof(ViewSelectorOpen));
    }
  }

  private readonly ObservableCollection<InterfaceLanguageModel> _availableLanguages;
  public IEnumerable<InterfaceLanguageModel> AvailableLanguages => _availableLanguages;
  public InterfaceLanguageModel? SelectedLanguage { get; set; }

  private bool _languageSelectorOpen;
  public bool LanguageSelectorOpen {
    get {
      return _languageSelectorOpen;
    }
    set {
      _languageSelectorOpen = value;
      OnPropertyChanged(nameof(LanguageSelectorOpen));
    }
  }

  private string? _currentAirport;
  public string CurrentAirport {
    get {
      return _currentAirport ?? string.Empty;
    }
    set {
      _currentAirport = value;
      OnPropertyChanged(nameof(CurrentAirport));
    }
  }

  private string? _currentDatabase;
  public string CurrentDatabase {
    get {
      return _currentDatabase ?? string.Empty;
    }
    set {
      _currentDatabase = value;
      OnPropertyChanged(nameof(CurrentDatabase));
    }
  }

  private string _statusText  = string.Empty;
  public string StatusText {
    get {
      return _statusText;
    }
    set {
      _statusText = value;
      OnPropertyChanged(nameof(StatusText));
    }
  }

  private Brush _statusBrush = Brushes.Black;
  public Brush StatusBrush {
    get {
      return _statusBrush;
    }
    set {
      _statusBrush = value;
      OnPropertyChanged(nameof(StatusBrush));
    }
  }

  private void OnGameSessionStarted(GameSessionStartedEventArgs args) {
    _logger?.LogDebug("Recieved GameInfoChanged event for {@GameInfo}", args.Info);
    CurrentAirport = args.Info.AirportICAO ?? string.Empty;
    CurrentDatabase = args.Info.DatabaseFolder ?? string.Empty;
  }

  private void OnGameSessionEnded() {
    CurrentAirport = string.Empty; CurrentDatabase = string.Empty;
  }

}
