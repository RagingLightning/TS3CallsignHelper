﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Events;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.API.Services;
using TS3CallsignHelper.API.Stores;
using TS3CallsignHelper.Modules.FrequencyInformation.Models;

namespace TS3CallsignHelper.Modules.FrequencyInformation;
class FrequencyInfoViewModel : IViewModel {

  private readonly ILogger<FrequencyInfoViewModel>? _logger;
  private readonly IGuiMessageService? _guiMessageService;
  private readonly IGameStateStore _gameStateStore;
  private readonly IAirportDataStore _airportDataStore;
  public override Type Translation => typeof(FrequencyInfoModule);
  public override Type View => typeof(FrequencyInfoView);
  public override double InitialWidth => 600;
  public override double InitialHeight => 120;

  private readonly ObservableCollection<FrequencyModel> _groundFrequencies;
  private readonly ObservableCollection<FrequencyModel> _towerFrequencies;
  private readonly ObservableCollection<FrequencyModel> _departureFrequencies;

  public IEnumerable<FrequencyModel> GroundFrequencies => _groundFrequencies;
  public IEnumerable<FrequencyModel> TowerFrequencies => _towerFrequencies;
  public IEnumerable<FrequencyModel> DepartureFrequencies => _departureFrequencies;

  public FrequencyInfoViewModel(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<FrequencyInfoViewModel>();
    _guiMessageService = dependencyStore.TryGet<IGuiMessageService>();

    _logger?.LogInformation("Initializing");
    _gameStateStore = dependencyStore.TryGet<IGameStateStore>() ?? throw new MissingDependencyException(typeof(IGameStateStore));
    _airportDataStore = dependencyStore.TryGet<IAirportDataStore>() ?? throw new MissingDependencyException(typeof(IAirportDataStore));

    _groundFrequencies = new ObservableCollection<FrequencyModel>();
    _towerFrequencies = new ObservableCollection<FrequencyModel>();
    _departureFrequencies = new ObservableCollection<FrequencyModel>();

    _logger?.LogDebug("Registering event handlers");
    _gameStateStore.GameSessionStarted += OnGameSessionStarted;
    _logger?.LogTrace("{Method} registered", nameof(OnGameSessionStarted));
    _gameStateStore.GameSessionEnded += () => Dispatcher.CurrentDispatcher.BeginInvoke(OnGameSessionEnded);
    _logger?.LogTrace("{Method} registered", nameof(OnGameSessionEnded));

    if (_gameStateStore.CurrentGameInfo is GameInfo info) {
      _logger?.LogDebug("Initial frequency population");
      OnGameSessionStarted(new GameSessionStartedEventArgs(info));
    }
  }

  public override void Dispose() {
    _logger?.LogDebug("Unregistering event handlers");
    _gameStateStore.GameSessionStarted -= OnGameSessionStarted;
    _logger?.LogTrace("{Method} unegistered", nameof(OnGameSessionStarted));
    _gameStateStore.GameSessionEnded -= OnGameSessionEnded;
    _logger?.LogTrace("{Method} registered", nameof(OnGameSessionEnded));

  }

  private void OnGameSessionStarted(GameSessionStartedEventArgs args) {
    IEnumerable<FrequencyModel>? groundFrequencies = _airportDataStore.GroundFrequencies?.Select(i => new FrequencyModel(this, i.Value, false));
    if (groundFrequencies != null)
      _groundFrequencies.AddRangeSafe(groundFrequencies.OrderBy(p => p.Frequency));
    IEnumerable<FrequencyModel>? towerFrequencies = _airportDataStore.TowerFrequencies?.Select(i => new FrequencyModel(this, i.Value, false));
    if (towerFrequencies != null)
      _towerFrequencies.AddRangeSafe(towerFrequencies.OrderBy(p => p.Frequency));
    IEnumerable<FrequencyModel>? departureFrequencies = _airportDataStore.DepartureFrequencies?.Select(i => new FrequencyModel(this, i.Value, false));
    if (departureFrequencies != null)
      _departureFrequencies.AddRangeSafe(departureFrequencies.OrderBy(p => p.Frequency));

    if (_airportDataStore.DepartureFrequencies?.Any(p => p.Value.Position == PlayerPosition.Unknown) == true) {
      var freq = _airportDataStore.DepartureFrequencies?.First(p => p.Value.Position == PlayerPosition.Unknown).Value;
      _guiMessageService?.ShowWarning(FrequencyInformation.Translation.FrequencyInfoModule.E_FrequencyPositionUnknown + freq.Writename);
    }
  }

  private void OnGameSessionEnded() {
    _groundFrequencies.ClearSafe();
    _towerFrequencies.ClearSafe();
    _departureFrequencies.ClearSafe();
  }
}
