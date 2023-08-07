using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Events;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.API.LogParsing;
using TS3CallsignHelper.API.Stores;
using TS3CallsignHelper.Game.Exceptions;
using TS3CallsignHelper.Game.LogParsers;
using TS3CallsignHelper.Game.Services;

namespace TS3CallsignHelper.Game.Stores;
public class GameStateStore : IGameStateStore {
  private readonly ILogger<GameStateStore>? _logger;
  private readonly IInitializationProgressService _initializationProgressService;

  private string? _installationPath;

  public event PlayerPositionChangedEvent? ActivePositionChanged;
  public event AirplaneChangedEvent? CurrentAirplaneChanged;
  public event Action? GameSessionEnded;
  public event GameSessionStartedEventHandler? GameSessionStarted;
  public event PlaneStateChangedEvent? PlaneStateChanged;

  private string _currentAirplane = string.Empty;
  public string CurrentAirplane {
    get => _currentAirplane;
    set {
      _currentAirplane = value;
      CurrentAirplaneChanged?.Invoke(new AirplaneChangedEventArgs(value));
    }
  }

  private GameInfo? _gameInfo = null;
  public GameInfo? CurrentGameInfo {
    get => _gameInfo;
    set => _gameInfo = value;
  }

  private readonly IAirportDataStore _airportDataStore;
  public ImmutableDictionary<string, AirportAirline>? Airlines => _airportDataStore.Airlines;
  public ImmutableDictionary<string, AirportAirplane>? Airplanes => _airportDataStore.Airplanes;
  public ImmutableDictionary<string, AirportGa>? GaPlanes => _airportDataStore.GaPlanes;
  public ImmutableDictionary<string, AirportScheduleEntry>? Schedule => _airportDataStore.Schedule;
  public ImmutableDictionary<string, AirportFrequency>? GroundFrequencies => _airportDataStore.GroundFrequencies;
  public ImmutableDictionary<string, AirportFrequency>? TowerFrequencies => _airportDataStore.TowerFrequencies;
  public ImmutableDictionary<string, AirportFrequency>? DepartureFrequencies => _airportDataStore.DepartureFrequencies;

  private readonly Dictionary<string, PlaneStateInfo> _planeStates = new();
  public ImmutableDictionary<string, PlaneStateInfo> PlaneStates => _planeStates.ToImmutableDictionary();

  private readonly List<PlayerPosition> _playerPositions = new();
  public ImmutableList<PlayerPosition> PlayerPositions => _playerPositions.ToImmutableList();

  /// <summary>
  /// Requires <seealso cref="IInitializationProgressService"/>, <seealso cref="IGameLogParser"/>, <seealso cref="IAirportDataStore"/>
  /// Adds <seealso cref="IGameLogParser"/>
  /// </summary>
  /// <param name="dependencyStore"></param>
  /// <exception cref="MissingDependencyException"></exception>
  internal GameStateStore(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<GameStateStore>();
    _initializationProgressService = dependencyStore.TryGet<IInitializationProgressService>() ?? throw new MissingDependencyException(typeof(IInitializationProgressService));
    _airportDataStore = dependencyStore.TryGet<IAirportDataStore>() ?? throw new MissingDependencyException(typeof(IAirportDataStore));
  }

  public void Dispose() {
    _airportDataStore?.Unload();
  }

  internal void SetInstallDir(string installDir) {
    if (!string.IsNullOrEmpty(_installationPath))
      _logger?.LogWarning("Installation directory was changed during runtime! {OldInstallation} -> {NewInstallation}", _installationPath, installDir);
    else
      _logger?.LogInformation("Tower Simulator installation directory: {Installation}", installDir);
    _installationPath = installDir;
  }

