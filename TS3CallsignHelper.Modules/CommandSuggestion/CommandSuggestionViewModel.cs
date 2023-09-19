using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Events;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.API.Stores;
using TS3CallsignHelper.Modules.CommandSuggestion.Models;

namespace TS3CallsignHelper.Modules.CommandSuggestion;
public class CommandSuggestionViewModel : IViewModel {
  private readonly ILogger<CommandSuggestionViewModel>? _logger;
  private readonly IGameStateStore _gameStateStore;

  public override Type Translation => typeof(Translation.CommandSuggestionModule);

  public override Type View => typeof(CommandSuggestionView);

  public override double InitialWidth => 400;

  public override double InitialHeight => 200;

  public CommandSuggestionViewModel(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<CommandSuggestionViewModel>();
    _gameStateStore = dependencyStore.TryGet<IGameStateStore>() ?? throw new MissingDependencyException(typeof(IGameStateStore));

    _gameStateStore.CurrentAirplaneChanged += OnCurrentAirplaneChanged;
    _gameStateStore.PlaneStateChanged += OnPlaneStateChanged;
    _gameStateStore.ActivePositionChanged += OnActivePositionsChanged;
  }

  private void OnCurrentAirplaneChanged(AirplaneChangedEventArgs args) {
    _logger?.LogInformation("Updating command list for {Airplane} because of new selection", args.Callsign);
    UpdateCommands(_gameStateStore.PlaneStates.GetValueOrDefault(args.Callsign, new PlaneStateInfo()));
  }

  private void OnPlaneStateChanged(PlaneStateChangedEventArgs args) {
    if (args.Callsign == _gameStateStore.CurrentAirplane) {
      _logger?.LogInformation("Updating command list for {Airplane} because of state change", args.Callsign);
      UpdateCommands(args.State);
    }
  }

  private void OnActivePositionsChanged(PlayerPositionChangedEventArgs args) {
    _logger?.LogInformation("Updating command list for {Airplane} because of position change", _gameStateStore.CurrentAirplane);
    UpdateCommands(_gameStateStore.PlaneStates.GetValueOrDefault(_gameStateStore.CurrentAirplane, new PlaneStateInfo()));
  }

