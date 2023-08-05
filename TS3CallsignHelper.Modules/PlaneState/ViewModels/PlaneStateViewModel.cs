using Microsoft.Extensions.Logging;
using System;
using TS3CallsignHelper.Api;
using TS3CallsignHelper.Api.Dependencies;
using TS3CallsignHelper.Api.Events;
using TS3CallsignHelper.Api.Exceptions;
using TS3CallsignHelper.Api.Logging;
using TS3CallsignHelper.Api.Stores;

namespace TS3CallsignHelper.Modules.PlaneState.ViewModels;
internal class PlaneStateViewModel : IViewModel {
  private readonly ILogger<PlaneStateViewModel>? _logger;
  private readonly IGameStateStore _gameStateStore;
  private readonly IAirportDataStore _airportDataStore;
  public override Type Translation => typeof(Translation.PlaneStateModule);
  public override Type View => typeof(Views.PlaneStateView);

  public override double InitialWidth => 420;

  public override double InitialHeight => 120;

  public PlaneStateViewModel(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<PlaneStateViewModel>();

    _logger?.LogDebug("Initializing");
    _gameStateStore = dependencyStore.TryGet<IGameStateStore>() ?? throw new MissingDependencyException(typeof(IGameStateStore));
    _airportDataStore = dependencyStore.TryGet<IAirportDataStore>() ?? throw new MissingDependencyException(typeof(IAirportDataStore));

    _callsign = string.Empty;
    _state = string.Empty;
    _direction = string.Empty;

    _logger?.LogDebug("Registering event handlers");
    _gameStateStore.CurrentAirplaneChanged += OnCurrentAirplaneChanged;
    _logger?.LogTrace("{Method} registered", nameof(OnCurrentAirplaneChanged));
    _gameStateStore.PlaneStateChanged += OnPlaneStateChanged;
    _logger?.LogTrace("{Method} registered", nameof(OnPlaneStateChanged));
  }

  public override void Dispose() {
    _logger?.LogDebug("Unegistering event handlers");
    _gameStateStore.CurrentAirplaneChanged -= OnCurrentAirplaneChanged;
    _logger?.LogTrace("{Method} unregistered", nameof(OnCurrentAirplaneChanged));
    _gameStateStore.PlaneStateChanged -= OnPlaneStateChanged;
    _logger?.LogTrace("{Method} unregistered", nameof(OnPlaneStateChanged));
  }

  private void OnCurrentAirplaneChanged(AirplaneChangedEventArgs args) {
    if (args.Callsign == string.Empty || !_gameStateStore.PlaneStates.ContainsKey(args.Callsign)) {
      Callsign = string.Empty;
      Direction = string.Empty;
      State = string.Empty;
      return;
    }
    Callsign = args.Callsign;
    if ((_gameStateStore.PlaneStates[Callsign] & Api.PlaneState.IS_INCOMING) == 0)
      Direction = "Direction_Out";
    else
      Direction = "Direction_In";
    State = $"State_{_gameStateStore.PlaneStates[Callsign]}";
  }

  private void OnPlaneStateChanged(PlaneStateChangedEventArgs args) {
    if (args.Callsign == string.Empty) {
      Callsign = string.Empty;
      Direction = string.Empty;
      State = string.Empty;
      return;
    }
    Callsign = args.Callsign;
    if ((args.State & Api.PlaneState.IS_INCOMING) == 0)
      Direction = "Direction_Out";
    else
      Direction = "Direction_In";
    State = $"State_{args.State}";
  }

  private string _callsign;
  public string Callsign {
    get {
      return _callsign;
    }
    set {
      _callsign = value;
      OnPropertyChanged(nameof(Callsign));
    }
  }

  private string _state;
  public string State {
    get {
      return _state;
    }
    set {
      _state = value;
      OnPropertyChanged(nameof(State));
    }
  }

  private string _direction;
  public string Direction {
    get {
      return _direction;
    }
    set {
      _direction = value;
      OnPropertyChanged(nameof(Direction));
    }
  }

  private string _origin = "WIP";
  public string Origin {
    get {
      return _origin;
    }
    set {
      _origin = value;
      OnPropertyChanged(nameof(Origin));
    }
  }

  private string _destination = "WIP";
  public string Destination {
    get {
      return _destination;
    }
    set {
      _destination = value;
      OnPropertyChanged(nameof(Destination));
    }
  }

  private string _command = "WIP";
  public string Command {
    get {
      return _command;
    }
    set {
      _command = value;
      OnPropertyChanged(nameof(Command));
    }
  }
}