  public void StartGame(GameInfo info) {
    _logger?.LogDebug("Starting game session {@GameInfo}", info);
    _initializationProgressService.Details = $"{info.AirportICAO}/{info.DatabaseFolder} @ {info.StartHour}:00";
    _initializationProgressService.AirplaneProgress = 0;
    _initializationProgressService.AirlineProgess = 0;
    _initializationProgressService.FrequencyProgress = 0;
    _initializationProgressService.GaProgress = 0;
    _initializationProgressService.ScheduleProgress = 0;
    CurrentAirplane = "";
    _planeStates.Clear();

    if (string.IsNullOrEmpty(_installationPath)) {
      _logger?.LogError("Tower Simulator installation not located yet");
      throw new IncompleteGameInfoException(info, "Installation");
    }
    if (string.IsNullOrEmpty(info.AirportICAO)) {
      _logger?.LogError("Game info is missing ICAO code of airport");
      throw new IncompleteGameInfoException(info, nameof(info.AirportICAO));
    }
    if (string.IsNullOrEmpty(info.DatabaseFolder)) {
      _logger?.LogError("Game info is missing selected database");
      throw new IncompleteGameInfoException(info, nameof(info.DatabaseFolder));
    }
    if (string.IsNullOrEmpty(info.AirplaneSetFolder)) {
      _logger?.LogError("Game info is missing selected airplane set");
      throw new IncompleteGameInfoException(info, nameof(info.AirplaneSetFolder));
    }

    _logger?.LogInformation("Starting new game session for {Airport} / {Database}", info.AirportICAO, info.DatabaseFolder);
    _airportDataStore.Load(_installationPath, info);
    _initializationProgressService.StatusMessage = "State_LogFile";

    CurrentGameInfo = info;
    _logger?.LogDebug("Raising {Event}", nameof(GameSessionStarted));
    GameSessionStarted?.Invoke(new GameSessionStartedEventArgs(info));
  }

  public void EndGame() {
    _logger?.LogDebug("Ending game session");
    _initializationProgressService.Details = "";
    CurrentGameInfo = null;
    CurrentAirplane = "";
    _planeStates.Clear();

    _airportDataStore.Unload();

    _logger?.LogDebug("Raising {Event}", nameof(GameSessionEnded));
    GameSessionEnded?.Invoke();
  }

  /// <inheritdoc/>
  public void SetPlaneState(string callsign, PlaneStateInfo state) {
    if (!ValidatePlaneState(callsign, state)) return;
    _logger?.LogDebug("State of {Airplane} changed to {State}", callsign, state);
    _planeStates[callsign] = state;
    PlaneStateChanged?.Invoke(new PlaneStateChangedEventArgs(callsign, state));
  }

  internal void ForcePlaneState(string callsign, PlaneStateInfo state) {
    _logger?.LogDebug("State of {Airplane} changed to {State}", callsign, state);
    _planeStates[callsign] = state;
    PlaneStateChanged?.Invoke(new PlaneStateChangedEventArgs(callsign, state));
  }

  /// <summary>
  /// Validates the rough validity of a plane state
  /// this takes into account the active positions and restrictions for initial contact
  /// </summary>
  /// <param name="airplane">airplane</param>
  /// <param name="stateInfo">state to validate</param>
  /// <returns><c>true</c>, if the assignment is valid, <c>false</c> otherwise</returns>
  private bool ValidatePlaneState(string airplane, PlaneStateInfo stateInfo) {
    if (stateInfo.State == PlaneState.UNKNOWN)
      return !_planeStates.ContainsKey(airplane) || _planeStates[airplane].State == PlaneState.UNKNOWN;
    foreach (var position in _playerPositions) {
      if (stateInfo.State.Is(position) && (_planeStates.ContainsKey(airplane) || stateInfo.State.IsInitial(position)))
        return true;
    }
    _logger?.LogWarning("Airplane {Airplane} state {@State} is not valid with {@Positions} selected", airplane, stateInfo, _playerPositions);
    return false;
  }

  public void SetPlayerPosition(PlayerPosition position, bool active) {
    if (active == _playerPositions.Contains(position)) return;
    if (active)
      _playerPositions.Add(position);
    else
      _playerPositions.Remove(position);
  }
}
