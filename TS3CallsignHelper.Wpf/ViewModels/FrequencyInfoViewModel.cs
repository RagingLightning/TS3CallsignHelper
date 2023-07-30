using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TS3CallsignHelper.Game.DTOs;
using TS3CallsignHelper.Game.Stores;
using TS3CallsignHelper.Wpf.Extensions;
using TS3CallsignHelper.Wpf.Models;

namespace TS3CallsignHelper.Wpf.ViewModels;
class FrequencyInfoViewModel : ViewModelBase {
  private readonly ILogger<FrequencyInfoViewModel> _logger;
  private readonly GameStateStore _gameStateStore;
  public override Type Translation => typeof(Translation.FrequencyInfoView);
  public override double InitialWidth => 450;
  public override double InitialHeight => 150;

  private ObservableCollection<FrequencyModel> _groundFrequencies;
  private ObservableCollection<FrequencyModel> _towerFrequencies;
  private ObservableCollection<FrequencyModel> _departureFrequencies;

  public IEnumerable<FrequencyModel> GroundFrequencies => _groundFrequencies;
  public IEnumerable<FrequencyModel> TowerFrequencies => _towerFrequencies;
  public IEnumerable<FrequencyModel> DepartureFrequencies => _departureFrequencies;

  public FrequencyInfoViewModel(GameStateStore gameStateStore, IServiceProvider serviceProvider) {
    _logger = serviceProvider.GetRequiredService<ILogger<FrequencyInfoViewModel>>();

    _logger.LogInformation("Initializing");
    _gameStateStore = gameStateStore;

    _groundFrequencies = new ObservableCollection<FrequencyModel>();
    _towerFrequencies = new ObservableCollection<FrequencyModel>();
    _departureFrequencies = new ObservableCollection<FrequencyModel>();

    _logger.LogDebug("Registering event handlers");
    _gameStateStore.GameSessionStarted += OnGameSessionStarted;
    _logger.LogTrace("{Method} registered", nameof(OnGameSessionStarted));
    _gameStateStore.GameSessionEnded += OnGameSessionEnded;
    _logger.LogTrace("{Method} registered", nameof(OnGameSessionEnded));

  }

  public override void Dispose() {

    _logger.LogDebug("Unregistering event handlers");
    _gameStateStore.GameSessionStarted -= OnGameSessionStarted;
    _logger.LogTrace("{Method} unegistered", nameof(OnGameSessionStarted));
    _gameStateStore.GameSessionEnded -= OnGameSessionEnded;
    _logger.LogTrace("{Method} registered", nameof(OnGameSessionEnded));

  }

  private void OnGameSessionStarted(GameInfo info) {
    IEnumerable<FrequencyModel>? groundFrequencies = _gameStateStore.GroundFrequencies?.Select(i => new FrequencyModel(i, false));
    if (groundFrequencies != null)
      _groundFrequencies.AddRange(groundFrequencies);
    IEnumerable<FrequencyModel>? towerFrequencies = _gameStateStore.TowerFrequencies?.Select(i => new FrequencyModel(i, false));
    if (towerFrequencies != null)
      _towerFrequencies.AddRange(towerFrequencies);
    IEnumerable<FrequencyModel>? departureFrequencies = _gameStateStore.DepartureFrequencies?.Select(i => new FrequencyModel(i, false));
    if (departureFrequencies != null)
      _departureFrequencies.AddRange(departureFrequencies);
  }

  private void OnGameSessionEnded() {

  }
}
