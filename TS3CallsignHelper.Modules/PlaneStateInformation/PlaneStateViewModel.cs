using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Events;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.API.Stores;

namespace TS3CallsignHelper.Modules.PlaneStateInformation;
internal class PlaneStateViewModel : IViewModel {
  private readonly ILogger<PlaneStateViewModel>? _logger;
  private readonly IGameStateStore _gameStateStore;
  private readonly IAirportDataStore _airportDataStore;
  public override Type Translation => typeof(Translation.PlaneStateModule);
  public override Type View => typeof(PlaneStateView);

  public override double InitialWidth => 420;

  public override double InitialHeight => 120;

  public PlaneStateViewModel(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<PlaneStateViewModel>();

    _logger?.LogDebug("Initializing");
    _gameStateStore = dependencyStore.TryGet<IGameStateStore>() ?? throw new MissingDependencyException(typeof(IGameStateStore));
    _airportDataStore = dependencyStore.TryGet<IAirportDataStore>() ?? throw new MissingDependencyException(typeof(IAirportDataStore));

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
    _logger?.LogInformation("Updating plane state for {Airplane} because of new selection", args.Callsign);
    if (args.Callsign == string.Empty || !_gameStateStore.PlaneStates.ContainsKey(args.Callsign)) {
      Callsign = string.Empty;
      State = string.Empty;
      Direction = string.Empty;
      Origin = string.Empty;
      Destination = string.Empty;
      Command = string.Empty;
      return;
    }
    Callsign = args.Callsign;
    var planeStateInfo = _gameStateStore.PlaneStates[args.Callsign];
    FillData(planeStateInfo);
  }

  private void OnPlaneStateChanged(PlaneStateChangedEventArgs args) {
    _logger?.LogInformation("Updating plane state for {Airplane} because of state change", args.Callsign);
    if (args.Callsign != _gameStateStore.CurrentAirplane) return;
    if (args.Callsign == string.Empty) {
      Callsign = string.Empty;
      Direction = string.Empty;
      State = string.Empty;
      return;
    }
    Callsign = args.Callsign;
    FillData(args.State);
  }

  private void FillData(PlaneStateInfo planeStateInfo) {
    if (!planeStateInfo.State.IsIncoming()) {
      Direction = "Direction_Out";
      Origin = planeStateInfo.Gate ?? string.Empty;
      Destination = string.Empty;
      if (planeStateInfo.TaxiIntersection is (string A, string B) intersection) {
        Destination += intersection.A;
        if (intersection.B != string.Empty)
          Destination += $"x{intersection.B}";
      }
      else {
        if (planeStateInfo.Runway is string rwy) Destination += rwy;
        if (planeStateInfo.RunwayIntersection is string rwyintersect) Destination += $"@{rwyintersect}";
      }
    }
    else {
      Direction = "Direction_In";
      Origin = planeStateInfo.Runway ?? string.Empty;
      Destination = string.Empty;
      if (planeStateInfo.TaxiIntersection is (string A, string B) intersection) {
        Destination += intersection.A;
        if (intersection.B != string.Empty)
          Destination += $"x{intersection.B}";
      }
      else {
        if (planeStateInfo.Gate is string gate) Destination += gate;
        else Destination += "Gate";
      }
    }
    var commandList = new List<string>();
    if (planeStateInfo.TaxiVia.Count > 0)
      commandList.Add($"VIA: {planeStateInfo.TaxiVia.Aggregate((a, b) => $"{a}, {b}")}");
    if (planeStateInfo.RunwayCross is string rwycross)
      commandList.Add($"CROSS: {rwycross}");
    if (planeStateInfo.HoldShort is string hold)
      commandList.Add($"HOLD: {hold}");
    Command = commandList.Count == 0 ? string.Empty : commandList.Aggregate((a, b) => $"{a} | {b}");
    State = $"State_{_gameStateStore.PlaneStates[Callsign].State}";
  }

  private string _callsign = string.Empty;
  public string Callsign {
    get {
      return _callsign;
    }
    set {
      _callsign = value;
      OnPropertyChanged(nameof(Callsign));
    }
  }

  private string _state = string.Empty;
  public string State {
    get {
      return _state;
    }
    set {
      _state = value;
      OnPropertyChanged(nameof(State));
    }
  }

  private string _direction = string.Empty;
  public string Direction {
    get {
      return _direction;
    }
    set {
      _direction = value;
      OnPropertyChanged(nameof(Direction));
    }
  }

  private string _origin = string.Empty;
  public string Origin {
    get {
      return _origin;
    }
    set {
      _origin = value;
      OnPropertyChanged(nameof(Origin));
    }
  }

  private string _destination = string.Empty;
  public string Destination {
    get {
      return _destination;
    }
    set {
      _destination = value;
      OnPropertyChanged(nameof(Destination));
    }
  }

  private string _command = string.Empty;
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
