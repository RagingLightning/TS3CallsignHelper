using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Controls;
using TS3CallsignHelper.Game.DTOs;
using TS3CallsignHelper.Game.Stores;
using TS3CallsignHelper.Wpf.Commands;
using TS3CallsignHelper.Wpf.Models;
using TS3CallsignHelper.Wpf.Stores;

namespace TS3CallsignHelper.Wpf.ViewModels;
public class MainViewModel : ViewModelBase {
  private IServiceProvider _serviceProvider;
  private ILogger<MainViewModel> _logger;
  public override Type Translation => typeof(Translation.MainView);
  public override double InitialWidth => throw new NotImplementedException();
  public override double InitialHeight => throw new NotImplementedException();

  public event Action<ViewModelBase> ViewModelAdded;
  public event Action<ViewModelBase> ViewModelRemoved;
  
  private readonly ObservableCollection<ViewModelBase> _activeViews;
  public IEnumerable<ViewModelBase> ActiveViews => _activeViews;

  private GameStateStore _gameStateStore;

  public MainViewModel(IServiceProvider serviceProvider) {
    _serviceProvider = serviceProvider;
    _logger = serviceProvider.GetRequiredService<ILogger<MainViewModel>>();
    _gameStateStore = serviceProvider.GetRequiredService<GameStateStore>();

    _activeViews = new ObservableCollection<ViewModelBase>();

    DonateCommand = serviceProvider.GetRequiredService<PayPalDonateCommand>();
    SettingsCommand = new NavigateCommand(() => throw new NotImplementedException(), serviceProvider.GetRequiredService<NavigationStore>());

    _logger.LogDebug("Registering event handlers");
    _gameStateStore.GameSessionStarted += OnGameSessionStarted;
    _logger.LogTrace("{Method} registered", nameof(OnGameSessionStarted));

    _availableViews = new ObservableCollection<ViewConfigurationModel> {
      new ViewConfigurationModel("CallsignInformationView:Name", new AddViewModelCommand(this, () => new CallsignInformationViewModel(_gameStateStore, serviceProvider))),
      new ViewConfigurationModel("FrequencyInfoView:Name", new AddViewModelCommand(this, () => new FrequencyInfoViewModel(_gameStateStore, serviceProvider)))
    };

    _availableLanguages = new ObservableCollection<InterfaceLanguageModel>();
    foreach (string lang in InterfaceLanguageModel.SupportedLanguages)
      _availableLanguages.Add(new InterfaceLanguageModel(lang, new SelectLanguageCommand(this, new CultureInfo(lang), serviceProvider)));

  }

  public override void Dispose() {
    _logger.LogDebug("Unegistering event handlers");
    _gameStateStore.GameSessionStarted -= OnGameSessionStarted;
    _logger.LogTrace("{Method} unregistered", nameof(OnGameSessionStarted));
  }

  public void RemoveView(CanvasContainerViewModel viewContainer) {
    _logger.LogInformation("Closing {$view}", viewContainer.CurrentViewModel);
    _activeViews.Remove(viewContainer);
    ViewModelRemoved?.Invoke(viewContainer);
    viewContainer.Dispose();
  }

  public void AddView(ViewModelBase view) {
    _logger.LogInformation("Adding new {$view}", view);

    var container = new CanvasContainerViewModel(this, view);
    var contentControl = new ContentControl();
    contentControl.Content = container;
    _activeViews.Add(container);
    ViewModelAdded?.Invoke(container);
  }
  public CommandBase DonateCommand { get; }
  public CommandBase SettingsCommand { get; }

  private readonly ObservableCollection<ViewConfigurationModel> _availableViews;
  public IEnumerable<ViewConfigurationModel> AvailableViews => _availableViews;
  public ViewConfigurationModel SelectedView { get; set; }

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
  public InterfaceLanguageModel SelectedLanguage { get; set; }

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

  private string _currentAirport;
  public string CurrentAirport {
    get {
      return _currentAirport;
    }
    set {
      _currentAirport = value;
      OnPropertyChanged(nameof(CurrentAirport));
    }
  }

  private string _currentDatabase;
  public string CurrentDatabase {
    get {
      return _currentDatabase;
    }
    set {
      _currentDatabase = value;
      OnPropertyChanged(nameof(CurrentDatabase));
    }
  }

  private void OnGameSessionStarted(GameInfo info) {
    _logger.LogDebug("Recieved GameInfoChanged event for {@GameInfo}", info);
    CurrentAirport = info.AirportICAO ?? string.Empty;
    CurrentDatabase = info.DatabaseFolder ?? string.Empty;
  }

}