  private void UpdateCommands(PlaneStateInfo state) {
    Commands.ClearSafe();
    switch (state.State) {
      case PlaneState.OUT_STARTUP_REQUEST:
        Commands.AddSafe(SuggestedCommand.StartupApproved(state.Runway));
        break;
      case PlaneState.OUT_PUSHBACK_REQUEST:
        Commands.AddSafe(SuggestedCommand.PushbackApproved(state.Runway));
        break;
      case PlaneState.OUT_PUSHBACK_PROGRESS:
        Commands.AddSafe(SuggestedCommand.PULL_BACK);
        Commands.AddSafe(SuggestedCommand.HOLD_POSITION);
        break;
      case PlaneState.OUT_TAXI_REQUEST:
        Commands.AddSafe(SuggestedCommand.RunwayVia(state.Runway));
        Commands.AddSafe(SuggestedCommand.RunwayAtVia(state.Runway, state.RunwayIntersection));
        Commands.AddSafe(SuggestedCommand.TAXI_HOLD_INTERSECTION);
        break;
      case PlaneState.OUT_TAXI_PROGRESS:
        Commands.AddSafe(SuggestedCommand.HOLD_SHORT_TAXIWAY);
        Commands.AddSafe(SuggestedCommand.HOLD_POSITION);
        Commands.AddSafe(SuggestedCommand.CONTINUE_TAXI);
        if (_gameStateStore.PlayerPositions.Contains(PlayerPosition.Tower)) {
          Commands.AddSafe(SuggestedCommand.CROSS_RUNWAY);
          Commands.AddSafe(SuggestedCommand.LineUp(state.Runway));
          Commands.AddSafe(SuggestedCommand.ClearedTakeoff(state.Runway));
          Commands.AddSafe(SuggestedCommand.ClearedImmediateTakeoff(state.Runway));
        }
        if (_gameStateStore.PlayerPositions.Contains(PlayerPosition.Ground) != _gameStateStore.PlayerPositions.Contains(PlayerPosition.Tower)) {
          if (_gameStateStore.GroundFrequencies is not null)
            foreach (var freq in _gameStateStore.GroundFrequencies.Values)
              Commands.AddSafe(SuggestedCommand.Contact(freq.Writename));

          if (_gameStateStore.TowerFrequencies is not null)
            foreach (var freq in _gameStateStore.TowerFrequencies.Values)
              Commands.AddSafe(SuggestedCommand.Contact(freq.Writename));
        }
        break;
      case PlaneState.OUT_RWY_WAITING:
        Commands.AddSafe(SuggestedCommand.CROSS_RUNWAY);
        Commands.AddSafe(SuggestedCommand.LineUp(state.Runway));
        Commands.AddSafe(SuggestedCommand.ClearedTakeoff(state.Runway));
        Commands.AddSafe(SuggestedCommand.ClearedImmediateTakeoff(state.Runway));
        break;
      case PlaneState.OUT_RWY_LINE_UP:
        Commands.AddSafe(SuggestedCommand.ClearedTakeoff(state.Runway));
        break;
      case PlaneState.OUT_RWY_TAKEOFF:
      case PlaneState.IN_RWY_GO_AROUND:
        if (_gameStateStore.DepartureFrequencies is not null)
          foreach (var freq in _gameStateStore.DepartureFrequencies.Values)
            Commands.AddSafe(SuggestedCommand.Contact(freq.Writename));
        break;
      case PlaneState.IN_RWY_APPROACH:
        Commands.AddSafe(SuggestedCommand.ClearedLand(state.Runway));
        Commands.AddSafe(SuggestedCommand.CHANGE_RUNWAY);
        Commands.AddSafe(SuggestedCommand.GO_AROUND);
        break;
      case PlaneState.IN_RWY_CLR_LAND:
      case PlaneState.IN_RWY_CLR_LAHS:
        Commands.AddSafe(SuggestedCommand.EXIT_RUNWAY_AT);
        Commands.AddSafe(SuggestedCommand.EXIT_RUNWAY_ON);
        Commands.AddSafe(SuggestedCommand.EXIT_RUNWAY_AT_ON);
        Commands.AddSafe(SuggestedCommand.GO_AROUND);
        if (_gameStateStore.PlayerPositions.Contains(PlayerPosition.Ground)) {
          Commands.AddSafe(SuggestedCommand.TAXI_TERMINAL);
          Commands.AddSafe(SuggestedCommand.TAXI_HOLD_INTERSECTION);
        }
        else {
          Commands.AddSafe(SuggestedCommand.TAXI_HOLD_INTERSECTION);
          if (_gameStateStore.GroundFrequencies is not null)
            foreach (var freq in _gameStateStore.GroundFrequencies.Values)
              Commands.AddSafe(SuggestedCommand.Contact(freq.Writename));
        }
        break;
      case PlaneState.IN_RWY_WAITING:
        Commands.AddSafe(SuggestedCommand.CROSS_RUNWAY);
        if (_gameStateStore.GroundFrequencies is not null)
          foreach (var freq in _gameStateStore.GroundFrequencies.Values)
            Commands.AddSafe(SuggestedCommand.Contact(freq.Writename));
        break;
      case PlaneState.IN_TAXI_REQUEST:
        Commands.AddSafe(SuggestedCommand.TAXI_TERMINAL);
        Commands.AddSafe(SuggestedCommand.TAXI_HOLD_INTERSECTION);
        break;
      case PlaneState.IN_TAXI_PROGRESS:
        Commands.AddSafe(SuggestedCommand.HOLD_SHORT_TAXIWAY);
        Commands.AddSafe(SuggestedCommand.HOLD_POSITION);
        Commands.AddSafe(SuggestedCommand.CONTINUE_TAXI);
        if (_gameStateStore.PlayerPositions.Contains(PlayerPosition.Tower))
          Commands.AddSafe(SuggestedCommand.CROSS_RUNWAY);
        if (_gameStateStore.PlayerPositions.Contains(PlayerPosition.Ground) != _gameStateStore.PlayerPositions.Contains(PlayerPosition.Tower)) {
          if (_gameStateStore.GroundFrequencies is not null)
            foreach (var freq in _gameStateStore.GroundFrequencies.Values)
              Commands.AddSafe(SuggestedCommand.Contact(freq.Writename));
        }
        break;
    }
  }

  public ObservableCollection<SuggestedCommand> Commands { get; } = new();

  public int SelectedIndex {
    get => -1;
    set {
      if (value == -1) return;

      CommandSuggestionModule.CopySegments.Clear();
      foreach (var segment in Commands[value].Segments)
        CommandSuggestionModule.CopySegments.Enqueue(segment);
      OnPropertyChanged(nameof(SelectedIndex));
    }
  }
}
